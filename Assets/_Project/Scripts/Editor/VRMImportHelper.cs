#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

namespace ClubNeko.Editor
{
    /// <summary>
    /// VRMインポートヘルパー（簡略版）
    /// </summary>
    public static class VRMImportHelper
    {
        [MenuItem("Club Neko/Import VRM Package")]
        public static void ImportVRMPackage()
        {
            string packagePath = Path.Combine(Application.dataPath, "../VRM-0.129.3_ddf2.unitypackage");
            
            if (File.Exists(packagePath))
            {
                Debug.Log($"🎮 Importing VRM package: {packagePath}");
                AssetDatabase.ImportPackage(packagePath, true);
                Debug.Log("✅ VRM package import dialog opened.");
            }
            else
            {
                Debug.LogError($"❌ VRM package not found at: {packagePath}");
                EditorUtility.DisplayDialog("VRM Package Not Found", 
                    "Please ensure VRM-0.129.3_ddf2.unitypackage is in the project root folder.", "OK");
            }
        }
        
        [MenuItem("Club Neko/Check VRM Installation")]
        public static void CheckVRMInstallation()
        {
            Debug.Log("=== VRM Installation Check ===");
            
            bool vrmExists = Directory.Exists(Path.Combine(Application.dataPath, "VRM"));
            bool vrm10Exists = Directory.Exists(Path.Combine(Application.dataPath, "VRM10"));
            bool uniGLTFExists = Directory.Exists(Path.Combine(Application.dataPath, "UniGLTF"));
            
            Debug.Log($"VRM Folder: {(vrmExists ? "✅ Found" : "❌ Missing")}");
            Debug.Log($"VRM10 Folder: {(vrm10Exists ? "✅ Found" : "❌ Missing")}");
            Debug.Log($"UniGLTF Folder: {(uniGLTFExists ? "✅ Found" : "❌ Missing")}");
            
            if (vrmExists || vrm10Exists)
            {
                Debug.Log("🎮 VRM is ready to use!");
                EditorUtility.DisplayDialog("VRM Status", "✅ VRM is installed and ready to use!", "OK");
            }
            else
            {
                Debug.Log("⚠️ VRM not installed. Use 'Import VRM Package' first.");
                EditorUtility.DisplayDialog("VRM Status", "❌ VRM not found. Please import VRM package first.", "OK");
            }
        }
        
        [MenuItem("Club Neko/Open VRM Package Location")]
        public static void OpenVRMPackageLocation()
        {
            string packagePath = Path.Combine(Application.dataPath, "../VRM-0.129.3_ddf2.unitypackage");
            string directory = Path.GetDirectoryName(packagePath);
            
            if (Directory.Exists(directory))
            {
                EditorUtility.RevealInFinder(packagePath);
            }
            else
            {
                Debug.LogError("Package directory not found.");
            }
        }
    }
}
#endif