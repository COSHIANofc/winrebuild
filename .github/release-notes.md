# WinRebuilder v.1.0.a-pre2

Windows 11 向け GUI アプリです。日本語と英語、ライトとダークの画面表示を切り替えられます。

`WinRebuilder.zip` または `WinRebuilder-portable.exe` をダウンロードし、インストール前に `config.yml` を確認してください。ZIP には日本語版 README、EXE、`config.yml`、`config.example.yml` が含まれます。

- GUI から winget ソフトウェアの追加・削除、選択または一括インストール、状態確認ができます。
- 外部で編集した `config.yml` は自動的に再読み込みします。設定が無効な場合は、最後に有効だった設定を保持します。
- ダークテーマの文字色と見出しの視認性を改善しました。
- ExplorerPatcher は検証済み winget パッケージを最終フェーズで扱い、許可された `.reg` 設定だけを検証して適用します。
- レジストリの変更前にバックアップを保存・検証し、設定画面から値単位で復元できます。

実際のインストールとレジストリ変更は、破棄可能な Windows 11 環境で引き続き検証が必要です。
