using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ClubNeko.Golf
{
    /// <summary>
    /// ゴルフのメインコントローラー
    /// </summary>
    public class GolfController : MonoBehaviour
    {
        [Header("Golf Ball")]
        public GameObject golfBall;
        public Rigidbody ballRigidbody;
        public TrailRenderer ballTrail;
        
        [Header("Current Weapon")]
        public WeaponType currentWeapon = WeaponType.Driver;
        public float shotPower = 0f;
        public float maxShotPower = 100f;
        
        [Header("Aiming")]
        public Transform aimingArrow;
        public LineRenderer trajectoryLine;
        public int trajectoryPoints = 30;
        
        [Header("Team Settings")]
        public bool isTeamLinked = false;
        public Transform partnerTransform;
        public float linkDistance = 2f;
        
        [Header("Shot Settings")]
        public float chargeSpeed = 50f;
        public AnimationCurve powerCurve;
        
        private Vector3 shotDirection;
        private bool isCharging = false;
        private bool canShoot = true;
        private WeatherSystem weatherSystem;
        private Camera mainCamera;
        
        private void Start()
        {
            mainCamera = Camera.main;
            weatherSystem = FindFirstObjectByType<WeatherSystem>();
            
            if (ballRigidbody == null && golfBall != null)
            {
                ballRigidbody = golfBall.GetComponent<Rigidbody>();
            }
            
            InitializeWeapon();
        }
        
        private void Update()
        {
            if (!canShoot) return;
            
            UpdateAiming();
            UpdateTeamLink();
            
            // ショット処理
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                StartCharging();
            }
            else if (Mouse.current.leftButton.wasReleasedThisFrame && isCharging)
            {
                ReleaseShot();
            }
            
            if (isCharging)
            {
                UpdateCharge();
            }
            
            // 武器切り替え
            if (Keyboard.current.digit1Key.wasPressedThisFrame) SwitchWeapon(WeaponType.Driver);
            if (Keyboard.current.digit2Key.wasPressedThisFrame) SwitchWeapon(WeaponType.Iron);
            if (Keyboard.current.digit3Key.wasPressedThisFrame) SwitchWeapon(WeaponType.Putter);
            if (Keyboard.current.digit4Key.wasPressedThisFrame) SwitchWeapon(WeaponType.Bazooka);
            if (Keyboard.current.digit5Key.wasPressedThisFrame) SwitchWeapon(WeaponType.Bat);
            if (Keyboard.current.digit6Key.wasPressedThisFrame) SwitchWeapon(WeaponType.AirCannon);
        }
        
        private void UpdateAiming()
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            Plane groundPlane = new Plane(Vector3.up, golfBall.transform.position);
            
            if (groundPlane.Raycast(ray, out float distance))
            {
                Vector3 targetPoint = ray.GetPoint(distance);
                shotDirection = (targetPoint - golfBall.transform.position).normalized;
                shotDirection.y = 0;
                
                if (aimingArrow != null)
                {
                    aimingArrow.position = golfBall.transform.position;
                    aimingArrow.rotation = Quaternion.LookRotation(shotDirection);
                }
                
                UpdateTrajectory();
            }
        }
        
        private void UpdateTrajectory()
        {
            if (trajectoryLine == null) return;
            
            Vector3[] points = new Vector3[trajectoryPoints];
            Vector3 startPos = golfBall.transform.position;
            Vector3 velocity = CalculateShotVelocity();
            
            for (int i = 0; i < trajectoryPoints; i++)
            {
                float time = i * 0.1f;
                points[i] = startPos + velocity * time;
                points[i].y = startPos.y + velocity.y * time - 0.5f * Physics.gravity.magnitude * time * time;
                
                // 風の影響を追加
                if (weatherSystem != null)
                {
                    points[i] += weatherSystem.GetWindForce() * time;
                }
            }
            
            trajectoryLine.positionCount = trajectoryPoints;
            trajectoryLine.SetPositions(points);
        }
        
        private void StartCharging()
        {
            isCharging = true;
            shotPower = 0f;
        }
        
        private void UpdateCharge()
        {
            shotPower = Mathf.Min(shotPower + chargeSpeed * Time.deltaTime, maxShotPower);
            
            // チャージUI更新
            UpdateChargeUI();
        }
        
        private void ReleaseShot()
        {
            isCharging = false;
            PerformShot();
        }
        
        private void PerformShot()
        {
            canShoot = false;
            
            Vector3 velocity = CalculateShotVelocity();
            
            // チーム連携ボーナス
            if (isTeamLinked)
            {
                velocity *= 1.2f; // 20%パワーアップ
            }
            
            // ボールを飛ばす
            ballRigidbody.linearVelocity = velocity;
            
            // エフェクト
            PlayShotEffect();
            
            // ショット後の処理
            StartCoroutine(ShotCooldown());
        }
        
        private Vector3 CalculateShotVelocity()
        {
            WeaponData weaponData = GetWeaponData(currentWeapon);
            
            float power = shotPower * weaponData.powerMultiplier;
            float angle = weaponData.launchAngle;
            
            // 基本速度計算
            Vector3 velocity = shotDirection * power;
            velocity.y = power * Mathf.Sin(angle * Mathf.Deg2Rad);
            
            // 天候による影響
            if (weatherSystem != null && !weaponData.isWeatherResistant)
            {
                velocity *= weatherSystem.GetPowerModifier();
            }
            
            return velocity;
        }
        
        private void UpdateTeamLink()
        {
            if (partnerTransform == null) return;
            
            float distance = Vector3.Distance(transform.position, partnerTransform.position);
            isTeamLinked = distance <= linkDistance;
            
            // リンク状態のビジュアル表示
            if (isTeamLinked)
            {
                Debug.DrawLine(transform.position, partnerTransform.position, Color.green);
            }
        }
        
        private void SwitchWeapon(WeaponType newWeapon)
        {
            currentWeapon = newWeapon;
            InitializeWeapon();
        }
        
        private void InitializeWeapon()
        {
            WeaponData data = GetWeaponData(currentWeapon);
            maxShotPower = data.maxPower;
            
            // 武器モデルの切り替えなど
        }
        
        private WeaponData GetWeaponData(WeaponType type)
        {
            // 武器データを返す（実際にはScriptableObjectなどから取得）
            return type switch
            {
                WeaponType.Driver => new WeaponData { powerMultiplier = 2.0f, launchAngle = 45f, maxPower = 100f },
                WeaponType.Iron => new WeaponData { powerMultiplier = 1.5f, launchAngle = 35f, maxPower = 80f },
                WeaponType.Putter => new WeaponData { powerMultiplier = 0.5f, launchAngle = 5f, maxPower = 30f, isWeatherResistant = true },
                WeaponType.Bazooka => new WeaponData { powerMultiplier = 3.0f, launchAngle = 60f, maxPower = 150f },
                WeaponType.Bat => new WeaponData { powerMultiplier = 1.2f, launchAngle = 25f, maxPower = 60f },
                WeaponType.AirCannon => new WeaponData { powerMultiplier = 1.8f, launchAngle = 50f, maxPower = 90f, isWeatherResistant = true },
                _ => new WeaponData { powerMultiplier = 1.0f, launchAngle = 30f, maxPower = 50f }
            };
        }
        
        private void PlayShotEffect()
        {
            // パーティクルエフェクトやサウンド再生
            if (ballTrail != null)
            {
                ballTrail.enabled = true;
            }
        }
        
        private void UpdateChargeUI()
        {
            // チャージゲージのUI更新
        }
        
        private IEnumerator ShotCooldown()
        {
            yield return new WaitForSeconds(0.5f);
            
            // ボールが止まるまで待つ
            while (ballRigidbody.linearVelocity.magnitude > 0.1f)
            {
                yield return new WaitForSeconds(0.1f);
            }
            
            canShoot = true;
            
            if (ballTrail != null)
            {
                ballTrail.enabled = false;
            }
        }
    }
    
    public enum WeaponType
    {
        Driver,      // ドライバー
        Iron,        // アイアン
        Putter,      // パター
        Bazooka,     // バズーカ
        Bat,         // バット
        AirCannon    // 空気砲
    }
    
    [System.Serializable]
    public class WeaponData
    {
        public float powerMultiplier = 1.0f;
        public float launchAngle = 30f;
        public float maxPower = 100f;
        public bool isWeatherResistant = false;
        public float accuracy = 1.0f;
    }
}