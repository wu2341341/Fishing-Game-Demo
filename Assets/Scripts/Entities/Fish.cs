using UnityEngine;
using System;

namespace FishingGame.Entities
{
    [RequireComponent(typeof(CircleCollider2D))]
    public class Fish : MonoBehaviour
    {
        [Header("游动设置")]
        public float moveSpeed = 2f;
        public float minX = -9f;
        public float maxX = 9f;
        private bool moveRight = false;

        private bool isCaught = false;
        private SpriteRenderer sr;
        private CircleCollider2D col;
        private Camera mainCamera;
        public bool IsMovingRight => moveRight;
        public bool IsCaught => isCaught;

        private Action<Fish> onRecycleAction;

        [Header("上钩挣扎表现设置")]
        public float baseRotationZ = 0f;      // 上钩时的基础角度
        public float struggleAmplitude = 10f; // 挣扎摆动幅度 (度)
        public float struggleFrequency = 8f;  // 挣扎摆动频率
        public float swingSmoothness = 5f;    // 跟随鱼钩摆动的平滑度

        private Transform hookTransform;
        private float lastHookX;
        private Vector3 originalScale;//记录预制体最原始的缩放值
        public FishConfig config { get; private set; }

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            col = GetComponent<CircleCollider2D>();
            mainCamera = Camera.main;
            originalScale = transform.localScale;
        }

        public void Init(float min, float max, bool startRight, Action<Fish> recycleAction)
        {
            minX = min;
            maxX = max;
            moveRight = startRight;
            onRecycleAction = recycleAction;

            isCaught = false;
            transform.rotation = Quaternion.identity; // 重置旋转
            UpdateSpriteDirection();
        }

        public void Setup(FishConfig newConfig)
        {
            config = newConfig;
            if (config == null) return;

            if (sr == null) sr = GetComponent<SpriteRenderer>();
            if (col == null) col = GetComponent<CircleCollider2D>();

            sr.sprite = config.fishSprite;

            if (config.fishSprite != null && col != null)
            {
                Vector2 spriteSize = config.fishSprite.bounds.size;
                col.radius = (Mathf.Max(spriteSize.x, spriteSize.y) / 2f) * 0.8f;
            }
        }

        private void Update()
        {
            // 如果游戏暂停，直接跳过所有逻辑，防止计算出 NaN
            if (Time.timeScale == 0f) return;

            if (isCaught) { HandleCaughtBehavior(); return; }

            // 左右游动
            float dir = moveRight ? 1f : -1f;
            transform.Translate(Vector3.right * dir * moveSpeed * Time.deltaTime);

            float fishWidth = sr.bounds.size.x;

            // 边界检测与翻转补偿
            if (transform.position.x >= maxX && moveRight)
            {
                moveRight = false;

                // 补偿：向右游时头在右，翻转向左时，头要退到左边
                transform.position -= new Vector3(fishWidth * 0.8f, 0, 0);

                UpdateSpriteDirection();
            }
            else if (transform.position.x <= minX && !moveRight)
            {
                moveRight = true;

                // 补偿：向左游时头在左，翻转向右时，头要退到右边
                transform.position += new Vector3(fishWidth * 0.8f, 0, 0);

                UpdateSpriteDirection();
            }

            CheckOutOfViewAndRecycle();
        }

        /// <summary>
        /// 上钩后的表现逻辑：惯性摆动 + 挣扎 (带角度限制)
        /// </summary>
        private void HandleCaughtBehavior()
        {
            if (hookTransform == null) return;

            // 如果 deltaTime 为 0，说明游戏暂停了，不计算速度
            if (Time.deltaTime == 0f)
            {
                return;
            }

            // 计算鱼钩当前在水平方向上的移动速度
            float hookVelocityX = (hookTransform.position.x - lastHookX) / Time.deltaTime;
            lastHookX = hookTransform.position.x;

            // 计算惯性倾斜角度
            float rawSwingAngle = -hookVelocityX * 2f;

            // 核心修改：限制惯性引起的最大偏转角
            float clampedSwingAngle = Mathf.Clamp(rawSwingAngle, -35f, 35f);

            // 计算挣扎的正弦波角度
            float struggleAngle = Mathf.Sin(Time.time * struggleFrequency) * struggleAmplitude;

            // 合成最终角度：基础角度 + 限制后的惯性角度 + 挣扎角度
            float finalAngle = baseRotationZ + clampedSwingAngle + struggleAngle;

            // 第二重保险：限制最终角度
            finalAngle = Mathf.Clamp(finalAngle, -135f, 135f);

            // 平滑插值旋转，避免生硬
            Quaternion targetRotation = Quaternion.Euler(0, 0, finalAngle);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * swingSmoothness);
        }

        private void CheckOutOfViewAndRecycle()
        {
            if (mainCamera == null) return;

            float camY = mainCamera.transform.position.y;
            float camHalfHeight = mainCamera.orthographicSize;

            if (transform.position.y > camY + camHalfHeight + 15f ||
                transform.position.y < camY - camHalfHeight - 15f)
            {
                if (!isCaught)
                {
                    onRecycleAction?.Invoke(this);
                }
            }
        }

        private void UpdateSpriteDirection()
        {
            if (sr == null) sr = GetComponent<SpriteRenderer>();
            sr.flipX = !moveRight;
        }

        // 上钩时调用
        public void OnCaught(float initialAngle, Transform hook)
        {
            isCaught = true;
            // 被抓后立刻关闭碰撞体，防止反复触发碰撞
            if (col != null) col.enabled = false;

            baseRotationZ = initialAngle;
            hookTransform = hook;
            lastHookX = hookTransform.position.x;
        }

        public void ResetFish()
        {
            isCaught = false;
            // 回收后重新开启碰撞体
            if (col != null) col.enabled = true;
            // 重置旋转
            transform.localRotation = Quaternion.identity;
            // 恢复原本的尺寸
            transform.localScale = originalScale;
        }
    }
}