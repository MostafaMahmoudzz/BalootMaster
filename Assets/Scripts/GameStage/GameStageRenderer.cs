using System;
using System.Collections.Generic;
using UnityEngine;
using Pebble;

//-------------------------------------------------------
// GameStageRenderer
//-------------------------------------------------------
// Purpose:
//   Handles visual representation of the game state: spawns/destroys
//   card views, positions hands and the current fold, and draws simple
//   on-screen HUD (score, trump, dealer, bidder, current player).
//
// How it connects to other scripts:
//   - Subscribes to `BeloteCard.Played`, `GameStage.NewRoundEvent`,
//     and `GameStage.NewTurnEvent` to refresh the display.
//   - Uses `CardComponent` to render each `BeloteCard` model.
//   - Reads from `GameStage` (players, folds, score, trump, etc.).
//-------------------------------------------------------
public class GameStageRenderer
{
    //----------------------------------------------
    // Variables
    private List<CardComponent> m_cards; // All instantiated card views
    private CardComponent m_faceUpCardComponent; // Track the face-up card component specifically
    private GameStage.RoundEndScoreEvent m_lastRoundScore; // Store last round's score details for display
    private float m_scoreDisplayTimer = 0f; // Timer to show score details
    private const float SCORE_DISPLAY_DURATION = 5f; // How long to show score details

    //----------------------------------------------
    // Properties

    public GameStage Stage
    {
        get; set;
    }

    //----------------------------------------------
    // Methods
    //-------------------------------------------------------
    public GameStageRenderer()
    {
        m_cards = new List<CardComponent>(); // Prepare storage
    }

    public void Init()
    {
        GameEventDispatcher.Subscribe<BeloteCard.Played>(this.OnCardPlayed, EventChannel.Post); // React after plays
        GameEventDispatcher.Subscribe<GameStage.NewRoundEvent>(this.OnNewRound);                 // Spawn/unspawn on round
        GameEventDispatcher.Subscribe<GameStage.NewTurnEvent>(this.OnNewTurn);                   // Re-layout on turn
        GameEventDispatcher.Subscribe<BiddingStartEvent>(this.OnBiddingStart);                   // Handle bidding start
        GameEventDispatcher.Subscribe<BiddingCompleteEvent>(this.OnBiddingComplete);             // Handle bidding complete
        GameEventDispatcher.Subscribe<GameStage.CardsCollectedEvent>(this.OnCardsCollected);     // Handle card collection
        GameEventDispatcher.Subscribe<GameStage.RoundEndScoreEvent>(this.OnRoundEndScore);       // Handle round end scoring
    }

    public  void Shutdown()
    {
        GameEventDispatcher.UnSubscribe<BeloteCard.Played>(this.OnCardPlayed, EventChannel.Post);
        GameEventDispatcher.UnSubscribe<GameStage.NewRoundEvent>(this.OnNewRound);
        GameEventDispatcher.UnSubscribe<GameStage.NewTurnEvent>(this.OnNewTurn);
        GameEventDispatcher.UnSubscribe<BiddingStartEvent>(this.OnBiddingStart);
        GameEventDispatcher.UnSubscribe<BiddingCompleteEvent>(this.OnBiddingComplete);
        GameEventDispatcher.UnSubscribe<GameStage.CardsCollectedEvent>(this.OnCardsCollected);
        GameEventDispatcher.UnSubscribe<GameStage.RoundEndScoreEvent>(this.OnRoundEndScore);
        
        // Clean up face-up card
        UnSpawnFaceUpCard();
        
        // Final cleanup of any remaining face-up cards
        CleanupAllFaceUpCards();
    }

    //---------------------------------------------
    public void Update()
    {
        // Update score display timer
        if (m_scoreDisplayTimer > 0)
        {
            m_scoreDisplayTimer -= Time.deltaTime;
        }
    }

    public void UpdateGUI()
    {
        if(!Stage.HasEnded)
        {
            if(Stage.Score != null)
            {
                GUI.Label(new Rect(UnityEngine.Screen.width - 320, 200, 100, 30), "Score : " + Stage.Score.GetScore(PlayerTeam.Team1) + " / " + Stage.Score.GetScore(PlayerTeam.Team2)); // Simple HUD
                GUI.Label(new Rect(UnityEngine.Screen.width - 320, 230, 100, 30), "Contract : " + (Stage.Trump != null ? Stage.Trump.ToString() : "Sun"));
                GUI.Label(new Rect(UnityEngine.Screen.width - 320, 260, 100, 30), "Dealer : " + (Stage.Dealer != null ? Stage.Dealer.Name : "Not Set"));
                GUI.Label(new Rect(UnityEngine.Screen.width - 320, 290, 100, 30), "Bidder : " + (Stage.Bidder != null ? Stage.Bidder.Name : "Not Set"));
                
                // Show current bidder during bidding, current player during card play
                string currentPlayerName = "Not Set";
                string statusMessage = "";
                
                bool biddingActive = (Stage.BiddingSystem != null && !Stage.BiddingSystem.IsComplete);
                
                if (biddingActive)
                {
                    // During bidding, show current bidder - ALWAYS read from system
                    Player systemCurrentBidder = Stage.BiddingSystem.CurrentBidder;
                    currentPlayerName = systemCurrentBidder != null ? systemCurrentBidder.Name : "Not Set";
                    
                    // CRITICAL DEBUG: Log EVERY call to see what's being read
                    Debug.LogError($"[GameStageRenderer] ⚠️ OnGUI READING from system: CurrentBidder = {systemCurrentBidder?.Name}, will display: {currentPlayerName}");
                    
                    // Check if we're waiting for trump suit selection
                    if (Stage.BiddingSystem.WaitingForTrumpSuitSelection)
                    {
                        statusMessage = "Waiting for Trump Suit Selection";
                        currentPlayerName = "Select Trump Suit";
                    }
                    else
                    {
                        statusMessage = "Bidding in Progress";
                    }
                }
                else if (Stage.CurrentPlayer != null)
                {
                    // During card play, show current player
                    currentPlayerName = Stage.CurrentPlayer.Name;
                    statusMessage = "Card Play in Progress";
                }
                GUI.Label(new Rect(UnityEngine.Screen.width - 320, 320, 100, 30), "Current : " + currentPlayerName);
                
                // Display status message
                if (!string.IsNullOrEmpty(statusMessage))
                {
                    GUI.Label(new Rect(UnityEngine.Screen.width - 320, 350, 200, 30), "Status: " + statusMessage);
                }
                
                // Display face-up card information during bidding
                if (Stage.FaceUpCard != null)
                {
                    GUI.Label(new Rect(UnityEngine.Screen.width - 320, 380, 200, 30), "Face-up Card: " + Stage.FaceUpCard.Value + " of " + Stage.FaceUpCard.Family);
                    
                    // Show trump suit selection instructions if waiting for suit selection
                    if (Stage.BiddingSystem != null && Stage.BiddingSystem.WaitingForTrumpSuitSelection)
                    {
                        GUI.Label(new Rect(UnityEngine.Screen.width - 320, 410, 250, 30), "Cannot choose: " + Stage.FaceUpCard.Family + " (face-up suit)");
                        GUI.Label(new Rect(UnityEngine.Screen.width - 320, 440, 250, 30), "Choose any other suit as trump");
                    }
                }
                
                // Display round end score breakdown if available
                if (m_lastRoundScore != null && m_scoreDisplayTimer > 0)
                {
                    DisplayRoundScoreBreakdown();
                }

            }
            

            /*// UI display
            HumanPlayer human = Screen.CurrentPlayer as HumanPlayer;
            if (human != null)
            {
                if (GUI.Button(new Rect(UnityEngine.Screen.width - 120, UnityEngine.Screen.height - 60, 100, 30), "End turn"))
                {
                    EventManager.SendEmptyPooledEvent<EndTurnButtonClicked>();
                }

                GUI.Label(new Rect(20, UnityEngine.Screen.height - 160, 100, 30), "Energy : " + human.Energy);
                GUI.Label(new Rect(20, UnityEngine.Screen.height - 120, 100, 30), "DrawPile : " + human.DrawPile.Size);
                GUI.Label(new Rect(UnityEngine.Screen.width - 120, UnityEngine.Screen.height - 120, 100, 30), "Discard : " + human.DiscardPile.Size);
            }


            // MinionDisplay

            foreach (Player combattant in Screen.Players)
            {
                int y = 50;
                int x = UnityEngine.Screen.width - 60;
                if (combattant is HumanPlayer)
                {
                    x = 30;
                }
            }*/
        }
        else
        {
            if(Stage.Succeded)
            {
                GUI.TextField(new Rect(20, UnityEngine.Screen.height - 160, 100, 30), "You win");
            }
            else
            {
                GUI.TextField(new Rect(20, UnityEngine.Screen.height - 160, 100, 30), "You Fail");
            }
        }
    }

    private void OnNewRound(GameStage.NewRoundEvent evt)
    {
        if(evt.Start)
        {
            // Cards are already spawned from OnBiddingStart, so we don't need to spawn them again
            // Just refresh positions in case cards were added/removed during bidding
            Refresh();
        }
        else
        {
            UnSpawnCards();    // Destroy all views
        }
    }

    private void OnNewTurn(GameStage.NewTurnEvent evt)
    {
       Refresh();              // Re-layout hands and fold
    }

    protected void OnCardPlayed(BeloteCard.Played evt)
    {
        Refresh();             // Move played card to fold area
    }

    //----------------------------------------------
    protected void OnBiddingStart(BiddingStartEvent evt)
    {
        // Show player cards during bidding so they can make informed decisions
        // Only spawn if cards aren't already spawned
        if (m_cards.Count == 0)
        {
            SpawnCards();
        }
        else
        {
            // Cards are already spawned, just refresh positions
            Refresh();
        }
        
        // Display face-up card for bidding
        SpawnFaceUpCard();
        
        DebugCardCount();
    }

    //----------------------------------------------
    protected void OnBiddingComplete(BiddingCompleteEvent evt)
    {
        // Hide face-up card after bidding
        // Note: For Sun contracts, the face-up card has already been transferred to the winner's hand
        // For Trump contracts, the face-up card should still be available
        UnSpawnFaceUpCard();
        
        // Cards are already spawned from OnBiddingStart, so we don't need to spawn them again
        // Just refresh positions in case cards were added/removed during bidding
        Refresh();
        
        DebugCardCount();
    }

    //----------------------------------------------
    protected void OnCardsCollected(GameStage.CardsCollectedEvent evt)
    {
        Debug.Log("[GameStageRenderer] Cards collected event received - cleaning up all visual components");
        
        // Clean up all card visual components since cards have been collected back to deck
        UnSpawnCards();
        
        // Clean up face-up card as well
        UnSpawnFaceUpCard();
        
        // Final cleanup of any remaining orphaned cards
        CleanupAllFaceUpCards();
        
        DebugCardCount();
    }

    //----------------------------------------------
    protected void OnRoundEndScore(GameStage.RoundEndScoreEvent evt)
    {
        Debug.Log("[GameStageRenderer] Round end score event received - displaying score breakdown");
        m_lastRoundScore = evt;
        m_scoreDisplayTimer = SCORE_DISPLAY_DURATION;
    }

    protected void SpawnCards()
    {
        // Clean up existing cards first to prevent duplication
        UnSpawnCards();
        
        foreach (Player player in Stage.Players)
        {
            SpawnCards(player);
        }
        
        // Position all cards in their designated positions
        Refresh();
    }
    protected void SpawnCards(Player player)
    {
        foreach (BeloteCard card in player.Hand)
        {
            // Check if this card already has a visual component
            CardComponent existingCard = GetCardComponent(card);
            if (existingCard == null)
            {
                CardComponent newCard = card.Spawn();
                if (newCard)
                {
                    m_cards.Add(newCard); // Track spawned card
                }
            }
            else
            {
                Debug.Log($"Card already exists: {card.Value} of {card.Family} for {player.Name}");
            }
        }
    }

    // Spawn a single card and return its CardComponent
    protected CardComponent SpawnCard(BeloteCard card, Player player)
    {
        CardComponent newCard = card.Spawn();
        if (newCard)
        {
            m_cards.Add(newCard); // Track spawned card
            Debug.Log($"Spawned new card: {card.Value} of {card.Family} for {player.Name}");
        }
        return newCard;
    }

    protected void UnSpawnCards()
    {
        foreach (CardComponent cardObj in m_cards)
        {
            // Don't destroy the face-up card here - it's managed separately
            if (cardObj != m_faceUpCardComponent)
            {
                UnityEngine.Object.Destroy(cardObj.gameObject);
            }
        }
        m_cards.Clear();
    }

    //----------------------------------------------
    protected void SpawnFaceUpCard()
    {
        // Always clean up any existing face-up cards first (safety measure)
        CleanupAllFaceUpCards();
        
        // Don't spawn if one already exists
        if (m_faceUpCardComponent != null)
        {
            Debug.Log("Face-up card already exists, skipping spawn");
            return;
        }
        
        if (Stage.FaceUpCard != null)
        {
            m_faceUpCardComponent = Stage.FaceUpCard.Spawn();
            if (m_faceUpCardComponent != null)
            {
                // Position face-up card in center of screen
                float halfHeight = Camera.main.orthographicSize;
                float halfWidth = halfHeight * Camera.main.aspect;
                
                Vector3 faceUpPosition = new Vector3(0, 0, -1); // Center of screen
                m_faceUpCardComponent.SetInitialPosition(faceUpPosition);
                
                // Make it slightly larger and elevated to stand out
                m_faceUpCardComponent.transform.localScale = new Vector3(1.2f, 1.2f, 1.0f);
                m_faceUpCardComponent.transform.position = new Vector3(faceUpPosition.x, faceUpPosition.y, -2);
                
                // Don't add face-up card to m_cards list - it's tracked separately
                Debug.Log($"Spawned face-up card: {Stage.FaceUpCard.Value} of {Stage.FaceUpCard.Family}");
            }
        }
    }

    //----------------------------------------------
    protected void UnSpawnFaceUpCard()
    {
        if (m_faceUpCardComponent != null)
        {
            Debug.Log($"UnSpawning face-up card: {m_faceUpCardComponent.Card?.Value} of {m_faceUpCardComponent.Card?.Family}");
            
            // Destroy the GameObject
            UnityEngine.Object.Destroy(m_faceUpCardComponent.gameObject);
            
            // Clear reference
            m_faceUpCardComponent = null;
        }
        
        // Also clean up any remaining face-up cards in the scene (safety net)
        CleanupAllFaceUpCards();
    }
    
    //----------------------------------------------
    protected void CleanupAllFaceUpCards()
    {
        // Find all CardComponents in the scene
        CardComponent[] allCards = GameObject.FindObjectsOfType<CardComponent>();
        
        foreach (CardComponent cardComp in allCards)
        {
            if (cardComp != null && cardComp.Card != null && cardComp.Card.Owner == null)
            {
                // This is a face-up card (no owner)
                Debug.Log($"Found orphaned face-up card: {cardComp.Card.Value} of {cardComp.Card.Family}, destroying it");
                UnityEngine.Object.Destroy(cardComp.gameObject);
            }
        }
    }

    protected void UnSpawnCard(CardComponent cardObj)
    {
        m_cards.Remove(cardObj);
        UnityEngine.Object.Destroy(cardObj.gameObject);
    }

    void Refresh()
    {
        foreach (Player player in Stage.Players)
        {
            RefreshHand(player);
        }

        RefreshCurrentFold();

        RemovePastFolds();
    }

     private Vector3 spawnRef = new Vector3();
    private Vector3 rotation = new Vector3();
    protected void RefreshHand(Player player)
    {
        float halfHeight = Camera.main.orthographicSize;
        float halfWidth = halfHeight*Camera.main.aspect;

        float spacing = -0.4f;
        
        // Reset rotation for each player
        rotation = Vector3.zero;

        if(player.Position == PlayerPosition.South)
        {
            spawnRef.x = -0.5f * halfWidth;
            spawnRef.y = -0.75f * halfHeight;   
        }
        else  if(player.Position == PlayerPosition.West)
        {
            spawnRef.x = -0.85f * halfWidth;
            spawnRef.y = 0.8f * halfHeight;     
        }
        else if(player.Position == PlayerPosition.North)
        {
            spawnRef.x = -0.5f * halfWidth;
            spawnRef.y = 0.75f * halfHeight;   
        }
        else // East
        {
            spawnRef.x = 0.85f * halfWidth;
            spawnRef.y = 0.8f * halfHeight;    
        }
        
        foreach (BeloteCard card in player.Hand)
        {
            CardComponent cardComp = GetCardComponent(card);
            
            // If card doesn't have a visual component yet, spawn one
            if (cardComp == null)
            {
                cardComp = SpawnCard(card, player);
            }
            
            if (cardComp)
            {
                cardComp.SetInitialPosition(spawnRef); // Place card at computed anchor

                Renderer renderer = cardComp.gameObject.GetComponent<Renderer>();

                if(player.Position == PlayerPosition.South)
                {
                    spawnRef.x += renderer.bounds.size.x + spacing;
                }
                else  if(player.Position == PlayerPosition.West)
                {
                    spawnRef.y -= (renderer.bounds.size.x + spacing);
                    rotation.z = 90.0f;
                    cardComp.gameObject.transform.eulerAngles = rotation;
                }
                else if(player.Position == PlayerPosition.North)
                {
                   spawnRef.x += renderer.bounds.size.x + spacing;
                }
                else // East
                {
                    spawnRef.y -= (renderer.bounds.size.x + spacing);
                    rotation.z = -90.0f;
                    cardComp.gameObject.transform.eulerAngles = rotation;
                }
                
            }
        }
    }

    void RefreshCurrentFold()
    {
        float halfHeight = Camera.main.orthographicSize;
        float halfWidth = halfHeight*Camera.main.aspect;

        foreach (BeloteCard card in Stage.CurrentFold.Deck)
        {
            Player player = card.Owner as Player;

            CardComponent cardComp = GetCardComponent(card);
            if (cardComp)
            {
                if(player.Position == PlayerPosition.South)
                {
                    spawnRef.x = 0.0f;
                    spawnRef.y = -0.25f * halfHeight;  
                }
                else  if(player.Position == PlayerPosition.West)
                {
                    spawnRef.x = -0.20f * halfWidth;  
                    spawnRef.y = 0.0f;  
                }
                else if(player.Position == PlayerPosition.North)
                {
                    spawnRef.x = 0.0f;
                    spawnRef.y = 0.25f * halfHeight;
                }
                else // East
                {
                    spawnRef.x = 0.20f * halfWidth;  
                    spawnRef.y = 0.0f;  
                }
                
                cardComp.SetInitialPosition(spawnRef); // Place card in fold area
            }
        }
    }

    void RemovePastFolds()
    {
        Fold lastFold = Stage.LastFold;
        if(lastFold != null)
        {
            foreach (BeloteCard card in lastFold.Deck)
            {
                CardComponent cardComp = GetCardComponent(card);
                if(cardComp != null)
                {
                    UnSpawnCard(cardComp); // Remove visual for archived fold
                }
            }
        }
    }

    protected CardComponent GetCardComponent(BeloteCard card)
    {
        foreach (CardComponent cardObj in m_cards)
        {
            if(cardObj.Card == card)
            {
                return cardObj;
            }
        }
        return null;
    }

    //----------------------------------------------
    // Debug method to count cards in scene
    public void DebugCardCount()
    {
        int totalCards = GameObject.FindObjectsOfType<CardComponent>().Length;
        Debug.Log($"Total CardComponents in scene: {totalCards}, Tracked cards: {m_cards.Count}, Face-up card: {(m_faceUpCardComponent != null ? "Yes" : "No")}");
    }

    //----------------------------------------------
    // Display detailed round score breakdown
    private void DisplayRoundScoreBreakdown()
    {
        if (m_lastRoundScore == null) return;
        
        float startX = 20;
        float startY = 20;
        float lineHeight = 25;
        float panelWidth = 450;
        int line = 0;
        
        // Background box
        GUI.Box(new Rect(startX - 10, startY - 10, panelWidth + 20, lineHeight * 18 + 20), "");
        
        // Title
        GUI.Label(new Rect(startX, startY + line++ * lineHeight, panelWidth, lineHeight), "=== ROUND END SCORE BREAKDOWN ===");
        line++;
        
        // Team 1 Scores
        GUI.Label(new Rect(startX, startY + line++ * lineHeight, panelWidth, lineHeight), 
            $"Team 1 (South & North) {(m_lastRoundScore.BiddingTeam == PlayerTeam.Team1 ? "[BIDDER]" : "")}");
        GUI.Label(new Rect(startX + 20, startY + line++ * lineHeight, panelWidth, lineHeight), 
            $"Raw Points: {m_lastRoundScore.Team1RawPoints}");
        
        int team1Divided = m_lastRoundScore.IsKaboot && m_lastRoundScore.Team1RawPoints > 0 ? 
            16 : Mathf.RoundToInt(m_lastRoundScore.Team1RawPoints / 10f);
        GUI.Label(new Rect(startX + 20, startY + line++ * lineHeight, panelWidth, lineHeight), 
            m_lastRoundScore.IsKaboot && m_lastRoundScore.Team1RawPoints > 0 ? 
            $"Kaboot Bonus: {team1Divided}" : $"Divided by 10: {team1Divided}");
        
        if (m_lastRoundScore.Multiplier > 1 && m_lastRoundScore.Team1RoundScore > 0)
        {
            GUI.Label(new Rect(startX + 20, startY + line++ * lineHeight, panelWidth, lineHeight), 
                $"Multiplier: ×{m_lastRoundScore.Multiplier}");
        }
        
        GUI.Label(new Rect(startX + 20, startY + line++ * lineHeight, panelWidth, lineHeight), 
            $"Round Score: +{m_lastRoundScore.Team1RoundScore} {(m_lastRoundScore.Team1RoundScore > 0 ? "✓" : "✗")}");
        
        line++;
        
        // Team 2 Scores
        GUI.Label(new Rect(startX, startY + line++ * lineHeight, panelWidth, lineHeight), 
            $"Team 2 (West & East) {(m_lastRoundScore.BiddingTeam == PlayerTeam.Team2 ? "[BIDDER]" : "")}");
        GUI.Label(new Rect(startX + 20, startY + line++ * lineHeight, panelWidth, lineHeight), 
            $"Raw Points: {m_lastRoundScore.Team2RawPoints}");
        
        int team2Divided = m_lastRoundScore.IsKaboot && m_lastRoundScore.Team2RawPoints > 0 ? 
            16 : Mathf.RoundToInt(m_lastRoundScore.Team2RawPoints / 10f);
        GUI.Label(new Rect(startX + 20, startY + line++ * lineHeight, panelWidth, lineHeight), 
            m_lastRoundScore.IsKaboot && m_lastRoundScore.Team2RawPoints > 0 ? 
            $"Kaboot Bonus: {team2Divided}" : $"Divided by 10: {team2Divided}");
        
        if (m_lastRoundScore.Multiplier > 1 && m_lastRoundScore.Team2RoundScore > 0)
        {
            GUI.Label(new Rect(startX + 20, startY + line++ * lineHeight, panelWidth, lineHeight), 
                $"Multiplier: ×{m_lastRoundScore.Multiplier}");
        }
        
        GUI.Label(new Rect(startX + 20, startY + line++ * lineHeight, panelWidth, lineHeight), 
            $"Round Score: +{m_lastRoundScore.Team2RoundScore} {(m_lastRoundScore.Team2RoundScore > 0 ? "✓" : "✗")}");
        
        line++;
        
        // Summary
        if (m_lastRoundScore.IsKaboot)
        {
            GUI.Label(new Rect(startX, startY + line++ * lineHeight, panelWidth, lineHeight), 
                "*** KABOOT! (Winner took all tricks) ***");
        }
        
        if (m_lastRoundScore.Multiplier > 1)
        {
            GUI.Label(new Rect(startX, startY + line++ * lineHeight, panelWidth, lineHeight), 
                $"Multiplier: ×{m_lastRoundScore.Multiplier} (Double/Triple/Quad)");
        }
        
        bool bidderWon = (m_lastRoundScore.BiddingTeam == m_lastRoundScore.WinningTeam);
        string bidderName = m_lastRoundScore.BiddingTeam == PlayerTeam.Team1 ? "Team 1" : "Team 2";
        
        GUI.Label(new Rect(startX, startY + line++ * lineHeight, panelWidth, lineHeight), 
            bidderWon ? $"{bidderName} (Bidder) WON!" : $"{bidderName} (Bidder) LOST!");
        
        line++;
        
        // Cumulative Totals
        GUI.Label(new Rect(startX, startY + line++ * lineHeight, panelWidth, lineHeight), 
            $"Total Scores: Team1={m_lastRoundScore.Team1CumulativeScore} | Team2={m_lastRoundScore.Team2CumulativeScore}");
        
        // Timer
        GUI.Label(new Rect(startX, startY + line++ * lineHeight, panelWidth, lineHeight), 
            $"(Closing in {Mathf.CeilToInt(m_scoreDisplayTimer)}s...)");
    }

}

//-------------------------------------------------------------------------------------
//-------------------------------------------------------------------------------------
// EndTurnButtonClicked
//-------------------------------------------------------------------------------------
//-------------------------------------------------------------------------------------
public class EndTurnButtonClicked : PooledEvent
{
    public override void Reset()
    {

    }
}