using System;
using UnityEngine;
using Pebble;

//-------------------------------------------------------
// Bidding Events
//-------------------------------------------------------
// Purpose:
//   Events related to the bidding system for communication
//   between bidding logic and UI systems.
//-------------------------------------------------------

//-------------------------------------------------------
// BiddingStartEvent
//-------------------------------------------------------
public class BiddingStartEvent : PooledEvent
{
    public Player CurrentBidder { get; set; }
    public Bid HighestBid { get; set; }

    public override void Reset()
    {
        CurrentBidder = null;
        HighestBid = null;
    }
}

//-------------------------------------------------------
// BiddingCompleteEvent
//-------------------------------------------------------
public class BiddingCompleteEvent : PooledEvent
{
    public Player WinningBidder { get; set; }
    public Bid WinningBid { get; set; }

    public override void Reset()
    {
        WinningBidder = null;
        WinningBid = null;
    }
}

//-------------------------------------------------------
// BidSubmittedEvent
//-------------------------------------------------------
public class BidSubmittedEvent : PooledEvent
{
    public Player Player { get; set; }
    public Bid Bid { get; set; }

    public override void Reset()
    {
        Player = null;
        Bid = null;
    }
}

//-------------------------------------------------------
// BiddingTurnEvent
//-------------------------------------------------------
public class BiddingTurnEvent : PooledEvent
{
    public Player CurrentBidder { get; set; }
    public Bid HighestBid { get; set; }

    public override void Reset()
    {
        CurrentBidder = null;
        HighestBid = null;
    }
}
