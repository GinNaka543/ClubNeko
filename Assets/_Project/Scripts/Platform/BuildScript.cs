using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.Collections.Generic;

namespace ClubNeko.Build
{
    /// <summary>
    /// Nintendo Switch向けビルドスクリプト
    /// </summary>
    public class BuildScript
    {
        private const string GAME_NAME = "ClubNeko";
        private const string COMPANY_NAME = "ClubNekoStudio";
        
        // ビルド出力パス
        private static readonly string BUILD_PATH = Path.Combine(Application.dataPath, "../Builds");
        private static readonly string SWITCH_BUILD_PATH = Path.Combine(BUILD_PATH, "Switch");
        
        [MenuItem("Club Neko/Build/Switch Development")]
        public static void BuildSwitchDev()
        {
            BuildSwitch(true);
        }
        
        [MenuItem("Club Neko/Build/Switch Release")]
        public static void BuildSwitchRelease()
        {
            BuildSwitch(false);
        }
        
        private static void BuildSwitch(bool isDevelopment)
        {
            Debug.Log($"========== Starting Nintendo Switch {(isDevelopment ? "Development" : "Release")} Build ==========");
            
            // ビルドパスの準備
            string outputPath = Path.Combine(SWITCH_BUILD_PATH, isDevelopment ? "Dev" : "Release");
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }
            
            // ビルド対象シーンの取得
            List<string> scenes = GetBuildScenes();
            
            // Player Settings の設定
            ConfigureSwitchPlayerSettings(isDevelopment);
            
            // ビルドオプション
            BuildOptions buildOptions = BuildOptions.None;
            if (isDevelopment)
            {
                buildOptions |= BuildOptions.Development;
                buildOptions |= BuildOptions.AllowDebugging;
                buildOptions |= BuildOptions.ConnectWithProfiler;
            }
            else
            {
                buildOptions |= BuildOptions.CompressWithLz4;
            }
            
            // ビルドターゲットの設定
            BuildTarget buildTarget = BuildTarget.Switch;
            BuildTargetGroup targetGroup = BuildTargetGroup.Switch;
            
            // ビルド実行
            string outputFile = Path.Combine(outputPath, $"{GAME_NAME}.nsp");
            
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes.ToArray(),
                locationPathName = outputFile,
                target = buildTarget,
                targetGroup = targetGroup,
                options = buildOptions
            };
            
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;
            
            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"✅ Build succeeded: {outputFile}");
                Debug.Log($"   Total time: {summary.totalTime}");
                Debug.Log($"   Total size: {summary.totalSize / 1024 / 1024} MB");
                
                // ビルド後処理
                PostBuildProcess(outputPath, isDevelopment);
            }
            else
            {
                Debug.LogError($"❌ Build failed: {summary.result}");
                throw new Exception("Build failed!");
            }
        }
        
        private static void ConfigureSwitchPlayerSettings(bool isDevelopment)
        {
            // 基本設定
            PlayerSettings.companyName = COMPANY_NAME;
            PlayerSettings.productName = GAME_NAME;
            PlayerSettings.bundleVersion = "1.0.0";
            
            // Switch固有の設定
            #if UNITY_SWITCH
            PlayerSettings.Switch.applicationID = "0x0100000000000001"; // 実際のIDに置き換え
            PlayerSettings.Switch.releaseVersion = "1.0.0";
            PlayerSettings.Switch.displayVersion = "1.0.0";
            
            // パフォーマンス設定
            if (isDevelopment)
            {
                // 開発ビルド設定
                PlayerSettings.Switch.isDebugBuild = true;
                PlayerSettings.Switch.enableNEXPlugin = true; // NEXプロファイリング
            }
            else
            {
                // リリースビルド設定
                PlayerSettings.Switch.isDebugBuild = false;
                PlayerSettings.Switch.enableNEXPlugin = false;
            }
            
            // レーティング設定
            PlayerSettings.Switch.ratingAge = 12; // CERO B相当
            #else
            Debug.Log("[Build] Switch-specific settings skipped (Switch platform not available)");
            #endif
            
            // アイコン設定
            SetSwitchIcons();
            
            // スプラッシュ画面
            PlayerSettings.SplashScreen.show = true;
            PlayerSettings.SplashScreen.showUnityLogo = false;
            
            // 最適化設定
            #if UNITY_SWITCH
                #if UNITY_2021_2_OR_NEWER
                var namedBuildTarget = UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(BuildTargetGroup.Switch);
                PlayerSettings.SetScriptingBackend(namedBuildTarget, ScriptingImplementation.IL2CPP);
                PlayerSettings.SetApiCompatibilityLevel(namedBuildTarget, ApiCompatibilityLevel.NET_Standard_2_0);
                #else
                PlayerSettings.SetScriptingBackend(BuildTargetGroup.Switch, ScriptingImplementation.IL2CPP);
                PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Switch, ApiCompatibilityLevel.NET_Standard_2_0);
                #endif
            #else
                Debug.Log("[Build] Switch optimization settings skipped (Switch platform not available)");
            #endif
            PlayerSettings.stripEngineCode = !isDevelopment;
            
            Debug.Log("✅ Player Settings configured for Nintendo Switch");
        }
        
        private static void SetSwitchIcons()
        {
            #if UNITY_SWITCH
            // アイコンパス
            string iconPath = "Assets/_Project/Icons/";
            // 各サイズのアイコンを設定
            // Small (40x40)
            Texture2D smallIcon = AssetDatabase.LoadAssetAtPath<Texture2D>($"{iconPath}icon_small.png");
            if (smallIcon != null)
            {
                PlayerSettings.Switch.smallIcon = smallIcon;
            }
            
            // Large (256x256)
            Texture2D largeIcon = AssetDatabase.LoadAssetAtPath<Texture2D>($"{iconPath}icon_large.png");
            if (largeIcon != null)
            {
                PlayerSettings.Switch.largeIcon = largeIcon;
            }
            #else
            Debug.Log("[Build] Switch icons can only be set when Switch platform is available");
            #endif
        }
        
        private static List<string> GetBuildScenes()
        {
            List<string> scenes = new List<string>();
            
            // EditorBuildSettingsからシーンを取得
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                {
                    scenes.Add(scene.path);
                    Debug.Log($"  Added scene: {scene.path}");
                }
            }
            
            // シーンが設定されていない場合はデフォルトシーンを追加
            if (scenes.Count == 0)
            {
                scenes.Add("Assets/Scenes/MainMenu/MainMenu.unity");
                scenes.Add("Assets/Scenes/Golf/GolfCourse.unity");
                scenes.Add("Assets/Scenes/House/House.unity");
            }
            
            return scenes;
        }
        
        private static void PostBuildProcess(string outputPath, bool isDevelopment)
        {
            Debug.Log("========== Post Build Process ==========");
            
            // メタデータファイルの生成
            GenerateBuildMetadata(outputPath, isDevelopment);
            
            // リリースノートの生成
            if (!isDevelopment)
            {
                GenerateReleaseNotes(outputPath);
            }
            
            // ビルドフォルダを開く
            EditorUtility.RevealInFinder(outputPath);
        }
        
        private static void GenerateBuildMetadata(string outputPath, bool isDevelopment)
        {
            string metadataPath = Path.Combine(outputPath, "build_info.txt");
            
            string metadata = $@"Club Neko - Nintendo Switch Build
=====================================
Build Type: {(isDevelopment ? "Development" : "Release")}
Build Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
Unity Version: {Application.unityVersion}
Platform: Nintendo Switch
Company: {COMPANY_NAME}
Product: {GAME_NAME}
Version: 1.0.0

Performance Targets:
- Handheld: 1280x720 @ 30 FPS
- Docked: 1920x1080 @ 60 FPS
- Memory Limit: 3.5 GB

Build Configuration:
- Scripting Backend: IL2CPP
- API Level: .NET Standard 2.0
- Compression: LZ4
=====================================";
            
            File.WriteAllText(metadataPath, metadata);
            Debug.Log($"✅ Build metadata saved: {metadataPath}");
        }
        
        private static void GenerateReleaseNotes(string outputPath)
        {
            string releaseNotesPath = Path.Combine(outputPath, "RELEASE_NOTES.md");
            
            string releaseNotes = $@"# Club Neko - Release Notes

## Version 1.0.0
Release Date: {DateTime.Now:yyyy-MM-dd}

### Features
- ゴルフバトルシステム
- VRMキャラクター対応
- ハウスシステム
- マルチプレイヤー対応

### Known Issues
- 初回リリース

### Performance
- 携帯モード: 30 FPS安定
- TVモード: 60 FPS目標

### Requirements
- Nintendo Switch (全モデル対応)
- 空き容量: 2GB以上
- インターネット接続（マルチプレイ時）";
            
            File.WriteAllText(releaseNotesPath, releaseNotes);
            Debug.Log($"✅ Release notes saved: {releaseNotesPath}");
        }
        
        [MenuItem("Club Neko/Build/Clean Build Cache")]
        public static void CleanBuildCache()
        {
            if (Directory.Exists(BUILD_PATH))
            {
                Directory.Delete(BUILD_PATH, true);
                Debug.Log("✅ Build cache cleaned");
            }
            
            // Library/BuildCacheもクリア
            string buildCachePath = Path.Combine(Application.dataPath, "../Library/BuildCache");
            if (Directory.Exists(buildCachePath))
            {
                Directory.Delete(buildCachePath, true);
                Debug.Log("✅ Library build cache cleaned");
            }
        }
        
        [MenuItem("Club Neko/Build/Validate Switch Settings")]
        public static void ValidateSwitchSettings()
        {
            Debug.Log("========== Validating Switch Settings ==========");
            
            bool isValid = true;
            
            // Platform check
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Switch)
            {
                Debug.LogWarning("⚠️ Build target is not set to Switch");
                isValid = false;
            }
            
            // Graphics API check
            var graphicsAPIs = PlayerSettings.GetGraphicsAPIs(BuildTarget.Switch);
            Debug.Log($"Graphics APIs: {string.Join(", ", graphicsAPIs)}");
            
            // Memory settings
            #if UNITY_SWITCH
                #if UNITY_2021_2_OR_NEWER
                var namedBuildTarget = UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(BuildTargetGroup.Switch);
                Debug.Log($"Scripting Backend: {PlayerSettings.GetScriptingBackend(namedBuildTarget)}");
                Debug.Log($"API Compatibility: {PlayerSettings.GetApiCompatibilityLevel(namedBuildTarget)}");
                #else
                Debug.Log($"Scripting Backend: {PlayerSettings.GetScriptingBackend(BuildTargetGroup.Switch)}");
                Debug.Log($"API Compatibility: {PlayerSettings.GetApiCompatibilityLevel(BuildTargetGroup.Switch)}");
                #endif
            #else
                Debug.Log("[Build] Switch settings validation skipped (Switch platform not available)");
            #endif
            
            // Icons check
            #if UNITY_SWITCH
            if (PlayerSettings.Switch.smallIcon == null || PlayerSettings.Switch.largeIcon == null)
            {
                Debug.LogWarning("⚠️ Switch icons are not set");
                isValid = false;
            }
            #endif
            
            if (isValid)
            {
                Debug.Log("✅ All Switch settings are valid!");
            }
            else
            {
                Debug.LogError("❌ Some Switch settings need attention");
            }
        }
    }
}