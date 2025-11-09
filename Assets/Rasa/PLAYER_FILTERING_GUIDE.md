# Rassa Player Filtering - How It Works

## ✅ What Was Updated

The Rassa prompt now **only shows for human players** who are the current bidder. AI players automatically respond without showing the UI.

---

## 🎮 Behavior

### For Human Players:
1. **Panel shows** with their name prominently displayed
2. They can click **YES** or **NO**
3. Game waits for their response

### For AI Players:
1. **Panel does NOT show**
2. AI automatically responds
3. Game continues immediately

---

## 🎯 How It Works

### Flow:
```
Round Starts
    ↓
Current bidder is determined
    ↓
Is the bidder a HumanPlayer?
    ↓
YES (Human) → Show panel with player's name
    ↓
    Player clicks YES or NO
    ↓
    Game continues
    
NO (AI) → Auto-respond (configurable)
    ↓
    Game continues immediately
```

---

## ⚙️ Settings (In RassaPromptUI Inspector)

### AI Behavior Settings:

**AI Can Use Rassa:**
- ☐ Unchecked (default): AI always uses random deck (NO)
- ✓ Checked: AI can choose to use Rassa

**AI Rassa Chance (0-100):**
- Only used if "AI Can Use Rassa" is checked
- `0` = AI never uses Rassa (always NO)
- `50` = AI uses Rassa 50% of the time
- `100` = AI always uses Rassa (always YES)

**Example:**
- AI Can Use Rassa: ✓ Checked
- AI Rassa Chance: 30
- Result: AI has 30% chance to use Rassa, 70% chance for random deck

---

## 📺 Visual Display

### What Players See:

```
┌─────────────────────────────────────┐
│                                     │
│           **SOUTH**                 │
│                                     │
│   Play with Rassa?                  │
│ (Use your custom card arrangement)  │
│                                     │
│    [  YES  ]         [  NO  ]       │
│                                     │
└─────────────────────────────────────┘
```

- Player name is **large and bold** at the top
- Clear prompt message below
- Two buttons for choice

---

## 🔍 Debug Messages

In Console, you'll see:

### For Human Player:
```
[RassaPromptUI] Received prompt for player: South
[RassaPromptUI] Showing prompt for HUMAN player: South
[RassaPromptUI] Player South chose YES - Use Rassa
```

### For AI Player:
```
[RassaPromptUI] Received prompt for player: East (AI)
[RassaPromptUI] AI player East - auto-responding NO (Random)
```

---

## 🎓 Player Types

### HumanPlayer:
- **Shows UI prompt**
- Waits for manual button click
- Can choose YES or NO

### AIPlayer:
- **Does NOT show UI**
- Automatically responds immediately
- Response based on AI settings

---

## 🔧 Customization

### To Change AI Behavior:

1. Select your RassaPromptPanel in Hierarchy
2. Find the `RassaPromptUI` component
3. Scroll to "AI Behavior" section
4. Adjust settings:

**Make AI never use Rassa (default):**
- AI Can Use Rassa: ☐ Unchecked

**Make AI sometimes use Rassa:**
- AI Can Use Rassa: ✓ Checked
- AI Rassa Chance: 30 (for 30% chance)

**Make AI always use Rassa:**
- AI Can Use Rassa: ✓ Checked
- AI Rassa Chance: 100

---

## 💡 Use Cases

### Single Player vs 3 AI:
- Panel shows only once (for the human player)
- AI players auto-respond instantly
- Game feels smooth

### Multiplayer (Future):
- Each human sees prompt on their turn
- Other players don't see it
- Can be extended for networked games

### Practice/Training Mode:
- Set AI Rassa Chance to 50
- AI will sometimes use Rassa
- Players can see both scenarios

---

## ✅ Testing Checklist

When testing, verify:

- [ ] Panel appears for human player bidder
- [ ] Player name shows clearly at top
- [ ] Panel does NOT appear for AI bidders
- [ ] AI responds automatically
- [ ] Game continues smoothly after both responses
- [ ] Console shows correct debug messages

---

## 🐛 Troubleshooting

**Panel shows for everyone:**
- Make sure you have the latest RassaPromptUI.cs
- Check Console for "Showing prompt for HUMAN player"

**Panel never shows:**
- Check if first bidder is AI (they auto-respond)
- Wait for human player's turn to bid
- Check Console for messages

**Want AI to use Rassa:**
- Enable "AI Can Use Rassa" in RassaPromptUI
- Set AI Rassa Chance > 0

---

**Version:** 1.1  
**Update:** Added player filtering and AI auto-response  
**Status:** ✅ Production Ready

