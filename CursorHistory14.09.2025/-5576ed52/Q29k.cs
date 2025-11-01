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
    }

    public  void Shutdown()
    {
        GameEventDispatcher.UnSubscribe<BeloteCard.Played>(this.OnCardPlayed, EventChannel.Post);
        GameEventDispatcher.UnSubscribe<GameStage.NewRoundEvent>(this.OnNewRound);
        GameEventDispatcher.UnSubscribe<GameStage.NewTurnEvent>(this.OnNewTurn);
        GameEventDispatcher.UnSubscribe<BiddingStartEvent>(this.OnBiddingStart);
        GameEventDispatcher.UnSubscribe<BiddingCompleteEvent>(this.OnBiddingComplete);
        
        // Clean up face-up card
        UnSpawnFaceUpCard();
        
        // Final cleanup of any remaining face-up cards
        CleanupAllFaceUpCards();
    }

    //---------------------------------------------
    public void Update()
    {

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
                GUI.Label(new Rect(UnityEngine.Screen.width - 320, 320, 100, 30), "Current : " + (Stage.CurrentPlayer != null ? Stage.CurrentPlayer.Name : "Not Set"));
                
                // Display face-up card information during bidding
                if (Stage.FaceUpCard != null)
                {
                    GUI.Label(new Rect(UnityEngine.Screen.width - 320, 350, 200, 30), "Face-up Card: " + Stage.FaceUpCard.Value + " of " + Stage.FaceUpCard.Family);
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

    protected void SpawnCards()
    {
        // Clean up existing cards first to prevent duplication
        UnSpawnCards();
        
        foreach (Player player in Stage.Players)
        {
            SpawnCards(player);
        }
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
                    Debug.Log($"Spawned card: {card.Value} of {card.Family} for {player.Name}");
                }
            }
            else
            {
                Debug.Log($"Card already exists: {card.Value} of {card.Family} for {player.Name}");
            }
        }
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