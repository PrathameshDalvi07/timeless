using UnityEngine;
using TMPro;
using System.Collections;
using DG.Tweening;
using UnityEngine.UI;

namespace TimeLessLove.Managers
{
    /// <summary>
    /// Manages the outro scene with word-by-word text reveal animation
    /// Includes an exit button to quit or return to main menu
    /// </summary>
    public class OutroSceneManager : MonoBehaviour
    {
        [Header("Text Settings")]
        [SerializeField] private TextMeshProUGUI outroTextField;
        [SerializeField] private TextRevealSettings[] textToReveal;
        [SerializeField] private AudioClip voiceOverAudio;

        [Header("Reveal Settings")]
        [SerializeField] private float delayBeforeStart = 0.5f;
        [SerializeField] private float delayAfterEnd = 1.0f;

        [Header("Exit Button")]
        [SerializeField] private Button exitButton;
        [SerializeField] private GameObject exitButtonObject;
        [SerializeField] private bool showExitButtonAfterReveal = true;
        [SerializeField] private float exitButtonFadeInDuration = 0.5f;

        [Header("Options")]
        [SerializeField] private bool canSkip = true;
        [SerializeField] private KeyCode skipKey = KeyCode.Space;

        [Header("Exit Behavior")]
        [SerializeField] private ExitBehaviorType exitBehavior = ExitBehaviorType.QuitApplication;
        [SerializeField] private string sceneToLoad = "MainMenu"; // Used if exitBehavior is LoadScene

        private bool isRevealing = false;
        private bool isComplete = false;

        [Header("Visual Elements (Optional)")]
        [SerializeField] private SpriteRenderer fadeInImage;
        [SerializeField] private Transform[] animatedElements;

        public enum ExitBehaviorType
        {
            QuitApplication,
            LoadScene,
            ReturnToMainMenu
        }

        private void Start()
        {
            if (outroTextField == null)
            {
                Debug.LogError("OutroSceneManager: OutroTextField not assigned!");
                return;
            }

            if (textToReveal == null || textToReveal.Length == 0)
            {
                Debug.LogError("OutroSceneManager: No text to reveal!");
                return;
            }

            // Hide exit button initially
            if (exitButtonObject != null)
            {
                exitButtonObject.SetActive(false);
            }

            // Setup exit button listener
            if (exitButton != null)
            {
                exitButton.onClick.AddListener(OnExitButtonClicked);
            }

            // Play outro music
            // if (AudioManager.Instance != null)
            // {
            //     AudioManager.Instance.PlayOutroMusic(fade: false);
            // }

            // Clear the text field initially
            outroTextField.text = "";

            // Start the reveal sequence
            StartCoroutine(RevealTextSequence());
        }

        private void Update()
        {
            // Allow skipping if enabled
            if (canSkip && Input.GetKeyDown(skipKey) && isRevealing)
            {
                StopAllCoroutines();
                CompleteOutro();
            }
        }

        /// <summary>
        /// Main coroutine that reveals text letter by letter
        /// Letters are revealed in place without shifting previous letters
        /// Only one sentence is visible at a time
        /// </summary>
        private IEnumerator RevealTextSequence()
        {
            isRevealing = true;

            StartAnimatedElements();

            // Initial delay before starting
            yield return new WaitForSeconds(delayBeforeStart);

            PlayVoiceOverAudio();

            // Loop through each sentence
            for (int i = 0; i < textToReveal.Length; i++)
            {
                TextRevealSettings settings = textToReveal[i];
                string currentText = settings.text;

                // Create placeholder text for this sentence (all spaces)
                string displayText = new(' ', currentText.Length);

                // Replace spaces with actual spaces in placeholder
                char[] displayChars = displayText.ToCharArray();
                for (int j = 0; j < currentText.Length; j++)
                {
                    if (currentText[j] == ' ')
                    {
                        displayChars[j] = ' ';
                    }
                }

                // Set initial text with all placeholders for this sentence
                outroTextField.text = new(displayChars);

                // Reveal each letter by replacing placeholder
                for (int j = 0; j < currentText.Length; j++)
                {
                    // Skip spaces as they're already shown
                    if (currentText[j] == ' ')
                        continue;

                    // Replace the placeholder with the actual letter
                    displayChars[j] = currentText[j];

                    // Update the display text
                    outroTextField.text = ApplyTextFormatting(new(displayChars));

                    // Use the delay specific to this text line
                    yield return new WaitForSeconds(settings.delayBetweenLetters);
                }

                // Delay after sentence using the specific delay for this line
                if (i < textToReveal.Length - 1)
                {
                    yield return new WaitForSeconds(settings.delayAfterSentence);
                }
            }

            // Wait after last sentence completes
            yield return new WaitForSeconds(delayAfterEnd);

            outroTextField.text = "";

            if (Camera.main != null)
            {
                Camera.main.transform.DOMoveY(Camera.main.transform.position.y + 10f, 10f)
                    .SetEase(Ease.InOutQuad);
            }

            yield return new WaitForSeconds(7f);
            
            fadeInImage.DOFade(1f, delayBeforeStart);
            yield return new WaitForSeconds(3f);

            // All text has been revealed
            CompleteOutro();
        }

        /// <summary>
        /// Starts animation for optional visual elements
        /// </summary>
        private void StartAnimatedElements()
        {
            if (animatedElements == null || animatedElements.Length == 0)
            {
                return;
            }

            // Animate each element
            foreach (Transform element in animatedElements)
            {
                if (element == null) continue;

                // Random fade in animation
                SpriteRenderer sr = element.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.DOFade(1f, Random.Range(1f, 2f));
                }

                // Random scale animation
                element.DOScale(element.localScale * 1.1f, Random.Range(2f, 4f))
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo);
            }
        }

        /// <summary>
        /// Applies formatting to specific words (can be customized)
        /// </summary>
        private string ApplyTextFormatting(string text)
        {
            // Apply custom formatting here if needed
            // Example: text = text.Replace("TimeLess", "<b>TimeLess</b>");
            return text;
        }

        /// <summary>
        /// Plays voice-over audio for the outro
        /// </summary>
        private void PlayVoiceOverAudio()
        {
            if (voiceOverAudio != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(voiceOverAudio);
            }
        }

        /// <summary>
        /// Called when the outro sequence is complete
        /// </summary>
        private void CompleteOutro()
        {
            isRevealing = false;
            isComplete = true;

            Debug.Log("Outro sequence completed!");

            // Show exit button
            if (showExitButtonAfterReveal && exitButtonObject != null)
            {
                ShowExitButton();
            }
        }

        /// <summary>
        /// Shows the exit button with fade-in animation
        /// </summary>
        private void ShowExitButton()
        {
            if (exitButtonObject == null) return;

            exitButtonObject.SetActive(true);

            // Fade in animation
            CanvasGroup canvasGroup = exitButtonObject.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = exitButtonObject.AddComponent<CanvasGroup>();
            }

            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, exitButtonFadeInDuration).SetEase(Ease.InOutQuad);

            // Scale animation
            exitButtonObject.transform.localScale = Vector3.zero;
            exitButtonObject.transform.DOScale(Vector3.one, exitButtonFadeInDuration).SetEase(Ease.OutBack);
        }

        /// <summary>
        /// Called when exit button is clicked
        /// </summary>
        private void OnExitButtonClicked()
        {
            // Play button click sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonClick();
            }

            // Disable button to prevent multiple clicks
            if (exitButton != null)
            {
                exitButton.interactable = false;
            }

            // Execute exit behavior
            StartCoroutine(ExecuteExit());
        }

        /// <summary>
        /// Executes the exit behavior
        /// </summary>
        private IEnumerator ExecuteExit()
        {
            Debug.Log($"Executing exit behavior: {exitBehavior}");

            // Optional fade out
            yield return new WaitForSeconds(0.5f);

            switch (exitBehavior)
            {
                case ExitBehaviorType.QuitApplication:
                    QuitApplication();
                    break;

                case ExitBehaviorType.LoadScene:
                    LoadScene(sceneToLoad);
                    break;

                case ExitBehaviorType.ReturnToMainMenu:
                    ReturnToMainMenu();
                    break;
            }
        }

        /// <summary>
        /// Quits the application
        /// </summary>
        private void QuitApplication()
        {
            Debug.Log("Quitting application...");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        /// <summary>
        /// Loads a specific scene
        /// </summary>
        private void LoadScene(string sceneName)
        {
            Debug.Log($"Loading scene: {sceneName}");

            if (TransitionSceneManager.Instance != null)
            {
                TransitionSceneManager.Instance.TransitionToScene_(sceneName);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
            }
        }

        /// <summary>
        /// Returns to main menu
        /// </summary>
        private void ReturnToMainMenu()
        {
            Debug.Log("Returning to main menu...");
            LoadScene("MainMenu");
        }
    }
}
