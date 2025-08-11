# 🎮 Nintendo Switch ビルド手順

## 📋 現在の作業手順

### 1. Unity Hub でモジュールを追加
✅ Unity Hub を開く
⬜ Installs タブを選択
⬜ Unity 6000.0.41f1 の歯車アイコンをクリック
⬜ "Add modules" を選択
⬜ "Nintendo Switch Build Support" にチェック
⬜ Install をクリック

### 2. Unity でプラットフォーム切り替え
```
1. Unity でプロジェクトを開く
2. File → Build Settings
3. Platform リストから "Nintendo Switch" を選択
4. "Switch Platform" ボタンをクリック
5. 切り替え完了まで待つ（初回は時間がかかります）
```

### 3. Player Settings の設定
```
Build Settings → Player Settings

🎯 Company Settings:
- Company Name: あなたの会社名
- Product Name: Club Neko

🎮 Icon Settings:
- Small Icon (40x40): icon_small.png
- Large Icon (256x256): icon_large.png

📱 Resolution and Presentation:
- Default Screen Width: 1280
- Default Screen Height: 720
- Fullscreen Mode: Fullscreen Window

⚡ Other Settings:
- Configuration: Master
- Scripting Backend: IL2CPP
- Api Compatibility Level: .NET Standard 2.1
- Target Architectures: ARM64

🔧 Publishing Settings:
- Application ID: 0x0100[YOUR_ID]00000001
- Startup User Account: Required On
- User Account Switching: Enabled
- Screenshot: Allow
- Video Capture: Manual
```

### 4. 最初のテストビルド
```
メニューバー:
Club Neko → Build → Switch Development

または手動:
1. File → Build Settings
2. Build ボタンをクリック
3. 保存先: Builds/Switch/Dev/
4. ファイル名: ClubNeko.nsp
```

## 🔍 確認事項

### Unity Hub モジュール確認
```bash
# Unity Hubでインストール済みモジュールを確認
# Nintendo Switch Build Support が表示されているか
```

### NDI (Nintendo Dev Interface) 確認
- [ ] NDI にログインできる
- [ ] SDK をダウンロード済み
- [ ] Unity Support Package をダウンロード済み

### 開発機材
- [ ] NDEV/SDEV を持っている
- [ ] DevMenu インストール済み
- [ ] 開発機とPCが同じネットワーク

## ⚠️ トラブルシューティング

### "Nintendo Switch Build Support" が表示されない
1. Nintendo Developer アカウントでUnityにログイン
2. Unity Hub → Preferences → Unity Account
3. サインアウト → 再サインイン

### ビルドエラー: SDK not found
1. Edit → Preferences → External Tools
2. Nintendo Switch SDK Path を設定
3. 通常: `/Users/[username]/Nintendo/SDK/`

### ビルドエラー: Application ID invalid
1. NDI で Application ID を生成
2. Player Settings → Publishing Settings
3. Application ID を入力

## 📱 実機での実行

### NSPファイルの転送
```bash
# 1. DevMenu を起動
# 2. Application → Install → From Network
# 3. PCでコマンド実行:
nxlink ClubNeko.nsp -s

# または DevMenu で:
# Application → Install → From SD Card
```

### デバッグ接続
```
Unity → Window → Analysis → Profiler
Connect to → Nintendo Switch
```

## ✅ チェックリスト

開発開始前の確認:
- [ ] Unity Switch Support インストール完了
- [ ] プラットフォーム切り替え完了
- [ ] Player Settings 設定完了
- [ ] テストビルド成功
- [ ] 実機で起動確認

---
準備ができたら、Unity で実際の開発を開始できます！