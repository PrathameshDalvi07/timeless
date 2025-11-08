using UnityEngine;
using System.Collections;

namespace TimeLessLove.Managers
{
    /// <summary>
    /// Manages all audio including background music and sound effects
    /// Singleton pattern with DontDestroyOnLoad
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager instance;
        public static AudioManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<AudioManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("AudioManager");
                        instance = go.AddComponent<AudioManager>();
                    }
                }
                return instance;
            }
        }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Volume Settings")]
        [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.7f;
        [SerializeField] [Range(0f, 1f)] private float sfxVolume = 1f;

        [Header("Fade Settings")]
        [SerializeField] private float musicFadeDuration = 1f;

        [Header("Sound Effects")]
        [SerializeField] private AudioClip buttonClickSFX;
        [SerializeField] private AudioClip correctAnswerSFX;
        [SerializeField] private AudioClip wrongAnswerSFX;
        [SerializeField] private AudioClip affectionUpSFX;
        [SerializeField] private AudioClip affectionDownSFX;

        private Coroutine musicFadeCoroutine;

        private void Awake()
        {
            // Singleton pattern
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeAudioSources();
        }

        /// <summary>
        /// Initializes audio sources if not assigned
        /// </summary>
        private void InitializeAudioSources()
        {
            if (musicSource == null)
            {
                GameObject musicGO = new GameObject("MusicSource");
                musicGO.transform.SetParent(transform);
                musicSource = musicGO.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }

            if (sfxSource == null)
            {
                GameObject sfxGO = new GameObject("SFXSource");
                sfxGO.transform.SetParent(transform);
                sfxSource = sfxGO.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
            }

            musicSource.volume = musicVolume;
            sfxSource.volume = sfxVolume;
        }

        /// <summary>
        /// Plays background music with crossfade
        /// </summary>
        public void PlayMusic(AudioClip clip, bool fade = true)
        {
            if (clip == null)
            {
                Debug.LogWarning("AudioManager: Cannot play null music clip!");
                return;
            }

            // If same clip is already playing, don't restart
            if (musicSource.clip == clip && musicSource.isPlaying)
            {
                return;
            }

            if (fade)
            {
                if (musicFadeCoroutine != null)
                {
                    StopCoroutine(musicFadeCoroutine);
                }
                musicFadeCoroutine = StartCoroutine(CrossfadeMusic(clip));
            }
            else
            {
                musicSource.clip = clip;
                musicSource.Play();
            }

            Debug.Log($"<color=cyan>Playing music: {clip.name}</color>");
        }

        /// <summary>
        /// Stops background music with fade
        /// </summary>
        public void StopMusic(bool fade = true)
        {
            if (fade)
            {
                if (musicFadeCoroutine != null)
                {
                    StopCoroutine(musicFadeCoroutine);
                }
                musicFadeCoroutine = StartCoroutine(FadeOutMusic());
            }
            else
            {
                musicSource.Stop();
            }
        }

        /// <summary>
        /// Crossfades between current music and new music
        /// </summary>
        private IEnumerator CrossfadeMusic(AudioClip newClip)
        {
            float halfDuration = musicFadeDuration / 2f;

            // Fade out current music
            if (musicSource.isPlaying)
            {
                yield return FadeVolume(musicSource, musicSource.volume, 0f, halfDuration);
                musicSource.Stop();
            }

            // Switch clip
            musicSource.clip = newClip;
            musicSource.Play();

            // Fade in new music
            yield return FadeVolume(musicSource, 0f, musicVolume, halfDuration);
        }

        /// <summary>
        /// Fades out music completely
        /// </summary>
        private IEnumerator FadeOutMusic()
        {
            yield return FadeVolume(musicSource, musicSource.volume, 0f, musicFadeDuration);
            musicSource.Stop();
            musicSource.volume = musicVolume;
        }

        /// <summary>
        /// Generic volume fade coroutine
        /// </summary>
        private IEnumerator FadeVolume(AudioSource source, float startVolume, float targetVolume, float duration)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                source.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
                yield return null;
            }

            source.volume = targetVolume;
        }

        /// <summary>
        /// Plays a one-shot sound effect
        /// </summary>
        public void PlaySFX(AudioClip clip, float volumeMultiplier = 1f)
        {
            if (clip == null)
            {
                Debug.LogWarning("AudioManager: Cannot play null SFX clip!");
                return;
            }

            sfxSource.PlayOneShot(clip, sfxVolume * volumeMultiplier);
        }

        /// <summary>
        /// Plays button click sound
        /// </summary>
        public void PlayButtonClick()
        {
            if (buttonClickSFX != null)
            {
                PlaySFX(buttonClickSFX);
            }
        }

        /// <summary>
        /// Plays correct answer sound
        /// </summary>
        public void PlayCorrectAnswer()
        {
            if (correctAnswerSFX != null)
            {
                PlaySFX(correctAnswerSFX);
            }
        }

        /// <summary>
        /// Plays wrong answer sound
        /// </summary>
        public void PlayWrongAnswer()
        {
            if (wrongAnswerSFX != null)
            {
                PlaySFX(wrongAnswerSFX);
            }
        }

        /// <summary>
        /// Plays affection increase sound
        /// </summary>
        public void PlayAffectionUp()
        {
            if (affectionUpSFX != null)
            {
                PlaySFX(affectionUpSFX);
            }
        }

        /// <summary>
        /// Plays affection decrease sound
        /// </summary>
        public void PlayAffectionDown()
        {
            if (affectionDownSFX != null)
            {
                PlaySFX(affectionDownSFX);
            }
        }

        /// <summary>
        /// Sets music volume
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            musicSource.volume = musicVolume;
        }

        /// <summary>
        /// Sets SFX volume
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            sfxSource.volume = sfxVolume;
        }

        /// <summary>
        /// Gets current music volume
        /// </summary>
        public float GetMusicVolume() => musicVolume;

        /// <summary>
        /// Gets current SFX volume
        /// </summary>
        public float GetSFXVolume() => sfxVolume;

        /// <summary>
        /// Mutes all audio
        /// </summary>
        public void MuteAll()
        {
            musicSource.mute = true;
            sfxSource.mute = true;
        }

        /// <summary>
        /// Unmutes all audio
        /// </summary>
        public void UnmuteAll()
        {
            musicSource.mute = false;
            sfxSource.mute = false;
        }

        /// <summary>
        /// Pauses background music
        /// </summary>
        public void PauseMusic()
        {
            musicSource.Pause();
        }

        /// <summary>
        /// Resumes background music
        /// </summary>
        public void ResumeMusic()
        {
            musicSource.UnPause();
        }
    }
}
