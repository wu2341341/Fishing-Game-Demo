using UnityEngine;
using FishingGame.Controllers;

namespace FishingGame.States
{
    public class SinkingState : IHookState
    {
        public void Enter(HookController hook)
        {
            // 下降时，解除限制，看到水面
            hook.SetCameraLimitY(5f);
            hook.SetCameraMinYLimit(-999f);
        }

        public void Update(HookController hook)
        {
            hook.CurrentVelocityY = -hook.data.sinkSpeed; // 告诉相机在向下
            hook.transform.Translate(Vector3.down * hook.data.sinkSpeed * Time.deltaTime);
            hook.data.currentDepth = -hook.transform.position.y;

            // 到达最大深度缓冲区，传入底部坐标作为目标
            if (hook.data.currentDepth + 10 >= hook.data.maxDepth) 
            {
                hook.TryTriggerBuffer(-hook.data.maxDepth);
            }
        }

        public void Exit(HookController hook) { }
    }
}