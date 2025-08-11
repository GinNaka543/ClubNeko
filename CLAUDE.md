# CLAUDE.md - Club Neko (クラブネコ) 

AIアシスタント向けのプロジェクトガイドライン。Unity 6で開発するiOS向け3Dゴルフバトル×メディアコレクションゲーム。

## 🎮 プロジェクト概要

**Club Neko**は、ゴルフのルールをベースにした戦略的チームバトルゲームと、個人ハウスでのメディアコレクション機能を融合させた次世代ソーシャルゲーム。

### コアコンセプト
- **ゴルフ×バトルロイヤル**: 武器と戦略でホールを狙う新感覚ゴルフ
- **メディアハウス**: 写真・動画を3D空間でコレクション
- **VRoid連携**: カスタムキャラクターを召喚可能
- **ハピネスゲーム連携**: 既存iOSアプリとのデータ共有

### ビジュアルスタイル
- **風景**: ゼルダの伝説 Tears of the Kingdom風のスタイライズド表現
- **キャラクター**: VRMベースのアニメ調3Dモデル
- **UI**: 猫モチーフのポップでフレンドリーなデザイン

## 🛠️ 技術スタック

### 開発環境
- **エンジン**: Unity 6 (6000.0.x LTS)
- **言語**: C# 12.0
- **IDE**: Visual Studio 2022 / Rider
- **バージョン管理**: Git

### 主要パッケージ
```json
{
  "dependencies": {
    "com.unity.render-pipelines.universal": "17.0.0",
    "com.unity.inputsystem": "1.8.0",
    "com.unity.cinemachine": "3.0.0",
    "com.unity.addressables": "2.0.0",
    "com.unity.netcode": "2.0.0",
    "com.vrmc.univrm": "0.125.0",
    "com.unity.visualeffectgraph": "17.0.0"
  }
}
```

### 外部連携
- **Houdini Engine**: プロシージャルゴルフコース生成
- **Firebase**: バックエンド・認証・データベース
- **Photon Fusion 2**: リアルタイムマルチプレイヤー

## 📁 プロジェクト構造

```
ClubNeko/
├── Assets/
│   ├── _Project/              # プロジェクト固有アセット
│   │   ├── Scripts/           # C#スクリプト
│   │   │   ├── Core/         # コアシステム
│   │   │   ├── Golf/         # ゴルフメカニクス
│   │   │   ├── House/        # ハウスシステム
│   │   │   ├── Character/    # キャラクター制御
│   │   │   ├── Network/      # ネットワーク
│   │   │   └── UI/           # UIシステム
│   │   ├── Prefabs/          # プレハブ
│   │   ├── Materials/        # マテリアル・シェーダー
│   │   ├── Textures/         # テクスチャ
│   │   ├── Models/           # 3Dモデル
│   │   ├── Animations/       # アニメーション
│   │   ├── Audio/            # サウンド・BGM
│   │   ├── VFX/              # ビジュアルエフェクト
│   │   └── UI/               # UI要素
│   ├── Resources/             # 動的ロードリソース
│   ├── StreamingAssets/       # ストリーミングアセット
│   │   ├── VRM/              # VRMモデル
│   │   └── MediaCache/       # メディアキャッシュ
│   ├── Plugins/              # ネイティブプラグイン
│   │   └── iOS/              # iOS固有プラグイン
│   ├── ThirdParty/           # サードパーティアセット
│   │   ├── Houdini/          # Houdiniアセット
│   │   └── NatureAssets/     # 環境アセット
│   └── Scenes/               # シーン
│       ├── Boot/             # 起動シーン
│       ├── MainMenu/         # メインメニュー
│       ├── Golf/             # ゴルフステージ
│       ├── House/            # ハウスシーン
│       └── Test/             # テストシーン
├── Packages/                  # パッケージ設定
├── ProjectSettings/           # プロジェクト設定
└── UserSettings/             # ユーザー設定
```

## 🎯 コア機能実装

### 1. ゴルフシステム (Golf/)

#### WeaponSystem.cs
```csharp
public enum WeaponType {
    Driver,      // 遠距離、風影響大
    Iron,        // バランス型
    Putter,      // 近距離精密
    Bazooka,     // 超遠距離
    Bat,         // 中距離連打
    AirCannon    // 天候無効
}

public interface IWeapon {
    float Power { get; }
    float Accuracy { get; }
    bool IsWeatherResistant { get; }
    void Fire(Vector3 direction, float charge);
}
```

#### WeatherSystem.cs
```csharp
public enum WeatherType {
    Sunny,    // 基本状態
    Rainy,    // 視界-30%、精度低下
    Windy,    // 軌道変化
    Stormy,   // 視界-90%、雷撃
    Foggy     // 視界-95%、音重要
}

public class WeatherManager : MonoBehaviour {
    public void ChangeWeather(WeatherType type);
    public Vector3 GetWindForce();
    public float GetVisibilityModifier();
}
```

### 2. ハウスシステム (House/)

#### HouseManager.cs
```csharp
public class HouseManager : MonoBehaviour {
    private Dictionary<string, Room> rooms;
    private MediaGallery gallery;
    private VRMCharacterDisplay characterDisplay;
    
    public void LoadHouseLayout(string layoutId);
    public void PlaceMediaItem(MediaItem item, Vector3 position);
    public void SpawnVRMCharacter(string vrmPath);
}
```

#### MediaGallery.cs
```csharp
public class MediaItem {
    public string id;
    public MediaType type; // Photo, Video
    public string sourcePath;
    public DisplayStyle style; // Frame, Projection, Hologram
}

public class MediaGallery : MonoBehaviour {
    public void ImportFromHappinessGame(string[] mediaIds);
    public void DisplayMedia(MediaItem item, Transform anchor);
}
```

### 3. キャラクターシステム (Character/)

#### VRMLoader.cs
```csharp
public class VRMLoader : MonoBehaviour {
    public async Task<GameObject> LoadVRM(string path) {
        // UniVRMを使用してVRMモデルをロード
        var instance = await VrmUtility.LoadAsync(path);
        SetupVRMComponents(instance);
        return instance.gameObject;
    }
}
```

### 4. ネットワーク (Network/)

#### PhotonManager.cs
```csharp
public class PhotonManager : MonoBehaviourPunCallbacks {
    public void CreateRoom(RoomSettings settings);
    public void JoinRoom(string roomCode);
    public void SendGolfShot(ShotData data);
    public void SyncWeatherChange(WeatherType weather);
}
```

## 🎨 アートパイプライン

### Houdiniワークフロー
1. **ゴルフコース生成**
   - プロシージャルな地形生成
   - 動的ホール配置システム
   - 環境プロップの自動配置

2. **天候エフェクト**
   - 雨・風・霧のVFX生成
   - 動的な雲システム

### ゼルダ風ビジュアル実現
1. **Stylized Rendering**
   - トゥーンシェーディング
   - アウトライン描画
   - 水彩画風テクスチャ

2. **環境デザイン**
   - 層状の浮遊島
   - 古代遺跡モチーフ
   - 発光する鉱石・植物

## 🔧 開発ガイドライン

### コーディング規約
```csharp
// 命名規則
public class PlayerController     // PascalCase for classes
private float moveSpeed;          // camelCase for fields
public float MoveSpeed { get; }   // PascalCase for properties
public void CalculateDamage()     // PascalCase for methods

// 名前空間
namespace ClubNeko.Golf.Weapons {
    // ゲーム固有の実装
}
```

### パフォーマンス最適化
- **DrawCall削減**: 同一マテリアルのバッチング
- **LOD設定**: 距離に応じたモデル品質切り替え
- **オクルージョンカリング**: 見えないオブジェクトの非描画
- **テクスチャアトラス**: UI要素の統合

### iOS最適化
- **Metal API**: 最適なグラフィックスAPI
- **テクスチャ圧縮**: ASTC形式を使用
- **メモリ管理**: 2GB以下のメモリ使用量
- **バッテリー最適化**: 30FPSモード提供

## 📱 ハピネスゲーム連携

### データ同期
```csharp
public class HappinessGameBridge : MonoBehaviour {
    [DllImport("__Internal")]
    private static extern string GetHappinessCharacterData();
    
    public async Task<List<Character>> ImportCharacters() {
        #if UNITY_IOS
        var jsonData = GetHappinessCharacterData();
        return JsonUtility.FromJson<List<Character>>(jsonData);
        #endif
    }
}
```

### 共有データ構造
```csharp
[Serializable]
public class HappinessCharacter {
    public string id;
    public string name;
    public string imageIdentifier;
    public Dictionary<string, string> customFields;
    public DateTime birthday;
}
```

## 🚀 ビルド設定

### iOS向け設定
- **Bundle ID**: com.clubneko.game
- **Minimum iOS Version**: 14.0
- **Target Device**: iPhone & iPad
- **Orientation**: Landscape & Portrait

### ビルドコマンド
```bash
# 開発ビルド
unity -batchmode -quit -projectPath . -buildTarget iOS -executeMethod BuildScript.BuildDevelopment

# リリースビルド
unity -batchmode -quit -projectPath . -buildTarget iOS -executeMethod BuildScript.BuildRelease
```

## 🐛 デバッグツール

### インゲームコンソール
- `F1`: FPS表示
- `F2`: メモリ使用量
- `F3`: ネットワーク統計
- `F4`: 天候強制変更
- `F5`: キャラクタースポーン

### テストシーン
- `Test_Golf`: ゴルフメカニクステスト
- `Test_House`: ハウス機能テスト
- `Test_Network`: マルチプレイヤーテスト
- `Test_VRM`: VRMロードテスト

## 📝 重要な注意事項

1. **VRMライセンス**: 各VRMモデルの利用規約を確認
2. **メモリ管理**: iOSでのメモリ制限に注意
3. **ネットワーク**: オフラインモードを必ず実装
4. **プライバシー**: 写真・動画の取り扱いに注意
5. **猫要素**: UIやエフェクトに猫モチーフを積極的に使用

## 🎯 現在の優先タスク

1. ✅ 基本プロジェクト構造の作成
2. ⏳ ゴルフ物理システムの実装
3. ⏳ ハウス表示システムの構築
4. ⏳ VRMローダーの統合
5. ⏳ ハピネスゲーム連携API

## 📞 技術サポート

問題が発生した場合は以下を確認：
- Unity Forum: Unity 6固有の問題
- UniVRM Issues: VRM関連の問題
- Photon Discord: ネットワーク関連
- Houdini Forum: プロシージャル生成

---
*このドキュメントは開発と共に更新されます*
*最終更新: 2025-08-10*