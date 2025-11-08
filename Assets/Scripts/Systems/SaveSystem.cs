using UnityEngine;
using System;
using System.Collections.Generic;

namespace TimeLessLove.Systems
{
    /// <summary>
    /// Handles saving and loading game data using PlayerPrefs
    /// Stores affection, day progress, and scene history
    /// </summary>
    public class SaveSystem : MonoBehaviour
    {
        private static SaveSystem instance;
        public static SaveSystem Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<SaveSystem>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("SaveSystem");
                        instance = go.AddComponent<SaveSystem>();
                    }
                }
                return instance;
            }
        }

        // PlayerPrefs keys
        private const string KEY_AFFECTION = "TimeLessLove_Affection";
        private const string KEY_DAY = "TimeLessLove_Day";
        private const string KEY_LAST_SCENE = "TimeLessLove_LastScene";
        private const string KEY_PLAYED_SCENES = "TimeLessLove_PlayedScenes";
        private const string KEY_MUSIC_VOLUME = "TimeLessLove_MusicVolume";
        private const string KEY_SFX_VOLUME = "TimeLessLove_SFXVolume";
        private const string KEY_FIRST_LAUNCH = "TimeLessLove_FirstLaunch";

        [Header("Auto Save Settings")]
        [SerializeField] private bool autoSave = true;
        [SerializeField] private float autoSaveInterval = 60f; // seconds

        private float autoSaveTimer = 0f;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            if (autoSave)
            {
                autoSaveTimer += Time.deltaTime;
                if (autoSaveTimer >= autoSaveInterval)
                {
                    SaveGame();
                    autoSaveTimer = 0f;
                }
            }
        }

        /// <summary>
        /// Saves all game data
        /// </summary>
        public void SaveGame()
        {
            try
            {
                // Save affection
                AffectionSystem affection = FindObjectOfType<AffectionSystem>();
                if (affection != null)
                {
                    PlayerPrefs.SetInt(KEY_AFFECTION, affection.CurrentAffection);
                }

                // Save day progress
                var questBank = Managers.QuestBankManager.Instance;
                if (questBank != null)
                {
                    PlayerPrefs.SetInt(KEY_DAY, questBank.CurrentDay);

                    if (questBank.CurrentScene != null)
                    {
                        PlayerPrefs.SetString(KEY_LAST_SCENE, questBank.CurrentScene.sceneName);
                    }
                }

                PlayerPrefs.Save();
                Debug.Log("<color=green>Game saved successfully!</color>");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save game: {e.Message}");
            }
        }

        /// <summary>
        /// Loads all game data
        /// </summary>
        public void LoadGame()
        {
            try
            {
                // Load affection
                if (PlayerPrefs.HasKey(KEY_AFFECTION))
                {
                    int savedAffection = PlayerPrefs.GetInt(KEY_AFFECTION);
                    AffectionSystem affection = FindObjectOfType<AffectionSystem>();
                    if (affection != null)
                    {
                        affection.SetAffection(savedAffection);
                    }
                }

                // Load day progress
                if (PlayerPrefs.HasKey(KEY_DAY))
                {
                    int savedDay = PlayerPrefs.GetInt(KEY_DAY);
                    // QuestBankManager will use this when initialized
                }

                Debug.Log("<color=green>Game loaded successfully!</color>");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load game: {e.Message}");
            }
        }

        /// <summary>
        /// Saves audio settings
        /// </summary>
        public void SaveAudioSettings(float musicVolume, float sfxVolume)
        {
            PlayerPrefs.SetFloat(KEY_MUSIC_VOLUME, musicVolume);
            PlayerPrefs.SetFloat(KEY_SFX_VOLUME, sfxVolume);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Loads audio settings
        /// </summary>
        public void LoadAudioSettings(out float musicVolume, out float sfxVolume)
        {
            musicVolume = PlayerPrefs.GetFloat(KEY_MUSIC_VOLUME, 0.7f);
            sfxVolume = PlayerPrefs.GetFloat(KEY_SFX_VOLUME, 1f);
        }

        /// <summary>
        /// Checks if this is the first time launching the game
        /// </summary>
        public bool IsFirstLaunch()
        {
            bool isFirst = !PlayerPrefs.HasKey(KEY_FIRST_LAUNCH);
            if (isFirst)
            {
                PlayerPrefs.SetInt(KEY_FIRST_LAUNCH, 1);
                PlayerPrefs.Save();
            }
            return isFirst;
        }

        /// <summary>
        /// Checks if a save file exists
        /// </summary>
        public bool HasSaveData()
        {
            return PlayerPrefs.HasKey(KEY_AFFECTION);
        }

        /// <summary>
        /// Deletes all save data (New Game)
        /// </summary>
        public void DeleteSaveData()
        {
            PlayerPrefs.DeleteKey(KEY_AFFECTION);
            PlayerPrefs.DeleteKey(KEY_DAY);
            PlayerPrefs.DeleteKey(KEY_LAST_SCENE);
            PlayerPrefs.DeleteKey(KEY_PLAYED_SCENES);
            PlayerPrefs.Save();

            Debug.Log("<color=yellow>Save data deleted.</color>");
        }

        /// <summary>
        /// Deletes ALL PlayerPrefs data (including settings)
        /// </summary>
        public void DeleteAllData()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();

            Debug.Log("<color=red>All data deleted!</color>");
        }

        /// <summary>
        /// Gets saved affection value
        /// </summary>
        public int GetSavedAffection()
        {
            return PlayerPrefs.GetInt(KEY_AFFECTION, 50); // Default 50
        }

        /// <summary>
        /// Gets saved day number
        /// </summary>
        public int GetSavedDay()
        {
            return PlayerPrefs.GetInt(KEY_DAY, 1); // Default day 1
        }

        /// <summary>
        /// Gets last played scene name
        /// </summary>
        public string GetLastSceneName()
        {
            return PlayerPrefs.GetString(KEY_LAST_SCENE, "");
        }

        /// <summary>
        /// Save data structure for JSON serialization (future expansion)
        /// </summary>
        [Serializable]
        public class SaveData
        {
            public int affection;
            public int day;
            public string lastScene;
            public List<string> playedScenes;
            public float musicVolume;
            public float sfxVolume;
            public string saveDate;

            public SaveData()
            {
                playedScenes = new List<string>();
                saveDate = DateTime.Now.ToString();
            }
        }

        /// <summary>
        /// Creates a save data snapshot
        /// </summary>
        public SaveData CreateSaveDataSnapshot()
        {
            SaveData data = new SaveData();

            AffectionSystem affection = FindObjectOfType<AffectionSystem>();
            if (affection != null)
            {
                data.affection = affection.CurrentAffection;
            }

            var questBank = Managers.QuestBankManager.Instance;
            if (questBank != null)
            {
                data.day = questBank.CurrentDay;
                data.lastScene = questBank.CurrentScene?.sceneName ?? "";
            }

            var audioManager = Managers.AudioManager.Instance;
            if (audioManager != null)
            {
                data.musicVolume = audioManager.GetMusicVolume();
                data.sfxVolume = audioManager.GetSFXVolume();
            }

            return data;
        }

        /// <summary>
        /// Debug: Logs all saved values
        /// </summary>
        public void DebugPrintSaveData()
        {
            Debug.Log("=== SAVE DATA ===");
            Debug.Log($"Affection: {GetSavedAffection()}");
            Debug.Log($"Day: {GetSavedDay()}");
            Debug.Log($"Last Scene: {GetLastSceneName()}");
            Debug.Log($"Music Volume: {PlayerPrefs.GetFloat(KEY_MUSIC_VOLUME, 0.7f)}");
            Debug.Log($"SFX Volume: {PlayerPrefs.GetFloat(KEY_SFX_VOLUME, 1f)}");
            Debug.Log("================");
        }
    }
}
