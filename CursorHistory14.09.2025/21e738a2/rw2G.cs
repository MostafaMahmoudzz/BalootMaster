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
    private BelootBiddingSystem.BiddingRound m_currentRound; // Track current round for fallback GUI
    private BeloteCard m_faceUpCard;              // Track face-up card for fallback GUI
    private bool m_anotherTrumpChosen;            // Track if someone chose "Another Trump" in Round 2
    private bool m_trumpChosen;                   // Track if someone chose "Trump" in Round 1
    
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
        GameEventDispatcher.Subscribe<BiddingRound2StartEvent>(this.OnBiddingRound2Start);

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
        GameEventDispatcher.UnSubscribe<BiddingRound2StartEvent>(this.OnBiddingRound2Start);
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
        Debug.Log($"[BiddingUI] OnBiddingStart received - CurrentBidder: {evt.CurrentBidder?.Name}, Round: {evt.Round}");
        
        m_isBiddingActive = true;
        m_currentBidder = evt.CurrentBidder;
        m_highestBid = evt.HighestBid;
        m_currentRound = evt.Round;
        m_faceUpCard = evt.FaceUpCard;
        m_anotherTrumpChosen = false;
        m_trumpChosen = false;
        
        Debug.Log($"[BiddingUI] After OnBiddingStart - m_currentBidder: {m_currentBidder?.Name}");
        
        ShowBiddingUI();
        UpdateBiddingDisplay(evt.Round, evt.FaceUpCard);
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
        Debug.Log($"[BiddingUI] OnBiddingTurn received - CurrentBidder: {evt.CurrentBidder?.Name}, Round: {evt.Round}");
        
        m_currentBidder = evt.CurrentBidder;
        m_highestBid = evt.HighestBid;
        m_currentRound = evt.Round;
        
        Debug.Log($"[BiddingUI] After OnBiddingTurn - m_currentBidder: {m_currentBidder?.Name}");
        
        UpdateBiddingDisplay(evt.Round, null);
    }

    //----------------------------------------------
    void OnBiddingRound2Start(BiddingRound2StartEvent evt)
    {
        m_currentBidder = evt.CurrentBidder;
        m_currentRound = BelootBiddingSystem.BiddingRound.Round2;
        m_faceUpCard = evt.FaceUpCard;
        m_anotherTrumpChosen = false;
        
        Debug.Log($"[BiddingUI] Round 2 started - Current bidder: {m_currentBidder?.Name}, Trump taker: {evt.TrumpTaker?.Name}");
        
        ShowBiddingUI();
        UpdateBiddingDisplay(BelootBiddingSystem.BiddingRound.Round2, evt.FaceUpCard);
    }

    //----------------------------------------------
    void OnBidSubmitted(BidSubmittedEvent evt)
    {
        // Update display when any player submits a bid
        if (m_highestBid == null || evt.Bid.IsHigherThan(m_highestBid))
        {
            m_highestBid = evt.Bid;
        }
        
        UpdateBiddingDisplay(m_currentRound, m_faceUpCard);
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
    void UpdateBiddingDisplay(BelootBiddingSystem.BiddingRound round, BeloteCard faceUpCard)
    {
        if (!m_showBiddingUI)
            return;

        // Keep internal state in sync for fallback GUI
        m_currentRound = round;
        if (faceUpCard != null)
        {
            m_faceUpCard = faceUpCard;
        }

        // Debug: show current bidding round in console
        Debug.Log($"[BiddingUI] Current Bidding Round: {(m_currentRound == BelootBiddingSystem.BiddingRound.Round1 ? "1" : "2")}");

        // Update round text
        if (roundText != null)
        {
            roundText.text = $"Round: {(round == BelootBiddingSystem.BiddingRound.Round1 ? "1" : "2")}";
        }

        // Update face-up card text
        if (faceUpCardText != null && faceUpCard != null)
        {
            faceUpCardText.text = $"Face-up Card: {faceUpCard.Family}";
        }

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

        // Enable/disable buttons based on current bidder and round rules
        bool isHumanTurn = m_currentBidder is HumanPlayer;
        SetButtonsInteractable(isHumanTurn, round, faceUpCard);
    }

    //----------------------------------------------
    void SetButtonsInteractable(bool interactable, BelootBiddingSystem.BiddingRound round, BeloteCard faceUpCard)
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

        if (round == BelootBiddingSystem.BiddingRound.Round1)
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
            // Debug: Log what's being displayed in the fallback GUI
            if (Time.frameCount % 60 == 0) // Log every 60 frames to avoid spam
            {
                Debug.Log($"[BiddingUI] OnGUI - m_currentBidder: {m_currentBidder?.Name}, m_isBiddingActive: {m_isBiddingActive}, m_showBiddingUI: {m_showBiddingUI}");
            }
            
            GUI.Box(new Rect(10, 10, 360, 180), "Bidding Round");
            
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

            // Get bidding system to check trump taker
            BelootBiddingSystem biddingSystem = m_stage.BiddingSystem;
            
            // Check if we're waiting for trump suit selection (show to winning bidder, not current bidder)
            if (biddingSystem.WaitingForTrumpSuitSelection && biddingSystem.WinningBidder is HumanPlayer)
            {
                Debug.Log($"[BiddingUI] Showing trump suit selection to winning bidder: {biddingSystem.WinningBidder?.Name}");
                ShowTrumpSuitSelection();
                return;
            }
            else if (biddingSystem.WaitingForTrumpSuitSelection)
            {
                Debug.Log($"[BiddingUI] Waiting for trump suit selection but winning bidder is not human: {biddingSystem.WinningBidder?.Name}");
            }
            
            if (m_currentBidder is HumanPlayer)
            {
                GUI.Label(new Rect(20, 80, 280, 20), "Choose your bid:");
                
                bool isTrumpTaker = (biddingSystem.TrumpTaker == m_currentBidder);
                bool round1 = (m_currentRound == BelootBiddingSystem.BiddingRound.Round1);
                
                // Debug logging
                Debug.Log($"[BiddingUI] Current bidder: {m_currentBidder?.Name}, Trump taker: {biddingSystem.TrumpTaker?.Name}, Is trump taker: {isTrumpTaker}, Round: {(round1 ? "1" : "2")}");
                
                if (round1)
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
                    bool anotherTrumpChosen = m_anotherTrumpChosen || biddingSystem.AnotherTrumpChosen;
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
                                Card32Family faceUpSuit = m_faceUpCard != null ? m_faceUpCard.Family : Card32Family.Clubs;
                                Card32Family anotherSuit = GetAnotherTrumpSuit(faceUpSuit);
                                SubmitBid(Bid.CreateTrump(anotherSuit));
                                m_anotherTrumpChosen = true; // Mark that Another Trump was chosen
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
}
