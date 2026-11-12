using System;
using UnityEngine;

namespace FishingGame.Models
{
    /// <summary>
    /// 鱼钩的核心数据模型 (Model)
    /// 负责存储游戏的数值，不包含任何表现和逻辑代码
    /// </summary>
    [Serializable]
    public class HookData
    {
        [Header("基础配置")]
        public float maxDepth = 100f;          // 最大深度
        public float sinkSpeed = 10f;           // 基础下沉速度
        public float riseSpeed = 10f;           // 基础上升速度
        public float speedUpMultiplier = 2f; // 满载时的上升速度倍率

        [Header("鱼与保护机制")]
        public int maxFishCapacity = 10;       // 最大捕鱼数量
        public int maxProtectionCount = 2;     // 初始保护次数

        [Header("当前运行状态 (运行时数据)")]
        public float currentDepth = 0f;        // 当前深度
        public int currentFishCount = 0;       // 当前抓到的鱼数量
        public int currentProtectionCount;     // 剩余保护次数
        public bool hasTriggeredBuffer = false; // 本局是否已经触发过缓冲减速

        /// <summary>
        /// 重置数据，用于游戏重新开始
        /// </summary>
        public void ResetData()
        {
            currentDepth = 0f;
            currentFishCount = 0;
            currentProtectionCount = maxProtectionCount;
            hasTriggeredBuffer = false;
        }
    }
}