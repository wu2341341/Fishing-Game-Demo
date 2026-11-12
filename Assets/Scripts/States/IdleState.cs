using UnityEngine;
using UnityEngine.EventSystems; // 用于检测是否点击了 UI
using FishingGame.Controllers;

namespace FishingGame.States
{
    /// <summary>
    /// 待机状态：等待玩家点击才开始下放鱼钩
    /// </summary>
    public class IdleState : IHookState
    {
        public void Enter(HookController hook)
        {
            // 确保鱼钩停在半空中（水面上方）
            hook.transform.position = new Vector3(0, 7f, 0);
            hook.data.currentDepth = 0f;
            hook.CurrentVelocityY = 0f; // 相机不偏移
            float camSize = Camera.main.orthographicSize;
            hook.SetCameraMinYLimit(camSize);
            hook.SnapCameraToTarget();

            Debug.Log("等待玩家开始游戏...");

            hook.SetPauseBtnVisibility(true);

            // 通知 UI 显示提示
            hook.TriggerStartTip(true);
        }

        public void Update(HookController hook)
        {
            // 等待玩家输入 (鼠标左键、触摸)
            bool isInputDetected = Input.GetMouseButtonDown(0);

            if (isInputDetected)
            {
                // 排除点击 UI 的情况（比如玩家点了一下暂停按钮）
                if (HookController.IsPointerOverUI()) return;

                // 玩家点击了，通知 UI 隐藏提示
                hook.TriggerStartTip(false);

                // 输入合法，切换到下放状态，正式开始游戏
                hook.ChangeState(new StartDropState());
            }
        }

        public void Exit(HookController hook)
        {
            // 隐藏提示 UI
            hook.TriggerStartTip(false);
        }
    }
}