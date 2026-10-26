using WinRebuilder.Core;
using Xunit;

namespace WinRebuilder.Core.Tests;

public sealed class ConfigurationChangeWatcherTests
{
    private const string First = "version: 1\npackages: []\n";
    private const string Second = "version: 1\npackages:\n  - name: 7-Zip\n    provider: winget\n    id: 7zip.7zip\n";

    [Fact]
    public async Task ExternalEditInvalidRecoveryAndManualReloadKeepLastValidModel()
    {
        await InFolder(async path =>
        {
            var workspace = await ConfigurationWorkspace.OpenAsync(path);
            await File.WriteAllTextAsync(path, Second);
            await workspace.ReloadAsync();
            Assert.Single(workspace.Current.Profile.Packages);
            await File.WriteAllTextAsync(path, "version: [");
            await Assert.ThrowsAsync<FormatException>(() => workspace.ReloadIfChangedAsync());
            Assert.Single(workspace.Current.Profile.Packages);
            await File.WriteAllTextAsync(path, First);
            Assert.True(await workspace.ReloadIfChangedAsync());
            Assert.Empty(workspace.Current.Profile.Packages);
            Assert.False(await workspace.ReloadIfChangedAsync());
        });
    }

    [Fact]
    public async Task WatcherCoalescesChangesAndDetectsReplacementRenameAndRecreation()
    {
        await InFolder(async path =>
        {
            var count = 0;
            using var watcher = new ConfigurationChangeWatcher(path, TimeSpan.FromMilliseconds(300));
            watcher.Changed += (_, _) => Interlocked.Increment(ref count);
            await File.WriteAllTextAsync(path, Second);
            await File.AppendAllTextAsync(path, "# one\n");
            await File.AppendAllTextAsync(path, "# two\n");
            await Until(() => Volatile.Read(ref count) == 1);
            await Task.Delay(450);
            Assert.Equal(1, Volatile.Read(ref count));

            var temp = path + ".tmp";
            await File.WriteAllTextAsync(temp, First);
            File.Move(temp, path, true);
            await Until(() => Volatile.Read(ref count) == 2);

            File.Delete(path);
            await Until(() => Volatile.Read(ref count) == 3);
            await File.WriteAllTextAsync(path, Second);
            await Until(() => Volatile.Read(ref count) == 4);
            watcher.Dispose();
            await File.WriteAllTextAsync(path, First);
            await Task.Delay(450);
            Assert.Equal(4, Volatile.Read(ref count));
        });
    }

    [Fact]
    public async Task WorkspaceSaveDoesNotCreateAReloadCycle()
    {
        await InFolder(async path =>
        {
            var workspace = await ConfigurationWorkspace.OpenAsync(path);
            var changed = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            using var watcher = new ConfigurationChangeWatcher(path, TimeSpan.FromMilliseconds(300));
            watcher.Changed += async (_, _) =>
            {
                try { changed.TrySetResult(await workspace.ReloadIfChangedAsync()); }
                catch (Exception e) { changed.TrySetException(e); }
            };
            await workspace.AddPackageAsync(new PackageSpec("7-Zip", PackageProvider.Winget, "7zip.7zip",
                null, null, null, null, null, null, null, Phase.Normal));
            Assert.False(await changed.Task.WaitAsync(TimeSpan.FromSeconds(3)));
            Assert.Single(workspace.Current.Profile.Packages);
        });
    }

    private static async Task InFolder(Func<string, Task> action)
    {
        var folder = Path.Combine(Path.GetTempPath(), "wrb-watch-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            var path = Path.Combine(folder, "config.yml");
            await File.WriteAllTextAsync(path, First);
            await action(path);
        }
        finally { Directory.Delete(folder, true); }
    }

    private static async Task Until(Func<bool> check)
    {
        for (var i = 0; i < 100 && !check(); i++) await Task.Delay(30);
        Assert.True(check());
    }
}
