using System.Collections;
using UnityEngine;
using Pebble;

//----------------------------------------------
// BeloteCard
//----------------------------------------------
// Purpose:
//   Specialized 32-card deck card for Belote with normal and trump
//   point values, spawn capability, and utility methods.
//
// How it connects to other scripts:
//   - Extends `Card32` (value/family) used across card systems.
//   - Uses `CardStaticData` to spawn a `CardComponent` view.
//   - Raises `Played` events consumed by `GameStage`.
//   - Compared with other cards using `GetBestCard` by `Fold` logic.
//----------------------------------------------
public partial class BeloteCard  : Card32
{
    //----------------------------------------------
    // Variables

    public int Point { get; set; }            // Points when not trump
    public int TrumpPoint { get; set; }       // Points when card family is trump

    //----------------------------------------------
    public BeloteCard()
    {
    }

    //----------------------------------------------
    public CardComponent Spawn()
    {
        if (CardStaticData.Instance.Prefab != null)             // Ensure a prefab is configured
        {
            GameObject cardObj = Object.Instantiate(CardStaticData.Instance.Prefab) as GameObject; // Create view
            CardComponent cardComp = cardObj.GetComponent<CardComponent>();
            if(cardComp != null)
            {
                cardComp.Init(this);                             // Bind this model to the view
            }
            return cardComp;                                     // Return component for further manipulation
        }
        return null;                                              // No prefab configured
    }

    public void OnPlay()
    {
        Played evt = Pools.Claim<Played>();                      // Allocate pooled event
        evt.Init(this);                                          // Attach payload
        GameEventDispatcher.SendEvent(evt);                      // Notify listeners (e.g., `GameStage`)
    }

    public int GetPoint(Card32Family? trumpFamily)
    {
        if(trumpFamily != null && Family == trumpFamily)         // Trump family uses trump points
        {
            return TrumpPoint;
        }
        return Point;                                            // Otherwise normal points
    }

    public override string ToString()
    {
        return "(" + Value + " " + Family + ")";              // Human-readable identifier
    }

     public static BeloteCard GetBestCard(BeloteCard a, BeloteCard b, Card32Family trumpFamily)
    {
        if(a.Family == b.Family)                                 // Same suit: compare points, then value
        {
            int aCardPoint = a.GetPoint(trumpFamily);
            int bCardpoint = b.GetPoint(trumpFamily);
            if(aCardPoint > bCardpoint)
            {
                return a;
            }
            else if(aCardPoint == bCardpoint) // Same point, value wins
            {
                if(a.Value > b.Value)
                {
                    return a;
                }
            }
        }
        else
        {
            if(a.Family == trumpFamily)                          // Different suit: trump beats non-trump
            {
                return a;
            }     
        }
        return b;
    }
}
