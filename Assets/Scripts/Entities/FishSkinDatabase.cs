using UnityEngine;

namespace FishingGame.Entities
{
    public class FishSkinDatabase : MonoBehaviour
    {
        [Header("放入所有的鱼图片")]
        public Sprite[] fishSkins;

        /// <summary>
        /// 随机获取一张鱼的图片
        /// </summary>
        public Sprite GetRandomSkin()
        {
            if (fishSkins == null || fishSkins.Length == 0) return null;
            int randomIndex = Random.Range(0, fishSkins.Length);
            return fishSkins[randomIndex];
        }
    }
}