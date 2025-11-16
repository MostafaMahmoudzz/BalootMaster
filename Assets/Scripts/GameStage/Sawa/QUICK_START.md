# Sawa Feature - Quick Start Guide

## What is Sawa?

**Sawa** (صوا) is a feature that allows players to claim all remaining tricks when they have an unbeatable hand. Instead of playing out cards one by one when the outcome is obvious, the player can press the "Sawa" button to instantly win all remaining tricks.

---

## When Does the Sawa Button Appear?

The green "صوا (Sawa)" button appears when:
1. ✅ It's your turn to play
2. ✅ Your hand can win all remaining tricks

---

## How to Use

### Step 1: Look for the Button
During your turn, if you have unbeatable cards, a green button will appear at the bottom center of the screen:

```
┌─────────────────────┐
│   صوا (Sawa)        │
└─────────────────────┘
```

### Step 2: Click the Button
Simply click the button to claim Sawa.

### Step 3: Automatic Resolution
- All remaining tricks are instantly won by your team
- Points are calculated automatically
- The round ends
- A new round begins

---

## Example Scenario

**Situation:**
- You're playing and it's your turn
- You have: Jack of Hearts (trump), Nine of Hearts (trump), Ace of Spades
- Opponents have only low cards
- The Sawa button appears!

**Action:**
Click "صوا (Sawa)"

**Result:**
- Your team automatically wins the remaining 3 tricks
- You get all the points from those tricks
- Plus the "10 de der" bonus for winning the last trick
- Round ends, next round starts

---

## Benefits

✨ **Saves Time:** No need to play obvious wins card by card
✨ **Less Boring:** Speeds up the game when outcome is clear
✨ **Fair:** Only available when you can truly win all remaining tricks
✨ **Automatic:** Points calculated correctly including bonuses

---

## Technical Details

**Files Added:**
- `SawaDetector.cs` - Detects when Sawa is possible
- `SawaAutoPlay.cs` - Auto-resolves remaining tricks
- `SawaEvents.cs` - Event system for Sawa
- `SawaUI.cs` - The green button you see on screen

**Integration:**
- Fully integrated with `GameStage.cs`
- Works with existing scoring system
- No changes needed to existing gameplay

---

## Notes

⚠️ **Important:** The button only appears when you can win ALL remaining tricks. If there's any doubt, the button won't show.

💡 **Tip:** The detection system uses smart heuristics to determine if you have unbeatable cards. It considers trump suits, card rankings, and all Baloot rules.

🎮 **For AI Players:** The system can be extended to allow AI players to automatically claim Sawa when beneficial.

---

## Troubleshooting

**Q: The button doesn't appear but I think I can win all tricks?**
A: The detection system is conservative to prevent errors. It may not show the button in complex situations even if you could technically win.

**Q: Can I claim Sawa during bidding?**
A: No, Sawa is only available during the playing phase after trump has been determined.

**Q: What if I click Sawa by mistake?**
A: The system double-checks that you can actually win before executing. If you can't win all tricks, it will reject the claim.

**Q: Does my partner get notified when I claim Sawa?**
A: In the current single-player version, the action happens immediately. In multiplayer, all players would see the Sawa claim.

---

**Enjoy faster, smoother gameplay with the Sawa feature! 🎉**

