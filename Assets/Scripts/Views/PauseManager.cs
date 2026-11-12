using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using FishingGame.Controllers;

namespace FishingGame.Views
{
    public class PauseManager : MonoBehaviour
    {
        public GameObject pausePanel;
        public Button pauseBtn;
        public Button resumeBtn;
        public Button quitBtn;

        private HookController hook;

        private void Start()
        {
            hook = FindObjectOfType<HookController>();
            if (hook != null)
            {
                hook.OnPauseBtnVisibilityChanged += HandlePauseBtnVisibility;
            }

            pauseBtn.onClick.AddListener(OnPauseClicked);
            resumeBtn.onClick.AddListener(OnResumeClicked);
            quitBtn.onClick.AddListener(OnQuitClicked);

            pausePanel.SetActive(false);
        }

        private void OnDestroy()
        {
            if (hook != null)
            {
                hook.OnPauseBtnVisibilityChanged -= HandlePauseBtnVisibility;
            }
        }

        private void HandlePauseBtnVisibility(bool isVisible)
        {
            if (pauseBtn != null)
            {
                pauseBtn.gameObject.SetActive(isVisible);
            }
        }

        private void OnPauseClicked()
        {
            pausePanel.SetActive(true); // 面板黑底瞬间铺满

            // 只让内容部分有弹性弹出的效果
            Transform content = pausePanel.transform.Find("Content");
            if (content != null)
            {
                // 暂停时 Time.timeScale = 0。Dotween 默认也会被冻结。
                // 加上 .SetUpdate(true) 让面板的动画无视时间暂停，正常播放
                content.localScale = Vector3.zero;
                content.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
            }

            Time.timeScale = 0f;// 冻结游戏内所有时间
        }

        private void OnResumeClicked()
        {
            Time.timeScale = 1f; // 恢复时间
            pausePanel.SetActive(false);
        }

        private void OnQuitClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}