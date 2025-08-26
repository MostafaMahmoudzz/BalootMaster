using System.Collections.Generic;
using Pebble;

//----------------------------------------------
// Player
//----------------------------------------------
// Purpose:
//   Base class for all players (human and AI). Manages the player's
//   hand (`BeloteDeck`), turn flow, and permissible actions.
//
// How it connects to other scripts:
//   - `GameStage` sets up players, dispatches turn events, and queries
//     playable cards via `ComputePlayableCards`.
//   - `BeloteDeck` stores cards in hand and exposes movement helpers.
//   - `BeloteCard` instances are played into a `Fold`.
//   - `GameEventDispatcher` notifies players about new turns.
//   - Derived classes (`AIPlayer`, `HumanPlayer`) implement behavior by
//     overriding `OnInit`, `OnUpdate`, `OnTurnStart`, `OnTurnStop`.
//----------------------------------------------
public class Player : IDeckOwner
{

    //----------------------------------------------
    // Variables
    protected GameStage m_stage;            // Reference to current `GameStage` controlling the flow
    private   BeloteDeck m_hand;            // The player's hand (owned deck)

    private bool m_isAllowedToPlay = false; // True only during the player's turn

    //----------------------------------------------
    // Properties

    public GameStage Stage
    {
        get
        {
            return m_stage;                 // Expose current stage to users/derived classes
        }
        set
        {
            m_stage = value;                // Set by `GameStage` during player creation
        }
    }

    public PlayerTeam Team { get; set; }

    public BeloteDeck Hand
    {
        get
        {
            return m_hand;                  // Read-only accessor to the hand deck
        }
    }

    public BeloteDeck TurnPlayableCards
    {
        get; set;
    }

    public string Name { get; set; }

    public PlayerPosition Position { get; set; }

    //----------------------------------------------
    public Player()
    {
        m_hand = new BeloteDeck(this);      // Hand is owned by this player
    }

    //----------------------------------------------
    public void Init()
    {
        GameEventDispatcher.Subscribe<GameStage.NewTurnEvent>(this.OnNewTurn); // Listen to turn changes

        OnInit();                           // Allow derived classes to initialize
    }

    //----------------------------------------------
    protected virtual void OnInit()
    {

    }

    //--------------------------------------------------------------------
    public void Shutdown()
    {
        OnShutdown();                       // Derived cleanup
        GameEventDispatcher.UnSubscribe<GameStage.NewTurnEvent>(this.OnNewTurn); // Stop listening
    }

    //--------------------------------------------------------------------
    protected virtual void OnShutdown()
    {

    }

   
    //----------------------------------------------
    public void Update()
    {
        OnUpdate();                         // Per-frame update hook
    }

    //----------------------------------------------
    protected virtual void OnUpdate()
    {

    }

    //----------------------------------------------
    protected void Play(BeloteCard card, Fold fold)
    {
        if (CanPlay(card))                  // Guard: only play legal cards
        {
            DoPlay(card, fold);             // Execute the play
        }
    }

    //----------------------------------------------
    public bool CanPlay(BeloteCard card)
    {
        if (m_isAllowedToPlay && Hand.Contains(card)) // Only if it's our turn and we own the card
        {
            if(TurnPlayableCards != null && TurnPlayableCards.Contains(card))
                return true;                // The card is part of the precomputed legal set
        }
        return false;
    }

    //----------------------------------------------
    protected void DoPlay(BeloteCard card, Fold fold)
    {
        m_hand.MoveCardTo(card, fold.Deck); // Move card from hand to the active fold
        card.OnPlay();                      // Raise card played event
    }

    List<BeloteCard>  m_trumpCards = new List<BeloteCard> ();
    List<BeloteCard>  m_trumpBetterCards = new List<BeloteCard> ();

    protected BeloteDeck ComputePlayableCards(Fold fold, Card32Family trumpFamily)
    {
        BeloteDeck playables = new BeloteDeck(); // Result deck of legal cards

        m_trumpCards.Clear();
        m_trumpBetterCards.Clear();

        if(!Hand.Empty)
        {
            // No cards in the fold, all cards are valid
            if(fold.RequestedFamily == null)
            {
                playables.CopyFrom(Hand);   // First player: any card
            }
            else
            {
                BeloteCard bestCard = fold.GetBest(trumpFamily); // Current winning card in the fold
                Player bestPlayer = bestCard.Owner as Player;     // Player who currently wins the fold

                Card32Family requestedFamily = (Card32Family)fold.RequestedFamily;

                // We look for cards of the requested families
                foreach(BeloteCard card in Hand.Cards)
                {
                    if(card.Family == requestedFamily)
                    {
                        playables.AddCard(card); // Must follow suit if possible
                    }

                    if(card.Family == trumpFamily)
                    {
                        m_trumpCards.Add(card); // Track all trumps

                        if(bestCard.Family == trumpFamily)
                        {
                            if(BeloteCard.GetBestCard(card, bestCard, trumpFamily) == card)
                            {
                                m_trumpBetterCards.Add(card); // Trumps that overtake best
                            }
                        }
                    }
                }

                // Remove all trump cards that are too low
                if(!playables.Empty && trumpFamily == requestedFamily)
                {
                    if(m_trumpBetterCards.Count > 0)
                    {
                        playables.Clear();             // If following trump, must overtrump when possible
                        playables.AddCards(m_trumpBetterCards);
                    }
                }


                // No card of the requested family
                if(playables.Empty)
                {
                    // Best card is partner we can play what we want
                    if(bestPlayer.Team == this.Team)
                    {
                        playables.CopyFrom(Hand); // Partner leads: free play
                    }
                    else
                    {
                        if(bestCard.Family == trumpFamily)
                        {
                            if(m_trumpBetterCards.Count > 0)
                            {
                                playables.AddCards(m_trumpBetterCards); // Must overtrump if possible
                            }
                            else // TODO : Add "pisser" rules
                            {
                                playables.AddCards(m_trumpCards); // Otherwise any trump
                            }
                        }
                        else
                        {
                            playables.AddCards(m_trumpCards); // No suit: may cut with trump
                        }

                        if(playables.Empty)
                        {
                            playables.CopyFrom(Hand); // Still empty: truly free play
                        }    
                    }
                }
            }
        }
        return playables;
    }

    //----------------------------------------------
    private void OnNewTurn(GameStage.NewTurnEvent evt)
    {
       if(evt.Previous == this)
       {
           m_isAllowedToPlay = false;       // Our turn ended
           OnTurnStop();
           TurnPlayableCards = null;        // Clear cached legal moves
       }

       if(evt.Current == this)
       {
           m_isAllowedToPlay = true;        // Our turn starts

           TurnPlayableCards = ComputePlayableCards(Stage.CurrentFold, Stage.Trump); // Precompute legal moves
           OnTurnStart();
       }
    }

    protected virtual void OnTurnStart() {}
    protected virtual void OnTurnStop() {}

    public void PrintHand()
    {
        Hand.Print(Name);                   // Debug helper: print hand to logs/console
    }
}
