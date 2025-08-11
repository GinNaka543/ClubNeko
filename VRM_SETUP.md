# VRM 1.0 セットアップガイド

## 📦 UniVRM のインストール手順

### 方法1: Unity Package Manager から追加（推奨）

1. **Unity でプロジェクトを開く**

2. **Window → Package Manager**

3. **+ ボタン → Add package from git URL**

4. **以下のURLを入力:**
   ```
   https://github.com/vrm-c/UniVRM.git?path=/Assets/VRM10#v0.127.0
   ```

5. **Add をクリック**

### 方法2: 手動ダウンロード

1. **UniVRM リリースページ**
   ```
   https://github.com/vrm-c/UniVRM/releases
   ```

2. **最新版をダウンロード**
   - UniVRM-0.127.0_*.unitypackage

3. **Unity でインポート**
   - Assets → Import Package → Custom Package
   - ダウンロードしたファイルを選択

## ✅ インストール確認

Unity のコンソールで以下を確認：
- エラーが出ていないこと
- "VRM" メニューが追加されていること

## 🎮 VRM テスト用モデル

無料のVRMモデルをダウンロード：
- https://hub.vroid.com/ (VRoid Hub)
- https://3d.nicovideo.jp/works/td32797 (アリシア・ソリッド)

---
インストール後、ゴルフゲームの基本機能実装に進みます。