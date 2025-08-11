using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace ClubNeko.Character
{
    /// <summary>
    /// 実際のVRMローダー（VRMパッケージインストール後）
    /// </summary>
    public class VRMLoaderReal : MonoBehaviour
    {
        [Header("VRM Settings")]
        public bool debugMode = true;
        
        /// <summary>
        /// VRMファイルをロード（実装版）
        /// </summary>
        public async Task<GameObject> LoadVRM(string path)
        {
            if (debugMode)
            {
                Debug.Log($"[VRM Loader] Loading VRM: {path}");
            }
            
            try
            {
                #if UNIVRM_INSTALLED || VRM_INSTALLED
                
                // VRM 1.0の場合
                var bytes = await File.ReadAllBytesAsync(path);
                var vrm10Instance = await Vrm10.LoadBytesAsync(bytes);
                
                if (vrm10Instance != null)
                {
                    var gameObject = vrm10Instance.gameObject;
                    SetupVRMComponents(gameObject);
                    
                    if (debugMode)
                    {
                        Debug.Log($"✅ VRM loaded successfully: {gameObject.name}");
                    }
                    
                    return gameObject;
                }
                
                #endif
                
                // VRMパッケージがない場合はプレースホルダー
                Debug.LogWarning("VRM package not installed. Using placeholder.");
                await Task.Delay(100); // 非同期処理のシミュレーション
                return CreatePlaceholderCharacter(Path.GetFileNameWithoutExtension(path));
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ Failed to load VRM: {e.Message}");
                await Task.Delay(100);
                return CreatePlaceholderCharacter(Path.GetFileNameWithoutExtension(path));
            }
        }
        
        private void SetupVRMComponents(GameObject vrmObject)
        {
            // VRMオブジェクトにゲーム用コンポーネントを追加
            
            // コライダー追加
            if (vrmObject.GetComponent<Collider>() == null)
            {
                CapsuleCollider capsule = vrmObject.AddComponent<CapsuleCollider>();
                capsule.center = new Vector3(0, 1f, 0);
                capsule.radius = 0.3f;
                capsule.height = 2f;
            }
            
            // Rigidbody追加
            if (vrmObject.GetComponent<Rigidbody>() == null)
            {
                Rigidbody rb = vrmObject.AddComponent<Rigidbody>();
                rb.freezeRotation = true;
            }
            
            // アニメーター確認
            Animator animator = vrmObject.GetComponent<Animator>();
            if (animator == null)
            {
                animator = vrmObject.AddComponent<Animator>();
            }
            
            // ゲーム用タグ
            vrmObject.tag = "VRMCharacter";
        }
        
        private GameObject CreatePlaceholderCharacter(string name)
        {
            // プレースホルダー作成（既存のSimpleVRMLoaderから）
            GameObject character = new GameObject($"VRM_Placeholder_{name}");
            
            // シンプルなカプセルキャラクター
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(character.transform);
            body.transform.localPosition = new Vector3(0, 1f, 0);
            body.transform.localScale = new Vector3(0.5f, 1f, 0.5f);
            
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(character.transform);
            head.transform.localPosition = new Vector3(0, 2.2f, 0);
            head.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
            
            // ランダムな色
            Color randomColor = new Color(
                UnityEngine.Random.Range(0.5f, 1f),
                UnityEngine.Random.Range(0.5f, 1f),
                UnityEngine.Random.Range(0.5f, 1f)
            );
            
            foreach (Renderer renderer in character.GetComponentsInChildren<Renderer>())
            {
                renderer.material.color = randomColor;
            }
            
            return character;
        }
    }
}