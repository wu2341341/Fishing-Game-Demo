using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FishingGame.Controllers;
using FishingGame.Entities;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using System;

namespace FishingGame.Views
{
    public class ResultPanelManager : MonoBehaviour
    {
        [Header("UI 引用")]
        public Transform gridArea;
        public GameObject fishItemPrefab;
        public TextMeshProUGUI depthText;

        [Header("收益与按钮")]
        public TextMeshProUGUI totalCoinText;
        public TextMeshProUGUI totalPearlText;
        public Button exitButton;
        public Button restartButton;

        private HookController hook;

        private void Start()
        {
            hook = FindObjectOfType<HookController>();
            if (hook != null)
            {
                hook.OnGameEnded += ShowPanel;
            }

            // 绑定按钮事件
            restartButton.onClick.AddListener(OnRestartClicked);
            exitButton.onClick.AddListener(OnExitClicked);

            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (hook != null) hook.OnGameEnded -= ShowPanel;
        }

        private void ShowPanel(float maxDepth, Dictionary<FishConfig, int> caughtStats)
        {
            gameObject.SetActive(true);

            depthText.text = $"下潜深度：{Mathf.RoundToInt(maxDepth)}米";

            // 清理旧卡片
            foreach (Transform child in gridArea)
            {
                Destroy(child.gameObject);
            }

            // 面板弹出动画
            // 将面板初始缩放设为 0
            transform.localScale = Vector3.zero;
            // 用 Dotween 在 0.4 秒内弹到 1，OutBack 曲线会产生回弹效果
            transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack).SetUpdate(true);

            int totalCoins = 0;
            int totalPearls = 0;

            // 将字典转换为 List，然后按星级降序 (OrderByDescending)，星级相同按 ID 升序 (ThenBy)
            var sortedStats = caughtStats.ToList()
                .OrderByDescending(kvp => kvp.Key.rarity)
                .ThenBy(kvp => kvp.Key.fishID)
                .ToList();

            int index = 0; // 用于控制卡片的延迟时间

            // 遍历排序后的列表
            foreach (var kvp in sortedStats)
            {
                FishConfig config = kvp.Key;
                int count = kvp.Value;

                // 生成卡片
                GameObject itemObj = Instantiate(fishItemPrefab, gridArea);
                itemObj.GetComponent<FishResultItem>().Setup(config, count);

                // 卡片依次飞入动画
                // 卡片初始大小为 0
                itemObj.transform.localScale = Vector3.zero;

                // 延迟 0.1 秒后，每张卡片错开 0.05 秒弹出
                float delay = 0.1f + (index * 0.05f);
                itemObj.transform.DOScale(Vector3.one, 0.3f)
                    .SetDelay(delay)
                    .SetEase(Ease.OutBack);

                // 累加收益
                totalCoins += config.coinValue * count;
                totalPearls += config.pearlValue * count;
            }

            // 更新底部的总计数字
            totalCoinText.text = totalCoins.ToString();
            totalPearlText.text = totalPearls.ToString();

            index++;
        }

        private void OnRestartClicked()
        {
            transform.localScale = Vector3.one; // 恢复原本大小
            gameObject.SetActive(false);
            hook.RestartGame();
        }

        /// <summary>
        /// 点击退出按钮的逻辑
        /// </summary>
        private void OnExitClicked()
        {
            Debug.Log("玩家点击了退出游戏");

            // 在 Unity 编辑器中，Application.Quit() 不会生效
            // 所以这里加上宏定义，方便测试
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}