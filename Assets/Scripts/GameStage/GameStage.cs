using System;
using System.Collections.Generic;
using UnityEngine;
using Pebble;

//----------------------------------------------
// GameStage
//----------------------------------------------
// Purpose:
//   Core runtime logic for a Belote game round and turn management.
//   Handles players, dealing, bidding placeholder, turn flow, folds,
//   scoring, and rendering integration.
//
// How it connects to other scripts:
//   - Hosted by `BeloteGame` via `StageComponent<GameStage, GameStageDefinition>`.
//   - Uses `GameStageDefinition` for rules like dealing and scoring.
//   - Manages `Player` instances (`HumanPlayer`, `AIPlayer`).
//   - Uses `BeloteDeck`, `BeloteCard`, and `Fold` for card gameplay.
//   - Coordinates UI via `GameStageRenderer`.
//   - Broadcasts events (`NewRoundEvent`, `NewTurnEvent`) through `GameEventDispatcher`.
//----------------------------------------------
public class GameStage : Stage, IDeckOwner
{
    public enum EndState
    {
        None,
        Success, 
        Fail
    }

    //----------------------------------------------
    // Variables
    private List<Player>                       m_players;        // All players participating
    private BeloteDeck                         m_deck;           // The draw deck / stock
    private Fold                               m_currentFold;    // The active fold collecting played cards

    private List<Fold>[]                       m_pastFolds;      // Past folds per team index

    private ActionQueue                        m_actionQueue;    // Queues player/engine actions

    private GameStageRenderer                  m_renderer;       // UI layer for this stage

    private EndState m_endState;                                // Success/Fail/None state

    private static int s_invalidRoundCount = -1;                // Sentinel for uninitialized round

    private int m_currentRound = s_invalidRoundCount;           // Current round index


    private static float s_afterPlayDuration = 1.0f;            // Delay after each play to show UI
    private float m_afterPlayTimer = -1.0f;                      // Countdown for after-play processing

    public Score Score { get; set; }                             // Cumulative score across rounds

    //----------------------------------------------
    // Properties
    public Player CurrentPlayer
    {
        get; set;
    }

    public Player Dealer
    {
        get; set;
    }

    public Player RoundFirstPlayer
    {
        get; set;
    }

    public Player Bidder
    {
        get; set;
    }

    public List<Player> Players
    {
        get
        {
            return m_players;                                      // Expose players list
        }
    }

    public ActionQueue ActionQueue
    {
        get
        {
            return m_actionQueue;                                  // Expose action queue
        }
    }

    public bool HasEnded
    {
        get{ return !(m_endState == EndState.None); }              // Convenience flags
    }

    public bool Succeded
    {
        get { return m_endState == EndState.Success; }
    }

    public bool Failed
    {
        get { return m_endState == EndState.Fail; }
    }

    public Fold CurrentFold
    {
        get { return m_currentFold; }
    }

    public List<Fold>[] PastFolds
    {
        get { return m_pastFolds; }
    }

    public PlayerTeam? LastFoldingTeam
    {
        get; set;
    }

    public Fold LastFold
    {
        get 
        { 
            if(LastFoldingTeam != null && PastFolds[(int)LastFoldingTeam].Count > 0)
            {
                return PastFolds[(int)LastFoldingTeam].Last();    // Last completed fold of the last folding team
            }
            return null;
        }
    }

    public new GameStageDefinition Definition
    {
        get 
        { 
            return base.Definition as GameStageDefinition;         // Strongly-typed definition
        }
    }

    
    public Card32Family Trump {get; set; }                         // Trump family for the current round

    //----------------------------------------------
    public GameStage()
    {
        m_players = new List<Player>();                            // Initialize containers
        m_actionQueue = new ActionQueue();
        m_endState = EndState.None;
        m_deck = new BeloteDeck(this);                             // Main deck owned by stage
        m_currentFold = new Fold();                                // Start with an empty fold
        m_pastFolds = new List<Fold>[Enum.GetValues(typeof(PlayerTeam)).Length];
        m_renderer = new GameStageRenderer();                      // UI renderer instance

        for(int i = 0; i < m_pastFolds.Length; ++i)
        {
            m_pastFolds[i] = new List<Fold>();                    // Prepare storage per team
        }

        Score = new Score();                                       // Global score
    }

    //----------------------------------------------
    protected override void OnInit()
    {
        m_renderer.Stage = this;                                   // Bind renderer to this stage
        m_renderer.Init();                                         // Initialize UI

        m_deck.Init(Definition.Scoring);                           // Create a Belote deck using scoring data

        GameEventDispatcher.Subscribe<BeloteCard.Played>(this.OnCardPlayed); // Listen to plays

        AddPlayers();                                              // Create players for the match
    }

    
    protected override void OnShutdown()
    {
        m_renderer.Shutdown();                                     // Cleanup UI

        GameEventDispatcher.UnSubscribe<BeloteCard.Played>(this.OnCardPlayed); // Stop listening

        foreach (Player player in m_players)
        {
            player.Shutdown();                                     // Let players cleanup
        }
        m_players.Clear();

        m_deck.Clear();                                            // Return all cards to deck and clear

        
    }


    //----------------------------------------------
    protected override void OnStart() 
    {
        m_deck.Shuffle();                                          // Shuffle before dealing

        StartRound();                                              // Begin the first round
    }

    protected override void OnStop()
    {
        // TODO : recompute deck                                        // Placeholder for cleanup if needed
    }

    //----------------------------------------------
    protected override void OnGUI()
    {
        m_renderer.UpdateGUI();                                    // Draw GUI via renderer
    }

    //----------------------------------------------
    protected override void OnUpdate()
    {
        UpdatePlayers();                                           // Per-frame player updates

        if(m_afterPlayTimer >= 0.0f)                               // Handle post-play delay
        {
            m_afterPlayTimer -= Time.deltaTime;
            if(m_afterPlayTimer <= 0.0f)
            {
                m_afterPlayTimer = -1.0f;
                OnAfterPlayTimerDone();                            // Continue flow after delay
            }
        }


        if(CurrentPlayer != null)                                  // Process queued actions only if a player is active
        {
            m_actionQueue.Process();
        }

        m_renderer.Update();                                       // Update non-GUI renderer logic
    }

    //-------------------------GameStag--------------
    protected void SetEndState(EndState state)
    {
        if(m_endState != state)
        {
            m_endState = state;                                    // Update end state once
        }
    }

    //----------------------------------------------
    protected void AddPlayer<PlayerType>(PlayerTeam team, PlayerPosition position, string name)  where PlayerType : Player, new()
    {
        PlayerType newPlayer = new PlayerType();                   // Create
        newPlayer.Stage = this;                                    // Wire back-reference
        newPlayer.Team = team;                                     // Assign team
        newPlayer.Name = name;                                     // Assign display name
        newPlayer.Position = position;                             // Seat position
        newPlayer.Init();                                          // Initialize hooks
        m_players.Add(newPlayer);                                  // Register
        
    }

    //----------------------------------------------
    protected void AddPlayers()
    {
        AddPlayer<HumanPlayer>(PlayerTeam.Team1, PlayerPosition.South, "South"); // Human at South
        AddPlayer<AIPlayer>(PlayerTeam.Team2, PlayerPosition.West, "West");     // AI at West
        AddPlayer<AIPlayer>(PlayerTeam.Team1, PlayerPosition.North, "North");   // AI at North
        AddPlayer<AIPlayer>(PlayerTeam.Team2, PlayerPosition.East, "East");     // AI at East
    }

    protected Player GetLeftPlayer(Player player)
    {
        if(m_players.Count > 0)
        {
            if(player != null)
            {
                 int index = m_players.IndexOf(player);            // Find current index
                 index = (index + 1)% m_players.Count;             // Move one seat to the left
                 return m_players[index];
            }
            return m_players[0];                                   // Default to first player
        }
        return null;
    }

    protected void DealCards()
    {
        // TODO : Cut
        // New dealer is the left player of the current player
        Dealer = GetLeftPlayer(Dealer);                            // Rotate dealer each round
        RoundFirstPlayer = GetLeftPlayer(Dealer);                   // First to play after dealer
   
        for(int  iDeal = 0; iDeal < Definition.DealingRules.Dealings.Count; ++iDeal)
        {
            int dealing = Definition.DealingRules.Dealings[iDeal];

            Player player = RoundFirstPlayer;
            do
            {
                m_deck.MoveCardsTo(dealing, player.Hand);          // Deal a block to current player
                player = GetLeftPlayer(player);                    // Next player clockwise
            }
            while(player != RoundFirstPlayer);
        }

        foreach (Player player in m_players)
        {
            player.Hand.SortByFamilyAndValue(null);                // Sort with no trump known yet
        }
    }

    //----------------------------------------------
    void StartRound()
    {
        m_currentRound++;                                          // Advance round index

        DealCards();                                               // Distribute cards

        // TODO : Bidding round, Random Trump for now
        // TODO : Bidder
        Bidder = RoundFirstPlayer;                                 // Temporary bidder placeholder
        Trump = (Card32Family) UnityEngine.Random.Range(0, Enum.GetValues(typeof(Card32Family)).Length); // Random trump
        foreach (Player player in m_players)
        {
            player.Hand.SortByFamilyAndValue(Trump);               // Resort with known trump
        }

        NewRoundEvent evt = Pools.Claim<NewRoundEvent>();          // Announce round start
        evt.Start = true;
        GameEventDispatcher.SendEvent(evt);

        StartTurn(RoundFirstPlayer);                               // First turn goes to first player
    }

    Score m_roundScore = new Score();
    void EndRound()
    {
        m_roundScore.Reset();                                      // Compute points from folds
    
        for(int i = 0; i < m_pastFolds.Length; ++i)
        {
            PlayerTeam team = (PlayerTeam) i;
            List<Fold> folds = m_pastFolds[i];
            foreach(Fold fold in folds)
            {
                m_roundScore.AddScore(team, fold.Points);          // Sum points of each fold
                fold.Deck.MoveAllCardsTo(m_deck);                   // Return cards to deck
            }
        }

        PlayerTeam winningTeam = m_roundScore.GetLeadingTeam(Bidder.Team); // Determine round winner

        // TODO : Round points
        // TODO : Bet
        Score.AddScore(winningTeam, m_roundScore.GetScore(winningTeam)); // Add round points to global score

        // 10 de der
        if(LastFoldingTeam != null)
        {
            Score.AddScore((PlayerTeam)LastFoldingTeam, 10);       // Last trick bonus
        }
        NewRoundEvent evt = Pools.Claim<NewRoundEvent>();          // Announce round end
        evt.Start = false;
        GameEventDispatcher.SendEvent(evt);
    }

    //----------------------------------------------
    void StartTurn(Player player)
    {
        Player previous = CurrentPlayer;                           // Preserve previous for event
        CurrentPlayer = player;                                    // Swap to new current player

        NewTurnEvent evt = Pools.Claim<NewTurnEvent>();            // Broadcast turn change
        evt.Current = CurrentPlayer;
        evt.Previous = previous;
        GameEventDispatcher.SendEvent(evt);
    }

    //----------------------------------------------
    protected  void UpdatePlayers()
    {
        foreach (Player player in m_players)
        {
            player.Update();                                       // Delegate per-player logic
        }
    }

    protected void OnCardPlayed(BeloteCard.Played evt)
    {
        m_afterPlayTimer = s_afterPlayDuration;                    // Start post-play cooldown
    }

    protected void OnAfterPlayTimerDone()
    {
        // One Fold is done, select new player.
        if(CurrentFold.Deck.Size == Players.Count)
        {
            CurrentFold.Finalize(Trump);                           // Evaluate winner and points

            Player winner = CurrentFold.Winner;                    // Winner leads next fold
            LastFoldingTeam = winner.Team;

            Fold newFold = new Fold();                             // Archive current fold and start a new one
            CurrentFold.MoveTo(newFold);
            PastFolds[(int)winner.Team].Add(newFold);

            // New player has no cards in hand, we end the round
            if(winner.Hand.Empty)
            {
                // Next Round;
                EndRound();                                        // Score and cleanup round
                // TODO : Win condition
                StartRound();                                      // Begin next round
            }
            else
            {
                StartTurn(winner);                                 // Winner leads next
            }
        }
        else
        {
            StartTurn(GetLeftPlayer(CurrentPlayer));                // Next player clockwise
        }
    }

    //------------------------------------
    // Events
    public class NewRoundEvent : PooledEvent
    {
        public bool Start { get; set;}                             // True when round starts, false when it ends
        public override void Reset()
        {
            Start = true;                                          // Default to start
        }
    }

    public class NewTurnEvent : PooledEvent
    {
        public Player Current { get; set;}                         // New current player
        public Player Previous { get; set;}                        // Player who just finished
        public override void Reset()
        {

        }
    }
}




