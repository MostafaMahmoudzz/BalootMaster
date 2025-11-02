# RoundInfoTracker - Usage Guide

## Overview
`RoundInfoTracker` is a Unity Inspector tool that displays real-time information about the current game round, dealer, and dealing order in your Baloot game.

## What It Shows

### 1. **Round Information**
- **Current Round Number**: Starts at 1, increases when:
  - A game round ends (all cards are played)
  - All players pass in both bidding rounds 1 and 2

### 2. **Dealer Information**
- **Current Dealer**: The player currently dealing cards
- **Next Dealer**: The player who will deal next round (always the player to the right, anti-clockwise)

### 3. **Dealing Order**
- Shows all players being dealt to in the correct order (anti-clockwise from the dealer)
- Indicates the first player to receive cards with a ▶ marker

### 4. **Additional Game State**
- **First Player This Round**: The player who plays first after dealing
- **Current Bidder**: The player who won the bidding (contract maker)
- **Trump Suit**: The trump suit for the current round
- **All Players**: Complete list of players with their positions, teams, and current status

## How to Use

### Step 1: Add the Component to Your Scene

1. Open your Main scene in Unity
2. Select any GameObject in the scene (or create a new empty GameObject)
   - **Recommended**: Create a new GameObject named "Round Info Tracker" for organization
3. In the Inspector, click "Add Component"
4. Type "RoundInfoTracker" and select it from the list
5. The component will automatically find the BeloteGame in your scene

### Step 2: View the Information

- The Inspector will automatically display all the information
- All fields are **read-only** and update automatically as the game plays
- Information is grouped into clear sections with helpful labels

### Step 3: Refresh if Needed

- If the information seems stale, click the **"Refresh Information"** button at the bottom
- Click **"How to Use"** for a quick help message in the Console

## Technical Details

### How It Works
- The component automatically finds the `BeloteGame` MonoBehaviour in your scene
- It reads the `GameStage` data through the public `Stage` property
- All information is computed on-demand when the Inspector refreshes
- Uses Odin Inspector attributes for a beautiful, organized display

### Requirements
- **Unity**: The project must be open in Unity
- **BeloteGame**: A BeloteGame component must exist in the scene
- **Odin Inspector**: The project uses Odin Inspector for enhanced Inspector display (already included)

### Performance
- Very lightweight - only computes values when Inspector is visible
- Caches the BeloteGame reference for efficiency
- No impact on gameplay performance

## Troubleshooting

### "Not assigned yet" appears
- **Cause**: The game hasn't started yet, or the specific value hasn't been set
- **Solution**: Start playing the game, and values will appear automatically

### All values show 0 or empty
- **Cause**: The component can't find the BeloteGame in the scene
- **Solution**: 
  1. Make sure there's a GameObject with the BeloteGame component in your scene
  2. Click the "Refresh Information" button
  3. Check the Console for any error messages

### Information doesn't update
- **Cause**: Inspector needs to refresh
- **Solution**: 
  1. Click anywhere in the Inspector to force a refresh
  2. Click the "Refresh Information" button
  3. Enter/exit Play mode

## Code Location
- **Script**: `Assets/Scripts/GameStage/RoundInfoTracker.cs`
- **Added Property**: `GameStage.CurrentRound` (public getter for round tracking)

## Example Display

```
┌─ Round Information ─────────────────────────────────────┐
│ Current Round: 3                                        │
│ ℹ The current round number (starts at 1, increases     │
│   when round ends or all players pass)                 │
└─────────────────────────────────────────────────────────┘

┌─ Dealer Information ────────────────────────────────────┐
│ Current Dealer: West (West) - Team Team2               │
│ Next Dealer: North (North) - Team Team1                │
└─────────────────────────────────────────────────────────┘

┌─ Dealing Order (Anti-Clockwise) ────────────────────────┐
│ Players Being Dealt To:                                 │
│ Dealing starts from the player to the right of dealer: │
│ ▶ 1. North (North) - Team Team1                        │
│   2. West (West) - Team Team2                          │
│   3. South (South) - Team Team1                        │
│   4. East (East) - Team Team2                          │
└─────────────────────────────────────────────────────────┘

┌─ Additional Info ───────────────────────────────────────┐
│ First Player This Round: North (North) - Team Team1    │
│ Current Bidder: South (South) - Team Team1             │
│ Trump Suit: Hearts                                      │
└─────────────────────────────────────────────────────────┘

┌─ All Players ──────────────────────────────────────────┐
│ South (South) - Team1 ▶ CURRENT                       │
│ West (West) - Team2 🃏 DEALER                         │
│ North (North) - Team1                                  │
│ East (East) - Team2                                    │
└─────────────────────────────────────────────────────────┘
```

## Additional Notes

- **Round Logic**: The round number is stored in `GameStage.m_currentRound` and incremented at the start of each new round
- **Dealer Rotation**: The dealer rotates anti-clockwise (to the right) each round in the `DealCards()` method
- **Dealing Order**: Cards are dealt starting from the player to the right of the dealer, continuing anti-clockwise
- **No Contract**: When all players pass, the cards are collected, and a new round starts with the next dealer

## Questions or Issues?

If you encounter any issues or have questions:
1. Check the Console for error messages
2. Verify the BeloteGame component exists in your scene
3. Make sure the game is running (Enter Play mode)
4. Click the "How to Use" button in the Inspector for quick help

---

*Created for BalootMaster project - November 2025*

