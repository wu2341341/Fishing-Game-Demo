using UnityEngine;

namespace FishingGame.Controllers
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        private AudioSource sfxSource;

        private void Awake()
        {
            // 单例模式
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject); // 切场景不销毁
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            // 创建一个用于播放音效的 AudioSource
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }

        /// <summary>
        /// 播放单次音效
        /// </summary>
        public void PlaySFX(AudioClip clip, float volume = 1f)
        {
            if (clip != null)
            {
                sfxSource.PlayOneShot(clip, volume);
            }
        }
    }
}