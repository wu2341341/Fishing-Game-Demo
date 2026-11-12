using UnityEngine;
using FishingGame.Controllers;

namespace FishingGame.States
{
    public class BufferState : IHookState
    {
        private float startY;         // 缓冲起始位置
        private float targetY;        // 缓冲目标位置
        private float bufferDuration; // 缓冲总耗时 (靠距离和速度估算)

        private float elapsedTime = 0f;

        /// <summary>
        /// 构造缓冲状态
        /// </summary>
        /// <param name="targetPositionY">目标Y轴位置（负数）</param>
        public BufferState(float targetPositionY)
        {
            targetY = targetPositionY;
        }

        public void Enter(HookController hook)
        {
            startY = hook.transform.position.y;
            elapsedTime = 0f;

            // 动态计算缓冲时间：保证不管多长距离，速度都保持在一个合理的区间
            // 距离越大，时间越长，防止瞬移
            float distance = Mathf.Abs(targetY - startY);
            bufferDuration = Mathf.Clamp(distance / hook.data.sinkSpeed, 0.8f, 3f);

            Debug.Log($"触发缓冲，目标 Y: {targetY}，耗时: {bufferDuration}秒");
        }

        public void Update(HookController hook)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / bufferDuration);
            hook.CurrentVelocityY = 0f; // 告诉相机在掉头

            // 使用 SmoothStep 产生 S 型缓动曲线：
            // 起步时减速，中间悬停/缓慢，快到终点时再加速（掉头上升）
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            // 直接控制位置移动
            float newY = Mathf.Lerp(startY, targetY, smoothT);
            hook.transform.position = new Vector3(hook.transform.position.x, newY, 0);
            hook.data.currentDepth = -hook.transform.position.y;

            if (t >= 1f)
            {
                hook.ChangeState(new RisingState());
            }
        }

        public void Exit(HookController hook) { }
    }
}