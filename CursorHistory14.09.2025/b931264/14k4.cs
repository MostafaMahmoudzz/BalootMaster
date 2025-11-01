using System.Collections.Generic;
using UnityEngine;
using Pebble;

//----------------------------------------------
// HumanPlayer
//----------------------------------------------
// Purpose:
//   Human-controlled player. Listens to card selection events coming
//   from the UI (`CardComponent`) and plays legal cards accordingly.
//
// How it connects to other scripts:
//   - Inherits from `Player` and uses its turn and hand logic.
//   - Subscribes to `BeloteCard.Selected` raised by UI.
//   - Uses `Stage.CurrentFold` to play selected cards.
//----------------------------------------------
public class HumanPlayer : Player
{


    //----------------------------------------------
    public HumanPlayer()
    {
    }

    //----------------------------------------------
    protected override void OnInit()
    {
        GameEventDispatcher.Subscribe<BeloteCard.Selected>(this.OnCardSelectedEvent); // Listen to UI selection
        GameEventDispatcher.Subscribe<BiddingTurnEvent>(this.OnBiddingTurn); // Listen to bidding turns
    }

    //--------------------------------------------------------------------
    protected override void OnShutdown()
    {
        GameEventDispatcher.UnSubscribe<BeloteCard.Selected>(this.OnCardSelectedEvent); // Cleanup listener
        GameEventDispatcher.UnSubscribe<BiddingTurnEvent>(this.OnBiddingTurn); // Cleanup bidding listener
    }

    private void OnCardSelectedEvent(BeloteCard.Selected evt)
    {
        if(evt.IsSelected == false && evt.OutsideOfHand) // Card released outside hand area means "play"
        {
            Play(evt.Card, Stage.CurrentFold); // Delegate legality to base `Play`
        }
    }

    private void OnBiddingTurn(BiddingTurnEvent evt)
    {
        if(evt.CurrentBidder == this)
        {
            // Human player's turn to bid - UI will handle this
            // The UI will call SubmitBid() when user makes a choice
        }
    }

    // Override SubmitBid to handle UI feedback
    public override void SubmitBid(Bid bid)
    {
        base.SubmitBid(bid);
        Debug.Log($"{Name} bid: {bid.DisplayName}");
    }
}

