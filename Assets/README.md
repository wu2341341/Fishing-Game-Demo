## 项目概述
*   **开发环境**：Unity 2022.3.52f1
*   **项目说明**：基于测试题要求复刻并深度优化的 2D 冰钓小游戏，包含完整的游戏闭环、平滑的物理手感以及多端输入适配。
*   **第三方插件**：DOTween (用于动画与缓动曲线)

---

## 运行与操作说明

### 1. 操作方式
*   **PC端（编辑器/Windows）**：默认使用**鼠标拖拽**控制鱼钩左右移动。玩家可在暂停界面切换为**键盘操作**（A/D 或 左右方向键）。
*   **移动端（Android）**：默认使用**触屏滑动**，玩家可在暂停界面切换为**重力感应**操作。

### 2. 多端适配策略
*   利用 Unity 宏定义（`#if UNITY_ANDROID || UNITY_EDITOR`）自动识别运行平台。PC端隐藏重力感应按钮，移动端隐藏键盘按钮，避免玩家误触不可用的功能。
*   使用 `PlayerPrefs` 记录玩家的操作偏好，下次启动自动应用。
*   UI 采用 `Canvas Scaler` (Scale With Screen Size, Match Width) 进行分辨率适配，结合锚点设置，确保在不同比例的手机屏幕上不发生越界和错位。

---

## 核心架构与设计模式

本项目严格遵循 **MVC 架构**，并大量应用了经典设计模式，确保代码的高内聚、低耦合。

### 1. 状态模式 (State Pattern)
鱼钩的行为被拆分为独立的状态类（`IdleState`, `StartDropState`, `SinkingState`, `BufferState`, `RisingState`, `ReelingState`），通过实现 `IHookState` 接口进行切换。
*   **优势**：避免了 `Update` 中冗长的 `if-else` 判断，使得状态流转清晰明了。例如在 `BufferState`（触底/碰鱼缓冲）中，专门处理了速度的平滑过渡。

### 2. 数据驱动设计 (Data-Driven)
*   使用 **`ScriptableObject`** (`FishConfig`) 配置鱼类的深度范围、生成权重、星级和收益。
*   `FishSpawner` 实现了**深度过滤 + 权重抽奖算法**，实现了“深海出大鱼、浅海出小鱼”的生态，可直接在 Inspector 中调整数值，无需修改代码。

### 3. 对象池模式 (Object Pool)
*   针对高频生成的鱼群和金币飘字特效，采用了对象池 (`FishPool`) 管理。
*   彻底避免了频繁的 `Instantiate` 和 `Destroy` 产生的 GC 垃圾，保证了游戏长时间运行的帧率稳定。并在回收时重置了尺寸与旋转，解决了复用导致的显示异常问题。

### 4. 事件驱动与解耦 (Event-Driven)
*   表现层（UI/音效）与逻辑层（Controller）彻底解耦。核心控制器 `HookController` 通过 `C# Action` 广播事件（如 `OnDataChanged`, `OnGameEnded`, `OnShowStartTip`）。
*   `UIManager`、`ResultPanelManager` 等表现层脚本只负责监听事件并刷新 UI，不包含任何游戏逻辑。

---

## 手感与视觉表现调优 (Game Feel)

*   **缓冲掉头**：在 `BufferState` 中，使用 `Mathf.SmoothStep` 对**位置**进行 S 型曲线插值，完美消除了触底/碰鱼转向时的“急刹车”感。
*   **镜头前瞻 (Look-ahead)**：`CameraFollow` 根据鱼钩的当前逻辑速度动态偏移镜头，并在状态切换时（如缓冲阶段）平滑归零。
*   **出入水视觉欺骗**：上升时动态限制相机的 `MaxY`，隐藏冰面截面；出水瞬间解除限制并拉出水面，配合粒子特效，完美规避了穿帮。
*   **鱼上钩的物理挣扎**：根据鱼钩的水平移动速度，计算出惯性倾斜角（使用 `Mathf.Clamp` 限制最大角度防转圈），并叠加正弦波进行挣扎模拟。
*   **UI 动效**：全面接入 DOTween，实现了结算面板的弹性缩放、卡片的依次飞入、以及金币飘字渐隐，提升了整体的“打击感”和“爽感”。

---

## 目录结构说明

```text
Assets/
├── Art/          # 美术资源（Sprite、UI图、特效图）
├── Audio/        # 音效文件（BGM、入水、抓鱼、金币）
├── Data/         # ScriptableObject 配置文件 (FishConfig)
├── Plugins/      # 第三方插件（DOTween）
├── Prefab/       # 预制体（Fish、UI卡片、飘字、水花特效）
├── Scenes/       # 游戏场景 (SampleScene)
└── Scripts/      # 核心代码
    ├── Controllers/ # 控制器 (HookController, CameraFollow, AudioManager)
    ├── Entities/    # 实体类 (Fish, FishConfig, FishSpawner)
    ├── Models/      # 纯数据类 (HookData)
    ├── Pool/        # 对象池 (FishPool)
    ├── States/      # 状态机类 (IHookState 及各状态实现)
    └── Views/       # UI 表现层 (UIManager, ResultPanel, FloatingText等)