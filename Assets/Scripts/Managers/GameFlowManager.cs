using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TimeLessLove.Data;
using TimeLessLove.Systems;

namespace TimeLessLove.Managers
{
    /// <summary>
    /// Main game flow controller: Dialogue → Questions → Results → Next Scene
    /// Orchestrates all managers to create the complete game loop
    /// </summary>
    public class GameFlowManager : MonoBehaviour
    {
        [Header("Manager References")]
        [SerializeField] private DialogueManager dialogueManager;
        [SerializeField] private UIManager uiManager;
        [SerializeField] private AffectionSystem affectionSystem;

        [Header("Gameplay Settings")]
        [SerializeField] private float delayAfterDialogue = 1f;
        [SerializeField] private float delayAfterQuestion = 1.5f;
        [SerializeField] private float delayBeforeNextScene = 2f;

        [Header("Question Flow")]
        [SerializeField] private bool askAllQuestions = true;
        [SerializeField] private int questionsPerScene = 3;

        // State
        private PlayerSceneData currentSceneData;
        private List<QuestionData> currentQuestions;
        private int currentQuestionIndex = 0;
        private int correctAnswersCount = 0;
        private bool isFirstDialogue = true; // Track if this is the first dialogue
        private bool isTransitioning = false; // Prevent multiple continue button clicks

        private enum GameState
        {
            Idle,
            Dialogue,
            Questions,
            Results,
            Transitioning
        }

        private GameState currentState = GameState.Idle;

        private void Awake()
        {
            // Get references if not assigned
            if (dialogueManager == null)
                dialogueManager = FindObjectOfType<DialogueManager>();

            if (uiManager == null)
                uiManager = FindObjectOfType<UIManager>();

            if (affectionSystem == null)
                affectionSystem = FindObjectOfType<AffectionSystem>();

            ValidateReferences();
        }

        private void Start()
        {
            SubscribeToEvents();
            StartGameLoop();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        /// <summary>
        /// Validates that all required managers are present
        /// </summary>
        private void ValidateReferences()
        {
            if (dialogueManager == null)
                Debug.LogError("GameFlowManager: DialogueManager not found!");

            if (uiManager == null)
                Debug.LogError("GameFlowManager: UIManager not found!");

            if (affectionSystem == null)
                Debug.LogError("GameFlowManager: AffectionSystem not found!");
        }

        /// <summary>
        /// Subscribes to events from other managers
        /// </summary>
        private void SubscribeToEvents()
        {
            if (dialogueManager != null)
            {
                dialogueManager.OnDialogueCompleted += OnDialogueCompleted;
                dialogueManager.OnLineChanged += uiManager.UpdateDialogueProgress;
            }

            if (uiManager != null)
            {
                uiManager.OnNextDialogueClicked += OnNextDialogueClicked;
                uiManager.OnSubmitAnswerClicked += OnAnswerSubmitted;
                uiManager.OnContinueClicked += OnContinueAfterResult;
            }

            if (affectionSystem != null)
            {
                affectionSystem.OnAffectionChanged += OnAffectionChanged;
                affectionSystem.OnGameOver += OnGameOver;
            }
        }

        /// <summary>
        /// Unsubscribes from events
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            if (dialogueManager != null)
            {
                dialogueManager.OnDialogueCompleted -= OnDialogueCompleted;
                dialogueManager.OnLineChanged -= uiManager.UpdateDialogueProgress;
            }

            if (uiManager != null)
            {
                uiManager.OnNextDialogueClicked -= OnNextDialogueClicked;
                uiManager.OnSubmitAnswerClicked -= OnAnswerSubmitted;
                uiManager.OnContinueClicked -= OnContinueAfterResult;
            }

            if (affectionSystem != null)
            {
                affectionSystem.OnAffectionChanged -= OnAffectionChanged;
                affectionSystem.OnGameOver -= OnGameOver;
            }
        }

        /// <summary>
        /// Starts the main game loop
        /// </summary>
        public void StartGameLoop()
        {
            // Get random scene from QuestBankManager
            currentSceneData = QuestBankManager.Instance.GetRandomSceneData();

            if (currentSceneData == null)
            {
                Debug.LogError("GameFlowManager: Failed to get scene data!");
                return;
            }

            // Setup scene
            SetupScene();

            // Start dialogue phase
            StartDialoguePhase();
        }

        /// <summary>
        /// Sets up the scene visuals and data
        /// </summary>
        private void SetupScene()
        {
            // Update UI
            uiManager.SetBackground(currentSceneData.backgroundSprite);
            uiManager.UpdateSceneName(currentSceneData.displayName);
            uiManager.UpdateDayCounter(QuestBankManager.Instance.CurrentDay);
            uiManager.UpdateAffectionDisplay(affectionSystem.CurrentAffection, affectionSystem.MaxAffection);

            // Play in-game music loop
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayInGameMusic(fade: true);
            }

            Debug.Log($"<color=cyan>Scene setup: {currentSceneData.sceneName}</color>");
        }

        /// <summary>
        /// Starts the dialogue phase
        /// </summary>
        private void StartDialoguePhase()
        {
            currentState = GameState.Dialogue;
            uiManager.ShowGameplayUI();

            if (currentSceneData.dialogueLines != null && currentSceneData.dialogueLines.Length > 0)
            {
                // Add delay only for the first dialogue
                if (isFirstDialogue)
                {
                    StartCoroutine(DelayedFirstDialogue());
                    isFirstDialogue = false; // Set to false after first time
                }
                else
                {
                    dialogueManager.StartDialogue(currentSceneData.dialogueLines);
                }
            }
            else
            {
                Debug.LogWarning("No dialogue lines found, skipping to questions.");
                OnDialogueCompleted();
            }
        }

        /// <summary>
        /// Coroutine to delay the first dialogue
        /// </summary>
        private IEnumerator DelayedFirstDialogue()
        {
            yield return new WaitForSeconds(0.15f);
            uiManager.EnableHUDPanel();
            yield return new WaitForSeconds(0.75f);
            uiManager.EnableDialoguePanel();
            yield return new WaitForSeconds(0.5f);
            dialogueManager.StartDialogue(currentSceneData.dialogueLines);
        }

        /// <summary>
        /// Called when next dialogue button is clicked
        /// </summary>
        private void OnNextDialogueClicked()
        {
            if (currentState == GameState.Dialogue)
            {
                dialogueManager.DisplayNextLine();
            }
        }

        /// <summary>
        /// Called when all dialogue is finished
        /// </summary>
        private void OnDialogueCompleted()
        {
            Debug.Log("<color=green>Dialogue phase completed</color>");
            StartCoroutine(TransitionToQuestions());
        }

        /// <summary>
        /// Transitions from dialogue to question phase
        /// </summary>
        private IEnumerator TransitionToQuestions()
        {
            yield return new WaitForSeconds(delayAfterDialogue);
            StartQuestionPhase();
        }

        /// <summary>
        /// Starts the question phase
        /// </summary>
        private void StartQuestionPhase()
        {
            currentState = GameState.Questions;
            currentQuestionIndex = 0;
            correctAnswersCount = 0;

            // Switch to questions music
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayQuestionsMusic(fade: true);
            }

            // Get questions from current scene
            currentQuestions = currentSceneData.questions;

            if (currentQuestions == null || currentQuestions.Count == 0)
            {
                Debug.LogWarning("No questions found, skipping to next scene.");
                EndQuestionPhase();
                return;
            }

            // Limit questions if needed
            if (!askAllQuestions && currentQuestions.Count > questionsPerScene)
            {
                currentQuestions = currentQuestions.GetRange(0, questionsPerScene);
            }

            // Display first question
            DisplayCurrentQuestion();
        }

        /// <summary>
        /// Displays the current question
        /// </summary>
        private void DisplayCurrentQuestion()
        {
            if (currentQuestionIndex < currentQuestions.Count)
            {
                QuestionData question = currentQuestions[currentQuestionIndex];
                uiManager.DisplayQuestion(question);
                uiManager.ResetAnswerButtons();

                Debug.Log($"<color=yellow>Question {currentQuestionIndex + 1}/{currentQuestions.Count}</color>");
            }
            else
            {
                EndQuestionPhase();
            }
        }

        /// <summary>
        /// Called when player submits their answer
        /// </summary>
        private void OnAnswerSubmitted()
        {
            int selectedIndex = uiManager.GetSelectedAnswerIndex();
            QuestionData currentQuestion = currentQuestions[currentQuestionIndex];

            bool isCorrect = currentQuestion.IsCorrect(selectedIndex);

            // Play sound effect based on answer
            if (AudioManager.Instance != null)
            {
                if (isCorrect)
                {
                    AudioManager.Instance.PlayCorrectAnswer();
                }
                else
                {
                    AudioManager.Instance.PlayWrongAnswer();
                }
            }

            // Update affection
            int affectionChange;
            if (isCorrect)
            {
                correctAnswersCount++;
                affectionChange = currentSceneData.perfectAffectionBonus;
                affectionSystem.AddAffection(affectionChange);
            }
            else
            {
                affectionChange = -currentSceneData.wrongAnswerPenalty;
                affectionSystem.SubtractAffection(currentSceneData.wrongAnswerPenalty);
            }

            // Visual feedback
            uiManager.HighlightAnswer(currentQuestion.correctAnswerIndex, selectedIndex);

            // Show result
            string responseText = isCorrect ? currentQuestion.correctResponse : currentQuestion.wrongResponse;
            uiManager.DisplayResult(isCorrect, responseText, isCorrect ? affectionChange : affectionChange);

            Debug.Log($"<color={(isCorrect ? "green" : "red")}>Answer: {(isCorrect ? "Correct" : "Wrong")}</color>");
        }

        /// <summary>
        /// Called when player clicks continue after seeing result
        /// </summary>
        private void OnContinueAfterResult()
        {
            // Prevent multiple clicks from triggering multiple transitions
            if (isTransitioning)
            {
                Debug.Log("Already transitioning, ignoring continue button click");
                return;
            }

            isTransitioning = true;
            currentQuestionIndex++;
            StartCoroutine(TransitionToNextQuestion());
        }

        /// <summary>
        /// Transitions to next question or ends phase
        /// </summary>
        private IEnumerator TransitionToNextQuestion()
        {
            yield return new WaitForSeconds(delayAfterQuestion);

            if (currentQuestionIndex < currentQuestions.Count)
            {
                DisplayCurrentQuestion();
                isTransitioning = false; // Reset flag when next question is displayed
            }
            else
            {
                EndQuestionPhase();
                isTransitioning = false; // Reset flag when phase ends
            }
        }

        /// <summary>
        /// Ends the question phase
        /// </summary>
        private void EndQuestionPhase()
        {
            Debug.Log($"<color=green>Question phase completed: {correctAnswersCount}/{currentQuestions.Count} correct</color>");
            StartCoroutine(TransitionToNextScene());
        }

        /// <summary>
        /// Transitions to the next scene
        /// </summary>
        private IEnumerator TransitionToNextScene()
        {
            currentState = GameState.Transitioning;

            yield return new WaitForSeconds(delayBeforeNextScene);

            // Check if affection is too low for game over
            if (affectionSystem.CurrentAffection <= 0)
            {
                // Game Over handled by OnGameOver event
                yield break;
            }

            // Switch back to in-game music for next scene
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayInGameMusic(fade: true);
            }

            // Load next random scene
            StartGameLoop();
        }

        /// <summary>
        /// Called when affection changes
        /// </summary>
        private void OnAffectionChanged(int newValue)
        {
            uiManager.UpdateAffectionDisplay(newValue, affectionSystem.MaxAffection);
        }

        /// <summary>
        /// Called when affection reaches 0
        /// </summary>
        private void OnGameOver()
        {
            Debug.LogWarning("<color=red>Game Over: Aria has forgotten you...</color>");

            // Load game over scene or show game over screen
            if (TransitionSceneManager.Instance != null)
            {
                TransitionSceneManager.Instance.LoadScene("GameOver");
            }
        }

        /// <summary>
        /// Restarts the game
        /// </summary>
        public void RestartGame()
        {
            affectionSystem.ResetAffection();
            QuestBankManager.Instance.ResetDailyProgress();
            StartGameLoop();
        }

        /// <summary>
        /// Gets current game state info
        /// </summary>
        public string GetGameStateInfo()
        {
            return $"State: {currentState} | Scene: {currentSceneData?.sceneName} | Question: {currentQuestionIndex + 1}/{currentQuestions?.Count ?? 0}";
        }
    }
}
