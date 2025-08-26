using System.Collections.Generic;
using UnityEngine;
using Pebble;

//-------------------------------------------------------
// Fold
//-------------------------------------------------------
// Purpose:
//   Represents a single trick (set of one card played by each player).
//   Tracks the cards, determines the winner and aggregates trick points.
//
// How it connects to other scripts:
//   - Used by `GameStage` to collect plays and decide turn flow.
//   - Uses `BeloteCard.GetBestCard` and scoring to compute winner/points.
//-------------------------------------------------------
public class Fold
{
    public BeloteDeck Deck { get; set;}           // Cards played this trick

    public Player Winner { get; set; }            // Player who won this trick

    public int Points { get; set; }               // Total points of the trick

    public Card32Family? RequestedFamily
    {
        get
        {
             if(Deck.Cards.Count > 0)
             {
                 return Deck.Cards.Front().Family; // Suit of the first played card
             }   
             return null;
        }
    }

    public Fold()
    {
        Deck = new BeloteDeck();                 // Own a deck to store played cards
    }

    public void MoveTo(Fold fold)
    {
        Deck.MoveAllCardsTo(fold.Deck);          // Transfer cards to another fold instance
        fold.Winner = Winner;                    // Copy metadata
        fold.Points = Points;
        Winner = null;                           // Reset this fold
        Points = 0;
    }

    public void Finalize(Card32Family trumpFamily)
    {
        BeloteCard bestCard = GetBest(trumpFamily); // Determine best card by rules
        if(bestCard != null)
        {
            Winner = bestCard.Owner as Player;      // Winner owns the best card
            Points = GetPoints(trumpFamily);        // Sum points for this trick
        }
    }

    public BeloteCard GetBest(Card32Family trumpFamily)
    {
        Card32Family? requested = RequestedFamily;
        if(requested != null)
        {
            BeloteCard bestCard = Deck.Cards[0];   // Start with first card
            if(Deck.Cards.Count > 1)
            {
                for(int  i = 1; i < Deck.Cards.Count ; ++i)
                {
                    BeloteCard card = Deck.Cards[i];

                    bestCard = BeloteCard.GetBestCard(card, bestCard, trumpFamily); // Update winner
                }    
            }
            return bestCard;
        }
        return null;
    }

    public int GetPoints(Card32Family trumpFamily)
    {
        int points = 0;
        foreach(BeloteCard card in Deck.Cards)
        {
            points += card.GetPoint(trumpFamily);   // Sum trump-adjusted points
        }
        return points;
    }
}