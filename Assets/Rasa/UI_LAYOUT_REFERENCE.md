# Rassa System - UI Layout Reference

## 📐 Recommended Layout

```
┌─────────────────────────────────────────────────────────────────┐
│                        RASSA CARD ARRANGER                      │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌──────────────── CARD SELECTION AREA ─────────────────┐      │
│  │                                                       │      │
│  │  [7♣] [8♣] [9♣] [J♣] [Q♣] [K♣] [10♣] [A♣]          │      │
│  │  [7♥] [8♥] [9♥] [J♥] [Q♥] [K♥] [10♥] [A♥]          │      │
│  │  [7♦] [8♦] [9♦] [J♦] [Q♦] [K♦] [10♦] [A♦]          │      │
│  │  [7♠] [8♠] [9♠] [J♠] [Q♠] [K♠] [10♠] [A♠]          │      │
│  │                                                       │      │
│  │                     (32 Buttons)                      │      │
│  └───────────────────────────────────────────────────────┘      │
│                                                                 │
│  ┌──────────────── STATUS & CONTROLS ─────────────────┐        │
│  │                                                      │       │
│  │     Cards Selected: 0 / 32                          │       │
│  │                                                      │       │
│  │     [Undo]      [Reset]      [Done]                 │       │
│  │                                                      │       │
│  └──────────────────────────────────────────────────────┘       │
│                                                                 │
│  ┌──────────────── SELECTED CARDS DISPLAY ────────────┐        │
│  │                                                      │       │
│  │  [ ] [ ] [ ] [ ] [ ] [ ] [ ] [ ] [ ] [ ] [ ] [ ] [ ] [ ] [ ] [ ]  │
│  │  [ ] [ ] [ ] [ ] [ ] [ ] [ ] [ ] [ ] [ ] [ ] [ ] [ ] [ ] [ ] [ ]  │
│  │                                                      │       │
│  │                 (32 Display Slots)                   │       │
│  │              (Empty until cards selected)            │       │
│  └──────────────────────────────────────────────────────┘       │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## 🎨 Detailed Layout Specifications

### 1. Canvas Setup
```
Resolution: 1920x1080 (or your target resolution)
Canvas Scaler: Scale with Screen Size
Match: 0.5 (Width/Height)
```

### 2. Card Selection Area (Top)

**Container:** Panel or Empty GameObject with GridLayoutGroup

**Layout:**
- Position: Top of screen
- Size: 1600x600 (approximate)
- Layout: 4 rows × 8 columns = 32 cards

**Individual Card Button:**
- Size: 80×112 pixels (standard playing card ratio)
- Spacing: 10 pixels between cards
- Components needed:
  - Image (for card sprite)
  - Button (for clicking)
  - CardInfoComponent (for card data)

**Layout Component Settings (GridLayoutGroup):**
```
Cell Size: 80 × 112
Spacing: 10 × 10
Start Corner: Upper Left
Start Axis: Horizontal
Child Alignment: Upper Center
Constraint: Fixed Column Count = 8
```

---

### 3. Status & Controls (Middle)

**Status Text:**
- Font: TextMeshProUGUI
- Size: 32pt
- Color: White
- Alignment: Center
- Text: "Cards Selected: 0 / 32"
- Position: Center of screen, below card selection

**Control Buttons (Horizontal Layout):**
```
┌──────────┐  ┌──────────┐  ┌──────────┐
│   Undo   │  │  Reset   │  │   Done   │
└──────────┘  └──────────┘  └──────────┘
   120×50        120×50        120×50
```

**Spacing:** 20 pixels between buttons

---

### 4. Selected Cards Display (Bottom)

**Container:** Panel or Empty GameObject

**Layout:**
- Position: Bottom of screen
- Size: 1600x200 (approximate)
- Layout: 2 rows × 16 columns = 32 slots

**Individual Display Slot:**
- Size: 60×84 pixels (smaller than buttons)
- Spacing: 5 pixels between slots
- Components needed:
  - Image (initially disabled)
- Background: Light border to show slot

**Layout Component Settings (GridLayoutGroup):**
```
Cell Size: 60 × 84
Spacing: 5 × 5
Start Corner: Upper Left
Start Axis: Horizontal
Child Alignment: Upper Center
Constraint: Fixed Column Count = 16
```

---

## 🎨 Color Scheme Suggestions

### Option 1: Classic Green Table
```
Background: #0B5C29 (poker table green)
Card Buttons: White background
Selected Cards: Semi-transparent white
UI Panel: Rgba(0, 0, 0, 0.5) - semi-transparent black
Buttons: #2196F3 (blue)
Text: White
```

### Option 2: Modern Dark
```
Background: #1E1E1E (dark gray)
Card Buttons: White background
Selected Cards: White with glow
UI Panel: #2A2A2A
Buttons: #4CAF50 (green)
Text: White
```

### Option 3: Royal Blue
```
Background: #1A237E (deep blue)
Card Buttons: White background
Selected Cards: Gold border
UI Panel: Rgba(255, 255, 255, 0.1)
Buttons: #FFC107 (amber)
Text: White
```

---

## 📱 Responsive Layout (Alternative Sizes)

### Mobile Portrait (9:16)
```
Card Selection: 2 rows × 16 columns (scrollable)
Button Size: 60 × 84
Display: 4 rows × 8 columns
```

### Tablet (4:3)
```
Card Selection: 4 rows × 8 columns
Button Size: 90 × 126
Display: 2 rows × 16 columns
```

### Ultra-Wide (21:9)
```
Card Selection: 4 rows × 8 columns (centered)
Button Size: 100 × 140
Display: 1 row × 32 columns
```

---

## 🖼️ Visual States

### Card Button States

**Normal (Not Selected):**
```
- Full opacity (1.0)
- Enabled
- Interactable
- Normal color tint
```

**Selected (Clicked):**
```
- Image disabled (hidden)
- Button disabled
- Semi-transparent (0.5)
- Gray color tint
```

**Hover (Mouse Over):**
```
- Slight scale up (1.05)
- Highlight color
- Shadow effect
```

### Display Slot States

**Empty (Default):**
```
- Image disabled/hidden
- Light border showing slot
- Empty/transparent
```

**Filled (Card Selected):**
```
- Image enabled
- Shows card sprite
- Full opacity
- Optional glow effect
```

---

## 🎭 Animation Suggestions (Optional)

### Card Selection Animation
```
1. Button clicked
2. Scale down (0.95) over 0.1s
3. Fade out over 0.2s
4. Display slot appears with scale up (0 → 1) over 0.2s
```

### Undo Animation
```
1. Display slot fades out over 0.2s
2. Button fades in over 0.2s
3. Button scales up (0.95 → 1) over 0.1s
```

### Reset Animation
```
All cards fade in simultaneously with staggered timing
Delay: index × 0.02s (creates wave effect)
```

---

## 📏 Hierarchy Example

```
Canvas
├── BackgroundImage
├── TitleText ("RASSA CARD ARRANGER")
├── CardSelectionPanel
│   ├── CardButton_00 (7♣)
│   ├── CardButton_01 (8♣)
│   ├── ... (30 more buttons)
│   └── CardButton_31 (A♠)
├── StatusAndControlsPanel
│   ├── StatusText
│   └── ButtonsPanel
│       ├── UndoButton
│       ├── ResetButton
│       └── DoneButton
├── SelectedCardsPanel
│   ├── DisplaySlot_00
│   ├── DisplaySlot_01
│   ├── ... (30 more slots)
│   └── DisplaySlot_31
└── RassaController (Empty GameObject)
```

---

## 🔧 Unity Components Summary

### Per Card Button:
- RectTransform
- CanvasRenderer
- Image (card sprite)
- Button
- CardInfoComponent ← Custom Script

### Per Display Slot:
- RectTransform
- CanvasRenderer
- Image (initially disabled)

### Control Buttons:
- RectTransform
- CanvasRenderer
- Image (button background)
- Button
- TextMeshProUGUI (label)

---

## 💡 Pro Tips

1. **Use Prefabs**
   - Create a card button prefab
   - Create a display slot prefab
   - Duplicate 32 times

2. **Use Anchors**
   - Anchor panels to corners/edges
   - Makes responsive design easier
   - Works across different resolutions

3. **Use Layout Groups**
   - GridLayoutGroup for cards
   - HorizontalLayoutGroup for buttons
   - Automatic positioning

4. **Use Content Size Fitter**
   - Auto-resize panels
   - Fits content perfectly
   - Less manual adjustment

5. **Test Multiple Resolutions**
   - Use Unity's Game window resolution dropdown
   - Test 16:9, 4:3, 21:9
   - Test different aspect ratios

---

## 🎯 Quick Setup Checklist

- [ ] Create Canvas
- [ ] Add Background Image
- [ ] Create Card Selection Panel
- [ ] Add GridLayoutGroup to Card Selection
- [ ] Create 32 Card Buttons (or use RassaUIBuilder)
- [ ] Create Status Text
- [ ] Create Control Buttons Panel
- [ ] Create Undo, Reset, Done buttons
- [ ] Create Selected Cards Panel
- [ ] Add GridLayoutGroup to Selected Cards
- [ ] Create 32 Display Slots (or use RassaUIBuilder)
- [ ] Add RassaController GameObject
- [ ] Assign all references in Inspector
- [ ] Set colors and styling
- [ ] Test in Play mode

---

## 📐 Pixel Perfect Calculations

### For 1920×1080 Screen:

**Card Selection Area:**
- Total width: 8 buttons × 80px + 7 spaces × 10px = 710px
- Total height: 4 buttons × 112px + 3 spaces × 10px = 478px
- Position: Center-top

**Selected Cards Display:**
- Total width: 16 cards × 60px + 15 spaces × 5px = 1035px
- Total height: 2 rows × 84px + 1 space × 5px = 173px
- Position: Center-bottom

---

**This layout is optimized for desktop play at 1920×1080 resolution. Adjust sizes proportionally for other resolutions.**

For automatic UI generation, use the `RassaUIBuilder` script!

