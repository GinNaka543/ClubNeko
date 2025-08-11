using UnityEngine;
using System.Collections.Generic;

namespace ClubNeko.Data
{
    /// <summary>
    /// ゴルフコースデータ用ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "GolfCourseData", menuName = "Club Neko/Golf Course Data")]
    public class GolfCourseData : ScriptableObject
    {
        [Header("Course Info")]
        public string courseName = "New Course";
        public string courseID;
        public Sprite courseThumbnail;
        public string description;
        
        [Header("Course Settings")]
        public CourseTheme theme = CourseTheme.Grassland;
        public CourseDifficulty difficulty = CourseDifficulty.Normal;
        public int numberOfHoles = 18;
        public int totalPar = 72;
        
        [Header("Environment")]
        public GameObject coursePrefab;
        public TerrainSettings terrainSettings;
        public Material skyboxMaterial;
        public Gradient lightingGradient;
        public AnimationCurve fogDensityCurve;
        
        [Header("Holes Data")]
        public List<HoleData> holes = new List<HoleData>();
        
        [Header("Weather Settings")]
        public bool dynamicWeather = true;
        public List<WeatherPreset> availableWeathers = new List<WeatherPreset>();
        public float weatherChangeInterval = 180f; // 3 minutes
        
        [Header("Obstacles")]
        public List<ObstacleData> obstacles = new List<ObstacleData>();
        
        [Header("Power-ups")]
        public List<PowerUpSpawnPoint> powerUpSpawns = new List<PowerUpSpawnPoint>();
        
        [Header("Zelda-style Elements")]
        public bool hasFloatingIslands = false;
        public bool hasAncientRuins = false;
        public bool hasGlowingCrystals = false;
        public List<GameObject> zeldaProps = new List<GameObject>();
        
        [Header("Cat Theme Elements")]
        public bool hasCatStatues = false;
        public List<GameObject> catDecorations = new List<GameObject>();
        public GameObject giantCatPawObstacle;
        
        [Header("Special Features")]
        public bool hasMovingPlatforms = false;
        public bool hasPortals = false;
        public bool hasWindStreams = false;
        public List<SpecialFeature> specialFeatures = new List<SpecialFeature>();
    }
    
    [System.Serializable]
    public class HoleData
    {
        public int holeNumber = 1;
        public string holeName;
        public int par = 4;
        public float distance = 380f; // in yards
        
        [Header("Positions")]
        public Vector3 teePosition;
        public Vector3 holePosition;
        public List<Vector3> fairwayPoints = new List<Vector3>();
        
        [Header("Hazards")]
        public List<Vector3> waterHazards = new List<Vector3>();
        public List<Vector3> sandBunkers = new List<Vector3>();
        public List<Vector3> roughAreas = new List<Vector3>();
        
        [Header("Special")]
        public bool hasAlternateRoute = false;
        public Vector3 alternateRouteStart;
        public bool hasSecretPath = false;
        public Vector3 secretPathEntrance;
        
        [Header("Dynamic Elements")]
        public bool hasMovingHole = false;
        public AnimationCurve holeMovementPattern;
        public float holeMovementSpeed = 1f;
    }
    
    [System.Serializable]
    public class ObstacleData
    {
        public string obstacleName;
        public GameObject obstaclePrefab;
        public Vector3 position;
        public Quaternion rotation;
        public ObstacleType type;
        public bool isDestructible = false;
        public float respawnTime = 30f;
    }
    
    [System.Serializable]
    public class PowerUpSpawnPoint
    {
        public Vector3 position;
        public PowerUpType powerUpType;
        public float respawnInterval = 60f;
        public bool isRandomType = false;
    }
    
    [System.Serializable]
    public class WeatherPreset
    {
        public string weatherName;
        public WeatherType weatherType;
        public float probability = 0.2f;
        public float minDuration = 60f;
        public float maxDuration = 180f;
        public Color fogColor;
        public float fogDensity;
        public float windStrength;
        public Vector3 windDirection;
    }
    
    [System.Serializable]
    public class SpecialFeature
    {
        public string featureName;
        public GameObject featurePrefab;
        public Vector3 position;
        public FeatureType type;
        public bool isActive = true;
        
        public enum FeatureType
        {
            JumpPad,
            SpeedBoost,
            Portal,
            WindStream,
            GravityField,
            TimeSlow,
            CatPawPrint
        }
    }
    
    public enum CourseTheme
    {
        Grassland,      // 草原
        Desert,         // 砂漠
        Mountain,       // 山岳
        Beach,          // ビーチ
        Forest,         // 森林
        Sky,            // 空中
        Ruins,          // 古代遺跡
        CatKingdom,     // 猫の王国
        Crystal,        // クリスタル
        Volcanic        // 火山
    }
    
    public enum CourseDifficulty
    {
        Beginner,
        Easy,
        Normal,
        Hard,
        Expert,
        Legendary
    }
    
    public enum ObstacleType
    {
        Static,         // 静的
        Moving,         // 動く
        Rotating,       // 回転
        Appearing,      // 出現/消滅
        Interactive,    // インタラクティブ
        Environmental   // 環境
    }
    
    public enum PowerUpType
    {
        PowerBoost,     // パワーブースト
        AccuracyBoost,  // 精度ブースト
        WindShield,     // 風シールド
        SlowMotion,     // スローモーション
        Teleport,       // テレポート
        MegaShot,       // メガショット
        CatPower        // 猫パワー
    }
    
    public enum WeatherType
    {
        Sunny,
        Rainy,
        Windy,
        Stormy,
        Foggy,
        Snowy,
        Aurora,         // オーロラ（特殊）
        CatStorm        // 猫の嵐（特殊）
    }
}