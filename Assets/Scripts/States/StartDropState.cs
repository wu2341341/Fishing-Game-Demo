using UnityEngine;
using FishingGame.Controllers;

namespace FishingGame.States
{
    /// <summary>
    /// 开局下放状态：鱼钩从水面上方垂直落入水中
    /// </summary>
    public class StartDropState : IHookState
    {
        private float dropSpeed = 8f; // 下放速度
        private float startY = 7f;    // 初始高度

        // 水花特效预制体
        private GameObject splashEffectPrefab;

        public void Enter(HookController hook)
        {
            // 将鱼钩移动到水面上方
            hook.transform.position = new Vector3(0, startY, 0);
            hook.data.currentDepth = 0f;
            hook.CurrentVelocityY = 0f; // 告诉相机保持稳定

            // 解除限制
            hook.SetCameraLimitY(5f);
            hook.SetCameraMinYLimit(-999f);

            Debug.Log("开始下放鱼钩...");
        }

        public void Update(HookController hook)
        {
            // 垂直向下移动
            hook.transform.Translate(Vector3.down * dropSpeed * Time.deltaTime);

            // 当鱼钩到达水面时
            if (hook.transform.position.y <= 0f)
            {
                hook.transform.position = new Vector3(hook.transform.position.x, 0, 0);
                hook.data.currentDepth = 0f;

                // 播放入水音效
                if (AudioManager.Instance != null && hook.splashEffectPrefab != null)
                {
                    AudioManager.Instance.PlaySFX(hook.splashSound);
                    Object.Instantiate(hook.splashEffectPrefab, new Vector3(hook.transform.position.x, 0, 0), Quaternion.identity);
                }

                hook.ChangeState(new SinkingState());
                hook.ActivateInvincibility(3f);
            }
        }

        public void Exit(HookController hook)
        {
        }
    }
}