using System;
using UnityEngine;

//-------------------------------------------------------
// AIPlayer
//-------------------------------------------------------
// Purpose:
//   Simple AI-controlled player. Currently plays a random legal card
//   at the start of its turn.
//
// How it connects to other scripts:
//   - Inherits from `Player` and uses `TurnPlayableCards` computed by
//     the base class to decide valid actions.
//   - Uses `Stage.CurrentFold` to submit plays.
//-------------------------------------------------------
public class AIPlayer : Player
{
    public AIPlayer()
    {

    }

    //----------------------------------------------
    protected override void OnInit()
    {
        GameEventDispatcher.Subscribe<BiddingTurnEvent>(this.OnBiddingTurn); // Listen to bidding turns
    }

    //--------------------------------------------------------------------
    protected override void OnShutdown()
    {
        GameEventDispatcher.UnSubscribe<BiddingTurnEvent>(this.OnBiddingTurn); // Cleanup bidding listener
    }

    //--------------------------------------------------------------------
    protected override void OnTurnStart() 
    {
        PlayAtRandom(); // Naive policy: random choice among legal cards
    }

    //--------------------------------------------------------------------
    protected override void OnTurnStop() 
    {

    }

    //--------------------------------------------------------------------
    void PlayAtRandom()
    {
        if(TurnPlayableCards != null && ! TurnPlayableCards.Empty)
        {
            int indexToPlay = UnityEngine.Random.Range(0, TurnPlayableCards.Size); // Pick a random index
            Play(TurnPlayableCards.Cards[indexToPlay], Stage.CurrentFold);         // Play selected card
        }
         
    }

    //--------------------------------------------------------------------
    private void OnBiddingTurn(BiddingTurnEvent evt)
    {
        if(evt.CurrentBidder == this)
        {
            // AI's turn to bid - make a simple decision
            Bid aiBid = MakeBidDecision(evt.HighestBid);
            SubmitBid(aiBid);
        }
    }

    //--------------------------------------------------------------------
    private Bid MakeBidDecision(Bid highestBid)
    {
        // Simple AI bidding strategy
        // 1. If no one has bid yet, bid on a random suit
        // 2. If someone has bid, either pass or bid higher
        // 3. 70% chance to pass if there's already a bid

        if (highestBid == null || highestBid.IsPass)
        {
            // No one has bid yet - bid on a random suit
            Card32Family[] suits = { Card32Family.Clubs, Card32Family.Diamonds, Card32Family.Hearts, Card32Family.Spades };
            Card32Family randomSuit = suits[UnityEngine.Random.Range(0, suits.Length)];
            return Bid.CreateBid(randomSuit);
        }
        else
        {
            // Someone has already bid - 70% chance to pass
            if (UnityEngine.Random.Range(0f, 1f) < 0.7f)
            {
                return Bid.CreatePass();
            }
            else
            {
                // Try to bid higher
                Card32Family[] suits = { Card32Family.Clubs, Card32Family.Diamonds, Card32Family.Hearts, Card32Family.Spades };
                Card32Family randomSuit = suits[UnityEngine.Random.Range(0, suits.Length)];
                Bid newBid = Bid.CreateBid(randomSuit);
                
                // If our bid isn't higher, just pass
                if (!newBid.IsHigherThan(highestBid))
                {
                    return Bid.CreatePass();
                }
                
                return newBid;
            }
        }
    }
}

