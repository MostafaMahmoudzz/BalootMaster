# Assaa System - Card Deck Reordering Feature

## Overview

The **Assaa System** is an advanced feature that activates **AFTER** a player chooses to use Rassa (YES). It allows opposing team members to potentially reorder the deck before dealing begins.

## 🎯 How Assaa Works

### Activation Flow

```
Player chooses Rassa (YES)
    ↓
Deck is arranged with Rassa order
    ↓
🆕 ASSAA SYSTEM ACTIVATES
    ↓
Ask player to RIGHT of Rassa chooser: "Assaa yes/no?"
    ↓
IF YES → Card Reordering UI
IF NO → Ask their TEAMMATE: "Assaa yes/no?"
    ↓
IF teammate says YES → Card Reordering UI
IF teammate says NO → Normal bidding begins
```

### Card Reordering Process

When a player accepts Assaa, they see a UI with two number inputs:

1. **Start Position (1-32)**: Choose which card position to start from
   - All cards from this position to the end (32) are selected
   - Example: Position 10 = cards 10-32 (23 cards selected)

2. **Target Position**: Where to move the selected cards
   - Must be LESS than the start position
   - Example: Target = 5 means selected cards go to position 5

**Result**: The selected cards are moved to the target position, effectively shuffling the deck.

## 📁 System Files

### Core Components

1. **`AssaaEvents.cs`** - Event system for communication
   - `AssaaPromptEvent` - Ask player yes/no
   - `AssaaResponseEvent` - Player's response
   - `AssaaReorderPromptEvent` - Show card reorder UI
   - `AssaaReorderCompleteEvent` - Reordering finished
   - `AssaaProcessCompleteEvent` - Entire process done

2. **`AssaaPromptUI.cs`** - Yes/No prompt UI
   - Shows to human players
   - AI responds automatically
   - Configurable AI behavior

3. **`AssaaCardReorderUI.cs`** - Card reordering interface
   - Two number input fields
   - Validation and preview
   - Confirms and applies changes

4. **`AssaaSystem.cs`** - Main controller
   - Manages the entire flow
   - Handles player selection logic
   - Coordinates UI and deck manipulation

### Integration

5. **`RassaGameIntegration.cs`** (Modified)
   - Triggers Assaa when Rassa is chosen
   - Waits for Assaa completion
   - Continues game flow

## 🎮 Setup Instructions

### 1. Add Components to Scene

In your Unity scene, add these components to the same GameObject as `BeloteGame`:

```
BeloteGame GameObject
├── RassaGameIntegration (existing)
├── AssaaSystem (NEW)
├── RassaDeckManager (existing)
├── RassaPromptUI (existing)
├── AssaaPromptUI (NEW)
└── AssaaCardReorderUI (NEW)
```

### 2. Create UI Panels

#### Assaa Prompt Panel (Yes/No)

Create a UI panel with:
- `promptPanel` (GameObject) - The main panel
- `messageText` (TextMeshProUGUI) - Displays message and player name
- `yesButton` (Button) - Accept Assaa
- `noButton` (Button) - Decline Assaa
- `yesButtonText` (TextMeshProUGUI) - "Yes" text
- `noButtonText` (TextMeshProUGUI) - "No" text

#### Card Reorder Panel

Create a UI panel with:
- `reorderPanel` (GameObject) - The main panel
- `titleText` (TextMeshProUGUI) - Title with player name
- `instructionsText` (TextMeshProUGUI) - Instructions
- `startPositionInput` (TMP_InputField) - First number input (1-32)
- `targetPositionInput` (TMP_InputField) - Second number input
- `confirmButton` (Button) - Confirm changes
- `cancelButton` (Button) - Cancel operation
- `errorText` (TextMeshProUGUI) - Error messages
- `previewText` (TextMeshProUGUI) - Preview of changes

### 3. Connect References

In Unity Inspector:

#### RassaGameIntegration
- Drag `AssaaSystem` component to `assaaSystem` field

#### AssaaSystem
- Drag `AssaaPromptUI` component to `assaaPromptUI` field
- Drag `AssaaCardReorderUI` component to `assaaCardReorderUI` field
- Check `enableAssaaSystem` to enable the feature

#### AssaaPromptUI
- Connect all UI references from the prompt panel
- Configure AI behavior:
  - `aiCanUseAssaa` - Allow AI to use Assaa?
  - `aiAssaaChance` - Probability (0-100) AI chooses Assaa

#### AssaaCardReorderUI
- Connect all UI references from the reorder panel

## 🎮 Example Usage Scenario

### Scenario 1: Right Player Accepts

```
1. South chooses Rassa (YES)
2. Deck is arranged with Rassa order
3. System asks West (right of South): "Assaa yes/no?"
4. West says YES
5. West sees card reorder UI
6. West enters: Start=15, Target=3
7. Cards 15-32 (18 cards) move to position 3
8. Bidding begins with reordered deck
```

### Scenario 2: Teammate Accepts

```
1. East chooses Rassa (YES)
2. Deck is arranged with Rassa order
3. System asks South (right of East): "Assaa yes/no?"
4. South says NO
5. System asks North (South's teammate): "Assaa yes/no?"
6. North says YES
7. North sees card reorder UI and reorders
8. Bidding begins
```

### Scenario 3: Both Decline

```
1. West chooses Rassa (YES)
2. Deck is arranged with Rassa order
3. System asks North (right of West): "Assaa yes/no?"
4. North says NO
5. System asks South (North's teammate): "Assaa yes/no?"
6. South says NO
7. Bidding begins immediately (no reordering)
```

## ⚙️ Settings & Configuration

### AssaaSystem Settings

- **Enable Assaa System**: Master on/off switch
  - If OFF: System is completely bypassed

### AssaaPromptUI Settings

- **AI Can Use Assaa**: Allow AI players to accept Assaa
  - If OFF: AI always declines
  - If ON: AI uses `aiAssaaChance` probability

- **AI Assaa Chance (0-100)**: Probability AI accepts
  - 0% = Never uses Assaa
  - 50% = Uses half the time
  - 100% = Always uses Assaa

## 🔧 Technical Details

### Team Structure

In Baloot (4 players, 2 teams):
- **Team 1**: South + North (opposite players)
- **Team 2**: West + East (opposite players)

### Player Order (Anti-clockwise)

South → West → North → East → South

### Finding Right Player

The system uses `GameStage.GetRightPlayer()` which moves anti-clockwise (to the right in card game terms).

### Finding Teammate

Teammates are on the same team but in opposite positions.

## 🐛 Troubleshooting

### Issue: Assaa UI doesn't appear

**Solution**: 
- Check `enableAssaaSystem` is checked in AssaaSystem
- Verify all UI references are connected
- Check that panels are children of a Canvas

### Issue: AI always declines Assaa

**Solution**:
- Check `aiCanUseAssaa` is enabled in AssaaPromptUI
- Increase `aiAssaaChance` value

### Issue: Card reordering doesn't work

**Solution**:
- Verify input validation rules:
  - Start position: 1-32
  - Target position: Less than start position
  - Target position: At least 1
- Check console for error messages

### Issue: Wrong player is asked

**Solution**:
- Verify player positions are set correctly in GameStage
- Check that GetRightPlayer() returns expected player
- Review team assignments (Team1 vs Team2)

## 📊 Event Flow Diagram

```
RassaResponseEvent (UseRassa = true)
    ↓
[RassaGameIntegration]
    ↓
AssaaSystem.StartAssaaProcess()
    ↓
AssaaPromptEvent → [AssaaPromptUI]
    ↓
Player chooses YES/NO
    ↓
AssaaResponseEvent
    ↓
IF YES: AssaaReorderPromptEvent → [AssaaCardReorderUI]
IF NO & first prompt: Ask teammate (loop back)
IF NO & second prompt: Complete
    ↓
[Player reorders cards]
    ↓
AssaaReorderCompleteEvent
    ↓
AssaaProcessCompleteEvent
    ↓
[RassaGameIntegration]
    ↓
RassaChoiceCompleteEvent
    ↓
GameStage continues with bidding
```

## ✅ Testing Checklist

- [ ] Rassa chosen → Assaa prompt appears
- [ ] Right player says YES → Card reorder UI appears
- [ ] Right player says NO → Teammate is asked
- [ ] Both say NO → Bidding starts immediately
- [ ] Card reordering validates input correctly
- [ ] Reordered deck is used for dealing
- [ ] AI players respond automatically
- [ ] Human players see proper UI
- [ ] Cancel button works in reorder UI
- [ ] Error messages display correctly

## 🎨 UI Customization

You can customize the appearance by modifying:
- Panel backgrounds and colors
- Text fonts and sizes
- Button styles
- Message text content
- Default instructions

All UI elements are standard Unity UI components and can be styled as needed.

## 📝 Notes

- Assaa ONLY activates when Rassa is chosen (YES)
- The opposing team members (right player and teammate) are asked
- Only ONE reorder is allowed per round
- If Assaa is disabled, it's completely skipped
- Deck changes persist for the entire round
- The system is fully event-driven for modularity

---

**Created**: November 10, 2025  
**Version**: 1.0  
**Compatibility**: Unity 2020.3+, TextMeshPro

