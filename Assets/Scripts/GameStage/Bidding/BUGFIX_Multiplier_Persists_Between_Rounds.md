# Bug Fix: Multiplier Bidding State Persists Between Rounds

**Date:** November 17, 2025  
**Status:** ✅ FIXED

## 🐛 The Problem

After playing a round where the multiplier bidding (Double/Triple/Quadruple) was used, when a new round started, the bidding UI would still show "Double" or the previous multiplier state from the last round, even though the game should have reset to normal 1x bidding.

### User Report
> "Either I play a line, then I choose a referee and play until the end, and the game ends and we say we won't play a line, I find the bidding is on double, the bidding should go back, the round is over, it's perfectly normal"

## 🔍 Root Cause

The issue was in `BiddingUI.cs`. The UI maintains cached state variables for the multiplier bidding phase:
- `m_inMultiplierBidding` (bool) - Whether in multiplier bidding phase
- `m_currentMultiplier` (int) - Current multiplier (1, 2, 3, or 4)
- `m_trumpConfirmer` (Player) - Player who confirmed trump
- `m_isOpposingTeamTurn` (bool) - Whose turn it is in multiplier bidding

### What Was Happening:

1. **Round N (with Double):**
   - Player confirms trump in Round 2
   - Multiplier bidding starts → `m_inMultiplierBidding = true`
   - Player chooses "Double" → `m_currentMultiplier = 2`
   - UI correctly shows "Double (2x)" options
   - Round plays out and ends

2. **Round N+1 Starts:**
   - `GameStage.EndRound()` → `GameStage.StartRound()` called
   - `BelootBiddingSystem.Reset()` called ✅ (backend properly resets)
   - `BelootBiddingSystem.StartBidding()` called ✅ (backend starts fresh)
   - `BiddingStartEvent` sent to UI
   - `BiddingUI.OnBiddingStart()` called
   - ❌ **BUT**: UI's multiplier state was NOT reset!
   - Result: `m_inMultiplierBidding` still `true`, `m_currentMultiplier` still `2`
   - UI incorrectly shows "Double (2x)" options in the new round

### Why Backend Was OK But UI Wasn't:

The `BelootBiddingSystem.Reset()` method (lines 748-781) properly resets all multiplier state:
```csharp
m_inMultiplierBidding = false;
m_currentMultiplier = 1;
m_trumpConfirmer = null;
m_opposingBidder = null;
m_lastMultiplierBidder = null;
m_isOpposingTeamTurn = false;
```

However, the UI's `OnBiddingStart()` method was resetting other state variables like:
- `m_anotherTrumpChosen`
- `m_trumpChosen`
- `m_ignoreBiddingTurnEvents`
- `m_preventBidSubmission`

But **forgot to reset the multiplier bidding state**!

## ✅ The Fix

### File: `Assets/Scripts/GameStage/Bidding/BiddingUI.cs`

**Modified Method**: `OnBiddingStart()` (around line 183)

**Added lines 197-201:**
```csharp
// Reset multiplier bidding state for new round
m_inMultiplierBidding = false;
m_currentMultiplier = 1;
m_trumpConfirmer = null;
m_isOpposingTeamTurn = false;
```

### Complete Context:
```csharp
void OnBiddingStart(BiddingStartEvent evt)
{
    m_isBiddingActive = true;
    
    // Cache the event values (fallback only)
    m_currentBidder = evt.CurrentBidder;
    m_highestBid = evt.HighestBid;
    m_currentBiddingRound = evt.Round;
    m_faceUpCard = evt.FaceUpCard;
    m_anotherTrumpChosen = false;
    m_trumpChosen = false;
    
    // Clear the ignore flags when new bidding starts
    m_ignoreBiddingTurnEvents = false;
    m_preventBidSubmission = false;
    
    // Reset multiplier bidding state for new round  ← NEW!
    m_inMultiplierBidding = false;                    ← NEW!
    m_currentMultiplier = 1;                          ← NEW!
    m_trumpConfirmer = null;                          ← NEW!
    m_isOpposingTeamTurn = false;                     ← NEW!
    
    // ... rest of method
}
```

## ✨ Result

Now when a new round starts:
1. Backend resets properly (was already working) ✅
2. UI also resets multiplier state ✅
3. Both backend and UI start fresh with 1x multiplier ✅
4. Bidding UI shows correct normal options (Pass/Trump/Sun) ✅
5. If trump is confirmed again, multiplier bidding starts fresh from 1x ✅

## 🧪 Testing Checklist

- [ ] Play a round with Double (2x multiplier)
- [ ] Complete the round
- [ ] Start a new round
- [ ] Verify bidding UI shows normal options (NOT double)
- [ ] Verify multiplier bidding can be triggered again in new round
- [ ] Verify it starts from 1x (not continuing from previous round)

## 📝 Related Files

- **Fixed:** `Assets/Scripts/GameStage/Bidding/BiddingUI.cs`
- **Already Working:** `Assets/Scripts/GameStage/Bidding/BelootBiddingSystem.cs` (Reset method)
- **Already Working:** `Assets/Scripts/GameStage/GameStage.cs` (calls Reset properly)

## 🎓 Lesson Learned

When maintaining cached state in UI components, always ensure that cache is properly synchronized with backend state changes, especially during state transitions like:
- Round starts
- Round ends
- Game resets
- Phase transitions

The UI should subscribe to initialization events (like `BiddingStartEvent`) and use them as opportunities to reset all cached state to match the backend's fresh state.

---

**Status**: ✅ **FIXED AND READY FOR TESTING**

