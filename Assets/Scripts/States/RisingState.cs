using UnityEngine;
using FishingGame.Controllers;

namespace FishingGame.States
{
    public class RisingState : IHookState
    {
        private float currentSpeed;
        private float acceleration = 5f;

        public void Enter(HookController hook)
        {
            currentSpeed = 0f;
            hook.isForcedCentering = false;

            // 动态计算相机视野上限
            float camSize = Camera.main.orthographicSize;
            float dynamicLimit = -camSize;
            hook.SetCameraLimitY(dynamicLimit);
        }

        public void Update(HookController hook)
        {
            float targetSpeed = hook.data.riseSpeed;
            if (hook.data.currentFishCount >= hook.data.maxFishCapacity)
            {
                targetSpeed *= hook.data.speedUpMultiplier;
            }

            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.deltaTime);

            hook.CurrentVelocityY = currentSpeed;

            // 向上移动
            hook.transform.Translate(Vector3.up * currentSpeed * Time.deltaTime);
            hook.data.currentDepth = -hook.transform.position.y;

            if (hook.data.currentDepth <= 0)
            {
                hook.transform.position = new Vector3(0, 0, 0);
                hook.data.currentDepth = 0;

                // 出水瞬间，解除限制，让相机能看到水面和鱼竿
                hook.SetCameraLimitY(0f);


                // 播放入水音效和水花特效（出水时同样适用）
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(hook.splashSound);
                    if (hook.splashEffectPrefab != null)
                    {
                        Object.Instantiate(hook.splashEffectPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                    }
                }

                // 切换收杆
                hook.ChangeState(new ReelingState());
            }
        }

        public void Exit(HookController hook)
        {
            // 状态退出时恢复移动权限（为下一局准备）
            hook.isForcedCentering = false;
        }
    }
}