using System;
using UnityEngine;
using Pebble;

//-------------------------------------------------------
// Bid
//-------------------------------------------------------
// Purpose:
//   Represents a single bid made by a player during bidding round.
//   Supports Trump, Sun (No Trump), and Pass contract types.
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
    // Contract Types
    public enum ContractType
    {
        Pass,           // Player passes
        Trump,          // Player chooses trump suit
        Sun             // No trump (Sun contract)
    }

    //----------------------------------------------
    // Variables
    public ContractType Type { get; set; }          // Type of contract
    public Card32Family Suit { get; set; }         // Trump suit being bid on (if Trump)
    public int BidValue { get; set; }              // Bid value (for future scoring)

    //----------------------------------------------
    // Properties
    public bool IsPass
    {
        get { return Type == ContractType.Pass; }
    }

    public bool IsTrump
    {
        get { return Type == ContractType.Trump; }
    }

    public bool IsSun
    {
        get { return Type == ContractType.Sun; }
    }

    public string DisplayName
    {
        get
        {
            switch (Type)
            {
                case ContractType.Pass:
                    return "Pass";
                case ContractType.Sun:
                    return "Sun";
                case ContractType.Trump:
                    return Suit.ToString();
                default:
                    return "Unknown";
            }
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
            case Card32Family.Spade:
                return 4;
            case Card32Family.Heart:
                return 3;
            case Card32Family.Diamond:
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
