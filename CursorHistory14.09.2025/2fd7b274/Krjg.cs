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
        Type = ContractType.Pass;
        Suit = Card32Family.Clubs; // Default suit
        BidValue = 0;
    }

    //-------------------------------------------------------
    public Bid(ContractType type, Card32Family suit = Card32Family.Clubs)
    {
        Type = type;
        Suit = suit;
        BidValue = GetSuitValue(suit);
    }

    //-------------------------------------------------------
    public static Bid CreatePass()
    {
        return new Bid(ContractType.Pass);
    }

    //-------------------------------------------------------
    public static Bid CreateTrump(Card32Family suit)
    {
        return new Bid(ContractType.Trump, suit);
    }

    //-------------------------------------------------------
    public static Bid CreateSun()
    {
        return new Bid(ContractType.Sun);
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

        // Sun always wins
        if (IsSun)
        {
            return true;
        }

        if (other.IsSun)
        {
            return false;
        }

        // Trump beats Pass
        if (IsTrump && other.IsPass)
        {
            return true;
        }

        if (IsPass)
        {
            return false;
        }

        // Trump vs Trump comparison
        if (IsTrump && other.IsTrump)
        {
            return BidValue > other.BidValue;
        }

        return false;
    }

    //-------------------------------------------------------
    public override string ToString()
    {
        return DisplayName;
    }
}
