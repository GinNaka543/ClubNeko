using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ClubNeko.Golf
{
    /// <summary>
    /// ゴルフボールの物理挙動を管理
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(SphereCollider))]
    public class BallPhysics : MonoBehaviour
    {
        [Header("Ball Properties")]
        public float mass = 0.045f; // 標準的なゴルフボールの重さ (kg)
        public float dragCoefficient = 0.47f; // 空気抵抗係数
        public float magnusCoefficient = 0.1f; // マグヌス効果係数
        
        [Header("Physics Settings")]
        public float bounciness = 0.6f; // 反発係数
        public float rollingFriction = 0.1f; // 転がり摩擦
        public float minVelocity = 0.1f; // 停止判定速度
        
        [Header("State")]
        public bool isMoving = false;
        public bool isGrounded = false;
        public Vector3 lastStablePosition;
        
        private Rigidbody rb;
        private SphereCollider sphereCollider;
        private TrailRenderer trail;
        private float groundCheckRadius = 0.051f;
        
        // イベント
        public System.Action<Vector3> OnBallStopped;
        public System.Action<Vector3> OnBallLanded;
        
        private void Awake()
        {
            InitializeComponents();
        }
        
        private void InitializeComponents()
        {
            // Rigidbody設定
            rb = GetComponent<Rigidbody>();
            rb.mass = mass;
            rb.linearDamping = 0.3f; // 空気抵抗
            rb.angularDamping = 0.5f; // 回転抵抗
            rb.useGravity = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            
            // コライダー設定
            sphereCollider = GetComponent<SphereCollider>();
            sphereCollider.radius = 0.021f; // 標準的なゴルフボールの半径
            
            // 物理マテリアル設定
            PhysicsMaterial ballMaterial = new PhysicsMaterial("GolfBall");
            ballMaterial.bounciness = bounciness;
            ballMaterial.dynamicFriction = 0.4f;
            ballMaterial.staticFriction = 0.6f;
            ballMaterial.bounceCombine = PhysicsMaterialCombine.Average;
            ballMaterial.frictionCombine = PhysicsMaterialCombine.Average;
            sphereCollider.material = ballMaterial;
            
            // トレイル設定
            trail = GetComponent<TrailRenderer>();
            if (trail == null)
            {
                trail = gameObject.AddComponent<TrailRenderer>();
                ConfigureTrail();
            }
        }
        
        private void ConfigureTrail()
        {
            trail.time = 2f;
            trail.startWidth = 0.05f;
            trail.endWidth = 0.01f;
            trail.material = new Material(Shader.Find("Sprites/Default"));
            trail.startColor = new Color(1f, 1f, 1f, 0.5f);
            trail.endColor = new Color(1f, 1f, 1f, 0f);
            trail.enabled = false;
        }
        
        /// <summary>
        /// ボールを打つ
        /// </summary>
        public void Hit(Vector3 force, Vector3 hitPoint)
        {
            // トレイル有効化
            if (trail != null)
            {
                trail.Clear();
                trail.enabled = true;
            }
            
            // 力を加える
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.AddForce(force, ForceMode.Impulse);
            
            // スピン計算（打点によって変化）
            Vector3 localHitPoint = transform.InverseTransformPoint(hitPoint);
            Vector3 spin = CalculateSpin(localHitPoint, force);
            rb.AddTorque(spin, ForceMode.Impulse);
            
            isMoving = true;
            lastStablePosition = transform.position;
            
            StartCoroutine(TrackBallMovement());
        }
        
        /// <summary>
        /// スピン計算
        /// </summary>
        private Vector3 CalculateSpin(Vector3 localHitPoint, Vector3 force)
        {
            Vector3 spin = Vector3.zero;
            
            // バックスピン（上部を打った場合）
            if (localHitPoint.y > 0)
            {
                spin.x = -force.magnitude * 10f * localHitPoint.y;
            }
            // トップスピン（下部を打った場合）
            else if (localHitPoint.y < 0)
            {
                spin.x = force.magnitude * 10f * Mathf.Abs(localHitPoint.y);
            }
            
            // サイドスピン（左右を打った場合）
            spin.y = -force.magnitude * 5f * localHitPoint.x;
            
            return spin;
        }
        
        private void FixedUpdate()
        {
            if (!isMoving) return;
            
            // 空気抵抗
            ApplyAerodynamics();
            
            // 地面チェック
            CheckGrounded();
            
            // 転がり摩擦
            if (isGrounded)
            {
                ApplyRollingFriction();
            }
        }
        
        /// <summary>
        /// 空気力学的効果を適用
        /// </summary>
        private void ApplyAerodynamics()
        {
            Vector3 velocity = rb.linearVelocity;
            
            // 空気抵抗
            Vector3 dragForce = -0.5f * dragCoefficient * velocity.normalized * velocity.sqrMagnitude * 0.001f;
            rb.AddForce(dragForce);
            
            // マグヌス効果（回転による揚力）
            Vector3 angularVelocity = rb.angularVelocity;
            Vector3 magnusForce = magnusCoefficient * Vector3.Cross(angularVelocity, velocity) * 0.01f;
            rb.AddForce(magnusForce);
        }
        
        /// <summary>
        /// 転がり摩擦を適用
        /// </summary>
        private void ApplyRollingFriction()
        {
            Vector3 velocity = rb.linearVelocity;
            velocity.x *= (1f - rollingFriction * Time.fixedDeltaTime);
            velocity.z *= (1f - rollingFriction * Time.fixedDeltaTime);
            rb.linearVelocity = velocity;
        }
        
        /// <summary>
        /// 地面との接触チェック
        /// </summary>
        private void CheckGrounded()
        {
            RaycastHit hit;
            isGrounded = Physics.Raycast(transform.position, Vector3.down, out hit, groundCheckRadius);
            
            if (isGrounded && !wasGroundedLastFrame)
            {
                OnBallLanded?.Invoke(transform.position);
            }
            
            wasGroundedLastFrame = isGrounded;
        }
        
        private bool wasGroundedLastFrame = false;
        
        /// <summary>
        /// ボールの動きを追跡
        /// </summary>
        private IEnumerator TrackBallMovement()
        {
            while (isMoving)
            {
                // 速度チェック
                if (rb.linearVelocity.magnitude < minVelocity && isGrounded)
                {
                    // 停止処理
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    isMoving = false;
                    
                    if (trail != null)
                    {
                        trail.enabled = false;
                    }
                    
                    OnBallStopped?.Invoke(transform.position);
                    break;
                }
                
                yield return new WaitForSeconds(0.1f);
            }
        }
        
        /// <summary>
        /// ボールをリセット
        /// </summary>
        public void ResetBall(Vector3 position)
        {
            transform.position = position;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            isMoving = false;
            isGrounded = false;
            
            if (trail != null)
            {
                trail.Clear();
                trail.enabled = false;
            }
        }
        
        private void OnCollisionEnter(Collision collision)
        {
            // 衝突音などの処理
            float impactForce = collision.relativeVelocity.magnitude;
            if (impactForce > 1f)
            {
                // 音を再生
                Debug.Log($"Ball hit {collision.gameObject.name} with force {impactForce}");
            }
        }
        
        /// <summary>
        /// デバッグ表示
        /// </summary>
        private void OnDrawGizmos()
        {
            if (Application.isPlaying && isMoving)
            {
                // 速度ベクトル表示
                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(transform.position, GetComponent<Rigidbody>().linearVelocity * 0.1f);
                
                // 地面チェック
                Gizmos.color = isGrounded ? Color.green : Color.red;
                Gizmos.DrawWireSphere(transform.position, groundCheckRadius);
            }
        }
    }
}