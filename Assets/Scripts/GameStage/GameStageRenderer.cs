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
    }

    public  void Shutdown()
    {
        GameEventDispatcher.UnSubscribe<BeloteCard.Played>(this.OnCardPlayed, EventChannel.Post);
        GameEventDispatcher.UnSubscribe<GameStage.NewRoundEvent>(this.OnNewRound);
        GameEventDispatcher.UnSubscribe<GameStage.NewTurnEvent>(this.OnNewTurn);
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
                GUI.Label(new Rect(UnityEngine.Screen.width - 320, 230, 100, 30), "Trump : " + Stage.Trump);
                GUI.Label(new Rect(UnityEngine.Screen.width - 320, 260, 100, 30), "Dealer : " + Stage.Dealer.Name);
                GUI.Label(new Rect(UnityEngine.Screen.width - 320, 290, 100, 30), "Bidder : " + Stage.Bidder.Name);
                GUI.Label(new Rect(UnityEngine.Screen.width - 320, 320, 100, 30), "Current : " + Stage.CurrentPlayer.Name);

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
        // TODO : Spawn once, then invisible
        if(evt.Start)
        {
            SpawnCards();      // Create views for every card in hands
        }
        else
        {
            UnSpawnCards();    // Destroy all views
        }
            
        Refresh();             // Recompute positions
    }

    private void OnNewTurn(GameStage.NewTurnEvent evt)
    {
       Refresh();              // Re-layout hands and fold
    }

    protected void OnCardPlayed(BeloteCard.Played evt)
    {
        Refresh();             // Move played card to fold area
    }

    protected void SpawnCards()
    {
        foreach (Player player in Stage.Players)
        {
            SpawnCards(player);
        }
    }
    protected void SpawnCards(Player player)
    {
        foreach (BeloteCard card in player.Hand)
        {
            CardComponent newCard = card.Spawn();
            if (newCard)
            {
                m_cards.Add(newCard); // Track spawned card
            }
        }
    }

    protected void UnSpawnCards()
    {
        foreach (CardComponent cardObj in m_cards)
        {
            UnityEngine.Object.Destroy(cardObj.gameObject);
        }
        m_cards.Clear();
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