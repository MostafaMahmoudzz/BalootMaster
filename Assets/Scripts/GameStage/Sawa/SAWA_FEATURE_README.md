# Sawa Feature - Complete Implementation Guide

## Overview
The **Sawa** feature allows a player to claim all remaining tricks when they have an unbeatable hand, skipping the tedious process of playing out cards when the outcome is already determined. This is a common quality-of-life feature in trick-taking card games like Baloot.

---

## 🎯 Feature Behavior

### When Can a Player Claim Sawa?

A player can claim Sawa when **both** of the following conditions are met:

1. **It's their turn to play** - The player must be the current active player
2. **They can win all remaining tricks** - Their hand is in a position where they're guaranteed to win every upcoming trick

### What Happens When Sawa is Claimed?

1. All remaining tricks are automatically resolved in favor of the claiming player
2. All cards from all players' hands are played out (simulated)
3. The claiming player's team wins all those tricks
4. The claiming player's team gets the "10 de der" bonus (last trick bonus)
5. Points are calculated and the round ends
6. The next round begins automatically

---

## 📁 File Structure

```
Assets/Scripts/GameStage/Sawa/
├── SawaDetector.cs          # Logic to detect if player can claim Sawa
├── SawaAutoPlay.cs          # Auto-resolves remaining tricks
├── SawaEvents.cs            # Event classes (SawaAvailableEvent, SawaClaimedEvent)
└── SAWA_FEATURE_README.md   # This file

Assets/Scripts/GameStage/
└── GameStage.cs             # Integration: checks Sawa availability, handles claims

Assets/Scripts/GameStage/UI/
└── SawaUI.cs                # UI button component

Assets/Scripts/Player/
└── Player.cs                # Added ClaimSawa() method for programmatic claiming
```

---

## 🔧 Technical Implementation

### 1. SawaDetector.cs

**Purpose:** Determines if a player can win all remaining tricks.

**Key Method:**
```csharp
public static bool CanClaimSawa(
    Player player, 
    Fold currentFold, 
    Card32Family? trump, 
    List<Player> allPlayers)
```

**Detection Logic:**
- Checks if player can win the current partially-played fold (if any)
- Analyzes if player has enough "winning power" for all future tricks
- Uses a greedy simulation approach to verify winnability
- Considers trump suits, card rankings, and Baloot rules

**Limitations:**
The current implementation uses heuristics for performance. Perfect analysis of all possible card play sequences is computationally expensive, so we use practical approximations:
- Checks for unbeatable trump cards
- Verifies highest cards in each suit
- Simulates optimal opponent play

### 2. SawaAutoPlay.cs

**Purpose:** Automatically resolves all remaining tricks when Sawa is claimed.

**Key Method:**
```csharp
public static void AutoResolveRemainingTricks(
    Player claimingPlayer, 
    Fold currentFold, 
    Card32Family? trump, 
    List<Player> allPlayers,
    List<Fold>[] pastFolds,
    GameStage gameStage)
```

**Auto-Resolution Process:**
1. Complete any partially-played fold
2. For each remaining trick:
   - Create a new Fold
   - Move one card from each player's hand to the fold
   - Award the fold to the claiming player
   - Add fold points to the claiming player's team
3. Set the claiming team as the last folding team (for "10 de der")

### 3. SawaEvents.cs

**Event Classes:**

```csharp
// Dispatched when Sawa becomes available or unavailable
public class SawaAvailableEvent : PooledEvent
{
    public Player Player { get; set; }
    public bool IsAvailable { get; set; }
}

// Dispatched when a player claims Sawa
public class SawaClaimedEvent : PooledEvent
{
    public Player Player { get; set; }
}
```

### 4. GameStage.cs Integration

**New Methods:**

```csharp
// Called every turn to check if current player can claim Sawa
void CheckSawaAvailability(Player player)

// Handles Sawa claim events
void OnSawaClaimed(SawaClaimedEvent evt)
```

**Flow:**
1. When a turn starts, `CheckSawaAvailability()` is called
2. `SawaDetector.CanClaimSawa()` checks if Sawa is possible
3. `SawaAvailableEvent` is dispatched to notify UI
4. If player clicks Sawa button, `SawaClaimedEvent` is dispatched
5. `OnSawaClaimed()` verifies the claim and calls `SawaAutoPlay`
6. Round ends and next round begins

### 5. SawaUI.cs

**Purpose:** Displays the Sawa button and handles player interaction.

**Features:**
- Creates a styled button with green background
- Shows Arabic text "صوا (Sawa)"
- Positions at bottom-center of screen
- Appears only when Sawa is available
- Hides automatically after claiming or when round ends
- Includes both Canvas-based UI and OnGUI fallback for testing

**Button Appearance:**
- Color: Green (#33B333)
- Size: 200x60 pixels
- Position: Bottom center, 150px from bottom
- Text: "صوا (Sawa)" in Arabic with English transliteration

### 6. Player.cs

**New Method:**
```csharp
public void ClaimSawa()
```

Allows programmatic claiming of Sawa (useful for AI players in the future).

---

## 🎮 Usage

### For Players (UI)

1. During your turn, if you have an unbeatable hand, a green "صوا (Sawa)" button will appear at the bottom of the screen
2. Click the button to claim Sawa
3. All remaining tricks will be automatically won by your team
4. The round will end and points will be calculated
5. A new round will begin

### For Developers (Programmatic)

```csharp
// Check if a player can claim Sawa
bool canClaim = SawaDetector.CanClaimSawa(
    player, 
    gameStage.CurrentFold, 
    gameStage.Trump, 
    gameStage.Players
);

// Claim Sawa programmatically (for AI)
if (canClaim)
{
    player.ClaimSawa();
}
```

---

## 🔍 Testing & Debugging

### Debug Logs

The system outputs detailed debug logs:

```
[GameStage] {PlayerName} can claim Sawa!
[SawaUI] Sawa button shown
[SawaUI] {PlayerName} clicked Sawa button!
[GameStage] === {PlayerName} CLAIMED SAWA ===
[SawaAutoPlay] {PlayerName} claimed Sawa! Auto-resolving remaining tricks...
[SawaAutoPlay] Auto-resolving {N} remaining tricks
[SawaAutoPlay] Trick 1/N: 4 cards, {Points} points
[SawaAutoPlay] === Sawa Complete ===
[GameStage] Sawa complete - ending round
```

### Common Issues

**Issue:** Button doesn't appear
- **Solution:** Check that `SawaUI` GameObject is created in `GameStage.OnInit()`
- **Solution:** Ensure it's a player's turn (not during bidding)
- **Solution:** Verify player actually has unbeatable cards

**Issue:** Claiming Sawa doesn't work
- **Solution:** Check that `SawaClaimedEvent` is properly subscribed in `GameStage`
- **Solution:** Verify `SawaDetector` logic is correctly identifying winnability

**Issue:** Points not calculated correctly
- **Solution:** Check `SawaAutoPlay.AutoResolveRemainingTricks()` fold creation
- **Solution:** Verify "10 de der" is awarded to claiming team

---

## 🎨 Customization

### Change Button Appearance

Edit `SawaUI.cs`:

```csharp
// Button size
private const float BUTTON_WIDTH = 200f;
private const float BUTTON_HEIGHT = 60f;

// Button position
private const float BUTTON_BOTTOM_OFFSET = 150f;

// Button color
buttonImage.color = new Color(0.2f, 0.7f, 0.2f, 0.9f); // Green
```

### Change Button Text

Edit `SawaUI.cs`:

```csharp
m_buttonText.text = "صوا (Sawa)"; // Change this
m_buttonText.fontSize = 24;       // Change font size
```

### Adjust Detection Sensitivity

Edit `SawaDetector.cs` methods to make detection more or less strict:
- `CanWinAllFutureFolds()` - Main winnability check
- `HasUnbeatableTrumps()` - Trump checking logic

---

## 🚀 Future Enhancements

### Potential Improvements:

1. **AI Support:** Implement AI logic to automatically claim Sawa when appropriate
2. **Animation:** Add visual effects when Sawa is claimed (card animations, particle effects)
3. **Sound Effects:** Play a special sound when Sawa button appears or is clicked
4. **Confirmation Dialog:** Add "Are you sure?" dialog before claiming
5. **Perfect Detection:** Implement more sophisticated card analysis for 100% accurate detection
6. **Statistics:** Track how many times each player claims Sawa
7. **Tutorial:** Add first-time tutorial explaining what Sawa is

---

## 📝 Notes

### Game Rules Compliance

The Sawa feature follows standard Baloot rules:
- Only available during the playing phase (not bidding)
- Requires player to be able to win ALL remaining tricks
- Properly awards points and "10 de der" bonus
- Respects team partnerships

### Performance

The feature is designed to be lightweight:
- Detection runs only once per turn
- UI updates are event-driven
- Auto-resolution is instantaneous (no animations)

### Multiplayer Considerations

If implementing multiplayer:
- Ensure only the current player's client shows the Sawa button
- Validate Sawa claims on the server
- Broadcast Sawa claim to all clients for synchronization

---

## 📖 Related Documentation

- **Baloot Game Rules:** See `SCORING_SYSTEM_IMPLEMENTATION.md`
- **Event System:** See `Assets/Core/Event/` documentation
- **Fold/Trick System:** See `Assets/Scripts/Cards/Fold/Fold.cs`
- **Player System:** See `Assets/Scripts/Player/Player.cs`

---

## ✅ Implementation Checklist

- [x] SawaDetector logic implemented
- [x] SawaAutoPlay system implemented
- [x] Event classes created (SawaAvailableEvent, SawaClaimedEvent)
- [x] GameStage integration complete
- [x] SawaUI button component created
- [x] Player.ClaimSawa() method added
- [x] Event subscriptions and cleanup handled
- [x] Debug logging added
- [x] No linter errors
- [x] Documentation complete

---

## 🎉 Conclusion

The Sawa feature is now fully implemented and ready to use! Players will appreciate not having to play out obvious wins, making the game flow much smoother.

**Created:** November 16, 2025
**Author:** AI Assistant
**Version:** 1.0

