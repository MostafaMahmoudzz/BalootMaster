# Doubles, Triples, and Quadruples Feature

## Overview
This document describes the implementation of the doubles, triples, and quadruples feature in the Baloot game bidding system. This feature allows teams to escalate the stakes after the trump is confirmed in Round 2.

## Game Flow

### Phase 1: Round 1 (Existing)
- Players take turns choosing:
  - **Trump**: Take the face-up card's suit as trump
  - **Sun**: Play with no trump
  - **Pass**: Skip this turn
- If someone chooses Trump, that player becomes the "trump taker"

### Phase 2: Round 2 (Existing)
- All players bid again, starting with the same first bidder
- The trump taker can:
  - **Confirm Trump**: Keep the face-up suit as trump → **IMMEDIATELY starts Multiplier Bidding**
  - **Sun**: Change to no trump → Game starts immediately (no multiplier phase)
- Other players (if trump taker hasn't confirmed yet) can:
  - **Another Trump**: Choose a different suit (if no one took trump in Round 1) → Game starts immediately
  - **Sun**: Play with no trump → Game starts immediately
  - **Pass**: Skip

### Phase 3: Multiplier Bidding (NEW)
**Triggered when**: Trump is confirmed in Round 2 (starts IMMEDIATELY, doesn't wait for other players)

⚠️ **Important**: When the trump taker confirms trump, the system immediately transitions to Multiplier Bidding without asking other players to pass first.

The bidding enters a new escalation phase where teams can increase the multiplier:

1. **Start**: Opposing team gets first chance to double
   - **Pass**: Game starts with 1x multiplier
   - **Double**: Increase to 2x multiplier

2. **If Doubled**: Trump confirmer can escalate
   - **Pass**: Game starts with 2x multiplier
   - **Triple**: Increase to 3x multiplier

3. **If Tripled**: Opposing team can escalate again
   - **Pass**: Game starts with 3x multiplier
   - **Quadruple**: Increase to 4x multiplier (maximum)

4. **If Quadrupled**: Game automatically starts with 4x multiplier

## Implementation Details

### 1. Bid Class Extensions
**File**: `Assets/Scripts/GameStage/Bidding/Bid.cs`

#### New Contract Types
```csharp
public enum ContractType
{
    Pass,           // Player passes
    Trump,          // Player chooses trump suit
    Sun,            // No trump (Sun contract)
    Double,         // 2x multiplier
    Triple,         // 3x multiplier
    Quadruple       // 4x multiplier
}
```

#### New Properties
- `Multiplier`: Stores the score multiplier (1, 2, 3, or 4)
- `IsDouble`, `IsTriple`, `IsQuadruple`: Check for multiplier types
- `IsMultiplier`: Check if bid is any multiplier type

#### New Factory Methods
```csharp
Bid.CreateDouble()    // Creates a 2x multiplier bid
Bid.CreateTriple()    // Creates a 3x multiplier bid
Bid.CreateQuadruple() // Creates a 4x multiplier bid
```

### 2. BelootBiddingSystem Extensions
**File**: `Assets/Scripts/GameStage/Bidding/BelootBiddingSystem.cs`

#### New Bidding Round
```csharp
public enum BiddingRound
{
    BiddingRound1,     // First round of bidding
    BiddingRound2,     // Second round of bidding
    MultiplierBidding  // Multiplier escalation phase
}
```

#### New State Variables
- `m_inMultiplierBidding`: Flag indicating multiplier phase is active
- `m_currentMultiplier`: Current multiplier value (1, 2, 3, or 4)
- `m_trumpConfirmer`: Player who confirmed trump
- `m_lastMultiplierBidder`: Last player who escalated
- `m_isOpposingTeamTurn`: Tracks which team's turn it is

#### Key Methods

##### StartMultiplierBidding()
- Called after Round 2 when trump is confirmed
- Sets up the multiplier bidding phase
- Identifies the opposing team
- Sends `MultiplierBiddingStartEvent`

##### ProcessMultiplierBid()
- Handles bid submissions during multiplier phase
- Validates multiplier escalations
- Enforces team turn order
- Switches between teams after each escalation
- Sends `MultiplierBiddingTurnEvent`

##### GetOpposingTeamPlayer()
- Helper method to find any player from opposing team
- Used to determine the next bidder

### 3. New Events
**File**: `Assets/Scripts/GameStage/Bidding/BiddingEvents.cs`

#### MultiplierBiddingStartEvent
Sent when multiplier bidding phase begins
- `CurrentBidder`: First player to bid (opposing team)
- `TrumpConfirmer`: Player who confirmed trump
- `CurrentMultiplier`: Starting multiplier (always 1)
- `IsOpposingTeamTurn`: Flag for UI (always true at start)

#### MultiplierBiddingTurnEvent
Sent when turn changes during multiplier bidding
- `CurrentBidder`: Current player's turn
- `CurrentMultiplier`: Current multiplier value
- `IsOpposingTeamTurn`: Which team's turn it is

### 4. UI Updates
**File**: `Assets/Scripts/GameStage/Bidding/BiddingUI.cs`

#### New Event Handlers
- `OnMultiplierBiddingStart()`: Initialize multiplier phase UI
- `OnMultiplierBiddingTurn()`: Update UI for turn changes

#### New UI Method
`ShowMultiplierBiddingOptions()`: Displays multiplier bidding interface
- Shows current multiplier status
- Shows trump confirmer
- Shows team role (Opposing/Trump Confirmer Team)
- Shows available actions (Pass or Escalate)
- Only shows escalation if not at maximum (4x)

## Usage Example

### Scenario 1: No Escalation
1. Round 2: Player A (Team 1) confirms Trump → **Immediately starts Multiplier Phase**
2. Multiplier Phase: Player B (Team 2) passes
3. Result: Game starts with 1x multiplier

### Scenario 2: Double Only
1. Round 2: Player A (Team 1) confirms Trump → **Immediately starts Multiplier Phase**
2. Multiplier Phase: Player B (Team 2) doubles (2x)
3. Player A passes
4. Result: Game starts with 2x multiplier

### Scenario 3: Full Escalation
1. Round 2: Player A (Team 1) confirms Trump → **Immediately starts Multiplier Phase**
2. Multiplier Phase:
   - Player B (Team 2) doubles (2x)
   - Player A triples (3x)
   - Player B quadruples (4x)
3. Result: Game starts with 4x multiplier

### Scenario 4: Sun Declared
1. Round 2: Player A chooses Sun
2. Result: Game starts immediately (no multiplier phase)

### Scenario 5: Another Trump Chosen
1. Round 2: Player A chooses Another Trump
2. Result: Game starts immediately (no multiplier phase)

## Integration Points

### GameStage Integration
The `GameStage` class needs to:
1. Check `BiddingSystem.IsComplete` to know when bidding ends
2. Read `WinningBid.Multiplier` to apply score multiplier
3. Handle `MultiplierBiddingStartEvent` if needed for game flow

### Scoring Integration
When calculating scores:
```csharp
int baseScore = CalculateBaseScore();
int finalScore = baseScore * winningBid.Multiplier;
```

### AI Player Integration
AI players need to implement logic for multiplier bidding:
```csharp
// In AI bidding logic
if (biddingSystem.InMultiplierBidding)
{
    // Decide whether to pass or escalate
    if (ShouldEscalate())
    {
        int nextMultiplier = biddingSystem.CurrentMultiplier + 1;
        Bid escalateBid = nextMultiplier == 2 ? Bid.CreateDouble() :
                          nextMultiplier == 3 ? Bid.CreateTriple() :
                          Bid.CreateQuadruple();
        SubmitBid(escalateBid);
    }
    else
    {
        SubmitBid(Bid.CreatePass());
    }
}
```

## Testing Checklist

- [ ] Trump confirmed in Round 2 triggers multiplier bidding
- [ ] Sun in Round 2 skips multiplier bidding
- [ ] Another Trump in Round 2 skips multiplier bidding
- [ ] Opposing team bids first in multiplier phase
- [ ] Teams alternate correctly during escalation
- [ ] Pass ends multiplier bidding at current level
- [ ] Quadruple (4x) automatically ends bidding
- [ ] Multiplier value is correctly stored in winning bid
- [ ] UI shows correct options for each team
- [ ] AI players can participate in multiplier bidding

## Notes

### Design Decisions

1. **Team-Based**: Only one player per team needs to bid (automatic delegation)
2. **Alternating**: Teams alternate after each escalation
3. **Maximum**: 4x is the maximum multiplier (quadruple)
4. **Exclusive**: Only applies when trump is confirmed (not Sun or Another Trump)

### Future Enhancements

1. **Per-Player Options**: Allow each player on a team to vote
2. **Time Limits**: Add countdown timer for multiplier decisions
3. **Animation**: Add visual effects for multiplier escalation
4. **Sound Effects**: Add audio feedback for escalations
5. **Statistics**: Track multiplier bidding patterns for AI learning

## Troubleshooting

### Issue: Multiplier bidding not starting
**Solution**: Check that trump was confirmed (face-up suit chosen) in Round 2, not Sun or Another Trump

### Issue: Wrong team bidding
**Solution**: Verify team assignments are correct (Team1 vs Team2)

### Issue: Can't escalate past double
**Solution**: Check that the opposing team is escalating when it's their turn

### Issue: Game ends before multiplier bidding
**Solution**: Ensure Sun or Another Trump wasn't chosen, which skip multiplier phase

