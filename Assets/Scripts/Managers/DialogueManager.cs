using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System;

namespace TimeLessLove.Managers
{
    /// <summary>
    /// Handles displaying Aria's dialogue with typewriter effect
    /// Integrates with UIManager and triggers question phase after dialogue ends
    /// </summary>
    public class DialogueManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private TextMeshProUGUI characterNameText;
        [SerializeField] private Image characterPortrait;

        [Header("Typewriter Settings")]
        [SerializeField] private float typeSpeed = 0.05f;
        [SerializeField] private bool allowSkip = true;
        [SerializeField] private AudioClip typeSound;
        [SerializeField] private float typeSoundInterval = 0.1f;

        [Header("Character Settings")]
        [SerializeField] private string characterName = "Aria";
        [SerializeField] private Sprite defaultPortrait;

        // State tracking
        private string[] currentDialogueLines;
        private int currentLineIndex = 0;
        private bool isTyping = false;
        private bool skipTyping = false;
        private Coroutine typewriterCoroutine;

        // Events
        public event Action OnDialogueStarted;
        public event Action<int, int> OnLineChanged; // (currentLine, totalLines)
        public event Action OnDialogueCompleted;
        public event Action OnLineCompleted;

        // Properties
        public bool IsActive => dialoguePanel != null && dialoguePanel.activeSelf;
        public bool IsTyping => isTyping;
        public int CurrentLineIndex => currentLineIndex;
        public int TotalLines => currentDialogueLines?.Length ?? 0;

        private void Awake()
        {
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
            }

            if (characterNameText != null)
            {
                characterNameText.text = characterName;
            }

            if (characterPortrait != null && defaultPortrait != null)
            {
                characterPortrait.sprite = defaultPortrait;
            }
        }

        /// <summary>
        /// Starts dialogue sequence with provided lines
        /// </summary>
        public void StartDialogue(string[] dialogueLines, Sprite portrait = null)
        {
            if (dialogueLines == null || dialogueLines.Length == 0)
            {
                Debug.LogError("DialogueManager: No dialogue lines provided!");
                return;
            }

            currentDialogueLines = dialogueLines;
            currentLineIndex = 0;

            // Set portrait if provided
            if (portrait != null && characterPortrait != null)
            {
                characterPortrait.sprite = portrait;
            }

            // Show dialogue panel
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(true);
            }

            OnDialogueStarted?.Invoke();

            // Display first line
            DisplayNextLine();

            Debug.Log($"<color=cyan>Dialogue started: {dialogueLines.Length} lines</color>");
        }

        /// <summary>
        /// Displays the next dialogue line
        /// </summary>
        public void DisplayNextLine()
        {
            // If currently typing, complete the line instantly
            if (isTyping)
            {
                if (allowSkip)
                {
                    skipTyping = true;
                }
                return;
            }

            // Check if dialogue is complete
            if (currentLineIndex >= currentDialogueLines.Length)
            {
                EndDialogue();
                return;
            }

            // Display current line
            string line = currentDialogueLines[currentLineIndex];
            OnLineChanged?.Invoke(currentLineIndex + 1, currentDialogueLines.Length);

            if (typewriterCoroutine != null)
            {
                StopCoroutine(typewriterCoroutine);
            }

            typewriterCoroutine = StartCoroutine(TypewriterEffect(line));
        }

        /// <summary>
        /// Typewriter effect coroutine
        /// </summary>
        private IEnumerator TypewriterEffect(string line)
        {
            isTyping = true;
            skipTyping = false;
            dialogueText.text = "";

            float timeSinceLastSound = 0f;

            for (int i = 0; i < line.Length; i++)
            {
                // Check for skip
                if (skipTyping)
                {
                    dialogueText.text = line;
                    break;
                }

                dialogueText.text += line[i];

                // Play typing sound
                if (typeSound != null)
                {
                    timeSinceLastSound += typeSpeed;
                    if (timeSinceLastSound >= typeSoundInterval)
                    {
                        AudioSource.PlayClipAtPoint(typeSound, Camera.main.transform.position, 0.3f);
                        timeSinceLastSound = 0f;
                    }
                }

                yield return new WaitForSeconds(typeSpeed);
            }

            isTyping = false;
            currentLineIndex++;
            OnLineCompleted?.Invoke();
        }

        /// <summary>
        /// Immediately shows the full current line
        /// </summary>
        public void CompleteCurrentLine()
        {
            if (isTyping)
            {
                skipTyping = true;
            }
        }

        /// <summary>
        /// Ends the dialogue and triggers completion event
        /// </summary>
        public void EndDialogue()
        {
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
            }

            OnDialogueCompleted?.Invoke();

            Debug.Log("<color=green>Dialogue completed!</color>");
        }

        /// <summary>
        /// Skips to the end of dialogue immediately
        /// </summary>
        public void SkipDialogue()
        {
            if (typewriterCoroutine != null)
            {
                StopCoroutine(typewriterCoroutine);
            }

            isTyping = false;
            currentLineIndex = currentDialogueLines.Length;

            EndDialogue();
        }

        /// <summary>
        /// Sets the character name displayed
        /// </summary>
        public void SetCharacterName(string name)
        {
            characterName = name;
            if (characterNameText != null)
            {
                characterNameText.text = name;
            }
        }

        /// <summary>
        /// Sets the character portrait
        /// </summary>
        public void SetCharacterPortrait(Sprite portrait)
        {
            if (characterPortrait != null && portrait != null)
            {
                characterPortrait.sprite = portrait;
            }
        }

        /// <summary>
        /// Pauses the dialogue (stops typewriter)
        /// </summary>
        public void PauseDialogue()
        {
            if (typewriterCoroutine != null)
            {
                StopCoroutine(typewriterCoroutine);
                isTyping = false;
            }
        }

        /// <summary>
        /// Resumes dialogue from current position
        /// </summary>
        public void ResumeDialogue()
        {
            if (!isTyping && currentLineIndex < currentDialogueLines.Length)
            {
                DisplayNextLine();
            }
        }

        /// <summary>
        /// Gets the current progress as percentage
        /// </summary>
        public float GetProgress()
        {
            if (currentDialogueLines == null || currentDialogueLines.Length == 0)
                return 0f;

            return (float)currentLineIndex / currentDialogueLines.Length;
        }

        /// <summary>
        /// Displays a single line of dialogue (useful for questions/responses)
        /// </summary>
        public void ShowSingleLine(string text, Action onComplete = null)
        {
            StartCoroutine(ShowSingleLineCoroutine(text, onComplete));
        }

        private IEnumerator ShowSingleLineCoroutine(string text, Action onComplete)
        {
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(true);
            }

            yield return TypewriterEffect(text);

            onComplete?.Invoke();
        }
    }
}
