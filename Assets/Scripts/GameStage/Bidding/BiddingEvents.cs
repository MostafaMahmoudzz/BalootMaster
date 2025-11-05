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
    public BelootBiddingSystem.BiddingRound Round { get; set; }
    public BeloteCard FaceUpCard { get; set; }

    public override void Reset()
    {
        CurrentBidder = null;
        HighestBid = null;
        Round = BelootBiddingSystem.BiddingRound.BiddingRound1;
        FaceUpCard = null;
    }
}

//-------------------------------------------------------
// BiddingCompleteEvent
//-------------------------------------------------------
public class BiddingCompleteEvent : PooledEvent
{
    public Player WinningBidder { get; set; }
    public Bid WinningBid { get; set; }
    public bool SunDeclared { get; set; }

    public override void Reset()
    {
        WinningBidder = null;
        WinningBid = null;
        SunDeclared = false;
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
    public BelootBiddingSystem.BiddingRound Round { get; set; }

    public override void Reset()
    {
        CurrentBidder = null;
        HighestBid = null;
        Round = BelootBiddingSystem.BiddingRound.BiddingRound1;
    }
}

//-------------------------------------------------------
// BiddingRound2StartEvent
//-------------------------------------------------------
public class BiddingRound2StartEvent : PooledEvent
{
    public Player CurrentBidder { get; set; }
    public Player TrumpTaker { get; set; }
    public BeloteCard FaceUpCard { get; set; }

    public override void Reset()
    {
        CurrentBidder = null;
        TrumpTaker = null;
        FaceUpCard = null;
    }
}

//-------------------------------------------------------
// BiddingTurnEventIgnoreEvent
//-------------------------------------------------------
public class BiddingTurnEventIgnoreEvent : PooledEvent
{
    public bool Ignore { get; set; }

    public override void Reset()
    {
        Ignore = false;
    }
}

//-------------------------------------------------------
// BiddingEventSubscriptionEvent
//-------------------------------------------------------
public class BiddingEventSubscriptionEvent : PooledEvent
{
    public bool Subscribe { get; set; } // true = subscribe, false = unsubscribe

    public override void Reset()
    {
        Subscribe = false;
    }
}

//-------------------------------------------------------
// BiddingNoBidsEvent
//-------------------------------------------------------
public class BiddingNoBidsEvent : PooledEvent
{
    public bool BothRoundsNoBids { get; set; }

    public override void Reset()
    {
        BothRoundsNoBids = false;
    }
}

//-------------------------------------------------------
// MultiplierBiddingStartEvent
//-------------------------------------------------------
public class MultiplierBiddingStartEvent : PooledEvent
{
    public Player CurrentBidder { get; set; }
    public Player TrumpConfirmer { get; set; }
    public int CurrentMultiplier { get; set; } // 1, 2, 3, or 4
    public bool IsOpposingTeamTurn { get; set; } // true if opposing team can escalate

    public override void Reset()
    {
        CurrentBidder = null;
        TrumpConfirmer = null;
        CurrentMultiplier = 1;
        IsOpposingTeamTurn = false;
    }
}

//-------------------------------------------------------
// MultiplierBiddingTurnEvent
//-------------------------------------------------------
public class MultiplierBiddingTurnEvent : PooledEvent
{
    public Player CurrentBidder { get; set; }
    public int CurrentMultiplier { get; set; }
    public bool IsOpposingTeamTurn { get; set; }

    public override void Reset()
    {
        CurrentBidder = null;
        CurrentMultiplier = 1;
        IsOpposingTeamTurn = false;
    }
}