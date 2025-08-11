using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using ClubNeko.Character;

namespace ClubNeko.House
{
    /// <summary>
    /// ハウス機能の管理 - 写真・動画のコレクション空間
    /// </summary>
    public class HouseManager : MonoBehaviour
    {
        [Header("House Layout")]
        public Transform houseRoot;
        public GameObject[] roomPrefabs;
        private Dictionary<string, Room> rooms = new Dictionary<string, Room>();
        
        [Header("Media Gallery")]
        public MediaGallery mediaGallery;
        public Transform mediaDisplayRoot;
        
        [Header("VRM Characters")]
        public Transform characterSpawnPoint;
        public List<GameObject> spawnedCharacters = new List<GameObject>();
        
        [Header("Decoration")]
        public GameObject[] furniturePrefabs;
        public GameObject[] decorationPrefabs;
        
        [Header("Cat Theme")]
        public GameObject[] catDecorations;
        public ParticleSystem catPawEffect;
        
        private HappinessGameBridge happinessBridge;
        private SimpleVRMLoader vrmLoader;
        
        private void Start()
        {
            InitializeHouse();
            LoadHouseData();
            ConnectToHappinessGame();
        }
        
        private void InitializeHouse()
        {
            // VRMローダーの初期化
            vrmLoader = gameObject.AddComponent<SimpleVRMLoader>();
            
            // メディアギャラリーの初期化
            if (mediaGallery == null)
            {
                mediaGallery = gameObject.AddComponent<MediaGallery>();
            }
            
            // 基本レイアウトの生成
            LoadHouseLayout("default_house");
        }
        
        public void LoadHouseLayout(string layoutId)
        {
            // 既存の部屋をクリア
            foreach (var room in rooms.Values)
            {
                if (room.roomObject != null)
                    Destroy(room.roomObject);
            }
            rooms.Clear();
            
            // 新しいレイアウトをロード
            switch (layoutId)
            {
                case "default_house":
                    CreateDefaultHouse();
                    break;
                case "luxury_house":
                    CreateLuxuryHouse();
                    break;
                case "cat_castle":
                    CreateCatCastle();
                    break;
            }
        }
        
        private void CreateDefaultHouse()
        {
            // リビングルーム
            CreateRoom("living_room", RoomType.LivingRoom, new Vector3(0, 0, 0));
            
            // ベッドルーム
            CreateRoom("bedroom", RoomType.Bedroom, new Vector3(10, 0, 0));
            
            // ゲームルーム
            CreateRoom("game_room", RoomType.GameRoom, new Vector3(-10, 0, 0));
            
            // バルコニー
            CreateRoom("balcony", RoomType.Balcony, new Vector3(0, 0, 10));
        }
        
        private void CreateLuxuryHouse()
        {
            // より大きく豪華なレイアウト
            CreateRoom("grand_hall", RoomType.LivingRoom, new Vector3(0, 0, 0), 2f);
            CreateRoom("master_bedroom", RoomType.Bedroom, new Vector3(15, 0, 0), 1.5f);
            CreateRoom("theater_room", RoomType.GameRoom, new Vector3(-15, 0, 0), 1.5f);
            CreateRoom("rooftop_garden", RoomType.Balcony, new Vector3(0, 5, 15), 2f);
        }
        
        private void CreateCatCastle()
        {
            // 猫テーマの特別レイアウト
            CreateRoom("cat_lounge", RoomType.LivingRoom, new Vector3(0, 0, 0));
            CreateRoom("nap_room", RoomType.Bedroom, new Vector3(8, 0, 8));
            CreateRoom("play_room", RoomType.GameRoom, new Vector3(-8, 0, 8));
            CreateRoom("cat_tower", RoomType.Balcony, new Vector3(0, 10, 0));
            
            // 猫デコレーションを追加
            foreach (var catDeco in catDecorations)
            {
                if (catDeco != null)
                {
                    Vector3 randomPos = new Vector3(
                        Random.Range(-5f, 5f),
                        0,
                        Random.Range(-5f, 5f)
                    );
                    Instantiate(catDeco, randomPos, Quaternion.identity, houseRoot);
                }
            }
        }
        
        private Room CreateRoom(string roomId, RoomType type, Vector3 position, float scale = 1f)
        {
            GameObject roomPrefab = GetRoomPrefab(type);
            if (roomPrefab == null) return null;
            
            GameObject roomObj = Instantiate(roomPrefab, position, Quaternion.identity, houseRoot);
            roomObj.transform.localScale = Vector3.one * scale;
            
            Room room = new Room
            {
                roomId = roomId,
                roomType = type,
                roomObject = roomObj,
                decorations = new List<GameObject>(),
                mediaItems = new List<MediaDisplay>()
            };
            
            rooms[roomId] = room;
            return room;
        }
        
        private GameObject GetRoomPrefab(RoomType type)
        {
            // 実際にはタイプに応じたプレハブを返す
            if (roomPrefabs != null && roomPrefabs.Length > (int)type)
            {
                return roomPrefabs[(int)type];
            }
            
            // デフォルトの部屋を生成
            GameObject defaultRoom = GameObject.CreatePrimitive(PrimitiveType.Cube);
            defaultRoom.transform.localScale = new Vector3(10, 3, 10);
            return defaultRoom;
        }
        
        public void PlaceMediaItem(MediaItem item, string roomId, Vector3 position)
        {
            if (!rooms.ContainsKey(roomId)) return;
            
            Room room = rooms[roomId];
            MediaDisplay display = mediaGallery.CreateMediaDisplay(item, position, room.roomObject.transform);
            room.mediaItems.Add(display);
            
            // 猫の足跡エフェクト
            if (catPawEffect != null)
            {
                Instantiate(catPawEffect, position, Quaternion.identity);
            }
        }
        
        public async void SpawnVRMCharacter(string vrmPath)
        {
            if (vrmLoader == null) return;
            
            GameObject character = await vrmLoader.LoadVRM(vrmPath);
            if (character != null)
            {
                character.transform.position = characterSpawnPoint != null ? 
                    characterSpawnPoint.position : Vector3.zero;
                
                spawnedCharacters.Add(character);
                
                // キャラクターにインタラクション追加
                AddCharacterInteraction(character);
            }
        }
        
        private void AddCharacterInteraction(GameObject character)
        {
            // キャラクターに簡単なAI動作を追加
            CharacterAI ai = character.AddComponent<CharacterAI>();
            ai.wanderRadius = 5f;
            ai.idleAnimations = new string[] { "Idle", "Wave", "Dance" };
        }
        
        private void ConnectToHappinessGame()
        {
            happinessBridge = gameObject.AddComponent<HappinessGameBridge>();
            StartCoroutine(SyncWithHappinessGame());
        }
        
        private IEnumerator SyncWithHappinessGame()
        {
            yield return new WaitForSeconds(2f);
            
            // ハピネスゲームからデータ取得
            var characters = happinessBridge.GetCharacterData();
            var mediaItems = happinessBridge.GetMediaCollection();
            
            // キャラクターを表示
            foreach (var character in characters)
            {
                // キャラクター画像をフレームとして配置
                MediaItem item = new MediaItem
                {
                    mediaId = character.id,
                    mediaType = MediaType.Photo,
                    sourcePath = character.imageIdentifier,
                    displayStyle = DisplayStyle.Frame
                };
                
                PlaceMediaItem(item, "living_room", GetRandomWallPosition());
            }
            
            Debug.Log($"Synced {characters.Count} characters from Happiness Game");
        }
        
        private Vector3 GetRandomWallPosition()
        {
            // 壁のランダムな位置を返す
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float radius = 4f;
            float height = Random.Range(1f, 2.5f);
            
            return new Vector3(
                Mathf.Cos(angle) * radius,
                height,
                Mathf.Sin(angle) * radius
            );
        }
        
        private void LoadHouseData()
        {
            // セーブデータからハウス情報をロード
            string houseDataJson = PlayerPrefs.GetString("HouseData", "");
            if (!string.IsNullOrEmpty(houseDataJson))
            {
                HouseData data = JsonUtility.FromJson<HouseData>(houseDataJson);
                ApplyHouseData(data);
            }
        }
        
        private void ApplyHouseData(HouseData data)
        {
            // ハウスデータを適用
            LoadHouseLayout(data.layoutId);
            
            foreach (var mediaItem in data.mediaItems)
            {
                // メディアアイテムを配置
                MediaItem item = new MediaItem
                {
                    mediaId = mediaItem.mediaId,
                    mediaType = (MediaType)System.Enum.Parse(typeof(MediaType), mediaItem.mediaType),
                    sourcePath = mediaItem.sourcePath,
                    displayStyle = (DisplayStyle)System.Enum.Parse(typeof(DisplayStyle), mediaItem.displayStyle)
                };
                
                PlaceMediaItem(item, "living_room", mediaItem.position);
            }
        }
        
        public void SaveHouseData()
        {
            HouseData data = new HouseData();
            data.layoutId = "default_house";
            
            // 現在の配置を保存
            foreach (var room in rooms.Values)
            {
                foreach (var mediaDisplay in room.mediaItems)
                {
                    data.mediaItems.Add(new MediaItemData
                    {
                        mediaId = mediaDisplay.mediaItem.mediaId,
                        mediaType = mediaDisplay.mediaItem.mediaType.ToString(),
                        sourcePath = mediaDisplay.mediaItem.sourcePath,
                        displayStyle = mediaDisplay.mediaItem.displayStyle.ToString(),
                        position = mediaDisplay.transform.position,
                        rotation = mediaDisplay.transform.rotation
                    });
                }
            }
            
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString("HouseData", json);
            PlayerPrefs.Save();
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SaveHouseData();
            }
        }
    }
    
    [System.Serializable]
    public class Room
    {
        public string roomId;
        public RoomType roomType;
        public GameObject roomObject;
        public List<GameObject> decorations;
        public List<MediaDisplay> mediaItems;
    }
    
    public enum RoomType
    {
        LivingRoom,
        Bedroom,
        GameRoom,
        Balcony
    }
    
    [System.Serializable]
    public class HouseData
    {
        public string layoutId = "default_house";
        public List<RoomData> rooms = new List<RoomData>();
        public List<MediaItemData> mediaItems = new List<MediaItemData>();
    }
    
    [System.Serializable]
    public class RoomData
    {
        public string roomId;
        public string roomType;
        public List<DecorationData> decorations = new List<DecorationData>();
    }
    
    [System.Serializable]
    public class DecorationData
    {
        public string itemId;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale = Vector3.one;
    }
    
    [System.Serializable]
    public class MediaItemData
    {
        public string mediaId;
        public string mediaType;
        public string sourcePath;
        public string displayStyle;
        public Vector3 position;
        public Quaternion rotation;
    }
}