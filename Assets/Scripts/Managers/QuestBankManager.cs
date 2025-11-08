using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TimeLessLove.Data;

namespace TimeLessLove.Managers
{
    /// <summary>
    /// Singleton manager that stores all scene data and manages scene selection
    /// Persists across scene loads using DontDestroyOnLoad
    /// </summary>
    public class QuestBankManager : MonoBehaviour
    {
        private static QuestBankManager instance;
        public static QuestBankManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<QuestBankManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("QuestBankManager");
                        instance = go.AddComponent<QuestBankManager>();
                    }
                }
                return instance;
            }
        }

        [Header("Scene Data")]
        [SerializeField] private List<PlayerSceneData> allSceneData = new List<PlayerSceneData>();

        [Header("Scene Tracking")]
        [SerializeField] private List<string> playedScenesToday = new List<string>();
        [SerializeField] private int currentDayNumber = 1;

        private PlayerSceneData currentScene;
        private Queue<PlayerSceneData> dailySceneQueue = new Queue<PlayerSceneData>();

        // Properties
        public PlayerSceneData CurrentScene => currentScene;
        public int CurrentDay => currentDayNumber;
        public int TotalScenes => allSceneData.Count;

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

            ValidateSceneData();
        }

        /// <summary>
        /// Validates that all scene data is properly configured
        /// </summary>
        private void ValidateSceneData()
        {
            if (allSceneData == null || allSceneData.Count == 0)
            {
                Debug.LogError("QuestBankManager: No scene data assigned! Please add PlayerSceneData assets.");
                return;
            }

            foreach (var scene in allSceneData)
            {
                if (scene == null)
                {
                    Debug.LogWarning("QuestBankManager: Null scene data found in list!");
                    continue;
                }

                if (scene.questions.Count == 0)
                {
                    Debug.LogWarning($"QuestBankManager: Scene '{scene.sceneName}' has no questions!");
                }
            }

            Debug.Log($"QuestBankManager initialized with {allSceneData.Count} scenes.");
        }

        /// <summary>
        /// Gets a random scene that hasn't been played today
        /// </summary>
        public PlayerSceneData GetRandomSceneData()
        {
            // Filter out scenes already played today
            List<PlayerSceneData> availableScenes = allSceneData
                .Where(scene => !playedScenesToday.Contains(scene.sceneName))
                .ToList();

            // If all scenes played today, reset the day
            if (availableScenes.Count == 0)
            {
                playedScenesToday.Clear();
                currentDayNumber++;
                availableScenes = new List<PlayerSceneData>(allSceneData);
                Debug.Log($"<color=cyan>Day {currentDayNumber} started! All scenes reset.</color>");
            }

            // Pick random scene
            int randomIndex = Random.Range(0, availableScenes.Count);
            currentScene = availableScenes[randomIndex];

            // Mark as played
            playedScenesToday.Add(currentScene.sceneName);

            Debug.Log($"Selected scene: {currentScene.sceneName} ({playedScenesToday.Count}/{allSceneData.Count} played today)");

            return currentScene;
        }

        /// <summary>
        /// Gets a specific scene by name
        /// </summary>
        public PlayerSceneData GetSceneByName(string sceneName)
        {
            PlayerSceneData scene = allSceneData.FirstOrDefault(s => s.sceneName == sceneName);

            if (scene == null)
            {
                Debug.LogError($"QuestBankManager: Scene '{sceneName}' not found!");
            }
            else
            {
                currentScene = scene;
            }

            return scene;
        }

        /// <summary>
        /// Gets all questions for a specific scene
        /// </summary>
        public List<QuestionData> GetQuestions(string sceneName)
        {
            PlayerSceneData scene = GetSceneByName(sceneName);
            return scene?.questions ?? new List<QuestionData>();
        }

        /// <summary>
        /// Gets all questions for the current scene
        /// </summary>
        public List<QuestionData> GetCurrentQuestions()
        {
            if (currentScene == null)
            {
                Debug.LogWarning("QuestBankManager: No current scene set!");
                return new List<QuestionData>();
            }

            return currentScene.questions;
        }

        /// <summary>
        /// Shuffles and returns a queue of all scenes for daily rotation
        /// </summary>
        public Queue<PlayerSceneData> GetShuffledDailyScenes()
        {
            List<PlayerSceneData> shuffled = new List<PlayerSceneData>(allSceneData);

            // Fisher-Yates shuffle
            for (int i = shuffled.Count - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);
                (shuffled[i], shuffled[randomIndex]) = (shuffled[randomIndex], shuffled[i]);
            }

            dailySceneQueue = new Queue<PlayerSceneData>(shuffled);
            return dailySceneQueue;
        }

        /// <summary>
        /// Gets the next scene from the daily queue
        /// </summary>
        public PlayerSceneData GetNextSceneFromQueue()
        {
            if (dailySceneQueue.Count == 0)
            {
                GetShuffledDailyScenes();
                currentDayNumber++;
            }

            currentScene = dailySceneQueue.Dequeue();
            return currentScene;
        }

        /// <summary>
        /// Adds a new scene data to the bank (for DLC or updates)
        /// </summary>
        public void AddSceneData(PlayerSceneData newScene)
        {
            if (newScene == null)
            {
                Debug.LogError("Cannot add null scene data!");
                return;
            }

            if (allSceneData.Contains(newScene))
            {
                Debug.LogWarning($"Scene '{newScene.sceneName}' already exists in bank!");
                return;
            }

            allSceneData.Add(newScene);
            Debug.Log($"Added new scene: {newScene.sceneName}");
        }

        /// <summary>
        /// Resets daily progress (for debugging or new game)
        /// </summary>
        public void ResetDailyProgress()
        {
            playedScenesToday.Clear();
            currentDayNumber = 1;
            currentScene = null;
            dailySceneQueue.Clear();
            Debug.Log("Daily progress reset.");
        }

        /// <summary>
        /// Gets debug info about current state
        /// </summary>
        public string GetDebugInfo()
        {
            return $"Day: {currentDayNumber} | Scenes Played: {playedScenesToday.Count}/{allSceneData.Count} | Current: {currentScene?.sceneName ?? "None"}";
        }
    }
}
