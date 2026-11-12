using UnityEngine;
using FishingGame.Controllers;
using FishingGame.States;

namespace FishingGame.Controllers
{
    public class CameraFollow : MonoBehaviour
    {
        [Header("跟随设置")]
        public Transform target;
        public float smoothSpeed = 5f;

        [Header("位置限制")]
        public float maxY = 5f;
        public float minY = -999f;

        [Header("速度前瞻设置 (Look-ahead)")]
        public float velocityToOffsetMultiplier = 2f;
        public float maxOffset = 15f;
        public float offsetSmoothTime = 0.4f;

        private float currentOffsetY = 0f;
        private float offsetVelocity = 0f;

        private HookController hookController;

        private void Start()
        {
            if (target == null)
            {
                Debug.LogError("相机没有绑定目标！");
                return;
            }

            // 获取引用
            hookController = target.GetComponent<HookController>();
        }

        /// <summary>
        /// 动态设置相机的最高点限制
        /// </summary>
        public void SetMaxYLimit(float newMaxY){ maxY = newMaxY; }

        /// <summary>
        /// 动态设置相机的最低点限制
        /// </summary>
        public void SetMinYLimit(float newMinY) { minY = newMinY; }

        public void SnapToTarget()
        {
            if (target == null) return;

            float targetY = target.position.y + currentOffsetY;
            targetY = Mathf.Min(targetY, maxY);
            targetY = Mathf.Max(targetY, minY);

            // 瞬间将相机 Y 轴移动到目标位置
            transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
        }

        private void LateUpdate()
        {
            if (target == null) return;

            // 读取状态机传来的逻辑速度
            float logicVelocityY = hookController != null ? hookController.CurrentVelocityY : 0f;

            // 根据速度计算目标偏移量
            float targetOffset = logicVelocityY * velocityToOffsetMultiplier;
            targetOffset = Mathf.Clamp(targetOffset, -maxOffset, maxOffset);

            // 平滑过渡偏移量
            currentOffsetY = Mathf.SmoothDamp(currentOffsetY, targetOffset, ref offsetVelocity, offsetSmoothTime);

            // 计算相机最终位置
            float targetY = target.position.y + currentOffsetY;
            targetY = Mathf.Min(targetY, maxY);
            targetY = Mathf.Max(targetY, minY);

            Vector3 desiredPosition = new Vector3(transform.position.x, targetY, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        }
    }
}