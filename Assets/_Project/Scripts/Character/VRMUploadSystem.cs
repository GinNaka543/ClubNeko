using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
#if UNITY_IOS
using UnityEngine.iOS;
#endif

namespace ClubNeko.Character
{
    /// <summary>
    /// ユーザーがVRMファイルをアップロード・管理するシステム
    /// iOSではFiles app、PCではファイルダイアログから選択
    /// </summary>
    public class VRMUploadSystem : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject uploadPanel;
        public Button uploadButton;
        public Button selectFromGalleryButton;
        public Transform characterListContent;
        public GameObject characterCardPrefab;
        
        [Header("Upload Settings")]
        public long maxFileSize = 50 * 1024 * 1024; // 50MB
        public string[] allowedExtensions = { ".vrm" };
        
        [Header("Storage")]
        public string vrmStoragePath = "UserVRM";
        private string fullStoragePath;
        
        [Header("Preview")]
        public Transform previewArea;
        public Camera previewCamera;
        public RenderTexture previewRenderTexture;
        
        private SimpleVRMLoader vrmLoader;
        private List<UploadedVRMData> uploadedVRMs = new List<UploadedVRMData>();
        private GameObject currentPreviewModel;
        
        [System.Serializable]
        public class UploadedVRMData
        {
            public string id;
            public string fileName;
            public string localPath;
            public DateTime uploadDate;
            public Texture2D thumbnail;
            public VRMMetaData metadata;
        }
        
        [System.Serializable]
        public class VRMMetaData
        {
            public string title;
            public string author;
            public string version;
            public bool allowedForCommercialUse;
            public bool allowedForPoliticalUse;
            public bool allowedForSexualUse;
            public bool allowedForViolentUse;
        }
        
        private void Start()
        {
            InitializeSystem();
            SetupUI();
            LoadSavedVRMs();
        }
        
        private void InitializeSystem()
        {
            // VRMローダーの初期化
            vrmLoader = GetComponent<SimpleVRMLoader>();
            if (vrmLoader == null)
            {
                vrmLoader = gameObject.AddComponent<SimpleVRMLoader>();
            }
            
            // ストレージパスの設定
            fullStoragePath = Path.Combine(Application.persistentDataPath, vrmStoragePath);
            if (!Directory.Exists(fullStoragePath))
            {
                Directory.CreateDirectory(fullStoragePath);
            }
            
            Debug.Log($"VRM Storage Path: {fullStoragePath}");
        }
        
        private void SetupUI()
        {
            if (uploadButton != null)
            {
                uploadButton.onClick.AddListener(OnUploadButtonClicked);
            }
            
            if (selectFromGalleryButton != null)
            {
                selectFromGalleryButton.onClick.AddListener(OnSelectFromGalleryClicked);
            }
        }
        
        /// <summary>
        /// アップロードボタンクリック時
        /// </summary>
        public void OnUploadButtonClicked()
        {
            #if UNITY_IOS && !UNITY_EDITOR
            OpenIOSFilePicker();
            #elif UNITY_STANDALONE || UNITY_EDITOR
            OpenDesktopFilePicker();
            #elif UNITY_ANDROID
            OpenAndroidFilePicker();
            #else
            ShowMessage("この プラットフォームではファイル選択がサポートされていません");
            #endif
        }
        
        /// <summary>
        /// ギャラリーから選択（保存済みVRM一覧）
        /// </summary>
        public void OnSelectFromGalleryClicked()
        {
            ShowVRMGallery();
        }
        
        #region Platform Specific File Pickers
        
        /// <summary>
        /// iOS用ファイルピッカー
        /// </summary>
        private void OpenIOSFilePicker()
        {
            #if UNITY_IOS && !UNITY_EDITOR
            // iOS用のネイティブファイルピッカーを実装
            // NativeFilePicker プラグインを使用するか、
            // カスタムネイティブプラグインを実装
            
            NativeFilePicker.PickFile((path) =>
            {
                if (!string.IsNullOrEmpty(path))
                {
                    StartCoroutine(ProcessSelectedFile(path));
                }
            }, new string[] { "public.data", "public.item" });
            #endif
        }
        
        /// <summary>
        /// デスクトップ用ファイルピッカー
        /// </summary>
        private void OpenDesktopFilePicker()
        {
            #if UNITY_STANDALONE || UNITY_EDITOR
            // SimpleFileBrowser を使用（Unity Asset Storeから入手可能）
            // または System.Windows.Forms を使用（Windowsのみ）
            
            string[] paths = UnityEditor.EditorUtility.OpenFilePanel(
                "VRMファイルを選択",
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop),
                "vrm"
            ).Split(',');
            
            if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
            {
                StartCoroutine(ProcessSelectedFile(paths[0]));
            }
            #endif
        }
        
        /// <summary>
        /// Android用ファイルピッカー
        /// </summary>
        private void OpenAndroidFilePicker()
        {
            #if UNITY_ANDROID && !UNITY_EDITOR
            // Android用のファイルピッカー実装
            // NativeFilePicker や UnityNativeFilePicker を使用
            #endif
        }
        
        #endregion
        
        /// <summary>
        /// 選択されたファイルを処理
        /// </summary>
        private IEnumerator ProcessSelectedFile(string filePath)
        {
            ShowLoadingUI(true);
            
            // ファイル検証
            if (!ValidateFile(filePath))
            {
                ShowMessage("無効なファイルです。VRMファイルを選択してください。");
                ShowLoadingUI(false);
                yield break;
            }
            
            // ファイルをアプリのストレージにコピー
            string fileName = Path.GetFileName(filePath);
            string newPath = Path.Combine(fullStoragePath, Guid.NewGuid().ToString() + "_" + fileName);
            
            try
            {
                File.Copy(filePath, newPath, true);
            }
            catch (Exception e)
            {
                ShowMessage($"ファイルのコピーに失敗しました: {e.Message}");
                ShowLoadingUI(false);
                yield break;
            }
            
            // VRMをロード
            var loadTask = vrmLoader.LoadVRM(newPath);
            yield return new WaitUntil(() => loadTask.IsCompleted);
            
            GameObject vrmObject = loadTask.Result;
            
            if (vrmObject == null)
            {
                ShowMessage("VRMのロードに失敗しました");
                File.Delete(newPath);
                ShowLoadingUI(false);
                yield break;
            }
            
            // メタデータを抽出
            VRMMetaData metadata = ExtractVRMMetadata(vrmObject);
            
            // サムネイルを生成
            Texture2D thumbnail = GenerateThumbnail(vrmObject);
            
            // データを保存
            UploadedVRMData vrmData = new UploadedVRMData
            {
                id = Guid.NewGuid().ToString(),
                fileName = fileName,
                localPath = newPath,
                uploadDate = DateTime.Now,
                thumbnail = thumbnail,
                metadata = metadata
            };
            
            uploadedVRMs.Add(vrmData);
            SaveVRMList();
            
            // UIを更新
            AddVRMCard(vrmData);
            
            // プレビューエリアに表示
            ShowPreview(vrmObject);
            
            ShowLoadingUI(false);
            ShowMessage($"VRM「{metadata.title ?? fileName}」をアップロードしました！");
        }
        
        /// <summary>
        /// ファイルの検証
        /// </summary>
        private bool ValidateFile(string filePath)
        {
            if (!File.Exists(filePath))
                return false;
            
            FileInfo fileInfo = new FileInfo(filePath);
            
            // サイズチェック
            if (fileInfo.Length > maxFileSize)
            {
                ShowMessage($"ファイルサイズが大きすぎます（最大: {maxFileSize / 1024 / 1024}MB）");
                return false;
            }
            
            // 拡張子チェック
            string extension = Path.GetExtension(filePath).ToLower();
            bool validExtension = false;
            foreach (string allowed in allowedExtensions)
            {
                if (extension == allowed.ToLower())
                {
                    validExtension = true;
                    break;
                }
            }
            
            return validExtension;
        }
        
        /// <summary>
        /// VRMメタデータの抽出
        /// </summary>
        private VRMMetaData ExtractVRMMetadata(GameObject vrmObject)
        {
            VRMMetaData metadata = new VRMMetaData();
            
            // UniVRMがインストールされていない場合はダミーデータを返す
            #if UNIVRM_INSTALLED
            // VRM10の場合
            var vrm10Instance = vrmObject.GetComponent<UniVRM10.Vrm10Instance>();
            if (vrm10Instance != null && vrm10Instance.Vrm != null)
            {
                var meta = vrm10Instance.Vrm.Meta;
                if (meta != null)
                {
                    metadata.title = meta.Name;
                    metadata.author = meta.Authors?[0] ?? "Unknown";
                    metadata.version = meta.Version;
                    
                    // 利用条件
                    var usage = meta.AvatarPermission;
                    if (usage != null)
                    {
                        metadata.allowedForCommercialUse = usage.AvatarUsage == UniVRM10.AvatarUsageType.Allow;
                    }
                }
            }
            #else
            // プレースホルダーメタデータ
            metadata.title = vrmObject.name;
            metadata.author = "Unknown";
            metadata.version = "1.0.0";
            metadata.allowedForCommercialUse = true;
            #endif
            
            return metadata;
        }
        
        /// <summary>
        /// サムネイル生成
        /// </summary>
        private Texture2D GenerateThumbnail(GameObject vrmObject)
        {
            // プレビューカメラでレンダリング
            GameObject tempObject = Instantiate(vrmObject, previewArea);
            tempObject.transform.localPosition = Vector3.zero;
            tempObject.transform.localRotation = Quaternion.identity;
            
            // カメラを調整
            Bounds bounds = GetModelBounds(tempObject);
            previewCamera.transform.position = bounds.center + Vector3.forward * bounds.size.magnitude * 1.5f;
            previewCamera.transform.LookAt(bounds.center);
            
            // レンダーテクスチャに描画
            previewCamera.targetTexture = previewRenderTexture;
            previewCamera.Render();
            
            // Texture2Dに変換
            RenderTexture.active = previewRenderTexture;
            Texture2D thumbnail = new Texture2D(256, 256, TextureFormat.RGB24, false);
            thumbnail.ReadPixels(new Rect(0, 0, 256, 256), 0, 0);
            thumbnail.Apply();
            RenderTexture.active = null;
            
            Destroy(tempObject);
            
            return thumbnail;
        }
        
        /// <summary>
        /// モデルの境界を取得
        /// </summary>
        private Bounds GetModelBounds(GameObject model)
        {
            Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
                return new Bounds(model.transform.position, Vector3.one);
            
            Bounds bounds = renderers[0].bounds;
            foreach (Renderer renderer in renderers)
            {
                bounds.Encapsulate(renderer.bounds);
            }
            
            return bounds;
        }
        
        /// <summary>
        /// VRMギャラリーを表示
        /// </summary>
        private void ShowVRMGallery()
        {
            uploadPanel.SetActive(true);
            
            // 既存のカードをクリア
            foreach (Transform child in characterListContent)
            {
                Destroy(child.gameObject);
            }
            
            // VRMカードを追加
            foreach (var vrmData in uploadedVRMs)
            {
                AddVRMCard(vrmData);
            }
        }
        
        /// <summary>
        /// VRMカードをUIに追加
        /// </summary>
        private void AddVRMCard(UploadedVRMData vrmData)
        {
            GameObject card = Instantiate(characterCardPrefab, characterListContent);
            
            // カードの設定
            VRMCard cardComponent = card.GetComponent<VRMCard>();
            if (cardComponent != null)
            {
                cardComponent.SetData(vrmData);
                cardComponent.OnSelectAction = () => LoadVRMToGame(vrmData);
                cardComponent.OnDeleteAction = () => DeleteVRM(vrmData);
            }
        }
        
        /// <summary>
        /// VRMをゲームにロード
        /// </summary>
        public void LoadVRMToGame(UploadedVRMData vrmData)
        {
            StartCoroutine(LoadVRMCoroutine(vrmData));
        }
        
        private IEnumerator LoadVRMCoroutine(UploadedVRMData vrmData)
        {
            var loadTask = vrmLoader.LoadVRM(vrmData.localPath);
            yield return new WaitUntil(() => loadTask.IsCompleted);
            
            GameObject vrmObject = loadTask.Result;
            if (vrmObject != null)
            {
                // ゲーム内に配置
                vrmObject.transform.position = GetSpawnPosition();
                ShowMessage($"「{vrmData.metadata?.title ?? vrmData.fileName}」を召喚しました！");
                
                // イベント通知
                OnVRMLoaded?.Invoke(vrmObject, vrmData);
            }
        }
        
        /// <summary>
        /// VRMを削除
        /// </summary>
        public void DeleteVRM(UploadedVRMData vrmData)
        {
            if (File.Exists(vrmData.localPath))
            {
                File.Delete(vrmData.localPath);
            }
            
            uploadedVRMs.Remove(vrmData);
            SaveVRMList();
            ShowVRMGallery();
        }
        
        /// <summary>
        /// 保存済みVRMリストをロード
        /// </summary>
        private void LoadSavedVRMs()
        {
            string saveFile = Path.Combine(fullStoragePath, "vrm_list.json");
            if (File.Exists(saveFile))
            {
                string json = File.ReadAllText(saveFile);
                VRMListData listData = JsonUtility.FromJson<VRMListData>(json);
                
                if (listData != null && listData.vrmList != null)
                {
                    foreach (var data in listData.vrmList)
                    {
                        if (File.Exists(data.localPath))
                        {
                            uploadedVRMs.Add(data);
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// VRMリストを保存
        /// </summary>
        private void SaveVRMList()
        {
            VRMListData listData = new VRMListData
            {
                vrmList = uploadedVRMs
            };
            
            string json = JsonUtility.ToJson(listData, true);
            string saveFile = Path.Combine(fullStoragePath, "vrm_list.json");
            File.WriteAllText(saveFile, json);
        }
        
        private void ShowPreview(GameObject vrmObject)
        {
            if (currentPreviewModel != null)
            {
                Destroy(currentPreviewModel);
            }
            
            currentPreviewModel = Instantiate(vrmObject, previewArea);
            currentPreviewModel.transform.localPosition = Vector3.zero;
            currentPreviewModel.transform.localRotation = Quaternion.identity;
        }
        
        private Vector3 GetSpawnPosition()
        {
            // ゲーム内のスポーン位置を返す
            return new Vector3(0, 0, 0);
        }
        
        private void ShowLoadingUI(bool show)
        {
            // ローディングUIの表示/非表示
        }
        
        private void ShowMessage(string message)
        {
            Debug.Log($"[VRM Upload] {message}");
            // UIにメッセージを表示
        }
        
        /// <summary>
        /// VRMロード完了イベント
        /// </summary>
        public event Action<GameObject, UploadedVRMData> OnVRMLoaded;
        
        [System.Serializable]
        private class VRMListData
        {
            public List<UploadedVRMData> vrmList;
        }
    }
    
    /// <summary>
    /// VRMカードUIコンポーネント
    /// </summary>
    public class VRMCard : MonoBehaviour
    {
        public Image thumbnailImage;
        public Text titleText;
        public Text authorText;
        public Text dateText;
        public Button selectButton;
        public Button deleteButton;
        
        public Action OnSelectAction;
        public Action OnDeleteAction;
        
        public void SetData(VRMUploadSystem.UploadedVRMData data)
        {
            if (thumbnailImage != null && data.thumbnail != null)
            {
                thumbnailImage.sprite = Sprite.Create(
                    data.thumbnail,
                    new Rect(0, 0, data.thumbnail.width, data.thumbnail.height),
                    new Vector2(0.5f, 0.5f)
                );
            }
            
            if (titleText != null)
            {
                titleText.text = data.metadata?.title ?? data.fileName;
            }
            
            if (authorText != null)
            {
                authorText.text = $"作者: {data.metadata?.author ?? "不明"}";
            }
            
            if (dateText != null)
            {
                dateText.text = data.uploadDate.ToString("yyyy/MM/dd");
            }
            
            if (selectButton != null)
            {
                selectButton.onClick.RemoveAllListeners();
                selectButton.onClick.AddListener(() => OnSelectAction?.Invoke());
            }
            
            if (deleteButton != null)
            {
                deleteButton.onClick.RemoveAllListeners();
                deleteButton.onClick.AddListener(() => OnDeleteAction?.Invoke());
            }
        }
    }
}