namespace WinRebuilder.Core;

/// <summary>Signals one change after a short quiet period; loading remains the workspace's responsibility.</summary>
public sealed class ConfigurationChangeWatcher : IDisposable
{
    private readonly FileSystemWatcher watcher;
    private readonly Timer debounce;
    private readonly string fileName;
    private readonly object gate = new();
    private bool disposed;

    public event EventHandler? Changed;

    public ConfigurationChangeWatcher(string configPath, TimeSpan? delay = null)
    {
        var path = Path.GetFullPath(configPath);
        fileName = Path.GetFileName(path);
        var interval = delay ?? TimeSpan.FromMilliseconds(350);
        if (interval <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(delay));
        Delay = interval;
        debounce = new Timer(_ => Notify(), null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        watcher = new FileSystemWatcher(Path.GetDirectoryName(path)!)
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.Size,
            IncludeSubdirectories = false
        };
        watcher.Changed += OnFile;
        watcher.Created += OnFile;
        watcher.Deleted += OnFile;
        watcher.Renamed += OnRename;
        watcher.Error += OnError;
        watcher.EnableRaisingEvents = true;
    }

    private TimeSpan Delay { get; }
    private void OnFile(object sender, FileSystemEventArgs e)
    {
        if (string.Equals(e.Name, fileName, StringComparison.OrdinalIgnoreCase)) Schedule();
    }
    private void OnRename(object sender, RenamedEventArgs e)
    {
        if (string.Equals(e.Name, fileName, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(e.OldName, fileName, StringComparison.OrdinalIgnoreCase)) Schedule();
    }
    private void OnError(object sender, ErrorEventArgs e) => Schedule();
    private void Schedule()
    {
        lock (gate) if (!disposed) debounce.Change(Delay, Timeout.InfiniteTimeSpan);
    }
    private void Notify()
    {
        EventHandler? handler;
        lock (gate) handler = disposed ? null : Changed;
        handler?.Invoke(this, EventArgs.Empty);
    }
    public void Dispose()
    {
        lock (gate)
        {
            if (disposed) return;
            disposed = true;
            watcher.EnableRaisingEvents = false;
            watcher.Changed -= OnFile;
            watcher.Created -= OnFile;
            watcher.Deleted -= OnFile;
            watcher.Renamed -= OnRename;
            watcher.Error -= OnError;
            watcher.Dispose();
            debounce.Dispose();
            Changed = null;
        }
    }
}
