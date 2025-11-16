using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Pebble;

/// <summary>
/// UI for Assaa card reordering system
/// Allows player to reorder the deck by selecting a range and moving it to a new position
/// </summary>
public class AssaaCardReorderUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject reorderPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI instructionsText;
    public TMP_InputField startPositionInput;
    public TMP_InputField targetPositionInput;
    public Button confirmButton;
    public Button cancelButton;
    public TextMeshProUGUI errorText;
    public TextMeshProUGUI previewText;

    [Header("Settings")]
    public string defaultInstructions = "Enter two numbers to reorder the deck:\n\n" +
        "1. Start Position (1-32): Cards from this position to the end will be selected\n" +
        "2. Target Position: Where to move the selected cards (must be less than start position)\n\n" +
        "Example: Start=10, Target=5\n" +
        "Cards 10-32 (23 cards) will be moved to position 5";

    private Player currentPlayer;
    private BeloteDeck currentDeck;
    private bool waitingForInput = false;

    private void Awake()
    {
        // Subscribe to Assaa reorder prompt events
        GameEventDispatcher.Subscribe<AssaaReorderPromptEvent>(OnAssaaReorderPrompt);

        // Setup button listeners
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(OnConfirmClicked);
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(OnCancelClicked);
        }

        // Setup input field listeners for live preview
        if (startPositionInput != null)
        {
            startPositionInput.onValueChanged.AddListener(OnInputChanged);
        }

        if (targetPositionInput != null)
        {
            targetPositionInput.onValueChanged.AddListener(OnInputChanged);
        }

        // Hide the panel initially
        if (reorderPanel != null)
        {
            reorderPanel.SetActive(false);
        }

        // Set default instructions
        if (instructionsText != null)
        {
            instructionsText.text = defaultInstructions;
        }

        // Hide error text initially
        if (errorText != null)
        {
            errorText.text = "";
            errorText.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        GameEventDispatcher.UnSubscribe<AssaaReorderPromptEvent>(OnAssaaReorderPrompt);
    }

    /// <summary>
    /// Called when AssaaReorderPromptEvent is received
    /// </summary>
    private void OnAssaaReorderPrompt(AssaaReorderPromptEvent evt)
    {
        Debug.Log($"[AssaaCardReorderUI] Showing card reorder UI for player: {evt.ReorderingPlayer?.Name}");

        currentPlayer = evt.ReorderingPlayer;
        currentDeck = evt.Deck;
        waitingForInput = true;

        // Update title with player name
        if (titleText != null)
        {
            string playerName = evt.ReorderingPlayer?.Name ?? "Player";
            titleText.text = $"<b>{playerName} - Assaa Card Reordering</b>";
        }

        // Clear previous inputs
        if (startPositionInput != null) startPositionInput.text = "";
        if (targetPositionInput != null) targetPositionInput.text = "";
        if (errorText != null)
        {
            errorText.text = "";
            errorText.gameObject.SetActive(false);
        }
        if (previewText != null) previewText.text = "";

        // Show the panel
        if (reorderPanel != null)
        {
            reorderPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Called when input values change - update preview
    /// </summary>
    private void OnInputChanged(string value)
    {
        UpdatePreview();
    }

    /// <summary>
    /// Update the preview text based on current input values
    /// </summary>
    private void UpdatePreview()
    {
        if (previewText == null) return;

        // Try to parse input values
        if (int.TryParse(startPositionInput?.text, out int startPos) &&
            int.TryParse(targetPositionInput?.text, out int targetPos))
        {
            // Validate ranges
            if (startPos >= 1 && startPos <= 32)
            {
                int cardsToMove = 32 - startPos + 1;
                previewText.text = $"<color=#00FF00>Preview:\nWill move {cardsToMove} cards (positions {startPos}-32)\nto position {targetPos}</color>";
            }
            else
            {
                previewText.text = "";
            }
        }
        else
        {
            previewText.text = "";
        }
    }

    /// <summary>
    /// Validate the input values
    /// </summary>
    private bool ValidateInput(out int startPos, out int targetPos, out string error)
    {
        startPos = 0;
        targetPos = 0;
        error = "";

        // Check if inputs are not empty
        if (string.IsNullOrEmpty(startPositionInput?.text) || string.IsNullOrEmpty(targetPositionInput?.text))
        {
            error = "Please enter both numbers";
            return false;
        }

        // Try to parse start position
        if (!int.TryParse(startPositionInput.text, out startPos))
        {
            error = "Start position must be a valid number";
            return false;
        }

        // Try to parse target position
        if (!int.TryParse(targetPositionInput.text, out targetPos))
        {
            error = "Target position must be a valid number";
            return false;
        }

        // Validate start position range (1-32)
        if (startPos < 1 || startPos > 32)
        {
            error = "Start position must be between 1 and 32";
            return false;
        }

        // Validate target position is less than start position
        if (targetPos >= startPos)
        {
            error = "Target position must be less than start position";
            return false;
        }

        // Validate target position is positive
        if (targetPos < 1)
        {
            error = "Target position must be at least 1";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Show error message
    /// </summary>
    private void ShowError(string message)
    {
        if (errorText != null)
        {
            errorText.text = $"<color=#FF0000>Error: {message}</color>";
            errorText.gameObject.SetActive(true);
        }
        Debug.LogWarning($"[AssaaCardReorderUI] Validation error: {message}");
    }

    /// <summary>
    /// Hide error message
    /// </summary>
    private void HideError()
    {
        if (errorText != null)
        {
            errorText.text = "";
            errorText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Called when Confirm button is clicked
    /// </summary>
    private void OnConfirmClicked()
    {
        if (!waitingForInput) return;

        HideError();

        // Validate input
        if (!ValidateInput(out int startPos, out int targetPos, out string error))
        {
            ShowError(error);
            return;
        }

        Debug.Log($"[AssaaCardReorderUI] Reordering confirmed: Start={startPos}, Target={targetPos}");

        // Perform the reordering
        bool success = ReorderDeck(startPos, targetPos);

        if (success)
        {
            Debug.Log($"[AssaaCardReorderUI] Deck reordered successfully by {currentPlayer?.Name}");

            // Send completion event
            AssaaReorderCompleteEvent evt = Pools.Claim<AssaaReorderCompleteEvent>();
            evt.Success = true;
            evt.ReorderingPlayer = currentPlayer;
            evt.DeckCards = new System.Collections.Generic.List<BeloteCard>(currentDeck?.Cards ?? new System.Collections.Generic.List<BeloteCard>());
            GameEventDispatcher.SendEvent(evt);

            // Hide the panel
            HidePanel();
        }
        else
        {
            ShowError("Failed to reorder deck. Please try again.");
        }
    }

    /// <summary>
    /// Called when Cancel button is clicked
    /// </summary>
    private void OnCancelClicked()
    {
        if (!waitingForInput) return;

        Debug.Log($"[AssaaCardReorderUI] Player {currentPlayer?.Name} cancelled card reordering");

        // Send completion event with failure
        AssaaReorderCompleteEvent evt = Pools.Claim<AssaaReorderCompleteEvent>();
        evt.Success = false;
        evt.ReorderingPlayer = currentPlayer;
        evt.DeckCards = null;  // No deck cards when cancelled
        GameEventDispatcher.SendEvent(evt);

        // Hide the panel
        HidePanel();
    }

    /// <summary>
    /// Reorder the deck based on user input
    /// </summary>
    private bool ReorderDeck(int startPos, int targetPos)
    {
        if (currentDeck == null || currentDeck.Cards == null)
        {
            Debug.LogError("[AssaaCardReorderUI] Cannot reorder - deck is null!");
            return false;
        }

        // Convert to 0-based indices
        int startIndex = startPos - 1;  // User enters 1-32, we use 0-31
        int targetIndex = targetPos - 1;

        Debug.Log($"[AssaaCardReorderUI] Reordering deck: Moving cards [{startIndex}-31] to position {targetIndex}");
        Debug.Log($"[AssaaCardReorderUI] Deck size before reorder: {currentDeck.Size}");

        try
        {
            // Get all cards as a list
            var cards = new System.Collections.Generic.List<BeloteCard>(currentDeck.Cards);

            if (cards.Count != 32)
            {
                Debug.LogError($"[AssaaCardReorderUI] Deck has {cards.Count} cards, expected 32!");
                return false;
            }

            // Extract cards from startIndex to end
            int cardsToMove = cards.Count - startIndex;
            var selectedCards = cards.GetRange(startIndex, cardsToMove);
            
            Debug.Log($"[AssaaCardReorderUI] Selected {selectedCards.Count} cards to move");

            // Remove selected cards from original list
            cards.RemoveRange(startIndex, cardsToMove);

            // Insert selected cards at target position
            cards.InsertRange(targetIndex, selectedCards);

            Debug.Log($"[AssaaCardReorderUI] Cards reordered. New deck size: {cards.Count}");

            // Clear the deck and re-add cards in new order
            currentDeck.Clear();
            foreach (var card in cards)
            {
                currentDeck.AddCard(card);
            }

            Debug.Log($"[AssaaCardReorderUI] ✅ Deck reordering complete! Final size: {currentDeck.Size}");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AssaaCardReorderUI] Exception during reordering: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Hide the reorder panel
    /// </summary>
    private void HidePanel()
    {
        waitingForInput = false;
        currentPlayer = null;
        currentDeck = null;

        if (reorderPanel != null)
        {
            reorderPanel.SetActive(false);
        }
    }
}

