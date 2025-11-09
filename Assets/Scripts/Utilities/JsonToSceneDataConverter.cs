using UnityEngine;
using System;
using System.Collections.Generic;
using TimeLessLove.Data;

namespace TimeLessLove.Utilities
{
    /// <summary>
    /// Serializable JSON structure that mirrors PlayerSceneData
    /// Used for importing scene data from JSON files
    /// </summary>
    [Serializable]
    public class SceneDataJson
    {
        public string sceneName;
        public string displayName;
        public string backgroundSprite;
        public string backgroundMusic;
        public string[] dialogueLines;
        public QuestionDataJson[] questions;
        public int perfectAffectionBonus = 10;
        public int wrongAnswerPenalty = 5;
    }

    /// <summary>
    /// Serializable JSON structure that mirrors QuestionData
    /// </summary>
    [Serializable]
    public class QuestionDataJson
    {
        public string questionText;
        public string[] choices;
        public int correctAnswerIndex;
        public string correctResponse = "You remember! That makes me so happy!";
        public string wrongResponse = "You... you don't remember?";
    }

    /// <summary>
    /// Utility class to convert JSON data to PlayerSceneData ScriptableObjects
    /// </summary>
    public static class JsonToSceneDataConverter
    {
        /// <summary>
        /// Converts JSON string to SceneDataJson object
        /// </summary>
        public static SceneDataJson ParseJson(string jsonText)
        {
            try
            {
                SceneDataJson data = JsonUtility.FromJson<SceneDataJson>(jsonText);
                return data;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to parse JSON: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Converts SceneDataJson to PlayerSceneData ScriptableObject
        /// Note: This creates the data but doesn't handle sprite/audio loading
        /// </summary>
        public static PlayerSceneData ConvertToSceneData(SceneDataJson jsonData)
        {
            if (jsonData == null)
            {
                Debug.LogError("Cannot convert null JSON data");
                return null;
            }

            PlayerSceneData sceneData = ScriptableObject.CreateInstance<PlayerSceneData>();

            // Basic info
            sceneData.sceneName = jsonData.sceneName;
            sceneData.displayName = jsonData.displayName;

            // Note: Sprites and AudioClips need to be loaded from Resources or AssetDatabase
            // These will need to be assigned manually or loaded separately
            if (!string.IsNullOrEmpty(jsonData.backgroundSprite))
            {
                sceneData.backgroundSprite = Resources.Load<Sprite>(jsonData.backgroundSprite);
                if (sceneData.backgroundSprite == null)
                {
                    Debug.LogWarning($"Could not load sprite from Resources: {jsonData.backgroundSprite}");
                }
            }

            if (!string.IsNullOrEmpty(jsonData.backgroundMusic))
            {
                sceneData.backgroundMusic = Resources.Load<AudioClip>(jsonData.backgroundMusic);
                if (sceneData.backgroundMusic == null)
                {
                    Debug.LogWarning($"Could not load audio from Resources: {jsonData.backgroundMusic}");
                }
            }

            // Dialogue
            sceneData.dialogueLines = jsonData.dialogueLines ?? new string[0];

            // Questions
            sceneData.questions = new List<QuestionData>();
            if (jsonData.questions != null)
            {
                foreach (var questionJson in jsonData.questions)
                {
                    QuestionData question = new QuestionData
                    {
                        questionText = questionJson.questionText,
                        choices = questionJson.choices ?? new string[3],
                        correctAnswerIndex = questionJson.correctAnswerIndex,
                        correctResponse = questionJson.correctResponse,
                        wrongResponse = questionJson.wrongResponse
                    };
                    sceneData.questions.Add(question);
                }
            }

            // Rewards
            sceneData.perfectAffectionBonus = jsonData.perfectAffectionBonus;
            sceneData.wrongAnswerPenalty = jsonData.wrongAnswerPenalty;

            Debug.Log($"<color=green>Successfully converted JSON to SceneData: {sceneData.sceneName}</color>");
            return sceneData;
        }

        /// <summary>
        /// One-step conversion from JSON string to PlayerSceneData
        /// </summary>
        public static PlayerSceneData ConvertFromJsonString(string jsonText)
        {
            SceneDataJson jsonData = ParseJson(jsonText);
            if (jsonData == null) return null;

            return ConvertToSceneData(jsonData);
        }

        /// <summary>
        /// Converts PlayerSceneData to JSON string (for exporting)
        /// Note: Sprite and AudioClip paths are not included
        /// </summary>
        public static string ConvertToJsonString(PlayerSceneData sceneData, bool prettyPrint = true)
        {
            if (sceneData == null)
            {
                Debug.LogError("Cannot convert null SceneData");
                return null;
            }

            SceneDataJson jsonData = new SceneDataJson
            {
                sceneName = sceneData.sceneName,
                displayName = sceneData.displayName,
                backgroundSprite = sceneData.backgroundSprite != null ? sceneData.backgroundSprite.name : "",
                backgroundMusic = sceneData.backgroundMusic != null ? sceneData.backgroundMusic.name : "",
                dialogueLines = sceneData.dialogueLines,
                perfectAffectionBonus = sceneData.perfectAffectionBonus,
                wrongAnswerPenalty = sceneData.wrongAnswerPenalty
            };

            // Convert questions
            if (sceneData.questions != null)
            {
                jsonData.questions = new QuestionDataJson[sceneData.questions.Count];
                for (int i = 0; i < sceneData.questions.Count; i++)
                {
                    var question = sceneData.questions[i];
                    jsonData.questions[i] = new QuestionDataJson
                    {
                        questionText = question.questionText,
                        choices = question.choices,
                        correctAnswerIndex = question.correctAnswerIndex,
                        correctResponse = question.correctResponse,
                        wrongResponse = question.wrongResponse
                    };
                }
            }

            return JsonUtility.ToJson(jsonData, prettyPrint);
        }
    }
}
