# Projects (Masharie3) - Quick Start Guide

## What Was Added?

A complete **Projects (Masharie3)** system for Baloot, including:
- Automatic detection of all project types (Sara, Khamsin, Mia, Arba'miya, Belote)
- Player declaration UI with buttons
- Project comparison and validation
- Automatic scoring at round end
- Belote detection during gameplay

---

## How to Use

### 1. The System is Already Integrated! ✅

Everything is automatically set up. When you run the game:

1. **After bidding completes**, the project declaration phase begins
2. **Each player** sees a UI panel with their available projects
3. **Click projects** to declare them (multiple selections allowed)
4. **Click "Finish / Pass"** when done
5. **Projects are compared** automatically
6. **During play**, Belote is detected when you play K or Q of trump
7. **At round end**, project points are added to scores

### 2. No Setup Required

The system is already integrated into `GameStage`. Just start playing!

---

## What to Expect

### Declaration UI (for Human Players)

When it's your turn to declare:
```
╔═══════════════════════════════╗
║ [Player Name] - Declare Projects ║
╠═══════════════════════════════╣
║  [ Sara (20) ]                ║
║  7-8-9 of Hearts              ║
║                               ║
║  [ 100 ]                      ║
║  10-J-Q-K-A of Spades         ║
║                               ║
║  [ Finish / Pass ]            ║
╚═══════════════════════════════╝
```

- **Green buttons** = Selected
- **Blue buttons** = Available
- **Orange button** = Finish

### AI Behavior
AI players automatically declare all their projects without showing UI.

### Belote During Play
When you play K or Q of trump and hold the other:
```
Console: "*** BELOTE! [Player Name] declared Belote! ***"
```
+20 points added at round end.

---

## File Structure

```
Assets/Scripts/GameStage/Projects/
├── ProjectType.cs               # Project type enum
├── Project.cs                   # Project data class
├── ProjectDetector.cs           # Detection logic
├── ProjectManager.cs            # Manager (lifecycle)
├── ProjectEvents.cs             # Event definitions
├── ProjectUI.cs                 # UI component
├── README_PROJECTS_SYSTEM.md   # Full documentation
└── QUICK_START.md              # This file
```

---

## Customization

### Change Button Colors
Edit `ProjectUI.cs`:
```csharp
private Color m_buttonColor = new Color(0.2f, 0.4f, 0.8f, 0.9f);  // Blue
private Color m_selectedColor = new Color(0.2f, 0.8f, 0.4f, 0.9f); // Green
private Color m_finishColor = new Color(0.8f, 0.4f, 0.2f, 0.9f);  // Orange
```

### Change Button Size
Edit `ProjectUI.cs`:
```csharp
private float m_buttonWidth = 150f;
private float m_buttonHeight = 50f;
private float m_buttonSpacing = 10f;
```

### Scoring Rules
Edit `ProjectManager.ScoreProjects()` in `ProjectManager.cs`:
```csharp
// Current: Always score projects
score.AddScore(team, totalPoints);

// Option: Only score if team won
if (winningTeam == team) {
    score.AddScore(team, totalPoints);
}
```

---

## Testing Checklist

- [ ] Sara (3 consecutive) is detected
- [ ] Khamsin (4 consecutive) is detected
- [ ] Mia (5 consecutive) is detected
- [ ] Mia (4 of a kind: 10/J/Q/K) is detected
- [ ] Mia (4 Aces in Hukm) is detected
- [ ] Arba'miya (4 Aces in Sun) is detected
- [ ] Declaration UI appears after bidding
- [ ] Projects can be selected/deselected
- [ ] "Finish" button works
- [ ] AI auto-declares
- [ ] Projects are compared correctly
- [ ] Belote is declared when K/Q of trump is played
- [ ] Points are added at round end

---

## Troubleshooting

**Issue:** UI doesn't appear
- **Solution:** Check that player is `HumanPlayer` (not AI)
- **Solution:** Wait for bidding to complete first

**Issue:** Projects not detected
- **Solution:** Check cards are actually consecutive (7-8-9-10-J-Q-K-A)
- **Solution:** For 4-of-a-kind, only 10/J/Q/K count

**Issue:** Belote not declared
- **Solution:** Only works in Trump rounds (not Sun)
- **Solution:** Must have both K and Q of trump suit

**Issue:** Points not added
- **Solution:** Check `EndRound()` is calling `ScoreProjects()`
- **Solution:** Look for `[ProjectManager]` logs in console

---

## For More Info

See **`README_PROJECTS_SYSTEM.md`** for:
- Complete technical documentation
- Architecture details
- Event system explanation
- Advanced customization

---

**Enjoy your enhanced Baloot game with Projects! 🎴**

