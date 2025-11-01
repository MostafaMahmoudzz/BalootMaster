using System;
using System.Collections.Generic;
using UnityEngine;
using Pebble;

//-------------------------------------------------------
// BelootBiddingSystem
//-------------------------------------------------------
// Purpose:
//   Manages the complete Beloot bidding system with 2 rounds,
//   face-up card, and proper contract types (Trump, Sun, Pass).
//
// How it connects to other scripts:
//   - Used by `GameStage` to conduct bidding rounds
//   - Manages `Bid` instances for each player's bid
//   - Integrates with `Player` system for turn management
//-------------------------------------------------------
public class BelootBiddingSystem
{
    //----------------------------------------------
    // Bidding Rounds
    public enum BiddingRound
    {
        Round1,     // First round of bidding
        Round2       // Second round of bidding
    }

    //----------------------------------------------
    // Variables
    private List<Player> m_biddingOrder;           // Order of players for bidding
    private int m_currentBidderIndex;              // Index of current bidding player
    private Bid m_highestBid;                      // Current highest bid
    private bool m_biddingComplete;                // Is bidding round finished?
    private Player m_winningBidder;                // Player who won the bidding
    private BiddingRound m_currentRound;           // Current bidding round
    private BeloteCard m_faceUpCard;               // Face-up card revealed by dealer
    private Player m_trumpTaker;                   // Player who took trump in Round 1
    private bool m_sunDeclared;                    // Has Sun been declared?
    private Player m_firstBidder;                  // First player who made a bid in current round
    private bool m_round1NoBids;                   // Whether no player made Trump/Sun bid in Round 1
    private bool m_round2NoBids;                   // Whether no player made Trump/Sun bid in Round 2
    private bool m_anotherTrumpChosen;             // Whether "Another Trump" was chosen in Round 2

    //----------------------------------------------
    // Properties
    public Player CurrentBidder
    {
        get
        {
            if (m_biddingOrder != null && m_currentBidderIndex < m_biddingOrder.Count)
            {
                Player currentBidder = m_biddingOrder[m_currentBidderIndex];
                // Debug: Log every time CurrentBidder is accessed
                if (Time.frameCount % 60 == 0) // Log every 60 frames to avoid spam
                {
                    Debug.Log($"[BiddingSystem] CurrentBidder accessed: {currentBidder?.Name} (index: {m_currentBidderIndex})");
                }
                return currentBidder;
            }
            Debug.LogWarning($"[BiddingSystem] CurrentBidder accessed but invalid state - order: {m_biddingOrder?.Count}, index: {m_currentBidderIndex}");
            return null;
        }
    }

    public Bid HighestBid
    {
        get { return m_highestBid; }
    }

    public bool IsComplete
    {
        get { return m_biddingComplete; }
    }

    public Player WinningBidder
    {
        get { return m_winningBidder; }
    }

    public BiddingRound CurrentRound
    {
        get { return m_currentRound; }
    }

    public BeloteCard FaceUpCard
    {
        get { return m_faceUpCard; }
    }

    public Player TrumpTaker
    {
        get { return m_trumpTaker; }
    }

    public bool SunDeclared
    {
        get { return m_sunDeclared; }
    }

    public bool AnotherTrumpChosen
    {
        get { return m_anotherTrumpChosen; }
    }

    //----------------------------------------------
    // Methods
    public BelootBiddingSystem()
    {
        m_biddingOrder = new List<Player>();
        m_currentBidderIndex = 0;
        m_highestBid = null;
        m_biddingComplete = false;
        m_winningBidder = null;
        m_currentRound = BiddingRound.Round1;
        m_faceUpCard = null;
        m_trumpTaker = null;
        m_sunDeclared = false;
        m_firstBidder = null;
        m_round1NoBids = false;
        m_round2NoBids = false;
        m_anotherTrumpChosen = false;
    }

    //-------------------------------------------------------
    public void StartBidding(List<Player> players, Player firstBidder, BeloteCard faceUpCard)
    {
        Debug.Log($"[BiddingSystem] Starting bidding with {players.Count} players, first bidder: {firstBidder?.Name}");
        
        // Debug: Show the input players list
        Debug.Log($"[BiddingSystem] Input players list:");
        for (int i = 0; i < players.Count; i++)
        {
            Debug.Log($"[BiddingSystem]   Input[{i}]: {players[i].Name}");
        }
        
        m_biddingOrder.Clear();
        m_biddingOrder.AddRange(players);
        
        // Debug: Show the bidding order after adding
        Debug.Log($"[BiddingSystem] Bidding order after AddRange:");
        for (int i = 0; i < m_biddingOrder.Count; i++)
        {
            Debug.Log($"[BiddingSystem]   Order[{i}]: {m_biddingOrder[i].Name}");
        }
        
        // Find first bidder index
        m_currentBidderIndex = m_biddingOrder.IndexOf(firstBidder);
        Debug.Log($"[BiddingSystem] Looking for first bidder: {firstBidder?.Name}");
        Debug.Log($"[BiddingSystem] IndexOf result: {m_currentBidderIndex}");
        
        // Debug: Verify each player in the list
        for (int i = 0; i < m_biddingOrder.Count; i++)
        {
            bool isMatch = m_biddingOrder[i] == firstBidder;
            Debug.Log($"[BiddingSystem]   Player[{i}]: {m_biddingOrder[i].Name} == {firstBidder?.Name}? {isMatch}");
        }
        
        if (m_currentBidderIndex == -1)
        {
            Debug.LogWarning($"[BiddingSystem] First bidder {firstBidder?.Name} not found in player list, using index 0");
            m_currentBidderIndex = 0; // Fallback to first player
        }
        
        Debug.Log($"[BiddingSystem] Bidding order set with {m_biddingOrder.Count} players, current bidder index: {m_currentBidderIndex}");
        Debug.Log($"[BiddingSystem] Current bidder is: {CurrentBidder?.Name}");

        m_highestBid = null;
        m_biddingComplete = false;
        m_winningBidder = null;
        m_currentRound = BiddingRound.Round1;
        
        // Debug: Verify the state after initialization
        Debug.Log($"[BiddingSystem] After StartBidding - CurrentBidder: {CurrentBidder?.Name}, Index: {m_currentBidderIndex}, Order Count: {m_biddingOrder.Count}");
        m_faceUpCard = faceUpCard;
        m_trumpTaker = null;
        m_sunDeclared = false;
        m_firstBidder = null;
        m_round1NoBids = false;
        m_round2NoBids = false;
        m_anotherTrumpChosen = false;

        // Send bidding start event
        BiddingStartEvent evt = Pools.Claim<BiddingStartEvent>();
        evt.CurrentBidder = CurrentBidder;
        evt.HighestBid = m_highestBid;
        evt.Round = m_currentRound;
        evt.FaceUpCard = m_faceUpCard;
        
        Debug.Log($"[BiddingSystem] Sending BiddingStartEvent - CurrentBidder: {evt.CurrentBidder?.Name}, Round: {evt.Round}");
        Debug.Log($"[BiddingSystem] BiddingStartEvent details - CurrentBidder: {evt.CurrentBidder?.Name}, HighestBid: {evt.HighestBid?.DisplayName}, Round: {evt.Round}, FaceUpCard: {evt.FaceUpCard?.Value} of {evt.FaceUpCard?.Family}");
        GameEventDispatcher.SendEvent(evt);
        Debug.Log($"[BiddingSystem] BiddingStartEvent sent successfully");
    }

    //-------------------------------------------------------
    public bool SubmitBid(Player player, Bid bid)
    {
        // Check if bidding system is properly initialized
        if (m_biddingOrder.Count == 0)
        {
            Debug.LogError($"[BiddingSystem] Cannot submit bid - bidding system not initialized! Player: {player?.Name}");
            return false;
        }

        // Validate it's the player's turn
        if (player != CurrentBidder)
        {
            Debug.LogWarning($"It's not {player.Name}'s turn to bid!");
            return false;
        }

        // Validate bid based on current round and rules
        if (!IsValidBid(bid))
        {
            Debug.LogWarning($"Invalid bid from {player.Name}!");
            return false;
        }

        // Process the bid
        ProcessBid(player, bid);

        // Check if Sun was declared (immediate end)
        if (bid.IsSun)
        {
            m_sunDeclared = true;
            m_biddingComplete = true;
            m_winningBidder = player;
            m_highestBid = bid;
            FinalizeBidding();
            return true;
        }

        // Mark this player as having bid
        player.HasBid = true;

        // Store current round before checking completion
        BiddingRound roundBeforeCheck = m_currentRound;

        // Check if current round has completed (all players bid)
        CheckBiddingComplete();

        // Only move to next bidder if bidding is not complete
        if (!m_biddingComplete)
        {
            // Move to next bidder (with safety check)
            if (m_biddingOrder.Count > 0)
            {
                MoveToNextBidder();
            }
            else
            {
                Debug.LogError("[BiddingSystem] Cannot move to next bidder - bidding order is empty!");
            }
        }
        else
        {
            // Bidding is complete, don't move to next bidder
            Debug.Log("[BiddingSystem] Bidding complete - not moving to next bidder");
        }

        return true;
    }

    //-------------------------------------------------------
    private bool IsValidBid(Bid bid)
    {
        if (bid == null)
        {
            return false;
        }

        // Pass is always valid
        if (bid.IsPass)
        {
            return true;
        }

        // Sun is always valid
        if (bid.IsSun)
        {
            return true;
        }

        // Trump validation depends on round
        if (bid.IsTrump)
        {
            // If "Another Trump" was already chosen, no more Trump bids allowed
            if (m_anotherTrumpChosen)
            {
                return false;
            }
            
            if (m_currentRound == BiddingRound.Round1)
            {
                // Round 1: Can only choose face-up card suit
                return bid.Suit == m_faceUpCard.Family;
            }
            else // Round 2
            {
                if (m_trumpTaker != null)
                {
                    // Case A: Trump taker can only choose face-up suit (Confirm Trump)
                    return bid.Suit == m_faceUpCard.Family;
                }
                else
                {
                    // Case B: Can choose any suit except face-up card
                    return bid.Suit != m_faceUpCard.Family;
                }
            }
        }

        return false;
    }

    //-------------------------------------------------------
    private void ProcessBid(Player player, Bid bid)
    {
        // Track first bidder in current round (including Pass)
        if (m_firstBidder == null)
        {
            m_firstBidder = player;
            Debug.Log($"[BiddingSystem] First bidder in Round {(m_currentRound == BiddingRound.Round1 ? "1" : "2")}: {player.Name}");
        }

        if (bid.IsTrump)
        {
            m_highestBid = bid;
            m_winningBidder = player;
            
            // In Round 1, remember who took trump
            if (m_currentRound == BiddingRound.Round1)
            {
                m_trumpTaker = player;
                Debug.Log($"[BiddingSystem] {player.Name} chose Trump in Round 1 - set as trump taker");
            }
            else if (m_currentRound == BiddingRound.Round2 && m_trumpTaker == null)
            {
                // Round 2, Case B: Someone chose "Another Trump" (different from face-up suit)
                if (bid.Suit != m_faceUpCard.Family)
                {
                    m_anotherTrumpChosen = true;
                    Debug.Log($"[BiddingSystem] {player.Name} chose Another Trump ({bid.Suit}) in Round 2 - limiting remaining players to Sun/Pass");
                }
            }
        }
        else if (bid.IsSun)
        {
            m_highestBid = bid;
            m_winningBidder = player;
            m_sunDeclared = true;
            
            // Sun in Round 2 ends bidding immediately (override rule)
            if (m_currentRound == BiddingRound.Round2)
            {
                Debug.Log($"[BiddingSystem] {player.Name} chose Sun in Round 2 - ending bidding immediately");
                m_biddingComplete = true;
                FinalizeBidding();
                return; // Exit early, don't check bidding complete
            }
        }

        // Check if bidding is complete (only if Sun didn't end it immediately)
        CheckBiddingComplete();
    }

    //-------------------------------------------------------
    private void MoveToNextBidder()
    {
        // Safety check to prevent division by zero
        if (m_biddingOrder.Count == 0)
        {
            Debug.LogError("[BiddingSystem] Cannot move to next bidder - bidding order is empty!");
            return;
        }
        
        m_currentBidderIndex = (m_currentBidderIndex - 1 + m_biddingOrder.Count) % m_biddingOrder.Count;
    }

    //-------------------------------------------------------
    private void CheckBiddingComplete()
    {
        // Safety check to prevent issues with empty bidding order
        if (m_biddingOrder.Count == 0)
        {
            Debug.LogError("[BiddingSystem] Cannot check bidding completion - bidding order is empty!");
            return;
        }
        
        // A bidding round ends when all players have taken their chance (bid or pass)
        int bidsSubmitted = 0;
        bool hasActualBid = false; // True if any player made Trump or Sun bid
        
        foreach (Player player in m_biddingOrder)
        {
            if (player.HasBid)
            {
                bidsSubmitted++;
                if (player.CurrentBid != null && (player.CurrentBid.IsTrump || player.CurrentBid.IsSun))
                {
                    hasActualBid = true;
                }
            }
        }

        if (bidsSubmitted >= m_biddingOrder.Count)
        {
            if (m_currentRound == BiddingRound.Round1)
            {
                // Check if no player made Trump/Sun bid in Round 1
                if (!hasActualBid)
                {
                    m_round1NoBids = true;
                    Debug.Log("[BiddingSystem] Round 1 complete: no Trump/Sun bids made. Starting Round 2.");
                }
                else
                {
                    Debug.Log("[BiddingSystem] Round 1 complete: some players made Trump/Sun bids. Starting Round 2.");
                }
                StartRound2();
            }
            else
            {
                // Check if no player made Trump/Sun bid in Round 2
                if (!hasActualBid)
                {
                    m_round2NoBids = true;
                    Debug.Log("[BiddingSystem] Round 2 complete: no Trump/Sun bids made.");
                }
                else
                {
                    Debug.Log("[BiddingSystem] Round 2 complete: some players made Trump/Sun bids.");
                }
                
                // Check if both rounds had no actual bids (Trump/Sun)
                if (m_round1NoBids && m_round2NoBids)
                {
                    Debug.Log("[BiddingSystem] Both rounds had no Trump/Sun bids - need new dealer and new round.");
                    m_biddingComplete = true; // Set bidding complete to prevent further actions
                    SendNoBidsEvent();
                }
                else
                {
                    m_biddingComplete = true;
                    FinalizeBidding();
                }
            }
        }
    }

    //-------------------------------------------------------
    private void StartRound2()
    {
        m_currentRound = BiddingRound.Round2;
        
        // Reset all players' bidding state for Round 2
        foreach (Player player in m_biddingOrder)
        {
            player.ResetBidding();
        }

        // Reset first bidder for Round 2
        m_firstBidder = null;

        // Determine starting player for Round 2
        // IMPORTANT: Bidding continues in normal anti-clockwise order from where Round 1 ended
        // The trump taker does NOT jump to the front - they get their turn in the normal order
        if (m_trumpTaker != null)
        {
            Debug.Log($"[BiddingSystem] Round 2: Trump taker is {m_trumpTaker.Name}, but bidding continues in normal order");
            Debug.Log($"[BiddingSystem] Round 2: Current bidder index remains {m_currentBidderIndex} (normal anti-clockwise order)");
        }
        else
        {
            // Case B: Start with player to right of dealer (same as Round 1)
            // This is already set from Round 1
            Debug.Log("[BiddingSystem] Round 2 starting with dealer's right (no trump taker)");
        }

        // Send Round 2 start event
        BiddingRound2StartEvent evt = Pools.Claim<BiddingRound2StartEvent>();
        evt.CurrentBidder = CurrentBidder;
        evt.TrumpTaker = m_trumpTaker;
        evt.FaceUpCard = m_faceUpCard;
        GameEventDispatcher.SendEvent(evt);
        
        // Send bidding turn event to start Round 2 bidding
        BiddingTurnEvent turnEvt = Pools.Claim<BiddingTurnEvent>();
        turnEvt.CurrentBidder = CurrentBidder;
        turnEvt.HighestBid = m_highestBid;
        turnEvt.Round = m_currentRound;
        GameEventDispatcher.SendEvent(turnEvt);
    }

    //-------------------------------------------------------
    private void SendNoBidsEvent()
    {
        // Send event indicating both rounds had no Trump/Sun bids - need new dealer
        BiddingNoBidsEvent evt = Pools.Claim<BiddingNoBidsEvent>();
        evt.BothRoundsNoBids = true;
        GameEventDispatcher.SendEvent(evt);
    }

    //-------------------------------------------------------
    private void FinalizeBidding()
    {
        // If no contract was made, set default
        if (m_highestBid == null || m_highestBid.IsPass)
        {
            // Case C: All passed, no contract
            m_winningBidder = null;
            m_highestBid = null;
        }

        // Send bidding complete event
        BiddingCompleteEvent evt = Pools.Claim<BiddingCompleteEvent>();
        evt.WinningBidder = m_winningBidder;
        evt.WinningBid = m_highestBid;
        evt.SunDeclared = m_sunDeclared;
        GameEventDispatcher.SendEvent(evt);
    }

    //-------------------------------------------------------
    public void Reset()
    {
        Debug.Log("[BiddingSystem] Resetting bidding system...");
        
        // Only reset if bidding is complete or not started
        if (!m_biddingComplete && m_biddingOrder.Count > 0)
        {
            Debug.LogWarning("[BiddingSystem] Resetting while bidding is active - this may cause issues");
        }
        
        m_biddingOrder.Clear();
        m_currentBidderIndex = 0;
        m_highestBid = null;
        m_biddingComplete = false;
        m_winningBidder = null;
        m_currentRound = BiddingRound.Round1;
        m_faceUpCard = null;
        m_trumpTaker = null;
        m_sunDeclared = false;
        m_firstBidder = null;
        m_round1NoBids = false;
        m_round2NoBids = false;
        m_anotherTrumpChosen = false;
        
        Debug.Log("[BiddingSystem] Bidding system reset complete");
    }
}
