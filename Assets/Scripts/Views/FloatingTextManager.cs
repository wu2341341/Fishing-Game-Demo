using UnityEngine;
using TMPro;
using DG.Tweening;
using FishingGame.Controllers;

namespace FishingGame.Views
{
    public class FloatingTextManager : MonoBehaviour
    {
        public GameObject coinFlyPrefab;
        public RectTransform floatingTextLayer;
        public Camera uiCamera;

        [Header("音效")]
        public AudioClip coinSound; //金币飘字的音效

        /// <summary>
        /// 在指定的世界坐标位置生成金币飘字
        /// </summary>
        public void PlayCoinFlyAtPosition(int value, Vector3 worldPosition)
        {
            if (coinFlyPrefab == null || floatingTextLayer == null) return;

            // 实例化 UI
            GameObject coinObj = Instantiate(coinFlyPrefab, floatingTextLayer);

            // 设置文本
            coinObj.GetComponentInChildren<TextMeshProUGUI>().text = $"+{value}";

            // 世界坐标转 UI 坐标
            RectTransform rect = coinObj.GetComponent<RectTransform>();
            Vector2 screenPoint = Camera.main.WorldToScreenPoint(worldPosition);
            Vector2 localPoint;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                floatingTextLayer,
                screenPoint,
                uiCamera,
                out localPoint
            );

            rect.anchoredPosition = localPoint;

            // 获取 CanvasGroup 用于淡出
            CanvasGroup cg = coinObj.GetComponent<CanvasGroup>();
            if (cg == null) cg = coinObj.AddComponent<CanvasGroup>();
            cg.alpha = 1f;

            // 播放金币音效
            if (AudioManager.Instance != null && coinSound != null)
            {
                // 稍微降低一点音量 (0.6f)，防止抓10条鱼时音效重叠导致刺耳
                AudioManager.Instance.PlaySFX(coinSound, 0.6f);
            }

            // Dotween 动画序列：向上飘 + 淡出 + 销毁
            Sequence seq = DOTween.Sequence();

            float targetX = localPoint.x + Random.Range(-50f, 50f);
            float targetY = localPoint.y + Random.Range(150f, 250f);

            seq.Append(rect.DOAnchorPos(new Vector2(targetX, targetY), 1.2f).SetEase(Ease.OutQuad));
            seq.Join(cg.DOFade(0f, 0.4f).SetDelay(0.8f));

            seq.OnComplete(() => Destroy(coinObj));
        }
    }
}