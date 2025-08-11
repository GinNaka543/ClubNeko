using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ClubNeko.Core
{
    /// <summary>
    /// ゲーム全体を管理するシングルトンマネージャー
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        private static GameManager instance;
        public static GameManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<GameManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("GameManager");
                        instance = go.AddComponent<GameManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }

        [Header("Game Settings")]
        public GameMode currentGameMode = GameMode.MainMenu;
        public GameMode gameMode = GameMode.SinglePlayer;
        public bool isOnlineMode = false;
        
        [Header("Player Data")]
        public PlayerData currentPlayer;
        public HouseData playerHouse;
        
        [Header("System")]
        public bool debugMode = false;
        public int targetFrameRate = 60;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeGame();
        }

        private void InitializeGame()
        {
            // フレームレート設定
            Application.targetFrameRate = targetFrameRate;
            
            // iOS向け設定
            #if UNITY_IOS
            UnityEngine.iOS.Device.SetNoBackupFlag(Application.persistentDataPath);
            #endif
            
            // プレイヤーデータロード
            LoadPlayerData();
            
            // 初期シーンロード
            if (SceneManager.GetActiveScene().name != "Boot")
            {
                SceneManager.LoadScene("MainMenu");
            }
        }

        public void LoadPlayerData()
        {
            // PlayerPrefsまたはセーブファイルからデータロード
            string playerDataJson = PlayerPrefs.GetString("PlayerData", "");
            if (!string.IsNullOrEmpty(playerDataJson))
            {
                currentPlayer = JsonUtility.FromJson<PlayerData>(playerDataJson);
            }
            else
            {
                currentPlayer = new PlayerData();
                currentPlayer.playerId = System.Guid.NewGuid().ToString();
                currentPlayer.playerName = "ネコプレイヤー";
            }
        }

        public void SavePlayerData()
        {
            string json = JsonUtility.ToJson(currentPlayer);
            PlayerPrefs.SetString("PlayerData", json);
            PlayerPrefs.Save();
        }

        public void LoadGolfScene(string courseName)
        {
            currentGameMode = GameMode.Golf;
            StartCoroutine(LoadSceneAsync($"Golf_{courseName}"));
        }

        public void LoadHouseScene()
        {
            currentGameMode = GameMode.House;
            StartCoroutine(LoadSceneAsync("House"));
        }

        public void LoadMainMenu()
        {
            currentGameMode = GameMode.MainMenu;
            StartCoroutine(LoadSceneAsync("MainMenu"));
        }

        private IEnumerator LoadSceneAsync(string sceneName)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            
            while (!asyncLoad.isDone)
            {
                float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
                // ローディング画面の更新など
                yield return null;
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SavePlayerData();
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                SavePlayerData();
            }
        }
    }

    public enum GameMode
    {
        MainMenu,
        Golf,
        House,
        Loading,
        SinglePlayer,
        Multiplayer,
        Tournament
    }

    [System.Serializable]
    public class PlayerData
    {
        public string playerId;
        public string playerName;
        public int level = 1;
        public int experience = 0;
        public int coins = 1000;
        public int gems = 50;
        public List<string> unlockedWeapons = new List<string>();
        public List<string> vrmCharacters = new List<string>();
    }

    [System.Serializable]
    public class HouseData
    {
        public string layoutId = "basic_house";
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
        public string mediaType; // Photo, Video
        public string sourcePath;
        public string displayStyle; // Frame, Projection, Hologram
        public Vector3 position;
        public Quaternion rotation;
    }
}