using UnityEngine;
using System;

namespace TimeLessLove.Systems
{
    /// <summary>
    /// Manages player's affection level with Aria (0-100)
    /// Triggers events at key thresholds for game endings
    /// </summary>
    public class AffectionSystem : MonoBehaviour
    {
        [Header("Affection Settings")]
        [SerializeField] private int maxAffection = 100;
        [SerializeField] private int startingAffection = 50;

        [Header("Current State")]
        [SerializeField] private int currentAffection;

        // Affection level thresholds
        private const int PERFECT_LOVE_THRESHOLD = 90;
        private const int HAPPY_ENDING_THRESHOLD = 70;
        private const int NEUTRAL_THRESHOLD = 40;
        private const int FADING_MEMORY_THRESHOLD = 20;

        // Events for affection changes
        public event Action<int> OnAffectionChanged;
        public event Action<int, int> OnAffectionUpdated; // (oldValue, newValue)

        // Threshold events
        public event Action OnPerfectLoveReached;
        public event Action OnHappyEndingReached;
        public event Action OnNeutralReached;
        public event Action OnFadingMemoryReached;
        public event Action OnGameOver; // Affection reached 0

        // Properties
        public int CurrentAffection => currentAffection;
        public int MaxAffection => maxAffection;
        public float AffectionPercentage => (float)currentAffection / maxAffection;

        private void Awake()
        {
            currentAffection = startingAffection;
        }

        /// <summary>
        /// Adds affection and triggers appropriate events
        /// </summary>
        public void AddAffection(int value)
        {
            if (value <= 0) return;

            int oldValue = currentAffection;
            currentAffection = Mathf.Min(currentAffection + value, maxAffection);

            OnAffectionChanged?.Invoke(currentAffection);
            OnAffectionUpdated?.Invoke(oldValue, currentAffection);

            CheckThresholds();

            Debug.Log($"<color=green>Affection increased by {value}. Current: {currentAffection}/{maxAffection}</color>");
        }

        /// <summary>
        /// Subtracts affection and checks for game over
        /// </summary>
        public void SubtractAffection(int value)
        {
            if (value <= 0) return;

            int oldValue = currentAffection;
            currentAffection = Mathf.Max(currentAffection - value, 0);

            OnAffectionChanged?.Invoke(currentAffection);
            OnAffectionUpdated?.Invoke(oldValue, currentAffection);

            // Check for game over
            if (currentAffection <= 0)
            {
                OnGameOver?.Invoke();
                Debug.LogWarning("<color=red>Game Over: Affection reached 0!</color>");
            }
            else
            {
                CheckThresholds();
            }

            Debug.Log($"<color=red>Affection decreased by {value}. Current: {currentAffection}/{maxAffection}</color>");
        }

        /// <summary>
        /// Sets affection to a specific value (use for loading saves)
        /// </summary>
        public void SetAffection(int value)
        {
            int oldValue = currentAffection;
            currentAffection = Mathf.Clamp(value, 0, maxAffection);

            OnAffectionChanged?.Invoke(currentAffection);
            OnAffectionUpdated?.Invoke(oldValue, currentAffection);

            CheckThresholds();
        }

        /// <summary>
        /// Checks if affection has crossed any important thresholds
        /// </summary>
        private void CheckThresholds()
        {
            if (currentAffection >= PERFECT_LOVE_THRESHOLD)
            {
                OnPerfectLoveReached?.Invoke();
            }
            else if (currentAffection >= HAPPY_ENDING_THRESHOLD)
            {
                OnHappyEndingReached?.Invoke();
            }
            else if (currentAffection >= NEUTRAL_THRESHOLD)
            {
                OnNeutralReached?.Invoke();
            }
            else if (currentAffection > 0 && currentAffection <= FADING_MEMORY_THRESHOLD)
            {
                OnFadingMemoryReached?.Invoke();
            }
        }

        /// <summary>
        /// Returns the current affection tier as a string
        /// </summary>
        public string GetAffectionTier()
        {
            if (currentAffection >= PERFECT_LOVE_THRESHOLD)
                return "Perfect Love";
            else if (currentAffection >= HAPPY_ENDING_THRESHOLD)
                return "Happy Ending";
            else if (currentAffection >= NEUTRAL_THRESHOLD)
                return "Neutral";
            else if (currentAffection > 0)
                return "Fading Memory";
            else
                return "Forgotten";
        }

        /// <summary>
        /// Resets affection to starting value
        /// </summary>
        public void ResetAffection()
        {
            SetAffection(startingAffection);
            Debug.Log($"Affection reset to {startingAffection}");
        }
    }
}
