using UnityEngine;
using Pebble;
using System.Collections.Generic;

/// <summary>
/// Main controller for the Assaa system
/// Manages the flow: ask right player → ask teammate → card reordering
/// </summary>
public class AssaaSystem : MonoBehaviour
{
    [Header("References")]
    public AssaaPromptUI assaaPromptUI;
    public AssaaCardReorderUI assaaCardReorderUI;

    [Header("Settings")]
    public bool enableAssaaSystem = true;

    private bool waitingForAssaaPrompt = false;
    private bool waitingForCardReorder = false;
    private int currentPromptNumber = 0;  // 1 = right player, 2 = teammate
    private Player rassaChooser;
    private Player rightPlayer;
    private Player teammate;
    private BeloteDeck currentDeck;
    private GameStage gameStage;

    private void Awake()
    {
        // Subscribe to events
        GameEventDispatcher.Subscribe<AssaaResponseEvent>(OnAssaaResponse);
        GameEventDispatcher.Subscribe<AssaaReorderCompleteEvent>(OnAssaaReorderComplete);

        Debug.Log("[AssaaSystem] Initialized");
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        GameEventDispatcher.UnSubscribe<AssaaResponseEvent>(OnAssaaResponse);
        GameEventDispatcher.UnSubscribe<AssaaReorderCompleteEvent>(OnAssaaReorderComplete);
    }

    /// <summary>
    /// Start the Assaa process after Rassa was chosen
    /// </summary>
    public void StartAssaaProcess(Player rassaChooserPlayer, BeloteDeck deck, GameStage stage)
    {
        if (!enableAssaaSystem)
        {
            Debug.Log("[AssaaSystem] Assaa system disabled, skipping");
            SendAssaaComplete(false);
            return;
        }

        Debug.Log($"[AssaaSystem] === STARTING ASSAA PROCESS ===");
        Debug.Log($"[AssaaSystem] Rassa chosen by: {rassaChooserPlayer?.Name}");

        rassaChooser = rassaChooserPlayer;
        currentDeck = deck;
        gameStage = stage;

        // Find the player to the right of Rassa chooser
        rightPlayer = GetRightPlayer(rassaChooserPlayer);
        
        // Find the teammate of the right player
        teammate = GetTeammate(rightPlayer);

        Debug.Log($"[AssaaSystem] Right player: {rightPlayer?.Name}");
        Debug.Log($"[AssaaSystem] Right player's teammate: {teammate?.Name}");

        // Start by asking the right player
        AskRightPlayer();
    }

    /// <summary>
    /// Ask the player to the right of Rassa chooser
    /// </summary>
    private void AskRightPlayer()
    {
        Debug.Log($"[AssaaSystem] Asking right player: {rightPlayer?.Name}");
        currentPromptNumber = 1;
        waitingForAssaaPrompt = true;

        // Send prompt event
        AssaaPromptEvent evt = Pools.Claim<AssaaPromptEvent>();
        evt.AskingPlayer = rightPlayer;
        evt.RassaChooser = rassaChooser;
        evt.PromptNumber = 1;
        GameEventDispatcher.SendEvent(evt);
    }

    /// <summary>
    /// Ask the teammate of the right player
    /// </summary>
    private void AskTeammate()
    {
        if (teammate == null)
        {
            Debug.LogError("[AssaaSystem] Cannot ask teammate - teammate is null!");
            SendAssaaComplete(false);
            return;
        }

        Debug.Log($"[AssaaSystem] ========================================");
        Debug.Log($"[AssaaSystem] ASKING TEAMMATE NOW!");
        Debug.Log($"[AssaaSystem] Teammate: {teammate?.Name} ({teammate?.Position})");
        Debug.Log($"[AssaaSystem] Right player was: {rightPlayer?.Name} ({rightPlayer?.Position})");
        Debug.Log($"[AssaaSystem] Sending AssaaPromptEvent for teammate...");
        Debug.Log($"[AssaaSystem] ========================================");
        
        currentPromptNumber = 2;
        waitingForAssaaPrompt = true;

        // Send prompt event
        AssaaPromptEvent evt = Pools.Claim<AssaaPromptEvent>();
        evt.AskingPlayer = teammate;
        evt.RassaChooser = rassaChooser;
        evt.PromptNumber = 2;
        GameEventDispatcher.SendEvent(evt);
        
        Debug.Log($"[AssaaSystem] AssaaPromptEvent sent for {teammate?.Name} - waiting for response");
    }

    /// <summary>
    /// Called when a player responds to Assaa prompt
    /// </summary>
    private void OnAssaaResponse(AssaaResponseEvent evt)
    {
        Debug.Log($"[AssaaSystem] ========================================");
        Debug.Log($"[AssaaSystem] RECEIVED ASSAA RESPONSE");
        Debug.Log($"[AssaaSystem] Player: {evt.RespondingPlayer?.Name}");
        Debug.Log($"[AssaaSystem] Response: {(evt.UseAssaa ? "YES" : "NO")}");
        Debug.Log($"[AssaaSystem] Prompt Number: {evt.PromptNumber}");
        Debug.Log($"[AssaaSystem] Waiting for prompt: {waitingForAssaaPrompt}");
        Debug.Log($"[AssaaSystem] ========================================");
        
        if (!waitingForAssaaPrompt)
        {
            Debug.LogWarning("[AssaaSystem] Received unexpected Assaa response - not waiting!");
            return;
        }

        waitingForAssaaPrompt = false;

        Debug.Log($"[AssaaSystem] Processing response from {evt.RespondingPlayer?.Name}: {(evt.UseAssaa ? "YES" : "NO")}");

        if (evt.UseAssaa)
        {
            // Player said YES - start card reordering
            Debug.Log($"[AssaaSystem] {evt.RespondingPlayer?.Name} chose YES - starting card reorder UI");
            StartCardReordering(evt.RespondingPlayer);
        }
        else
        {
            // Player said NO
            if (evt.PromptNumber == 1)
            {
                // First player (right) said NO - ask teammate
                Debug.Log($"[AssaaSystem] ========================================");
                Debug.Log($"[AssaaSystem] RIGHT PLAYER SAID NO - WILL ASK TEAMMATE");
                Debug.Log($"[AssaaSystem] Right player: {rightPlayer?.Name}");
                Debug.Log($"[AssaaSystem] Teammate to ask: {teammate?.Name}");
                Debug.Log($"[AssaaSystem] ========================================");
                AskTeammate();
            }
            else if (evt.PromptNumber == 2)
            {
                // Teammate also said NO - Assaa not used, continue normal bidding
                Debug.Log("[AssaaSystem] Teammate also said NO - Both players declined Assaa");
                SendAssaaComplete(false);
            }
            else
            {
                // Unknown prompt number - safety fallback
                Debug.LogError($"[AssaaSystem] Unknown prompt number: {evt.PromptNumber} - ending Assaa process");
                SendAssaaComplete(false);
            }
        }
    }

    /// <summary>
    /// Start the card reordering UI
    /// </summary>
    private void StartCardReordering(Player reorderingPlayer)
    {
        Debug.Log($"[AssaaSystem] Showing card reorder UI for {reorderingPlayer?.Name}");
        waitingForCardReorder = true;

        // Send event to show card reorder UI
        AssaaReorderPromptEvent evt = Pools.Claim<AssaaReorderPromptEvent>();
        evt.ReorderingPlayer = reorderingPlayer;
        evt.Deck = currentDeck;
        GameEventDispatcher.SendEvent(evt);
    }

    /// <summary>
    /// Called when card reordering is complete
    /// </summary>
    private void OnAssaaReorderComplete(AssaaReorderCompleteEvent evt)
    {
        if (!waitingForCardReorder)
        {
            Debug.LogWarning("[AssaaSystem] Received unexpected Assaa reorder complete event");
            return;
        }

        waitingForCardReorder = false;

        if (evt.Success)
        {
            Debug.Log($"[AssaaSystem] ✅ Card reordering complete by {evt.ReorderingPlayer?.Name}");
            SendAssaaComplete(true);
        }
        else
        {
            Debug.Log($"[AssaaSystem] Card reordering cancelled by {evt.ReorderingPlayer?.Name}");
            SendAssaaComplete(false);
        }
    }

    /// <summary>
    /// Send event that Assaa process is complete
    /// </summary>
    private void SendAssaaComplete(bool assaaWasUsed)
    {
        Debug.Log($"[AssaaSystem] === ASSAA PROCESS COMPLETE === (Used: {assaaWasUsed})");
        Debug.Log($"[AssaaSystem] Sending AssaaProcessCompleteEvent to notify game to continue");

        // Reset state
        waitingForAssaaPrompt = false;
        waitingForCardReorder = false;
        currentPromptNumber = 0;
        rassaChooser = null;
        rightPlayer = null;
        teammate = null;
        currentDeck = null;

        // Send completion event
        AssaaProcessCompleteEvent evt = Pools.Claim<AssaaProcessCompleteEvent>();
        evt.AssaaWasUsed = assaaWasUsed;
        GameEventDispatcher.SendEvent(evt);
        
        Debug.Log($"[AssaaSystem] AssaaProcessCompleteEvent sent - game should continue now");
    }

    /// <summary>
    /// Get the player to the right (anti-clockwise) of the given player
    /// </summary>
    private Player GetRightPlayer(Player player)
    {
        if (gameStage == null || gameStage.Players == null || gameStage.Players.Count == 0)
        {
            Debug.LogError("[AssaaSystem] Cannot get right player - GameStage or Players is null!");
            return null;
        }

        return gameStage.GetRightPlayer(player);
    }

    /// <summary>
    /// Get the teammate of the given player
    /// In Baloot, teammates are opposite each other (2 positions away)
    /// Team1: South (0) + North (2)
    /// Team2: West (1) + East (3)
    /// </summary>
    private Player GetTeammate(Player player)
    {
        if (gameStage == null || gameStage.Players == null || gameStage.Players.Count != 4)
        {
            Debug.LogError("[AssaaSystem] Cannot get teammate - invalid player count!");
            return null;
        }

        if (player == null)
        {
            Debug.LogError("[AssaaSystem] Cannot get teammate - player is null!");
            return null;
        }

        // Find all players with the same team
        foreach (Player p in gameStage.Players)
        {
            if (p != player && p.Team == player.Team)
            {
                return p;
            }
        }

        Debug.LogError($"[AssaaSystem] Could not find teammate for {player.Name}!");
        return null;
    }

    /// <summary>
    /// Check if currently waiting for Assaa response
    /// </summary>
    public bool IsWaitingForResponse()
    {
        return waitingForAssaaPrompt || waitingForCardReorder;
    }

    /// <summary>
    /// Reset for new game
    /// </summary>
    public void Reset()
    {
        waitingForAssaaPrompt = false;
        waitingForCardReorder = false;
        currentPromptNumber = 0;
        rassaChooser = null;
        rightPlayer = null;
        teammate = null;
        currentDeck = null;
        Debug.Log("[AssaaSystem] Reset");
    }
}

