using System;
using System.Collections.Generic;
using UnityEngine;
using Pebble;

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
//-------------------------------------------------------
public class BiddingUI : MonoBehaviour
{
    //----------------------------------------------
    // Variables
    private GameStage m_stage;                    // Reference to game stage
    private bool m_isBiddingActive;               // Is bidding currently active?
    private Player m_currentBidder;               // Current player bidding
    private Bid m_highestBid;                     // Current highest bid
    private bool m_showBiddingUI;                 // Should UI be visible?

    // UI Elements (these would be assigned in Unity Inspector)
    public GameObject biddingPanel;               // Main bidding panel
    public UnityEngine.UI.Button passButton;      // Pass button
    public UnityEngine.UI.Button clubsButton;     // Clubs bid button
    public UnityEngine.UI.Button diamondsButton;  // Diamonds bid button
    public UnityEngine.UI.Button heartsButton;    // Hearts bid button
    public UnityEngine.UI.Button spadesButton;    // Spades bid button
    public UnityEngine.UI.Text currentBidderText; // Current bidder display
    public UnityEngine.UI.Text highestBidText;    // Highest bid display
    public UnityEngine.UI.Text biddingInstructions; // Instructions text

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
        
        // Subscribe to bidding events
        GameEventDispatcher.Subscribe<BiddingStartEvent>(this.OnBiddingStart);
        GameEventDispatcher.Subscribe<BiddingCompleteEvent>(this.OnBiddingComplete);
        GameEventDispatcher.Subscribe<BiddingTurnEvent>(this.OnBiddingTurn);
        GameEventDispatcher.Subscribe<BidSubmittedEvent>(this.OnBidSubmitted);

        // Setup button listeners
        SetupButtonListeners();

        // Hide UI initially
        HideBiddingUI();
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        GameEventDispatcher.UnSubscribe<BiddingStartEvent>(this.OnBiddingStart);
        GameEventDispatcher.UnSubscribe<BiddingCompleteEvent>(this.OnBiddingComplete);
        GameEventDispatcher.UnSubscribe<BiddingTurnEvent>(this.OnBiddingTurn);
        GameEventDispatcher.UnSubscribe<BidSubmittedEvent>(this.OnBidSubmitted);
    }

    //----------------------------------------------
    void SetupButtonListeners()
    {
        if (passButton != null)
            passButton.onClick.AddListener(() => SubmitBid(Bid.CreatePass()));

        if (clubsButton != null)
            clubsButton.onClick.AddListener(() => SubmitBid(Bid.CreateBid(Card32Family.Clubs)));

        if (diamondsButton != null)
            diamondsButton.onClick.AddListener(() => SubmitBid(Bid.CreateBid(Card32Family.Diamond)));

        if (heartsButton != null)
            heartsButton.onClick.AddListener(() => SubmitBid(Bid.CreateBid(Card32Family.Heart)));

        if (spadesButton != null)
            spadesButton.onClick.AddListener(() => SubmitBid(Bid.CreateBid(Card32Family.Spade)));
    }

    //----------------------------------------------
    void OnBiddingStart(BiddingStartEvent evt)
    {
        m_isBiddingActive = true;
        m_currentBidder = evt.CurrentBidder;
        m_highestBid = evt.HighestBid;
        
        ShowBiddingUI();
        UpdateBiddingDisplay();
    }

    //----------------------------------------------
    void OnBiddingComplete(BiddingCompleteEvent evt)
    {
        m_isBiddingActive = false;
        HideBiddingUI();
    }

    //----------------------------------------------
    void OnBiddingTurn(BiddingTurnEvent evt)
    {
        m_currentBidder = evt.CurrentBidder;
        m_highestBid = evt.HighestBid;
        
        UpdateBiddingDisplay();
    }

    //----------------------------------------------
    void OnBidSubmitted(BidSubmittedEvent evt)
    {
        // Update display when any player submits a bid
        if (m_highestBid == null || evt.Bid.IsHigherThan(m_highestBid))
        {
            m_highestBid = evt.Bid;
        }
        
        UpdateBiddingDisplay();
    }

    //----------------------------------------------
    void ShowBiddingUI()
    {
        m_showBiddingUI = true;
        if (biddingPanel != null)
        {
            biddingPanel.SetActive(true);
        }
    }

    //----------------------------------------------
    void HideBiddingUI()
    {
        m_showBiddingUI = false;
        if (biddingPanel != null)
        {
            biddingPanel.SetActive(false);
        }
    }

    //----------------------------------------------
    void UpdateBiddingDisplay()
    {
        if (!m_showBiddingUI)
            return;

        // Update current bidder text
        if (currentBidderText != null)
        {
            currentBidderText.text = $"Current Bidder: {m_currentBidder?.Name ?? "None"}";
        }

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

        // Update instructions
        if (biddingInstructions != null)
        {
            if (m_currentBidder != null)
            {
                biddingInstructions.text = $"{m_currentBidder.Name}, choose your bid:";
            }
            else
            {
                biddingInstructions.text = "Bidding in progress...";
            }
        }

        // Enable/disable buttons based on current bidder
        bool isHumanTurn = m_currentBidder is HumanPlayer;
        SetButtonsInteractable(isHumanTurn);
    }

    //----------------------------------------------
    void SetButtonsInteractable(bool interactable)
    {
        if (passButton != null)
            passButton.interactable = interactable;
        if (clubsButton != null)
            clubsButton.interactable = interactable;
        if (diamondsButton != null)
            diamondsButton.interactable = interactable;
        if (heartsButton != null)
            heartsButton.interactable = interactable;
        if (spadesButton != null)
            spadesButton.interactable = interactable;
    }

    //----------------------------------------------
    void SubmitBid(Bid bid)
    {
        if (m_stage != null && m_currentBidder is HumanPlayer)
        {
            m_stage.SubmitBid(m_currentBidder, bid);
        }
    }

    //----------------------------------------------
    void OnGUI()
    {
        // Fallback GUI for testing (remove when proper UI is implemented)
        if (m_isBiddingActive && m_showBiddingUI)
        {
            GUI.Box(new Rect(10, 10, 300, 200), "Bidding Round");
            
            if (m_currentBidder != null)
            {
                GUI.Label(new Rect(20, 40, 280, 20), $"Current Bidder: {m_currentBidder.Name}");
            }
            
            if (m_highestBid != null && !m_highestBid.IsPass)
            {
                GUI.Label(new Rect(20, 60, 280, 20), $"Highest Bid: {m_highestBid.DisplayName}");
            }
            else
            {
                GUI.Label(new Rect(20, 60, 280, 20), "Highest Bid: None");
            }

            if (m_currentBidder is HumanPlayer)
            {
                GUI.Label(new Rect(20, 80, 280, 20), "Choose your bid:");
                
                if (GUI.Button(new Rect(20, 100, 60, 30), "Pass"))
                {
                    SubmitBid(Bid.CreatePass());
                }
                
                if (GUI.Button(new Rect(90, 100, 60, 30), "Clubs"))
                {
                    SubmitBid(Bid.CreateBid(Card32Family.Clubs));
                }
                
                if (GUI.Button(new Rect(160, 100, 60, 30), "Diamonds"))
                {
                    SubmitBid(Bid.CreateBid(Card32Family.Diamond));
                }
                
                if (GUI.Button(new Rect(20, 140, 60, 30), "Hearts"))
                {
                    SubmitBid(Bid.CreateBid(Card32Family.Heart));
                }
                
                if (GUI.Button(new Rect(90, 140, 60, 30), "Spades"))
                {
                    SubmitBid(Bid.CreateBid(Card32Family.Spade));
                }
            }
            else
            {
                GUI.Label(new Rect(20, 80, 280, 20), "Waiting for AI to bid...");
            }
        }
    }
}
