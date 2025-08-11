using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ClubNeko.Character
{
    /// <summary>
    /// キャラクターAI - VRMキャラクターの基本的な動作を制御
    /// </summary>
    public class CharacterAI : MonoBehaviour
    {
        [Header("Wander Settings")]
        public float wanderRadius = 5f;
        public float wanderSpeed = 2f;
        public float waitTimeMin = 2f;
        public float waitTimeMax = 5f;
        
        [Header("Animation")]
        public string[] idleAnimations = { "Idle", "Wave", "Dance" };
        public float animationChangeInterval = 10f;
        
        [Header("Interaction")]
        public float interactionRadius = 3f;
        public LayerMask playerLayer;
        public bool canInteract = true;
        
        private Vector3 homePosition;
        private Vector3 targetPosition;
        private Animator animator;
        private float nextAnimationTime;
        private bool isMoving = false;
        private Coroutine wanderCoroutine;
        
        private void Start()
        {
            homePosition = transform.position;
            targetPosition = homePosition;
            animator = GetComponent<Animator>();
            
            if (animator == null)
            {
                // アニメーターがない場合は簡単なアニメーションコンポーネントを追加
                gameObject.AddComponent<SimpleCharacterAnimation>();
            }
            
            StartWandering();
        }
        
        public void StartWandering()
        {
            if (wanderCoroutine != null)
            {
                StopCoroutine(wanderCoroutine);
            }
            wanderCoroutine = StartCoroutine(WanderRoutine());
        }
        
        public void StopWandering()
        {
            if (wanderCoroutine != null)
            {
                StopCoroutine(wanderCoroutine);
                wanderCoroutine = null;
            }
            isMoving = false;
        }
        
        private IEnumerator WanderRoutine()
        {
            while (true)
            {
                // 新しい目標地点を設定
                targetPosition = GetRandomWanderPosition();
                isMoving = true;
                
                // 目標地点まで移動
                while (Vector3.Distance(transform.position, targetPosition) > 0.5f)
                {
                    Vector3 direction = (targetPosition - transform.position).normalized;
                    transform.position += direction * wanderSpeed * Time.deltaTime;
                    
                    // キャラクターの向きを調整
                    if (direction != Vector3.zero)
                    {
                        Quaternion targetRotation = Quaternion.LookRotation(direction);
                        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
                    }
                    
                    yield return null;
                }
                
                isMoving = false;
                
                // 待機
                float waitTime = Random.Range(waitTimeMin, waitTimeMax);
                yield return new WaitForSeconds(waitTime);
                
                // ランダムなアイドルアニメーション
                if (Random.Range(0f, 1f) > 0.5f)
                {
                    PlayRandomIdleAnimation();
                    yield return new WaitForSeconds(2f);
                }
            }
        }
        
        private Vector3 GetRandomWanderPosition()
        {
            Vector2 randomCircle = Random.insideUnitCircle * wanderRadius;
            Vector3 randomPosition = homePosition + new Vector3(randomCircle.x, 0, randomCircle.y);
            
            // 地面の高さに合わせる
            RaycastHit hit;
            if (Physics.Raycast(randomPosition + Vector3.up * 10f, Vector3.down, out hit, 20f))
            {
                randomPosition.y = hit.point.y;
            }
            
            return randomPosition;
        }
        
        private void PlayRandomIdleAnimation()
        {
            if (idleAnimations == null || idleAnimations.Length == 0) return;
            
            string animationName = idleAnimations[Random.Range(0, idleAnimations.Length)];
            
            if (animator != null)
            {
                animator.SetTrigger(animationName);
            }
        }
        
        private void Update()
        {
            // プレイヤーとのインタラクションチェック
            if (canInteract)
            {
                CheckPlayerInteraction();
            }
            
            // 移動アニメーション
            if (animator != null)
            {
                animator.SetBool("IsMoving", isMoving);
                animator.SetFloat("MoveSpeed", isMoving ? wanderSpeed : 0f);
            }
        }
        
        private void CheckPlayerInteraction()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, interactionRadius, playerLayer);
            
            if (colliders.Length > 0)
            {
                // プレイヤーが近くにいる場合
                Transform player = colliders[0].transform;
                
                // プレイヤーの方を向く
                Vector3 lookDirection = (player.position - transform.position).normalized;
                lookDirection.y = 0;
                
                if (lookDirection != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 3f);
                }
                
                // 手を振るアニメーション
                if (Time.time > nextAnimationTime)
                {
                    if (animator != null)
                    {
                        animator.SetTrigger("Wave");
                    }
                    nextAnimationTime = Time.time + animationChangeInterval;
                }
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            // Wanderエリアの表示
            Gizmos.color = Color.yellow;
            Vector3 center = Application.isPlaying ? homePosition : transform.position;
            Gizmos.DrawWireSphere(center, wanderRadius);
            
            // インタラクションエリアの表示
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, interactionRadius);
        }
    }
    
    /// <summary>
    /// シンプルなキャラクターアニメーション（アニメーターがない場合）
    /// </summary>
    public class SimpleCharacterAnimation : MonoBehaviour
    {
        private float bobSpeed = 2f;
        private float bobAmount = 0.1f;
        private float swaySpeed = 1f;
        private float swayAmount = 5f;
        private Vector3 originalPosition;
        
        private void Start()
        {
            originalPosition = transform.localPosition;
        }
        
        private void Update()
        {
            // 上下の揺れ
            float newY = originalPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobAmount;
            
            // 左右の揺れ
            float swayAngle = Mathf.Sin(Time.time * swaySpeed) * swayAmount;
            
            transform.localPosition = new Vector3(
                originalPosition.x,
                newY,
                originalPosition.z
            );
            
            transform.localRotation = Quaternion.Euler(0, swayAngle, 0);
        }
    }
}