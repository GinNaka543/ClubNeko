using UnityEngine;

namespace ClubNeko.Golf
{
    /// <summary>
    /// シンプルなゴルフコース地形生成
    /// </summary>
    public class SimpleTerrainGenerator : MonoBehaviour
    {
        [Header("Terrain Settings")]
        public int terrainWidth = 200;
        public int terrainLength = 500;
        public int terrainHeight = 30;
        public float terrainScale = 0.01f;
        
        [Header("Course Elements")]
        public Vector3 teePosition = new Vector3(100, 10, 50);
        public Vector3 holePosition = new Vector3(100, 5, 450);
        public float fairwayWidth = 30f;
        
        private Terrain terrain;
        private TerrainData terrainData;
        
        private void Start()
        {
            GenerateTerrain();
            CreateCourseElements();
        }
        
        public void GenerateTerrain()
        {
            // TerrainData作成
            terrainData = new TerrainData();
            terrainData.heightmapResolution = 513;
            terrainData.size = new Vector3(terrainWidth, terrainHeight, terrainLength);
            
            // 高さマップ生成
            GenerateHeights();
            
            // Terrainオブジェクト作成
            GameObject terrainObject = Terrain.CreateTerrainGameObject(terrainData);
            terrainObject.name = "Golf Course Terrain";
            terrain = terrainObject.GetComponent<Terrain>();
            
            // テレイン設定
            terrain.materialTemplate = new Material(Shader.Find("Standard"));
            terrain.materialTemplate.color = new Color(0.2f, 0.8f, 0.2f); // 緑色
        }
        
        private void GenerateHeights()
        {
            int width = terrainData.heightmapResolution;
            int height = terrainData.heightmapResolution;
            float[,] heights = new float[width, height];
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float xCoord = (float)x / width * 5;
                    float yCoord = (float)y / height * 5;
                    
                    // Perlinノイズで基本地形
                    float baseHeight = Mathf.PerlinNoise(xCoord, yCoord) * 0.05f;
                    
                    // フェアウェイは平坦に
                    float centerX = width / 2;
                    float distanceFromCenter = Mathf.Abs(x - centerX) / (width * 0.5f);
                    
                    if (distanceFromCenter < 0.3f) // フェアウェイ幅
                    {
                        baseHeight *= 0.3f; // より平坦に
                    }
                    
                    // ティーグラウンド周辺を平坦に
                    float teeDistance = Vector2.Distance(
                        new Vector2(x, y),
                        new Vector2(width * 0.5f, height * 0.1f)
                    );
                    if (teeDistance < 30)
                    {
                        baseHeight *= 0.2f;
                    }
                    
                    // グリーン周辺を平坦に
                    float greenDistance = Vector2.Distance(
                        new Vector2(x, y),
                        new Vector2(width * 0.5f, height * 0.9f)
                    );
                    if (greenDistance < 40)
                    {
                        baseHeight *= 0.1f;
                    }
                    
                    heights[x, y] = baseHeight;
                }
            }
            
            terrainData.SetHeights(0, 0, heights);
        }
        
        private void CreateCourseElements()
        {
            // ティーマーカー
            CreateTeeMarker();
            
            // ホール（カップ）
            CreateHole();
            
            // ゴルフボール配置
            CreateGolfBall();
        }
        
        private void CreateTeeMarker()
        {
            GameObject teeMarker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            teeMarker.name = "Tee Marker";
            teeMarker.transform.position = teePosition;
            teeMarker.transform.localScale = new Vector3(2f, 0.1f, 2f);
            
            Renderer renderer = teeMarker.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Standard"));
            renderer.material.color = Color.blue;
        }
        
        private void CreateHole()
        {
            // ホール（カップ）
            GameObject hole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hole.name = "Golf Hole";
            hole.transform.position = holePosition;
            hole.transform.localScale = new Vector3(0.3f, 0.01f, 0.3f);
            
            Renderer renderer = hole.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Standard"));
            renderer.material.color = Color.black;
            
            // ホールトリガー
            GameObject holeTrigger = new GameObject("Hole Trigger");
            holeTrigger.transform.position = holePosition;
            SphereCollider trigger = holeTrigger.AddComponent<SphereCollider>();
            trigger.radius = 0.2f;
            trigger.isTrigger = true;
            
            // ホール検出スクリプト
            holeTrigger.AddComponent<HoleDetector>();
            
            // フラッグポール
            CreateFlagPole(holePosition + Vector3.up * 2f);
        }
        
        private void CreateFlagPole(Vector3 position)
        {
            // ポール
            GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pole.name = "Flag Pole";
            pole.transform.position = position;
            pole.transform.localScale = new Vector3(0.05f, 2f, 0.05f);
            
            Renderer poleRenderer = pole.GetComponent<Renderer>();
            poleRenderer.material = new Material(Shader.Find("Standard"));
            poleRenderer.material.color = Color.white;
            
            // フラッグ
            GameObject flag = GameObject.CreatePrimitive(PrimitiveType.Quad);
            flag.name = "Flag";
            flag.transform.position = position + Vector3.up * 1.5f + Vector3.right * 0.3f;
            flag.transform.localScale = new Vector3(0.6f, 0.4f, 1f);
            flag.transform.rotation = Quaternion.Euler(0, 90, 0);
            
            Renderer flagRenderer = flag.GetComponent<Renderer>();
            flagRenderer.material = new Material(Shader.Find("Standard"));
            flagRenderer.material.color = Color.red;
        }
        
        private void CreateGolfBall()
        {
            GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ball.name = "Golf Ball";
            ball.transform.position = teePosition + Vector3.up * 0.5f;
            ball.transform.localScale = Vector3.one * 0.042f; // 実際のゴルフボールサイズ
            
            // 物理設定
            Rigidbody rb = ball.AddComponent<Rigidbody>();
            rb.mass = 0.045f;
            
            // BallPhysicsコンポーネント追加
            ball.AddComponent<BallPhysics>();
            
            // マテリアル設定
            Renderer renderer = ball.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Standard"));
            renderer.material.color = Color.white;
        }
    }
    
    /// <summary>
    /// ホール検出
    /// </summary>
    public class HoleDetector : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("GolfBall") || other.name.Contains("Golf Ball"))
            {
                Debug.Log("⛳ HOLE IN! ⛳");
                
                // パーティクルエフェクトなど
                StartCoroutine(CelebrationEffect(other.transform.position));
            }
        }
        
        private System.Collections.IEnumerator CelebrationEffect(Vector3 position)
        {
            // 簡単なお祝いエフェクト
            for (int i = 0; i < 10; i++)
            {
                GameObject particle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                particle.transform.position = position;
                particle.transform.localScale = Vector3.one * 0.1f;
                
                Rigidbody rb = particle.AddComponent<Rigidbody>();
                rb.linearVelocity = Random.insideUnitSphere * 5f + Vector3.up * 10f;
                
                Destroy(particle, 2f);
                
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
}