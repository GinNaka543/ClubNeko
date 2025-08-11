# VRM 0.129.3 パッケージのインポート手順

## 📦 ダウンロード済みパッケージ
- ファイル: `VRM-0.129.3_ddf2.unitypackage`
- サイズ: 約6.7MB
- 場所: `/Users/nakajimaginsei/Downloads/`

## 🎮 Unity でのインポート手順

### 方法1: Unity Editor から直接インポート（推奨）

1. **Unity でプロジェクトを開く**
   - ClubNeko プロジェクトを開く

2. **メニューからインポート**
   ```
   Assets → Import Package → Custom Package...
   ```

3. **ファイルを選択**
   - `/Users/nakajimaginsei/Downloads/VRM-0.129.3_ddf2.unitypackage` を選択
   - または、Finderからファイルを Unity の Project ウィンドウにドラッグ＆ドロップ

4. **インポート設定**
   - Import Unity Package ウィンドウが開く
   - すべてにチェックが入っていることを確認
   - 「Import」ボタンをクリック

5. **インポート完了確認**
   - コンソールにエラーがないか確認
   - メニューバーに「VRM」が追加されているか確認

### 方法2: ダブルクリックでインポート

1. **Finder で以下を実行**
   ```
   /Users/nakajimaginsei/Downloads/VRM-0.129.3_ddf2.unitypackage
   ```
   をダブルクリック

2. **Unity が自動的に開く**
   - ClubNeko プロジェクトが開いている場合は、そこにインポート
   - プロジェクト選択ダイアログが出たら ClubNeko を選択

### 方法3: コマンドラインから配置

```bash
# パッケージをプロジェクトフォルダにコピー
cp /Users/nakajimaginsei/Downloads/VRM-0.129.3_ddf2.unitypackage /Users/nakajimaginsei/ClubNeko/

# Unity で開いてからインポート
```

## ✅ インポート後の確認事項

1. **VRM メニューの確認**
   - Unity メニューバーに「VRM」または「VRM0」が表示される

2. **フォルダ構造の確認**
   ```
   Assets/
   ├── VRM/        # VRM0 用
   ├── VRM10/      # VRM1.0 用
   └── UniGLTF/    # 共通ライブラリ
   ```

3. **サンプルシーンの確認**
   - `Assets/VRM/Samples/` にサンプルシーンがある場合は開いてテスト

4. **エラーチェック**
   - Console ウィンドウ（Window → General → Console）でエラーを確認
   - 赤いエラーがなければ成功

## 🔧 トラブルシューティング

### エラー: "Duplicate files"
- 既存の VRM フォルダを削除してから再インポート
- `Assets/VRM`, `Assets/VRM10`, `Assets/UniGLTF` を削除

### エラー: "Missing dependencies"
- Package Manager で以下を確認：
  - Unity.Mathematics
  - Unity.Collections
  - Unity.Burst

### コンパイルエラー
- Unity バージョンを確認（6000.0.41f1）
- .NET Standard 2.1 に設定されているか確認

## 📝 インポート後の作業

1. **VRM ローダーの更新**
   - `SimpleVRMLoader.cs` の `#if UNIVRM_INSTALLED` を有効化
   - 実際の VRM ロード機能を実装

2. **テスト用 VRM モデル**
   - https://hub.vroid.com/ から無料モデルをダウンロード
   - `Assets/StreamingAssets/VRM/` に配置

3. **動作確認**
   - 新しいシーンを作成
   - VRM モデルをロードしてテスト

---
インポート完了後、VRM キャラクターが使用可能になります！