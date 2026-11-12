using UnityEngine;
using UnityEngine.UI;
using FishingGame.Controllers;

namespace FishingGame.Views
{
    public class InputSettingsUI : MonoBehaviour
    {
        [Header("UI 按钮")]
        public Button btnKeyboard;
        public Button btnTouch;
        public Button btnGravity;

        [Header("颜色配置 (视觉反馈)")]
        public Color normalColor = Color.white;       // 未选中时的颜色
        public Color selectedColor = Color.green;     // 选中时的颜色

        private HookController hook;
        private const string SAVE_KEY = "InputMode";

        private void Start()
        {
            hook = FindObjectOfType<HookController>();

            if (btnKeyboard != null) btnKeyboard.onClick.AddListener(() => SetInputMode(HookController.InputMode.Keyboard));
            if (btnTouch != null) btnTouch.onClick.AddListener(() => SetInputMode(HookController.InputMode.Touch));
            if (btnGravity != null) btnGravity.onClick.AddListener(() => SetInputMode(HookController.InputMode.Gravity));

            LoadSettingsAndAdaptPlatform();
        }

        private void LoadSettingsAndAdaptPlatform()
        {
            int defaultMode = (int)HookController.InputMode.Touch;
            int savedMode = PlayerPrefs.GetInt(SAVE_KEY, defaultMode);
            HookController.InputMode finalMode = (HookController.InputMode)savedMode;

#if UNITY_STANDALONE || UNITY_EDITOR
            if (btnGravity != null) btnGravity.gameObject.SetActive(false);
            if (finalMode == HookController.InputMode.Gravity) finalMode = HookController.InputMode.Touch;
#elif UNITY_ANDROID || UNITY_IOS
            if (btnKeyboard != null) btnKeyboard.gameObject.SetActive(false);
            if (finalMode == HookController.InputMode.Keyboard) finalMode = HookController.InputMode.Touch;
#endif

            ApplyMode(finalMode);
        }

        private void SetInputMode(HookController.InputMode mode)
        {
            if (hook == null) return;
            hook.inputMode = mode;
            PlayerPrefs.SetInt(SAVE_KEY, (int)mode);
            PlayerPrefs.Save();
            UpdateButtonVisual(mode);
        }

        private void ApplyMode(HookController.InputMode mode)
        {
            if (hook == null) return;
            hook.inputMode = mode;
            UpdateButtonVisual(mode);
        }

        /// <summary>
        /// 视觉反馈：根据当前模式改变按钮背景色
        /// </summary>
        private void UpdateButtonVisual(HookController.InputMode currentMode)
        {
            // 安全地获取按钮的 Image 组件并改色
            if (btnKeyboard != null && btnKeyboard.TryGetComponent(out Image kbImg))
                kbImg.color = (currentMode == HookController.InputMode.Keyboard) ? selectedColor : normalColor;

            if (btnTouch != null && btnTouch.TryGetComponent(out Image touchImg))
                touchImg.color = (currentMode == HookController.InputMode.Touch) ? selectedColor : normalColor;

            if (btnGravity != null && btnGravity.TryGetComponent(out Image gravImg))
                gravImg.color = (currentMode == HookController.InputMode.Gravity) ? selectedColor : normalColor;
        }
    }
}