# Sawa Button - Manual Setup Guide

## ✅ Perfect! Now You Can Create Your Own Button!

The code has been updated so you can create your own custom Sawa button in Unity's UI system.

---

## 🎨 How to Setup Your Own Sawa Button

### Step 1: Create Your Button in Unity

1. **Open your Scene** (Main.unity or wherever your Canvas is)

2. **Find your Canvas** in the Hierarchy

3. **Right-click on the Canvas** → UI → Button

4. **Name it** "SawaButton" (or whatever you want)

5. **Design your button:**
   - Change the text to "صوا" or "Sawa" or whatever you like
   - Change colors (make it green, blue, whatever you prefer)
   - Change size and position
   - Add images, icons, animations - make it yours!

---

### Step 2: Find the SawaUI GameObject

When the game runs, a GameObject called **"SawaUI"** is automatically created.

**To assign your button:**

#### Option A: Assign at Runtime (In Code)
This is already handled in `GameStage.cs`, but you need to make a small modification.

#### Option B: Assign in Scene (Easier)
1. **Before running the game:**
   - Create a GameObject in your scene called "SawaUI"
   - Add the `SawaUI` component to it
   - In the Inspector, you'll see: **"Manual Setup - Assign Your Button Here"**
   - Drag your SawaButton from the Hierarchy into this field

2. **Make sure** you update `GameStage.cs` to find your scene's SawaUI instead of creating a new one.

---

### Step 3: Alternative - Modify GameStage Setup

Let me show you a better way. I'll modify the code so it finds your button automatically:

**Your button should be named exactly:** `SawaButton` (or you can change the name in the code below)

---

## 🔧 Automatic Button Finding

Let me update the code to automatically find your button by name:

```csharp
// In GameStage.cs, find this section and replace:

// OLD CODE:
GameObject sawaUIObj = new GameObject("SawaUI");
Canvas existingCanvas = GameObject.FindObjectOfType<Canvas>();
if (existingCanvas != null)
{
    sawaUIObj.transform.SetParent(existingCanvas.transform, false);
}
m_sawaUI = sawaUIObj.AddComponent<SawaUI>();

// NEW CODE:
GameObject sawaUIObj = new GameObject("SawaUI");
m_sawaUI = sawaUIObj.AddComponent<SawaUI>();

// Find your custom button by name
Button customButton = GameObject.Find("SawaButton")?.GetComponent<Button>();
if (customButton != null)
{
    m_sawaUI.SawaButton = customButton;
    Debug.Log("[GameStage] Custom Sawa button found and assigned!");
}
else
{
    Debug.LogWarning("[GameStage] SawaButton not found! Create a button named 'SawaButton' in your Canvas.");
}
```

---

## 📋 Quick Setup Summary

### Easy Way (Recommended):

1. **In Unity:**
   - Create a Button in your Canvas
   - Name it: **"SawaButton"**
   - Design it however you want

2. **In Code:**
   - I'll update `GameStage.cs` to automatically find this button

3. **Done!** The button will show/hide automatically during gameplay

---

### Manual Way:

1. Create button in Canvas
2. Create SawaUI GameObject in scene
3. Add SawaUI component
4. Drag button into Inspector field
5. Modify GameStage to NOT create new SawaUI

---

## ⚙️ Let Me Update the Code for You

I'll modify `GameStage.cs` to automatically find your button by name. Just create a button called "SawaButton" in your Canvas!

---

## 🎨 Button Design Tips

Your button can have:
- ✅ Custom sprites/images
- ✅ Animations (fade in/out, pulse, etc.)
- ✅ Arabic text styling
- ✅ Custom colors and effects
- ✅ Sound effects on hover/click
- ✅ Any Unity UI features you want!

The script only cares about:
- Showing/hiding it (SetActive)
- Listening for clicks (onClick)

Everything else is up to you!

---

## 🚀 Next Steps

**Tell me which setup you prefer:**

**Option 1:** "Auto-find by name" - I'll update the code to find a button named "SawaButton"

**Option 2:** "Inspector assignment" - You manually drag and drop in Unity Inspector

**Option 3:** Something else you prefer?

Let me know and I'll update the code accordingly! 🎯

