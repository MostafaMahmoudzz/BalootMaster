using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Pebble;

/// <summary>
/// UI Dialog that prompts players to choose whether to use Assaa
/// Shown AFTER Rassa is chosen (YES)
/// First asks player to the right of Rassa chooser, then their teammate if first says NO
/// </summary>
public class AssaaPromptUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject promptPanel;
    public TextMeshProUGUI messageText;
    public Button yesButton;
    public Button noButton;
    public TextMeshProUGUI yesButtonText;
    public TextMeshProUGUI noButtonText;

    [Header("Settings")]
    public string promptMessage = "Use Assaa?\n(Reorder the deck before dealing)";
    
    [Header("AI Behavior")]
    [Tooltip("If true, AI can randomly choose to use Assaa. If false, AI always says no.")]
    public bool aiCanUseAssaa = false;
    [Tooltip("Chance (0-100) that AI will choose Assaa if allowed")]
    [Range(0, 100)]
    public int aiAssaaChance = 30;

    private Player currentPlayer;
    private int currentPromptNumber = 0;
    private bool waitingForResponse = false;

    private void Awake()
    {
        // Subscribe to Assaa prompt events
        GameEventDispatcher.Subscribe<AssaaPromptEvent>(OnAssaaPrompt);

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
        GameEventDispatcher.UnSubscribe<AssaaPromptEvent>(OnAssaaPrompt);
    }

    /// <summary>
    /// Called when an AssaaPromptEvent is received
    /// </summary>
    private void OnAssaaPrompt(AssaaPromptEvent evt)
    {
        Debug.Log($"[AssaaPromptUI] ========================================");
        Debug.Log($"[AssaaPromptUI] RECEIVED ASSAA PROMPT EVENT");
        Debug.Log($"[AssaaPromptUI] Player: {evt.AskingPlayer?.Name} ({evt.AskingPlayer?.Position})");
        Debug.Log($"[AssaaPromptUI] Prompt Number: {evt.PromptNumber}");
        Debug.Log($"[AssaaPromptUI] Is Human: {(evt.AskingPlayer is HumanPlayer)}");
        Debug.Log($"[AssaaPromptUI] ========================================");

        currentPlayer = evt.AskingPlayer;
        currentPromptNumber = evt.PromptNumber;

        // Check if this is a human player - only show UI for human players
        if (currentPlayer is HumanPlayer)
        {
            Debug.Log($"[AssaaPromptUI] ✅ This is a HUMAN player: {currentPlayer.Name}");
            Debug.Log($"[AssaaPromptUI] Current waiting state BEFORE: {waitingForResponse}");
            Debug.Log($"[AssaaPromptUI] Panel state BEFORE: {(promptPanel != null ? promptPanel.activeSelf.ToString() : "NULL")}");
            
            waitingForResponse = true;

            // Update message text with player identification and context
            if (messageText != null)
            {
                string playerName = evt.AskingPlayer?.Name ?? "Player";
                string rassaChooser = evt.RassaChooser?.Name ?? "Player";
                string contextText = evt.PromptNumber == 1 
                    ? $"{rassaChooser} chose Rassa!\nYou are to their right."
                    : $"Your teammate can use Assaa!";
                    
                messageText.text = $"<b><size=40>{playerName}</size></b>\n\n{contextText}\n\n{promptMessage}";
                Debug.Log($"[AssaaPromptUI] Message text updated: {messageText.text}");
            }
            else
            {
                Debug.LogError("[AssaaPromptUI] messageText is NULL!");
            }

            // Show the panel
            if (promptPanel != null)
            {
                Debug.Log($"[AssaaPromptUI] Setting promptPanel to ACTIVE...");
                promptPanel.SetActive(true);
                Debug.Log($"[AssaaPromptUI] Panel state AFTER: {promptPanel.activeSelf}");
                Debug.Log($"[AssaaPromptUI] ✅ PANEL SHOULD BE VISIBLE NOW!");
            }
            else
            {
                Debug.LogError("[AssaaPromptUI] promptPanel is NULL! Cannot show UI!");
            }
            
            Debug.Log($"[AssaaPromptUI] Waiting for response from {currentPlayer.Name}...");
        }
        else
        {
            // AI Player - automatically respond
            bool aiWillUseAssaa = false;
            
            if (aiCanUseAssaa)
            {
                // AI randomly decides based on chance
                int roll = UnityEngine.Random.Range(0, 100);
                aiWillUseAssaa = roll < aiAssaaChance;
            }
            
            Debug.Log($"[AssaaPromptUI] AI player {currentPlayer?.Name} - auto-responding {(aiWillUseAssaa ? "YES (Assaa)" : "NO")}");
            
            // Send automatic response for AI
            AssaaResponseEvent evt_response = Pools.Claim<AssaaResponseEvent>();
            evt_response.UseAssaa = aiWillUseAssaa;
            evt_response.RespondingPlayer = currentPlayer;
            evt_response.PromptNumber = evt.PromptNumber;
            GameEventDispatcher.SendEvent(evt_response);
        }
    }

    /// <summary>
    /// Called when Yes button is clicked
    /// </summary>
    private void OnYesClicked()
    {
        Debug.Log($"[AssaaPromptUI] ========================================");
        Debug.Log($"[AssaaPromptUI] YES BUTTON CLICKED");
        Debug.Log($"[AssaaPromptUI] Waiting for response: {waitingForResponse}");
        Debug.Log($"[AssaaPromptUI] Current player: {currentPlayer?.Name}");
        Debug.Log($"[AssaaPromptUI] Current prompt number: {currentPromptNumber}");
        Debug.Log($"[AssaaPromptUI] ========================================");
        
        if (!waitingForResponse)
        {
            Debug.LogWarning("[AssaaPromptUI] YES clicked but not waiting for response - ignoring!");
            return;
        }

        if (currentPlayer == null)
        {
            Debug.LogError("[AssaaPromptUI] YES clicked but currentPlayer is null - ignoring!");
            return;
        }

        Debug.Log($"[AssaaPromptUI] Processing YES from {currentPlayer?.Name} (Prompt #{currentPromptNumber})");

        // Save current values BEFORE hiding
        Player respondingPlayer = currentPlayer;
        int promptNum = currentPromptNumber;
        
        Debug.Log($"[AssaaPromptUI] Hiding panel BEFORE sending event...");
        
        // Hide the panel FIRST (before sending event)
        HidePrompt();
        
        Debug.Log($"[AssaaPromptUI] Panel hidden, now sending response event...");

        // Send response event AFTER hiding
        AssaaResponseEvent evt = Pools.Claim<AssaaResponseEvent>();
        evt.UseAssaa = true;
        evt.RespondingPlayer = respondingPlayer;
        evt.PromptNumber = promptNum;
        GameEventDispatcher.SendEvent(evt);

        Debug.Log($"[AssaaPromptUI] Response event sent - done!");
    }

    /// <summary>
    /// Called when No button is clicked
    /// </summary>
    private void OnNoClicked()
    {
        Debug.Log($"[AssaaPromptUI] ========================================");
        Debug.Log($"[AssaaPromptUI] NO BUTTON CLICKED");
        Debug.Log($"[AssaaPromptUI] Waiting for response: {waitingForResponse}");
        Debug.Log($"[AssaaPromptUI] Current player: {currentPlayer?.Name}");
        Debug.Log($"[AssaaPromptUI] Current prompt number: {currentPromptNumber}");
        Debug.Log($"[AssaaPromptUI] ========================================");
        
        if (!waitingForResponse)
        {
            Debug.LogWarning("[AssaaPromptUI] NO clicked but not waiting for response - ignoring!");
            return;
        }

        if (currentPlayer == null)
        {
            Debug.LogError("[AssaaPromptUI] NO clicked but currentPlayer is null - ignoring!");
            return;
        }

        Debug.Log($"[AssaaPromptUI] Processing NO from {currentPlayer?.Name} (Prompt #{currentPromptNumber})");

        // Save current values BEFORE hiding
        Player respondingPlayer = currentPlayer;
        int promptNum = currentPromptNumber;
        
        Debug.Log($"[AssaaPromptUI] Hiding panel BEFORE sending event...");
        
        // Hide the panel FIRST (before sending event)
        HidePrompt();
        
        Debug.Log($"[AssaaPromptUI] Panel hidden, now sending response event...");

        // Send response event AFTER hiding
        AssaaResponseEvent evt = Pools.Claim<AssaaResponseEvent>();
        evt.UseAssaa = false;
        evt.RespondingPlayer = respondingPlayer;
        evt.PromptNumber = promptNum;
        GameEventDispatcher.SendEvent(evt);

        Debug.Log($"[AssaaPromptUI] Response event sent - done!");
    }

    /// <summary>
    /// Hide the prompt panel
    /// </summary>
    private void HidePrompt()
    {
        Debug.Log($"[AssaaPromptUI] ========================================");
        Debug.Log($"[AssaaPromptUI] HIDING PROMPT PANEL");
        Debug.Log($"[AssaaPromptUI] Current player was: {currentPlayer?.Name}");
        Debug.Log($"[AssaaPromptUI] Panel state BEFORE hide: {(promptPanel != null ? promptPanel.activeSelf.ToString() : "NULL")}");
        
        waitingForResponse = false;
        currentPlayer = null;
        currentPromptNumber = 0;

        if (promptPanel != null)
        {
            promptPanel.SetActive(false);
            Debug.Log($"[AssaaPromptUI] Panel hidden - state now: {promptPanel.activeSelf}");
        }
        else
        {
            Debug.LogError("[AssaaPromptUI] promptPanel is NULL during hide!");
        }
        
        Debug.Log($"[AssaaPromptUI] Panel hidden successfully");
        Debug.Log($"[AssaaPromptUI] ========================================");
    }

    /// <summary>
    /// Public method to show the prompt (can be called directly if needed)
    /// </summary>
    public void ShowPrompt(Player player, Player rassaChooser, int promptNumber)
    {
        AssaaPromptEvent evt = Pools.Claim<AssaaPromptEvent>();
        evt.AskingPlayer = player;
        evt.RassaChooser = rassaChooser;
        evt.PromptNumber = promptNumber;
        GameEventDispatcher.SendEvent(evt);
    }
}

