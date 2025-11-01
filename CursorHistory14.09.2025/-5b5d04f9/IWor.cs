using System;
using System.Collections.Generic;
using UnityEngine;
using Pebble;

//-------------------------------------------------------
// BiddingSystem
//-------------------------------------------------------
// Purpose:
//   Manages the bidding round where players bid on trump suits.
//   Handles bidding order, bid validation, and winner determination.
//
// How it connects to other scripts:
//   - Used by `GameStage` to conduct bidding rounds
//   - Manages `Bid` instances for each player's bid
//   - Integrates with `Player` system for turn management
//-------------------------------------------------------
public class BiddingSystem
{
    //----------------------------------------------
    // Variables
    private List<Player> m_biddingOrder;           // Order of players for bidding
    private int m_currentBidderIndex;              // Index of current bidding player
    private Bid m_highestBid;                      // Current highest bid
    private bool m_biddingComplete;                // Is bidding round finished?
    private Player m_winningBidder;                // Player who won the bidding

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

    //----------------------------------------------
    // Methods
    public BiddingSystem()
    {
        m_biddingOrder = new List<Player>();
        m_currentBidderIndex = 0;
        m_highestBid = null;
        m_biddingComplete = false;
        m_winningBidder = null;
    }

    //-------------------------------------------------------
    public void StartBidding(List<Player> players, Player firstBidder)
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

        // Send bidding start event
        BiddingStartEvent evt = Pools.Claim<BiddingStartEvent>();
        evt.CurrentBidder = CurrentBidder;
        evt.HighestBid = m_highestBid;
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

        // Validate bid
        if (!IsValidBid(bid))
        {
            Debug.LogWarning($"Invalid bid from {player.Name}!");
            return false;
        }

        // Process the bid
        ProcessBid(player, bid);

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

        // Check if bid is higher than current highest
        if (m_highestBid == null)
        {
            return true; // First bid
        }

        if (m_highestBid.IsPass)
        {
            return true; // Can bid on anything if highest is pass
        }

        // Compare bid values (higher suit beats lower suit)
        return bid.IsHigherThan(m_highestBid);
    }

    //-------------------------------------------------------
    private void ProcessBid(Player player, Bid bid)
    {
        if (!bid.IsPass)
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
        // Count how many players have bid
        int bidsSubmitted = 0;
        foreach (Player player in m_biddingOrder)
        {
            if (player.HasBid)
            {
                bidsSubmitted++;
            }
        }

        // Bidding is complete when all players have bid
        if (bidsSubmitted >= m_biddingOrder.Count)
        {
            m_biddingComplete = true;
            FinalizeBidding();
        }
    }

    //-------------------------------------------------------
    private void FinalizeBidding()
    {
        // If no one bid (all passed), dealer gets to choose trump
        if (m_highestBid == null || m_highestBid.IsPass)
        {
            // In real Beloot, dealer would choose trump
            // For now, we'll use random trump as fallback
            m_winningBidder = m_biddingOrder[0]; // First player (dealer)
            m_highestBid = new Bid();
            m_highestBid.Suit = (Card32Family)UnityEngine.Random.Range(0, 4);
            m_highestBid.IsPass = false;
        }

        // Send bidding complete event
        BiddingCompleteEvent evt = Pools.Claim<BiddingCompleteEvent>();
        evt.WinningBidder = m_winningBidder;
        evt.WinningBid = m_highestBid;
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
    }
}
