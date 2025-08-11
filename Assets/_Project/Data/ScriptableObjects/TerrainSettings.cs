using UnityEngine;

namespace ClubNeko.Data
{
    /// <summary>
    /// テレイン設定用ScriptableObject
    /// ゼルダTotK風の美しい地形を生成
    /// </summary>
    [CreateAssetMenu(fileName = "TerrainSettings", menuName = "Club Neko/Terrain Settings")]
    public class TerrainSettings : ScriptableObject
    {
        [Header("Terrain Size")]
        public int terrainWidth = 2000;
        public int terrainLength = 2000;
        public int terrainHeight = 600;
        
        [Header("Resolution")]
        public int heightmapResolution = 513;
        public int detailResolution = 1024;
        public int detailResolutionPerPatch = 32;
        public int controlTextureResolution = 512;
        public int baseTextureResolution = 1024;
        
        [Header("Terrain Layers")]
        public TerrainLayer[] terrainLayers;
        
        [Header("Grass & Detail")]
        public DetailPrototype[] grassPrototypes;
        public TreePrototype[] treePrototypes;
        
        [Header("Wind Settings")]
        public float windSpeed = 1f;
        public float windSize = 1f;
        public float windBending = 1f;
        public Color grassTint = new Color(0.7f, 0.8f, 0.6f, 1f);
        
        [Header("Lighting")]
        public bool drawTreesAndFoliage = true;
        public float pixelError = 5;
        public float basemapDistance = 1000;
        public float shadowCastingMode = 2;
        
        [Header("Stylized Settings")]
        public bool useStylizedShading = true;
        public Gradient heightColorGradient;
        public AnimationCurve heightCurve;
        
        [Header("Biomes")]
        public BiomeSettings[] biomes;
        
        [System.Serializable]
        public class BiomeSettings
        {
            public string biomeName = "Grassland";
            public float minHeight = 0f;
            public float maxHeight = 1f;
            public Color biomeColor = Color.green;
            public TerrainLayer[] layers;
            public GameObject[] props;
        }
        
        public void ApplyToTerrain(Terrain terrain)
        {
            if (terrain == null) return;
            
            TerrainData terrainData = terrain.terrainData;
            
            // サイズ設定
            terrainData.heightmapResolution = heightmapResolution;
            terrainData.size = new Vector3(terrainWidth, terrainHeight, terrainLength);
            
            // 解像度設定
            terrainData.SetDetailResolution(detailResolution, detailResolutionPerPatch);
            
            // テレインレイヤー適用
            if (terrainLayers != null && terrainLayers.Length > 0)
            {
                terrainData.terrainLayers = terrainLayers;
            }
            
            // 草と木の設定
            if (grassPrototypes != null)
            {
                terrainData.detailPrototypes = grassPrototypes;
            }
            
            if (treePrototypes != null)
            {
                terrainData.treePrototypes = treePrototypes;
            }
            
            // 風の設定
            terrain.terrainData.wavingGrassSpeed = windSpeed;
            terrain.terrainData.wavingGrassAmount = windSize;
            terrain.terrainData.wavingGrassStrength = windBending;
            terrain.terrainData.wavingGrassTint = grassTint;
            
            // 描画設定
            terrain.drawTreesAndFoliage = drawTreesAndFoliage;
            terrain.heightmapPixelError = pixelError;
            terrain.basemapDistance = basemapDistance;
            
            Debug.Log($"Terrain settings applied to {terrain.name}");
        }
    }
}