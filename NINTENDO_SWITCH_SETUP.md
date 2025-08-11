# Nintendo Switch 開発セットアップガイド

## ⚠️ 重要な注意事項

Nintendo Switch向けの開発には、任天堂との正式な開発者契約が必要です。

## 📋 必要な手順

### 1. Nintendo Developer Portal への登録

1. **Nintendo Developer Portal にアクセス**
   - URL: https://developer.nintendo.com/
   - 「Register」をクリックして開発者登録

2. **必要な情報**
   - 会社情報または個人事業主情報
   - 開発経験の詳細
   - ゲームのコンセプト資料

### 2. Nintendo Developer Program への申請

申請が承認されると以下にアクセス可能：
- Nintendo Dev Interface (NDI)
- Nintendo SDK のダウンロード
- Unity for Nintendo Switch アドオン

### 3. Unity Nintendo Switch サポートのインストール

**承認後の手順：**

```bash
# Unity Hubから
1. Unity Hub を開く
2. "Installs" タブを選択
3. Unity 6000.0.41f1 の歯車アイコンをクリック
4. "Add modules" を選択
5. "Nintendo Switch Build Support" を選択（NDPメンバーのみ表示）
```

## 🎮 開発キット不要のSwitch向け最適化

開発者登録前でも、以下の最適化を実施できます：

### パフォーマンス最適化設定

```csharp
// SwitchOptimizationSettings.cs
public static class SwitchOptimizationSettings
{
    // Switch相当のスペック設定
    public const int MAX_TEXTURE_SIZE = 2048;
    public const int TARGET_FPS = 30; // 携帯モード
    public const int TARGET_FPS_DOCKED = 60; // ドックモード
    public const int MAX_VERTEX_COUNT = 100000;
    public const int MAX_DRAW_CALLS = 100;
    
    // メモリ制限
    public const int MAX_MEMORY_MB = 3500; // 利用可能メモリ
    public const int TEXTURE_MEMORY_MB = 1000;
}
```

### コントローラー入力の準備

```csharp
// Input System PackageでSwitch風コントローラー設定
public class SwitchControllerEmulator : MonoBehaviour
{
    // Joy-Con (L)
    public bool ButtonL;
    public bool ButtonZL;
    public Vector2 LeftStick;
    
    // Joy-Con (R)
    public bool ButtonR;
    public bool ButtonZR;
    public Vector2 RightStick;
    
    // 共通ボタン
    public bool ButtonA; // 決定
    public bool ButtonB; // キャンセル
    public bool ButtonX;
    public bool ButtonY;
}
```

## 🔧 プロジェクト設定（Switch向け最適化）

### 1. Project Settings の調整

```
Edit > Project Settings

Player Settings:
- Company Name: ClubNeko
- Product Name: Club Neko
- Default Icon: (猫アイコンを設定)

Resolution and Presentation:
- Default Screen Width: 1280 (携帯モード)
- Default Screen Height: 720
- Supported Aspect Ratios: 16:9 のみ

Graphics:
- Graphics APIs: Vulkan (Switchに近い)
- Texture Quality: Half Res
- Anisotropic Textures: Disabled
- Anti Aliasing: 2x Multi Sampling

Quality Settings:
- Pixel Light Count: 2
- Texture Quality: Half
- Shadows: Hard Shadows Only
- Shadow Resolution: Medium
- V Sync Count: Every V Blank
```

### 2. Build Settings

```
File > Build Settings

Platform: (開発中はStandalone/iOS)
- 実際のSwitchビルドは承認後のみ

Compression Method: LZ4
```

## 📦 必要なUnityパッケージ

標準的なSwitch対応パッケージ：

```json
{
  "dependencies": {
    "com.unity.inputsystem": "1.8.0",
    "com.unity.render-pipelines.universal": "17.0.0",
    "com.unity.burst": "1.8.0",
    "com.unity.collections": "2.4.0",
    "com.unity.mathematics": "1.3.0",
    "com.unity.jobs": "0.70.0"
  }
}
```

## 🎨 アセット最適化ガイドライン

### テクスチャ設定
- 最大サイズ: 2048x2048
- 圧縮: ASTC 6x6 または DXT5
- Mipmap: 有効

### モデル設定
- ポリゴン数: キャラクター 10,000以下
- ボーン数: 32以下
- マテリアル数: 3以下/モデル

### オーディオ設定
- 形式: Vorbis
- Quality: 70%
- Load Type: Compressed In Memory

## 🚀 開発の進め方

### 現時点で可能な作業：

1. **コアゲームプレイの実装**
   - Switch的な操作感の実現
   - 30/60 FPSでの安定動作

2. **最適化**
   - DrawCall削減
   - テクスチャアトラス化
   - LODシステム実装

3. **UIデザイン**
   - TV/携帯モード両対応
   - タッチ操作対応（携帯モード）

### Nintendo Developer登録後：

1. **実機テスト**
   - NDEV開発機材の入手
   - 実機でのパフォーマンステスト

2. **認証要件**
   - 年齢レーティング (CERO)
   - ロットチェック準備

## 📞 お問い合わせ

**Nintendo Developer Support**
- Email: developer.nintendo.com でサポートチケット
- 日本: 任天堂開発者サポート

## 🔗 参考リンク

- [Nintendo Developer Portal](https://developer.nintendo.com/)
- [Unity Nintendo Switch](https://unity.com/platforms/nintendo-switch)
- [Nintendo Developers Guidelines](https://developer.nintendo.com/guidelines)

---

**注意**: 実際のNintendo Switch向けビルドには、任天堂との契約とSDKアクセスが必須です。
このドキュメントは準備段階のガイドラインです。