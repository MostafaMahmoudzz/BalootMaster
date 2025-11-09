using Pebble;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controller for the Rassa system - allows players to arrange cards in custom order
/// </summary>
public class RassaController : MonoBehaviour
{
    [Header("UI References")]
    public Button[] RassaInitialButtons;
    public GameObject[] RassaFinalImages;
    public Button undoButton;
    public Button resetButton;
    public Button doneButton;
    public TextMeshProUGUI statusText;

    [Header("Data")]
    public CardsInfoScriptable RassaCardsFinalOrderScriptable;

    [Header("Visual Settings")]
    public Color normalButtonColor = Color.white;
    public Color disabledButtonColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    public Color highlightColor = Color.yellow;

    private int cardsCounter = 0;
    private List<CardInfo> orderedRassaCards = new List<CardInfo>();
    private List<Button> selectedButtons = new List<Button>();

    private void Start()
    {
        InitializeButtons();
        UpdateStatusText();
        UpdateUndoButton();
    }

    private void InitializeButtons()
    {
        // Setup click listeners for all card buttons
        for (int i = 0; i < RassaInitialButtons.Length; i++)
        {
            int index = i; // Capture for lambda
            Button button = RassaInitialButtons[i];
            button.onClick.AddListener(() => AddCardToRassa(button));
        }

        // Setup undo button
        if (undoButton != null)
        {
            undoButton.onClick.AddListener(UndoLastCard_UIEventHandler);
        }

        // Setup reset button
        if (resetButton != null)
        {
            resetButton.onClick.AddListener(ResetRassa_UIEventHandler);
        }

        // Setup done button
        if (doneButton != null)
        {
            doneButton.onClick.AddListener(DoneRassa_UIEventHandler);
        }
    }

    private void AddCardToRassa(Button clickedButton)
    {
        // Check if we've reached the maximum
        if (cardsCounter >= RassaFinalImages.Length)
        {
            Debug.Log("All cards have been selected!");
            return;
        }

        // Check if this button was already clicked
        if (!clickedButton.image.enabled)
        {
            Debug.Log("Card already selected!");
            return;
        }

        // Get card info from the button
        CardInfoComponent cardInfoComp = clickedButton.GetComponent<CardInfoComponent>();
        if (cardInfoComp == null || cardInfoComp.cardInfo == null)
        {
            Debug.LogError("Button doesn't have CardInfoComponent or cardInfo is null!");
            return;
        }

        // Disable the button visually
        clickedButton.image.enabled = false;
        ColorBlock colors = clickedButton.colors;
        colors.disabledColor = disabledButtonColor;
        clickedButton.colors = colors;
        clickedButton.interactable = false;

        // Display the card in the final images area
        Image finalImage = RassaFinalImages[cardsCounter].GetComponent<Image>();
        finalImage.sprite = clickedButton.image.sprite;
        finalImage.enabled = true;

        // Add to ordered list
        orderedRassaCards.Add(new CardInfo(cardInfoComp.cardInfo.Value, cardInfoComp.cardInfo.Family));
        selectedButtons.Add(clickedButton);

        cardsCounter++;

        // Update UI
        UpdateStatusText();
        UpdateUndoButton();

        Debug.Log($"Added card: {cardInfoComp.cardInfo.ToString()} - Total: {cardsCounter}/{RassaFinalImages.Length}");
    }

    public void UndoLastCard_UIEventHandler()
    {
        if (cardsCounter <= 0)
        {
            Debug.Log("No cards to undo!");
            return;
        }

        cardsCounter--;

        // Hide the last final image
        RassaFinalImages[cardsCounter].GetComponent<Image>().enabled = false;

        // Re-enable the button
        Button lastButton = selectedButtons[cardsCounter];
        lastButton.image.enabled = true;
        lastButton.interactable = true;

        // Remove from lists
        orderedRassaCards.RemoveAt(cardsCounter);
        selectedButtons.RemoveAt(cardsCounter);

        // Update UI
        UpdateStatusText();
        UpdateUndoButton();

        Debug.Log($"Undone last card - Remaining: {cardsCounter}");
    }

    public void ResetRassa_UIEventHandler()
    {
        cardsCounter = 0;
        orderedRassaCards.Clear();
        selectedButtons.Clear();

        // Hide all final images
        for (int i = 0; i < RassaFinalImages.Length; i++)
        {
            RassaFinalImages[i].GetComponent<Image>().enabled = false;
        }

        // Re-enable all buttons
        for (int i = 0; i < RassaInitialButtons.Length; i++)
        {
            RassaInitialButtons[i].image.enabled = true;
            RassaInitialButtons[i].interactable = true;
        }

        // Update UI
        UpdateStatusText();
        UpdateUndoButton();

        Debug.Log("Reset Rassa arrangement");
    }

    public void DoneRassa_UIEventHandler()
    {
        // Check if all cards have been selected
        if (cardsCounter < RassaFinalImages.Length)
        {
            Debug.LogWarning($"Not all cards selected! {cardsCounter}/{RassaFinalImages.Length}");
            if (statusText != null)
            {
                statusText.text = $"Please select all {RassaFinalImages.Length} cards!";
                statusText.color = Color.red;
            }
            return;
        }

        // Save to ScriptableObject
        if (RassaCardsFinalOrderScriptable != null)
        {
            RassaCardsFinalOrderScriptable.cardsInfo.Clear();
            orderedRassaCards.Reverse();
            foreach (var card in orderedRassaCards)
            {
                RassaCardsFinalOrderScriptable.cardsInfo.Add(new CardInfo(card.Value, card.Family));
            }

            Debug.Log($"Saved {orderedRassaCards.Count} cards to ScriptableObject");

            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(RassaCardsFinalOrderScriptable);
            UnityEditor.AssetDatabase.SaveAssets();
            #endif
        }

        // Save to PlayerPrefs as backup
        SaveToPlayerPrefs();

        if (statusText != null)
        {
            statusText.text = "Card arrangement saved successfully!";
            statusText.color = Color.green;
        }

        Debug.Log("Rassa arrangement completed and saved!");
    }

    private void SaveToPlayerPrefs()
    {
        string jsonData = JsonUtility.ToJson(new CardListWrapper { cards = orderedRassaCards });
        PlayerPrefs.SetString("RassaCardOrder", jsonData);
        PlayerPrefs.Save();
        Debug.Log("Saved to PlayerPrefs: " + jsonData);
    }

    public void LoadFromPlayerPrefs()
    {
        if (PlayerPrefs.HasKey("RassaCardOrder"))
        {
            string jsonData = PlayerPrefs.GetString("RassaCardOrder");
            CardListWrapper wrapper = JsonUtility.FromJson<CardListWrapper>(jsonData);
            
            if (wrapper != null && wrapper.cards != null)
            {
                // TODO: Implement loading and applying the saved order
                Debug.Log($"Loaded {wrapper.cards.Count} cards from PlayerPrefs");
            }
        }
    }

    private void UpdateStatusText()
    {
        if (statusText != null)
        {
            statusText.text = $"Cards Selected: {cardsCounter} / {RassaFinalImages.Length}";
            statusText.color = Color.white;
        }
    }

    private void UpdateUndoButton()
    {
        if (undoButton != null)
        {
            undoButton.interactable = cardsCounter > 0;
        }
    }

    // Helper method to initialize all 32 cards programmatically
    public void InitializeAllCards()
    {
        if (RassaInitialButtons.Length != 32)
        {
            Debug.LogWarning($"Expected 32 buttons, but found {RassaInitialButtons.Length}");
        }

        int buttonIndex = 0;
        Card32Family[] families = { Card32Family.Clubs, Card32Family.Heart, Card32Family.Diamond, Card32Family.Spade };
        Card32Value[] values = { Card32Value.Seven, Card32Value.Eight, Card32Value.Nine, Card32Value.Jack, 
                                 Card32Value.Queen, Card32Value.King, Card32Value.Ten, Card32Value.Ace };

        foreach (Card32Family family in families)
        {
            foreach (Card32Value value in values)
            {
                if (buttonIndex >= RassaInitialButtons.Length) break;

                CardInfoComponent cardComp = RassaInitialButtons[buttonIndex].GetComponent<CardInfoComponent>();
                if (cardComp == null)
                {
                    cardComp = RassaInitialButtons[buttonIndex].gameObject.AddComponent<CardInfoComponent>();
                }

                cardComp.SetCardInfo(value, family);
                buttonIndex++;
            }
        }

        Debug.Log($"Initialized {buttonIndex} cards");
    }
}

[Serializable]
public class CardListWrapper
{
    public List<CardInfo> cards;
}
