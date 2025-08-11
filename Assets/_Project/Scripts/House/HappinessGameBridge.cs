using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace ClubNeko.House
{
    /// <summary>
    /// ハピネスゲームとのデータ連携ブリッジ
    /// </summary>
    public class HappinessGameBridge : MonoBehaviour
    {
        [Header("Sync Settings")]
        public bool autoSync = true;
        public float syncInterval = 30f;
        
        private List<HappinessCharacter> cachedCharacters = new List<HappinessCharacter>();
        private List<MediaItem> cachedMediaItems = new List<MediaItem>();
        
        #if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern string GetHappinessCharacterData();
        
        [DllImport("__Internal")]
        private static extern string GetHappinessMediaData();
        
        [DllImport("__Internal")]
        private static extern void OpenHappinessGame();
        
        [DllImport("__Internal")]
        private static extern bool IsHappinessGameInstalled();
        #endif
        
        private void Start()
        {
            if (autoSync)
            {
                StartCoroutine(AutoSyncRoutine());
            }
            
            // 初回同期
            SyncData();
        }
        
        private IEnumerator AutoSyncRoutine()
        {
            while (autoSync)
            {
                yield return new WaitForSeconds(syncInterval);
                SyncData();
            }
        }
        
        public void SyncData()
        {
            #if UNITY_IOS && !UNITY_EDITOR
            if (IsHappinessGameInstalled())
            {
                ImportCharacterData();
                ImportMediaData();
            }
            else
            {
                Debug.LogWarning("Happiness Game is not installed");
                UseTestData();
            }
            #else
            // エディタまたは他のプラットフォームではテストデータを使用
            UseTestData();
            #endif
        }
        
        private void ImportCharacterData()
        {
            #if UNITY_IOS && !UNITY_EDITOR
            string jsonData = GetHappinessCharacterData();
            if (!string.IsNullOrEmpty(jsonData))
            {
                CharacterDataWrapper wrapper = JsonUtility.FromJson<CharacterDataWrapper>(jsonData);
                cachedCharacters = wrapper.characters;
                Debug.Log($"Imported {cachedCharacters.Count} characters from Happiness Game");
            }
            #endif
        }
        
        private void ImportMediaData()
        {
            #if UNITY_IOS && !UNITY_EDITOR
            string jsonData = GetHappinessMediaData();
            if (!string.IsNullOrEmpty(jsonData))
            {
                MediaDataWrapper wrapper = JsonUtility.FromJson<MediaDataWrapper>(jsonData);
                cachedMediaItems = ConvertToMediaItems(wrapper.mediaData);
                Debug.Log($"Imported {cachedMediaItems.Count} media items from Happiness Game");
            }
            #endif
        }
        
        private List<MediaItem> ConvertToMediaItems(List<HappinessMediaData> mediaData)
        {
            List<MediaItem> items = new List<MediaItem>();
            
            foreach (var data in mediaData)
            {
                MediaItem item = new MediaItem
                {
                    mediaId = data.id,
                    mediaType = data.isVideo ? MediaType.Video : MediaType.Photo,
                    sourcePath = data.path,
                    displayStyle = DisplayStyle.Frame,
                    title = data.title,
                    characterId = data.characterId
                };
                items.Add(item);
            }
            
            return items;
        }
        
        private void UseTestData()
        {
            // テスト用のダミーデータを生成
            cachedCharacters.Clear();
            cachedMediaItems.Clear();
            
            // テストキャラクター
            for (int i = 0; i < 5; i++)
            {
                HappinessCharacter character = new HappinessCharacter
                {
                    id = $"test_char_{i}",
                    name = $"テストキャラクター{i + 1}",
                    imageIdentifier = $"test_image_{i}.jpg",
                    tag = "テスト",
                    birthday = System.DateTime.Now.AddYears(-20).AddDays(i * 30).ToString(),
                    customFields = new Dictionary<string, string>
                    {
                        { "description", $"これはテスト用のキャラクター{i + 1}です" },
                        { "favorite", "猫" }
                    }
                };
                cachedCharacters.Add(character);
            }
            
            // テストメディア
            for (int i = 0; i < 10; i++)
            {
                MediaItem item = new MediaItem
                {
                    mediaId = $"test_media_{i}",
                    mediaType = i % 3 == 0 ? MediaType.Video : MediaType.Photo,
                    sourcePath = $"test_media_{i}.jpg",
                    displayStyle = (DisplayStyle)(i % 3),
                    title = $"テストメディア{i + 1}",
                    characterId = $"test_char_{i % 5}"
                };
                cachedMediaItems.Add(item);
            }
            
            Debug.Log("Using test data - Characters: 5, Media: 10");
        }
        
        public List<HappinessCharacter> GetCharacterData()
        {
            return new List<HappinessCharacter>(cachedCharacters);
        }
        
        public List<MediaItem> GetMediaCollection()
        {
            return new List<MediaItem>(cachedMediaItems);
        }
        
        public HappinessCharacter GetCharacterById(string characterId)
        {
            return cachedCharacters.Find(c => c.id == characterId);
        }
        
        public List<MediaItem> GetMediaByCharacter(string characterId)
        {
            return cachedMediaItems.FindAll(m => m.characterId == characterId);
        }
        
        public void OpenHappinessGameApp()
        {
            #if UNITY_IOS && !UNITY_EDITOR
            OpenHappinessGame();
            #else
            Debug.Log("Opening Happiness Game (not available in editor)");
            Application.OpenURL("https://apps.apple.com/app/happiness-game"); // 実際のApp Store URL
            #endif
        }
        
        public bool CheckHappinessGameInstalled()
        {
            #if UNITY_IOS && !UNITY_EDITOR
            return IsHappinessGameInstalled();
            #else
            return false;
            #endif
        }
    }
    
    [System.Serializable]
    public class HappinessCharacter
    {
        public string id;
        public string name;
        public string imageIdentifier;
        public string tag;
        public string birthday;
        public Dictionary<string, string> customFields;
    }
    
    [System.Serializable]
    public class HappinessMediaData
    {
        public string id;
        public string path;
        public string title;
        public string characterId;
        public bool isVideo;
    }
    
    [System.Serializable]
    public class CharacterDataWrapper
    {
        public List<HappinessCharacter> characters;
    }
    
    [System.Serializable]
    public class MediaDataWrapper
    {
        public List<HappinessMediaData> mediaData;
    }
}