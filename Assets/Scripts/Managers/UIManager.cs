using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using TimeLessLove.Data;

namespace TimeLessLove.Managers
{
    /// <summary>
    /// Manages all UI interactions including buttons, question displays, and HUD
    /// Uses Action events for decoupled communication
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject gameplayPanel;
        [SerializeField] private GameObject questionPanel;
        [SerializeField] private GameObject resultPanel;

        [Header("Dialogue UI")]
        [SerializeField] private Button nextDialogueButton;
        [SerializeField] private TextMeshProUGUI dialogueProgressText;

        [Header("Question UI")]
        [SerializeField] private TextMeshProUGUI questionText;
        [SerializeField] private Button[] answerButtons;
        [SerializeField] private TextMeshProUGUI[] answerTexts;
        [SerializeField] private Button submitAnswerButton;

        [Header("Result UI")]
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private TextMeshProUGUI affectionChangeText;
        [SerializeField] private Button continueButton;

        [Header("HUD")]
        [SerializeField] private Slider affectionSlider;
        [SerializeField] private TextMeshProUGUI affectionValueText;
        [SerializeField] private TextMeshProUGUI dayCounterText;
        [SerializeField] private TextMeshProUGUI sceneNameText;

        [Header("Background")]
        [SerializeField] private Image backgroundImage;

        [Header("Colors")]
        [SerializeField] private Color correctAnswerColor = Color.green;
        [SerializeField] private Color wrongAnswerColor = Color.red;
        [SerializeField] private Color normalButtonColor = Color.white;
        [SerializeField] private Color selectedButtonColor = new Color(0.8f, 0.8f, 1f);

        // Events
        public event Action OnNextDialogueClicked;
        public event Action<int> OnAnswerSelected; // selected index
        public event Action OnSubmitAnswerClicked;
        public event Action OnContinueClicked;

        // State
        private int selectedAnswerIndex = -1;
        private bool answeredCorrectly = false;

        private void Awake()
        {
            SetupButtonListeners();
            HideAllPanels();
        }

        /// <summary>
        /// Sets up all button click listeners
        /// </summary>
        private void SetupButtonListeners()
        {
            // Next dialogue button
            if (nextDialogueButton != null)
            {
                nextDialogueButton.onClick.AddListener(() => OnNextDialogueClicked?.Invoke());
            }

            // Answer buttons
            for (int i = 0; i < answerButtons.Length; i++)
            {
                int index = i; // Capture for lambda
                if (answerButtons[i] != null)
                {
                    answerButtons[i].onClick.AddListener(() => SelectAnswer(index));
                }
            }

            // Submit answer button
            if (submitAnswerButton != null)
            {
                submitAnswerButton.onClick.AddListener(() => OnSubmitAnswerClicked?.Invoke());
                submitAnswerButton.interactable = false;
            }

            // Continue button
            if (continueButton != null)
            {
                continueButton.onClick.AddListener(() => OnContinueClicked?.Invoke());
            }
        }

        /// <summary>
        /// Shows the question panel with provided question data
        /// </summary>
        public void DisplayQuestion(QuestionData question)
        {
            if (question == null)
            {
                Debug.LogError("UIManager: Cannot display null question!");
                return;
            }

            ShowPanel(questionPanel);

            // Set question text
            if (questionText != null)
            {
                questionText.text = question.questionText;
            }

            // Reset selection
            selectedAnswerIndex = -1;
            if (submitAnswerButton != null)
            {
                submitAnswerButton.interactable = false;
            }

            // Display answer choices
            for (int i = 0; i < answerButtons.Length; i++)
            {
                if (i < question.choices.Length)
                {
                    answerButtons[i].gameObject.SetActive(true);
                    answerTexts[i].text = question.choices[i];

                    // Reset button color
                    var colors = answerButtons[i].colors;
                    colors.normalColor = normalButtonColor;
                    answerButtons[i].colors = colors;
                }
                else
                {
                    answerButtons[i].gameObject.SetActive(false);
                }
            }

            Debug.Log($"<color=yellow>Question displayed: {question.questionText}</color>");
        }

        /// <summary>
        /// Handles answer button selection
        /// </summary>
        private void SelectAnswer(int index)
        {
            selectedAnswerIndex = index;

            // Visual feedback for selection
            for (int i = 0; i < answerButtons.Length; i++)
            {
                var colors = answerButtons[i].colors;
                colors.normalColor = (i == index) ? selectedButtonColor : normalButtonColor;
                answerButtons[i].colors = colors;

                // Force the button to refresh its visual state
                answerButtons[i].targetGraphic.color = (i == index) ? selectedButtonColor : normalButtonColor;
            }

            // Enable submit button
            if (submitAnswerButton != null)
            {
                submitAnswerButton.interactable = true;
            }

            OnAnswerSelected?.Invoke(index);

            Debug.Log($"Answer selected: {index}");
        }

        /// <summary>
        /// Shows the result of the answer
        /// </summary>
        public void DisplayResult(bool correct, string responseText, int affectionChange)
        {
            ShowPanel(resultPanel);

            answeredCorrectly = correct;

            // Set result text
            if (resultText != null)
            {
                resultText.text = responseText;
            }

            // Set affection change text
            if (affectionChangeText != null)
            {
                string sign = affectionChange >= 0 ? "+" : "";
                affectionChangeText.text = $"Affection: {sign}{affectionChange}";
            }

            Debug.Log($"<color={(correct ? "green" : "red")}>Result: {(correct ? "Correct" : "Wrong")}</color>");
        }

        /// <summary>
        /// Updates the affection HUD
        /// </summary>
        public void UpdateAffectionDisplay(int currentAffection, int maxAffection)
        {
            if (affectionSlider != null)
            {
                affectionSlider.maxValue = maxAffection;
                affectionSlider.value = currentAffection;
            }

            if (affectionValueText != null)
            {
                affectionValueText.text = $"{currentAffection} / {maxAffection}";
            }
        }

        /// <summary>
        /// Updates the day counter display
        /// </summary>
        public void UpdateDayCounter(int day)
        {
            if (dayCounterText != null)
            {
                dayCounterText.text = $"Day {day}";
            }
        }

        /// <summary>
        /// Updates the scene name display
        /// </summary>
        public void UpdateSceneName(string sceneName)
        {
            if (sceneNameText != null)
            {
                sceneNameText.text = sceneName;
            }
        }

        /// <summary>
        /// Sets the background image
        /// </summary>
        public void SetBackground(Sprite background)
        {
            if (backgroundImage != null && background != null)
            {
                backgroundImage.sprite = background;
            }
        }

        /// <summary>
        /// Shows a specific panel and hides others
        /// </summary>
        public void ShowPanel(GameObject panel)
        {
            HideAllPanels();
            if (panel != null)
            {
                panel.SetActive(true);
            }
        }

        /// <summary>
        /// Hides all UI panels
        /// </summary>
        public void HideAllPanels()
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (gameplayPanel != null) gameplayPanel.SetActive(false);
            if (questionPanel != null) questionPanel.SetActive(false);
            if (resultPanel != null) resultPanel.SetActive(false);
        }

        /// <summary>
        /// Shows the gameplay panel
        /// </summary>
        public void ShowGameplayUI()
        {
            ShowPanel(gameplayPanel);
        }

        /// <summary>
        /// Shows the main menu
        /// </summary>
        public void ShowMainMenu()
        {
            ShowPanel(mainMenuPanel);
        }

        /// <summary>
        /// Updates dialogue progress indicator
        /// </summary>
        public void UpdateDialogueProgress(int current, int total)
        {
            if (dialogueProgressText != null)
            {
                dialogueProgressText.text = $"{current} / {total}";
            }
        }

        /// <summary>
        /// Gets the currently selected answer index
        /// </summary>
        public int GetSelectedAnswerIndex()
        {
            return selectedAnswerIndex;
        }

        /// <summary>
        /// Highlights the correct/wrong answer after submission
        /// </summary>
        public void HighlightAnswer(int correctIndex, int selectedIndex)
        {
            for (int i = 0; i < answerButtons.Length; i++)
            {
                var colors = answerButtons[i].colors;

                if (i == correctIndex)
                {
                    colors.normalColor = correctAnswerColor;
                }
                else if (i == selectedIndex && selectedIndex != correctIndex)
                {
                    colors.normalColor = wrongAnswerColor;
                }
                else
                {
                    colors.normalColor = normalButtonColor;
                }

                answerButtons[i].colors = colors;
            }

            // Disable all answer buttons after submission
            foreach (var button in answerButtons)
            {
                button.interactable = false;
            }

            if (submitAnswerButton != null)
            {
                submitAnswerButton.interactable = false;
            }
        }

        /// <summary>
        /// Re-enables answer buttons for next question
        /// </summary>
        public void ResetAnswerButtons()
        {
            foreach (var button in answerButtons)
            {
                button.interactable = true;

                var colors = button.colors;
                colors.normalColor = normalButtonColor;
                button.colors = colors;

                // Reset the visual state
                button.targetGraphic.color = normalButtonColor;
            }

            selectedAnswerIndex = -1;
        }
    }
}
