# Sawa Button Behavior - Complete Explanation

## ✅ Current Behavior (Updated)

The Sawa button now follows the **correct Baloot rules** for when a player can claim all remaining tricks.

---

## 🎯 When Does Sawa Button Appear?

The button appears **ONLY** when **ALL** of these conditions are met:

### 1. ✅ Player's Turn
It must be the current player's turn to play.

### 2. ✅ Leading the Trick
Player must be **throwing the FIRST card** of the trick (fold is empty).

**Why?** Because claiming Sawa means "I'll win all remaining tricks starting from now". If other players already played cards in this trick, you can't claim those cards back.

### 3. ✅ Can Win All Remaining Tricks
The `SawaDetector` confirms the player has unbeatable cards.

### 4. ✅ At Least One Trick Completed
Not available on the very first trick of the round.

### 5. ✅ Bidding Complete
Trump must be determined (or Sun round).

### 6. ✅ No Prompts Active
Not during Rassa prompt or other interruptions.

---

## 📋 Step-by-Step Flow

### Example Scenario:

**Round 1, Trick 3:**
- South wins Trick 2 (has strong cards)
- **South's turn** (leading Trick 3)
- South has unbeatable cards for remaining tricks

**What happens:**
```
1. South's turn starts
2. Fold is empty (South is leading)
3. System checks: Can South win all remaining tricks?
4. YES! → Sawa button appears
5. South has 2 choices:
   a) Click Sawa → Win all remaining tricks instantly
   b) Play a card → Button disappears
```

---

## 🎮 Two Paths:

### Path A: Click Sawa
```
Player Turn Starts
  ↓
Fold is Empty (Leading)
  ↓
Can Win All Remaining? YES
  ↓
🟢 SAWA BUTTON APPEARS
  ↓
Player Clicks Sawa
  ↓
All remaining tricks won instantly
  ↓
Round ends
```

### Path B: Play Card Instead
```
Player Turn Starts
  ↓
Fold is Empty (Leading)
  ↓
Can Win All Remaining? YES
  ↓
🟢 SAWA BUTTON APPEARS
  ↓
Player Plays a Card (ignores Sawa)
  ↓
🔴 SAWA BUTTON DISAPPEARS
  ↓
Other players play their cards
  ↓
Trick completes
  ↓
Next trick starts...
  ↓
If player is leading again AND can still claim all remaining
  ↓
🟢 SAWA BUTTON APPEARS AGAIN
```

---

## ❌ When Sawa Button Does NOT Appear:

### ❌ Not Leading
```
Trick already started:
- North plays card
- South's turn  ← Sawa NOT available (not leading)
```

**Why?** Can't claim tricks already started by others.

### ❌ Following
```
- West leads with Ace of Spades
- North follows with 7 of Spades
- Your turn  ← Sawa NOT available (following, not leading)
```

**Why?** You're responding to someone else's lead.

### ❌ Can't Win All
```
- Your turn (leading)
- But opponents have higher cards
- Sawa NOT available (can't guarantee winning all)
```

### ❌ First Trick
```
Round just started:
- Everyone has 8 cards
- First trick begins
- Sawa NOT available (need at least 1 completed trick)
```

---

## 🔄 Behavior During Gameplay

### Turn 1 (Trick 1):
- Player leads
- **No Sawa** (first trick of round)

### Turn 2 (Trick 2):
- Player leads
- Checks if can win remaining 6 tricks
- If YES → **Sawa appears**
- Player plays a card
- **Sawa disappears**

### Turn 3 (Trick 3):
- Player leads again
- Checks if can win remaining 5 tricks
- If YES → **Sawa appears again**
- Player clicks Sawa
- All remaining tricks won instantly!

---

## 🎯 Key Logic

### Leading Check:
```csharp
bool isLeading = (m_currentFold.Deck.Size == 0);
```

If fold is empty = player is leading = Sawa can appear

### Hide on Card Play:
```csharp
protected void OnCardPlayed(BeloteCard.Played evt)
{
    // Hide Sawa button when ANY card is played
    // Player chose to play instead of claiming
    SawaAvailableEvent sawaEvt = Pools.Claim<SawaAvailableEvent>();
    sawaEvt.IsAvailable = false;
    GameEventDispatcher.SendEvent(sawaEvt);
}
```

### Show on Next Leading Turn:
```csharp
void CheckSawaAvailability(Player player)
{
    bool isLeading = (m_currentFold.Deck.Size == 0);
    
    if (isLeading)
    {
        canClaimSawa = SawaDetector.CanClaimSawa(...);
    }
}
```

---

## 📊 Timeline Example

```
Trick 1: [Card][Card][Card][Card] → Winner: South
Trick 2: South's turn (leading)
         🟢 Sawa appears (can win all remaining)
         South plays a card
         🔴 Sawa disappears
         [Card][Card][Card][Card] → Winner: South
         
Trick 3: South's turn (leading)
         🟢 Sawa appears again
         South clicks Sawa
         ⚡ Auto-win all remaining tricks!
         🎉 Round ends
```

---

## 🧪 Testing Scenarios

### Test 1: Leading Player
- ✅ Player leads trick
- ✅ Has unbeatable cards
- ✅ Button appears

### Test 2: Following Player
- ✅ Other player leads
- ❌ Button does NOT appear

### Test 3: Play Card
- ✅ Button appears
- ✅ Player plays card
- ✅ Button disappears

### Test 4: Next Turn
- ✅ Player leads next trick
- ✅ Still has unbeatable cards
- ✅ Button appears again

---

## 🎮 Player Experience

**Before:**
- Button appeared even when not leading
- Could be confusing (can't claim tricks already started)

**After:**
- Button only when leading
- Clear: "I'm starting this trick, and I can win all remaining tricks"
- Natural: "Or I can play this one card and decide later"

---

## ✨ Benefits

1. **Correct Rules:** Follows Baloot game rules
2. **Clear Choice:** Player decides each time they lead
3. **No Confusion:** Only appears when actually claimable
4. **Flexible:** Can play normally then claim later

---

## 🔧 Technical Summary

**Leading Detection:**
```csharp
bool isLeading = (m_currentFold.Deck.Size == 0);
```

**Sawa Check:**
```csharp
if (isLeading)
{
    canClaimSawa = SawaDetector.CanClaimSawa(player, m_currentFold, Trump, m_players);
}
```

**Hide on Play:**
```csharp
protected void OnCardPlayed(BeloteCard.Played evt)
{
    // Hide Sawa when card is played
    SendSawaAvailableEvent(false);
}
```

**Show on Next Turn:**
```csharp
void StartTurn(Player player)
{
    // Check again on next turn
    CheckSawaAvailability(player);
}
```

---

**Perfect Baloot Sawa behavior! 🎯**

