using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ClubNeko.Golf
{
    /// <summary>
    /// シンプルなゴルフショットコントローラー
    /// </summary>
    public class SimpleShotController : MonoBehaviour
    {
        [Header("References")]
        public BallPhysics golfBall;
        public Camera mainCamera;
        public LineRenderer trajectoryLine;
        public Transform aimArrow;
        
        [Header("Shot Settings")]
        public float minPower = 10f;
        public float maxPower = 100f;
        public float angleMin = 0f;
        public float angleMax = 60f;
        public float aimSensitivity = 2f;
        
        [Header("Current Shot")]
        public float currentPower = 0f;
        public float currentAngle = 30f;
        public Vector3 aimDirection;
        
        [Header("UI")]
        public Slider powerSlider;
        public Text powerText;
        public Text angleText;
        public Text distanceText;
        
        private bool isAiming = false;
        private bool isChargingPower = false;
        private Vector3 ballStartPosition;
        private int shotCount = 0;
        
        private void Start()
        {
            InitializeComponents();
            ResetShot();
        }
        
        private void InitializeComponents()
        {
            // カメラ取得
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
            
            // ボール取得
            if (golfBall == null)
            {
                golfBall = FindFirstObjectByType<BallPhysics>();
            }
            
            // 軌道線設定
            if (trajectoryLine == null)
            {
                GameObject lineObj = new GameObject("TrajectoryLine");
                trajectoryLine = lineObj.AddComponent<LineRenderer>();
            }
            ConfigureTrajectoryLine();
            
            // エイム矢印作成
            if (aimArrow == null)
            {
                CreateAimArrow();
            }
            
            // ボールイベント登録
            if (golfBall != null)
            {
                golfBall.OnBallStopped += OnBallStopped;
                ballStartPosition = golfBall.transform.position;
            }
        }
        
        private void ConfigureTrajectoryLine()
        {
            trajectoryLine.startWidth = 0.05f;
            trajectoryLine.endWidth = 0.02f;
            trajectoryLine.material = new Material(Shader.Find("Sprites/Default"));
            trajectoryLine.startColor = new Color(1f, 1f, 0f, 0.8f);
            trajectoryLine.endColor = new Color(1f, 0.5f, 0f, 0.2f);
            trajectoryLine.positionCount = 30;
            trajectoryLine.enabled = false;
        }
        
        private void CreateAimArrow()
        {
            GameObject arrow = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            arrow.name = "AimArrow";
            arrow.transform.localScale = new Vector3(0.2f, 2f, 0.2f);
            arrow.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            
            // マテリアル設定
            Renderer renderer = arrow.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Standard"));
            renderer.material.color = new Color(1f, 0f, 0f, 0.5f);
            
            // コライダー無効化
            Destroy(arrow.GetComponent<Collider>());
            
            aimArrow = arrow.transform;
        }
        
        private void Update()
        {
            if (golfBall == null || golfBall.isMoving) return;
            
            HandleInput();
            UpdateAiming();
            UpdateUI();
        }
        
        private void HandleInput()
        {
            // エイム開始/終了
            if (Input.GetMouseButtonDown(0))
            {
                StartAiming();
            }
            
            // パワー調整
            if (isAiming)
            {
                if (Input.GetKey(KeyCode.Space))
                {
                    ChargePower();
                }
                else if (Input.GetKeyUp(KeyCode.Space) && isChargingPower)
                {
                    Shoot();
                }
                
                // 角度調整
                if (Input.GetKey(KeyCode.Q))
                {
                    currentAngle = Mathf.Clamp(currentAngle - 30f * Time.deltaTime, angleMin, angleMax);
                }
                if (Input.GetKey(KeyCode.E))
                {
                    currentAngle = Mathf.Clamp(currentAngle + 30f * Time.deltaTime, angleMin, angleMax);
                }
            }
            
            // キャンセル
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CancelAiming();
            }
            
            // リセット
            if (Input.GetKeyDown(KeyCode.R))
            {
                ResetBallPosition();
            }
        }
        
        private void StartAiming()
        {
            isAiming = true;
            trajectoryLine.enabled = true;
            aimArrow.gameObject.SetActive(true);
        }
        
        private void CancelAiming()
        {
            isAiming = false;
            isChargingPower = false;
            currentPower = 0f;
            trajectoryLine.enabled = false;
            aimArrow.gameObject.SetActive(false);
        }
        
        private void ChargePower()
        {
            isChargingPower = true;
            currentPower = Mathf.Clamp(currentPower + 50f * Time.deltaTime, minPower, maxPower);
            
            if (powerSlider != null)
            {
                powerSlider.value = currentPower / maxPower;
            }
        }
        
        private void UpdateAiming()
        {
            if (!isAiming) return;
            
            // マウス位置から方向を計算
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, 100f, LayerMask.GetMask("Default")))
            {
                Vector3 targetPos = hit.point;
                targetPos.y = golfBall.transform.position.y;
                
                Vector3 direction = (targetPos - golfBall.transform.position).normalized;
                aimDirection = direction;
                
                // 矢印の位置と向き更新
                UpdateAimArrow();
                
                // 軌道予測更新
                UpdateTrajectory();
            }
        }
        
        private void UpdateAimArrow()
        {
            if (aimArrow == null) return;
            
            Vector3 arrowPos = golfBall.transform.position + aimDirection * 1f;
            arrowPos.y = golfBall.transform.position.y;
            aimArrow.position = arrowPos;
            
            // 矢印を方向に向ける
            aimArrow.rotation = Quaternion.LookRotation(aimDirection) * Quaternion.Euler(90f, 0f, 0f);
            
            // パワーに応じてサイズ変更
            float scale = 0.5f + (currentPower / maxPower) * 1.5f;
            aimArrow.localScale = new Vector3(0.2f, scale, 0.2f);
        }
        
        private void UpdateTrajectory()
        {
            if (trajectoryLine == null) return;
            
            Vector3 startPos = golfBall.transform.position;
            Vector3 velocity = CalculateInitialVelocity();
            
            for (int i = 0; i < trajectoryLine.positionCount; i++)
            {
                float time = i * 0.1f;
                Vector3 point = startPos + velocity * time;
                point.y = startPos.y + velocity.y * time - 0.5f * 9.81f * time * time;
                
                // 地面より下には表示しない
                if (point.y < 0) point.y = 0;
                
                trajectoryLine.SetPosition(i, point);
            }
        }
        
        private Vector3 CalculateInitialVelocity()
        {
            float radAngle = currentAngle * Mathf.Deg2Rad;
            float power = currentPower;
            
            Vector3 velocity = aimDirection * power * Mathf.Cos(radAngle);
            velocity.y = power * Mathf.Sin(radAngle);
            
            return velocity * 0.5f; // スケール調整
        }
        
        private void Shoot()
        {
            if (golfBall == null || currentPower <= 0) return;
            
            Vector3 force = CalculateInitialVelocity();
            Vector3 hitPoint = golfBall.transform.position + Vector3.down * 0.01f;
            
            golfBall.Hit(force, hitPoint);
            
            shotCount++;
            Debug.Log($"Shot {shotCount}: Power={currentPower:F1}, Angle={currentAngle:F1}°");
            
            CancelAiming();
        }
        
        private void OnBallStopped(Vector3 position)
        {
            float distance = Vector3.Distance(ballStartPosition, position);
            
            if (distanceText != null)
            {
                distanceText.text = $"Distance: {distance:F1}m";
            }
            
            Debug.Log($"Ball stopped at {position}. Distance: {distance:F1}m");
        }
        
        private void ResetBallPosition()
        {
            if (golfBall != null)
            {
                golfBall.ResetBall(ballStartPosition);
                shotCount = 0;
            }
        }
        
        private void ResetShot()
        {
            currentPower = 0f;
            currentAngle = 30f;
            aimDirection = Vector3.forward;
            isAiming = false;
            isChargingPower = false;
        }
        
        private void UpdateUI()
        {
            if (powerText != null)
            {
                powerText.text = $"Power: {currentPower:F0}%";
            }
            
            if (angleText != null)
            {
                angleText.text = $"Angle: {currentAngle:F0}°";
            }
        }
        
        private void OnDestroy()
        {
            if (golfBall != null)
            {
                golfBall.OnBallStopped -= OnBallStopped;
            }
        }
    }
}