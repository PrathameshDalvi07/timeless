# 🎮 TimeLess Love - Unity C# Game Scripts

A narrative-memory game where players must remember what Aria says to maintain their relationship. Answer questions correctly to keep affection high, or watch memories fade away.

---

## 📖 Game Concept

**Core Loop**: Scene → Dialogue → Memory Questions → Result → Next Scene

Each day presents 5 random story scenes. The player must:
1. Listen to Aria's dialogue carefully
2. Answer memory questions about what she said
3. Maintain affection level above 0
4. Experience different endings based on final affection score

---

## 🏗️ Architecture Overview

### Core Systems (9 Scripts)

| Script | Type | Purpose |
|--------|------|---------|
| **PlayerSceneData.cs** | ScriptableObject | Stores scene data (dialogue, questions, assets) |
| **AffectionSystem.cs** | Component | Tracks relationship score (0-100) |
| **QuestBankManager.cs** | Singleton | Manages all scene data and selection |
| **TransitionSceneManager.cs** | Singleton | Handles scene transitions with fade effects |
| **DialogueManager.cs** | Component | Displays dialogue with typewriter effect |
| **UIManager.cs** | Component | Manages all UI interactions and panels |
| **GameFlowManager.cs** | Component | Orchestrates game loop and state machine |
| **AudioManager.cs** | Singleton | Manages music and sound effects |
| **SaveSystem.cs** | Singleton | Handles save/load using PlayerPrefs |

### Utility Scripts

| Script | Purpose |
|--------|---------|
| **GameDebugger.cs** | Debug tools for testing (keyboard shortcuts) |

---

## 📂 File Structure

```
Assets/Scripts/
├── Data/
│   └── PlayerSceneData.cs
├── Systems/
│   ├── AffectionSystem.cs
│   └── SaveSystem.cs
├── Managers/
│   ├── QuestBankManager.cs
│   ├── TransitionSceneManager.cs
│   ├── DialogueManager.cs
│   ├── UIManager.cs
│   ├── GameFlowManager.cs
│   └── AudioManager.cs
└── Utilities/
    └── GameDebugger.cs
```

---

## 🔄 Data Flow

```
Start Game
    ↓
GameFlowManager.StartGameLoop()
    ↓
QuestBankManager.GetRandomSceneData()
    ↓
[DIALOGUE PHASE]
DialogueManager displays lines → Player clicks Next → OnDialogueCompleted
    ↓
[QUESTION PHASE]
UIManager displays question → Player selects answer → Validates
    ↓
AffectionSystem.AddAffection() or SubtractAffection()
    ↓
Check affection > 0?
    ├─ Yes → Loop to next scene
    └─ No  → Game Over
```

---

## 🎯 Key Features

### Event-Driven Architecture
All systems communicate through C# Actions/Events:
```csharp
// Example
public event Action<int> OnAffectionChanged;
public event Action OnDialogueCompleted;
public event Action<int> OnAnswerSelected;
```

### Modular Design
Each manager has single responsibility:
- **DialogueManager**: Only handles dialogue display
- **UIManager**: Only handles UI interactions
- **GameFlowManager**: Only coordinates other managers

### Data-Driven Content
Create new scenes without touching code:
1. Right-click → Create → TimeLessLove → PlayerSceneData
2. Fill in dialogue and questions
3. Add to QuestBankManager list
4. Done! ✅

---

## 🚀 Quick Start

### 1. Import Scripts
Copy all scripts to your Unity project following the folder structure above.

### 2. Create Scene Data
```
Right-click in Project → Create → TimeLessLove → PlayerSceneData
```

Fill in:
- Scene name and display name
- Background sprite and music
- Dialogue lines array
- Questions with multiple choices

### 3. Setup Scene
Create GameObjects with these scripts:
- GameFlowManager
- DialogueManager
- UIManager
- AffectionSystem
- QuestBankManager (DontDestroyOnLoad)
- TransitionSceneManager (DontDestroyOnLoad)

### 4. Wire References
Assign UI elements to managers in Inspector (see SETUP_GUIDE.md for details).

### 5. Press Play
Game should start automatically with random scene selection!

---

## 📊 System Interactions

### GameFlowManager Coordinates Everything

```csharp
// Simplified GameFlowManager workflow
void Start()
{
    SubscribeToEvents();
    StartGameLoop();
}

void StartGameLoop()
{
    currentSceneData = QuestBankManager.Instance.GetRandomSceneData();
    SetupScene();
    StartDialoguePhase();
}

void OnDialogueCompleted()
{
    StartQuestionPhase();
}

void OnAnswerSubmitted()
{
    bool correct = ValidateAnswer();
    affectionSystem.AddAffection(correct ? 10 : -10);
    uiManager.DisplayResult(correct, response);
}

void OnContinueClicked()
{
    NextQuestion() or StartGameLoop();
}
```

### Event Subscription Example

```csharp
// In GameFlowManager
void SubscribeToEvents()
{
    dialogueManager.OnDialogueCompleted += OnDialogueCompleted;
    uiManager.OnSubmitAnswerClicked += OnAnswerSubmitted;
    affectionSystem.OnGameOver += OnGameOver;
}

void OnDestroy()
{
    // Always unsubscribe!
    dialogueManager.OnDialogueCompleted -= OnDialogueCompleted;
    uiManager.OnSubmitAnswerClicked -= OnAnswerSubmitted;
    affectionSystem.OnGameOver -= OnGameOver;
}
```

---

## 🎨 Creating Content

### Example PlayerSceneData Configuration

**Scene: First Meeting**
```
Scene Name: Meeting
Display Name: First Meeting at the Café

Dialogue Lines:
  [0] "Hey! I didn't expect to see you here."
  [1] "This place has the best coffee, don't you think?"
  [2] "I come here every Thursday at 3 PM."
  [3] "I always order the caramel latte."

Questions:
  1. "What day did I say I come here?"
     Choices: ["Monday", "Thursday", "Saturday"]
     Correct: 1 (Thursday)

  2. "What's my favorite drink here?"
     Choices: ["Espresso", "Caramel Latte", "Green Tea"]
     Correct: 1 (Caramel Latte)

  3. "What time do I usually arrive?"
     Choices: ["2 PM", "3 PM", "4 PM"]
     Correct: 1 (3 PM)

Perfect Affection Bonus: 10
Wrong Answer Penalty: 10
```

---

## 🔧 Configuration

### Affection Thresholds
```csharp
// In AffectionSystem.cs
PERFECT_LOVE_THRESHOLD = 90
HAPPY_ENDING_THRESHOLD = 70
NEUTRAL_THRESHOLD = 40
FADING_MEMORY_THRESHOLD = 20
Game Over = 0
```

### Timing Settings
```csharp
// In GameFlowManager
delayAfterDialogue = 1f        // Pause before questions
delayAfterQuestion = 1.5f      // Show result duration
delayBeforeNextScene = 2f      // Wait before transition
```

### Dialogue Speed
```csharp
// In DialogueManager
typeSpeed = 0.05f              // Lower = faster typing
allowSkip = true               // Can skip typewriter effect
```

---

## 🐛 Debug Tools

### GameDebugger Keyboard Shortcuts
```
+ (Plus)     : Add 10 affection
- (Minus)    : Subtract 10 affection
S            : Skip current dialogue
N            : Load next random scene
I            : Print debug info to console
```

### Inspector Context Menu
Right-click GameDebugger component:
- Add 10 Affection
- Subtract 10 Affection
- Reset Affection
- Print Debug Info
- Save Game
- Load Game
- Delete Save Data

---

## 💾 Save System

Uses PlayerPrefs to store:
- Current affection value
- Day number
- Last played scene
- Audio volume settings

```csharp
// Save/Load example
SaveSystem.Instance.SaveGame();
SaveSystem.Instance.LoadGame();

// Check for existing save
bool hasSave = SaveSystem.Instance.HasSaveData();

// Delete save (New Game)
SaveSystem.Instance.DeleteSaveData();
```

---

## 🎵 Audio System

### Playing Music
```csharp
// In scene setup
AudioManager.Instance.PlayMusic(sceneData.backgroundMusic, fade: true);
```

### Playing SFX
```csharp
// Predefined methods
AudioManager.Instance.PlayButtonClick();
AudioManager.Instance.PlayCorrectAnswer();
AudioManager.Instance.PlayWrongAnswer();
AudioManager.Instance.PlayAffectionUp();
AudioManager.Instance.PlayAffectionDown();

// Custom SFX
AudioManager.Instance.PlaySFX(myClip, volumeMultiplier: 0.8f);
```

---

## 🎬 Scene Transitions

### Basic Usage
```csharp
TransitionSceneManager.Instance.LoadScene("GameOver");
TransitionSceneManager.Instance.LoadNextScene();
TransitionSceneManager.Instance.ReloadCurrentScene();
```

### Transition Effects
```csharp
// Play effect without loading scene
TransitionSceneManager.Instance.PlayTransitionEffect(() => {
    Debug.Log("Transition complete!");
});
```

---

## 📈 Extending the System

### Add New Question Type
```csharp
[System.Serializable]
public class TimedQuestionData : QuestionData
{
    public float timeLimit = 10f;
    public int timeBonusPoints = 5;
}
```

### Add New Affection Event
```csharp
// In AffectionSystem
public event Action OnTrueLoveReached;

private void CheckThresholds()
{
    if (currentAffection >= 95)
        OnTrueLoveReached?.Invoke();
}
```

### Add Custom Scene Type
Create new ScriptableObject inheriting from PlayerSceneData:
```csharp
[CreateAssetMenu(fileName = "BossScene", menuName = "TimeLessLove/BossSceneData")]
public class BossSceneData : PlayerSceneData
{
    public int requiredCorrectAnswers = 5;
    public float timeLimit = 60f;
}
```

---

## 🏆 Production Readiness

### ✅ SOLID Principles
- **Single Responsibility**: Each manager has one job
- **Open/Closed**: Extend via inheritance, not modification
- **Liskov Substitution**: ScriptableObjects can be swapped
- **Interface Segregation**: Events provide clean interfaces
- **Dependency Inversion**: Managers depend on events, not concrete classes

### ✅ Performance
- ScriptableObjects reduce memory usage
- Event-driven reduces polling
- Async scene loading prevents frame drops
- Singleton pattern prevents duplication

### ✅ Maintainability
- Clear naming conventions (Manager, System suffixes)
- Comprehensive XML documentation comments
- Separation of concerns
- Unit-testable architecture

---

## 📚 Documentation Files

| File | Purpose |
|------|---------|
| **README.md** | This file - overview and quick reference |
| **ARCHITECTURE.md** | Detailed system architecture and data flow |
| **SETUP_GUIDE.md** | Step-by-step setup instructions |

---

## 🎓 Learning Resources

### Unity Concepts Used
- ScriptableObjects for data storage
- Events/Actions for decoupling
- Singleton pattern with DontDestroyOnLoad
- Coroutines for async operations
- TextMeshPro for UI text
- Async scene loading
- PlayerPrefs for save data

### C# Features Used
- C# 9 syntax
- Namespaces for organization
- Properties with expression bodies
- Null-conditional operators
- Lambda expressions
- LINQ for collections

---

## 🤝 Contributing

To add new features:
1. Follow existing naming conventions
2. Document public methods with XML comments
3. Use events for cross-system communication
4. Test with GameDebugger
5. Update ARCHITECTURE.md if needed

---

## 📜 License

Feel free to use this code for your own Unity projects!

---

## 🎉 Credits

**Built for TimeLess Love** - A narrative memory game about love, loss, and remembering.

Generated with ❤️ using modular, production-ready C# architecture.

---

## 🔗 Quick Links

- [Architecture Documentation](ARCHITECTURE.md)
- [Setup Guide](SETUP_GUIDE.md)
- Unity Version: 2021+
- TextMeshPro: Required
- Target Platform: PC, Mobile

---

**Have questions? Check ARCHITECTURE.md for detailed system explanations!**
