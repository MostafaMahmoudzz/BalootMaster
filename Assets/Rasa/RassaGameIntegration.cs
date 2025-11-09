using UnityEngine;
using Pebble;

/// <summary>
/// Integrates the Rassa system with the GameStage
/// Add this component to the same GameObject as BeloteGame
/// </summary>
[RequireComponent(typeof(BeloteGame))]
public class RassaGameIntegration : MonoBehaviour
{
    [Header("References")]
    public RassaDeckManager rassaDeckManager;
    public RassaPromptUI rassaPromptUI;

    [Header("Settings")]
    public bool enableRassaSystem = true;
    public bool askEveryRound = false; // If false, only ask on first round

    private BeloteGame beloteGame;
    private GameStage gameStage;
    private bool waitingForRassaResponse = false;
    private bool rassaChoiceMade = false;
    private bool useRassaForThisGame = false;

    private void Awake()
    {
        beloteGame = GetComponent<BeloteGame>();
        
        // Subscribe to events
        GameEventDispatcher.Subscribe<RassaResponseEvent>(OnRassaResponse);
        
        Debug.Log("[RassaGameIntegration] Initialized");
    }

    private void Start()
    {
        // Get the GameStage reference
        if (beloteGame != null)
        {
            gameStage = beloteGame.Stage;
        }

        // Validate references
        if (rassaDeckManager == null)
        {
            Debug.LogWarning("[RassaGameIntegration] RassaDeckManager not assigned! Looking for it...");
            rassaDeckManager = FindObjectOfType<RassaDeckManager>();
        }

        if (rassaPromptUI == null)
        {
            Debug.LogWarning("[RassaGameIntegration] RassaPromptUI not assigned! Looking for it...");
            rassaPromptUI = FindObjectOfType<RassaPromptUI>();
        }

        if (rassaDeckManager == null || rassaPromptUI == null)
        {
            Debug.LogError("[RassaGameIntegration] Missing required components! Rassa system will not work.");
            enableRassaSystem = false;
        }
    }

    private void OnDestroy()
    {
        GameEventDispatcher.UnSubscribe<RassaResponseEvent>(OnRassaResponse);
    }

    /// <summary>
    /// Call this method BEFORE DealCards in GameStage
    /// Returns true if should proceed with dealing, false if waiting for player response
    /// </summary>
    public bool CheckRassaBeforeDealing(Player roundFirstPlayer, int currentRound, BeloteDeck deck)
    {
        if (!enableRassaSystem)
        {
            Debug.Log("[RassaGameIntegration] Rassa system disabled, proceeding with normal dealing");
            return true; // Proceed normally
        }

        // Check if we should ask (first round only, or every round)
        bool shouldAsk = (currentRound == 1) || askEveryRound;
        
        if (!shouldAsk && rassaChoiceMade)
        {
            // Player already made choice in round 1, apply it
            if (useRassaForThisGame)
            {
                Debug.Log("[RassaGameIntegration] Using Rassa order (from previous choice)");
                ApplyRassaToDeck(deck);
            }
            return true; // Proceed with dealing
        }

        if (!shouldAsk)
        {
            return true; // Don't ask, just proceed normally
        }

        // Check if Rassa order exists
        if (!rassaDeckManager.HasSavedRassaOrder())
        {
            Debug.Log("[RassaGameIntegration] No saved Rassa order, proceeding with normal dealing");
            return true; // No Rassa saved, proceed normally
        }

        // If we're already waiting for response, don't ask again
        if (waitingForRassaResponse)
        {
            Debug.Log("[RassaGameIntegration] Still waiting for Rassa response...");
            return false; // Don't proceed yet
        }

        // Ask the player
        Debug.Log($"[RassaGameIntegration] Asking {roundFirstPlayer?.Name} about using Rassa");
        waitingForRassaResponse = true;
        
        RassaPromptEvent evt = Pools.Claim<RassaPromptEvent>();
        evt.AskingPlayer = roundFirstPlayer;
        evt.RoundNumber = currentRound;
        GameEventDispatcher.SendEvent(evt);
        
        return false; // Don't proceed yet, wait for response
    }

    /// <summary>
    /// Called when player responds to Rassa prompt
    /// </summary>
    private void OnRassaResponse(RassaResponseEvent evt)
    {
        if (!waitingForRassaResponse)
        {
            Debug.LogWarning("[RassaGameIntegration] Received unexpected Rassa response");
            return;
        }

        waitingForRassaResponse = false;
        rassaChoiceMade = true;
        useRassaForThisGame = evt.UseRassa;

        Debug.Log($"[RassaGameIntegration] Player {evt.RespondingPlayer?.Name} chose: {(evt.UseRassa ? "Use Rassa" : "Random deck")}");

        // Apply Rassa if chosen
        if (evt.UseRassa && gameStage != null)
        {
            // We need to arrange the deck before dealing
            // This will be done in the next frame by GameStage
            Debug.Log("[RassaGameIntegration] Rassa will be applied to deck");
        }

        // Notify GameStage to continue (via custom event or direct call)
        RassaChoiceCompleteEvent choiceEvt = Pools.Claim<RassaChoiceCompleteEvent>();
        choiceEvt.UseRassa = evt.UseRassa;
        GameEventDispatcher.SendEvent(choiceEvt);
    }

    /// <summary>
    /// Apply the Rassa order to the deck
    /// </summary>
    public bool ApplyRassaToDeck(BeloteDeck deck)
    {
        if (rassaDeckManager == null)
        {
            Debug.LogError("[RassaGameIntegration] Cannot apply Rassa - RassaDeckManager is null!");
            return false;
        }

        Debug.Log("[RassaGameIntegration] Applying Rassa order to deck...");
        bool success = rassaDeckManager.ArrangeDeckWithRassaOrder(deck);

        if (success)
        {
            Debug.Log("[RassaGameIntegration] ✅ Deck arranged with Rassa order successfully!");
            RassaDeckArrangedEvent evt = Pools.Claim<RassaDeckArrangedEvent>();
            evt.Success = true;
            GameEventDispatcher.SendEvent(evt);
        }
        else
        {
            Debug.LogError("[RassaGameIntegration] ❌ Failed to arrange deck with Rassa order!");
            RassaDeckArrangedEvent evt = Pools.Claim<RassaDeckArrangedEvent>();
            evt.Success = false;
            GameEventDispatcher.SendEvent(evt);
        }

        return success;
    }

    /// <summary>
    /// Reset for a new game
    /// </summary>
    public void ResetForNewGame()
    {
        rassaChoiceMade = false;
        useRassaForThisGame = false;
        waitingForRassaResponse = false;
        Debug.Log("[RassaGameIntegration] Reset for new game");
    }

    /// <summary>
    /// Check if currently waiting for Rassa response
    /// </summary>
    public bool IsWaitingForResponse()
    {
        return waitingForRassaResponse;
    }
}

/// <summary>
/// Event sent when the player has made their Rassa choice
/// </summary>
public class RassaChoiceCompleteEvent : PooledEvent
{
    public bool UseRassa { get; set; }

    public override void Reset()
    {
        UseRassa = false;
    }
}

