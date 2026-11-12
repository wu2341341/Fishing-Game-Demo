using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FishingGame.Entities;

namespace FishingGame.Views
{
    public class FishResultItem : MonoBehaviour
    {
        [Header("UI 引用")]
        public Image fishIconImage;      // 鱼图
        public TextMeshProUGUI coinText;   // 单条金币
        public TextMeshProUGUI pearlText;  // 单条珍珠
        public TextMeshProUGUI countText;  // 抓取数量

        [Header("星级显示")]
        public Transform starContainer;    // 存放星星的父节点
        public GameObject starIconPrefab;  // 单颗星星的预制体

        /// <summary>
        /// 根据配置和数量初始化卡片
        /// </summary>
        public void Setup(FishConfig config, int count)
        {
            if (config == null) return;

            fishIconImage.sprite = config.fishSprite;
            coinText.text = config.coinValue.ToString();
            pearlText.text = config.pearlValue.ToString();
            countText.text = $"x{count}";

            // 动态生成星星
            UpdateStars(config.rarity);
        }

        private void UpdateStars(int rarity)
        {
            if (starContainer == null || starIconPrefab == null) return;

            // 1. 清理掉旧的星星（防止对象池复用时残留）
            foreach (Transform child in starContainer)
            {
                Destroy(child.gameObject);
            }

            // 2. 根据稀有度生成对应数量的星星
            for (int i = 0; i < rarity; i++)
            {
                Instantiate(starIconPrefab, starContainer);
            }
        }
    }
}