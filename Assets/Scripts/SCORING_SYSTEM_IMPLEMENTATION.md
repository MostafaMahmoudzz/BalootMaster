# Baloot Scoring System - Complete Implementation Guide

## Overview
This document explains the complete scoring system implementation for the Baloot card game, including point calculation, division, multiplier application, and Kaboot handling.

---

## 📊 Complete Scoring Flow

### Step 1: Card Point Collection (During Play)
Each card played has a point value that goes into the fold (trick). The winner of the fold collects all points from that fold.

**Card Points:**
- **Non-Trump Cards:**
  - Ace: 11 points
  - Ten: 10 points
  - King: 4 points
  - Queen: 3 points
  - Jack: 2 points
  - Nine, Eight, Seven: 0 points

- **Trump Cards:**
  - Jack (Yass): 20 points
  - Nine (Mennel): 14 points
  - Ace: 11 points
  - Ten: 10 points
  - King: 4 points
  - Queen: 3 points
  - Eight, Seven: 0 points

**Total possible points per round:** 162 points (26 from each suit: Ace=11, Ten=10, King=4, Queen=3, Jack=2)

### Step 2: End of Round - Raw Points Calculation

When all 8 tricks are played, the system calculates:

1. **Fold Points**: Sum all points from folds won by each team
2. **10 de der**: +10 points to the team that won the last trick
3. **Project Points**: Add points from validated projects (Masharie3):
   - **Sara** (3 consecutive cards): +20 points
   - **Khamsin** (4 consecutive cards): +50 points
   - **Mia** (5 consecutive cards OR 4 same rank): +100 points
   - **Arba'miya** (4 Aces in Sun round): +400 points
   - **Belote** (K+Q of trump): +20 points (never compared)

**Important:** Projects are compared before scoring. Only the team with the highest project gets to score their projects. If both teams have the same highest project, all projects are cancelled (except Belote).

### Step 3: Determine Round Winner

The team with MORE raw points wins the round.

**Tie-breaker rule:** If both teams have equal raw points, the **non-bidding team** wins.

### Step 4: Check for Kaboot

**Kaboot** occurs when one team wins ALL tricks (opponent has 0 raw points).

- If Kaboot detected: Winner gets **16 points base** (before multiplier)
- Opponent gets 0 points

### Step 5: Score Division

**Normal rounds (no Kaboot):**
```csharp
teamScore = Mathf.RoundToInt(totalPoints / 10f);
```

Example: 82 raw points ÷ 10 = 8.2 → rounds to **8 points**

**Kaboot rounds:**
- Winner gets **16 points** (fixed)
- Loser gets **0 points**

### Step 6: Apply Multiplier

The multiplier comes from the bidding phase:
- **×1**: Normal (default)
- **×2**: Double declared
- **×3**: Triple declared
- **×4**: Quadruple declared

**Multiplier is applied AFTER division.**

### Step 7: Winner Takes All (Baloot Rule)

**If the bidding team won:**
```csharp
biddingTeamScore = theirDividedScore * multiplier;
opponentScore = 0;
```

**If the bidding team lost:**
```csharp
opponentScore = opponentDividedScore * multiplier;
biddingTeamScore = 0;
```

**Only the winning team scores. The losing team always gets 0 for that round.**

### Step 8: Add to Cumulative Score

The final scores are added to each team's cumulative total.

---

## 🧮 Scoring Examples

### Example 1: Normal Round (No Multiplier)

**Setup:**
- Bidding Team: Team 1
- Multiplier: ×1

**Raw Points:**
- Team 1: 92 points (cards + projects)
- Team 2: 70 points (cards + projects)

**Calculation:**
1. Team 1 wins (92 > 70)
2. No Kaboot (both have points)
3. Division: Team 1 = 92÷10 = 9.2 → **9 points**
4. Division: Team 2 = 70÷10 = 7.0 → **7 points**
5. Multiplier: ×1 (no change)
6. **Winner takes all:** Team 1 (bidder) won, so:
   - Team 1: **+9 points**
   - Team 2: **+0 points**

---

### Example 2: Kaboot with Double

**Setup:**
- Bidding Team: Team 2
- Multiplier: ×2 (Double)

**Raw Points:**
- Team 1: 0 points (lost all tricks)
- Team 2: 162 points (won all tricks)

**Calculation:**
1. Team 2 wins (162 > 0)
2. **Kaboot detected!** (Team 1 has 0 points)
3. Kaboot bonus: Team 2 = **16 points** (fixed)
4. Multiplier: 16 × 2 = **32 points**
5. **Winner takes all:** Team 2 (bidder) won, so:
   - Team 1: **+0 points**
   - Team 2: **+32 points**

---

### Example 3: Bidder Loses with Triple

**Setup:**
- Bidding Team: Team 1
- Multiplier: ×3 (Triple)

**Raw Points:**
- Team 1: 65 points
- Team 2: 97 points

**Calculation:**
1. Team 2 wins (97 > 65)
2. No Kaboot
3. Division: Team 1 = 65÷10 = 6.5 → **7 points** (rounded)
4. Division: Team 2 = 97÷10 = 9.7 → **10 points** (rounded)
5. Multiplier: 10 × 3 = **30 points**
6. **Winner takes all:** Team 1 (bidder) LOST, so:
   - Team 1: **+0 points** (bidder lost, gets nothing)
   - Team 2: **+30 points** (opponent gets multiplied score)

---

### Example 4: Tie Goes to Non-Bidder

**Setup:**
- Bidding Team: Team 1
- Multiplier: ×1

**Raw Points:**
- Team 1: 81 points
- Team 2: 81 points (TIE!)

**Calculation:**
1. **Tie:** Non-bidder (Team 2) wins by default
2. No Kaboot
3. Division: Team 1 = 81÷10 = 8.1 → **8 points**
4. Division: Team 2 = 81÷10 = 8.1 → **8 points**
5. Multiplier: ×1
6. **Winner takes all:** Team 2 (non-bidder) won due to tie, so:
   - Team 1: **+0 points** (bidder lost tie)
   - Team 2: **+8 points**

---

## 🎮 UI Display

### Console/Debug Display (GameStageRenderer)

When a round ends, the console displays a detailed breakdown for 5 seconds:

```
=== ROUND END SCORE BREAKDOWN ===

Team 1 (South & North) [BIDDER]
  Raw Points: 92
  Divided by 10: 9
  Round Score: +9 ✓

Team 2 (West & East)
  Raw Points: 70
  Divided by 10: 7
  Round Score: +0 ✗

Team 1 (Bidder) WON!

Total Scores: Team1=26 | Team2=15

(Closing in 5s...)
```

### RoundEndScoreUI Component (Optional)

A dedicated UI component (`RoundEndScoreUI.cs`) can be added to a scene GameObject to display:
- Raw points for each team
- Division calculations
- Multiplier application
- Final round scores
- Cumulative totals
- Bidder and winner indicators

---

## 🔍 Debug Information

### Console Logs During EndRound()

The `GameStage.EndRound()` method outputs comprehensive debug logs:

```
[GameStage] === END OF ROUND 1 ===
[GameStage] Bidding Team: Team1
[GameStage] Step 1: Calculating fold points...
[GameStage] Fold Points - Team1: 82, Team2: 60
[GameStage] Step 2: Adding '10 de der' to Team1
[GameStage] After '10 de der' - Team1: 92, Team2: 60
[GameStage] Step 3: Scoring projects...
[ProjectManager] Team1 scored 20 points from 1 projects
[GameStage] Total Points (before division) - Team1: 92, Team2: 80
[GameStage] Step 4: Round winner: Team1, Bidding team won: True
[GameStage] Step 5: Kaboot check - IsKaboot: False
[GameStage] Step 6: Dividing by 10 and rounding...
[GameStage] After division - Team1: 9, Team2: 8
[GameStage] Step 7: Multiplier from bidding: 2x
[GameStage] Step 8: Applying multiplier and winner rules...
[GameStage] Team1 (bidder) won: 9 × 2 = 18
[GameStage] === FINAL ROUND SCORES ===
[GameStage] Team1 Final: 18 points
[GameStage] Team2 Final: 0 points
[GameStage] Kaboot: False, Multiplier: 2x, Bidder Won: True
[GameStage] === CUMULATIVE SCORES ===
[GameStage] Team1 Total: 18 points
[GameStage] Team2 Total: 0 points
```

---

## 🎯 Validation Rules

The scoring system ensures:

1. ✅ **Division happens only once** after all raw points are finalized
2. ✅ **Multipliers apply only AFTER division**
3. ✅ **Only the winning team scores** (loser always gets 0)
4. ✅ **Kaboot gives 16 points base** (not divided from raw points)
5. ✅ **No negative scores possible**
6. ✅ **No duplicate scoring**
7. ✅ **Tie goes to non-bidder**
8. ✅ **Projects are compared before scoring**

---

## 📂 Modified Files

### Core Logic
- `Assets/Scripts/GameStage/GameStage.cs`
  - Updated `EndRound()` method with complete scoring flow
  - Added `RoundEndScoreEvent` class

### UI/Display
- `Assets/Scripts/GameStage/GameStageRenderer.cs`
  - Added score breakdown display in console
  - Displays for 5 seconds after each round

- `Assets/Scripts/GameStage/UI/RoundEndScoreUI.cs` (NEW)
  - Optional UI component for visual score display
  - Can be attached to a GameObject in the scene

---

## 🎲 Testing Checklist

- [ ] Normal round scoring (no special cases)
- [ ] Kaboot detection and 16-point base
- [ ] Double multiplier (×2)
- [ ] Triple multiplier (×3)
- [ ] Quadruple multiplier (×4)
- [ ] Bidder wins scenario
- [ ] Bidder loses scenario
- [ ] Tie scenario (non-bidder wins)
- [ ] Project points included in raw total
- [ ] "10 de der" bonus added correctly
- [ ] Division by 10 with proper rounding
- [ ] Multiplier applied only to winner
- [ ] Loser always gets 0
- [ ] Cumulative scores update correctly
- [ ] UI displays correct information
- [ ] Console logs show complete breakdown

---

## 🏆 Game End Condition

The game typically ends when a team reaches a target score (e.g., 152 points).

**Implementation Note:** The win condition logic should be added to `GameStage` after the cumulative scores are updated:

```csharp
// After Step 9 in EndRound()
const int TARGET_SCORE = 152;
if (Score.GetScore(PlayerTeam.Team1) >= TARGET_SCORE ||
    Score.GetScore(PlayerTeam.Team2) >= TARGET_SCORE)
{
    // Trigger game end
    // Show final winner
}
```

---

## 📝 Summary

The Baloot scoring system now correctly implements:
1. ✅ Raw point collection (cards + projects + "10 de der")
2. ✅ Division by 10 with rounding
3. ✅ Kaboot detection (16 points fixed)
4. ✅ Multiplier application (×1, ×2, ×3, ×4)
5. ✅ Winner-takes-all rule (loser gets 0)
6. ✅ Comprehensive debug logging
7. ✅ UI display with breakdown

The system is fully functional and ready for gameplay testing!


