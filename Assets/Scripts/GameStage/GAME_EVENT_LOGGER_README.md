# Game Event Logger System

## Overview

The **GameEventLogger** is a comprehensive event monitoring system that subscribes to all major game events and logs detailed information about game flow, player actions, scoring, and other key game events. It serves as a debugging and monitoring tool for understanding the complete game state throughout a match.

## Purpose

The GameEventLogger provides real-time logging of:
- Player decisions (Rassa choices, ASSA usage, project declarations)
- Bidding process (bids submitted, winners, contract types)
- Card dealing and distribution
- Card play and fold winners
- Scoring (round scores, cumulative scores)
- Special features (Sawa eligibility and claims)

## Integration

The logger is automatically initialized by `GameStage` in the same way as `GameStageRenderer`:

```csharp
// In GameStage.OnInit()
m_eventLogger.Stage = this;
m_eventLogger.Init();  // Subscribes to all events

// In GameStage.OnShutdown()
m_eventLogger.Shutdown();  // Unsubscribes from all events
```

## Events Captured

### 1. Rassa Events

#### RassaPromptEvent
Logged when a player is asked about using Rassa:
```
╔════════════════════════════════════════════════════════════════╗
║                    RASSA PROMPT EVENT                          ║
╠════════════════════════════════════════════════════════════════╣
║ Player Being Asked: South
║ Position: South
║ Team: Team1
║ Round Number: 1
╚════════════════════════════════════════════════════════════════╝
```

#### RassaResponseEvent
Logged when a player chooses to use Rassa or not:
```
╔════════════════════════════════════════════════════════════════╗
║                    RASSA CHOICE MADE                           ║
╠════════════════════════════════════════════════════════════════╣
║ Player: South
║ Position: South
║ Team: Team1
║ Choice: ✓ YES - Use Rassa
╚════════════════════════════════════════════════════════════════╝
```

### 2. ASSA Events

#### AssaaPromptEvent
Logged when a player is asked about using Assaa:
```
╔════════════════════════════════════════════════════════════════╗
║                    ASSAA PROMPT EVENT                          ║
╠════════════════════════════════════════════════════════════════╣
║ Prompt Type: Right Player (#1)
║ Player Being Asked: West
║ Position: West
║ Team: Team2
║ Rassa Chooser: South
╚════════════════════════════════════════════════════════════════╝
```

#### AssaaReorderCompleteEvent
Logged when card reordering is complete:
```
╔════════════════════════════════════════════════════════════════╗
║                ASSAA CARD REORDERING COMPLETE                  ║
╠════════════════════════════════════════════════════════════════╣
║ Player: West
║ Position: West
║ Team: Team2
║ Success: YES - Cards reordered
║ Result: Deck has been reordered
║ Note: New card order affects dealing
╚════════════════════════════════════════════════════════════════╝
```

### 3. Project (Masharie3) Events

#### ProjectDeclaredEvent
Logged when a player declares a project:
```
╔════════════════════════════════════════════════════════════════╗
║                  PROJECT DECLARED                              ║
╠════════════════════════════════════════════════════════════════╣
║ Player: North
║ Position: North
║ Team: Team1
║ Project Type: Khamsin
║ Project Exists: YES
║ Project Points: 50
║ Cards in Project: 5
╚════════════════════════════════════════════════════════════════╝
```

### 4. Card Dealing Events

#### Initial Card Dealing
Cards are dealt in two phases during bidding:
- First 3 cards to each player
- Then 2 more cards (total 5 cards for bidding)

This is logged in `DealCards()` method with existing Debug.Log statements.

#### Final Card Dealing (NewRoundEvent)
After bidding completes, all players receive their final cards (total 8):
```
╔════════════════════════════════════════════════════════════════╗
║              ★★★ NEW ROUND STARTED ★★★                        ║
╠════════════════════════════════════════════════════════════════╣
║ Dealer: East
║ Bidder: South
║ Trump: Hearts
║ Round First Player: North
║
║ Cards Dealt to Each Player (Total: 8):
║   ✓ South (South): 8 cards
║   ✓ West (West): 8 cards
║   ✓ North (North): 8 cards
║   ✓ East (East): 8 cards
╚════════════════════════════════════════════════════════════════╝
```

### 5. Fold Winner Events

#### FoldWinnerEvent (NEW!)
Logged when a fold is completed and a winner is determined:
```
╔════════════════════════════════════════════════════════════════╗
║              ★★★ FOLD WINNER ★★★                              ║
╠════════════════════════════════════════════════════════════════╣
║ Winner: South
║ Position: South
║ Team: Team1
║ Fold Points: 18
║ Cards in Fold: 4
║ Next Action: Winner leads the next fold
╚════════════════════════════════════════════════════════════════╝
```

### 6. Round Score Events

#### RoundEndScoreEvent
Logged at the end of each round with complete scoring breakdown:
```
╔════════════════════════════════════════════════════════════════╗
║              ★★★ ROUND SCORE ★★★                              ║
╠════════════════════════════════════════════════════════════════╣
║ === RAW POINTS ===
║ Team 1: 102 points
║ Team 2: 60 points
║
║ === ROUND SCORE (÷10 and multiplier applied) ===
║ Team 1: +10 points
║ Team 2: +0 points
║
║ Bidding Team: Team1
║ Winning Team: Team1
║ Multiplier: 1x
║ Kaboot (All Tricks): NO
║
╠════════════════════════════════════════════════════════════════╣
║              ★★★ CUMULATIVE GAME SCORE ★★★                    ║
╠════════════════════════════════════════════════════════════════╣
║ Team 1 Total: 10 points
║ Team 2 Total: 0 points
║ Leading: Team 1 by 10 points
╚════════════════════════════════════════════════════════════════╝
```

### 7. Bidding Complete Events

#### BiddingCompleteEvent
Logged when bidding is finalized, announcing the winner, game type, and referee type:
```
╔════════════════════════════════════════════════════════════════╗
║              ★★★ BIDDING COMPLETE ★★★                         ║
╠════════════════════════════════════════════════════════════════╣
║ Winner: South
║ Position: South
║ Team: Team1
║ Winning Bid: Trump Hearts
║ Game Type: TRUMP
║ Trump Suit: Hearts
║ Referee Type: Trump (Hearts)
╚════════════════════════════════════════════════════════════════╝
```

For Sun contracts:
```
║ Game Type: SUN (No Trump)
║ Referee Type: Sun
```

### 8. Sawa Events

#### SawaAvailableEvent
Logged when Sawa becomes available or unavailable for a player:
```
╔════════════════════════════════════════════════════════════════╗
║                  SAWA ELIGIBILITY                              ║
╠════════════════════════════════════════════════════════════════╣
║ Player: South
║ Position: South
║ Team: Team1
║ Eligible for Sawa: YES ✓
║ Status: Player can claim Sawa
╚════════════════════════════════════════════════════════════════╝
```

#### SawaClaimedEvent
Logged when a player claims Sawa:
```
╔════════════════════════════════════════════════════════════════╗
║              ★★★ SAWA CLAIMED! ★★★                            ║
╠════════════════════════════════════════════════════════════════╣
║ Player: South
║ Position: South
║ Team: Team1
║ Result: Trump changes to player's hand
╚════════════════════════════════════════════════════════════════╝
```

## Additional Events Logged

The logger also captures:
- **BiddingStartEvent** - When bidding begins
- **BidSubmittedEvent** - Each bid submitted by players
- **BiddingRound2StartEvent** - When second bidding round starts
- **MultiplierBiddingStartEvent** - When multiplier bidding begins
- **BeloteCard.Played** - Each card played
- **NewTurnEvent** - Each player's turn
- **ProjectDeclarationStartEvent** - When project declaration phase begins
- **ProjectDeclarationCompleteEvent** - When all projects are compared
- **BeloteDeclaredEvent** - When Belote (King + Queen of trump) is declared
- **ProjectScoredEvent** - When projects are scored
- **CardsCollectedEvent** - When cards are collected back to deck

## File Structure

```
Assets/Scripts/GameStage/
├── GameEventLogger.cs           # Main logger class
├── GameStage.cs                 # Integrates logger, dispatches FoldWinnerEvent
└── GAME_EVENT_LOGGER_README.md  # This documentation
```

## New Events Created

### FoldWinnerEvent
A new event was created in `GameStage.cs` to capture fold winners explicitly:

```csharp
public class FoldWinnerEvent : PooledEvent
{
    public Player Winner { get; set; }
    public PlayerTeam WinningTeam { get; set; }
    public int FoldPoints { get; set; }
    public int CardsInFold { get; set; }
}
```

This event is dispatched in `GameStage.OnAfterPlayTimerDone()` right after `CurrentFold.Finalize()` is called.

## Usage

The logger runs automatically once the game starts. All events are logged to Unity's Console with clear, formatted output using box-drawing characters for visual clarity.

### Viewing Logs

1. Open Unity Console (Window → General → Console)
2. Start a game
3. Watch as events are logged in real-time with clear formatting
4. Use Console search/filter to find specific events (e.g., "FOLD WINNER", "RASSA", "BIDDING")

### Disabling/Enabling

To temporarily disable event logging:
- Comment out `m_eventLogger.Init()` in `GameStage.OnInit()`
- Or comment out specific event subscriptions in `GameEventLogger.Init()`

## Benefits

1. **Complete Game State Visibility** - See exactly what's happening at every stage
2. **Debugging Aid** - Quickly identify issues in game flow
3. **Player Action Tracking** - Monitor all player decisions
4. **Scoring Verification** - Verify scoring calculations are correct
5. **Event Flow Understanding** - Understand the order and timing of events

## Future Enhancements

Possible future additions:
- Export logs to file for analysis
- Filter logs by event type or player
- Display logs in-game UI for players
- Statistical analysis of logged events
- Replay functionality based on logged events

---

**Created:** November 16, 2025
**Version:** 1.0
**Author:** Baloot Master Development Team

