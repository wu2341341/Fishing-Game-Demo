using System.Collections.Generic;
using UnityEngine;
using FishingGame.Pool;
using FishingGame.Entities;
using FishingGame.Controllers;

namespace FishingGame.Entities
{
    public class FishSpawner : MonoBehaviour
    {
        [Header("引用")]
        public FishPool fishPool;
        public HookController hook;

        [Header("生成密度设置")]
        public float spawnIntervalMin = 0.5f;
        public float spawnIntervalMax = 1.5f;

        private float timer;
        private float nextSpawnTime;
        private Camera mainCamera;

        private void Start()
        {
            mainCamera = Camera.main;
            SetNextSpawnTime();
        }

        private void Update()
        {
            timer += Time.deltaTime;
            if (timer >= nextSpawnTime)
            {
                SpawnFish();
                SetNextSpawnTime();
            }
        }

        private void SetNextSpawnTime()
        {
            timer = 0f;
            // 密度在这里控制：时间间隔越小，鱼越密
            nextSpawnTime = Random.Range(spawnIntervalMin, spawnIntervalMax);
        }

        /// <summary>
        /// 根据当前深度和权重随机抽取一种鱼。
        /// 实现原理：累加所有合法鱼的总权重，生成一个 0~总权重的随机数，
        /// 然后遍历，当随机数小于当前累加权重时，即命中该鱼。
        /// </summary>
        private void SpawnFish()
        {
            if (fishPool == null || mainCamera == null) return;

            float camY = mainCamera.transform.position.y;
            float camHalfHeight = mainCamera.orthographicSize;

            float spawnY;

            // 决定是在镜头下方还是上方生成
            if (hook.currentState is States.RisingState || hook.transform.position.y > -5f)
                spawnY = camY + camHalfHeight + Random.Range(2f, 8f);
            else
                spawnY = camY - camHalfHeight - Random.Range(2f, 8f);

            // 边界拦截：超出水面或最大深度，且前 10 米不生成
            if (spawnY > -1f || spawnY < -hook.data.maxDepth || spawnY >= -10f)
            {
                return;
            }

            // 转换成正数的深度值 (比如 Y = -35.5，深度就是 35.5)
            float currentSpawnDepth = -spawnY;

            // 根据深度筛选出所有合法的鱼，并计算总权重
            List<FishConfig> validConfigs = new List<FishConfig>();
            int totalWeight = 0;

            if (fishPool.fishConfigs != null)
            {
                foreach (var config in fishPool.fishConfigs)
                {
                    if (currentSpawnDepth >= config.minDepth && currentSpawnDepth <= config.maxDepth)
                    {
                        validConfigs.Add(config);
                        totalWeight += config.spawnWeight;
                    }
                }
            }

            // 如果当前深度没有任何配置的鱼，直接放弃本次生成
            if (validConfigs.Count == 0 || totalWeight == 0) return;

            // 权重抽奖算法
            int randomValue = Random.Range(0, totalWeight);
            int currentWeight = 0;
            FishConfig selectedConfig = validConfigs[0];

            foreach (var config in validConfigs)
            {
                currentWeight += config.spawnWeight;
                if (randomValue < currentWeight)
                {
                    selectedConfig = config;
                    break;
                }
            }

            // 生成选中的鱼
            GameObject fishObj = fishPool.GetFish(selectedConfig);
            float spawnX = Random.Range(-8f, 8f);
            fishObj.transform.position = new Vector3(spawnX, spawnY, 0);

            Fish fishComp = fishObj.GetComponent<Fish>();
            fishComp.moveSpeed = Random.Range(1.5f, 3.5f); // 也可以把速度配置到 FishConfig 里
        }
    }
}