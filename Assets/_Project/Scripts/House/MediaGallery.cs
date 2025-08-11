using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace ClubNeko.House
{
    /// <summary>
    /// メディアギャラリーシステム
    /// 写真と動画を3D空間に表示
    /// </summary>
    public class MediaGallery : MonoBehaviour
    {
        [Header("Display Settings")]
        public GameObject photoFramePrefab;
        public GameObject videoScreenPrefab;
        public GameObject hologramPrefab;
        
        [Header("Materials")]
        public Material photoMaterial;
        public Material videoMaterial;
        public Material hologramMaterial;
        
        private List<MediaDisplay> activeDisplays = new List<MediaDisplay>();
        
        /// <summary>
        /// メディアディスプレイを作成
        /// </summary>
        public MediaDisplay CreateMediaDisplay(MediaItem item, Vector3 position, Transform parent = null)
        {
            GameObject displayObject = null;
            
            // ディスプレイスタイルに応じてプレハブを選択
            switch (item.displayStyle)
            {
                case DisplayStyle.Frame:
                    displayObject = Instantiate(photoFramePrefab ?? CreateDefaultFrame(), position, Quaternion.identity, parent);
                    break;
                case DisplayStyle.Projection:
                    displayObject = Instantiate(videoScreenPrefab ?? CreateDefaultScreen(), position, Quaternion.identity, parent);
                    break;
                case DisplayStyle.Hologram:
                    displayObject = Instantiate(hologramPrefab ?? CreateDefaultHologram(), position, Quaternion.identity, parent);
                    break;
            }
            
            if (displayObject == null) return null;
            
            // MediaDisplayコンポーネントを追加
            MediaDisplay display = displayObject.AddComponent<MediaDisplay>();
            display.Initialize(item, this);
            
            activeDisplays.Add(display);
            
            return display;
        }
        
        /// <summary>
        /// デフォルトのフォトフレームを作成
        /// </summary>
        private GameObject CreateDefaultFrame()
        {
            GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frame.name = "PhotoFrame";
            frame.transform.localScale = new Vector3(2f, 2f, 0.1f);
            
            // フレームの装飾
            GameObject border = GameObject.CreatePrimitive(PrimitiveType.Cube);
            border.name = "FrameBorder";
            border.transform.SetParent(frame.transform);
            border.transform.localPosition = Vector3.zero;
            border.transform.localScale = new Vector3(1.1f, 1.1f, 0.9f);
            
            Renderer borderRenderer = border.GetComponent<Renderer>();
            borderRenderer.material.color = new Color(0.5f, 0.3f, 0.1f); // 木目調
            
            return frame;
        }
        
        /// <summary>
        /// デフォルトのビデオスクリーンを作成
        /// </summary>
        private GameObject CreateDefaultScreen()
        {
            GameObject screen = GameObject.CreatePrimitive(PrimitiveType.Quad);
            screen.name = "VideoScreen";
            screen.transform.localScale = new Vector3(4f, 2.25f, 1f); // 16:9 aspect ratio
            
            // スクリーンフレーム
            GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frame.name = "ScreenFrame";
            frame.transform.SetParent(screen.transform);
            frame.transform.localPosition = new Vector3(0, 0, 0.05f);
            frame.transform.localScale = new Vector3(1.05f, 1.05f, 0.1f);
            
            Renderer frameRenderer = frame.GetComponent<Renderer>();
            frameRenderer.material.color = Color.black;
            
            return screen;
        }
        
        /// <summary>
        /// デフォルトのホログラムディスプレイを作成
        /// </summary>
        private GameObject CreateDefaultHologram()
        {
            GameObject hologram = new GameObject("HologramDisplay");
            
            // ベースプレート
            GameObject basePlate = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            basePlate.name = "HologramBase";
            basePlate.transform.SetParent(hologram.transform);
            basePlate.transform.localScale = new Vector3(1f, 0.1f, 1f);
            basePlate.GetComponent<Renderer>().material.color = new Color(0.2f, 0.3f, 0.4f);
            
            // ホログラム投影エリア
            GameObject projection = GameObject.CreatePrimitive(PrimitiveType.Quad);
            projection.name = "HologramProjection";
            projection.transform.SetParent(hologram.transform);
            projection.transform.localPosition = new Vector3(0, 1f, 0);
            projection.transform.localScale = new Vector3(2f, 2f, 1f);
            
            // ホログラムエフェクト
            if (hologramMaterial != null)
            {
                projection.GetComponent<Renderer>().material = hologramMaterial;
            }
            else
            {
                Material mat = projection.GetComponent<Renderer>().material;
                mat.color = new Color(0.3f, 0.8f, 1f, 0.7f);
                mat.SetFloat("_Mode", 3); // Transparent
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.renderQueue = 3000;
            }
            
            return hologram;
        }
        
        /// <summary>
        /// メディアアイテムをロード
        /// </summary>
        public IEnumerator LoadMediaContent(MediaDisplay display, string path)
        {
            if (display.mediaItem.mediaType == MediaType.Photo)
            {
                yield return LoadPhoto(display, path);
            }
            else if (display.mediaItem.mediaType == MediaType.Video)
            {
                yield return LoadVideo(display, path);
            }
        }
        
        private IEnumerator LoadPhoto(MediaDisplay display, string path)
        {
            // ローカルファイルまたはURLから画像をロード
            Texture2D texture = null;
            
            if (path.StartsWith("http"))
            {
                using (var www = UnityEngine.Networking.UnityWebRequestTexture.GetTexture(path))
                {
                    yield return www.SendWebRequest();
                    
                    if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                    {
                        texture = UnityEngine.Networking.DownloadHandlerTexture.GetContent(www);
                    }
                }
            }
            else
            {
                // ローカルファイルの場合
                byte[] fileData = System.IO.File.ReadAllBytes(path);
                texture = new Texture2D(2, 2);
                texture.LoadImage(fileData);
            }
            
            if (texture != null)
            {
                display.SetTexture(texture);
            }
        }
        
        private IEnumerator LoadVideo(MediaDisplay display, string path)
        {
            VideoPlayer videoPlayer = display.gameObject.AddComponent<VideoPlayer>();
            videoPlayer.url = path;
            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = true;
            videoPlayer.renderMode = VideoRenderMode.MaterialOverride;
            
            Renderer renderer = display.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                videoPlayer.targetMaterialRenderer = renderer;
                videoPlayer.targetMaterialProperty = "_MainTex";
            }
            
            videoPlayer.Prepare();
            
            while (!videoPlayer.isPrepared)
            {
                yield return null;
            }
            
            videoPlayer.Play();
        }
        
        /// <summary>
        /// 全てのディスプレイをクリア
        /// </summary>
        public void ClearAllDisplays()
        {
            foreach (var display in activeDisplays)
            {
                if (display != null)
                {
                    Destroy(display.gameObject);
                }
            }
            activeDisplays.Clear();
        }
    }
    
    /// <summary>
    /// メディアディスプレイコンポーネント
    /// </summary>
    public class MediaDisplay : MonoBehaviour
    {
        public MediaItem mediaItem { get; private set; }
        private MediaGallery gallery;
        private Renderer displayRenderer;
        private Material originalMaterial;
        
        public void Initialize(MediaItem item, MediaGallery parentGallery)
        {
            mediaItem = item;
            gallery = parentGallery;
            displayRenderer = GetComponentInChildren<Renderer>();
            
            if (displayRenderer != null)
            {
                originalMaterial = displayRenderer.material;
            }
            
            // コンテンツをロード
            if (!string.IsNullOrEmpty(item.sourcePath))
            {
                StartCoroutine(gallery.LoadMediaContent(this, item.sourcePath));
            }
            
            // インタラクション追加
            AddInteraction();
        }
        
        public void SetTexture(Texture2D texture)
        {
            if (displayRenderer != null)
            {
                displayRenderer.material.mainTexture = texture;
            }
        }
        
        private void AddInteraction()
        {
            // コライダー追加（まだない場合）
            if (GetComponent<Collider>() == null)
            {
                gameObject.AddComponent<BoxCollider>();
            }
            
            // インタラクションスクリプト追加
            MediaInteraction interaction = gameObject.AddComponent<MediaInteraction>();
            interaction.mediaDisplay = this;
        }
        
        private void OnDestroy()
        {
            if (originalMaterial != null)
            {
                Destroy(originalMaterial);
            }
        }
    }
    
    /// <summary>
    /// メディアインタラクション
    /// </summary>
    public class MediaInteraction : MonoBehaviour
    {
        public MediaDisplay mediaDisplay;
        private Vector3 originalScale;
        
        private void Start()
        {
            originalScale = transform.localScale;
        }
        
        private void OnMouseEnter()
        {
            // ホバー時に少し大きくする
            transform.localScale = originalScale * 1.1f;
        }
        
        private void OnMouseExit()
        {
            transform.localScale = originalScale;
        }
        
        private void OnMouseDown()
        {
            // クリック時の処理
            Debug.Log($"Clicked on media: {mediaDisplay.mediaItem.title}");
            
            // 詳細表示やフルスクリーン表示など
        }
    }
    
    /// <summary>
    /// メディアアイテムデータ
    /// </summary>
    [System.Serializable]
    public class MediaItem
    {
        public string mediaId;
        public MediaType mediaType;
        public string sourcePath;
        public DisplayStyle displayStyle;
        public string title;
        public string characterId;
        
        public MediaItem()
        {
            mediaId = System.Guid.NewGuid().ToString();
            mediaType = MediaType.Photo;
            displayStyle = DisplayStyle.Frame;
        }
    }
    
    public enum MediaType
    {
        Photo,
        Video
    }
    
    public enum DisplayStyle
    {
        Frame,      // フレーム表示
        Projection, // 投影表示
        Hologram    // ホログラム表示
    }
}