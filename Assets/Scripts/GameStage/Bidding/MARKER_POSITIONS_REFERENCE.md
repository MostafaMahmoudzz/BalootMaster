# 📍 Current Bidder Marker - Position Reference

## Visual Layout

```
┌─────────────────────────────────────────────────────┐
│                                                     │
│                      NORTH ⭐                       │
│                   (Top Center)                      │
│                                                     │
│                                                     │
│  WEST ⭐                              ⭐ EAST       │
│  (Left)                              (Right)       │
│                                                     │
│                                                     │
│                                                     │
│                      SOUTH ⭐                       │
│                  (Bottom Center)                    │
│                    [HUMAN PLAYER]                   │
└─────────────────────────────────────────────────────┘
```

## Exact Position Coordinates

The marker positions are calculated dynamically based on your camera's view:

### Formula
```
halfHeight = Camera.main.orthographicSize
halfWidth = halfHeight * Camera.aspect
```

### Position Table

| Position | X Coordinate | Y Coordinate | Z Coordinate | Visual Location |
|----------|-------------|--------------|--------------|-----------------|
| **South** | `0` | `-0.65 × height` | `-1` | Bottom center (near human player's cards) |
| **West**  | `-0.75 × width` | `0.5 × height` | `-1` | Left side (mid-left) |
| **North** | `0` | `0.65 × height` | `-1` | Top center (opposite human) |
| **East**  | `0.75 × width` | `0.5 × height` | `-1` | Right side (mid-right) |

### Example with Common Screen Sizes

#### 1920x1080 (16:9 aspect ratio, ortho size = 5)
```
halfHeight = 5
halfWidth = 8.889

SOUTH: (0, -3.25, -1)
WEST:  (-6.667, 2.5, -1)
NORTH: (0, 3.25, -1)
EAST:  (6.667, 2.5, -1)
```

#### 1280x720 (Same proportions)
```
Same relative positions - scales automatically!
```

## Card Position Alignment

The marker positions are designed to align with where cards are dealt:

```
                    🂠 🂠 🂠 North Cards 🂠 🂠 🂠
                              ⭐ Marker here
                              
                              
   🂠              Playing Area              🂠
   🂠                                        🂠
   🂠 West                              East 🂠
   🂠 Cards                            Cards 🂠
   🂠                                        🂠
   ⭐                                        ⭐
Marker                                   Marker


                              ⭐ Marker here
                    🂠 🂠 🂠 South Cards 🂠 🂠 🂠
                        (Your Hand - Human)
```

## Z-Position (Depth)

All markers use `Z = -1`:
- **Negative Z** = Closer to camera
- Ensures marker appears **in front** of cards
- Cards are at Z = 0 or positive

### Layering Order
```
Camera (Z = -10)
   ↓
Marker (Z = -1)    ← YOU ARE HERE (most visible)
   ↓
Cards (Z = 0)
   ↓
Background (Z = 1+)
```

## Customizing Positions

Want to adjust where the marker appears? Edit `BiddingUI.cs` → `GetMarkerPositionForPlayer()`:

### Move South Marker Higher
```csharp
case PlayerPosition.South:
    markerPos.x = 0f;
    markerPos.y = -0.55f * halfHeight;  // Changed from -0.65 to -0.55
    markerPos.z = -1f;
    break;
```

### Move West Marker Closer to Center
```csharp
case PlayerPosition.West:
    markerPos.x = -0.65f * halfWidth;  // Changed from -0.75 to -0.65
    markerPos.y = 0.5f * halfHeight;
    markerPos.z = -1f;
    break;
```

### Move Marker Further Forward (More Visible)
```csharp
markerPos.z = -2f;  // Changed from -1 to -2 (even closer to camera)
```

## Safe Adjustment Ranges

To keep markers visible and not overlapping with cards:

| Axis | Minimum | Maximum | Notes |
|------|---------|---------|-------|
| **X** | `-0.9 × width` | `0.9 × width` | Stay within screen bounds |
| **Y** | `-0.8 × height` | `0.8 × height` | Avoid screen edges |
| **Z** | `-5` | `0` | Negative = in front, but not past camera |

## Responsive Design

The positions automatically adjust for:
- ✅ Different screen resolutions
- ✅ Different aspect ratios (16:9, 4:3, 21:9, etc.)
- ✅ Different camera orthographic sizes
- ✅ Portrait vs landscape mode

**No manual adjustments needed!**

## Testing Different Positions

To test positions quickly:

1. Play the game
2. In Unity Editor, select your marker GameObject
3. In Inspector, manually change Transform position values
4. Note the values that look good
5. Update `GetMarkerPositionForPlayer()` with those values

## Common Positioning Issues

### Marker Too Close to Cards
**Problem**: Marker overlaps with cards  
**Solution**: Adjust Y offset (make it smaller/larger)

### Marker Off-Screen
**Problem**: Can't see the marker  
**Solution**: Reduce X or Y multipliers (e.g., 0.75 → 0.6)

### Marker Behind Cards
**Problem**: Cards cover the marker  
**Solution**: Make Z more negative (e.g., -1 → -2)

### Marker Too Small to See
**Problem**: Marker visible but hard to notice  
**Solution**: Increase marker GameObject scale

---

## Quick Reference - Copy & Paste

Default positions (copy this to your notes):

```csharp
// SOUTH (Bottom Center)
markerPos.x = 0f;
markerPos.y = -0.65f * halfHeight;
markerPos.z = -1f;

// WEST (Left Side)
markerPos.x = -0.75f * halfWidth;
markerPos.y = 0.5f * halfHeight;
markerPos.z = -1f;

// NORTH (Top Center)
markerPos.x = 0f;
markerPos.y = 0.65f * halfHeight;
markerPos.z = -1f;

// EAST (Right Side)
markerPos.x = 0.75f * halfWidth;
markerPos.y = 0.5f * halfHeight;
markerPos.z = -1f;
```

---

**File**: `BiddingUI.cs` → Method: `GetMarkerPositionForPlayer()`  
**Last Updated**: November 5, 2025

