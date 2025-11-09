# Rassa System - Card Arrangement Feature

## Overview

The **Rassa System** is a custom card arrangement feature for the Baloot game. It allows players to view all 32 cards and select them one by one to create a custom deck order. Each selected card is displayed in sequence, showing the player the new arrangement they're creating.

## Features

✅ **Card Selection UI** - All 32 cards displayed as buttons  
✅ **Visual Feedback** - Selected cards are disabled and shown in order below  
✅ **Undo Functionality** - Remove the last selected card  
✅ **Reset Button** - Start over from scratch  
✅ **Duplicate Prevention** - Can't select the same card twice  
✅ **Progress Tracking** - Shows how many cards have been selected  
✅ **Save System** - Saves to both ScriptableObject and PlayerPrefs  
✅ **Auto-Initialization** - Helper method to set up all 32 cards automatically  

## File Structure

```
Assets/Rasa/
├── CardInfo.cs                  # Data class for card information
├── CardInfoComponent.cs         # MonoBehaviour to attach card data to UI
├── CardsInfoScriptable.cs       # ScriptableObject for saving card order
├── RassaController.cs           # Main controller for the Rassa scene
├── RassaSpriteManager.cs        # Helper for managing card sprites
└── RASSA_SYSTEM_README.md       # This file

Assets/UnityEditor/
└── RassaControllerEditor.cs     # Custom editor for easier setup
```

## Setup Instructions

### 1. Scene Setup

1. Open or create the "Rassa" scene
2. Create a Canvas for the UI

### 2. Card Selection Area (Top)

Create 32 buttons for card selection:

```
Canvas
└── CardSelectionPanel
    ├── Button_Card1 (with Image component)
    ├── Button_Card2 (with Image component)
    ├── ... (32 buttons total)
    └── Button_Card32 (with Image component)
```

**For each button:**
- Add an `Image` component with the card sprite
- Add the `CardInfoComponent` script (or use the auto-initialization)
- The button should be interactable

### 3. Display Area (Bottom)

Create 32 Image objects to show selected cards in order:

```
Canvas
└── SelectedCardsPanel
    ├── Image_Slot1 (Image component, initially disabled)
    ├── Image_Slot2 (Image component, initially disabled)
    ├── ... (32 images total)
    └── Image_Slot32 (Image component, initially disabled)
```

### 4. Control Buttons

```
Canvas
└── ControlPanel
    ├── UndoButton
    ├── ResetButton
    └── DoneButton
```

### 5. Status Text

Add a TextMeshProUGUI element to show progress:

```
Canvas
└── StatusText (TextMeshProUGUI)
```

### 6. RassaController Setup

1. Create an empty GameObject named "RassaController"
2. Add the `RassaController` component
3. In the Inspector:
   - **Rassa Initial Buttons**: Drag all 32 card buttons
   - **Rassa Final Images**: Drag all 32 display images
   - **Undo Button**: Drag the undo button
   - **Reset Button**: Drag the reset button
   - **Done Button**: Drag the done button
   - **Status Text**: Drag the status text element
   - **Rassa Cards Final Order Scriptable**: Create and assign a ScriptableObject

### 7. Create ScriptableObject for Saving

1. In Unity: Right-click in Project window
2. Create → ScriptableObjects → CardsInfo
3. Name it "RassaCardOrder"
4. Assign it to the RassaController

### 8. Auto-Initialize Cards

1. Select the RassaController in the scene
2. In the Inspector, click **"Initialize All Card Info Components"**
3. This will automatically add CardInfoComponent to all 32 buttons and set their values

## Usage Flow

### For Players:

1. **View All Cards** - All 32 cards are displayed at the top
2. **Select Cards** - Click cards one by one in the desired order
3. **See Progress** - Selected cards appear below in order
4. **Undo if Needed** - Click "Undo" to remove the last selection
5. **Reset** - Click "Reset" to start over
6. **Finish** - Click "Done" when all 32 cards are selected

### Card Order:

The system uses standard 32-card deck:
- **Families**: Clubs, Hearts, Diamonds, Spades
- **Values**: Seven, Eight, Nine, Jack, Queen, King, Ten, Ace

### Sprite Naming Convention:

Card sprites should follow this format:
- `Club01.png` = Ace of Clubs
- `Club07.png` = Seven of Clubs
- `Club08.png` = Eight of Clubs
- `Heart11.png` = Jack of Hearts
- `Diamond12.png` = Queen of Diamonds
- `Spade13.png` = King of Spades
- etc.

## Code API

### RassaController Public Methods

```csharp
// UI Event Handlers
public void UndoLastCard_UIEventHandler()
public void ResetRassa_UIEventHandler()
public void DoneRassa_UIEventHandler()

// Setup Helper
public void InitializeAllCards()

// Load previously saved order
public void LoadFromPlayerPrefs()
```

### CardInfo Methods

```csharp
// Create new card info
CardInfo card = new CardInfo(Card32Value.Ace, Card32Family.Spade);

// Get sprite name
string spriteName = card.GetSpriteName(); // Returns "Spade01"

// Display card info
string info = card.ToString(); // Returns "Ace of Spade"
```

### CardInfoComponent

```csharp
// Attach to button and set card info
CardInfoComponent component = button.AddComponent<CardInfoComponent>();
component.SetCardInfo(Card32Value.King, Card32Family.Heart);
```

## Save System

The Rassa system saves the card order in two places:

1. **ScriptableObject** (`RassaCardsFinalOrderScriptable`)
   - Saved as an asset in the project
   - Persists in the Unity Editor
   - Best for development and testing

2. **PlayerPrefs** (JSON backup)
   - Saved to player's local machine
   - Persists across game sessions
   - Best for runtime/build

## Accessing Saved Data

To use the saved card order in your game:

```csharp
// From ScriptableObject
public CardsInfoScriptable savedOrder;

void LoadCustomDeck()
{
    List<CardInfo> cards = savedOrder.cardsInfo;
    
    foreach (CardInfo card in cards)
    {
        Debug.Log($"Card: {card.Value} of {card.Family}");
        // Use this to arrange your game deck
    }
}

// From PlayerPrefs
void LoadFromPrefs()
{
    if (PlayerPrefs.HasKey("RassaCardOrder"))
    {
        string json = PlayerPrefs.GetString("RassaCardOrder");
        CardListWrapper wrapper = JsonUtility.FromJson<CardListWrapper>(json);
        
        foreach (CardInfo card in wrapper.cards)
        {
            // Use card data
        }
    }
}
```

## Troubleshooting

### Cards not initializing
- Make sure you have exactly 32 buttons in `RassaInitialButtons`
- Click "Initialize All Card Info Components" in the Inspector

### Sprites not showing
- Check that card sprites are in `Assets/Rasa/Cards/`
- Ensure sprite names match the naming convention
- Verify Image components have sprites assigned

### Save not persisting
- Check ScriptableObject is assigned in Inspector
- In builds, only PlayerPrefs will persist

### Undo button not working
- Ensure undo button is assigned in Inspector
- Check that `UndoLastCard_UIEventHandler` is connected to button click

## Future Enhancements

Potential features to add:

- 🔄 Load and apply previously saved arrangements
- 🎨 Different visual themes for cards
- 📊 Show card statistics
- 🎯 Quick arrangements (sort by family, value, etc.)
- 💾 Multiple save slots
- 🔀 Shuffle option
- ⚡ Keyboard shortcuts

## Support

For questions or issues, refer to:
- `RassaController.cs` - Main logic
- `CardInfo.cs` - Card data structure
- Unity Console - Check for debug logs

---

**Version**: 1.0  
**Created**: November 2025  
**Game**: Baloot Master  

