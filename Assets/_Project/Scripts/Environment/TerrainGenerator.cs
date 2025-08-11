using UnityEngine;
using ClubNeko.Data;

namespace ClubNeko.Environment
{
    /// <summary>
    /// ゼルダTotK風のスタイライズドテレイン生成
    /// </summary>
    public class TerrainGenerator : MonoBehaviour
    {
        [Header("Terrain Settings")]
        public TerrainSettings terrainSettings;
        public Terrain targetTerrain;
        
        [Header("Generation Parameters")]
        public int seed = 12345;
        public float noiseScale = 0.01f;
        public int octaves = 4;
        public float persistence = 0.5f;
        public float lacunarity = 2f;
        
        [Header("Height Layers")]
        public AnimationCurve heightCurve;
        public float heightMultiplier = 300f;
        
        [Header("Stylized Features")]
        public bool generateFloatingIslands = false;
        public int numberOfIslands = 5;
        public float minIslandHeight = 100f;
        public float maxIslandHeight = 300f;
        
        [Header("Biome Blending")]
        public bool useMultipleBiomes = true;
        public float biomeTransitionSmoothness = 50f;
        
        private TerrainData terrainData;
        
        private void Start()
        {
            if (targetTerrain == null)
            {
                targetTerrain = GetComponent<Terrain>();
            }
            
            if (targetTerrain != null && terrainSettings != null)
            {
                GenerateTerrain();
            }
        }
        
        [ContextMenu("Generate Terrain")]
        public void GenerateTerrain()
        {
            if (targetTerrain == null)
            {
                Debug.LogError("No terrain assigned!");
                return;
            }
            
            terrainData = targetTerrain.terrainData;
            
            // Apply settings
            if (terrainSettings != null)
            {
                terrainSettings.ApplyToTerrain(targetTerrain);
            }
            
            // Generate heightmap
            GenerateHeightmap();
            
            // Generate textures
            PaintTerrain();
            
            // Place vegetation
            PlaceVegetation();
            
            // Add floating islands if enabled
            if (generateFloatingIslands)
            {
                GenerateFloatingIslands();
            }
            
            // Apply stylized shading
            ApplyStylizedShading();
            
            Debug.Log("Terrain generation complete!");
        }
        
        private void GenerateHeightmap()
        {
            int width = terrainData.heightmapResolution;
            int height = terrainData.heightmapResolution;
            float[,] heights = new float[width, height];
            
            System.Random prng = new System.Random(seed);
            Vector2[] octaveOffsets = new Vector2[octaves];
            
            for (int i = 0; i < octaves; i++)
            {
                float offsetX = prng.Next(-100000, 100000);
                float offsetY = prng.Next(-100000, 100000);
                octaveOffsets[i] = new Vector2(offsetX, offsetY);
            }
            
            float maxNoiseHeight = float.MinValue;
            float minNoiseHeight = float.MaxValue;
            
            // Generate noise map
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float amplitude = 1;
                    float frequency = 1;
                    float noiseHeight = 0;
                    
                    for (int i = 0; i < octaves; i++)
                    {
                        float sampleX = (x + octaveOffsets[i].x) * noiseScale * frequency;
                        float sampleY = (y + octaveOffsets[i].y) * noiseScale * frequency;
                        
                        float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                        noiseHeight += perlinValue * amplitude;
                        
                        amplitude *= persistence;
                        frequency *= lacunarity;
                    }
                    
                    maxNoiseHeight = Mathf.Max(maxNoiseHeight, noiseHeight);
                    minNoiseHeight = Mathf.Min(minNoiseHeight, noiseHeight);
                    
                    heights[x, y] = noiseHeight;
                }
            }
            
            // Normalize and apply curve
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float normalizedHeight = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, heights[x, y]);
                    
                    // Apply height curve for more interesting terrain
                    if (heightCurve != null && heightCurve.length > 0)
                    {
                        normalizedHeight = heightCurve.Evaluate(normalizedHeight);
                    }
                    
                    // Add some plateau areas (Zelda-style)
                    if (normalizedHeight > 0.6f && normalizedHeight < 0.7f)
                    {
                        normalizedHeight = Mathf.Lerp(normalizedHeight, 0.65f, 0.5f);
                    }
                    
                    heights[x, y] = normalizedHeight;
                }
            }
            
            terrainData.SetHeights(0, 0, heights);
        }
        
        private void PaintTerrain()
        {
            if (terrainSettings == null || terrainSettings.terrainLayers == null) return;
            
            TerrainLayer[] terrainLayers = terrainSettings.terrainLayers;
            if (terrainLayers.Length == 0) return;
            
            terrainData.terrainLayers = terrainLayers;
            
            int width = terrainData.alphamapWidth;
            int height = terrainData.alphamapHeight;
            float[,,] alphamaps = new float[width, height, terrainLayers.Length];
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float normalizedX = x / (float)width;
                    float normalizedY = y / (float)height;
                    
                    float terrainHeight = terrainData.GetInterpolatedHeight(normalizedX, normalizedY);
                    float normalizedHeight = terrainHeight / terrainData.size.y;
                    
                    // Calculate slope
                    float steepness = terrainData.GetSteepness(normalizedX, normalizedY);
                    
                    // Layer 0: Grass (low areas)
                    if (terrainLayers.Length > 0)
                    {
                        alphamaps[x, y, 0] = Mathf.Clamp01(1f - normalizedHeight * 2f - steepness / 90f);
                    }
                    
                    // Layer 1: Rock (steep areas)
                    if (terrainLayers.Length > 1)
                    {
                        alphamaps[x, y, 1] = Mathf.Clamp01(steepness / 45f);
                    }
                    
                    // Layer 2: Dirt (mid areas)
                    if (terrainLayers.Length > 2)
                    {
                        alphamaps[x, y, 2] = Mathf.Clamp01(normalizedHeight - steepness / 90f);
                    }
                    
                    // Layer 3: Snow (high areas)
                    if (terrainLayers.Length > 3)
                    {
                        alphamaps[x, y, 3] = Mathf.Clamp01((normalizedHeight - 0.7f) * 3f);
                    }
                    
                    // Normalize weights
                    float total = 0;
                    for (int i = 0; i < terrainLayers.Length; i++)
                    {
                        total += alphamaps[x, y, i];
                    }
                    
                    if (total > 0)
                    {
                        for (int i = 0; i < terrainLayers.Length; i++)
                        {
                            alphamaps[x, y, i] /= total;
                        }
                    }
                }
            }
            
            terrainData.SetAlphamaps(0, 0, alphamaps);
        }
        
        private void PlaceVegetation()
        {
            if (terrainSettings == null) return;
            
            // Place grass
            if (terrainSettings.grassPrototypes != null && terrainSettings.grassPrototypes.Length > 0)
            {
                PlaceGrass();
            }
            
            // Place trees
            if (terrainSettings.treePrototypes != null && terrainSettings.treePrototypes.Length > 0)
            {
                PlaceTrees();
            }
        }
        
        private void PlaceGrass()
        {
            int detailResolution = terrainData.detailResolution;
            
            for (int layerIndex = 0; layerIndex < terrainData.detailPrototypes.Length; layerIndex++)
            {
                int[,] detailLayer = new int[detailResolution, detailResolution];
                
                for (int y = 0; y < detailResolution; y++)
                {
                    for (int x = 0; x < detailResolution; x++)
                    {
                        float normalizedX = x / (float)detailResolution;
                        float normalizedY = y / (float)detailResolution;
                        
                        float height = terrainData.GetInterpolatedHeight(normalizedX, normalizedY);
                        float normalizedHeight = height / terrainData.size.y;
                        float steepness = terrainData.GetSteepness(normalizedX, normalizedY);
                        
                        // Only place grass on relatively flat, low areas
                        if (normalizedHeight < 0.5f && steepness < 30f)
                        {
                            float noise = Mathf.PerlinNoise(x * 0.05f, y * 0.05f);
                            if (noise > 0.3f)
                            {
                                detailLayer[x, y] = Random.Range(1, 4);
                            }
                        }
                    }
                }
                
                terrainData.SetDetailLayer(0, 0, layerIndex, detailLayer);
            }
        }
        
        private void PlaceTrees()
        {
            int numberOfTrees = 100; // Adjust based on terrain size
            
            for (int i = 0; i < numberOfTrees; i++)
            {
                float x = Random.Range(0f, 1f);
                float z = Random.Range(0f, 1f);
                
                float height = terrainData.GetInterpolatedHeight(x, z);
                float normalizedHeight = height / terrainData.size.y;
                float steepness = terrainData.GetSteepness(x, z);
                
                // Only place trees in suitable areas
                if (normalizedHeight > 0.1f && normalizedHeight < 0.6f && steepness < 25f)
                {
                    TreeInstance tree = new TreeInstance();
                    tree.position = new Vector3(x, 0, z);
                    tree.prototypeIndex = Random.Range(0, terrainData.treePrototypes.Length);
                    tree.widthScale = Random.Range(0.8f, 1.2f);
                    tree.heightScale = Random.Range(0.8f, 1.2f);
                    tree.rotation = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                    tree.color = Color.white;
                    tree.lightmapColor = Color.white;
                    
                    targetTerrain.AddTreeInstance(tree);
                }
            }
            
            targetTerrain.Flush();
        }
        
        private void GenerateFloatingIslands()
        {
            for (int i = 0; i < numberOfIslands; i++)
            {
                Vector3 position = new Vector3(
                    Random.Range(100f, terrainData.size.x - 100f),
                    Random.Range(minIslandHeight, maxIslandHeight),
                    Random.Range(100f, terrainData.size.z - 100f)
                );
                
                GameObject island = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                island.name = $"FloatingIsland_{i}";
                island.transform.position = position;
                island.transform.localScale = new Vector3(
                    Random.Range(30f, 80f),
                    Random.Range(10f, 20f),
                    Random.Range(30f, 80f)
                );
                
                // Apply a stylized material
                Renderer renderer = island.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = new Color(0.4f, 0.6f, 0.3f);
                }
                
                // Add some vegetation on top
                // This would require additional implementation
            }
        }
        
        private void ApplyStylizedShading()
        {
            // Apply Zelda-style toon shading
            // This would typically involve custom shaders
            
            // Set up lighting for stylized look
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.5f, 0.7f, 0.9f);
            RenderSettings.ambientEquatorColor = new Color(0.4f, 0.5f, 0.6f);
            RenderSettings.ambientGroundColor = new Color(0.2f, 0.3f, 0.3f);
            
            // Fog settings for atmosphere
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.7f, 0.8f, 0.9f, 1f);
            RenderSettings.fogDensity = 0.01f;
        }
    }
}