using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using FishingGame.Controllers;
using FishingGame.Models;

namespace FishingGame.Views
{
    public class UIManager : MonoBehaviour
    {
        [Header("深度 UI")]
        public RectTransform depthTrackRect;
        public RectTransform depthLineRect;
        public TextMeshProUGUI depthText;

        [Header("捕鱼数量 UI (跟随鱼钩)")]
        public TextMeshProUGUI fishCountText;
        public Image fishCountBg;

        [Header("颜色配置")]
        public Color safeColor = Color.green;
        public Color warningColor = Color.yellow;
        public Color dangerColor = Color.red;

        [Header("屏幕特效")]
        public Image screenFlashImage;

        [Header("开始提示 UI")]
        public StartTipUI startTipUI;

        private HookController hook;

        private void Start()
        {
            hook = FindObjectOfType<HookController>();
            if (hook != null)
            {
                hook.OnDataChanged += UpdateUI;
                hook.OnScreenFlashRequested += PlayScreenFlash;

                // 订阅开始提示事件
                hook.OnShowStartTip += HandleStartTip;

                // 游戏开局主动触发一次显示提示
                if (hook.currentState is FishingGame.States.IdleState)
                {
                    HandleStartTip(true);
                }

                UpdateUI(hook.data);
            }
        }

        private void OnDestroy()
        {
            if (hook != null)
            {
                hook.OnDataChanged -= UpdateUI;
                hook.OnScreenFlashRequested -= PlayScreenFlash;

                // 取消订阅
                hook.OnShowStartTip -= HandleStartTip;
            }
        }

        private void HandleStartTip(bool show)
        {
            if (startTipUI != null)
            {
                if (show) startTipUI.ShowTip();
                else startTipUI.HideTip();
            }
        }

        private void PlayScreenFlash()
        {
            if (screenFlashImage != null)
            {
                StartCoroutine(FlashRoutine());
            }
        }

        private IEnumerator FlashRoutine()
        {
            Color color = screenFlashImage.color;
            color.a = 0.8f;
            screenFlashImage.color = color;

            float duration = 0.3f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                color.a = Mathf.Lerp(0.8f, 0f, elapsed / duration);
                screenFlashImage.color = color;
                yield return null;
            }

            color.a = 0f;
            screenFlashImage.color = color;
        }

        private void UpdateUI(HookData data)
        {
            // 更新深度线
            if (depthLineRect != null && depthTrackRect != null)
            {
                float trackHeight = depthTrackRect.rect.height;
                if (trackHeight <= 0) trackHeight = depthTrackRect.sizeDelta.y;

                float halfHeight = trackHeight / 2f;
                float progress = data.currentDepth / data.maxDepth;

                // 计算线的目标 Y 坐标 (从 halfHeight 到 -halfHeight)
                float targetY = Mathf.Lerp(halfHeight, -halfHeight, progress);

                depthLineRect.anchoredPosition = new Vector2(depthLineRect.anchoredPosition.x, targetY);
            }

            // 更新深度数字
            if (depthText != null)
            {
                depthText.text = $"{Mathf.RoundToInt(data.currentDepth)}";
            }

            // 更新跟随计数器
            if (fishCountText != null)
            {
                fishCountText.text = $"{data.currentFishCount} / {data.maxFishCapacity}";
            }

            // 计数器背景变色
            if (fishCountBg != null)
            {
                float ratio = (float)data.currentFishCount / data.maxFishCapacity;
                if (ratio < 0.4f)
                    fishCountBg.color = safeColor;
                else if (ratio < 0.8f)
                    fishCountBg.color = warningColor;
                else
                    fishCountBg.color = dangerColor;
            }
        }
    }
}