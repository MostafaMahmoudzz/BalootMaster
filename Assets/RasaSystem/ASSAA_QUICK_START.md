# Assaa System - Quick Start Guide

## ⚡ 5-Minute Setup

### Step 1: Add Components (2 minutes)

Find your `BeloteGame` GameObject in the scene hierarchy and add these components:

1. Click `Add Component` → Search "AssaaSystem" → Add
2. Click `Add Component` → Search "AssaaPromptUI" → Add  
3. Click `Add Component` → Search "AssaaCardReorderUI" → Add

### Step 2: Create UI Panels (2 minutes)

#### Panel 1: Assaa Yes/No Prompt

1. Right-click Canvas → UI → Panel (name it "AssaaPromptPanel")
2. Add these as children:
   - TextMeshPro Text (name: "MessageText")
   - Button (name: "YesButton") with child TextMeshPro Text
   - Button (name: "NoButton") with child TextMeshPro Text

#### Panel 2: Card Reorder UI

1. Right-click Canvas → UI → Panel (name it "AssaaReorderPanel")
2. Add these as children:
   - TextMeshPro Text (name: "TitleText")
   - TextMeshPro Text (name: "InstructionsText")
   - TMP InputField (name: "StartPositionInput")
   - TMP InputField (name: "TargetPositionInput")
   - Button (name: "ConfirmButton")
   - Button (name: "CancelButton")
   - TextMeshPro Text (name: "ErrorText")
   - TextMeshPro Text (name: "PreviewText")

**Important**: Hide both panels by unchecking them in Inspector!

### Step 3: Connect References (1 minute)

#### In RassaGameIntegration
- Drag `AssaaSystem` component → `assaaSystem` field

#### In AssaaSystem
- Drag `AssaaPromptUI` component → `assaaPromptUI` field
- Drag `AssaaCardReorderUI` component → `assaaCardReorderUI` field
- ✓ Check `enableAssaaSystem`

#### In AssaaPromptUI
- Drag panel/UI elements to their matching fields:
  - `promptPanel` → AssaaPromptPanel
  - `messageText` → MessageText
  - `yesButton` → YesButton
  - `noButton` → NoButton
  - etc.

#### In AssaaCardReorderUI
- Drag panel/UI elements to their matching fields:
  - `reorderPanel` → AssaaReorderPanel
  - `titleText` → TitleText
  - `startPositionInput` → StartPositionInput
  - `targetPositionInput` → TargetPositionInput
  - `confirmButton` → ConfirmButton
  - `cancelButton` → CancelButton
  - `errorText` → ErrorText
  - `previewText` → PreviewText

### ✅ Done! Test It

1. Start the game
2. Choose Rassa (YES) when prompted
3. You should see the Assaa prompt appear for the player to the right

## 🎮 How to Use In-Game

### As a Player

1. **When asked "Use Assaa?"**
   - Click YES if you want to reorder the deck
   - Click NO to decline

2. **In the Card Reorder screen:**
   - Enter **Start Position** (1-32): Where to start selecting cards
   - Enter **Target Position**: Where to move selected cards (must be less than start)
   - Click **Confirm** to apply changes
   - Click **Cancel** to skip reordering

### Example

Want to move cards 10-32 to position 5?
- Start Position: `10`
- Target Position: `5`
- Result: 23 cards (10-32) move to position 5

## 🎨 Quick Styling Tips

### Make panels look nice:
- Add background image to panels
- Adjust text colors and sizes
- Style buttons with colors/images
- Add padding/margins for spacing

### Recommended sizes:
- Prompt Panel: 600x400 pixels
- Reorder Panel: 800x600 pixels
- Button size: 150x60 pixels
- Text size: 24-36 for titles, 18-24 for body

## ⚙️ Settings

### Enable/Disable Assaa
In `AssaaSystem` component:
- ✓ `enableAssaaSystem` = ON
- ☐ `enableAssaaSystem` = OFF (completely disabled)

### AI Behavior
In `AssaaPromptUI` component:
- `aiCanUseAssaa`: Should AI players use Assaa?
- `aiAssaaChance`: Probability (0-100) AI says YES

## 🐛 Common Issues

**Problem**: UI doesn't show  
**Fix**: Make sure panels are initially hidden (unchecked in Inspector)

**Problem**: Can't click buttons  
**Fix**: Ensure Canvas has a GraphicRaycaster component

**Problem**: Input fields don't work  
**Fix**: Verify TMP InputField components are properly configured

**Problem**: No teammate is asked  
**Fix**: Check that player teams are set up correctly (Team1: South+North, Team2: West+East)

## 📚 For More Details

See `ASSAA_SYSTEM_README.md` for:
- Complete feature explanation
- Event flow diagrams
- Technical details
- Advanced customization
- Troubleshooting guide

---

**Need Help?** Check the full README or review the component tooltips in Unity Inspector.

