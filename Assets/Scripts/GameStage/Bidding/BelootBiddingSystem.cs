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
        BiddingRound1,     // First round of bidding
        BiddingRound2       // Second round of bidding
    }

    //----------------------------------------------
    // Variables
    private List<Player> m_biddingOrder;           // Order of players for bidding
    private int m_currentBidderIndex;              // Index of current bidding player
    private Bid m_highestBid;                      // Current highest bid
    private bool m_biddingComplete;                // Is bidding round finished?
    private Player m_winningBidder;                // Player who won the bidding
    private BiddingRound m_currentBiddingRound;           // Current bidding round
    private BeloteCard m_faceUpCard;               // Face-up card revealed by dealer
    private Player m_trumpTaker;                   // Player who took trump in Round 1
    private bool m_sunDeclared;                    // Has Sun been declared?
    private Player m_firstBidder;                  // First player who made a bid in current round
    private bool m_round1NoBids;                   // Whether no player made Trump/Sun bid in Round 1
    private bool m_round2NoBids;                   // Whether no player made Trump/Sun bid in Round 2
    private bool m_anotherTrumpChosen;             // Whether "Another Trump" was chosen in Round 2
    private bool m_trumpConfirmed;                 // Whether Trump was confirmed in Round 2 (face-up suit chosen)
    private bool m_waitingForTrumpSuitSelection;   // Whether we're waiting for trump suit selection after "Another Trump"

    //----------------------------------------------
    // Properties
    public Player CurrentBidder
    {
        get
        {
            if (m_biddingOrder != null && m_currentBidderIndex < m_biddingOrder.Count)
            {
                Player currentBidder = m_biddingOrder[m_currentBidderIndex];
                Debug.Log($"[FIRST BIDDER DEBUG] CurrentBidder: {currentBidder?.Name} (index: {m_currentBidderIndex})");
                return currentBidder;
            }
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

    public BiddingRound CurrentBiddingRound
    {
        get { return m_currentBiddingRound; }
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

    public bool TrumpConfirmed
    {
        get { return m_trumpConfirmed; }
    }

    public bool WaitingForTrumpSuitSelection
    {
        get { return m_waitingForTrumpSuitSelection; }
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
        m_currentBiddingRound = BiddingRound.BiddingRound1;
        m_faceUpCard = null;
        m_trumpTaker = null;
        m_sunDeclared = false;
        m_firstBidder = null;
        m_round1NoBids = false;
        m_round2NoBids = false;
        m_anotherTrumpChosen = false;
        m_trumpConfirmed = false;
        m_waitingForTrumpSuitSelection = false;
    }

    //-------------------------------------------------------
    public void StartBidding(List<Player> players, Player firstBidder, BeloteCard faceUpCard)
    {
        Debug.Log($"[FIRST BIDDER DEBUG] StartBidding called with first bidder: {firstBidder?.Name}");
        
        m_biddingOrder.Clear();
        m_biddingOrder.AddRange(players);
        
        // Find first bidder index
        m_currentBidderIndex = m_biddingOrder.IndexOf(firstBidder);
        Debug.Log($"[FIRST BIDDER DEBUG] First bidder index: {m_currentBidderIndex}");
        
        // ============================================
        // CLEAR BIDDING SYSTEM INFO - READ THIS!
        // ============================================
        Debug.LogError("╔════════════════════════════════════════════════════════╗");
        Debug.LogError($"║ BIDDING SYSTEM STARTED");
        Debug.LogError($"║ First bidder parameter: {firstBidder?.Name}");
        Debug.LogError($"║ First bidder index: {m_currentBidderIndex}");
        Debug.LogError($"║ CurrentBidder property: {CurrentBidder?.Name}");
        Debug.LogError($"║ Bidding order in system:");
        for (int i = 0; i < m_biddingOrder.Count; i++)
        {
            string marker = (i == m_currentBidderIndex) ? " ← FIRST BIDDER" : "";
            Debug.LogError($"║   [{i}]: {m_biddingOrder[i].Name}{marker}");
        }
        Debug.LogError("╚════════════════════════════════════════════════════════╝");
        // ============================================
        
        if (m_currentBidderIndex == -1)
        {
            Debug.LogWarning($"[FIRST BIDDER DEBUG] First bidder {firstBidder?.Name} not found in player list, using index 0");
            m_currentBidderIndex = 0; // Fallback to first player
        }

        m_highestBid = null;
        m_biddingComplete = false;
        m_winningBidder = null;
        m_currentBiddingRound = BiddingRound.BiddingRound1;
        m_faceUpCard = faceUpCard;
        m_trumpTaker = null;
        m_sunDeclared = false;
        m_firstBidder = firstBidder; // Set the correct first bidder from the start
        m_round1NoBids = false;
        m_round2NoBids = false;
        m_anotherTrumpChosen = false;
        m_trumpConfirmed = false;
        m_waitingForTrumpSuitSelection = false;

        // Send bidding start event
        BiddingStartEvent evt = Pools.Claim<BiddingStartEvent>();
        evt.CurrentBidder = CurrentBidder;
        evt.HighestBid = m_highestBid;
        evt.Round = m_currentBiddingRound;
        evt.FaceUpCard = m_faceUpCard;
        
        GameEventDispatcher.SendEvent(evt);
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

        // Check if bidding is already complete
        if (m_biddingComplete)
        {
            Debug.LogWarning($"[BiddingSystem] Cannot submit bid - bidding is already complete! Player: {player?.Name}");
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

        // Check if Another Trump was declared in Round 2 (wait for trump suit selection if human, immediate end if AI)
        if (m_currentBiddingRound == BiddingRound.BiddingRound2 && m_anotherTrumpChosen)
        {
            player.HasBid = true;
            
            // Check if player is human - if so, wait for suit selection UI
            if (player is HumanPlayer)
            {
                Debug.Log($"[BiddingSystem] {player.Name} (Human) declared Another Trump - waiting for trump suit selection");
                m_waitingForTrumpSuitSelection = true;
                // Don't move to next bidder, don't finalize yet - wait for trump suit selection via UI
                return true;
            }
            else
            {
                // AI player - suit already selected, finalize immediately
                Debug.Log($"[BiddingSystem] {player.Name} (AI) declared Another Trump ({bid.Suit}) - ending bidding immediately");
                m_biddingComplete = true;
                FinalizeBidding();
                return true;
            }
        }

        // Mark this player as having bid
        player.HasBid = true;

        // Store current round before checking completion
        BiddingRound biddingRoundBeforeCheck = m_currentBiddingRound;

        // Check if current round has completed (all players bid)
        CheckBiddingComplete();

        // CRITICAL FIX: Check if round changed (e.g., Round 1 → Round 2)
        bool roundChanged = (m_currentBiddingRound != biddingRoundBeforeCheck);
        
        if (roundChanged)
        {
            Debug.LogError($"[BIDDING SYSTEM] ⚠️ Round changed from {biddingRoundBeforeCheck} to {m_currentBiddingRound}");
            Debug.LogError($"[BIDDING SYSTEM] ⚠️ NOT calling MoveToNextBidder because round changed!");
            Debug.LogError($"[BIDDING SYSTEM] ⚠️ Current bidder should stay: {CurrentBidder?.Name}");
            // DON'T move to next bidder when round changes - StartBiddingRound2 already set the correct bidder
        }
        else if (!m_biddingComplete)
        {
            // Same round, bidding not complete - move to next bidder
            if (m_biddingOrder.Count > 0)
            {
                Debug.Log($"[BIDDING SYSTEM] Same round, moving to next bidder");
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
            
            if (m_currentBiddingRound == BiddingRound.BiddingRound1)
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
        // Note: m_firstBidder should already be set correctly in StartBidding/StartBiddingRound2
        // No need to track first bidder here - it's already set to the correct player

        if (bid.IsTrump)
        {
            m_highestBid = bid;
            m_winningBidder = player;
            
            // In Round 1, remember who took trump
            if (m_currentBiddingRound == BiddingRound.BiddingRound1)
            {
                m_trumpTaker = player;
                Debug.Log($"[BiddingSystem] {player.Name} chose Trump in Round 1 - set as trump taker");
            }
            else if (m_currentBiddingRound == BiddingRound.BiddingRound2)
            {
                if (m_trumpTaker != null && bid.Suit == m_faceUpCard.Family)
                {
                    // Round 2, Case A: Trump taker confirms Trump (face-up suit)
                    m_trumpConfirmed = true;
                    Debug.Log($"[BiddingSystem] {player.Name} confirmed Trump ({bid.Suit}) in Round 2 - contract will be finalized after Round 2 completes");
                }
                else if (m_trumpTaker == null && bid.Suit != m_faceUpCard.Family)
                {
                    // Round 2, Case B: Someone chose "Another Trump" (different from face-up suit)
                    // Set flag and wait for trump suit selection, then immediately finalize
                    m_anotherTrumpChosen = true;
                    Debug.Log($"[BiddingSystem] {player.Name} chose Another Trump ({bid.Suit}) in Round 2 - waiting for suit selection, then will end bidding immediately");
                }
            }
        }
        else if (bid.IsSun)
        {
            m_highestBid = bid;
            m_winningBidder = player;
            m_sunDeclared = true;
            
            // Sun in Round 2 ends bidding immediately (override rule)
            if (m_currentBiddingRound == BiddingRound.BiddingRound2)
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
        
        // Send turn event for the new current bidder
        BiddingTurnEvent evt = Pools.Claim<BiddingTurnEvent>();
        evt.CurrentBidder = CurrentBidder;
        evt.HighestBid = m_highestBid;
        evt.Round = m_currentBiddingRound;
        
        // DEBUG: Verify event contains correct bidder
        Debug.LogError($"[MOVE TO NEXT] ⚠️ SENDING BiddingTurnEvent with CurrentBidder = {evt.CurrentBidder?.Name}");
        Debug.LogError($"[MOVE TO NEXT] ⚠️ m_currentBidderIndex = {m_currentBidderIndex}, bidder = {m_biddingOrder[m_currentBidderIndex]?.Name}");
        
        GameEventDispatcher.SendEvent(evt);
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
        
        Debug.Log($"[BiddingSystem] CheckBiddingComplete - Round: {m_currentBiddingRound}");
        Debug.Log($"[BiddingSystem] Trump confirmed: {m_trumpConfirmed}, Sun declared: {m_sunDeclared}, Another Trump chosen: {m_anotherTrumpChosen}");
        
        foreach (Player player in m_biddingOrder)
        {
            if (player.HasBid)
            {
                bidsSubmitted++;
                Debug.Log($"[BiddingSystem] Player {player.Name} has bid: {player.CurrentBid?.ToString()}");
                if (player.CurrentBid != null && (player.CurrentBid.IsTrump || player.CurrentBid.IsSun))
                {
                    hasActualBid = true;
                    Debug.Log($"[BiddingSystem] Found actual bid from {player.Name}: {player.CurrentBid.ToString()}");
                }
            }
            else
            {
                Debug.Log($"[BiddingSystem] Player {player.Name} has no bid yet");
            }
        }
        
        // For Round 2, we need to consider Trump confirmation, Sun declaration, and Another Trump
        bool hasActualBidInCurrentRound = hasActualBid;
        if (m_currentBiddingRound == BiddingRound.BiddingRound2)
        {
            // In Round 2, we also consider Trump confirmation, Sun declaration, and Another Trump as "actual bids"
            if (m_trumpConfirmed || m_sunDeclared || m_anotherTrumpChosen)
            {
                hasActualBid = true;
                hasActualBidInCurrentRound = true; // IMPORTANT: Also set this for Round 2 completion logic
                Debug.Log($"[BiddingSystem] Round 2: Considering Trump confirmed ({m_trumpConfirmed}), Sun declared ({m_sunDeclared}), or Another Trump ({m_anotherTrumpChosen}) as actual bid");
            }
        }
        
        Debug.Log($"[BiddingSystem] Bids submitted: {bidsSubmitted}/{m_biddingOrder.Count}, Has actual bid: {hasActualBid} (current round: {hasActualBidInCurrentRound})");

        if (bidsSubmitted >= m_biddingOrder.Count)
        {
            Debug.Log($"[BiddingSystem] All players have submitted bids in {m_currentBiddingRound}");
            
            if (m_currentBiddingRound == BiddingRound.BiddingRound1)
            {
                // Check if no player made Trump/Sun bid in Round 1
                if (!hasActualBidInCurrentRound)
                {
                    m_round1NoBids = true;
                    Debug.Log($"[FIRST BIDDER DEBUG] Round 1 complete: no Trump/Sun bids made. Setting m_round1NoBids = true");
                }
                else
                {
                    Debug.Log("[BiddingSystem] Round 1 complete: some players made Trump/Sun bids. Starting Round 2.");
                }
                StartBiddingRound2();
            }
            else if (m_currentBiddingRound == BiddingRound.BiddingRound2)
            {
                Debug.Log("[BiddingSystem] Round 2 is complete - this is the FINAL bidding round!");
                // Check if no player made Trump/Sun bid in Round 2
                if (!hasActualBidInCurrentRound)
                {
                    m_round2NoBids = true;
                    Debug.Log($"[FIRST BIDDER DEBUG] Round 2 complete: no Trump/Sun bids made. Setting m_round2NoBids = true");
                    Debug.Log($"[BiddingSystem] Trump confirmed: {m_trumpConfirmed}, Sun declared: {m_sunDeclared}, Another Trump: {m_anotherTrumpChosen}");
                }
                else
                {
                    Debug.Log("[BiddingSystem] Round 2 complete: some players made Trump/Sun bids in current round.");
                    Debug.Log($"[BiddingSystem] Trump confirmed: {m_trumpConfirmed}, Sun declared: {m_sunDeclared}, Another Trump: {m_anotherTrumpChosen}");
                }
                
                // Check if both rounds had no actual bids (Trump/Sun)
                Debug.Log($"[FIRST BIDDER DEBUG] CheckBiddingComplete - Round1NoBids: {m_round1NoBids}, Round2NoBids: {m_round2NoBids}");
                if (m_round1NoBids && m_round2NoBids)
                {
                    Debug.Log("[BiddingSystem] Both rounds had no Trump/Sun bids - finalizing with no contract.");
                    m_biddingComplete = true; 
                    FinalizeBidding(); // Send BiddingCompleteEvent with no contract instead of SendNoBidsEvent
                }
                else
                {
                    // Round 2 completed with actual bids
                    Debug.Log("[BiddingSystem] Round 2 completed with actual bids - checking contract type...");
                    if (m_trumpConfirmed)
                    {
                        // Trump was confirmed - finalize the Trump contract
                        Debug.Log("[BiddingSystem] Round 2 complete with Trump confirmed - finalizing Trump contract");
                        Debug.Log("[BiddingSystem] About to call FinalizeBidding() for Trump contract");
                        m_biddingComplete = true;
                        FinalizeBidding();
                        Debug.Log("[BiddingSystem] FinalizeBidding() completed for Trump contract");
                    }
                    else if (m_sunDeclared)
                    {
                        // Sun was declared - contract already finalized in ProcessBid
                        Debug.Log("[BiddingSystem] Round 2 complete with Sun declared - contract already finalized");
                        // Contract was already finalized when Sun was declared
                    }
                    else if (m_anotherTrumpChosen)
                    {
                        // Another Trump was chosen - this shouldn't happen as bidding should end after trump suit selection
                        Debug.LogWarning("[BiddingSystem] Round 2 complete with Another Trump chosen - this shouldn't happen (should have ended after suit selection)");
                        // This case shouldn't occur as SelectTrumpSuit should have already finalized
                    }
                    else
                    {
                        // This shouldn't happen, but handle it gracefully
                        Debug.LogWarning("[BiddingSystem] Round 2 complete with actual bids but no clear contract type - finalizing anyway");
                        m_biddingComplete = true;
                        FinalizeBidding();
                    }
                }
            }
        }
    }

    //-------------------------------------------------------
    private void StartBiddingRound2()
    {
        m_currentBiddingRound = BiddingRound.BiddingRound2;
        
        // Reset all players' bidding state for Round 2
        foreach (Player player in m_biddingOrder)
        {
            player.ResetBidding();
        }

        // Reset Trump confirmation for Round 2 (but keep the same first bidder)
        m_trumpConfirmed = false;
        m_waitingForTrumpSuitSelection = false;
        m_round2NoBids = false; // Reset Round 2 no bids flag
        Debug.Log($"[FIRST BIDDER DEBUG] StartBiddingRound2 - Reset m_round2NoBids to false");

        // CRITICAL FIX: Round 2 must start with the SAME first bidder as Round 1
        // Reset current bidder index back to the first bidder (player to dealer's right)
        if (m_firstBidder != null)
        {
            m_currentBidderIndex = m_biddingOrder.IndexOf(m_firstBidder);
            Debug.Log($"[BiddingSystem] Round 2: Resetting to first bidder {m_firstBidder.Name} (index: {m_currentBidderIndex})");
            
            if (m_currentBidderIndex == -1)
            {
                Debug.LogError($"[BiddingSystem] ERROR: First bidder {m_firstBidder.Name} not found in bidding order!");
                m_currentBidderIndex = 0; // Fallback
            }
        }
        else
        {
            Debug.LogError("[BiddingSystem] ERROR: m_firstBidder is null in Round 2!");
            m_currentBidderIndex = 0; // Fallback to first player
        }

        // ============================================
        // VERIFY ROUND 2 START
        // ============================================
        Debug.LogError("╔════════════════════════════════════════════════════════╗");
        Debug.LogError($"║ ROUND 2 BIDDING STARTED");
        Debug.LogError($"║ First bidder (should be same as Round 1): {m_firstBidder?.Name}");
        Debug.LogError($"║ Current bidder index: {m_currentBidderIndex}");
        Debug.LogError($"║ Current bidder: {CurrentBidder?.Name}");
        Debug.LogError($"║ Trump taker from Round 1: {m_trumpTaker?.Name ?? "None"}");
        Debug.LogError("╚════════════════════════════════════════════════════════╝");
        // ============================================

        // Send Round 2 start event
        BiddingRound2StartEvent evt = Pools.Claim<BiddingRound2StartEvent>();
        evt.CurrentBidder = CurrentBidder;
        evt.TrumpTaker = m_trumpTaker;
        evt.FaceUpCard = m_faceUpCard;
        
        // DEBUG: Verify event contains correct bidder
        Debug.LogError($"[BIDDING SYSTEM] ⚠️ SENDING BiddingRound2StartEvent with CurrentBidder = {evt.CurrentBidder?.Name}");
        Debug.LogError($"[BIDDING SYSTEM] ⚠️ System CurrentBidder property value = {CurrentBidder?.Name}");
        Debug.LogError($"[BIDDING SYSTEM] ⚠️ m_currentBidderIndex = {m_currentBidderIndex}, bidder at that index = {m_biddingOrder[m_currentBidderIndex]?.Name}");
        
        GameEventDispatcher.SendEvent(evt);
        
        // Don't send BiddingTurnEvent here - let the normal bidding flow continue
        // The current bidder should already be set correctly
    }

    //-------------------------------------------------------
    // SendNoBidsEvent method removed - no longer needed since FinalizeBidding handles no-contract case

    //-------------------------------------------------------
    public void SelectTrumpSuit(Card32Family trumpSuit)
    {
        Debug.Log($"[BiddingSystem] === SelectTrumpSuit called with suit: {trumpSuit} ===");
        Debug.Log($"[BiddingSystem] WaitingForTrumpSuitSelection: {m_waitingForTrumpSuitSelection}");
        Debug.Log($"[BiddingSystem] BiddingComplete: {m_biddingComplete}");
        
        if (!m_waitingForTrumpSuitSelection)
        {
            Debug.LogWarning("[BiddingSystem] SelectTrumpSuit called but not waiting for trump suit selection");
            return;
        }

        Debug.Log($"[BiddingSystem] Trump suit selected: {trumpSuit}");
        
        // Update the highest bid with the selected trump suit
        if (m_highestBid != null && m_highestBid.IsTrump)
        {
            m_highestBid = Bid.CreateTrump(trumpSuit);
            Debug.Log($"[BiddingSystem] Updated highest bid to: {m_highestBid.ToString()}");
        }

        // Clear the waiting state
        m_waitingForTrumpSuitSelection = false;
        
        // Set bidding complete and finalize
        Debug.Log("[BiddingSystem] Setting bidding complete and finalizing with selected trump suit");
        m_biddingComplete = true;
        FinalizeBidding();
    }

    //-------------------------------------------------------
    private void FinalizeBidding()
    {
        Debug.Log("[BiddingSystem] === FinalizeBidding() called ===");
        Debug.Log($"[BiddingSystem] WinningBidder: {m_winningBidder?.Name}");
        Debug.Log($"[BiddingSystem] WinningBid: {m_highestBid?.ToString()}");
        Debug.Log($"[BiddingSystem] SunDeclared: {m_sunDeclared}");
        
        // If no contract was made, set default
        if (m_highestBid == null || m_highestBid.IsPass)
        {
            // Case C: All passed, no contract
            Debug.Log("[BiddingSystem] No contract made - setting defaults");
            m_winningBidder = null;
            m_highestBid = null;
        }
        else
        {
            Debug.Log($"[BiddingSystem] Contract made: {m_highestBid.ToString()} by {m_winningBidder.Name}");
        }

        // Send bidding complete event
        Debug.Log("[BiddingSystem] Sending BiddingCompleteEvent...");
        BiddingCompleteEvent evt = Pools.Claim<BiddingCompleteEvent>();
        evt.WinningBidder = m_winningBidder;
        evt.WinningBid = m_highestBid;
        evt.SunDeclared = m_sunDeclared;
        GameEventDispatcher.SendEvent(evt);
        Debug.Log("[BiddingSystem] BiddingCompleteEvent sent successfully");
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
        m_currentBiddingRound = BiddingRound.BiddingRound1;
        m_faceUpCard = null;
        m_trumpTaker = null;
        m_sunDeclared = false;
        m_firstBidder = null;
        m_round1NoBids = false;
        m_round2NoBids = false;
        m_anotherTrumpChosen = false;
        m_trumpConfirmed = false;
        m_waitingForTrumpSuitSelection = false;
        
        Debug.Log("[BiddingSystem] Bidding system reset complete");
    }
}
