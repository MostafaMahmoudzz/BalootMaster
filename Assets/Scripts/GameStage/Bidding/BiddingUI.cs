using System;
using System.Collections.Generic;
using UnityEngine;
using Pebble;
using TMPro;

//-------------------------------------------------------
// BiddingUI
//-------------------------------------------------------
// Purpose:
//   Handles the user interface for the bidding round.
//   Shows bidding options, current highest bid, and player turns.
//
// How it connects to other scripts:
//   - Subscribes to bidding events to update UI
//   - Integrates with GameStage for bid submission
//   - Provides visual feedback for bidding process
//
// ⚠️ CRITICAL DESIGN PRINCIPLE:
//   The BelootBiddingSystem is the SINGLE SOURCE OF TRUTH for:
//   - Current Bidder (m_stage.BiddingSystem.CurrentBidder)
//   - First Bidder (m_stage.RoundFirstPlayer)
//   - Dealer (m_stage.Dealer)
//   
//   ALWAYS read these values directly from the system, NOT from cached
//   event variables like m_currentBidder. Cached variables are only for
//   fallback display purposes and should NOT drive game logic.
//-------------------------------------------------------
public class BiddingUI : MonoBehaviour
{
    //----------------------------------------------
    // Variables
    private GameStage m_stage;                    // Reference to game stage
    private bool m_isBiddingActive;               // Is bidding currently active?
    
    // ⚠️ WARNING: These cached values are for DISPLAY ONLY (fallback GUI)
    // DO NOT use these for game logic! Always read from m_stage.BiddingSystem directly.
    private Player m_currentBidder;               // CACHED for display - DO NOT USE FOR LOGIC
    private Bid m_highestBid;                     // CACHED for display - DO NOT USE FOR LOGIC
    
    private bool m_showBiddingUI;                 // Should UI be visible?
    private BelootBiddingSystem.BiddingRound m_currentBiddingRound; // Track current round for fallback GUI
    private BeloteCard m_faceUpCard;              // Track face-up card for fallback GUI
    private bool m_anotherTrumpChosen;            // Track if someone chose "Another Trump" in Round 2
    private bool m_trumpChosen;                   // Track if someone chose "Trump" in Round 1
    private bool m_ignoreBiddingTurnEvents;       // Flag to ignore BiddingTurnEvent during transitions
    private bool m_preventBidSubmission;          // Flag to prevent bid submission during transitions
    private bool m_inMultiplierBidding;           // Are we in multiplier bidding phase?
    private int m_currentMultiplier;              // Current multiplier (1, 2, 3, or 4)
    private Player m_trumpConfirmer;              // Player who confirmed trump
    private bool m_isOpposingTeamTurn;            // Is it the opposing team's turn?
    
    // UI Elements (these would be assigned in Unity Inspector)
    public GameObject biddingPanel;               // Main bidding panel
    public UnityEngine.UI.Button passButton;      // Pass button
    public UnityEngine.UI.Button sunButton;        // Sun bid button
    public UnityEngine.UI.Button clubsButton;     // Clubs bid button
    public UnityEngine.UI.Button diamondsButton;  // Diamonds bid button
    public UnityEngine.UI.Button heartsButton;    // Hearts bid button
    public UnityEngine.UI.Button spadesButton;    // Spades bid button
    public TMP_Text currentBidderText; // Current bidder display
    public TMP_Text highestBidText;    // Highest bid display
    public TMP_Text biddingInstructions; // Instructions text
    public TMP_Text faceUpCardText;    // Face-up card display
    public TMP_Text roundText;         // Current round display
    
    // Visual Marker for Current Bidder
    [Header("Current Bidder Marker")]
    [UnityEngine.Tooltip("Visual marker that shows which player is currently bidding (e.g., an arrow, icon, or highlight)")]
    public GameObject currentBidderMarker;        // Visual marker for current bidder position

    //----------------------------------------------
    // Properties
    public bool IsBiddingActive
    {
        get { return m_isBiddingActive; }
    }

    //----------------------------------------------
    // Methods
    void Start()
    {
        // Find GameStage reference through BeloteGame component
        BeloteGame beloteGame = FindObjectOfType<BeloteGame>();
        if (beloteGame != null)
        {
            m_stage = beloteGame.Stage;
        }
        
        // CRITICAL: Check if UI elements are assigned
        Debug.LogWarning($"[BiddingUI] Checking UI elements:");
        Debug.LogWarning($"  - currentBidderText: {(currentBidderText != null ? "✅ ASSIGNED" : "❌ NULL")}");
        Debug.LogWarning($"  - biddingPanel: {(biddingPanel != null ? "✅ ASSIGNED" : "❌ NULL")}");
        Debug.LogWarning($"  - roundText: {(roundText != null ? "✅ ASSIGNED" : "❌ NULL")}");
        
        // Subscribe to bidding events
        GameEventDispatcher.Subscribe<BiddingStartEvent>(this.OnBiddingStart);
        GameEventDispatcher.Subscribe<BiddingCompleteEvent>(this.OnBiddingComplete);
        GameEventDispatcher.Subscribe<BiddingTurnEvent>(this.OnBiddingTurn);
        GameEventDispatcher.Subscribe<BidSubmittedEvent>(this.OnBidSubmitted);
        GameEventDispatcher.Subscribe<BiddingRound2StartEvent>(this.OnBiddingRound2Start);
        GameEventDispatcher.Subscribe<BiddingTurnEventIgnoreEvent>(this.OnBiddingTurnEventIgnore);
        GameEventDispatcher.Subscribe<BiddingEventSubscriptionEvent>(this.OnBiddingEventSubscription);
        GameEventDispatcher.Subscribe<MultiplierBiddingStartEvent>(this.OnMultiplierBiddingStart);
        GameEventDispatcher.Subscribe<MultiplierBiddingTurnEvent>(this.OnMultiplierBiddingTurn);

        // Setup button listeners
        SetupButtonListeners();

        // Hide UI initially
        HideBiddingUI();
        
        // Hide marker initially
        if (currentBidderMarker != null)
        {
            currentBidderMarker.SetActive(false);
        }
    }

    //----------------------------------------------
    void Update()
    {
        // بس نحدث الـ UI كل frame
        if (m_isBiddingActive && m_showBiddingUI)
        {
            RefreshCurrentBidderDisplay();
            UpdateBidderMarker(); // Keep marker updated with current bidder
        }
    }
    
    //----------------------------------------------
    void LateUpdate()
    {
        // تأكد تاني في آخر الـ frame إن الـ UI صح
        if (m_isBiddingActive && m_showBiddingUI)
        {
            RefreshCurrentBidderDisplay();
            UpdateBidderMarker(); // Keep marker updated with current bidder
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        GameEventDispatcher.UnSubscribe<BiddingStartEvent>(this.OnBiddingStart);
        GameEventDispatcher.UnSubscribe<BiddingCompleteEvent>(this.OnBiddingComplete);
        GameEventDispatcher.UnSubscribe<BiddingTurnEvent>(this.OnBiddingTurn);
        GameEventDispatcher.UnSubscribe<BidSubmittedEvent>(this.OnBidSubmitted);
        GameEventDispatcher.UnSubscribe<BiddingRound2StartEvent>(this.OnBiddingRound2Start);
        GameEventDispatcher.UnSubscribe<BiddingTurnEventIgnoreEvent>(this.OnBiddingTurnEventIgnore);
        GameEventDispatcher.UnSubscribe<BiddingEventSubscriptionEvent>(this.OnBiddingEventSubscription);
        GameEventDispatcher.UnSubscribe<MultiplierBiddingStartEvent>(this.OnMultiplierBiddingStart);
        GameEventDispatcher.UnSubscribe<MultiplierBiddingTurnEvent>(this.OnMultiplierBiddingTurn);
    }

    //----------------------------------------------
    void SetupButtonListeners()
    {
        if (passButton != null)
            passButton.onClick.AddListener(() => SubmitBid(Bid.CreatePass()));

        if (sunButton != null)
            sunButton.onClick.AddListener(() => SubmitBid(Bid.CreateSun()));

        if (clubsButton != null)
            clubsButton.onClick.AddListener(() => SubmitBid(Bid.CreateTrump(Card32Family.Clubs)));

        if (diamondsButton != null)
            diamondsButton.onClick.AddListener(() => SubmitBid(Bid.CreateTrump(Card32Family.Diamond)));

        if (heartsButton != null)
            heartsButton.onClick.AddListener(() => SubmitBid(Bid.CreateTrump(Card32Family.Heart)));

        if (spadesButton != null)
            spadesButton.onClick.AddListener(() => SubmitBid(Bid.CreateTrump(Card32Family.Spade)));
    }

    //----------------------------------------------
    void OnBiddingStart(BiddingStartEvent evt)
    {
        Debug.Log($"[FIRST BIDDER DEBUG] BiddingUI OnBiddingStart - CurrentBidder: {evt.CurrentBidder?.Name}, Round: {evt.Round}");
        
        m_isBiddingActive = true;
        
        // Cache the event values (fallback only)
        m_currentBidder = evt.CurrentBidder;
        m_highestBid = evt.HighestBid;
        m_currentBiddingRound = evt.Round;
        m_faceUpCard = evt.FaceUpCard;
        m_anotherTrumpChosen = false;
        m_trumpChosen = false;
        
        // Clear the ignore flags when new bidding starts
        m_ignoreBiddingTurnEvents = false;
        m_preventBidSubmission = false;
        
        // Reset multiplier bidding state for new round
        m_inMultiplierBidding = false;
        m_currentMultiplier = 1;
        m_trumpConfirmer = null;
        m_isOpposingTeamTurn = false;
        
        Debug.Log($"[FIRST BIDDER DEBUG] BiddingUI cached m_currentBidder: {m_currentBidder?.Name}");
        
        // CRITICAL: Verify against system value
        if (m_stage != null && m_stage.BiddingSystem != null && m_stage.BiddingSystem.CurrentBidder != null)
        {
            Debug.LogWarning($"[VERIFY] BiddingStart Event says: {evt.CurrentBidder?.Name}, System says: {m_stage.BiddingSystem.CurrentBidder?.Name}");
            if (evt.CurrentBidder != m_stage.BiddingSystem.CurrentBidder)
            {
                Debug.LogError($"[BUG DETECTED] BiddingStart event bidder ({evt.CurrentBidder?.Name}) DOES NOT MATCH system bidder ({m_stage.BiddingSystem.CurrentBidder?.Name})!");
            }
        }
        
        ShowBiddingUI();
        UpdateBiddingDisplay(evt.Round, evt.FaceUpCard);
        UpdateBidderMarker(); // Update marker to show current bidder
    }

    //----------------------------------------------
    public void SetIgnoreBiddingTurnEvents(bool ignore)
    {
        m_ignoreBiddingTurnEvents = ignore;
        m_preventBidSubmission = ignore; // Also prevent bid submission during transitions
        Debug.Log($"[FIRST BIDDER DEBUG] BiddingUI SetIgnoreBiddingTurnEvents: {ignore}");
    }

    //----------------------------------------------
    public void UnsubscribeFromBiddingEvents()
    {
        Debug.Log("[FIRST BIDDER DEBUG] BiddingUI Unsubscribing from all bidding events (keeping subscription control event)");
        GameEventDispatcher.UnSubscribe<BiddingStartEvent>(this.OnBiddingStart);
        GameEventDispatcher.UnSubscribe<BiddingCompleteEvent>(this.OnBiddingComplete);
        GameEventDispatcher.UnSubscribe<BiddingTurnEvent>(this.OnBiddingTurn);
        GameEventDispatcher.UnSubscribe<BidSubmittedEvent>(this.OnBidSubmitted);
        GameEventDispatcher.UnSubscribe<BiddingRound2StartEvent>(this.OnBiddingRound2Start);
        GameEventDispatcher.UnSubscribe<BiddingTurnEventIgnoreEvent>(this.OnBiddingTurnEventIgnore);
        GameEventDispatcher.UnSubscribe<MultiplierBiddingStartEvent>(this.OnMultiplierBiddingStart);
        GameEventDispatcher.UnSubscribe<MultiplierBiddingTurnEvent>(this.OnMultiplierBiddingTurn);
        // Note: We keep subscribed to BiddingEventSubscriptionEvent to allow re-subscription
    }

    //----------------------------------------------
    public void SubscribeToBiddingEvents()
    {
        Debug.Log("[FIRST BIDDER DEBUG] BiddingUI Subscribing to all bidding events");
        GameEventDispatcher.Subscribe<BiddingStartEvent>(this.OnBiddingStart);
        GameEventDispatcher.Subscribe<BiddingCompleteEvent>(this.OnBiddingComplete);
        GameEventDispatcher.Subscribe<BiddingTurnEvent>(this.OnBiddingTurn);
        GameEventDispatcher.Subscribe<BidSubmittedEvent>(this.OnBidSubmitted);
        GameEventDispatcher.Subscribe<BiddingRound2StartEvent>(this.OnBiddingRound2Start);
        GameEventDispatcher.Subscribe<BiddingTurnEventIgnoreEvent>(this.OnBiddingTurnEventIgnore);
        GameEventDispatcher.Subscribe<MultiplierBiddingStartEvent>(this.OnMultiplierBiddingStart);
        GameEventDispatcher.Subscribe<MultiplierBiddingTurnEvent>(this.OnMultiplierBiddingTurn);
    }

    //----------------------------------------------
    void OnBiddingTurnEventIgnore(BiddingTurnEventIgnoreEvent evt)
    {
        SetIgnoreBiddingTurnEvents(evt.Ignore);
    }

    //----------------------------------------------
    void OnBiddingEventSubscription(BiddingEventSubscriptionEvent evt)
    {
        if (evt.Subscribe)
        {
            SubscribeToBiddingEvents();
        }
        else
        {
            UnsubscribeFromBiddingEvents();
        }
    }

    //----------------------------------------------
    void OnBiddingComplete(BiddingCompleteEvent evt)
    {
        m_isBiddingActive = false;
        HideBiddingUI();
        
        // Hide the marker when bidding is complete
        if (currentBidderMarker != null)
        {
            currentBidderMarker.SetActive(false);
        }
    }

    //----------------------------------------------
    void OnBiddingTurn(BiddingTurnEvent evt)
    {
        Debug.Log($"[FIRST BIDDER DEBUG] BiddingUI OnBiddingTurn - CurrentBidder: {evt.CurrentBidder?.Name}, Round: {evt.Round}");
        
        // Check if we should ignore this event during transitions
        if (m_ignoreBiddingTurnEvents)
        {
            Debug.Log($"[FIRST BIDDER DEBUG] BiddingUI ignoring BiddingTurnEvent due to transition flag");
            return;
        }
        
        // Cache the event values (fallback only)
        m_currentBidder = evt.CurrentBidder;
        m_highestBid = evt.HighestBid;
        m_currentBiddingRound = evt.Round;
        
        Debug.Log($"[FIRST BIDDER DEBUG] BiddingUI cached m_currentBidder: {m_currentBidder?.Name}");
        
        // CRITICAL: Verify against system value
        if (m_stage != null && m_stage.BiddingSystem != null && m_stage.BiddingSystem.CurrentBidder != null)
        {
            Debug.LogWarning($"[VERIFY] Event says: {evt.CurrentBidder?.Name}, System says: {m_stage.BiddingSystem.CurrentBidder?.Name}");
            if (evt.CurrentBidder != m_stage.BiddingSystem.CurrentBidder)
            {
                Debug.LogError($"[BUG DETECTED] Event bidder ({evt.CurrentBidder?.Name}) DOES NOT MATCH system bidder ({m_stage.BiddingSystem.CurrentBidder?.Name})!");
            }
        }
        
        UpdateBiddingDisplay(evt.Round, null);
        UpdateBidderMarker(); // Update marker to show current bidder
    }

    //----------------------------------------------
    void OnBiddingRound2Start(BiddingRound2StartEvent evt)
    {
        Debug.Log($"[FIRST BIDDER DEBUG] BiddingUI OnBiddingRound2Start - CurrentBidder: {evt.CurrentBidder?.Name}, Trump taker: {evt.TrumpTaker?.Name}");
        
        // Cache the event values (fallback only)
        m_currentBidder = evt.CurrentBidder;
        m_currentBiddingRound = BelootBiddingSystem.BiddingRound.BiddingRound2;
        m_faceUpCard = evt.FaceUpCard;
        m_anotherTrumpChosen = false;
        
        Debug.Log($"[FIRST BIDDER DEBUG] BiddingUI cached m_currentBidder: {m_currentBidder?.Name}");
        
        // CRITICAL: Verify against system value
        if (m_stage != null && m_stage.BiddingSystem != null && m_stage.BiddingSystem.CurrentBidder != null)
        {
            Debug.LogWarning($"[VERIFY] Event says: {evt.CurrentBidder?.Name}, System says: {m_stage.BiddingSystem.CurrentBidder?.Name}");
            if (evt.CurrentBidder != m_stage.BiddingSystem.CurrentBidder)
            {
                Debug.LogError($"[BUG DETECTED] Round 2 event bidder ({evt.CurrentBidder?.Name}) DOES NOT MATCH system bidder ({m_stage.BiddingSystem.CurrentBidder?.Name})!");
            }
        }
        
        ShowBiddingUI();
        UpdateBiddingDisplay(BelootBiddingSystem.BiddingRound.BiddingRound2, evt.FaceUpCard);
        UpdateBidderMarker(); // Update marker to show current bidder
    }

    //----------------------------------------------
    void OnBidSubmitted(BidSubmittedEvent evt)
    {
        // Update display when any player submits a bid
        if (m_highestBid == null || evt.Bid.IsHigherThan(m_highestBid))
        {
            m_highestBid = evt.Bid;
        }
        
        UpdateBiddingDisplay(m_currentBiddingRound, m_faceUpCard);
    }

    //----------------------------------------------
    void OnMultiplierBiddingStart(MultiplierBiddingStartEvent evt)
    {
        Debug.Log($"[BiddingUI] OnMultiplierBiddingStart - CurrentBidder: {evt.CurrentBidder?.Name}, Multiplier: {evt.CurrentMultiplier}");
        
        m_inMultiplierBidding = true;
        m_currentMultiplier = evt.CurrentMultiplier;
        m_trumpConfirmer = evt.TrumpConfirmer;
        m_isOpposingTeamTurn = evt.IsOpposingTeamTurn;
        m_currentBidder = evt.CurrentBidder;
        m_currentBiddingRound = BelootBiddingSystem.BiddingRound.MultiplierBidding;
        
        UpdateBiddingDisplay(m_currentBiddingRound, m_faceUpCard);
        UpdateBidderMarker();
    }

    //----------------------------------------------
    void OnMultiplierBiddingTurn(MultiplierBiddingTurnEvent evt)
    {
        Debug.Log($"[BiddingUI] OnMultiplierBiddingTurn - CurrentBidder: {evt.CurrentBidder?.Name}, Multiplier: {evt.CurrentMultiplier}");
        
        m_currentMultiplier = evt.CurrentMultiplier;
        m_isOpposingTeamTurn = evt.IsOpposingTeamTurn;
        m_currentBidder = evt.CurrentBidder;
        
        UpdateBiddingDisplay(m_currentBiddingRound, m_faceUpCard);
        UpdateBidderMarker();
    }

    //----------------------------------------------
    void ShowBiddingUI()
    {
        Debug.Log("[BiddingUI] ShowBiddingUI() called");
        m_showBiddingUI = true;
        if (biddingPanel != null)
        {
            biddingPanel.SetActive(true);
            Debug.Log("[BiddingUI] BiddingPanel activated (SetActive(true))");
        }
        else
        {
            Debug.LogWarning("[BiddingUI] BiddingPanel is null! Cannot show UI.");
        }
    }

    //----------------------------------------------
    void HideBiddingUI()
    {
        Debug.Log("[BiddingUI] HideBiddingUI() called");
        m_showBiddingUI = false;
        if (biddingPanel != null)
        {
            biddingPanel.SetActive(false);
            Debug.Log("[BiddingUI] BiddingPanel deactivated (SetActive(false))");
        }
        else
        {
            Debug.LogWarning("[BiddingUI] BiddingPanel is null!");
        }
    }

    //----------------------------------------------
    void RefreshCurrentBidderDisplay()
    {
        // ببساطة: خلي الـ text = القيمة الصحيحة من الـ system
        if (currentBidderText == null)
        {
            Debug.LogWarning("[BiddingUI] currentBidderText is NULL! Can't update display!");
            return;
        }

        if (m_stage?.BiddingSystem?.CurrentBidder != null)
        {
            // استخدم القيمة الصحيحة من الـ system مباشرة
            currentBidderText.text = $"Current Bidder: {m_stage.BiddingSystem.CurrentBidder.Name}";
        }
        else
        {
            currentBidderText.text = "Current Bidder: None";
        }
    }

    //----------------------------------------------
    void UpdateBiddingDisplay(BelootBiddingSystem.BiddingRound biddingRound, BeloteCard faceUpCard)
    {
        if (!m_showBiddingUI)
            return;

        // Keep internal state in sync for fallback GUI
        m_currentBiddingRound = biddingRound;
        if (faceUpCard != null)
        {
            m_faceUpCard = faceUpCard;
        }

        // Debug: show current bidding round in console
        string roundName = m_currentBiddingRound == BelootBiddingSystem.BiddingRound.BiddingRound1 ? "1" : 
                          m_currentBiddingRound == BelootBiddingSystem.BiddingRound.BiddingRound2 ? "2" : "Multiplier";
        Debug.Log($"[BiddingUI] Current Bidding Round: {roundName}");

        // Update round text
        if (roundText != null)
        {
            string roundDisplayText = biddingRound == BelootBiddingSystem.BiddingRound.BiddingRound1 ? "1" : 
                                     biddingRound == BelootBiddingSystem.BiddingRound.BiddingRound2 ? "2" : "Multiplier";
            roundText.text = $"Round: {roundDisplayText}";
        }

        // Update face-up card text
        if (faceUpCardText != null && faceUpCard != null)
        {
            faceUpCardText.text = $"Face-up Card: {faceUpCard.Family}";
        }

        // FORCE UPDATE: Always refresh current bidder from system
        RefreshCurrentBidderDisplay();

        // Update highest bid text
        if (highestBidText != null)
        {
            if (m_highestBid == null || m_highestBid.IsPass)
            {
                highestBidText.text = "Highest Bid: None";
            }
            else
            {
                highestBidText.text = $"Highest Bid: {m_highestBid.DisplayName}";
            }
        }

        // Update instructions - ALWAYS read from system, not cached value
        if (biddingInstructions != null)
        {
            Player systemCurrentBidder = m_stage?.BiddingSystem?.CurrentBidder;
            if (systemCurrentBidder != null)
            {
                biddingInstructions.text = $"{systemCurrentBidder.Name}, choose your bid:";
            }
            else
            {
                biddingInstructions.text = "Bidding in progress...";
            }
        }

        // Enable/disable buttons - ALWAYS read from system, not cached value
        Player currentBidderFromSystem = m_stage?.BiddingSystem?.CurrentBidder;
        bool isHumanTurn = currentBidderFromSystem is HumanPlayer;
        SetButtonsInteractable(isHumanTurn, m_currentBiddingRound, faceUpCard);
    }

    //----------------------------------------------
    void SetButtonsInteractable(bool interactable, BelootBiddingSystem.BiddingRound biddingRound, BeloteCard faceUpCard)
    {
        if (!interactable)
        {
            // Disable all buttons if not human turn
            if (passButton != null) passButton.interactable = false;
            if (sunButton != null) sunButton.interactable = false;
            if (clubsButton != null) clubsButton.interactable = false;
            if (diamondsButton != null) diamondsButton.interactable = false;
            if (heartsButton != null) heartsButton.interactable = false;
            if (spadesButton != null) spadesButton.interactable = false;
            return;
        }

        // Enable buttons based on round rules
        if (passButton != null) passButton.interactable = true;
        if (sunButton != null) sunButton.interactable = true;

        if (biddingRound == BelootBiddingSystem.BiddingRound.BiddingRound1)
        {
            // Round 1: Can only choose face-up card suit
            if (faceUpCard != null)
            {
                if (clubsButton != null) clubsButton.interactable = (faceUpCard.Family == Card32Family.Clubs);
                if (diamondsButton != null) diamondsButton.interactable = (faceUpCard.Family == Card32Family.Diamond);
                if (heartsButton != null) heartsButton.interactable = (faceUpCard.Family == Card32Family.Heart);
                if (spadesButton != null) spadesButton.interactable = (faceUpCard.Family == Card32Family.Spade);
            }
        }
        else // Round 2
        {
            // Round 2: Can choose any suit EXCEPT the face-up suit
            bool disableFaceUp = (faceUpCard != null);
            if (clubsButton != null) clubsButton.interactable = !(disableFaceUp && faceUpCard.Family == Card32Family.Clubs);
            if (diamondsButton != null) diamondsButton.interactable = !(disableFaceUp && faceUpCard.Family == Card32Family.Diamond);
            if (heartsButton != null) heartsButton.interactable = !(disableFaceUp && faceUpCard.Family == Card32Family.Heart);
            if (spadesButton != null) spadesButton.interactable = !(disableFaceUp && faceUpCard.Family == Card32Family.Spade);
        }
    }

    //----------------------------------------------
    void SubmitBid(Bid bid)
    {
        // Check if we should prevent bid submission during transitions
        if (m_preventBidSubmission)
        {
            Debug.Log($"[FIRST BIDDER DEBUG] BiddingUI SubmitBid ignored due to transition flag");
            return;
        }
        
        // CRITICAL: Always read current bidder from system, not cached value
        Player currentBidderFromSystem = m_stage?.BiddingSystem?.CurrentBidder;
        
        if (m_stage != null && currentBidderFromSystem is HumanPlayer)
        {
            Debug.Log($"[BIDDING SYSTEM] Submitting bid for: {currentBidderFromSystem.Name} (from system, not cache)");
            m_stage.SubmitBid(currentBidderFromSystem, bid);
        }
        else
        {
            Debug.LogWarning($"[BIDDING SYSTEM] Cannot submit bid - current bidder is not human or system is null");
        }
    }

    //----------------------------------------------
    // Update the marker position based on the current bidder
    void UpdateBidderMarker()
    {
        if (currentBidderMarker == null)
        {
            // No marker assigned, skip
            return;
        }
        
        // Get the current bidder
        Player currentBidder = m_stage?.BiddingSystem?.CurrentBidder;
        
        if (currentBidder == null)
        {
            // No current bidder, hide the marker
            currentBidderMarker.SetActive(false);
            return;
        }
        
        // Show the marker
        currentBidderMarker.SetActive(true);
        
        // Position the marker based on player position
        Vector3 markerPosition = GetMarkerPositionForPlayer(currentBidder.Position);
        currentBidderMarker.transform.position = markerPosition;
        
        Debug.Log($"[BiddingUI] Marker moved to {currentBidder.Position} position for {currentBidder.Name}");
    }
    
    //----------------------------------------------
    // Get the world position for the marker based on player position
    Vector3 GetMarkerPositionForPlayer(PlayerPosition position)
    {
        // Get screen dimensions
        float halfHeight = Camera.main.orthographicSize;
        float halfWidth = halfHeight * Camera.main.aspect;
        
        Vector3 markerPos = Vector3.zero;
        
        // Position marker near each player's area
        // These positions match the card dealing positions from GameStageRenderer
        switch (position)
        {
            case PlayerPosition.South:
                // Bottom center
                markerPos.x = 0f;
                markerPos.y = -0.65f * halfHeight;
                markerPos.z = -1f; // Slightly in front for visibility
                break;
                
            case PlayerPosition.West:
                // Left side
                markerPos.x = -0.75f * halfWidth;
                markerPos.y = 0.5f * halfHeight;
                markerPos.z = -1f;
                break;
                
            case PlayerPosition.North:
                // Top center
                markerPos.x = 0f;
                markerPos.y = 0.65f * halfHeight;
                markerPos.z = -1f;
                break;
                
            case PlayerPosition.East:
                // Right side
                markerPos.x = 0.75f * halfWidth;
                markerPos.y = 0.5f * halfHeight;
                markerPos.z = -1f;
                break;
        }
        
        return markerPos;
    }

    //----------------------------------------------
    void OnGUI()
    {
        // Fallback GUI for testing (remove when proper UI is implemented)
        if (m_isBiddingActive && m_showBiddingUI)
        {
            // Debug: Log what's being displayed in the fallback GUI
            
            GUI.Box(new Rect(10, 10, 360, 250), "Bidding Round");
            
            // Get bidding system reference
            BelootBiddingSystem systemBidding = m_stage.BiddingSystem;
            
            // Debug: Show current round
            string biddingRoundText = m_currentBiddingRound == BelootBiddingSystem.BiddingRound.BiddingRound1 ? "Round 1" : 
                                     m_currentBiddingRound == BelootBiddingSystem.BiddingRound.BiddingRound2 ? "Round 2" : "Multiplier Bidding";
            GUI.Label(new Rect(20, 20, 280, 20), $"Current Round: {biddingRoundText}");
            
            // ALWAYS use the ACTUAL system value, not cached UI value
            Player systemCurrentBidder = systemBidding?.CurrentBidder;
            
            if (systemCurrentBidder != null)
            {
                GUI.Label(new Rect(20, 40, 280, 20), $"Current Bidder: {systemCurrentBidder.Name}");
                
                // Show dealer information
                if (m_stage != null && m_stage.Dealer != null)
                {
                    GUI.Label(new Rect(20, 200, 280, 20), $"Dealer: {m_stage.Dealer.Name}");
                }
                
                // Show round first player info
                if (m_stage != null && m_stage.RoundFirstPlayer != null)
                {
                    GUI.Label(new Rect(20, 220, 280, 20), $"First Bidder: {m_stage.RoundFirstPlayer.Name}");
                }
            }
            else
            {
                GUI.Label(new Rect(20, 40, 280, 20), "Current Bidder: None");
            }
            
            if (m_highestBid != null && !m_highestBid.IsPass)
            {
                GUI.Label(new Rect(20, 60, 280, 20), $"Highest Bid: {m_highestBid.DisplayName}");
            }
            else
            {
                GUI.Label(new Rect(20, 60, 280, 20), "Highest Bid: None");
            }
            
            // Check if we're waiting for trump suit selection (show to winning bidder, not current bidder)
            if (systemBidding != null && systemBidding.WaitingForTrumpSuitSelection && systemBidding.WinningBidder is HumanPlayer)
            {
                Debug.Log($"[BiddingUI] Showing trump suit selection to winning bidder: {systemBidding.WinningBidder?.Name}");
                ShowTrumpSuitSelection();
                return;
            }
            else if (systemBidding != null && systemBidding.WaitingForTrumpSuitSelection)
            {
                Debug.Log($"[BiddingUI] Waiting for trump suit selection but winning bidder is not human: {systemBidding.WinningBidder?.Name}");
            }
            
            // Check if we're in multiplier bidding phase
            if (m_inMultiplierBidding && systemCurrentBidder is HumanPlayer)
            {
                ShowMultiplierBiddingOptions(systemCurrentBidder);
                return;
            }
            
            // CRITICAL: Use system current bidder, not cached value
            if (systemCurrentBidder is HumanPlayer)
            {
                GUI.Label(new Rect(20, 80, 280, 20), "Choose your bid:");
                
                bool isTrumpTaker = (systemBidding != null && systemBidding.TrumpTaker == systemCurrentBidder);
                bool biddingRound1 = (m_currentBiddingRound == BelootBiddingSystem.BiddingRound.BiddingRound1);
                
                
                if (biddingRound1)
                {
                    // Round 1: Different options based on if Trump was already chosen
                    if (m_trumpChosen)
                    {
                        // If Trump was chosen, remaining players can only choose Sun or Pass
                        if (GUI.Button(new Rect(20, 100, 80, 30), "Sun"))
                        {
                            SubmitBid(Bid.CreateSun());
                        }
                        if (GUI.Button(new Rect(110, 100, 80, 30), "Pass"))
                        {
                            SubmitBid(Bid.CreatePass());
                        }
                    }
                    else
                    {
                        // First players see Sun, Trump, Pass
                        if (GUI.Button(new Rect(20, 100, 80, 30), "Sun"))
                        {
                            SubmitBid(Bid.CreateSun());
                        }
                        if (GUI.Button(new Rect(110, 100, 80, 30), "Trump"))
                        {
                            // In Round 1, Trump means face-up suit
                            Card32Family faceUpSuit = m_faceUpCard != null ? m_faceUpCard.Family : Card32Family.Clubs;
                            SubmitBid(Bid.CreateTrump(faceUpSuit));
                            m_trumpChosen = true; // Mark that Trump was chosen
                        }
                        if (GUI.Button(new Rect(200, 100, 80, 30), "Pass"))
                        {
                            SubmitBid(Bid.CreatePass());
                        }
                    }
                }
                else
                {
                    // Round 2: Different options based on player's Round 1 action and if Another Trump was chosen
                    bool anotherTrumpChosen = m_anotherTrumpChosen || (systemBidding != null && systemBidding.AnotherTrumpChosen);
                    if (anotherTrumpChosen)
                    {
                        // If Another Trump was chosen, all remaining players can only choose Sun or Pass
                        if (GUI.Button(new Rect(20, 100, 80, 30), "Sun"))
                        {
                            SubmitBid(Bid.CreateSun());
                        }
                        if (GUI.Button(new Rect(110, 100, 80, 30), "Pass"))
                        {
                            SubmitBid(Bid.CreatePass());
                        }
                    }
                    else if (isTrumpTaker)
                    {
                        // Trump taker sees Sun and Confirm Trump
                        if (GUI.Button(new Rect(20, 100, 80, 30), "Sun"))
                        {
                            SubmitBid(Bid.CreateSun());
                        }
                        if (GUI.Button(new Rect(110, 100, 120, 30), "Confirm Trump"))
                        {
                            // Confirm Trump means keeping the face-up suit
                            Card32Family faceUpSuit = m_faceUpCard != null ? m_faceUpCard.Family : Card32Family.Clubs;
                            SubmitBid(Bid.CreateTrump(faceUpSuit));
                        }
                    }
                    else
                    {
                        // Non-trump taker: options depend on whether anyone chose Trump in Round 1
                        if (GUI.Button(new Rect(20, 100, 80, 30), "Sun"))
                        {
                            SubmitBid(Bid.CreateSun());
                        }
                        
                        // Only show "Another Trump" if no one chose Trump in Round 1 (all passed)
                        if (!m_trumpChosen)
                        {
                            if (GUI.Button(new Rect(110, 100, 120, 30), "Another Trump"))
                            {
                                // Another Trump means choosing a different suit (not face-up)
                                // Submit a placeholder bid - the actual suit will be selected next
                                Card32Family faceUpSuit = m_faceUpCard != null ? m_faceUpCard.Family : Card32Family.Clubs;
                                Card32Family placeholderSuit = GetAnotherTrumpSuit(faceUpSuit); // Just a placeholder
                                SubmitBid(Bid.CreateTrump(placeholderSuit));
                                m_anotherTrumpChosen = true; // Mark that Another Trump was chosen
                                // The system will now wait for trump suit selection via ShowTrumpSuitSelection()
                            }
                            if (GUI.Button(new Rect(240, 100, 80, 30), "Pass"))
                            {
                                SubmitBid(Bid.CreatePass());
                            }
                        }
                        else
                        {
                            // If someone chose Trump in Round 1, non-trump takers can only Pass
                            if (GUI.Button(new Rect(110, 100, 80, 30), "Pass"))
                            {
                                SubmitBid(Bid.CreatePass());
                            }
                        }
                    }
                }
            }
            else
            {
                GUI.Label(new Rect(20, 80, 280, 20), "Waiting for AI to bid...");
            }
        }
    }
    
    // Helper method to get a different trump suit (not the face-up suit)
    private Card32Family GetAnotherTrumpSuit(Card32Family faceUpSuit)
    {
        // Return the first suit that's not the face-up suit
        if (faceUpSuit != Card32Family.Clubs) return Card32Family.Clubs;
        if (faceUpSuit != Card32Family.Diamond) return Card32Family.Diamond;
        if (faceUpSuit != Card32Family.Heart) return Card32Family.Heart;
        return Card32Family.Spade; // Default fallback
    }
    
    // Show trump suit selection UI
    private void ShowTrumpSuitSelection()
    {
        Debug.Log("[BiddingUI] === ShowTrumpSuitSelection() called ===");
        
        GUI.Label(new Rect(20, 80, 280, 20), "Select Trump Suit:");
        GUI.Label(new Rect(20, 100, 280, 20), "Choose any suit except the face-up card suit");
        
        // Get face-up card suit to exclude it
        Card32Family faceUpSuit = m_faceUpCard != null ? m_faceUpCard.Family : Card32Family.Clubs;
        
        // Show suit buttons (excluding face-up suit)
        if (faceUpSuit != Card32Family.Clubs)
        {
            if (GUI.Button(new Rect(20, 120, 80, 30), "Clubs"))
            {
                m_stage.BiddingSystem.SelectTrumpSuit(Card32Family.Clubs);
            }
        }
        
        if (faceUpSuit != Card32Family.Diamond)
        {
            if (GUI.Button(new Rect(110, 120, 80, 30), "Diamonds"))
            {
                m_stage.BiddingSystem.SelectTrumpSuit(Card32Family.Diamond);
            }
        }
        
        if (faceUpSuit != Card32Family.Heart)
        {
            if (GUI.Button(new Rect(200, 120, 80, 30), "Hearts"))
            {
                m_stage.BiddingSystem.SelectTrumpSuit(Card32Family.Heart);
            }
        }
        
        if (faceUpSuit != Card32Family.Spade)
        {
            if (GUI.Button(new Rect(290, 120, 80, 30), "Spades"))
            {
                m_stage.BiddingSystem.SelectTrumpSuit(Card32Family.Spade);
            }
        }
        
        // Show face-up card info
        if (m_faceUpCard != null)
        {
            GUI.Label(new Rect(20, 160, 280, 20), $"Face-up card: {m_faceUpCard.Value} of {m_faceUpCard.Family} (cannot be trump)");
        }
    }

    //----------------------------------------------
    // ShowMultiplierBiddingOptions
    //----------------------------------------------
    // Show UI for multiplier bidding (doubles/triples/quadruples)
    private void ShowMultiplierBiddingOptions(Player currentBidder)
    {
        GUI.Label(new Rect(20, 80, 280, 20), "Multiplier Bidding:");
        
        // Show current multiplier status
        string multiplierText = m_currentMultiplier == 1 ? "Normal (1x)" : 
                               m_currentMultiplier == 2 ? "Double (2x)" : 
                               m_currentMultiplier == 3 ? "Triple (3x)" : "Quadruple (4x)";
        GUI.Label(new Rect(20, 100, 280, 20), $"Current: {multiplierText}");
        
        // Show trump confirmer info
        if (m_trumpConfirmer != null)
        {
            GUI.Label(new Rect(20, 120, 280, 20), $"Trump Confirmer: {m_trumpConfirmer.Name}");
        }
        
        // Determine what options to show
        bool isOpposingTeam = (currentBidder.Team != m_trumpConfirmer.Team);
        
        if (isOpposingTeam != m_isOpposingTeamTurn)
        {
            // Not this player's turn (wrong team)
            GUI.Label(new Rect(20, 140, 280, 20), "Waiting for other team...");
            return;
        }
        
        GUI.Label(new Rect(20, 140, 280, 20), "Choose your action:");
        
        // Pass button (always available)
        if (GUI.Button(new Rect(20, 160, 80, 30), "Pass"))
        {
            SubmitBid(Bid.CreatePass());
        }
        
        // Escalation button (if not at maximum)
        if (m_currentMultiplier < 4)
        {
            string escalateButtonText = "";
            Bid escalateBid = null;
            
            switch (m_currentMultiplier)
            {
                case 1:
                    escalateButtonText = "Double (2x)";
                    escalateBid = Bid.CreateDouble();
                    break;
                case 2:
                    escalateButtonText = "Triple (3x)";
                    escalateBid = Bid.CreateTriple();
                    break;
                case 3:
                    escalateButtonText = "Quadruple (4x)";
                    escalateBid = Bid.CreateQuadruple();
                    break;
            }
            
            if (escalateBid != null && GUI.Button(new Rect(110, 160, 120, 30), escalateButtonText))
            {
                SubmitBid(escalateBid);
            }
        }
        
        // Show explanation
        string teamRole = isOpposingTeam ? "(Opposing Team)" : "(Trump Confirmer Team)";
        GUI.Label(new Rect(20, 200, 340, 20), $"You are {teamRole}");
        
        if (isOpposingTeam)
        {
            GUI.Label(new Rect(20, 220, 340, 20), "You can escalate or pass to end bidding");
        }
        else
        {
            GUI.Label(new Rect(20, 220, 340, 20), "You can escalate or pass to end bidding");
        }
    }
}
