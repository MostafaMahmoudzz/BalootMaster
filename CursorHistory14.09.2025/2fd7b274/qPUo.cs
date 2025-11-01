using System;
using UnityEngine;
using Pebble;

//-------------------------------------------------------
// Bid
//-------------------------------------------------------
// Purpose:
//   Represents a single bid made by a player during bidding round.
//   Can be either a pass or a bid on a specific trump suit.
//
// How it connects to other scripts:
//   - Used by `BiddingSystem` to track player bids
//   - Used by `Player` to store their bid
//   - Used by `GameStage` to determine trump and bidder
//-------------------------------------------------------
[Serializable]
public class Bid
{
    //----------------------------------------------
    // Variables
    public Card32Family Suit { get; set; }         // Trump suit being bid on
    public bool IsPass { get; set; }               // True if player passed
    public int BidValue { get; set; }              // Bid value (for future scoring)

    //----------------------------------------------
    // Properties
    public string DisplayName
    {
        get
        {
            if (IsPass)
            {
                return "Pass";
            }
            return Suit.ToString();
        }
    }

    //----------------------------------------------
    // Methods
    public Bid()
    {
        Suit = Card32Family.Clubs; // Default suit
        IsPass = false;
        BidValue = 0;
    }

    //-------------------------------------------------------
    public Bid(Card32Family suit)
    {
        Suit = suit;
        IsPass = false;
        BidValue = GetSuitValue(suit);
    }

    //-------------------------------------------------------
    public Bid(bool isPass)
    {
        IsPass = isPass;
        Suit = Card32Family.Clubs; // Default when passing
        BidValue = 0;
    }

    //-------------------------------------------------------
    public static Bid CreatePass()
    {
        return new Bid(true);
    }

    //-------------------------------------------------------
    public static Bid CreateBid(Card32Family suit)
    {
        return new Bid(suit);
    }

    //-------------------------------------------------------
    private int GetSuitValue(Card32Family suit)
    {
        // Suit hierarchy: Spades > Hearts > Diamonds > Clubs
        switch (suit)
        {
            case Card32Family.Spades:
                return 4;
            case Card32Family.Hearts:
                return 3;
            case Card32Family.Diamonds:
                return 2;
            case Card32Family.Clubs:
                return 1;
            default:
                return 0;
        }
    }

    //-------------------------------------------------------
    public bool IsHigherThan(Bid other)
    {
        if (other == null)
        {
            return !IsPass;
        }

        if (IsPass)
        {
            return false;
        }

        if (other.IsPass)
        {
            return true;
        }

        return BidValue > other.BidValue;
    }

    //-------------------------------------------------------
    public override string ToString()
    {
        return DisplayName;
    }
}
