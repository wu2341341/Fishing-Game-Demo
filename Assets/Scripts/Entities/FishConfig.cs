using UnityEngine;

namespace FishingGame.Entities
{
    /// <summary>
    /// 鱼类配置数据 (ScriptableObject)
    /// 用于在 Editor 中创建具体的鱼种资产
    /// </summary>
    [CreateAssetMenu(fileName = "NewFishConfig", menuName = "FishingGame/FishConfig", order = 1)]
    public class FishConfig : ScriptableObject
    {
        public int fishID;
        public string fishName;
        public Sprite fishSprite;

        [Header("稀有度与收益")]
        public int rarity = 1;        // 星级
        public int coinValue = 10;    // 金币收益
        public int pearlValue = 10;   // 珍珠收益

        [Header("生成规则")]
        [Tooltip("这条鱼只在这个深度范围内生成 (填正数，比如 10 到 50)")]
        public float minDepth = 0f;
        public float maxDepth = 100f;

        [Tooltip("权重：数值越大，生成概率越高。")]
        public int spawnWeight = 10;
    }
}