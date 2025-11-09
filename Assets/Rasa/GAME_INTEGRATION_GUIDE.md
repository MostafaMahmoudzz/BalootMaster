# Rassa Game Integration Guide

## 🎯 Overview

This guide shows you how to integrate the Rassa system into your Baloot game so that:
1. Before bidding starts (and before cards are dealt), the current bidder is asked: "Play with Rassa?"
2. If they select **NO** → Game continues with random shuffled deck
3. If they select **YES** → Deck is arranged according to saved Rassa order from CardInfoScriptable

---

## ✅ What Has Been Implemented

### New Scripts Created:
1. **RassaDeckManager.cs** - Manages applying Rassa order to the deck
2. **RassaEvents.cs** - Event system for Rassa communication
3. **RassaPromptUI.cs** - UI dialog that asks the player
4. **RassaGameIntegration.cs** - Bridges Rassa system with GameStage

### Modified Scripts:
1. **GameStage.cs** - Integrated Rassa check before dealing cards

---

## 📋 Step-by-Step Integration Setup

### Step 1: Setup the Rassa Prompt UI (3 minutes)

1. **Open your main game scene** (where BeloteGame runs)

2. **Create the Rassa Prompt Panel:**
   - Right-click Canvas → UI → Panel
   - Rename to "RassaPromptPanel"
   - Position: Center of screen
   - Size: 600×300
   - Background: Semi-transparent black (or your style)

3. **Create Message Text:**
   - Right-click RassaPromptPanel → UI → Text - TextMeshPro
   - Rename to "MessageText"
   - Position: Top center of panel
   - Text: "Player, Play with Rassa?"
   - Font Size: 32
   - Alignment: Center

4. **Create Yes Button:**
   - Right-click RassaPromptPanel → UI → Button - TextMeshPro
   - Rename to "YesButton"
   - Position: Left side, Y = -50
   - Size: 200×60
   - Button Text: "YES"
   - Color: Green (optional)

5. **Create No Button:**
   - Right-click RassaPromptPanel → UI → Button - TextMeshPro
   - Rename to "NoButton"
   - Position: Right side, Y = -50
   - Size: 200×60
   - Button Text: "NO"
   - Color: Red (optional)

6. **Hide the panel by default:**
   - Select RassaPromptPanel
   - Uncheck the checkbox at the top to disable it

---

### Step 2: Setup RassaPromptUI Component (2 minutes)

1. **Add RassaPromptUI script:**
   - Select RassaPromptPanel
   - Add Component → RassaPromptUI

2. **Assign references in Inspector:**
   - Prompt Panel: Drag RassaPromptPanel itself
   - Message Text: Drag MessageText
   - Yes Button: Drag YesButton
   - No Button: Drag NoButton
   - Yes Button Text: Expand YesButton, drag the child "Text (TMP)"
   - No Button Text: Expand NoButton, drag the child "Text (TMP)"

3. **Customize message (optional):**
   - Edit "Prompt Message" field to your liking

---

### Step 3: Setup RassaDeckManager (2 minutes)

1. **Create RassaDeckManager GameObject:**
   - Right-click in Hierarchy → Create Empty
   - Rename to "RassaDeckManager"

2. **Add RassaDeckManager component:**
   - Add Component → RassaDeckManager

3. **Assign Rassa ScriptableObject:**
   - In Inspector, find "Saved Rassa Order"
   - Drag your `RassaCardOrder` ScriptableObject (the one you created earlier)

---

### Step 4: Setup RassaGameIntegration (2 minutes)

1. **Find your BeloteGame GameObject:**
   - In Hierarchy, find the GameObject that has the `BeloteGame` component

2. **Add RassaGameIntegration component:**
   - Select the BeloteGame GameObject
   - Add Component → RassaGameIntegration

3. **Assign references in Inspector:**
   - Rassa Deck Manager: Drag the RassaDeckManager GameObject
   - Rassa Prompt UI: Drag the RassaPromptPanel GameObject (or its parent if you organized it)

4. **Configure settings:**
   - Enable Rassa System: ✓ Checked
   - Ask Every Round: 
     - ✓ Check if you want to ask every round
     - ☐ Uncheck if you only want to ask in Round 1 (recommended)

---

### Step 5: Test the Integration! (5 minutes)

1. **Make sure you have a saved Rassa order:**
   - Open the Rassa scene
   - Select all 32 cards in your desired order
   - Click "Done" to save

2. **Return to your main game scene**

3. **Press Play ▶**

4. **What should happen:**
   ```
   Round 1 starts
   ↓
   (Before cards are dealt)
   ↓
   Dialog appears: "Player, Play with Rassa?"
   ↓
   Player clicks YES or NO
   ↓
   If YES: Deck is arranged with Rassa order
   If NO: Deck remains randomly shuffled
   ↓
   Cards are dealt to players
   ↓
   Bidding starts normally
   ```

5. **Check the Console** for debug messages:
   - "[RassaGameIntegration] Asking [PlayerName] about using Rassa"
   - "[RassaGameIntegration] Player chose: Use Rassa / Random deck"
   - "[RassaDeckManager] Deck arranged successfully!"

---

## 🔧 Configuration Options

### RassaGameIntegration Settings

**Enable Rassa System:**
- If checked: System is active
- If unchecked: Game proceeds normally without asking

**Ask Every Round:**
- If checked: Player is asked every round
- If unchecked: Player is asked only in Round 1, choice applies to whole game

---

## 🎮 How It Works (Technical Flow)

```
GameStage.StartRound()
    ↓
    Determines RoundFirstPlayer (will be the bidder)
    ↓
    Calls RassaGameIntegration.CheckRassaBeforeDealing()
    ↓
    [If Rassa order exists]
    ↓
    Sends RassaPromptEvent
    ↓
    RassaPromptUI shows dialog
    ↓
    Player clicks YES or NO
    ↓
    RassaPromptUI sends RassaResponseEvent
    ↓
    RassaGameIntegration receives response
    ↓
    [If YES]
        RassaDeckManager.ArrangeDeckWithRassaOrder(deck)
        Deck is arranged in custom order
    [If NO]
        Deck remains shuffled
    ↓
    Sends RassaChoiceCompleteEvent
    ↓
    GameStage.OnRassaChoiceComplete()
    ↓
    GameStage.ContinueRoundAfterRassa()
    ↓
    DealCards() - Cards are dealt to players
    ↓
    StartBiddingRound() - Bidding begins
```

---

## 🐛 Troubleshooting

### Problem: Dialog doesn't appear
**Solutions:**
- Check that RassaPromptUI component is in the scene
- Check that the panel is assigned to RassaPromptUI
- Check that you have a valid saved Rassa order (32 cards)
- Check Console for warnings

### Problem: "No saved Rassa order" message
**Solutions:**
- Open Rassa scene
- Select all 32 cards
- Click "Done" to save
- Verify the ScriptableObject in RassaDeckManager is assigned

### Problem: Dialog appears but buttons don't work
**Solutions:**
- Check that Yes/No buttons are assigned in RassaPromptUI Inspector
- Check that button text components are assigned
- Check Console for errors

### Problem: Deck is not arranged correctly
**Solutions:**
- Check that RassaDeckManager is assigned in RassaGameIntegration
- Check that ScriptableObject has 32 cards saved
- Check Console for "[RassaDeckManager] Deck arranged successfully!"
- If you see errors, verify your saved card order

### Problem: Game freezes waiting for response
**Solutions:**
- Make sure buttons have onClick listeners
- Check that RassaPromptUI Start() method ran (should auto-setup)
- Try clicking the buttons - they might be there but invisible

---

## 🎨 Customization

### Change the prompt message:
Edit in RassaPromptUI Inspector → Prompt Message

### Add a timer (auto-select NO after X seconds):
Edit in RassaPromptUI Inspector → Display Duration (set to > 0)

### Only ask specific players:
Modify `RassaGameIntegration.CheckRassaBeforeDealing()` to check player type

### Different Rassa orders:
Create multiple ScriptableObjects and swap them in RassaDeckManager

---

## 📊 Verification Checklist

- [ ] RassaPromptPanel created in Canvas
- [ ] Yes and No buttons created
- [ ] RassaPromptUI component added and configured
- [ ] RassaDeckManager GameObject created
- [ ] RassaDeckManager component added
- [ ] ScriptableObject assigned to RassaDeckManager
- [ ] RassaGameIntegration added to BeloteGame GameObject
- [ ] References assigned in RassaGameIntegration
- [ ] Rassa order saved (32 cards in ScriptableObject)
- [ ] Tested in Play mode
- [ ] Dialog appears before bidding
- [ ] YES button arranges deck
- [ ] NO button uses random deck
- [ ] Cards are dealt after choice
- [ ] Bidding starts normally

---

## 💡 Advanced Features

### Multiple Save Slots
Create multiple ScriptableObjects and let players choose which one to use

### Rassa Preview
Show the first few cards of the Rassa order in the prompt dialog

### AI Players with Rassa
Modify AI to automatically respond based on game state

### Rassa Statistics
Track how often players use Rassa and win rates

---

## 🎉 You're Done!

The Rassa system is now fully integrated into your game!

**Testing Tips:**
1. Test with YES → verify cards come in the order you saved
2. Test with NO → verify cards are random
3. Test multiple rounds → verify behavior is consistent

**Next Steps:**
- Polish the UI dialog appearance
- Add sound effects for the prompt
- Add animations for the dialog
- Create multiple Rassa presets

---

**Need Help?** Check the Console for detailed debug logs, or review the implementation in the scripts.

**Version:** 1.0  
**Integration:** Complete  
**Status:** ✅ Ready for Production

