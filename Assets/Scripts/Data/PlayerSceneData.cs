using UnityEngine;
using System.Collections.Generic;

namespace TimeLessLove.Data
{
    /// <summary>
    /// ScriptableObject that holds all data for a single scene including background, music, and questions
    /// </summary>
    [CreateAssetMenu(fileName = "SceneData", menuName = "TimeLessLove/PlayerSceneData")]
    public class PlayerSceneData : ScriptableObject
    {
        [Header("Scene Information")]
        [Tooltip("Unique identifier for this scene")]
        public string sceneName;

        [Tooltip("Display name shown to player")]
        public string displayName;

        [Header("Visual & Audio")]
        [Tooltip("Background sprite for this scene")]
        public Sprite backgroundSprite;

        [Tooltip("Background music for this scene")]
        public AudioClip backgroundMusic;

        [Header("Dialogue")]
        [Tooltip("All dialogue lines Aria says in this scene")]
        [TextArea(3, 10)]
        public string[] dialogueLines;

        [Header("Questions")]
        [Tooltip("Memory questions for this scene")]
        public List<QuestionData> questions = new List<QuestionData>();

        [Header("Rewards")]
        [Tooltip("Affection gained for perfect answers")]
        [Range(0, 20)]
        public int perfectAffectionBonus = 10;

        [Tooltip("Affection lost for wrong answers")]
        [Range(0, 20)]
        public int wrongAnswerPenalty = 10;
    }

    /// <summary>
    /// Represents a single memory question with multiple choices
    /// </summary>
    [System.Serializable]
    public class QuestionData
    {
        [Tooltip("The question text Aria asks")]
        [TextArea(2, 5)]
        public string questionText;

        [Tooltip("Multiple choice answers (3-4 recommended)")]
        public string[] choices = new string[3];

        [Tooltip("Index of the correct answer (0-based)")]
        [Range(0, 3)]
        public int correctAnswerIndex;

        [Tooltip("Optional: Aria's response when player gets it right")]
        [TextArea(2, 3)]
        public string correctResponse = "You remember! That makes me so happy!";

        [Tooltip("Optional: Aria's response when player gets it wrong")]
        [TextArea(2, 3)]
        public string wrongResponse = "You... you don't remember?";

        /// <summary>
        /// Validates if the player's answer is correct
        /// </summary>
        public bool IsCorrect(int selectedIndex)
        {
            return selectedIndex == correctAnswerIndex;
        }
    }
}
