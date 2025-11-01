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

    private BelootBiddingSystem               m_biddingSystem;  // Bidding system for trump selection

    private EndState m_endState;                                // Success/Fail/None state

    private static int s_invalidRoundCount = 0;                 // Sentinel for uninitialized round (now starts from 0)

    private int m_currentRound = s_invalidRoundCount;           // Current round index (starts from 0, first round will be 1)


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

    
    public Card32Family? Trump {get; set; }                        // Trump family for the current round (null for Sun contract)

    public BeloteCard FaceUpCard { get; set; }                     // Face-up card revealed for bidding

    public BelootBiddingSystem BiddingSystem
    {
        get { return m_biddingSystem; }
    }

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
        m_biddingSystem = new BelootBiddingSystem();               // Bidding system instance

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
        GameEventDispatcher.Subscribe<BiddingCompleteEvent>(this.OnBiddingComplete); // Listen to bidding completion
        GameEventDispatcher.Subscribe<BiddingNoBidsEvent>(this.OnBiddingNoBids); // Listen to no bids event

        AddPlayers();                                              // Create players for the match
    }

    
    protected override void OnShutdown()
    {
        m_renderer.Shutdown();                                     // Cleanup UI

        GameEventDispatcher.UnSubscribe<BeloteCard.Played>(this.OnCardPlayed); // Stop listening
        GameEventDispatcher.UnSubscribe<BiddingCompleteEvent>(this.OnBiddingComplete); // Stop listening
        GameEventDispatcher.UnSubscribe<BiddingNoBidsEvent>(this.OnBiddingNoBids); // Stop listening

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
        AddPlayer<HumanPlayer>(PlayerTeam.Team2, PlayerPosition.West, "West");     // AI at West
        AddPlayer<HumanPlayer>(PlayerTeam.Team1, PlayerPosition.North, "North");   // AI at North
        AddPlayer<HumanPlayer>(PlayerTeam.Team2, PlayerPosition.East, "East");     // AI at East
    }

    protected Player GetRightPlayer(Player player)
    {
        if(m_players.Count > 0)
        {
            if(player != null)
            {
                 int index = m_players.IndexOf(player);            // Find current index
                 index = (index - 1 + m_players.Count) % m_players.Count; // Move one seat to the right (anti-clockwise)
                 return m_players[index];
            }
            return m_players[0];                                   // Default to first player
        }
        return null;
    }

    protected void DealCards()
    {
        Debug.Log($"[GameStage] === DEALING CARDS FOR ROUND {m_currentRound} ===");
        Debug.Log($"[GameStage] Deck size before dealing: {m_deck.Size}");
        
        // Initialize dealer if this is the first round
        if (Dealer == null)
        {
            Dealer = m_players[0]; // Start with first player as dealer
            Debug.Log($"[GameStage] Initializing dealer: {Dealer.Name}");
        }
        else
        {
            Debug.Log($"[GameStage] Previous dealer: {Dealer.Name}");
        }
        
        // TODO : Cut
        // New dealer is the right player of the current player (anti-clockwise)
        Player previousDealer = Dealer;
        Dealer = GetRightPlayer(Dealer);                            // Rotate dealer each round
        RoundFirstPlayer = GetRightPlayer(Dealer);                   // First to play after dealer
        
        Debug.Log($"[GameStage] Dealer rotation: {previousDealer?.Name} → {Dealer?.Name}");
        Debug.Log($"[GameStage] New dealer: {Dealer?.Name}, First player (bidder): {RoundFirstPlayer?.Name}");
        
        // Debug: Show the complete player order
        Debug.Log($"[GameStage] Player order (anti-clockwise):");
        for (int i = 0; i < m_players.Count; i++)
        {
            string marker = "";
            if (m_players[i] == Dealer) marker = " [DEALER]";
            if (m_players[i] == RoundFirstPlayer) marker = " [FIRST BIDDER]";
            Debug.Log($"[GameStage]   {i}: {m_players[i].Name}{marker}");
        }
   
        // Deal 3 cards to each player
        Debug.Log("[GameStage] Dealing 3 cards to each player...");
        DealCardsToPlayers(3);
        Debug.Log($"[GameStage] Deck size after dealing 3 cards: {m_deck.Size}");
        
        // Deal 2 more cards to each player (total 5)
        Debug.Log("[GameStage] Dealing 2 more cards to each player (total 5)...");
        DealCardsToPlayers(2);
        Debug.Log($"[GameStage] Deck size after dealing 2 more cards: {m_deck.Size}");

        foreach (Player player in m_players)
        {
            player.Hand.SortByFamilyAndValue(null);                // Sort with no trump known yet
            Debug.Log($"[GameStage] {player.Name} has {player.Hand.Size} cards");
        }
        
        Debug.Log("[GameStage] === CARD DEALING COMPLETE ===");
    }

    //-------------------------------------------------------
    private void DealCardsToPlayers(int cardsPerPlayer)
    {
        if (RoundFirstPlayer == null)
        {
            Debug.LogError("[GameStage] Cannot deal cards - RoundFirstPlayer is null!");
            return;
        }
        
        // Safety check: Ensure we have enough cards in deck
        int totalCardsNeeded = cardsPerPlayer * m_players.Count;
        if (m_deck.Size < totalCardsNeeded)
        {
            Debug.LogError($"[GameStage] Cannot deal {totalCardsNeeded} cards - only {m_deck.Size} cards available in deck!");
            return;
        }
        
        Debug.Log($"[GameStage] Dealing {cardsPerPlayer} cards to each of {m_players.Count} players (total: {totalCardsNeeded} cards)");

            Player player = RoundFirstPlayer;
            do
            {
            Debug.Log($"[GameStage] Dealing {cardsPerPlayer} cards to {player.Name} (deck has {m_deck.Size} cards)");
            m_deck.MoveCardsTo(cardsPerPlayer, player.Hand);       // Deal cards to current player
            Debug.Log($"[GameStage] {player.Name} now has {player.Hand.Size} cards (deck has {m_deck.Size} cards remaining)");
            player = GetRightPlayer(player);                       // Next player anti-clockwise
            }
            while(player != RoundFirstPlayer);
        }

    //-------------------------------------------------------
    private void DealRemainingCardsAfterContract(Player contractMaker, BeloteCard faceUpCard)
    {
        Debug.Log($"[GameStage] === DEALING REMAINING CARDS AFTER CONTRACT ===");
        Debug.Log($"[GameStage] Contract maker: {contractMaker.Name}");
        Debug.Log($"[GameStage] Face-up card: {(faceUpCard != null ? $"{faceUpCard.Value} of {faceUpCard.Family}" : "None")}");
        
        // Contract maker gets the face-up card + 2 additional cards (total 8)
        if (faceUpCard != null)
        {
            contractMaker.Hand.AddCard(faceUpCard);                // Give face-up card to contract maker
            faceUpCard.Owner = contractMaker;                      // Set ownership
            Debug.Log($"[GameStage] {contractMaker.Name} receives face-up card: {faceUpCard.Value} of {faceUpCard.Family}");
        }
        
        // Deal 2 additional cards to contract maker (since they already got the face-up card)
        m_deck.MoveCardsTo(2, contractMaker.Hand);
        Debug.Log($"[GameStage] {contractMaker.Name} receives 2 additional cards (total: {contractMaker.Hand.Size} cards)");
        
        // Deal 3 cards to all other players
        foreach (Player player in m_players)
        {
            if (player != contractMaker)
            {
                m_deck.MoveCardsTo(3, player.Hand);
                Debug.Log($"[GameStage] {player.Name} receives 3 additional cards (total: {player.Hand.Size} cards)");
            }
        }
        
        Debug.Log($"[GameStage] Contract completed - {contractMaker.Name} has {contractMaker.Hand.Size} cards total");
    }

    //----------------------------------------------
    void StartRound()
    {
        m_currentRound++;                                          // Advance round index
        Debug.Log($"[GameStage] === STARTING ROUND {m_currentRound} ===");
        Debug.Log($"[GameStage] StartRound() called - this should NOT happen after Trump confirmation!");
        Debug.Log($"[GameStage] Stack trace:");
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();
        for (int i = 1; i < Mathf.Min(stackTrace.FrameCount, 5); i++)
        {
            Debug.Log($"[GameStage]   {i}: {stackTrace.GetFrame(i).GetMethod().Name}");
        }

        // Only collect cards if this is not the first round (round 1)
        // Now that we start from 0, the first round will be 1, so we collect cards for round 2+
        if (m_currentRound > 1)
        {
            Debug.Log($"[GameStage] Collecting cards for Round {m_currentRound} (not first round)");
            Debug.Log($"[GameStage] This ensures all cards are returned to deck before new dealing");
            // Collect all cards from players and return to deck
            CollectAllCardsToDeck();
        }
        else
        {
            Debug.Log($"[GameStage] Skipping card collection for Round {m_currentRound} (first round)");
        }

        Debug.Log($"[GameStage] Dealing 3+2 cards to each player for Round {m_currentRound}");
        DealCards();                                               // Distribute cards

        // Reset bidding system for new round
        m_biddingSystem.Reset();
        Debug.Log($"[GameStage] Starting Round {m_currentRound} - Bidding system reset");

        // Start bidding round
        StartBiddingRound();
    }

    //----------------------------------------------
    private void CollectAllCardsToDeck()
    {
        Debug.Log("[GameStage] === COLLECTING ALL CARDS TO DECK ===");
        Debug.Log($"[GameStage] Starting card collection. Deck size before: {m_deck.Size}");
        
        // Clear the deck first
        Debug.Log($"[GameStage] Clearing deck. Deck size before clear: {m_deck.Size}");
        m_deck.Clear();
        Debug.Log($"[GameStage] Deck cleared. Deck size after clear: {m_deck.Size}");
        
        int totalCardsCollected = 0;
        
        // Collect all cards from all players' hands
        foreach (Player player in m_players)
        {
            if (player.Hand != null && player.Hand.Size > 0)
            {
                int cardsInHand = player.Hand.Size;
                Debug.Log($"[GameStage] Collecting {cardsInHand} cards from {player.Name}");
                
                // Debug: List the actual cards being collected
                for (int i = 0; i < player.Hand.Size; i++)
                {
                    BeloteCard card = player.Hand.Cards[i];
                    Debug.Log($"[GameStage]   - Card {i+1}: {card.Value} of {card.Family}");
                }
                
                // Move all cards from player's hand back to deck
                player.Hand.MoveCardsTo(player.Hand.Size, m_deck);
                totalCardsCollected += cardsInHand;
                
                Debug.Log($"[GameStage] After collection, {player.Name} now has {player.Hand.Size} cards");
            }
            else
            {
                Debug.Log($"[GameStage] {player.Name} has no cards to collect (Hand is null or empty)");
            }
        }
        
        // Also collect any cards from current fold if it exists
        if (m_currentFold != null && m_currentFold.Deck != null && m_currentFold.Deck.Size > 0)
        {
            int cardsInFold = m_currentFold.Deck.Size;
            Debug.Log($"[GameStage] Collecting {cardsInFold} cards from current fold");
            m_currentFold.Deck.MoveCardsTo(m_currentFold.Deck.Size, m_deck);
            totalCardsCollected += cardsInFold;
        }
        else
        {
            Debug.Log($"[GameStage] No cards in current fold to collect");
        }
        
        // Also collect the face-up card if it exists
        if (FaceUpCard != null)
        {
            Debug.Log($"[GameStage] Collecting face-up card: {FaceUpCard.Value} of {FaceUpCard.Family}");
            m_deck.AddCard(FaceUpCard);
            FaceUpCard.Owner = null; // Reset owner
            totalCardsCollected++;
        }
        else
        {
            Debug.Log($"[GameStage] No face-up card to collect");
        }
        
        Debug.Log($"[GameStage] Total cards collected: {totalCardsCollected}, Deck size after collection: {m_deck.Size}");
        
        // If no cards were collected, we need to reinitialize the deck
        if (m_deck.Size == 0)
        {
            Debug.LogWarning("[GameStage] No cards collected! Reinitializing deck.");
            m_deck.Init(Definition.Scoring);
        }
        
        // Shuffle the deck after collecting all cards
        m_deck.Shuffle();
        
        Debug.Log($"[GameStage] Final deck size after shuffle: {m_deck.Size}");
        
        // Validate deck integrity - check for duplicates
        ValidateDeckIntegrity();
        
        // Send event to notify renderer that cards have been collected
        CardsCollectedEvent evt = Pools.Claim<CardsCollectedEvent>();
        GameEventDispatcher.SendEvent(evt);

        Debug.Log("[GameStage] === CARD COLLECTION COMPLETE ===");
        
        // Additional validation
        if (m_deck.Size < 32)
        {
            Debug.LogWarning($"[GameStage] Deck has only {m_deck.Size} cards, expected 32. This might cause issues.");
        }
    }

    //----------------------------------------------
    private void ValidateDeckIntegrity()
    {
        Debug.Log("[GameStage] === VALIDATING DECK INTEGRITY ===");
        
        if (m_deck.Size != 32)
        {
            Debug.LogError($"[GameStage] Deck integrity check failed: Expected 32 cards, got {m_deck.Size}");
            return;
        }
        
        // Check for duplicate cards
        HashSet<string> cardSignatures = new HashSet<string>();
        bool hasDuplicates = false;
        
        for (int i = 0; i < m_deck.Size; i++)
        {
            BeloteCard card = m_deck.Cards[i];
            string signature = $"{card.Value}_{card.Family}";
            
            if (cardSignatures.Contains(signature))
            {
                Debug.LogError($"[GameStage] DUPLICATE CARD FOUND: {card.Value} of {card.Family} at position {i}");
                hasDuplicates = true;
            }
            else
            {
                cardSignatures.Add(signature);
            }
        }
        
        if (hasDuplicates)
        {
            Debug.LogError("[GameStage] Deck integrity check failed: Duplicate cards found!");
        }
        else
        {
            Debug.Log("[GameStage] Deck integrity check passed: No duplicate cards found");
        }
        
        Debug.Log($"[GameStage] Deck contains {cardSignatures.Count} unique cards out of {m_deck.Size} total cards");
    }

    //----------------------------------------------
    void StartBiddingRound()
    {
        Debug.Log($"[GameStage] Starting bidding round for Round {m_currentRound}");
        Debug.Log($"[GameStage] Deck size at start of bidding: {m_deck.Size}");
        
        // Reset all players' bidding state
        foreach (Player player in m_players)
        {
            player.ResetBidding();
        }

        // Check if deck has enough cards
        if (m_deck.Size < 1)
        {
            Debug.LogError("[GameStage] Deck is empty! Cannot start bidding round.");
            Debug.LogError($"[GameStage] Deck size: {m_deck.Size}");
            return;
        }

        // Reveal face-up card (proposed trump suit)
        BeloteDeck tempDeck = new BeloteDeck();
        m_deck.MoveCardsTo(1, tempDeck); // Move one card to temp deck
        
        if (tempDeck.Size == 0)
        {
            Debug.LogError("[GameStage] Failed to draw face-up card from deck!");
            return;
        }
        
        BeloteCard faceUpCard = tempDeck.Cards[0]; // Get the card
        faceUpCard.Owner = null; // Face-up card belongs to no one
        
        // Store face-up card for visual display
        FaceUpCard = faceUpCard;
        Debug.Log($"[GameStage] Revealed face-up card: {faceUpCard.Value} of {faceUpCard.Family}");

        // Start bidding with the first player
        Debug.Log($"[GameStage] Starting bidding with first bidder: {RoundFirstPlayer?.Name}");
        Debug.Log($"[GameStage] Dealer is: {Dealer?.Name}");
        Debug.Log($"[GameStage] First bidder should be to the right of dealer: {GetRightPlayer(Dealer)?.Name}");
        
        // Verify the bidding order matches our expectation
        Debug.Log($"[GameStage] Bidding order verification:");
        for (int i = 0; i < m_players.Count; i++)
        {
            string markers = "";
            if (m_players[i] == Dealer) markers += " [DEALER]";
            if (m_players[i] == RoundFirstPlayer) markers += " [FIRST BIDDER]";
            Debug.Log($"[GameStage]   Bidder {i}: {m_players[i].Name}{markers}");
        }
        
        m_biddingSystem.StartBidding(m_players, RoundFirstPlayer, faceUpCard);
        Debug.Log($"[GameStage] Bidding started with first bidder: {RoundFirstPlayer?.Name}");
    }

    //----------------------------------------------
    void OnBiddingComplete(BiddingCompleteEvent evt)
    {
        Debug.Log($"[GameStage] === OnBiddingComplete called ===");
        Debug.Log($"[GameStage] WinningBidder: {evt.WinningBidder?.Name}");
        Debug.Log($"[GameStage] WinningBid: {evt.WinningBid?.ToString()}");
        Debug.Log($"[GameStage] SunDeclared: {evt.SunDeclared}");
        
        if (evt.WinningBidder != null && evt.WinningBid != null)
        {
            Debug.Log($"[GameStage] Contract made - proceeding with card dealing completion");
            // Set the winning bidder and contract
            Bidder = evt.WinningBidder;
            
            if (evt.SunDeclared)
            {
                // Sun contract - no trump
                Trump = null;
                Debug.Log($"{evt.WinningBidder.Name} declared Sun contract - gets face-up card + 2 additional cards");
            }
            else if (evt.WinningBid.IsTrump)
            {
                // Trump contract
                Trump = evt.WinningBid.Suit;
            }

            // Clear face-up card reference before dealing (so renderer can clean up the visual)
            BeloteCard faceUpCardToClean = FaceUpCard;
            FaceUpCard = null;
            
            // Deal remaining cards to complete all hands to 8 cards
            DealRemainingCardsAfterContract(evt.WinningBidder, faceUpCardToClean);

            // Resort all hands with known trump
            foreach (Player player in m_players)
            {
                player.Hand.SortByFamilyAndValue(Trump);
            }

            // Announce round start
            NewRoundEvent roundEvt = Pools.Claim<NewRoundEvent>();
            roundEvt.Start = true;
            GameEventDispatcher.SendEvent(roundEvt);

            // Start the first turn
            StartTurn(RoundFirstPlayer);
        }
        else
        {
            // Case C: All passed, no contract - start new round with new dealer
            Debug.Log("[GameStage] === NO CONTRACT MADE - STARTING NEW ROUND ===");
            Debug.Log("[GameStage] All players passed in bidding - resetting and starting new round with new dealer");
            StartRound();
        }
    }

    //----------------------------------------------
    void OnBiddingNoBids(BiddingNoBidsEvent evt)
    {
        if (evt.BothRoundsNoBids)
        {
            // Both bidding rounds had no Trump/Sun bids - start new round with new dealer
            Debug.Log("[GameStage] === BOTH ROUNDS PASSED - STARTING NEW ROUND ===");
            Debug.Log("[GameStage] Both bidding rounds had no Trump/Sun bids - resetting everything and starting new round with new dealer");
            Debug.Log("[GameStage] IMPORTANT: No contract made - will NOT deal remaining cards, will collect all cards and restart");
            
            // Debug: Check deck size before starting new round
            Debug.Log($"[GameStage] Deck size before starting new round: {m_deck.Size}");
            
            // Debug: Check players' hands before starting new round
            foreach (Player player in m_players)
            {
                Debug.Log($"[GameStage] {player.Name} has {player.Hand.Size} cards before new round");
            }
            
            // Debug: Check face-up card
            if (FaceUpCard != null)
            {
                Debug.Log($"[GameStage] Face-up card to be collected: {FaceUpCard.Value} of {FaceUpCard.Family}");
            }
            else
            {
                Debug.Log($"[GameStage] No face-up card to collect");
            }
            
            // DISABLED FOR ANOTHER TRUMP DEBUGGING: StartRound();
            Debug.Log("[GameStage] DISABLED FOR ANOTHER TRUMP DEBUGGING: Would normally call StartRound()");
        }
    }

    //----------------------------------------------
    public void SubmitBid(Player player, Bid bid)
    {
        // Submit bid through bidding system
        if (m_biddingSystem.SubmitBid(player, bid))
        {
            // Bid was accepted - send turn event for next player
            if (!m_biddingSystem.IsComplete)
            {
                BiddingTurnEvent evt = Pools.Claim<BiddingTurnEvent>();
                evt.CurrentBidder = m_biddingSystem.CurrentBidder;
                evt.HighestBid = m_biddingSystem.HighestBid;
                evt.Round = m_biddingSystem.CurrentRound;
        GameEventDispatcher.SendEvent(evt);
            }
        }
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
            StartTurn(GetRightPlayer(CurrentPlayer));               // Next player anti-clockwise
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

    public class CardsCollectedEvent : PooledEvent
    {
        public override void Reset()
        {
            // No specific data needed for this event
        }
    }
}




