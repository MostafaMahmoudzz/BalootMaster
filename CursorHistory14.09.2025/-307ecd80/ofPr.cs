using Pebble;
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
        // Get current bidding system to understand context
        BelootBiddingSystem biddingSystem = Stage.BiddingSystem;
        if (biddingSystem == null)
        {
            return Bid.CreatePass();
        }

        // Simple AI bidding strategy for Beloot rules
        if (biddingSystem.CurrentRound == BelootBiddingSystem.BiddingRound.Round1)
        {
            return MakeRound1Bid(biddingSystem, highestBid);
        }
        else // Round 2
        {
            return MakeRound2Bid(biddingSystem, highestBid);
        }
    }

    //--------------------------------------------------------------------
    private Bid MakeRound1Bid(BelootBiddingSystem biddingSystem, Bid highestBid)
    {
        // Round 1 Rules:
        // - Can choose Sun, Trump (face-up suit), or Pass
        // - If Trump was already chosen by someone, can only choose Sun or Pass
        
        Card32Family faceUpSuit = biddingSystem.FaceUpCard.Family;
        
        // Check if Trump was already chosen by someone
        bool trumpAlreadyChosen = (highestBid != null && highestBid.IsTrump);
        
        if (trumpAlreadyChosen)
        {
            // Trump already chosen - can only choose Sun or Pass
            // 40% chance to declare Sun (override)
            if (UnityEngine.Random.Range(0f, 1f) < 0.4f)
            {
                return Bid.CreateSun();
            }
            else
            {
                return Bid.CreatePass();
            }
        }
        else
        {
            // No Trump chosen yet - can choose Sun, Trump, or Pass
            float random = UnityEngine.Random.Range(0f, 1f);
            
            if (random < 0.2f)
            {
                // 20% chance to declare Sun
                return Bid.CreateSun();
            }
            else if (random < 0.5f)
            {
                // 30% chance to take Trump (face-up suit)
                return Bid.CreateTrump(faceUpSuit);
            }
            else
            {
                // 50% chance to pass
                return Bid.CreatePass();
            }
        }
    }

    //--------------------------------------------------------------------
    private Bid MakeRound2Bid(BelootBiddingSystem biddingSystem, Bid highestBid)
    {
        if (biddingSystem.TrumpTaker == this)
        {
            // Case A: Trump taker can confirm trump or switch to Sun
            if (highestBid == null)
            {
                // 60% chance to confirm trump, 40% chance to switch to Sun
                if (UnityEngine.Random.Range(0f, 1f) < 0.6f)
                {
                    // Confirm trump with face-up suit
                    return Bid.CreateTrump(biddingSystem.FaceUpCard.Family);
                }
                else
                {
                    return Bid.CreateSun();
                }
            }
            else
            {
                return Bid.CreatePass();
            }
        }
        else
        {
            // Case B: Other players can choose different trump, Sun, or Pass
            if (highestBid == null)
            {
                // 25% chance to choose different trump
                if (UnityEngine.Random.Range(0f, 1f) < 0.25f)
                {
                    Card32Family[] suits = { Card32Family.Clubs, Card32Family.Diamond, Card32Family.Heart, Card32Family.Spade };
                    Card32Family randomSuit = suits[UnityEngine.Random.Range(0, suits.Length)];
                    
                    // Make sure it's different from face-up card
                    while (randomSuit == biddingSystem.FaceUpCard.Family)
                    {
                        randomSuit = suits[UnityEngine.Random.Range(0, suits.Length)];
                    }
                    
                    return Bid.CreateTrump(randomSuit);
                }
                
                // 30% chance to declare Sun
                if (UnityEngine.Random.Range(0f, 1f) < 0.3f)
                {
                    return Bid.CreateSun();
                }
            }
            else
            {
                // Someone has already bid - 40% chance to declare Sun
                if (UnityEngine.Random.Range(0f, 1f) < 0.4f)
                {
                    return Bid.CreateSun();
                }
            }
            
            // Otherwise pass
            return Bid.CreatePass();
        }
    }
}

