# Baloot Scoring System - Quick Reference

## 🎯 Scoring Formula

```
1. Calculate Raw Points = Card Points + "10 de der" + Projects
2. Check for Kaboot (opponent has 0 points)
3. If Kaboot: Winner gets 16 points (fixed)
   Else: Divide by 10 and round (Mathf.RoundToInt(points / 10f))
4. Apply Multiplier (×1, ×2, ×3, or ×4)
5. Winner-Takes-All:
   - If bidder won: bidder gets (score × multiplier), opponent gets 0
   - If bidder lost: opponent gets (score × multiplier), bidder gets 0
6. Add to cumulative total
```

---

## 📋 Scoring Steps in Code

Located in: `Assets/Scripts/GameStage/GameStage.cs` → `EndRound()` method

```csharp
// Step 1: Sum fold points
foreach fold → add points to m_roundScore

// Step 2: Add "10 de der" (+10 to last trick winner)
m_roundScore.AddScore(LastFoldingTeam, 10)

// Step 3: Add project points
m_projectManager.ScoreProjects(m_roundScore, Bidder)

// Step 4: Determine winner
PlayerTeam winner = m_roundScore.GetLeadingTeam(Bidder.Team)

// Step 5: Check Kaboot
bool isKaboot = (team1 == 0 || team2 == 0)

// Step 6: Divide by 10
if (isKaboot) → winner gets 16
else → Mathf.RoundToInt(points / 10f)

// Step 7: Apply multiplier
int multiplier = m_biddingSystem.HighestBid.Multiplier

// Step 8: Winner takes all
if (bidder won) → bidder gets (score × multiplier), opponent = 0
if (bidder lost) → opponent gets (score × multiplier), bidder = 0

// Step 9: Add to global score
Score.AddScore(team, finalScore)
```

---

## 🎮 Debug Output

After each round ends, check the Unity console for:

```
[GameStage] === END OF ROUND X ===
[GameStage] Bidding Team: TeamX
[GameStage] Total Points (before division) - Team1: XX, Team2: XX
[GameStage] Kaboot check - IsKaboot: True/False
[GameStage] After division - Team1: X, Team2: X
[GameStage] Multiplier from bidding: Xx
[GameStage] === FINAL ROUND SCORES ===
[GameStage] Team1 Final: X points
[GameStage] Team2 Final: X points
[GameStage] === CUMULATIVE SCORES ===
[GameStage] Team1 Total: XX points
[GameStage] Team2 Total: XX points
```

---

## 🖥️ UI Display

### In-Game HUD (Always Visible)
Located in: `Assets/Scripts/GameStage/GameStageRenderer.cs` → `UpdateGUI()`

Shows current cumulative scores at top-right:
```
Score: 26 / 15
```

### Round End Breakdown (5 seconds)
Automatically displays after each round with:
- Raw points for each team
- Division calculation
- Multiplier applied
- Final round score
- Cumulative totals
- Winner/loser indicators

---

## 🔧 Key Constants

```csharp
// Card Points
Non-Trump: A=11, 10=10, K=4, Q=3, J=2, 9/8/7=0
Trump: J=20, 9=14, A=11, 10=10, K=4, Q=3, 8/7=0

// Special Bonuses
"10 de der" (last trick) = +10
Kaboot base score = 16 (fixed)

// Multipliers
Normal = ×1
Double = ×2
Triple = ×3
Quadruple = ×4

// Projects
Sara = 20 points
Khamsin = 50 points
Mia = 100 points
Arba'miya = 400 points
Belote = 20 points (never cancelled)
```

---

## ⚠️ Important Rules

1. **Only the winning team scores** - loser always gets 0
2. **Tie goes to non-bidder** - bidder must have MORE points to win
3. **Multiplier applies AFTER division** - not before
4. **Kaboot = 16 points fixed** - not divided from raw points
5. **Projects are compared** - only highest project scores (except Belote)
6. **Division by 10 uses Mathf.RoundToInt()** - rounds to nearest integer

---

## 🐛 Testing Scenarios

**Test 1: Normal Round**
- Raw: Team1=92, Team2=70
- Expected: Team1=9 pts, Team2=0 pts (bidder wins)

**Test 2: Kaboot**
- Raw: Team1=162, Team2=0
- Expected: Team1=16 pts × multiplier

**Test 3: Bidder Loses**
- Raw: Team1(bidder)=65, Team2=97
- Expected: Team1=0 pts, Team2=10 pts × multiplier

**Test 4: With Double (×2)**
- Raw: Team1=82, Team2=70, Multiplier=2
- Expected: Team1=16 pts (8×2), Team2=0 pts

---

## 📊 Events

**RoundEndScoreEvent** sent after each round with:
```csharp
- Team1RawPoints / Team2RawPoints (before division)
- Team1RoundScore / Team2RoundScore (final, after all calculations)
- BiddingTeam / WinningTeam
- Multiplier (1-4)
- IsKaboot (bool)
- Team1CumulativeScore / Team2CumulativeScore
```

Subscribe to this event for custom UI or analytics:
```csharp
GameEventDispatcher.Subscribe<GameStage.RoundEndScoreEvent>(OnRoundEndScore);
```

---

## 📁 Files Modified

✅ `Assets/Scripts/GameStage/GameStage.cs` - Core scoring logic
✅ `Assets/Scripts/GameStage/GameStageRenderer.cs` - Debug UI display
✅ `Assets/Scripts/GameStage/UI/RoundEndScoreUI.cs` - Optional detailed UI

---

## 🎓 Quick Example

```
Round Setup:
- Bidding Team: Team 1
- Multiplier: ×2 (Double declared)

Raw Points After All Tricks:
- Team 1: 82 (cards) + 10 (10 de der) + 20 (Sara) = 112
- Team 2: 50 (cards) + 0 + 0 = 50

Calculation:
1. Team 1 wins (112 > 50)
2. Not Kaboot (both have points)
3. Divide: Team 1 = 112÷10 = 11.2 → 11
4. Divide: Team 2 = 50÷10 = 5.0 → 5
5. Apply multiplier: 11 × 2 = 22
6. Bidder won, so Team 1 gets all:
   
RESULT:
✅ Team 1: +22 points
❌ Team 2: +0 points
```

---

**For full details, see:** `SCORING_SYSTEM_IMPLEMENTATION.md`

