using UnityEngine;
using TMPro;
using System.Collections;

namespace TimeLessLove.Managers
{
    /// <summary>
    /// Manages the intro scene with word-by-word text reveal animation
    /// Calls StartGame() after all text has been revealed
    /// </summary>
    public class IntroSceneManager : MonoBehaviour
    {
        [Header("Text Settings")]
        [SerializeField] private TextMeshProUGUI introTextField;
        [SerializeField, TextArea(3, 10)] private string[] textToReveal;

        [Header("Reveal Settings")]
        [SerializeField] private float delayBetweenLetters = 0.05f;
        [SerializeField] private float delayBetweenSentences = 1.0f;
        [SerializeField] private float delayBeforeStart = 0.5f;
        [SerializeField] private float delayAfterEnd = 1.0f;

        [Header("Options")]
        [SerializeField] private bool canSkip = true;
        [SerializeField] private KeyCode skipKey = KeyCode.Space;

        private bool isRevealing = false;
        private bool isComplete = false;

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

            // Clear the text field initially
            introTextField.text = "";

            // Start the reveal sequence
            StartCoroutine(RevealTextSequence());
        }

        private void Update()
        {
            // Allow skipping if enabled
            if (canSkip && Input.GetKeyDown(skipKey) && isRevealing && !isComplete)
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

            // Initial delay before starting
            yield return new WaitForSeconds(delayBeforeStart);

            // Loop through each sentence
            for (int i = 0; i < textToReveal.Length; i++)
            {
                string currentText = textToReveal[i];

                // Create placeholder text for this sentence (all underscores)
                string displayText = new('.', currentText.Length);

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

                    yield return new WaitForSeconds(delayBetweenLetters);
                }

                // Delay between sentences (except after the last one)
                if (i < textToReveal.Length - 1)
                {
                    yield return new WaitForSeconds(delayBetweenSentences);
                }
            }

            // Wait after last sentence completes
            yield return new WaitForSeconds(delayAfterEnd);

            // All text has been revealed
            CompleteIntro();
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
