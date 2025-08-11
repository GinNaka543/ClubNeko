using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace ClubNeko.Character
{
    /// <summary>
    /// シンプルなVRMローダー（UniVRMパッケージ無しでも動作）
    /// UniVRMインストール後に完全版に切り替え可能
    /// </summary>
    public class SimpleVRMLoader : MonoBehaviour
    {
        [Header("VRM Settings")]
        public bool debugMode = true;
        public GameObject placeholderPrefab;
        
        /// <summary>
        /// VRMファイルをロード（プレースホルダー版）
        /// </summary>
        public async Task<GameObject> LoadVRM(string path)
        {
            if (debugMode)
            {
                Debug.Log($"[VRM Loader] Attempting to load: {path}");
            }
            
            // UniVRMがインストールされていない場合はプレースホルダーを返す
            #if !UNIVRM_INSTALLED
            
            if (debugMode)
            {
                Debug.LogWarning("[VRM Loader] UniVRM is not installed. Using placeholder.");
                Debug.Log("To enable VRM support:");
                Debug.Log("1. Open Package Manager");
                Debug.Log("2. Add package from git URL:");
                Debug.Log("   https://github.com/vrm-c/UniVRM.git?path=/Assets/VRM10#v0.127.0");
            }
            
            // プレースホルダーキャラクターを作成
            GameObject placeholder = CreatePlaceholderCharacter(Path.GetFileNameWithoutExtension(path));
            
            await Task.Delay(100); // 非同期処理のシミュレーション
            
            return placeholder;
            
            #else
            
            // UniVRMがインストールされている場合の実装
            // ここに実際のVRMロード処理を記述
            return null;
            
            #endif
        }
        
        /// <summary>
        /// プレースホルダーキャラクターを作成
        /// </summary>
        private GameObject CreatePlaceholderCharacter(string name)
        {
            GameObject character = null;
            
            if (placeholderPrefab != null)
            {
                character = Instantiate(placeholderPrefab);
            }
            else
            {
                // シンプルなカプセルキャラクターを作成
                character = CreateCapsuleCharacter();
            }
            
            character.name = $"VRM_Placeholder_{name}";
            
            // 基本的なコンポーネントを追加
            AddBasicComponents(character);
            
            return character;
        }
        
        /// <summary>
        /// カプセルキャラクターを作成
        /// </summary>
        private GameObject CreateCapsuleCharacter()
        {
            GameObject character = new GameObject("PlaceholderCharacter");
            
            // ボディ（カプセル）
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(character.transform);
            body.transform.localPosition = new Vector3(0, 1f, 0);
            body.transform.localScale = new Vector3(0.5f, 1f, 0.5f);
            
            // ヘッド（球）
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(character.transform);
            head.transform.localPosition = new Vector3(0, 2.2f, 0);
            head.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
            
            // 目（小さい球）
            GameObject leftEye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leftEye.name = "LeftEye";
            leftEye.transform.SetParent(head.transform);
            leftEye.transform.localPosition = new Vector3(-0.15f, 0, -0.4f);
            leftEye.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            leftEye.GetComponent<Renderer>().material.color = Color.black;
            
            GameObject rightEye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rightEye.name = "RightEye";
            rightEye.transform.SetParent(head.transform);
            rightEye.transform.localPosition = new Vector3(0.15f, 0, -0.4f);
            rightEye.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            rightEye.GetComponent<Renderer>().material.color = Color.black;
            
            // 腕（カプセル）
            GameObject leftArm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            leftArm.name = "LeftArm";
            leftArm.transform.SetParent(character.transform);
            leftArm.transform.localPosition = new Vector3(-0.6f, 1.5f, 0);
            leftArm.transform.localScale = new Vector3(0.2f, 0.6f, 0.2f);
            leftArm.transform.localRotation = Quaternion.Euler(0, 0, 30);
            
            GameObject rightArm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            rightArm.name = "RightArm";
            rightArm.transform.SetParent(character.transform);
            rightArm.transform.localPosition = new Vector3(0.6f, 1.5f, 0);
            rightArm.transform.localScale = new Vector3(0.2f, 0.6f, 0.2f);
            rightArm.transform.localRotation = Quaternion.Euler(0, 0, -30);
            
            // 脚（カプセル）
            GameObject leftLeg = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            leftLeg.name = "LeftLeg";
            leftLeg.transform.SetParent(character.transform);
            leftLeg.transform.localPosition = new Vector3(-0.2f, 0.5f, 0);
            leftLeg.transform.localScale = new Vector3(0.2f, 0.5f, 0.2f);
            
            GameObject rightLeg = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            rightLeg.name = "RightLeg";
            rightLeg.transform.SetParent(character.transform);
            rightLeg.transform.localPosition = new Vector3(0.2f, 0.5f, 0);
            rightLeg.transform.localScale = new Vector3(0.2f, 0.5f, 0.2f);
            
            // ランダムな色を適用
            Color randomColor = new Color(
                UnityEngine.Random.Range(0.5f, 1f),
                UnityEngine.Random.Range(0.5f, 1f),
                UnityEngine.Random.Range(0.5f, 1f)
            );
            
            foreach (Renderer renderer in character.GetComponentsInChildren<Renderer>())
            {
                if (renderer.name != "LeftEye" && renderer.name != "RightEye")
                {
                    renderer.material.color = randomColor;
                }
            }
            
            return character;
        }
        
        /// <summary>
        /// 基本的なコンポーネントを追加
        /// </summary>
        private void AddBasicComponents(GameObject character)
        {
            // コライダー
            CapsuleCollider capsule = character.AddComponent<CapsuleCollider>();
            capsule.center = new Vector3(0, 1f, 0);
            capsule.radius = 0.3f;
            capsule.height = 2.5f;
            
            // Rigidbody
            Rigidbody rb = character.AddComponent<Rigidbody>();
            rb.freezeRotation = true;
            
            // アニメーター（ダミー）
            Animator animator = character.AddComponent<Animator>();
            
            // VRMプレースホルダータグ
            character.tag = "VRMCharacter";
            
            // 簡単なアニメーション
            PlaceholderAnimation anim = character.AddComponent<PlaceholderAnimation>();
        }
    }
    
    /// <summary>
    /// プレースホルダーキャラクター用の簡単なアニメーション
    /// </summary>
    public class PlaceholderAnimation : MonoBehaviour
    {
        private float bobSpeed = 2f;
        private float bobAmount = 0.1f;
        private float rotateSpeed = 30f;
        private Vector3 originalPosition;
        
        private void Start()
        {
            originalPosition = transform.position;
        }
        
        private void Update()
        {
            // 上下に揺れる
            float newY = originalPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobAmount;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            
            // ゆっくり回転
            transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
        }
    }
}