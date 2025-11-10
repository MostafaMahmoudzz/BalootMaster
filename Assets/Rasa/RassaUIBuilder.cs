using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Pebble;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Helper script to programmatically build the Rassa UI
/// Can be used to quickly set up or rebuild the UI in the scene
/// </summary>
public class RassaUIBuilder : MonoBehaviour
{
    [Header("UI Parent Containers")]
    public Transform cardButtonsParent;
    public Transform selectedCardsParent;
    public Transform controlsParent;

    [Header("Prefabs")]
    public GameObject cardButtonPrefab;
    public GameObject cardImagePrefab;

    [Header("Sprites")]
    public Sprite[] allCardSprites; // Should contain 32 sprites

    [Header("Layout Settings")]
    public int buttonRows = 4;
    public int buttonColumns = 8;
    public Vector2 buttonSize = new Vector2(80, 112);
    public float buttonSpacing = 10f;

    [Header("Selected Cards Display")]
    public int displayRows = 2;
    public int displayColumns = 16;
    public Vector2 displaySize = new Vector2(60, 84);
    public float displaySpacing = 5f;

    /// <summary>
    /// Creates all 32 card buttons programmatically
    /// </summary>
    public GameObject[] CreateCardButtons()
    {
        if (cardButtonsParent == null)
        {
            Debug.LogError("Card buttons parent not assigned!");
            return null;
        }

        // Clear existing buttons
        ClearChildren(cardButtonsParent);

        GameObject[] buttons = new GameObject[32];

        Card32Family[] families = { Card32Family.Clubs, Card32Family.Heart, Card32Family.Diamond, Card32Family.Spade };
        Card32Value[] values = { Card32Value.Seven, Card32Value.Eight, Card32Value.Nine, Card32Value.Jack,
                                 Card32Value.Queen, Card32Value.King, Card32Value.Ten, Card32Value.Ace };

        int index = 0;
        int row = 0;
        int col = 0;

        foreach (Card32Family family in families)
        {
            foreach (Card32Value value in values)
            {
                // Create button
                GameObject buttonObj = CreateCardButton(index, row, col, value, family);
                buttons[index] = buttonObj;

                index++;
                col++;
                if (col >= buttonColumns)
                {
                    col = 0;
                    row++;
                }
            }
        }

        Debug.Log($"Created {buttons.Length} card buttons");
        return buttons;
    }

    private GameObject CreateCardButton(int index, int row, int col, Card32Value value, Card32Family family)
    {
        GameObject buttonObj;

        if (cardButtonPrefab != null)
        {
            buttonObj = Instantiate(cardButtonPrefab, cardButtonsParent);
        }
        else
        {
            buttonObj = new GameObject($"Card_{family}_{value}");
            buttonObj.transform.SetParent(cardButtonsParent);
            
            // Add Image
            Image img = buttonObj.AddComponent<Image>();
            img.sprite = GetSpriteForCard(value, family);

            // Add Button
            buttonObj.AddComponent<Button>();
        }

        // Set position
        RectTransform rect = buttonObj.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchoredPosition = new Vector2(
                col * (buttonSize.x + buttonSpacing),
                -row * (buttonSize.y + buttonSpacing)
            );
            rect.sizeDelta = buttonSize;
        }

        // Add CardInfoComponent
        CardInfoComponent cardComp = buttonObj.GetComponent<CardInfoComponent>();
        if (cardComp == null)
        {
            cardComp = buttonObj.AddComponent<CardInfoComponent>();
        }
        cardComp.SetCardInfo(value, family);

        buttonObj.name = $"Btn_Card_{index:D2}_{family}_{value}";
        return buttonObj;
    }

    /// <summary>
    /// Creates all 32 display slots for selected cards
    /// </summary>
    public GameObject[] CreateDisplaySlots()
    {
        if (selectedCardsParent == null)
        {
            Debug.LogError("Selected cards parent not assigned!");
            return null;
        }

        // Clear existing slots
        ClearChildren(selectedCardsParent);

        GameObject[] slots = new GameObject[32];

        int row = 0;
        int col = 0;

        for (int i = 0; i < 32; i++)
        {
            GameObject slotObj;

            if (cardImagePrefab != null)
            {
                slotObj = Instantiate(cardImagePrefab, selectedCardsParent);
            }
            else
            {
                slotObj = new GameObject($"Slot_{i:D2}");
                slotObj.transform.SetParent(selectedCardsParent);
                
                Image img = slotObj.AddComponent<Image>();
                img.enabled = false; // Start disabled
            }

            // Set position
            RectTransform rect = slotObj.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchoredPosition = new Vector2(
                    col * (displaySize.x + displaySpacing),
                    -row * (displaySize.y + displaySpacing)
                );
                rect.sizeDelta = displaySize;
            }

            slotObj.name = $"Display_Slot_{i:D2}";
            slots[i] = slotObj;

            col++;
            if (col >= displayColumns)
            {
                col = 0;
                row++;
            }
        }

        Debug.Log($"Created {slots.Length} display slots");
        return slots;
    }

    /// <summary>
    /// Automatically wires up buttons and slots to RassaController
    /// </summary>
    public void AutoConnectToController(RassaController controller)
    {
        if (controller == null)
        {
            Debug.LogError("RassaController not provided!");
            return;
        }

        // Get all buttons
        Button[] buttons = cardButtonsParent.GetComponentsInChildren<Button>();
        controller.RassaInitialButtons = buttons;

        // Get all display slots
        Image[] displayImages = selectedCardsParent.GetComponentsInChildren<Image>(true);
        GameObject[] displayObjs = new GameObject[displayImages.Length];
        for (int i = 0; i < displayImages.Length; i++)
        {
            displayObjs[i] = displayImages[i].gameObject;
        }
        controller.RassaFinalImages = displayObjs;

        Debug.Log($"Connected {buttons.Length} buttons and {displayObjs.Length} slots to controller");

        #if UNITY_EDITOR
        EditorUtility.SetDirty(controller);
        #endif
    }

    private void ClearChildren(Transform parent)
    {
        while (parent.childCount > 0)
        {
            DestroyImmediate(parent.GetChild(0).gameObject);
        }
    }

    private Sprite GetSpriteForCard(Card32Value value, Card32Family family)
    {
        CardInfo cardInfo = new CardInfo(value, family);
        string spriteName = cardInfo.GetSpriteName();

        if (allCardSprites != null)
        {
            foreach (Sprite sprite in allCardSprites)
            {
                if (sprite != null && sprite.name == spriteName)
                {
                    return sprite;
                }
            }
        }

        return null;
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(RassaUIBuilder))]
    public class RassaUIBuilderEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            RassaUIBuilder builder = (RassaUIBuilder)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Quick Setup", EditorStyles.boldLabel);

            if (GUILayout.Button("Create All Card Buttons", GUILayout.Height(30)))
            {
                builder.CreateCardButtons();
                EditorUtility.DisplayDialog("Success", "Created 32 card buttons!", "OK");
            }

            if (GUILayout.Button("Create All Display Slots", GUILayout.Height(30)))
            {
                builder.CreateDisplaySlots();
                EditorUtility.DisplayDialog("Success", "Created 32 display slots!", "OK");
            }

            EditorGUILayout.Space();

            RassaController controller = FindObjectOfType<RassaController>();
            if (controller != null)
            {
                if (GUILayout.Button("Auto-Connect to RassaController", GUILayout.Height(30)))
                {
                    builder.AutoConnectToController(controller);
                    EditorUtility.DisplayDialog("Success", "Connected UI to RassaController!", "OK");
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No RassaController found in scene.", MessageType.Warning);
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "1. Assign parent containers\n" +
                "2. Click 'Create All Card Buttons'\n" +
                "3. Click 'Create All Display Slots'\n" +
                "4. Click 'Auto-Connect to RassaController'",
                MessageType.Info
            );
        }
    }
    #endif
}


