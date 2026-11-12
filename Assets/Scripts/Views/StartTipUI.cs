using UnityEngine;
using DG.Tweening;
using TMPro;

namespace FishingGame.Views
{
    public class StartTipUI : MonoBehaviour
    {
        private TextMeshProUGUI tipText;

        private void Awake()
        {
            tipText = GetComponent<TextMeshProUGUI>();
        }

        /// <summary>
        /// 显示提示并播放呼吸动画
        /// </summary>
        public void ShowTip()
        {
            gameObject.SetActive(true);

            // 透明度呼吸灯效果
            tipText.color = new Color(tipText.color.r, tipText.color.g, tipText.color.b, 0f);
            tipText.DOFade(1f, 1f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        }

        /// <summary>
        /// 隐藏提示并清理动画
        /// </summary>
        public void HideTip()
        {
            // 停止所有该物体上的 Dotween 动画，防止残留
            tipText.DOKill();
            transform.DOKill();

            gameObject.SetActive(false);
        }
    }
}