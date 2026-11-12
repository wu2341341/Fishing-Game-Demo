using UnityEngine;
using FishingGame.Models;
using FishingGame.States;
using FishingGame.Entities;
using System.Collections.Generic;
using System;
using FishingGame.Pool;
using System.Collections;
using UnityEngine.EventSystems;
using FishingGame.Views;

namespace FishingGame.Controllers
{
    public class HookController : MonoBehaviour
    {
        //定义数据更新事件
        public event Action<HookData> OnDataChanged;
        // 屏幕闪白事件
        public event Action OnScreenFlashRequested;

        public event Action<float, Dictionary<FishConfig, int>> OnGameEnded;

        [Header("输入控制")]
        public InputMode inputMode = InputMode.Touch; // 默认触屏/鼠标
        public float horizontalSpeed = 10f;            // 固定移动速度
        public float gravitySensitivity = 15f;        // 重力感应的灵敏度
        public float screenMinX = -8.5f;                // 左边界
        public float screenMaxX = 8.5f;                 // 右边界

        [Header("泡泡保护特效")]
        public GameObject bubbleEffect;

        [Header("数据模型")]
        public HookData data = new HookData();

        [Header("状态与运行数据")]
        public IHookState currentState; // public 方便 InvincibleState 访问
        public bool isInvincible = false; // 无敌标志

        [Header("目标点")]
        public Transform reelTarget; // 用于收杆时的目标位置
        public float reelTargetOffsetY = -1.5f; // 收杆目标点的 Y 轴偏移量（负数代表向下）

        [Header("相机控制")]
        public CameraFollow cameraFollow;

        [Header("音效与特效")]
        public AudioClip catchFishSound; // 抓鱼声
        public GameObject splashEffectPrefab; // 水花特效预制体

        [Header("引用")]
        public FishPool fishPool;
        public FloatingTextManager floatManager;

        private List<Fish> caughtFishes = new List<Fish>(); // 抓到的鱼集合

        //开始提示 UI 的显隐事件
        public event Action<bool> OnShowStartTip;

        // 暂停按钮显隐事件
        public event Action<bool> OnPauseBtnVisibilityChanged;

        // 记录本局战利品：Key 是鱼的数据，Value 是抓到数量
        public Dictionary<FishConfig, int> CaughtStats { get; private set; } = new Dictionary<FishConfig, int>();

        // 记录下潜的最大深度
        public float MaxDepthReached { get; private set; } = 0f;

        // 记录当前的逻辑速度（下降为负，上升为正）
        public float CurrentVelocityY { get; set; } = 0f;

        // 是否强制锁定X轴（用于开局和结束）
        public bool isForcedCentering = false;

        public float forceCenterDepth = 10f; // 强制居中深度
        public AudioClip splashSound;
        private Coroutine flashCoroutine;
        private SpriteRenderer hookSprite;
        private int lastDepthInt = -1;
        private int lastFishCount = -1;

        public enum InputMode { Keyboard, Touch, Gravity }

        private void Awake()
        {
            hookSprite = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            data.ResetData();
            CaughtStats.Clear(); // 每次开局清空战利品字典
            MaxDepthReached = 0f; // 重置最大深度
            // 开局先进入待机状态
            ChangeState(new IdleState());
        }

        private void Update()
        {
            currentState?.Update(this);

            // 统一处理强制居中逻辑
            HandleForceCentering();

            HandleInput();

            // 记录本局到达的最大深度
            if (data.currentDepth > MaxDepthReached)
            {
                MaxDepthReached = data.currentDepth;
            }

            int currentDepthInt = Mathf.RoundToInt(data.currentDepth);
            if (currentDepthInt != lastDepthInt || data.currentFishCount != lastFishCount)
            {
                lastDepthInt = currentDepthInt;
                lastFishCount = data.currentFishCount;
                OnDataChanged?.Invoke(data); // 只有数值变了才广播
            }

            // 泡泡特效的显隐逻辑：只要不在上升/缓冲状态，且还有保护次数，就显示泡泡
            if (bubbleEffect != null)
            {
                bool shouldShowBubble = data.currentProtectionCount > 0
                                        && !(currentState is RisingState)
                                        && !(currentState is BufferState);
                bubbleEffect.SetActive(shouldShowBubble);
            }
        }

        public void ChangeState(IHookState newState)
        {
            currentState?.Exit(this);
            currentState = newState;
            currentState?.Enter(this);
        }

        private void HandleForceCentering()
        {
            // 如果深度 <= 10，强制拉回中心
            if (data.currentDepth <= forceCenterDepth)
            {
                isForcedCentering = true;
                float newX = Mathf.MoveTowards(transform.position.x, 0f, 10f * Time.deltaTime);
                transform.position = new Vector3(newX, transform.position.y, 0);
            }
            else
            {
                isForcedCentering = false;
            }
        }

        private void HandleInput()
        {
            // 如果正在强制居中，忽略所有输入
            if (isForcedCentering) return;

            // 如果游戏处于暂停状态，忽略输入
            if (Time.timeScale == 0f) return;

            float currentX = transform.position.x;
            float targetX = currentX;

            switch (inputMode)
            {
                case InputMode.Touch:
                    // 检查是否点在了 UI 上（防止点暂停按钮时拖动鱼钩）
                    if (Input.GetMouseButton(0) && !IsPointerOverUI())
                    {
                        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                        targetX = mousePos.x;
                    }
                    break;

                case InputMode.Keyboard:
                    // 获取键盘 A/D 或 左右方向键 的输入 (-1 到 1)
                    float moveDir = Input.GetAxisRaw("Horizontal");
                    if (moveDir != 0)
                    {
                        targetX = currentX + moveDir * horizontalSpeed * Time.deltaTime * 2f; // 乘以2是为了手感稍微快一点
                    }
                    break;

                case InputMode.Gravity:
                    // 根据手机倾斜角度移动 (x轴)
                    float tilt = Input.acceleration.x;
                    // 如果倾斜幅度超过一个死区，才移动
                    if (Mathf.Abs(tilt) > 0.05f)
                    {
                        targetX = currentX + tilt * gravitySensitivity * Time.deltaTime;
                    }
                    break;
            }

            // 限制边界
            targetX = Mathf.Clamp(targetX, screenMinX, screenMaxX);

            // 使用固定速度向目标 X 坐标移动 (MoveTowards)
            float newX = Mathf.MoveTowards(currentX, targetX, horizontalSpeed * Time.deltaTime);

            // 更新位置
            transform.position = new Vector3(newX, transform.position.y, 0);
        }

        /// <summary>
        /// 尝试触发缓冲减速
        /// </summary>
        /// <param name="targetDepth">缓冲到达的目标深度（负数）</param>
        public void TryTriggerBuffer(float targetDepth)
        {
            if (!data.hasTriggeredBuffer)
            {
                data.hasTriggeredBuffer = true;
                // 把目标位置传给 BufferState
                ChangeState(new BufferState(targetDepth));
            }
            else
            {
                if (!(currentState is RisingState) && !(currentState is BufferState))
                {
                    ChangeState(new RisingState());
                }
            }
        }

        /// <summary>
        /// 碰撞检测核心逻辑
        /// </summary>
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Fish")) return;
            Fish fish = other.GetComponent<Fish>();
            if (fish == null) return;

            if (isInvincible) return;
            if (data.currentFishCount >= data.maxFishCapacity) return;

            if (currentState is RisingState || currentState is BufferState)
            {
                CatchFish(fish);
                return;
            }

            if (currentState is SinkingState)
            {
                if (data.currentProtectionCount > 0)
                {
                    data.currentProtectionCount--;
                    TriggerScreenFlash();

                    // 调用无敌方法
                    ActivateInvincibility(1f);
                }
                else
                {
                    CatchFish(fish);
                    float extraSinkDistance = 5f;
                    float targetDepth = Mathf.Min(data.currentDepth + extraSinkDistance, data.maxDepth);
                    TryTriggerBuffer(-targetDepth);
                }
            }
        }

        /// <summary>
        /// 抓鱼逻辑：鱼头吸附、摆动挣扎、后抓的盖住先抓的
        /// </summary>
        private void CatchFish(Fish fish)
        {
            // 如果这条鱼已经被抓了，直接return，防止同一条鱼多次累加
            if (fish.IsCaught) return;

            data.currentFishCount++;
            caughtFishes.Add(fish);

            // 播放抓鱼声
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(catchFishSound);
            }

            //记录战利品统计
            if (fish.config != null)
            {
                if (CaughtStats.ContainsKey(fish.config))
                {
                    CaughtStats[fish.config]++;
                }
                else
                {
                    CaughtStats.Add(fish.config, 1);
                }
            }

            fish.transform.SetParent(this.transform);

            // 根据方向计算初始旋转角度
            // 如果是向右游，鱼头朝上，身体向左倾（正角度）
            // 如果是向左游，鱼头朝上，身体向右倾（负角度）
            float rotateAngle = fish.IsMovingRight ? 90f : -90f;

            // 将角度和鱼钩的Transform传给鱼，触发挣扎和摆动逻辑
            fish.OnCaught(rotateAngle, this.transform);

            // 吸附位置
            fish.transform.localPosition = Vector3.zero;

            // 层级控制：后抓的盖住先抓的
            SpriteRenderer fishSr = fish.GetComponent<SpriteRenderer>();
            if (fishSr != null)
            {
                fishSr.sortingOrder = 1 + data.currentFishCount;
            }

            if (data.currentFishCount >= data.maxFishCapacity && !(currentState is RisingState))
            {
                Debug.Log("鱼钩满载，开始加速上升！");
                ChangeState(new RisingState());
            }
        }

        private void TriggerScreenFlash()
        {
            // 广播事件，通知 UI 播放闪白
            OnScreenFlashRequested?.Invoke();
        }

        /// <summary>
        /// 触发游戏结束
        /// </summary>
        public void TriggerGameEnd()
        {
            // 停止一切更新，切换到一个停止状态
            currentState = null;

            if (fishPool != null)
            {
                fishPool.RecycleAllActiveFishes();
            }

            // 广播事件给结算面板
            OnGameEnded?.Invoke(MaxDepthReached, CaughtStats);
        }

        /// <summary>
        /// 重新开始游戏
        /// </summary>
        public void RestartGame()
        {
            // 清空数据
            data.ResetData();
            CaughtStats.Clear();
            MaxDepthReached = 0f;

            // 鱼钩和相机位置归零
            transform.position = new Vector3(0, 0, 0);
            Camera.main.transform.position = new Vector3(0, 0, -10);

            // 解除强制居中
            isForcedCentering = false;

            // 重新进入游戏
            ChangeState(new IdleState());
        }

        // 暴露给 ReelingState 用来遍历飞散
        public List<Fish> GetCaughtFishes()
        {
            return caughtFishes;
        }

        // 供 ReelingState 飞散完动画后调用，清理列表
        public void ClearCaughtFishesList()
        {
            caughtFishes.Clear();
        }

        /// <summary>
        /// 开启无敌状态（包含持续时间和闪烁效果）
        /// </summary>
        /// <param name="duration">持续时间</param>
        public void ActivateInvincibility(float duration)
        {
            // 如果已经在无敌状态，先停止之前的协程，重新计时
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);

            flashCoroutine = StartCoroutine(InvincibleRoutine(duration));
        }

        private IEnumerator InvincibleRoutine(float duration)
        {
            isInvincible = true;
            float timer = 0f;

            // 闪烁 + 计时
            while (timer < duration)
            {
                hookSprite.color = new Color(1f, 1f, 1f, 0.3f); // 变透明
                yield return new WaitForSeconds(0.1f);
                hookSprite.color = new Color(1f, 1f, 1f, 1f);   // 恢复
                yield return new WaitForSeconds(0.1f);

                timer += 0.2f;
            }

            // 无敌结束，恢复外观
            isInvincible = false;
            hookSprite.color = new Color(1f, 1f, 1f, 1f);
            flashCoroutine = null;
        }

        public void SetCameraLimitY(float limitY)
        {
            if (cameraFollow != null)
            {
                cameraFollow.SetMaxYLimit(limitY);
            }
        }

        public void SetCameraMinYLimit(float limitY)
        {
            if (cameraFollow != null) cameraFollow.SetMinYLimit(limitY);
        }

        /// <summary>
        /// 统一回收挂载在鱼钩上的所有鱼
        /// </summary>
        public void RecycleAllCaughtFishes()
        {
            if (caughtFishes.Count == 0) return;

            FishPool pool = fishPool;

            if (pool != null)
            {
                foreach (var fish in caughtFishes)
                {
                    if (fish != null)
                    {
                        pool.RecycleFish(fish);
                    }
                }
            }

            // 清空列表
            caughtFishes.Clear();
            ClearCaughtFishesList();
        }

        public void SnapCameraToTarget()
        {
            if (cameraFollow != null) cameraFollow.SnapToTarget();
        }

        /// <summary>
        /// 提供给外部状态类触发 UI 提示的接口
        /// </summary>
        public void TriggerStartTip(bool show)
        {
            OnShowStartTip?.Invoke(show);
        }

        public static bool IsPointerOverUI()
        {
            if (EventSystem.current == null) return false;

            // 手机端检测
            if (Input.touchCount > 0)
            {
                return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
            }
            // PC端检测
            return EventSystem.current.IsPointerOverGameObject();
        }

        /// <summary>
        /// 控制暂停按钮的显隐
        /// </summary>
        public void SetPauseBtnVisibility(bool isVisible)
        {
            OnPauseBtnVisibilityChanged?.Invoke(isVisible);
        }
    }
}