using UnityEngine;
using UnityEditor;
using System.IO;
using TimeLessLove.Data;
using TimeLessLove.Utilities;

namespace TimeLessLove.Editor
{
    /// <summary>
    /// Unity Editor window for converting JSON files to PlayerSceneData ScriptableObjects
    /// Access via: Tools > TimeLessLove > Scene Data Converter
    /// </summary>
    public class SceneDataJsonConverterEditor : EditorWindow
    {
        private TextAsset jsonFile;
        private string jsonText = "";
        private PlayerSceneData selectedSceneData;
        private string outputPath = "Assets/ScriptableObjects/SceneData/";
        private Vector2 scrollPosition;

        [MenuItem("Tools/TimeLessLove/Scene Data Converter")]
        public static void ShowWindow()
        {
            var window = GetWindow<SceneDataJsonConverterEditor>("Scene Data Converter");
            window.minSize = new Vector2(500, 600);
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            GUILayout.Label("JSON to PlayerSceneData Converter", EditorStyles.boldLabel);
            GUILayout.Space(10);

            // JSON Import Section
            EditorGUILayout.LabelField("Import from JSON", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Convert JSON files to PlayerSceneData ScriptableObjects", MessageType.Info);

            jsonFile = (TextAsset)EditorGUILayout.ObjectField("JSON File", jsonFile, typeof(TextAsset), false);

            if (jsonFile != null)
            {
                jsonText = jsonFile.text;
            }

            EditorGUILayout.LabelField("Or paste JSON directly:");
            jsonText = EditorGUILayout.TextArea(jsonText, GUILayout.Height(200));

            GUILayout.Space(10);

            outputPath = EditorGUILayout.TextField("Output Path", outputPath);
            EditorGUILayout.HelpBox("Path where the ScriptableObject will be saved", MessageType.None);

            GUILayout.Space(10);

            if (GUILayout.Button("Convert JSON to ScriptableObject", GUILayout.Height(40)))
            {
                ConvertJsonToScriptableObject();
            }

            GUILayout.Space(20);

            // Export Section
            EditorGUILayout.LabelField("Export to JSON", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Convert existing PlayerSceneData to JSON format", MessageType.Info);

            selectedSceneData = (PlayerSceneData)EditorGUILayout.ObjectField(
                "Scene Data",
                selectedSceneData,
                typeof(PlayerSceneData),
                false
            );

            if (GUILayout.Button("Convert ScriptableObject to JSON", GUILayout.Height(40)))
            {
                ConvertScriptableObjectToJson();
            }

            GUILayout.Space(20);

            // Template Section
            if (GUILayout.Button("Generate JSON Template", GUILayout.Height(30)))
            {
                GenerateTemplate();
            }

            EditorGUILayout.EndScrollView();
        }

        private void ConvertJsonToScriptableObject()
        {
            if (string.IsNullOrEmpty(jsonText))
            {
                EditorUtility.DisplayDialog("Error", "No JSON text provided!", "OK");
                return;
            }

            PlayerSceneData sceneData = JsonToSceneDataConverter.ConvertFromJsonString(jsonText);

            if (sceneData == null)
            {
                EditorUtility.DisplayDialog("Error", "Failed to convert JSON. Check console for details.", "OK");
                return;
            }

            // Ensure output directory exists
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            // Create asset
            string fileName = string.IsNullOrEmpty(sceneData.sceneName) ? "NewSceneData" : sceneData.sceneName;
            string assetPath = $"{outputPath}{fileName}.asset";

            // Make sure path is unique
            assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);

            AssetDatabase.CreateAsset(sceneData, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = sceneData;

            EditorUtility.DisplayDialog(
                "Success",
                $"ScriptableObject created at:\n{assetPath}\n\nNote: You may need to manually assign Sprite and AudioClip references.",
                "OK"
            );

            Debug.Log($"<color=green>Created SceneData at: {assetPath}</color>");
        }

        private void ConvertScriptableObjectToJson()
        {
            if (selectedSceneData == null)
            {
                EditorUtility.DisplayDialog("Error", "No SceneData selected!", "OK");
                return;
            }

            string json = JsonToSceneDataConverter.ConvertToJsonString(selectedSceneData, true);

            if (string.IsNullOrEmpty(json))
            {
                EditorUtility.DisplayDialog("Error", "Failed to convert SceneData to JSON.", "OK");
                return;
            }

            // Copy to clipboard
            GUIUtility.systemCopyBuffer = json;

            // Save to file
            string savePath = EditorUtility.SaveFilePanel(
                "Save JSON",
                "Assets/Json",
                selectedSceneData.sceneName + ".json",
                "json"
            );

            if (!string.IsNullOrEmpty(savePath))
            {
                File.WriteAllText(savePath, json);
                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog(
                    "Success",
                    $"JSON saved to:\n{savePath}\n\nAlso copied to clipboard!",
                    "OK"
                );
                Debug.Log($"<color=green>JSON exported to: {savePath}</color>");
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "Copied to Clipboard",
                    "JSON has been copied to clipboard!",
                    "OK"
                );
            }
        }

        private void GenerateTemplate()
        {
            string template = @"{
  ""sceneName"": ""example_scene"",
  ""displayName"": ""Example Scene"",
  ""backgroundSprite"": ""Sprites/Backgrounds/example_bg"",
  ""backgroundMusic"": ""Audio/Music/example_music"",
  ""dialogueLines"": [
    ""This is the first dialogue line."",
    ""This is the second dialogue line."",
    ""This is the third dialogue line.""
  ],
  ""questions"": [
    {
      ""questionText"": ""What is the first question?"",
      ""choices"": [
        ""Choice 1"",
        ""Choice 2"",
        ""Choice 3""
      ],
      ""correctAnswerIndex"": 0,
      ""correctResponse"": ""You remember! That makes me so happy!"",
      ""wrongResponse"": ""You... you don't remember?""
    },
    {
      ""questionText"": ""What is the second question?"",
      ""choices"": [
        ""Option A"",
        ""Option B"",
        ""Option C""
      ],
      ""correctAnswerIndex"": 1,
      ""correctResponse"": ""Yes! You got it right!"",
      ""wrongResponse"": ""That's not quite right...""
    }
  ],
  ""perfectAffectionBonus"": 10,
  ""wrongAnswerPenalty"": 5
}";

            GUIUtility.systemCopyBuffer = template;
            jsonText = template;

            EditorUtility.DisplayDialog(
                "Template Generated",
                "JSON template has been copied to clipboard and loaded into the text area!",
                "OK"
            );
        }
    }

    /// <summary>
    /// Quick context menu for converting selected JSON files
    /// Right-click on a .json TextAsset in the Project window
    /// </summary>
    public class JsonToSceneDataContextMenu
    {
        [MenuItem("Assets/Create/TimeLessLove/Convert JSON to SceneData")]
        private static void ConvertSelectedJson()
        {
            TextAsset jsonFile = Selection.activeObject as TextAsset;

            if (jsonFile == null)
            {
                EditorUtility.DisplayDialog("Error", "Selected file is not a valid TextAsset!", "OK");
                return;
            }

            PlayerSceneData sceneData = JsonToSceneDataConverter.ConvertFromJsonString(jsonFile.text);

            if (sceneData == null)
            {
                EditorUtility.DisplayDialog("Error", "Failed to convert JSON. Check console for details.", "OK");
                return;
            }

            // Save in the same directory as the JSON file
            string jsonPath = AssetDatabase.GetAssetPath(jsonFile);
            string directory = Path.GetDirectoryName(jsonPath);
            string fileName = Path.GetFileNameWithoutExtension(jsonPath);
            string assetPath = $"{directory}/{fileName}.asset";

            assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);

            AssetDatabase.CreateAsset(sceneData, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = sceneData;

            Debug.Log($"<color=green>Created SceneData at: {assetPath}</color>");
        }

        [MenuItem("Assets/Create/TimeLessLove/Convert JSON to SceneData", true)]
        private static bool ValidateConvertSelectedJson()
        {
            return Selection.activeObject is TextAsset;
        }
    }
}
