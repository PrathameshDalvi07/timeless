# TimeLess Love - Quick Setup Guide

## 📁 Folder Structure

```
Assets/
├── Scripts/
│   ├── Data/
│   │   └── PlayerSceneData.cs
│   ├── Systems/
│   │   ├── AffectionSystem.cs
│   │   └── SaveSystem.cs
│   ├── Managers/
│   │   ├── QuestBankManager.cs
│   │   ├── TransitionSceneManager.cs
│   │   ├── DialogueManager.cs
│   │   ├── UIManager.cs
│   │   ├── GameFlowManager.cs
│   │   └── AudioManager.cs
│   └── Utilities/
│       └── GameDebugger.cs
├── ScriptableObjects/
│   └── Scenes/
│       ├── Scene_Meeting.asset
│       ├── Scene_RainyWalk.asset
│       ├── Scene_MovieNight.asset
│       ├── Scene_Argument.asset
│       └── Scene_Reflection.asset
├── Scenes/
│   ├── MainMenu.unity
│   ├── Gameplay.unity
│   └── GameOver.unity
├── Sprites/
│   └── Backgrounds/
├── Audio/
│   ├── Music/
│   └── SFX/
└── Prefabs/
```

---

## 🚀 Step-by-Step Setup

### Step 1: Create Scene Data (ScriptableObjects)

1. In Unity, right-click in Project window
2. Create → TimeLessLove → PlayerSceneData
3. Name it "Scene_Meeting"
4. Configure in Inspector:

```
Scene Name: Meeting
Display Name: First Meeting at the Café
Dialogue Lines:
  [0] "Hey! I didn't expect to see you here."
  [1] "This place has the best coffee, don't you think?"
  [2] "I come here every Thursday at 3 PM."

Questions:
  Question 1:
    Text: "What day did I say I come to this café?"
    Choices:
      [0] "Monday"
      [1] "Thursday"
      [2] "Saturday"
    Correct Answer Index: 1
    Correct Response: "You remembered! That's so sweet!"
    Wrong Response: "You... you weren't listening?"

Perfect Affection Bonus: 10
Wrong Answer Penalty: 10
```

5. Repeat for 4 more scenes (RainyWalk, MovieNight, Argument, Reflection)

---

### Step 2: Create Persistent Managers Scene

Create a new scene called "PersistentManagers.unity":

1. Create empty GameObjects:
   - QuestBankManager (add script)
   - TransitionSceneManager (add script)
   - AudioManager (add script)
   - SaveSystem (add script)

2. **Configure QuestBankManager**:
   - Drag all 5 PlayerSceneData assets into "All Scene Data" list

3. **Configure TransitionSceneManager**:
   - Create Canvas → CanvasGroup
   - Name it "TransitionOverlay"
   - Set to fill screen (Anchor: Stretch, Rect: all zeros)
   - Add black Image component
   - Assign CanvasGroup to TransitionSceneManager

---

### Step 3: Create Gameplay Scene

1. Create new scene "Gameplay.unity"

#### Create UI Hierarchy:

**Canvas** (Screen Space - Overlay)
```
Canvas
├── BackgroundImage (Image, fill screen)
│
├── GameplayPanel
│   ├── DialoguePanel
│   │   ├── DialogueBox (Image)
│   │   │   ├── CharacterName (TextMeshProUGUI)
│   │   │   ├── CharacterPortrait (Image)
│   │   │   ├── DialogueText (TextMeshProUGUI)
│   │   │   ├── ProgressText (TextMeshProUGUI) "1 / 5"
│   │   │   └── NextButton (Button)
│   │   │       └── Text: "Next"
│   │
│   └── HUD
│       ├── AffectionBar
│       │   ├── Background (Image)
│       │   ├── Fill (Slider)
│       │   └── Text (TextMeshProUGUI) "50 / 100"
│       ├── DayCounter (TextMeshProUGUI) "Day 1"
│       └── SceneName (TextMeshProUGUI) "First Meeting"
│
├── QuestionPanel (inactive by default)
│   ├── QuestionBox (Image)
│   │   ├── QuestionText (TextMeshProUGUI)
│   │   ├── AnswerButton_1 (Button)
│   │   │   └── Text (TextMeshProUGUI)
│   │   ├── AnswerButton_2 (Button)
│   │   │   └── Text (TextMeshProUGUI)
│   │   ├── AnswerButton_3 (Button)
│   │   │   └── Text (TextMeshProUGUI)
│   │   └── SubmitButton (Button)
│   │       └── Text: "Submit Answer"
│
└── ResultPanel (inactive by default)
    ├── ResultBox (Image)
    │   ├── ResultText (TextMeshProUGUI)
    │   ├── AffectionChangeText (TextMeshProUGUI)
    │   └── ContinueButton (Button)
    │       └── Text: "Continue"
```

#### Create Manager GameObjects:

```
Managers (empty GameObject)
├── GameFlowManager (add script)
├── DialogueManager (add script)
├── UIManager (add script)
├── AffectionSystem (add script)
└── GameDebugger (add script, optional)
```

---

### Step 4: Wire Up References

#### **DialogueManager** Inspector:
- Dialogue Panel: Drag DialoguePanel
- Dialogue Text: Drag DialogueText
- Character Name Text: Drag CharacterName
- Character Portrait: Drag CharacterPortrait
- Type Speed: 0.05
- Allow Skip: ✓

#### **UIManager** Inspector:
- **Panels**:
  - Gameplay Panel: Drag GameplayPanel
  - Question Panel: Drag QuestionPanel
  - Result Panel: Drag ResultPanel

- **Dialogue UI**:
  - Next Dialogue Button: Drag NextButton
  - Dialogue Progress Text: Drag ProgressText

- **Question UI**:
  - Question Text: Drag QuestionText
  - Answer Buttons: Drag AnswerButton_1, _2, _3
  - Answer Texts: Drag Text children of buttons
  - Submit Answer Button: Drag SubmitButton

- **Result UI**:
  - Result Text: Drag ResultText
  - Affection Change Text: Drag AffectionChangeText
  - Continue Button: Drag ContinueButton

- **HUD**:
  - Affection Slider: Drag Fill slider
  - Affection Value Text: Drag affection text
  - Day Counter Text: Drag DayCounter
  - Scene Name Text: Drag SceneName

- **Background**:
  - Background Image: Drag BackgroundImage

#### **GameFlowManager** Inspector:
- Dialogue Manager: Drag DialogueManager
- UI Manager: Drag UIManager
- Affection System: Drag AffectionSystem
- Delay After Dialogue: 1
- Delay After Question: 1.5
- Delay Before Next Scene: 2
- Ask All Questions: ✓

#### **AffectionSystem** Inspector:
- Max Affection: 100
- Starting Affection: 50

---

### Step 5: Test in Play Mode

1. Press Play
2. Game should automatically:
   - Load random scene
   - Show dialogue with typewriter effect
   - Display questions after dialogue
   - Update affection based on answers
   - Loop to next scene

**Debug Controls** (if GameDebugger attached):
- `+` / `-` : Add/Subtract 10 affection
- `S` : Skip dialogue
- `N` : Load next random scene
- `I` : Print debug info to console

---

### Step 6: Add Audio (Optional)

1. Import music/SFX clips into Assets/Audio/
2. In Gameplay scene, find AudioManager (DontDestroyOnLoad)
3. Assign clips:
   - Button Click SFX
   - Correct Answer SFX
   - Wrong Answer SFX
   - Affection Up SFX
   - Affection Down SFX

4. In each PlayerSceneData asset:
   - Assign Background Music clip

---

### Step 7: Build Settings

1. File → Build Settings
2. Add scenes in order:
   - 0: PersistentManagers
   - 1: MainMenu
   - 2: Gameplay
   - 3: GameOver

---

## ⚙️ Configuration Tips

### Dialogue Speed
Adjust `typeSpeed` in DialogueManager:
- 0.03 = Fast
- 0.05 = Normal (recommended)
- 0.08 = Slow

### Affection Balance
In PlayerSceneData:
- Perfect Affection Bonus: 10-15 (rewards correct answers)
- Wrong Answer Penalty: 5-10 (punishes mistakes)

**Example**: 5 scenes × 3 questions each = 15 total questions
- All correct: +150 affection (max 100, caps at 100)
- All wrong: -150 affection (game over)

### Scene Selection
QuestBankManager has two modes:

**Random Daily** (current):
```csharp
GetRandomSceneData() // Picks unplayed scene today
```

**Shuffle Queue**:
```csharp
GetShuffledDailyScenes() // Creates full daily rotation
GetNextSceneFromQueue() // Pops next from queue
```

---

## 🐛 Troubleshooting

### "Manager not found" errors
- Ensure all manager scripts are attached to GameObjects
- Check DontDestroyOnLoad managers exist in initial scene

### Dialogue not showing
- Verify DialogueManager has UI references assigned
- Check GameplayPanel is active on scene start
- Ensure PlayerSceneData has dialogue lines filled

### Questions not appearing
- Check PlayerSceneData has questions with choices
- Verify UIManager has answer button array assigned
- Ensure buttons have TextMeshProUGUI children

### Affection not updating
- Check AffectionSystem component exists
- Verify GameFlowManager has AffectionSystem reference
- Check PlayerSceneData has bonus/penalty values > 0

### Scene not transitioning
- Ensure TransitionSceneManager exists with DontDestroyOnLoad
- Check CanvasGroup is assigned
- Verify target scene is in Build Settings

### Audio not playing
- Check AudioManager exists with DontDestroyOnLoad
- Verify AudioClips are assigned
- Check volume settings > 0

---

## 📊 Testing Checklist

- [ ] Create at least 2 PlayerSceneData assets
- [ ] Configure all UI references in managers
- [ ] Test dialogue typewriter effect
- [ ] Test question answering flow
- [ ] Test affection increase/decrease
- [ ] Test scene transitions
- [ ] Test game over at 0 affection
- [ ] Test save/load system
- [ ] Test audio playback
- [ ] Build and test standalone

---

## 🎨 Customization Ideas

### Add Visual Effects
- Particle effects on correct/wrong answers
- Screen shake on affection change
- Character portrait animations

### Enhance Dialogue
- Multiple character support
- Voice acting integration
- Dialogue choices (branching)

### Extend Questions
- Timed questions
- Multiple correct answers
- Difficulty levels

### New Features
- Achievement system
- Collectible memories
- Multiple endings based on affection tiers
- Replay gallery

---

## 📚 Next Steps

1. **Create Content**: Build 5 complete scenes with dialogue and questions
2. **Design UI**: Replace placeholder UI with polished graphics
3. **Add Audio**: Import music and SFX
4. **Test Balance**: Ensure affection progression feels fair
5. **Polish**: Add transitions, effects, animations
6. **Build**: Create standalone build for target platform

---

**You're all set! Happy developing! ❤️**