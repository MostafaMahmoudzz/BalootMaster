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
            Debug.Log($"[AIPlayer] {this.Name} is bidding in {evt.Round}");
            Bid aiBid = MakeBidDecision(evt.HighestBid);
            Debug.Log($"[AIPlayer] {this.Name} decided to bid: {aiBid.ToString()}");
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
        
        Debug.Log($"[AIPlayer] {this.Name} Round 1: Trump already chosen = {trumpAlreadyChosen}, Face-up suit = {faceUpSuit}");
        
        if (trumpAlreadyChosen)
        {
            // Trump already chosen - can only choose Sun or Pass
            // 40% chance to declare Sun (override)
            if (UnityEngine.Random.Range(0f, 1f) < 0.4f)
            {
                Debug.Log($"[AIPlayer] {this.Name} Round 1: Choosing Sun (override)");
                return Bid.CreateSun();
            }
            else
            {
                Debug.Log($"[AIPlayer] {this.Name} Round 1: Choosing Pass");
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
                Debug.Log($"[AIPlayer] {this.Name} Round 1: Choosing Sun");
                return Bid.CreateSun();
            }
            else if (random < 0.5f)
            {
                // 30% chance to take Trump (face-up suit)
                Debug.Log($"[AIPlayer] {this.Name} Round 1: Choosing Trump ({faceUpSuit})");
                return Bid.CreateTrump(faceUpSuit);
            }
            else
            {
                // 50% chance to pass
                Debug.Log($"[AIPlayer] {this.Name} Round 1: Choosing Pass");
                return Bid.CreatePass();
            }
        }
    }

    //--------------------------------------------------------------------
    private Bid MakeRound2Bid(BelootBiddingSystem biddingSystem, Bid highestBid)
    {
        // Round 2 Rules:
        // Case A: Someone chose Trump in Round 1
        //   - Trump taker: can choose Sun or Confirm Trump
        //   - Others: can only choose Sun or Pass
        // Case B: Everyone passed in Round 1  
        //   - All players: can choose Sun, Another Trump, or Pass
        //   - If Another Trump chosen: remaining players can only choose Sun or Pass
        
        bool isTrumpTaker = (biddingSystem.TrumpTaker == this);
        bool anotherTrumpChosen = biddingSystem.AnotherTrumpChosen;
        
        if (isTrumpTaker)
        {
            // Case A: Trump taker can confirm trump or switch to Sun
            float random = UnityEngine.Random.Range(0f, 1f);
            
            if (random < 0.7f)
            {
                // 70% chance to confirm Trump (face-up suit)
                return Bid.CreateTrump(biddingSystem.FaceUpCard.Family);
            }
            else
            {
                // 30% chance to switch to Sun
                return Bid.CreateSun();
            }
        }
        else if (anotherTrumpChosen)
        {
            // Another Trump was chosen - can only choose Sun or Pass
            // 50% chance to declare Sun (override)
            if (UnityEngine.Random.Range(0f, 1f) < 0.5f)
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
            // Case B: Can choose Sun, Another Trump, or Pass
            // Check if this is Case A (someone chose Trump in Round 1) or Case B (all passed)
            bool isCaseA = (biddingSystem.TrumpTaker != null);
            
            if (isCaseA)
            {
                // Case A: Non-trump taker can only choose Sun or Pass
                // 60% chance to declare Sun (override)
                if (UnityEngine.Random.Range(0f, 1f) < 0.6f)
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
                // Case B: Can choose Sun, Another Trump, or Pass
                float random = UnityEngine.Random.Range(0f, 1f);
                
                if (random < 0.3f)
                {
                    // 30% chance to declare Sun
                    return Bid.CreateSun();
                }
                else if (random < 0.6f)
                {
                    // 30% chance to choose Another Trump (different from face-up suit)
                    Card32Family faceUpSuit = biddingSystem.FaceUpCard.Family;
                    Card32Family[] suits = { Card32Family.Clubs, Card32Family.Diamond, Card32Family.Heart, Card32Family.Spade };
                    
                    // Find a suit different from face-up suit
                    Card32Family anotherSuit = faceUpSuit;
                    while (anotherSuit == faceUpSuit)
                    {
                        anotherSuit = suits[UnityEngine.Random.Range(0, suits.Length)];
                    }
                    
                    return Bid.CreateTrump(anotherSuit);
                }
                else
                {
                    // 40% chance to pass
                    return Bid.CreatePass();
                }
            }
        }
    }
}

