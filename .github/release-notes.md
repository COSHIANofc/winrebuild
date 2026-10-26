# WinRebuilder v.1.0.a-pre2

Windows 11 向け環境再構築アプリのプレリリースです。インストール前に `config.yml` の内容を確認してください。

## 新機能

- テキストエディターなどで変更した `config.yml` を自動的に再読み込みします。通常の保存に加え、一時ファイルからの置き換えや削除後の再作成にも対応しました。

## 修正

- ダークテーマで文字が暗く読みにくくなる箇所を修正し、見出しの太さを調整しました。
- 無効な設定ファイルは反映せず、最後に有効だった設定を使い続けます。ファイルを修正して保存すると再読み込みを試みます。
- 「設定」画面からの手動再読み込みで、設定に誤りがあるときの表示を改善しました。

## 変更

- README を利用者向けに整理し、winget Package ID の検索に [Winget ID Search](https://winget.coshian.dev) を案内しました。
- 画面の余白や見出しを調整しました。ファイル変更通知（FileSystemWatcher）は短時間にまとめて処理し、変更がない間はファイルを定期的に読み込みません。

## 既知の問題

- 実際のソフトウェア導入とレジストリ変更は、破棄可能な Windows 11 環境での確認が引き続き必要です。

## その他

- `WinRebuilder.zip` または `WinRebuilder-portable.exe` を利用できます。ZIP には `README.md`、`WinRebuilder.exe`、`config.yml`、`config.example.yml` が含まれます。
