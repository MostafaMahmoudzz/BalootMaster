# Rassa System - Quick Start Guide

## 🚀 Fast Setup (5 Minutes)

### Option 1: Automatic UI Builder (Recommended)

1. **Open the Rassa Scene**
   - Open `Scenes/Rassa.unity`

2. **Create Parent Containers**
   - Create a Canvas (if not exists)
   - Create 3 empty GameObjects as children:
     - `CardButtonsPanel` - for the 32 card buttons
     - `SelectedCardsPanel` - for showing selected cards
     - `ControlsPanel` - for undo/reset/done buttons

3. **Add RassaUIBuilder**
   - Create empty GameObject named "UIBuilder"
   - Add Component → `RassaUIBuilder`
   - Assign the three panels to:
     - Card Buttons Parent
     - Selected Cards Parent
     - Controls Parent
   - In Inspector, expand "Sprites" and assign your 32 card sprites

4. **Generate UI Automatically**
   - Click "Create All Card Buttons"
   - Click "Create All Display Slots"

5. **Add RassaController**
   - Create empty GameObject named "RassaController"
   - Add Component → `RassaController`
   - Click "Auto-Connect to RassaController" in UIBuilder

6. **Create Control Buttons**
   - Add 3 buttons to ControlsPanel:
     - Undo Button
     - Reset Button
     - Done Button
   - Drag them to RassaController's inspector

7. **Create Status Text**
   - Add TextMeshProUGUI to Canvas
   - Drag to RassaController's Status Text field

8. **Create ScriptableObject**
   - Right-click in Project → Create → ScriptableObjects → CardsInfo
   - Name it "RassaCardOrder"
   - Drag to RassaController's ScriptableObject field

9. **Initialize Cards**
   - Select RassaController
   - In Inspector, click "Initialize All Card Info Components"

10. **Done!** Press Play to test

---

### Option 2: Manual Setup

If you prefer manual control or already have UI setup:

1. **Create 32 Buttons** for card selection
2. **Create 32 Images** for displaying selected cards
3. **Add RassaController** to scene
4. **Drag all buttons** to `RassaInitialButtons` array
5. **Drag all images** to `RassaFinalImages` array
6. **Assign control buttons** and status text
7. **Click "Initialize All Card Info Components"** in Inspector

---

## 🎮 How to Use (Player Perspective)

1. **Start** - All 32 cards are visible
2. **Click Cards** - Select cards in the order you want
3. **View Progress** - Selected cards appear below in order
4. **Undo** - Remove last selection if you made a mistake
5. **Reset** - Start completely over
6. **Done** - Save the arrangement (must select all 32 cards)

---

## 📋 Checklist

- [ ] Rassa scene opened
- [ ] Canvas created
- [ ] Parent panels created (3 total)
- [ ] RassaUIBuilder added and configured
- [ ] 32 card buttons created
- [ ] 32 display slots created
- [ ] RassaController added
- [ ] Control buttons created (Undo, Reset, Done)
- [ ] Status text added
- [ ] ScriptableObject created and assigned
- [ ] Cards initialized
- [ ] Tested in Play mode

---

## 🔧 Customization

### Button Layout
Edit in `RassaUIBuilder`:
- `buttonRows` / `buttonColumns` - Grid size
- `buttonSize` - Size of each button
- `buttonSpacing` - Space between buttons

### Display Layout
Edit in `RassaUIBuilder`:
- `displayRows` / `displayColumns` - Grid size
- `displaySize` - Size of each card display
- `displaySpacing` - Space between displays

### Colors
Edit in `RassaController`:
- `normalButtonColor` - Default button color
- `disabledButtonColor` - Selected button color
- `highlightColor` - Highlight color (future use)

---

## 🐛 Troubleshooting

### Cards not showing sprites
➡️ Assign sprites in RassaUIBuilder or manually to button Images

### "CardInfoComponent not found" error
➡️ Click "Initialize All Card Info Components" in RassaController

### Can't click buttons
➡️ Make sure buttons have Button component and are interactable

### Save not working
➡️ Assign ScriptableObject in RassaController inspector

### Display slots not showing selected cards
➡️ Check that RassaFinalImages array is populated

---

## 📚 Next Steps

Once basic system is working:

1. **Style the UI** - Add backgrounds, borders, animations
2. **Load saved orders** - Implement loading previous arrangements
3. **Add sound effects** - Card selection sounds
4. **Add transitions** - Smooth card movements
5. **Multiple save slots** - Let players save different arrangements

See `RASSA_SYSTEM_README.md` for complete documentation.

---

## 💡 Tips

- Test with a small number of cards first (e.g., 8) to verify setup
- Use Unity's Layout Groups for automatic positioning
- Keep the Console window open to see helpful debug messages
- Save your scene frequently!

---

**Need Help?** Check the full documentation in `RASSA_SYSTEM_README.md`


