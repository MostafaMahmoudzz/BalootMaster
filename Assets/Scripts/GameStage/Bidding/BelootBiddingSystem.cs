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

    //----------------------------------------------
    // Properties
    public Player CurrentBidder
    {
        get
        {
            if (m_biddingOrder != null && m_currentBidderIndex < m_biddingOrder.Count)
            {
                return m_biddingOrder[m_currentBidderIndex];
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
    }

    //-------------------------------------------------------
    public void StartBidding(List<Player> players, Player firstBidder, BeloteCard faceUpCard)
    {
        m_biddingOrder.Clear();
        m_biddingOrder.AddRange(players);
        
        // Find first bidder index
        m_currentBidderIndex = m_biddingOrder.IndexOf(firstBidder);
        if (m_currentBidderIndex == -1)
        {
            m_currentBidderIndex = 0; // Fallback to first player
        }

        m_highestBid = null;
        m_biddingComplete = false;
        m_winningBidder = null;
        m_currentRound = BiddingRound.Round1;
        m_faceUpCard = faceUpCard;
        m_trumpTaker = null;
        m_sunDeclared = false;

        // Send bidding start event
        BiddingStartEvent evt = Pools.Claim<BiddingStartEvent>();
        evt.CurrentBidder = CurrentBidder;
        evt.HighestBid = m_highestBid;
        evt.Round = m_currentRound;
        evt.FaceUpCard = m_faceUpCard;
        GameEventDispatcher.SendEvent(evt);
    }

    //-------------------------------------------------------
    public bool SubmitBid(Player player, Bid bid)
    {
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

        // Move to next bidder
        MoveToNextBidder();

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
            if (m_currentRound == BiddingRound.Round1)
            {
                // Round 1: Can only choose face-up card suit
                return bid.Suit == m_faceUpCard.Family;
            }
            else // Round 2
            {
                if (m_trumpTaker != null)
                {
                    // Case A: Trump taker can choose any suit
                    return true;
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
        if (bid.IsTrump)
        {
            m_highestBid = bid;
            m_winningBidder = player;
            
            // In Round 1, remember who took trump
            if (m_currentRound == BiddingRound.Round1)
            {
                m_trumpTaker = player;
            }
        }
        else if (bid.IsSun)
        {
            m_highestBid = bid;
            m_winningBidder = player;
        }

        // Check if bidding is complete
        CheckBiddingComplete();
    }

    //-------------------------------------------------------
    private void MoveToNextBidder()
    {
        m_currentBidderIndex = (m_currentBidderIndex - 1 + m_biddingOrder.Count) % m_biddingOrder.Count;
    }

    //-------------------------------------------------------
    private void CheckBiddingComplete()
    {
        // Count how many players have bid in current round
        int bidsSubmitted = 0;
        foreach (Player player in m_biddingOrder)
        {
            if (player.HasBid)
            {
                bidsSubmitted++;
            }
        }

        // Round is complete when all players have bid
        if (bidsSubmitted >= m_biddingOrder.Count)
        {
            if (m_currentRound == BiddingRound.Round1)
            {
                StartRound2();
            }
            else
            {
                m_biddingComplete = true;
                FinalizeBidding();
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

        // Determine starting player for Round 2
        if (m_trumpTaker != null)
        {
            // Case A: Trump taker goes first
            m_currentBidderIndex = m_biddingOrder.IndexOf(m_trumpTaker);
        }
        else
        {
            // Case B: Start with player to right of dealer (same as Round 1)
            // This is already set from Round 1
        }

        // Send Round 2 start event
        BiddingRound2StartEvent evt = Pools.Claim<BiddingRound2StartEvent>();
        evt.CurrentBidder = CurrentBidder;
        evt.TrumpTaker = m_trumpTaker;
        evt.FaceUpCard = m_faceUpCard;
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
        m_biddingOrder.Clear();
        m_currentBidderIndex = 0;
        m_highestBid = null;
        m_biddingComplete = false;
        m_winningBidder = null;
        m_currentRound = BiddingRound.Round1;
        m_faceUpCard = null;
        m_trumpTaker = null;
        m_sunDeclared = false;
    }
}
