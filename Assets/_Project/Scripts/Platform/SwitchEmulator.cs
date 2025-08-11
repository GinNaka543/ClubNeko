using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace ClubNeko.Platform
{
    /// <summary>
    /// Switch Build Support がない環境でSwitch仕様をエミュレート
    /// </summary>
    public class SwitchEmulator : MonoBehaviour
    {
        [Header("Emulation Settings")]
        public bool enableSwitchEmulation = true;
        public bool showDebugOverlay = true;
        
        [Header("Performance Mode")]
        public SwitchMode currentMode = SwitchMode.Handheld;
        
        private Canvas debugCanvas;
        private Text debugText;
        
        private static SwitchEmulator instance;
        public static SwitchEmulator Instance => instance;
        
        private void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
                return;
            }
            
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            #if !UNITY_SWITCH
            if (enableSwitchEmulation)
            {
                Debug.Log("🎮 Switch Emulation Mode Active");
                ApplySwitchSettings();
                CreateDebugOverlay();
            }
            #endif
        }
        
        private void ApplySwitchSettings()
        {
            switch (currentMode)
            {
                case SwitchMode.Handheld:
                    ApplyHandheldMode();
                    break;
                case SwitchMode.Docked:
                    ApplyDockedMode();
                    break;
            }
            
            // 共通設定
            QualitySettings.vSyncCount = 1;
            QualitySettings.maxQueuedFrames = 2;
            
            Debug.Log($"✅ Applied Switch {currentMode} mode settings");
        }
        
        private void ApplyHandheldMode()
        {
            // 携帯モード: 1280x720 @ 30 FPS
            if (!Application.isEditor)
            {
                Screen.SetResolution(1280, 720, true);
            }
            Application.targetFrameRate = 30;
            
            // グラフィック設定
            QualitySettings.pixelLightCount = 2;
            QualitySettings.shadows = ShadowQuality.HardOnly;
            QualitySettings.shadowDistance = 20f;
            QualitySettings.antiAliasing = 0;
            QualitySettings.globalTextureMipmapLimit = 1; // Half texture resolution
            
            // メモリ制限シミュレーション
            System.GC.AddMemoryPressure(2_000_000_000); // 2GB
        }
        
        private void ApplyDockedMode()
        {
            // TVモード: 1920x1080 @ 60 FPS
            if (!Application.isEditor)
            {
                Screen.SetResolution(1920, 1080, true);
            }
            Application.targetFrameRate = 60;
            
            // グラフィック設定
            QualitySettings.pixelLightCount = 4;
            QualitySettings.shadows = ShadowQuality.All;
            QualitySettings.shadowDistance = 40f;
            QualitySettings.antiAliasing = 2;
            QualitySettings.globalTextureMipmapLimit = 0; // Full texture resolution
            
            // メモリ制限シミュレーション
            System.GC.AddMemoryPressure(3_500_000_000); // 3.5GB
        }
        
        private void CreateDebugOverlay()
        {
            if (!showDebugOverlay) return;
            
            // デバッグキャンバス作成
            GameObject canvasObj = new GameObject("Switch Debug Overlay");
            canvasObj.transform.SetParent(transform);
            
            debugCanvas = canvasObj.AddComponent<Canvas>();
            debugCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            debugCanvas.sortingOrder = 9999;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // デバッグテキスト
            GameObject textObj = new GameObject("Debug Text");
            textObj.transform.SetParent(canvasObj.transform);
            
            RectTransform rect = textObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(10, -10);
            rect.sizeDelta = new Vector2(400, 150);
            
            debugText = textObj.AddComponent<Text>();
            debugText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            debugText.fontSize = 14;
            debugText.color = Color.green;
            
            // 背景
            Image bg = textObj.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.7f);
            
            StartCoroutine(UpdateDebugInfo());
        }
        
        private IEnumerator UpdateDebugInfo()
        {
            while (true)
            {
                if (debugText != null)
                {
                    float fps = 1.0f / Time.deltaTime;
                    long memory = System.GC.GetTotalMemory(false) / 1024 / 1024;
                    
                    debugText.text = $"🎮 SWITCH EMULATION\n";
                    debugText.text += $"Mode: {currentMode}\n";
                    debugText.text += $"Resolution: {Screen.width}x{Screen.height}\n";
                    debugText.text += $"FPS: {fps:F1} / {Application.targetFrameRate}\n";
                    debugText.text += $"Memory: {memory} MB / {(currentMode == SwitchMode.Handheld ? 2000 : 3500)} MB\n";
                    debugText.text += $"Quality: {QualitySettings.names[QualitySettings.GetQualityLevel()]}\n";
                    debugText.text += $"[F9] Toggle Mode | [F10] Hide Overlay";
                }
                
                yield return new WaitForSeconds(0.5f);
            }
        }
        
        private void Update()
        {
            // デバッグキー
            if (UnityEngine.Input.GetKeyDown(KeyCode.F9))
            {
                ToggleMode();
            }
            
            if (UnityEngine.Input.GetKeyDown(KeyCode.F10))
            {
                if (debugCanvas != null)
                {
                    debugCanvas.gameObject.SetActive(!debugCanvas.gameObject.activeSelf);
                }
            }
        }
        
        public void ToggleMode()
        {
            currentMode = currentMode == SwitchMode.Handheld ? SwitchMode.Docked : SwitchMode.Handheld;
            ApplySwitchSettings();
        }
        
        // エディタメニュー
        [UnityEditor.MenuItem("Club Neko/Switch Emulator/Enable Handheld Mode")]
        private static void EnableHandheldMode()
        {
            if (Instance != null)
            {
                Instance.currentMode = SwitchMode.Handheld;
                Instance.ApplySwitchSettings();
            }
        }
        
        [UnityEditor.MenuItem("Club Neko/Switch Emulator/Enable Docked Mode")]
        private static void EnableDockedMode()
        {
            if (Instance != null)
            {
                Instance.currentMode = SwitchMode.Docked;
                Instance.ApplySwitchSettings();
            }
        }
    }
    
    public enum SwitchMode
    {
        Handheld,  // 携帯モード
        Docked     // TVモード
    }
}