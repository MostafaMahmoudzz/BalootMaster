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
       
    }

    //--------------------------------------------------------------------
    protected override void OnShutdown()
    {
        
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
}

