# 🎮 Club Neko - 3D Golf Battle & Media Collection Game

Unity 6で開発されたiOS向け3Dゴルフバトル×メディアコレクションゲーム

## 🌟 ゲーム概要

**Club Neko**は、ゴルフのルールをベースにした戦略的チームバトルゲームと、個人ハウスでの写真・動画コレクション機能を融合させた次世代ソーシャルゲームです。

### 主な特徴
- 🏌️ **戦略的ゴルフバトル**: 武器と天候を活用した新感覚ゴルフ
- 🏠 **パーソナルハウス**: 3D空間で写真・動画をコレクション
- 🎭 **VRMキャラクター**: VRoid Studioで作成したキャラクターを召喚
- 📱 **ハピネスゲーム連携**: 既存iOSアプリとデータ共有
- 🌈 **ゼルダ風ビジュアル**: Tears of the Kingdom風のスタイライズド表現
- 🐱 **猫テーマ**: UIやエフェクトに猫モチーフ

## 🛠️ 開発環境

### 必要環境
- **Unity**: 6000.0.x LTS以降
- **macOS**: 12.0以降（iOS開発用）
- **Xcode**: 15.0以降
- **Visual Studio / Rider**: 最新版推奨

### セットアップ手順

1. **リポジトリのクローン**
```bash
git clone https://github.com/yourusername/ClubNeko.git
cd ClubNeko
```

2. **Unity Hubでプロジェクトを開く**
- Unity Hubを起動
- 「追加」→ プロジェクトフォルダを選択
- Unity 6.0 LTSで開く

3. **必要パッケージのインストール**
- パッケージマネージャーが自動的に依存関係を解決
- VRM関連パッケージは手動インストールが必要な場合あり

4. **UniVRMのセットアップ**
```
Window → Package Manager → + → Add package from git URL
https://github.com/vrm-c/UniVRM.git?path=/Assets/VRM10#v0.125.0
```

5. **Houdini Engine（オプション）**
- Houdini Engineをインストール（プロシージャルコース生成用）
- SideFXアカウントが必要

## 📁 プロジェクト構造

```
ClubNeko/
├── Assets/
│   ├── _Project/          # プロジェクト固有アセット
│   │   ├── Scripts/       # ゲームスクリプト
│   │   ├── Prefabs/       # プレハブ
│   │   ├── Materials/     # マテリアル
│   │   └── ...
│   ├── StreamingAssets/   # VRM、メディアファイル
│   └── ThirdParty/        # サードパーティアセット
├── Packages/              # Unity パッケージ設定
├── ProjectSettings/       # プロジェクト設定
└── CLAUDE.md             # AI開発ガイドライン
```

## 🎮 ゲームプレイ

### ゴルフバトルモード
- 2人1組のチーム戦
- 武器を使ってボールを飛ばす
- 天候が戦略に影響
- 先にホールインしたチームがターン獲得

### ハウスモード
- 写真・動画を3D空間に展示
- VRMキャラクターと交流
- ハピネスゲームからデータインポート
- フレンドのハウス訪問

## 🔧 ビルド方法

### iOS向けビルド

1. **ビルド設定**
```
File → Build Settings → iOS → Switch Platform
```

2. **Player Settings**
- Bundle Identifier: `com.clubneko.game`
- Minimum iOS Version: 14.0
- Target Device: iPhone & iPad

3. **ビルド実行**
```
Build → 出力先フォルダ選択
```

4. **Xcodeでの設定**
- 署名設定
- Capability追加（必要に応じて）
- デバイスでテスト

## 🎨 アセット作成ガイド

### VRMキャラクター
1. VRoid Studioでキャラクター作成
2. VRM形式でエクスポート
3. StreamingAssets/VRMフォルダに配置

### ゴルフコース（Houdini）
1. Houdiniでプロシージャルコース作成
2. Unity向けにエクスポート
3. Prefab化して使用

## 📱 ハピネスゲーム連携

### データ同期
- キャラクターデータの自動同期
- 写真コレクションの共有
- 相互アプリ起動

### 実装方法
```csharp
// ハピネスゲームからデータ取得
var bridge = GetComponent<HappinessGameBridge>();
var characters = bridge.GetCharacterData();
```

## 🐛 デバッグ

### インゲームコンソール
- `F1`: FPS表示
- `F2`: メモリ使用量
- `F3`: ネットワーク統計
- `F4`: 天候変更
- `F5`: キャラクタースポーン

### テストシーン
- `Test_Golf`: ゴルフメカニクステスト
- `Test_House`: ハウステスト
- `Test_Network`: ネットワークテスト

## 📝 ライセンス

### 使用アセット
- UniVRM: MIT License
- その他のアセットは各ライセンスに準拠

### 注意事項
- VRMモデルは各モデルの利用規約を確認
- ハピネスゲームとの連携は許諾済み

## 🤝 コントリビューション

1. Forkする
2. Feature branchを作成 (`git checkout -b feature/AmazingFeature`)
3. Commitする (`git commit -m 'Add some AmazingFeature'`)
4. Pushする (`git push origin feature/AmazingFeature`)
5. Pull Requestを作成

## 📞 サポート

- Issues: [GitHub Issues](https://github.com/yourusername/ClubNeko/issues)
- Discord: [開発者コミュニティ](#)
- Email: support@clubneko.game

## 🎯 ロードマップ

- [x] 基本プロジェクト構造
- [ ] ゴルフ物理システム
- [ ] ハウス機能
- [ ] VRM統合
- [ ] ハピネスゲーム連携
- [ ] マルチプレイヤー
- [ ] リリース

---

*Club Neko - Where Golf Meets Memories* 🏌️‍♂️🏠🐱