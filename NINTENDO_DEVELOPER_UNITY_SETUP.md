# Nintendo Developer と Unity の連携設定

## 🔴 Switch Build Support が表示されない場合の対処法

### 1. Nintendo Developer Portal での Unity 連携

1. **Nintendo Developer Portal にログイン**
   ```
   https://developer.nintendo.com/
   ```

2. **NDI (Nintendo Dev Interface) にアクセス**
   - ダッシュボード → My Account
   - Unity Integration を確認

3. **Unity Organization ID の連携**
   - Unity Dashboard: https://id.unity.com/
   - Organization Settings → Organization ID をコピー
   - NDI で Unity Organization ID を登録

### 2. Unity での認証設定

#### Unity Hub での設定：
1. **Unity Hub → Preferences**
2. **Unity Account → Sign Out**
3. **再度 Sign In**
4. **Organization を正しく選択**
   - Nintendo Developer と連携済みの Organization を選択

#### Unity Editor での設定：
1. **Unity を開く**
2. **Edit → Project Settings → Services**
3. **Organization を確認**
4. **Services → Unity Collaborate を有効化**

### 3. Nintendo SDK の手動インストール

NDI からダウンロード：
1. **Unity for Nintendo Switch Support** パッケージ
2. **Nintendo SDK for Unity**

インストール手順：
```bash
# 1. ダウンロードしたファイルを解凍
unzip UnityForNintendoSwitch_*.zip

# 2. Unity Packageをインポート
# Unity Editor で:
# Assets → Import Package → Custom Package
# ダウンロードした .unitypackage を選択
```

## 🟡 代替案：Switch相当の仕様で開発を続ける

Switch Build Support が利用できない間も、以下の設定で開発可能：

### プラットフォーム設定（代替）
```
Build Settings:
- Platform: PC, Mac & Linux Standalone
- Architecture: x86_64
```

### Switch エミュレーション設定
```csharp
// Assets/_Project/Scripts/Platform/SwitchEmulator.cs
#if !UNITY_SWITCH
public static class SwitchEmulator
{
    public static void ApplySwitchLimitations()
    {
        // 解像度制限
        Screen.SetResolution(1280, 720, true);
        
        // FPS制限
        Application.targetFrameRate = 30;
        
        // メモリ制限シミュレーション
        System.GC.AddMemoryPressure(3_500_000_000);
    }
}
#endif
```

## 🔵 NDI アクセス確認チェックリスト

### アカウント状態の確認：
- [ ] Nintendo Developer アカウントが有効
- [ ] NDI にログインできる
- [ ] Developer Agreement に同意済み
- [ ] Unity Integration が「Active」

### Unity 側の確認：
- [ ] Unity ID でログイン中
- [ ] 正しい Organization を選択
- [ ] Unity Pro/Plus ライセンス（推奨）

### トラブルシューティング：

#### エラー: "Platform module not available"
```
原因: Nintendo との契約が未完了
対処: 
1. Nintendo Developer Portal で申請状態を確認
2. 承認メールを確認
3. 必要書類の提出を確認
```

#### エラー: "Organization not authorized"
```
原因: Unity Organization が Nintendo と未連携
対処:
1. Unity Dashboard で Organization ID 確認
2. NDI で Unity Organization を登録
3. 24時間待つ（反映に時間がかかる）
```

## 📧 サポート連絡先

### Nintendo Developer Support
- サポートチケット: NDI 内から送信
- メール返信: 通常 2-3 営業日

### Unity Support
- Unity Forum: https://forum.unity.com/
- Support Ticket: https://support.unity.com/

## 🚀 今すぐできること

Switch Build Support を待たずに進められる作業：

1. **ゲームロジックの実装**
   - すべてのゲームプレイ機能
   - UI/UX の完成
   - アセットの準備

2. **パフォーマンス最適化**
   - DrawCall 削減
   - テクスチャ最適化
   - メモリ使用量の調整

3. **コントローラー対応**
   - Input System でのJoy-Con エミュレーション
   - Pro Controller 配置での開発

4. **Switch 仕様での調整**
   - 720p/1080p 解像度対応
   - 30/60 FPS ターゲット
   - 3.5GB メモリ制限

---

**重要**: Nintendo Developer Program の承認には時間がかかる場合があります。
その間も通常のプラットフォームで開発を進め、承認後にSwitch ビルドに切り替えることを推奨します。