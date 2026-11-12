using UnityEngine;
using DG.Tweening;
using FishingGame.Controllers;
using FishingGame.Pool;

namespace FishingGame.States
{
    public class ReelingState : IHookState
    {
        public void Enter(HookController hook)
        {
            hook.isForcedCentering = true; // 锁定输入
            float camSize = Camera.main.orthographicSize;
            hook.SetCameraMinYLimit(camSize);

            hook.SetPauseBtnVisibility(false);

            // 直接读取挂在 HookController 上的引用
            Vector3 basePos = hook.reelTarget != null
                ? hook.reelTarget.position
                : new Vector3(0, 5, 0); // 兜底位置

            Vector3 targetPos = basePos + new Vector3(0, hook.reelTargetOffsetY, 0);

            // 鱼钩飞向目标位置
            hook.transform.DOMove(targetPos, 0.4f)
                .SetEase(Ease.OutBack) // 带一点轻微的回弹
                .OnComplete(() =>
                {
                    // 到达鱼竿后的回调
                    OnReelComplete(hook);
                });
        }

        private void OnReelComplete(HookController hook)
        {
            // 获取飘字管理器
            var floatManager = hook.floatManager;
            FishPool pool = hook.fishPool;

            // 鱼向上飞散并缩小
            foreach (var fish in hook.GetCaughtFishes())
            {
                if (fish == null) continue;

                fish.transform.SetParent(null); // 脱离鱼钩

                // 记录这条鱼消失后应该给的金币
                int coinValue = fish.config != null ? fish.config.coinValue : 10;

                // 随机向上飞散位置
                Vector3 randomTarget = fish.transform.position + new Vector3(Random.Range(-2f, 2f), Random.Range(3f, 5f), 0);

                // 飞散+缩小
                fish.transform.DOMove(randomTarget, 0.6f).SetEase(Ease.OutQuad).SetUpdate(true);

                // 缩小的动画，在完成时触发金币飘字
                fish.transform.DOScale(Vector3.zero, 0.4f).SetDelay(0.2f).SetUpdate(true).OnComplete(() =>
                {
                    // 记录鱼消失瞬间的世界坐标
                    Vector3 disappearPos = fish.transform.position;

                    // 在鱼消失的位置生成金币飘字
                    if (floatManager != null)
                    {
                        floatManager.PlayCoinFlyAtPosition(coinValue, disappearPos);
                    }
                });
            }

            // 延迟一会，弹出结算面板
            DOVirtual.DelayedCall(2.0f, () =>
            {
                hook.RecycleAllCaughtFishes(); // 统一还池子
                hook.TriggerGameEnd();
            });
        }

        public void Update(HookController hook) { }
        public void Exit(HookController hook) { }
    }
}