# Current Bidder Visual Marker - Setup Guide

## Overview
The **Current Bidder Marker** is a visual indicator that shows which player (North, South, East, or West) is currently bidding during the bidding phase. The marker automatically moves to the appropriate position as turns change.

## Features
- ✅ Automatically positions itself based on player position (N/S/E/W)
- ✅ Updates in real-time as bidders change
- ✅ Hides automatically when bidding is complete
- ✅ Works with both Round 1 and Round 2 bidding
- ✅ Integrates with all bidding events

## Setup Instructions

### Step 1: Create the Marker GameObject

You have several options for creating the marker:

#### Option A: Simple 2D Sprite Marker (Recommended)
1. In Unity, create a new **2D Sprite** GameObject:
   - Right-click in Hierarchy → `2D Object` → `Sprite`
   - Name it: `CurrentBidderMarker`

2. Assign a visual asset (choose one):
   - An **arrow** sprite pointing to the player
   - A **star** or **highlight** icon
   - A **colored circle** or ring
   - Any custom sprite that stands out visually

3. Configure the sprite:
   - Set the **Sorting Layer** to a high priority (so it appears in front)
   - Adjust the **scale** to an appropriate size (e.g., 0.5 to 1.0)
   - Set a bright color (yellow, green, or white work well)

#### Option B: 3D Model Marker
1. Create a new GameObject with a 3D model:
   - Import or use a simple 3D arrow, pointer, or icon
   - Name it: `CurrentBidderMarker`
   - Position it at Z = -1 so it appears in front of cards

#### Option C: UI Image Marker
1. Create a UI Canvas if you don't have one
2. Add a UI Image as a child:
   - Name it: `CurrentBidderMarker`
   - Set the Image to an arrow or icon sprite
   - Adjust the RectTransform size

### Step 2: Assign the Marker to BiddingUI

1. In your scene, find the GameObject that has the **BiddingUI** component
   - This is typically on your main UI or game controller

2. Select that GameObject in the Hierarchy

3. In the Inspector, find the **BiddingUI** component

4. Look for the **"Current Bidder Marker"** section (it has a header)

5. Drag and drop your `CurrentBidderMarker` GameObject into the field

### Step 3: Test It Out

1. Run your game in Unity
2. Start a bidding round
3. Watch as the marker automatically moves to show:
   - **South** (bottom center) - Human player
   - **West** (left side) - AI player
   - **North** (top center) - AI player
   - **East** (right side) - AI player

4. The marker should:
   - ✅ Appear when bidding starts
   - ✅ Move as turns change
   - ✅ Disappear when bidding is complete

## Marker Positioning

The marker positions are automatically calculated based on screen size:

| Player Position | Location | Coordinates (relative) |
|----------------|----------|------------------------|
| **South** | Bottom Center | (0, -65% height, -1) |
| **West** | Left Side | (-75% width, 50% height, -1) |
| **North** | Top Center | (0, 65% height, -1) |
| **East** | Right Side | (75% width, 50% height, -1) |

These positions are designed to be near the player's card area without overlapping.

## Customization Tips

### Adjusting Position Offsets
If you need to fine-tune the marker positions, edit the `GetMarkerPositionForPlayer()` method in `BiddingUI.cs`:

```csharp
case PlayerPosition.South:
    markerPos.x = 0f;           // Left/Right offset
    markerPos.y = -0.65f * halfHeight; // Up/Down offset (change this value)
    markerPos.z = -1f;          // Depth (negative = in front)
    break;
```

### Animating the Marker
To add animation:
1. Add an **Animator** component to your marker GameObject
2. Create animations for:
   - Pulsing/scaling effect
   - Rotating arrow
   - Fade in/out transitions
3. Trigger animations in the `UpdateBidderMarker()` method

### Adding Sound Effects
To add audio feedback when the marker moves:
1. Add an **AudioSource** component to the marker
2. In `UpdateBidderMarker()`, add:
   ```csharp
   AudioSource audio = currentBidderMarker.GetComponent<AudioSource>();
   if (audio != null)
   {
       audio.PlayOneShot(yourSoundClip);
   }
   ```

## Troubleshooting

### Marker Doesn't Appear
- ✅ Check that the marker GameObject is assigned in the Inspector
- ✅ Verify the marker has a visible Renderer or Image component
- ✅ Check the Sorting Layer / Render Order
- ✅ Ensure Camera can see the marker's position

### Marker Position is Wrong
- ✅ Check your camera is orthographic (not perspective)
- ✅ Adjust the position multipliers in `GetMarkerPositionForPlayer()`
- ✅ Verify the marker's Z position is negative (in front)

### Marker Doesn't Move
- ✅ Check the Debug Console for `[BiddingUI] Marker moved to...` messages
- ✅ Ensure the BiddingUI component is active
- ✅ Verify bidding events are firing correctly

## Technical Details

### When Marker Updates
The marker position is updated:
- When bidding starts (Round 1)
- When a player's turn begins
- When Round 2 starts
- Every frame during active bidding (for responsiveness)

### How It Works
1. The `BelootBiddingSystem` tracks the current bidder
2. When bidding events fire, `BiddingUI` receives them
3. `UpdateBidderMarker()` is called automatically
4. The marker's position is calculated based on `PlayerPosition` enum
5. The GameObject is moved to the calculated world position

### Events That Trigger Updates
- `BiddingStartEvent` - Initial bidding start
- `BiddingTurnEvent` - Turn changes
- `BiddingRound2StartEvent` - Round 2 begins
- `BiddingCompleteEvent` - Hides the marker

## Integration with Debug System

The marker integrates with the existing debug system:
- Debug messages show when the marker moves: `[BiddingUI] Marker moved to North position for North`
- The same debug boxes that show "CurrentBidder" will match the marker position
- Use this to verify the marker is correctly tracking the bidding system

## Example Marker Assets

Here are some suggestions for marker visuals:

### Simple Arrow
Create a triangle sprite pointing toward the player's card area

### Highlight Ring
Use a circular sprite that glows or pulses

### Player Badge
Display a small icon with the player's position letter (N/S/E/W)

### Animated Indicator
Use a sprite sheet with multiple frames for animation

## Next Steps

After setting up the basic marker, consider:
1. **Adding particle effects** - Sparkles or glows around the marker
2. **Implementing smooth transitions** - Use `Vector3.Lerp()` for smooth movement
3. **Player-specific markers** - Different colors for different positions
4. **Accessibility options** - Allow players to customize marker appearance

---

**File Modified**: `Assets/Scripts/GameStage/Bidding/BiddingUI.cs`  
**Date Added**: November 5, 2025  
**Feature**: Current Bidder Visual Marker System

