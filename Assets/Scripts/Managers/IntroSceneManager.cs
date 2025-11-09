using UnityEngine;
using TMPro;
using System.Collections;
using DG.Tweening;

namespace TimeLessLove.Managers
{
    /// <summary>
    /// Serializable class to hold text with individual delay settings
    /// </summary>
    [System.Serializable]
    public class TextRevealSettings
    {
        [TextArea(3, 10)]
        public string text;

        [Tooltip("Delay between each letter reveal")]
        public float delayBetweenLetters = 0.05f;

        [Tooltip("Delay after this sentence completes")]
        public float delayAfterSentence = 1.0f;
    }

    /// <summary>
    /// Manages the intro scene with word-by-word text reveal animation
    /// Calls StartGame() after all text has been revealed
    /// </summary>
    public class IntroSceneManager : MonoBehaviour
    {
        [Header("Text Settings")]
        [SerializeField] private TextMeshProUGUI introTextField;
        [SerializeField] private TextRevealSettings[] textToReveal;
        [SerializeField] private AudioClip voiceOverAudio;

        [Header("Reveal Settings")]
        [SerializeField] private float delayBeforeStart = 0.5f;
        [SerializeField] private float delayAfterEnd = 1.0f;

        [Header("Options")]
        [SerializeField] private bool canSkip = true;
        [SerializeField] private KeyCode skipKey = KeyCode.Space;

        private bool isRevealing = false;
        private bool isComplete = false;

        [SerializeField] SpriteRenderer gameNameSR;
        [SerializeField] Transform[] cloudTransforms;

        private void Start()
        {
            if (introTextField == null)
            {
                Debug.LogError("IntroSceneManager: IntroTextField not assigned!");
                return;
            }

            if (textToReveal == null || textToReveal.Length == 0)
            {
                Debug.LogError("IntroSceneManager: No text to reveal!");
                return;
            }

            // Play intro music
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayIntroMusic(fade: false);
            }

            // Clear the text field initially
            introTextField.text = "";

            // Start the reveal sequence
            StartCoroutine(RevealTextSequence());
        }

        private void Update()
        {
            // Allow skipping if enabled
            if (Input.GetKeyDown(skipKey))
            {
                StopAllCoroutines();
                CompleteIntro();
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

            gameNameSR.DOFade(1f, delayBeforeStart);
            StartCloudMovement();

            // Initial delay before starting
            yield return new WaitForSeconds(delayBeforeStart);

            // Set Camera Movement Down - It should Feel Like Moving from Sky to Ground at Ease
            if (Camera.main != null)
            {
                Camera.main.transform.DOMoveY(Camera.main.transform.position.y - 10f, 10f)
                    .SetEase(Ease.InOutQuad);
            }

            yield return new WaitForSeconds(3f);
            gameNameSR.DOFade(0f, 3f);
            yield return new WaitForSeconds(7f);

            PlayVoiceOverAudio();
            // Loop through each sentence
            for (int i = 0; i < textToReveal.Length; i++)
            {
                TextRevealSettings settings = textToReveal[i];
                string currentText = settings.text;

                // Play voice-over audio for this text segment

                // Create placeholder text for this sentence (all underscores)
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
                introTextField.text = new(displayChars);

                // Reveal each letter by replacing placeholder
                for (int j = 0; j < currentText.Length; j++)
                {
                    // Skip spaces as they're already shown
                    if (currentText[j] == ' ')
                        continue;

                    // Replace the placeholder with the actual letter
                    displayChars[j] = currentText[j];

                    // Update the display text with bold formatting applied
                    introTextField.text = ApplyBoldToTimeLess(new(displayChars));

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

            // All text has been revealed
            CompleteIntro();
        }

        void StartCloudMovement()
        {
            if (cloudTransforms == null || cloudTransforms.Length == 0)
            {
                Debug.LogWarning("IntroSceneManager: No cloud transforms assigned!");
                return;
            }

            // Animate each cloud to move either left or right
            foreach (Transform cloud in cloudTransforms)
            {
                if (cloud == null) continue;

                // Randomly choose left or right direction
                float direction = Random.Range(0, 2) == 0 ? -1f : 1f;

                // Random movement distance (adjust these values based on your screen size)
                float moveDistance = Random.Range(15f, 25f) * direction;

                // Random duration for variation
                float duration = Random.Range(10f, 25f);

                // Create the target position
                Vector3 targetPosition = cloud.position + new Vector3(moveDistance, 0f, 0f);

                // Animate cloud movement with DOTween
                cloud.DOMove(targetPosition, duration)
                    .SetEase(Ease.Linear)
                    .SetLoops(-1, LoopType.Yoyo); // Loop infinitely with yoyo effect
            }
        }

        /// <summary>
        /// Applies bold formatting to specific words
        /// </summary>
        private string ApplyBoldToTimeLess(string text)
        {
            // Replace "TimeLess" and "Eternal Bond." with bold versions using TextMeshPro tags
            text = text.Replace("TimeLess", "<b>TimeLess</b>");
            text = text.Replace("Eternal Bond.", "<b>Eternal Bond.</b>");
            return text;
        }

        /// <summary>
        /// Plays voice-over audio for the given text segment index
        /// </summary>
        private void PlayVoiceOverAudio()
        {
            AudioManager.Instance.PlaySFX(voiceOverAudio);
        }

        /// <summary>
        /// Called when the intro sequence is complete
        /// </summary>
        private void CompleteIntro()
        {
            isRevealing = false;
            isComplete = true;

            // Clear the text field
            introTextField.text = "";

            Debug.Log("Intro sequence completed!");

            // Start the game
            StartGame();
        }

        /// <summary>
        /// Starts the game after intro is complete
        /// TODO: Add game start logic here
        /// </summary>
        private void StartGame()
        {
            // TODO: Implement game start logic
            // For example:
            // - Load next scene
            // - Initialize game state
            // - Transition to main menu or gameplay

            Debug.Log("StartGame() called - Implement game start logic here");
            TransitionSceneManager.Instance.TransitionToScene_("Gameplay");
        }
    }
}
