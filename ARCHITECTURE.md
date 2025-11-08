# TimeLess Love - System Architecture

## Overview

TimeLess Love is a narrative-memory game built with a modular, event-driven architecture. The game follows a core loop: **Scene Selection → Dialogue → Memory Questions → Result → Next Scene**.

---

## System Components

### 1. Data Layer

#### **PlayerSceneData.cs** (ScriptableObject)
- **Purpose**: Stores all data for a single story scene
- **Contains**:
  - Scene metadata (name, display name)
  - Visual assets (background sprite, music)
  - Dialogue lines array
  - Question/Answer sets
  - Affection reward/penalty values

**Location**: `Assets/Scripts/Data/PlayerSceneData.cs`

**Usage Example**:
```csharp
// Create via Unity menu: Right-click → Create → TimeLessLove → PlayerSceneData
// Assign in QuestBankManager's inspector
```

---

### 2. Core Systems

#### **AffectionSystem.cs**
- **Purpose**: Tracks player's relationship score (0-100) with Aria
- **Features**:
  - Add/Subtract affection with events
  - Threshold detection (Perfect Love, Happy Ending, Fading Memory, Game Over)
  - Event-driven notifications for UI updates

**Key Events**:
- `OnAffectionChanged(int)` - Fires on any change
- `OnGameOver()` - Fires when affection reaches 0
- `OnPerfectLoveReached()` - Fires at 90+ affection

**Location**: `Assets/Scripts/Systems/AffectionSystem.cs`

---

### 3. Manager Layer

#### **QuestBankManager.cs** (Singleton, DontDestroyOnLoad)
- **Purpose**: Central repository for all scene data
- **Responsibilities**:
  - Stores list of all PlayerSceneData assets
  - Tracks which scenes played today
  - Provides random scene selection
  - Manages day progression

**Key Methods**:
```csharp
PlayerSceneData GetRandomSceneData()        // Gets unplayed scene for today
List<QuestionData> GetCurrentQuestions()    // Gets questions for active scene
void ResetDailyProgress()                   // Resets day counter and played scenes
```

**Location**: `Assets/Scripts/Managers/QuestBankManager.cs`

---

#### **TransitionSceneManager.cs** (Singleton, DontDestroyOnLoad)
- **Purpose**: Handles smooth scene transitions with fade effects
- **Features**:
  - Fade-in/fade-out using CanvasGroup or Animator
  - Async scene loading with progress
  - Customizable transition timing

**Key Methods**:
```csharp
void LoadScene(string sceneName)            // Load scene with transition
void PlayTransitionEffect(Action callback)  // Play effect without loading
```

**Setup Required**:
1. Create a Canvas with CanvasGroup component
2. Assign to TransitionSceneManager's `transitionCanvasGroup` field
3. Ensure canvas persists with DontDestroyOnLoad

**Location**: `Assets/Scripts/Managers/TransitionSceneManager.cs`

---

#### **DialogueManager.cs**
- **Purpose**: Displays Aria's dialogue with typewriter effect
- **Features**:
  - Character-by-character text reveal
  - Skip/fast-forward support
  - Optional typing sound effects
  - Progress tracking

**Key Events**:
- `OnDialogueCompleted()` - Fires when all lines shown
- `OnLineChanged(int current, int total)` - Progress updates

**Location**: `Assets/Scripts/Managers/DialogueManager.cs`

---

#### **UIManager.cs**
- **Purpose**: Central hub for all UI interactions
- **Responsibilities**:
  - Manages all panels (dialogue, questions, results)
  - Handles button clicks with Action events
  - Updates HUD (affection bar, day counter)
  - Displays questions and answer choices

**Key Events**:
```csharp
event Action OnNextDialogueClicked
event Action<int> OnAnswerSelected
event Action OnSubmitAnswerClicked
```

**Location**: `Assets/Scripts/Managers/UIManager.cs`

---

#### **GameFlowManager.cs**
- **Purpose**: Orchestrates the complete game loop
- **Responsibilities**:
  - Coordinates all managers
  - Controls game state machine
  - Implements core loop sequence

**Game States**:
1. **Idle** - Waiting to start
2. **Dialogue** - Showing Aria's lines
3. **Questions** - Player answering memory questions
4. **Results** - Showing answer feedback
5. **Transitioning** - Moving to next scene

**Location**: `Assets/Scripts/Managers/GameFlowManager.cs`

---

#### **AudioManager.cs** (Optional, Singleton, DontDestroyOnLoad)
- **Purpose**: Manages all audio (BGM + SFX)
- **Features**:
  - Music crossfading
  - Volume control
  - Predefined SFX methods (PlayCorrectAnswer, PlayAffectionUp, etc.)

**Location**: `Assets/Scripts/Managers/AudioManager.cs`

---

### 4. Utility Systems

#### **SaveSystem.cs** (Optional, Singleton, DontDestroyOnLoad)
- **Purpose**: Handles save/load using PlayerPrefs
- **Saves**:
  - Current affection value
  - Day number
  - Last played scene
  - Audio settings

**Location**: `Assets/Scripts/Systems/SaveSystem.cs`

---

## Data Flow Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                      GAME START                              │
└──────────────────┬──────────────────────────────────────────┘
                   │
                   ▼
┌─────────────────────────────────────────────────────────────┐
│  GameFlowManager.StartGameLoop()                            │
│    ├─ QuestBankManager.GetRandomSceneData()                 │
│    └─ Setup scene (background, music, HUD)                  │
└──────────────────┬──────────────────────────────────────────┘
                   │
                   ▼
┌─────────────────────────────────────────────────────────────┐
│  DIALOGUE PHASE                                              │
│    DialogueManager.StartDialogue(lines)                     │
│      │                                                       │
│      ├─ UIManager updates dialogue text (typewriter)        │
│      ├─ Player clicks "Next" → UIManager.OnNextDialogueClicked
│      └─ Repeat until all lines shown                        │
│          └─ DialogueManager.OnDialogueCompleted fires       │
└──────────────────┬──────────────────────────────────────────┘
                   │
                   ▼
┌─────────────────────────────────────────────────────────────┐
│  QUESTION PHASE                                              │
│    GameFlowManager.StartQuestionPhase()                     │
│      │                                                       │
│      ├─ For each question in currentSceneData.questions:    │
│      │    ├─ UIManager.DisplayQuestion(questionData)        │
│      │    ├─ Player selects answer                          │
│      │    ├─ UIManager.OnSubmitAnswerClicked fires          │
│      │    ├─ GameFlowManager validates answer               │
│      │    ├─ AffectionSystem.AddAffection() or SubtractAffection()
│      │    ├─ UIManager.DisplayResult(correct, response)     │
│      │    └─ Player clicks Continue → next question         │
│      │                                                       │
│      └─ All questions answered                              │
└──────────────────┬──────────────────────────────────────────┘
                   │
                   ▼
┌─────────────────────────────────────────────────────────────┐
│  TRANSITION PHASE                                            │
│    ├─ Check if affection > 0                                │
│    │    ├─ Yes: TransitionSceneManager.LoadNextScene()      │
│    │    │         OR GameFlowManager.StartGameLoop()        │
│    │    │                                                    │
│    │    └─ No: AffectionSystem.OnGameOver fires             │
│    │           → Load Game Over scene                       │
│    │                                                         │
│    └─ Loop back to GAME START                               │
└─────────────────────────────────────────────────────────────┘
```

---

## Event Flow

### Dialogue Phase Events
```
DialogueManager.StartDialogue()
  └─ OnDialogueStarted fires
       └─ UIManager shows dialogue panel
            └─ User clicks Next button
                 └─ UIManager.OnNextDialogueClicked fires
                      └─ DialogueManager.DisplayNextLine()
                           └─ Repeat until...
                                └─ DialogueManager.OnDialogueCompleted fires
                                     └─ GameFlowManager starts Question Phase
```

### Question Phase Events
```
GameFlowManager.StartQuestionPhase()
  └─ UIManager.DisplayQuestion(questionData)
       └─ User clicks answer button
            └─ UIManager.OnAnswerSelected(index) fires
                 └─ Submit button becomes clickable
                      └─ User clicks Submit
                           └─ UIManager.OnSubmitAnswerClicked fires
                                └─ GameFlowManager validates answer
                                     ├─ Correct: AffectionSystem.AddAffection()
                                     │             └─ OnAffectionChanged fires
                                     │                  └─ UIManager updates affection bar
                                     │
                                     └─ Wrong: AffectionSystem.SubtractAffection()
                                               └─ OnAffectionChanged fires
                                                    └─ Check if affection = 0
                                                         └─ OnGameOver fires
```

---

## Integration Guide

### Setting Up a New Scene

1. **Create ScriptableObject**:
   - Right-click in Project → Create → TimeLessLove → PlayerSceneData
   - Name it (e.g., "Scene_Meeting")

2. **Configure Scene Data**:
   ```
   Scene Name: "Meeting"
   Display Name: "First Meeting"
   Dialogue Lines: ["Hi there!", "Do you remember me?", ...]
   Questions: Add 3-5 QuestionData entries
   ```

3. **Add to QuestBankManager**:
   - Find QuestBankManager in scene
   - Add ScriptableObject to "All Scene Data" list

4. **Assign Assets**:
   - Background Sprite
   - Background Music (AudioClip)

---

### Hooking Up UI Elements

In your Unity scene, create these UI elements and assign to UIManager:

**Required Panels**:
- `gameplayPanel` - Active during dialogue
- `questionPanel` - Active during questions
- `resultPanel` - Active when showing answer results

**Required Buttons**:
- `nextDialogueButton` - Advances dialogue
- `answerButtons[]` - Array of 3-4 buttons for choices
- `submitAnswerButton` - Confirms answer selection
- `continueButton` - Proceeds after result

**Required Text Fields**:
- `dialogueText` - Shows Aria's dialogue
- `questionText` - Shows question text
- `affectionValueText` - Shows "50 / 100"
- `dayCounterText` - Shows "Day 3"

---

### Example Scene Hierarchy

```
Canvas
├── GameplayPanel
│   ├── DialogueBox
│   │   ├── CharacterName (TextMeshProUGUI)
│   │   ├── DialogueText (TextMeshProUGUI)
│   │   └── NextButton
│   └── HUD
│       ├── AffectionSlider
│       ├── AffectionText
│       └── DayCounter
│
├── QuestionPanel
│   ├── QuestionText (TextMeshProUGUI)
│   ├── AnswerButton_1
│   ├── AnswerButton_2
│   ├── AnswerButton_3
│   └── SubmitButton
│
├── ResultPanel
│   ├── ResultText (TextMeshProUGUI)
│   ├── AffectionChangeText
│   └── ContinueButton
│
└── BackgroundImage

Managers (Empty GameObjects)
├── GameFlowManager
├── DialogueManager
├── UIManager
├── AffectionSystem
├── QuestBankManager (DontDestroyOnLoad)
├── TransitionSceneManager (DontDestroyOnLoad)
├── AudioManager (DontDestroyOnLoad)
└── SaveSystem (DontDestroyOnLoad)
```

---

## Execution Order

Managers initialize in this order:

1. **QuestBankManager** (Singleton, DontDestroyOnLoad)
2. **TransitionSceneManager** (Singleton, DontDestroyOnLoad)
3. **AudioManager** (Singleton, DontDestroyOnLoad)
4. **SaveSystem** (Singleton, DontDestroyOnLoad)
5. **GameFlowManager** (Coordinates scene-specific managers)
6. **DialogueManager** (Scene-specific)
7. **UIManager** (Scene-specific)
8. **AffectionSystem** (Scene-specific)

---

## Best Practices

### Event Subscription
Always unsubscribe in `OnDestroy()`:
```csharp
void OnDestroy()
{
    uiManager.OnNextDialogueClicked -= HandleDialogueClick;
    affectionSystem.OnAffectionChanged -= UpdateUI;
}
```

### Null Checks
All manager references should be validated:
```csharp
if (dialogueManager != null)
    dialogueManager.StartDialogue(lines);
```

### Singleton Access
Use Instance property safely:
```csharp
if (QuestBankManager.Instance != null)
{
    var scene = QuestBankManager.Instance.GetRandomSceneData();
}
```

---

## Testing Workflow

1. **Create Sample Scene Data**: Make 2-3 PlayerSceneData assets
2. **Setup UI**: Build basic UI with required buttons/text fields
3. **Assign References**: Connect all manager references in inspector
4. **Test Dialogue**: Verify typewriter effect and line progression
5. **Test Questions**: Check answer validation and affection updates
6. **Test Transitions**: Ensure scene loads properly after questions
7. **Test Persistence**: Save/load using SaveSystem

---

## Common Issues

### "Manager not found" errors
- Ensure managers exist in scene hierarchy
- Check DontDestroyOnLoad managers are created at startup

### Events not firing
- Verify event subscriptions in `Start()` or `Awake()`
- Check unsubscribe isn't called too early

### Questions not displaying
- Ensure PlayerSceneData has questions assigned
- Check UIManager has answerButtons array assigned

### Affection not updating
- Verify AffectionSystem events are subscribed to UIManager
- Check bonus/penalty values in PlayerSceneData

---

## Extension Points

### Adding New Scene Types
1. Create new PlayerSceneData asset
2. Add to QuestBankManager list
3. No code changes needed!

### Custom Question Types
Extend `QuestionData` class:
```csharp
[System.Serializable]
public class TimedQuestionData : QuestionData
{
    public float timeLimit = 10f;
}
```

### New Affection Thresholds
Add in AffectionSystem:
```csharp
public event Action OnTrueLoveReached;

private void CheckThresholds()
{
    if (currentAffection >= 100)
        OnTrueLoveReached?.Invoke();
}
```

---

## Performance Notes

- **ScriptableObjects**: Efficient data storage, shared across scenes
- **Event-Driven**: Decoupled systems reduce dependencies
- **Singleton Pattern**: Managers persist without duplication
- **Async Loading**: TransitionSceneManager prevents frame drops

---

## Summary

The architecture follows these principles:

✅ **Modular**: Each system has single responsibility
✅ **Event-Driven**: Loose coupling via C# Actions
✅ **Data-Driven**: ScriptableObjects for easy content creation
✅ **Extensible**: Easy to add scenes, questions, features
✅ **Maintainable**: Clear naming conventions and documentation

---

**Generated with ❤️ for TimeLess Love**