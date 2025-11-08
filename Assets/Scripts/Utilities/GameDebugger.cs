using UnityEngine;
using TimeLessLove.Managers;
using TimeLessLove.Systems;

namespace TimeLessLove.Utilities
{
    /// <summary>
    /// Debug utility for testing game systems in the editor
    /// Press keys to trigger various game actions
    /// </summary>
    public class GameDebugger : MonoBehaviour
    {
        [Header("Debug Settings")]
        [SerializeField] private bool enableDebugMode = true;
        [SerializeField] private KeyCode addAffectionKey = KeyCode.Plus;
        [SerializeField] private KeyCode subtractAffectionKey = KeyCode.Minus;
        [SerializeField] private KeyCode skipDialogueKey = KeyCode.S;
        [SerializeField] private KeyCode nextSceneKey = KeyCode.N;
        [SerializeField] private KeyCode printInfoKey = KeyCode.I;

        private AffectionSystem affectionSystem;
        private DialogueManager dialogueManager;
        private GameFlowManager gameFlowManager;

        private void Start()
        {
            if (!enableDebugMode) return;

            affectionSystem = FindObjectOfType<AffectionSystem>();
            dialogueManager = FindObjectOfType<DialogueManager>();
            gameFlowManager = FindObjectOfType<GameFlowManager>();

            Debug.Log("<color=cyan>GameDebugger active! Press 'I' for debug info.</color>");
        }

        private void Update()
        {
            if (!enableDebugMode) return;

#if UNITY_EDITOR
            HandleDebugInput();
#endif
        }

        private void HandleDebugInput()
        {
            // Add affection
            if (Input.GetKeyDown(addAffectionKey))
            {
                if (affectionSystem != null)
                {
                    affectionSystem.AddAffection(10);
                    Debug.Log($"<color=green>+10 Affection (Total: {affectionSystem.CurrentAffection})</color>");
                }
            }

            // Subtract affection
            if (Input.GetKeyDown(subtractAffectionKey))
            {
                if (affectionSystem != null)
                {
                    affectionSystem.SubtractAffection(10);
                    Debug.Log($"<color=red>-10 Affection (Total: {affectionSystem.CurrentAffection})</color>");
                }
            }

            // Skip dialogue
            if (Input.GetKeyDown(skipDialogueKey))
            {
                if (dialogueManager != null && dialogueManager.IsActive)
                {
                    dialogueManager.SkipDialogue();
                    Debug.Log("<color=yellow>Dialogue skipped</color>");
                }
            }

            // Load next scene
            if (Input.GetKeyDown(nextSceneKey))
            {
                if (gameFlowManager != null)
                {
                    gameFlowManager.StartGameLoop();
                    Debug.Log("<color=cyan>Loading next random scene...</color>");
                }
            }

            // Print debug info
            if (Input.GetKeyDown(printInfoKey))
            {
                PrintDebugInfo();
            }
        }

        private void PrintDebugInfo()
        {
            Debug.Log("========== GAME DEBUG INFO ==========");

            if (affectionSystem != null)
            {
                Debug.Log($"Affection: {affectionSystem.CurrentAffection}/{affectionSystem.MaxAffection} ({affectionSystem.AffectionPercentage * 100:F1}%)");
                Debug.Log($"Affection Tier: {affectionSystem.GetAffectionTier()}");
            }

            if (QuestBankManager.Instance != null)
            {
                Debug.Log(QuestBankManager.Instance.GetDebugInfo());
            }

            if (gameFlowManager != null)
            {
                Debug.Log(gameFlowManager.GetGameStateInfo());
            }

            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.DebugPrintSaveData();
            }

            Debug.Log("====================================");
        }

        [ContextMenu("Add 10 Affection")]
        private void DebugAddAffection()
        {
            if (affectionSystem != null)
            {
                affectionSystem.AddAffection(10);
            }
        }

        [ContextMenu("Subtract 10 Affection")]
        private void DebugSubtractAffection()
        {
            if (affectionSystem != null)
            {
                affectionSystem.SubtractAffection(10);
            }
        }

        [ContextMenu("Reset Affection")]
        private void DebugResetAffection()
        {
            if (affectionSystem != null)
            {
                affectionSystem.ResetAffection();
            }
        }

        [ContextMenu("Print Debug Info")]
        private void DebugPrintInfo()
        {
            PrintDebugInfo();
        }

        [ContextMenu("Save Game")]
        private void DebugSaveGame()
        {
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.SaveGame();
            }
        }

        [ContextMenu("Load Game")]
        private void DebugLoadGame()
        {
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.LoadGame();
            }
        }

        [ContextMenu("Delete Save Data")]
        private void DebugDeleteSave()
        {
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.DeleteSaveData();
            }
        }
    }
}