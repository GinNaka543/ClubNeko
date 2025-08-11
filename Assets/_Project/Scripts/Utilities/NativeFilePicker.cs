using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace ClubNeko.Utilities
{
    /// <summary>
    /// ネイティブファイルピッカーのインターフェース
    /// iOS, Android, デスクトップ対応
    /// </summary>
    public static class NativeFilePicker
    {
        public delegate void FilePickerCallback(string path);
        
        #if UNITY_IOS && !UNITY_EDITOR
        
        [DllImport("__Internal")]
        private static extern void _ShowVRMPicker(FilePickerCallback callback);
        
        [DllImport("__Internal")]
        private static extern bool _IsICloudAvailable();
        
        [DllImport("__Internal")]
        private static extern string _GetDocumentsPath();
        
        #endif
        
        private static FilePickerCallback currentCallback;
        
        /// <summary>
        /// ファイルピッカーを表示（VRMファイル用）
        /// </summary>
        public static void PickVRMFile(Action<string> callback)
        {
            PickFile(callback, new string[] { ".vrm" });
        }
        
        /// <summary>
        /// ファイルピッカーを表示（汎用）
        /// </summary>
        public static void PickFile(Action<string> callback, string[] allowedExtensions)
        {
            #if UNITY_IOS && !UNITY_EDITOR
            currentCallback = (path) => {
                if (callback != null) {
                    UnityMainThreadDispatcher.Instance.Enqueue(() => callback(path));
                }
            };
            _ShowVRMPicker(currentCallback);
            
            #elif UNITY_ANDROID && !UNITY_EDITOR
            PickFileAndroid(callback, allowedExtensions);
            
            #elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            PickFileWindows(callback, allowedExtensions);
            
            #elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            PickFileMac(callback, allowedExtensions);
            
            #else
            Debug.LogError("File picker not supported on this platform");
            callback?.Invoke(null);
            #endif
        }
        
        #region Android Implementation
        
        private static void PickFileAndroid(Action<string> callback, string[] extensions)
        {
            #if UNITY_ANDROID && !UNITY_EDITOR
            using (AndroidJavaClass unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject currentActivity = unityClass.GetStatic<AndroidJavaObject>("currentActivity"))
            using (AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent"))
            using (AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent"))
            {
                intent.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_GET_CONTENT"));
                intent.Call<AndroidJavaObject>("setType", "*/*");
                
                // MIMEタイプの設定
                if (extensions != null && extensions.Length > 0)
                {
                    string[] mimeTypes = new string[extensions.Length];
                    for (int i = 0; i < extensions.Length; i++)
                    {
                        if (extensions[i] == ".vrm")
                        {
                            mimeTypes[i] = "application/octet-stream";
                        }
                        else
                        {
                            mimeTypes[i] = "*/*";
                        }
                    }
                    intent.Call<AndroidJavaObject>("putExtra", "android.intent.extra.MIME_TYPES", mimeTypes);
                }
                
                AndroidFilePickerCallback androidCallback = new AndroidFilePickerCallback(callback);
                currentActivity.Call("startActivityForResult", intent, 1001);
            }
            #endif
        }
        
        #endregion
        
        #region Windows Implementation
        
        private static void PickFileWindows(Action<string> callback, string[] extensions)
        {
            #if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            
            #if UNITY_EDITOR
            // Editorでの実装
            string path = UnityEditor.EditorUtility.OpenFilePanel(
                "Select File",
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop),
                extensions != null && extensions.Length > 0 ? extensions[0].TrimStart('.') : "*"
            );
            callback?.Invoke(string.IsNullOrEmpty(path) ? null : path);
            
            #else
            // Standaloneビルドでの実装
            // Windows.Forms を使用するか、ネイティブWin32 APIを使用
            // ここでは簡略化のため、SimpleFileBrowser等のアセットの使用を推奨
            Debug.LogWarning("Standalone Windows file picker requires additional implementation");
            callback?.Invoke(null);
            #endif
            
            #endif
        }
        
        #endregion
        
        #region Mac Implementation
        
        private static void PickFileMac(Action<string> callback, string[] extensions)
        {
            #if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            
            #if UNITY_EDITOR
            // Editorでの実装
            string path = UnityEditor.EditorUtility.OpenFilePanel(
                "Select File",
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop),
                extensions != null && extensions.Length > 0 ? extensions[0].TrimStart('.') : "*"
            );
            callback?.Invoke(string.IsNullOrEmpty(path) ? null : path);
            
            #else
            // Standaloneビルドでの実装
            // macOS用のネイティブファイルダイアログ
            Debug.LogWarning("Standalone macOS file picker requires additional implementation");
            callback?.Invoke(null);
            #endif
            
            #endif
        }
        
        #endregion
        
        /// <summary>
        /// iCloudが利用可能かチェック（iOS）
        /// </summary>
        public static bool IsICloudAvailable()
        {
            #if UNITY_IOS && !UNITY_EDITOR
            return _IsICloudAvailable();
            #else
            return false;
            #endif
        }
        
        /// <summary>
        /// ドキュメントディレクトリのパスを取得
        /// </summary>
        public static string GetDocumentsPath()
        {
            #if UNITY_IOS && !UNITY_EDITOR
            return _GetDocumentsPath();
            #else
            return Application.persistentDataPath;
            #endif
        }
    }
    
    #if UNITY_ANDROID && !UNITY_EDITOR
    /// <summary>
    /// Android用のファイルピッカーコールバック
    /// </summary>
    public class AndroidFilePickerCallback : AndroidJavaProxy
    {
        private Action<string> callback;
        
        public AndroidFilePickerCallback(Action<string> callback) : base("android.content.Intent")
        {
            this.callback = callback;
        }
        
        public void onActivityResult(int requestCode, int resultCode, AndroidJavaObject data)
        {
            if (requestCode == 1001 && resultCode == -1) // RESULT_OK = -1
            {
                AndroidJavaObject uri = data.Call<AndroidJavaObject>("getData");
                if (uri != null)
                {
                    string path = uri.Call<string>("getPath");
                    callback?.Invoke(path);
                }
                else
                {
                    callback?.Invoke(null);
                }
            }
            else
            {
                callback?.Invoke(null);
            }
        }
    }
    #endif
    
    /// <summary>
    /// メインスレッドディスパッチャー
    /// </summary>
    public class UnityMainThreadDispatcher : MonoBehaviour
    {
        private static UnityMainThreadDispatcher instance;
        private System.Collections.Generic.Queue<Action> actionQueue = new System.Collections.Generic.Queue<Action>();
        
        public static UnityMainThreadDispatcher Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("UnityMainThreadDispatcher");
                    instance = go.AddComponent<UnityMainThreadDispatcher>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }
        
        public void Enqueue(Action action)
        {
            lock (actionQueue)
            {
                actionQueue.Enqueue(action);
            }
        }
        
        private void Update()
        {
            lock (actionQueue)
            {
                while (actionQueue.Count > 0)
                {
                    actionQueue.Dequeue()?.Invoke();
                }
            }
        }
    }
}