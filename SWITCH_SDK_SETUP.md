# Nintendo Switch SDK セットアップガイド

## 📦 必要なファイルのダウンロード

### 1. Nintendo Developer Portal からダウンロード

1. **Nintendo Developer Portal にログイン**
   - https://developer.nintendo.com/

2. **NDI (Nintendo Dev Interface) にアクセス**
   - ダッシュボード → Downloads

3. **以下をダウンロード**：
   - Nintendo SDK for Unity (最新版)
   - Nintendo Switch SDK
   - Unity for Nintendo Switch Support
   - DevMenu (開発メニュー)

## 🔧 インストール手順

### 1. Nintendo SDK のインストール

```bash
# SDKのインストールパス（推奨）
/Users/nakajimaginsei/Nintendo/SDK/

# 環境変数の設定
export NINTENDO_SDK_ROOT="/Users/nakajimaginsei/Nintendo/SDK"
export PATH="$NINTENDO_SDK_ROOT/Tools/CommandLineTools:$PATH"
```

### 2. Unity プロジェクトの設定

1. **Unity を開く**
2. **Edit → Preferences → External Tools**
3. **Nintendo Switch SDK Path** を設定

### 3. プロジェクト設定の更新

```
File → Build Settings
1. Platform: Nintendo Switch を選択
2. Switch Platform をクリック
3. Player Settings を開く
```

## ⚙️ Player Settings (Switch向け)

### Application Settings
```
Product Name: Club Neko
Publisher: [あなたの開発者名]
Application ID: 0x01000[YOUR_ID]
Version: 1.0.0
Supported Languages: Japanese, English
```

### Icon Settings
```
Small Icon (40x40): /Icons/icon_small.jpg
Large Icon (256x256): /Icons/icon_large.jpg
```

### Publishing Settings
```
Startup User Account: Required
User Account Switching: Enabled
Screenshot: Enabled
Video Capture: Manual
```

### Performance Settings
```
CPU Mode:
- Handheld: 1020 MHz
- Docked: 1785 MHz

GPU Mode:
- Handheld: 384 MHz
- Docked: 768 MHz

Memory Mode: Standard (3.5GB)
```

## 🎯 ビルド設定

### Development Build
```bash
# Unityコマンドラインビルド
Unity -batchmode -quit \
  -projectPath /Users/nakajimaginsei/ClubNeko \
  -buildTarget Switch \
  -executeMethod BuildScript.BuildSwitchDev
```

### Release Build
```bash
Unity -batchmode -quit \
  -projectPath /Users/nakajimaginsei/ClubNeko \
  -buildTarget Switch \
  -executeMethod BuildScript.BuildSwitchRelease
```

## 📱 実機テスト

### NDEV (開発機) への転送

1. **DevMenu を起動**
2. **Application → Install**
3. **NSP ファイルを選択**
4. **インストール実行**

### デバッグ接続

```bash
# コンソール出力の確認
nxlink -s ClubNeko.nsp

# プロファイラー接続
Unity Profiler → Switch Device
```

## 🔍 パフォーマンス目標

### 携帯モード
- 解像度: 1280x720
- FPS: 30 (安定)
- メモリ使用: 2GB以下

### TVモード
- 解像度: 1920x1080
- FPS: 60 (目標)
- メモリ使用: 3GB以下

## ⚠️ 注意事項

1. **TRC (Technical Requirements Checklist)**
   - 任天堂のガイドラインを必ず確認
   - ロットチェック前に全項目をテスト

2. **年齢レーティング**
   - CERO申請が必要（日本向け）
   - ESRB/PEGI（海外向け）

3. **セーブデータ**
   - クラウドセーブ対応推奨
   - セーブデータサイズ: 最大64MB

## 📞 サポート

**Nintendo Developer Support**
- Email: NDI内のサポートチケット
- 緊急時: 任天堂開発者ホットライン

---
最終更新: 2024-01-11