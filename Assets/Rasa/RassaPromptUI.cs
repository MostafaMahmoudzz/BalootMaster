using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Pebble;

/// <summary>
/// UI Dialog that prompts the player to choose whether to use Rassa or not
/// </summary>
public class RassaPromptUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject promptPanel;
    public TextMeshProUGUI messageText;
    public Button yesButton;
    public Button noButton;
    public TextMeshProUGUI yesButtonText;
    public TextMeshProUGUI noButtonText;

    [Header("Settings")]
    public string promptMessage = "Play with Rassa?\n(Use your custom card arrangement)";
    public float displayDuration = 0f; // 0 = wait for player input, >0 = auto-close after X seconds

    private Player currentPlayer;
    private bool waitingForResponse = false;

    private void Awake()
    {
        // Subscribe to Rassa prompt events
        GameEventDispatcher.Subscribe<RassaPromptEvent>(OnRassaPrompt);

        // Setup button listeners
        if (yesButton != null)
        {
            yesButton.onClick.AddListener(OnYesClicked);
        }

        if (noButton != null)
        {
            noButton.onClick.AddListener(OnNoClicked);
        }

        // Hide the panel initially
        if (promptPanel != null)
        {
            promptPanel.SetActive(false);
        }

        // Set button texts
        if (yesButtonText != null) yesButtonText.text = "Yes";
        if (noButtonText != null) noButtonText.text = "No";
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        GameEventDispatcher.UnSubscribe<RassaPromptEvent>(OnRassaPrompt);
    }

    /// <summary>
    /// Called when a RassaPromptEvent is received
    /// </summary>
    private void OnRassaPrompt(RassaPromptEvent evt)
    {
        Debug.Log($"[RassaPromptUI] Received prompt for player: {evt.AskingPlayer?.Name}");

        currentPlayer = evt.AskingPlayer;
        waitingForResponse = true;

        // Update message text
        if (messageText != null)
        {
            string playerName = evt.AskingPlayer?.Name ?? "Player";
            messageText.text = $"{playerName}, {promptMessage}";
        }

        // Show the panel
        if (promptPanel != null)
        {
            promptPanel.SetActive(true);
        }

        // If auto-close is enabled, close after duration
        if (displayDuration > 0)
        {
            Invoke(nameof(OnNoClicked), displayDuration); // Default to "No" if no response
        }
    }

    /// <summary>
    /// Called when Yes button is clicked
    /// </summary>
    private void OnYesClicked()
    {
        if (!waitingForResponse) return;

        Debug.Log($"[RassaPromptUI] Player {currentPlayer?.Name} chose YES - Use Rassa");

        // Send response event
        RassaResponseEvent evt = Pools.Claim<RassaResponseEvent>();
        evt.UseRassa = true;
        evt.RespondingPlayer = currentPlayer;
        GameEventDispatcher.SendEvent(evt);

        // Hide the panel
        HidePrompt();
    }

    /// <summary>
    /// Called when No button is clicked
    /// </summary>
    private void OnNoClicked()
    {
        if (!waitingForResponse) return;

        Debug.Log($"[RassaPromptUI] Player {currentPlayer?.Name} chose NO - Random deck");

        // Send response event
        RassaResponseEvent evt = Pools.Claim<RassaResponseEvent>();
        evt.UseRassa = false;
        evt.RespondingPlayer = currentPlayer;
        GameEventDispatcher.SendEvent(evt);

        // Hide the panel
        HidePrompt();
    }

    /// <summary>
    /// Hide the prompt panel
    /// </summary>
    private void HidePrompt()
    {
        waitingForResponse = false;
        currentPlayer = null;

        if (promptPanel != null)
        {
            promptPanel.SetActive(false);
        }

        // Cancel auto-close if it was scheduled
        CancelInvoke(nameof(OnNoClicked));
    }

    /// <summary>
    /// Public method to show the prompt (can be called directly if needed)
    /// </summary>
    public void ShowPrompt(Player player)
    {
        RassaPromptEvent evt = Pools.Claim<RassaPromptEvent>();
        evt.AskingPlayer = player;
        evt.RoundNumber = 0;
        GameEventDispatcher.SendEvent(evt);
    }
}

