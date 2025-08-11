using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ClubNeko.Platform
{
    /// <summary>
    /// Nintendo Switch向け最適化設定
    /// </summary>
    public class SwitchOptimization : MonoBehaviour
    {
        [Header("Performance Settings")]
        public bool autoDetectMode = true;
        public PerformanceMode currentMode = PerformanceMode.Handheld;
        
        [Header("Quality Presets")]
        public QualityPreset handheldPreset;
        public QualityPreset dockedPreset;
        
        [Header("Optimization")]
        public bool dynamicResolution = true;
        public float minResolutionScale = 0.5f;
        public float maxResolutionScale = 1.0f;
        
        private static SwitchOptimization instance;
        public static SwitchOptimization Instance => instance;
        
        private void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
                return;
            }
            
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeOptimization();
        }
        
        private void InitializeOptimization()
        {
            // デフォルトプリセットの設定
            SetupDefaultPresets();
            
            // 初期モードの適用
            ApplyPerformanceMode(currentMode);
            
            // 動的解像度の設定
            if (dynamicResolution)
            {
                SetupDynamicResolution();
            }
        }
        
        private void SetupDefaultPresets()
        {
            // 携帯モードプリセット
            handheldPreset = new QualityPreset
            {
                targetFPS = 30,
                resolution = new Vector2Int(1280, 720),
                textureQuality = 1, // Half
                shadowQuality = UnityEngine.ShadowQuality.HardOnly,
                shadowDistance = 20f,
                antiAliasing = 0,
                anisotropicFiltering = AnisotropicFiltering.Disable,
                pixelLightCount = 2,
                renderScale = 0.75f
            };
            
            // ドックモードプリセット
            dockedPreset = new QualityPreset
            {
                targetFPS = 60,
                resolution = new Vector2Int(1920, 1080),
                textureQuality = 0, // Full
                shadowQuality = UnityEngine.ShadowQuality.All,
                shadowDistance = 40f,
                antiAliasing = 2,
                anisotropicFiltering = AnisotropicFiltering.Enable,
                pixelLightCount = 4,
                renderScale = 1.0f
            };
        }
        
        public void ApplyPerformanceMode(PerformanceMode mode)
        {
            currentMode = mode;
            QualityPreset preset = mode == PerformanceMode.Handheld ? handheldPreset : dockedPreset;
            
            // FPS設定
            Application.targetFrameRate = preset.targetFPS;
            
            // 解像度設定
            if (!Application.isEditor)
            {
                Screen.SetResolution(preset.resolution.x, preset.resolution.y, true);
            }
            
            // テクスチャ品質
            QualitySettings.globalTextureMipmapLimit = preset.textureQuality;
            
            // 影の設定
            QualitySettings.shadows = preset.shadowQuality;
            QualitySettings.shadowDistance = preset.shadowDistance;
            
            // アンチエイリアシング
            QualitySettings.antiAliasing = preset.antiAliasing;
            
            // 異方性フィルタリング
            QualitySettings.anisotropicFiltering = preset.anisotropicFiltering;
            
            // ピクセルライト数
            QualitySettings.pixelLightCount = preset.pixelLightCount;
            
            // レンダースケール（URP）
            SetRenderScale(preset.renderScale);
            
            Debug.Log($"[Switch Optimization] Applied {mode} mode settings");
        }
        
        private void SetRenderScale(float scale)
        {
            // Universal Render Pipeline Assetの取得と設定
            var urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (urpAsset != null)
            {
                urpAsset.renderScale = scale;
            }
        }
        
        private void SetupDynamicResolution()
        {
            // 動的解像度の有効化
            QualitySettings.resolutionScalingFixedDPIFactor = 1.0f;
            
            StartCoroutine(DynamicResolutionUpdate());
        }
        
        private IEnumerator DynamicResolutionUpdate()
        {
            while (true)
            {
                float currentFPS = 1.0f / Time.deltaTime;
                float targetFPS = currentMode == PerformanceMode.Handheld ? 30f : 60f;
                
                if (currentFPS < targetFPS * 0.95f)
                {
                    // パフォーマンス低下時は解像度を下げる
                    AdjustRenderScale(-0.05f);
                }
                else if (currentFPS > targetFPS * 1.05f)
                {
                    // 余裕がある時は解像度を上げる
                    AdjustRenderScale(0.02f);
                }
                
                yield return new WaitForSeconds(1f);
            }
        }
        
        private void AdjustRenderScale(float delta)
        {
            var urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (urpAsset != null)
            {
                float newScale = Mathf.Clamp(
                    urpAsset.renderScale + delta,
                    minResolutionScale,
                    maxResolutionScale
                );
                urpAsset.renderScale = newScale;
            }
        }
        
        // エディタでのモード切り替えテスト用
        [ContextMenu("Switch to Handheld Mode")]
        public void SwitchToHandheld()
        {
            ApplyPerformanceMode(PerformanceMode.Handheld);
        }
        
        [ContextMenu("Switch to Docked Mode")]
        public void SwitchToDocked()
        {
            ApplyPerformanceMode(PerformanceMode.Docked);
        }
    }
    
    public enum PerformanceMode
    {
        Handheld,  // 携帯モード
        Docked     // TVモード
    }
    
    [System.Serializable]
    public class QualityPreset
    {
        public int targetFPS = 30;
        public Vector2Int resolution = new Vector2Int(1280, 720);
        public int textureQuality = 1;
        public UnityEngine.ShadowQuality shadowQuality = UnityEngine.ShadowQuality.HardOnly;
        public float shadowDistance = 20f;
        public int antiAliasing = 0;
        public AnisotropicFiltering anisotropicFiltering = AnisotropicFiltering.Disable;
        public int pixelLightCount = 2;
        public float renderScale = 1.0f;
    }
    
    /// <summary>
    /// Switch向けメモリ管理
    /// </summary>
    public static class SwitchMemoryManager
    {
        // Switchのメモリ制限
        public const long MAX_HEAP_SIZE = 3_500_000_000; // 3.5GB
        public const long TEXTURE_MEMORY_BUDGET = 1_000_000_000; // 1GB
        public const long MESH_MEMORY_BUDGET = 500_000_000; // 500MB
        
        public static void OptimizeMemory()
        {
            // 未使用アセットのアンロード
            Resources.UnloadUnusedAssets();
            
            // ガベージコレクション
            System.GC.Collect();
            
            // テクスチャキャッシュのクリア
            Caching.ClearCache();
        }
        
        public static long GetCurrentMemoryUsage()
        {
            return System.GC.GetTotalMemory(false);
        }
        
        public static bool IsMemoryPressureHigh()
        {
            return GetCurrentMemoryUsage() > MAX_HEAP_SIZE * 0.8f;
        }
    }
}