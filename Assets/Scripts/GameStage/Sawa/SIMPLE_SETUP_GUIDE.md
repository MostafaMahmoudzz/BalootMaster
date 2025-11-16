# Sawa Button - Simple Setup Guide ✅

## 🎯 Super Easy Setup

### Step 1: Create Your Button

1. **In Unity Scene:**
   - Open your scene (Main.unity)
   - Find your **Canvas** in the Hierarchy
   - Right-click on Canvas → **UI** → **Button**
   - **Rename it to:** `SawaButton` (exactly this name)

2. **Design Your Button:**
   - Change text to "صوا" or "Sawa" or whatever you like
   - Change colors (make it green, blue, gold - your choice!)
   - Change size and position
   - Add images, icons, effects - make it yours!

---

### Step 2: That's It! ✨

The code automatically:
- ✅ Finds your button (by name "SawaButton")
- ✅ Disables it at game start
- ✅ Hides it at game start
- ✅ Shows it when player can claim Sawa
- ✅ Enables it when player can claim Sawa
- ✅ Hides it again when Sawa is claimed or turn ends

---

## 📋 What Happens Automatically:

### At Game Start:
```
[SawaUI] Button not assigned, trying to find 'SawaButton' in scene...
[SawaUI] Found SawaButton in scene!
[SawaUI] Button assigned and listener added
[SawaUI] Button disabled and hidden at start  ← YOUR BUTTON IS HIDDEN
```

### During Gameplay (When You Can Claim Sawa):
```
[GameStage] West can claim Sawa!
[SawaUI] OnSawaAvailable called - Player: West, Available: True
[SawaUI] Sawa button shown and enabled  ← YOUR BUTTON APPEARS!
```

### When You Click It:
```
[SawaUI] West clicked Sawa button!
[GameStage] === West CLAIMED SAWA ===
[SawaAutoPlay] Auto-resolving remaining tricks...
[SawaUI] Sawa button hidden and disabled  ← YOUR BUTTON HIDES AGAIN
```

---

## 🎨 Button Design Freedom

You can customize EVERYTHING:
- ✅ **Text** - Any language, any font
- ✅ **Colors** - Any color scheme
- ✅ **Size** - Big, small, wide, tall
- ✅ **Position** - Top, bottom, center, corner
- ✅ **Images** - Add sprites, backgrounds, icons
- ✅ **Animations** - Fade, pulse, bounce, glow
- ✅ **Effects** - Shadows, outlines, particles
- ✅ **Sounds** - Add click sounds, hover sounds

The script only controls:
- When to show/hide it
- When to enable/disable it
- What happens when you click it

Everything else is YOUR creative design! 🎨

---

## ⚙️ Technical Details

### Button States:

**Hidden (Default):**
```csharp
SawaButton.gameObject.SetActive(false);
SawaButton.interactable = false;
```

**Shown (When Available):**
```csharp
SawaButton.gameObject.SetActive(true);
SawaButton.interactable = true;
```

### Detection Code:

The button is found automatically in `SawaUI.Awake()`:
```csharp
GameObject buttonObj = GameObject.Find("SawaButton");
SawaButton = buttonObj.GetComponent<Button>();
```

Then immediately disabled:
```csharp
SawaButton.interactable = false;
SawaButton.gameObject.SetActive(false);
```

---

## 🚨 Troubleshooting

### "Button not found" Warning:
**Problem:** Button doesn't exist or has wrong name  
**Solution:** Make sure you created a button named exactly **"SawaButton"**

### Button Always Visible:
**Problem:** Button shows at game start  
**Solution:** This is now fixed! Button is disabled at start automatically

### Button Never Shows:
**Problem:** Conditions not met  
**Solution:** Check console logs to see if "[GameStage] can claim Sawa!" appears

### Button Doesn't Work When Clicked:
**Problem:** Button component missing or listener failed  
**Solution:** Make sure your button has a Button component (not just an Image)

---

## 📝 Checklist

- [ ] Created a Button in Canvas
- [ ] Renamed it to "SawaButton"
- [ ] Designed it how you want
- [ ] Started the game
- [ ] Checked console for "Found SawaButton in scene!"
- [ ] Checked console for "Button disabled and hidden at start"
- [ ] Played until you can claim Sawa
- [ ] Button appears automatically!

---

## 🎮 Example Unity Setup

```
Scene Hierarchy:
├── Canvas
│   ├── Score Panel
│   ├── Bidding Panel
│   ├── SawaButton  ← Your custom button here!
│   │   ├── Text (child)
│   │   └── Icon (optional child)
│   └── Other UI...
└── Game Manager
```

---

## ✨ That's All!

Just create a button named "SawaButton" and the rest is automatic! 🚀

**The code will:**
1. Find it ✅
2. Disable it at start ✅
3. Show it when needed ✅
4. Hide it when done ✅

You just design it! 🎨

