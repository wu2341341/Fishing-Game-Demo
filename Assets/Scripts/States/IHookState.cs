namespace FishingGame.States
{
    /// <summary>
    /// 鱼钩状态接口 (State Pattern)
    /// </summary>
    public interface IHookState
    {
        /// <summary>进入状态时调用</summary>
        void Enter(Controllers.HookController hook);

        /// <summary>状态每帧更新调用</summary>
        void Update(Controllers.HookController hook);

        /// <summary>退出状态时调用</summary>
        void Exit(Controllers.HookController hook);
    }
}