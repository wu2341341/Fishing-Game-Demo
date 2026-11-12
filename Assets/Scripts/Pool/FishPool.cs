using System.Collections.Generic;
using UnityEngine;
using FishingGame.Entities;

namespace FishingGame.Pool
{
    public class FishPool : MonoBehaviour
    {
        public GameObject fishPrefab;
        public int initialPoolSize = 20;

        [Header("鱼的配置库")]
        public FishConfig[] fishConfigs;

        [Header("边界设置")]
        public float screenMinX = -9f;
        public float screenMaxX = 9f;

        private Queue<GameObject> poolQueue = new Queue<GameObject>();
        private List<GameObject> activeFishes = new List<GameObject>();

        private void Awake()
        {
            InitializePool();
        }

        private void InitializePool()
        {
            for (int i = 0; i < initialPoolSize; i++)
            {
                GameObject fish = Instantiate(fishPrefab, transform);
                fish.SetActive(false);
                poolQueue.Enqueue(fish);
            }
        }

        public GameObject GetFish(FishConfig targetConfig)
        {
            GameObject fish;
            if (poolQueue.Count > 0) fish = poolQueue.Dequeue();
            else fish = Instantiate(fishPrefab, transform);

            Fish fishComp = fish.GetComponent<Fish>();

            if (targetConfig != null)
            {
                fishComp.Setup(targetConfig);
            }

            bool startRight = Random.value > 0.5f;
            fishComp.Init(screenMinX, screenMaxX, startRight, RecycleFish);

            fish.SetActive(true);
            activeFishes.Add(fish);
            return fish;
        }

        public void RecycleFish(Fish fishComp)
        {
            GameObject fish = fishComp.gameObject;
            if (activeFishes.Contains(fish))
            {
                fishComp.ResetFish();
                activeFishes.Remove(fish);
                fish.SetActive(false);
                fish.transform.SetParent(this.transform);
                poolQueue.Enqueue(fish);
            }
        }

        /// <summary>
        /// 强制回收当前场景里所有存活的鱼（用于游戏结束清场）
        /// </summary>
        public void RecycleAllActiveFishes()
        {
            // 倒序遍历，因为回收时会从 List 中移除，正序遍历会报错
            for (int i = activeFishes.Count - 1; i >= 0; i--)
            {
                GameObject fishObj = activeFishes[i];
                if (fishObj != null)
                {
                    Fish fishComp = fishObj.GetComponent<Fish>();
                    if (fishComp != null)
                    {
                        RecycleFish(fishComp);
                    }
                }
            }
        }
    }
}