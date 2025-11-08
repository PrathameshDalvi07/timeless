using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System;

namespace TimeLessLove.Managers
{
    /// <summary>
    /// Handles smooth scene transitions with fade effects
    /// Uses CanvasGroup for fade-in/fade-out animations
    /// </summary>
    public class TransitionSceneManager : MonoBehaviour
    {
        private static TransitionSceneManager instance;
        public static TransitionSceneManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<TransitionSceneManager>();
                }
                return instance;
            }
        }

        [Header("Transition Settings")]
        [SerializeField] private CanvasGroup transitionCanvasGroup;
        [SerializeField] private float fadeInDuration = 0.5f;
        [SerializeField] private float fadeOutDuration = 0.5f;
        [SerializeField] private float delayBeforeLoad = 0.3f;

        [Header("Alternative: Animator")]
        [SerializeField] private Animator transitionAnimator;
        [SerializeField] private string fadeInTrigger = "FadeIn";
        [SerializeField] private string fadeOutTrigger = "FadeOut";

        [Header("Options")]
        [SerializeField] private bool useAnimator = false;
        [SerializeField] private bool showLoadingProgress = false;

        // Events
        public event Action OnTransitionStarted;
        public event Action OnTransitionCompleted;

        private bool isTransitioning = false;

        private void Awake()
        {
            // Singleton setup
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            // Validate canvas group
            if (!useAnimator && transitionCanvasGroup == null)
            {
                Debug.LogError("TransitionSceneManager: TransitionCanvasGroup not assigned!");
            }

            // Start with transparent overlay
            if (transitionCanvasGroup != null)
            {
                transitionCanvasGroup.alpha = 0f;
                transitionCanvasGroup.blocksRaycasts = false;
            }
        }

        /// <summary>
        /// Loads a scene by name with transition effect
        /// </summary>
        public void LoadScene(string sceneName)
        {
            if (isTransitioning)
            {
                Debug.LogWarning("Transition already in progress!");
                return;
            }

            StartCoroutine(TransitionToScene(sceneName));
        }

        /// <summary>
        /// Loads the next scene in build index
        /// </summary>
        public void LoadNextScene()
        {
            int nextSceneIndex = (SceneManager.GetActiveScene().buildIndex + 1) % SceneManager.sceneCountInBuildSettings;
            LoadScene(SceneManager.GetSceneByBuildIndex(nextSceneIndex).name);
        }

        /// <summary>
        /// Reloads the current scene
        /// </summary>
        public void ReloadCurrentScene()
        {
            LoadScene(SceneManager.GetActiveScene().name);
        }


        public void TransitionToScene_(string sceneName)
        {
            StartCoroutine(TransitionToScene(sceneName));
        }

        /// <summary>
        /// Main transition coroutine
        /// </summary>
        private IEnumerator TransitionToScene(string sceneName)
        {
            isTransitioning = true;
            OnTransitionStarted?.Invoke();

            // Fade out (to black)
            if (useAnimator && transitionAnimator != null)
            {
                yield return AnimatorFadeOut();
            }
            else
            {
                yield return FadeOut();
            }

            // Optional delay
            if (delayBeforeLoad > 0)
            {
                yield return new WaitForSeconds(delayBeforeLoad);
            }

            // Load scene asynchronously
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            asyncLoad.allowSceneActivation = false;

            // Wait until scene is almost loaded
            while (asyncLoad.progress < 0.9f)
            {
                if (showLoadingProgress)
                {
                    Debug.Log($"Loading: {asyncLoad.progress * 100}%");
                }
                yield return null;
            }

            // Activate the scene
            asyncLoad.allowSceneActivation = true;

            // Wait for scene to fully load
            yield return new WaitUntil(() => asyncLoad.isDone);

            // Fade in (from black)
            if (useAnimator && transitionAnimator != null)
            {
                yield return AnimatorFadeIn();
            }
            else
            {
                yield return FadeIn();
            }

            isTransitioning = false;
            OnTransitionCompleted?.Invoke();

            Debug.Log($"<color=cyan>Scene loaded: {sceneName}</color>");
        }

        /// <summary>
        /// Fades to black using CanvasGroup
        /// </summary>
        private IEnumerator FadeOut()
        {
            if (transitionCanvasGroup == null) yield break;

            transitionCanvasGroup.blocksRaycasts = true;
            float elapsed = 0f;

            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                transitionCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeOutDuration);
                yield return null;
            }

            transitionCanvasGroup.alpha = 1f;
        }

        /// <summary>
        /// Fades from black using CanvasGroup
        /// </summary>
        private IEnumerator FadeIn()
        {
            if (transitionCanvasGroup == null) yield break;

            float elapsed = 0f;

            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                transitionCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeInDuration);
                yield return null;
            }

            transitionCanvasGroup.alpha = 0f;
            transitionCanvasGroup.blocksRaycasts = false;
        }

        /// <summary>
        /// Fade out using Animator
        /// </summary>
        private IEnumerator AnimatorFadeOut()
        {
            transitionAnimator.SetTrigger(fadeOutTrigger);
            yield return new WaitForSeconds(fadeOutDuration);
        }

        /// <summary>
        /// Fade in using Animator
        /// </summary>
        private IEnumerator AnimatorFadeIn()
        {
            transitionAnimator.SetTrigger(fadeInTrigger);
            yield return new WaitForSeconds(fadeInDuration);
        }

        /// <summary>
        /// Instantly sets the transition overlay to black (useful for scene start)
        /// </summary>
        public void SetBlackScreen()
        {
            if (transitionCanvasGroup != null)
            {
                transitionCanvasGroup.alpha = 1f;
                transitionCanvasGroup.blocksRaycasts = true;
            }
        }

        /// <summary>
        /// Instantly clears the transition overlay
        /// </summary>
        public void ClearScreen()
        {
            if (transitionCanvasGroup != null)
            {
                transitionCanvasGroup.alpha = 0f;
                transitionCanvasGroup.blocksRaycasts = false;
            }
        }

        /// <summary>
        /// Plays just the transition effect without loading a scene
        /// </summary>
        public void PlayTransitionEffect(Action onComplete = null)
        {
            StartCoroutine(PlayTransitionEffectCoroutine(onComplete));
        }

        private IEnumerator PlayTransitionEffectCoroutine(Action onComplete)
        {
            yield return FadeOut();
            yield return new WaitForSeconds(0.2f);
            yield return FadeIn();
            onComplete?.Invoke();
        }
    }
}
