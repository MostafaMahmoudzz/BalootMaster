using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Pebble;

//-------------------------------------------------------
// GameEventLogger
//-------------------------------------------------------
// Purpose:
//   Subscribes to all major game events and logs detailed
//   information about game flow, player actions, scoring,
//   and other key game events.
//
// How it connects to other scripts:
//   - Subscribes to events from GameStage, BiddingSystem,
//     ProjectManager, RassaSystem, AssaaSystem, and Sawa.
//   - Similar to GameStageRenderer.Init() pattern.
//   - Can be initialized by GameStage or standalone.
//-------------------------------------------------------
public class GameEventLogger
{
    //----------------------------------------------
    // Properties
    public GameStage Stage { get; set; }

    //----------------------------------------------
    // Constructor
    public GameEventLogger()
    {
        Debug.Log("[GameEventLogger] Created");
    }

    //----------------------------------------------
    // Init - Subscribe to all events
    public void Init()
    {
        Debug.Log("[GameEventLogger] === Initializing Event Subscriptions ===");

        // Rassa events
        GameEventDispatcher.Subscribe<RassaPromptEvent>(this.OnRassaPrompt);
        GameEventDispatcher.Subscribe<RassaResponseEvent>(this.OnRassaResponse);
        GameEventDispatcher.Subscribe<RassaChoiceCompleteEvent>(this.OnRassaChoiceComplete);

        // Assaa events
        GameEventDispatcher.Subscribe<AssaaPromptEvent>(this.OnAssaaPrompt);
        GameEventDispatcher.Subscribe<AssaaResponseEvent>(this.OnAssaaResponse);
        GameEventDispatcher.Subscribe<AssaaReorderCompleteEvent>(this.OnAssaaReorderComplete);
        GameEventDispatcher.Subscribe<AssaaProcessCompleteEvent>(this.OnAssaaProcessComplete);

        // Project events
        GameEventDispatcher.Subscribe<ProjectDeclarationStartEvent>(this.OnProjectDeclarationStart);
        GameEventDispatcher.Subscribe<ProjectDeclaredEvent>(this.OnProjectDeclared);
        GameEventDispatcher.Subscribe<ProjectDeclarationCompleteEvent>(this.OnProjectDeclarationComplete);
        GameEventDispatcher.Subscribe<BeloteDeclaredEvent>(this.OnBeloteDeclared);
        GameEventDispatcher.Subscribe<ProjectScoredEvent>(this.OnProjectScored);

        // Bidding events
        GameEventDispatcher.Subscribe<BiddingStartEvent>(this.OnBiddingStart);
        GameEventDispatcher.Subscribe<BidSubmittedEvent>(this.OnBidSubmitted);
        GameEventDispatcher.Subscribe<BiddingCompleteEvent>(this.OnBiddingComplete);
        GameEventDispatcher.Subscribe<BiddingRound2StartEvent>(this.OnBiddingRound2Start);
        GameEventDispatcher.Subscribe<MultiplierBiddingStartEvent>(this.OnMultiplierBiddingStart);

        // Card play events
        GameEventDispatcher.Subscribe<BeloteCard.Played>(this.OnCardPlayed);

        // Round/Turn events
        GameEventDispatcher.Subscribe<GameStage.NewRoundEvent>(this.OnNewRound);
        GameEventDispatcher.Subscribe<GameStage.NewTurnEvent>(this.OnNewTurn);
        GameEventDispatcher.Subscribe<GameStage.CardsCollectedEvent>(this.OnCardsCollected);
        GameEventDispatcher.Subscribe<GameStage.RoundEndScoreEvent>(this.OnRoundEndScore);
        GameEventDispatcher.Subscribe<GameStage.FoldWinnerEvent>(this.OnFoldWinner);

        // Sawa events
        GameEventDispatcher.Subscribe<SawaAvailableEvent>(this.OnSawaAvailable);
        GameEventDispatcher.Subscribe<SawaClaimedEvent>(this.OnSawaClaimed);

        Debug.Log("[GameEventLogger] === All Event Subscriptions Complete ===");
    }

    //----------------------------------------------
    // Shutdown - Unsubscribe from all events
    public void Shutdown()
    {
        Debug.Log("[GameEventLogger] === Shutting Down Event Subscriptions ===");

        // Rassa events
        GameEventDispatcher.UnSubscribe<RassaPromptEvent>(this.OnRassaPrompt);
        GameEventDispatcher.UnSubscribe<RassaResponseEvent>(this.OnRassaResponse);
        GameEventDispatcher.UnSubscribe<RassaChoiceCompleteEvent>(this.OnRassaChoiceComplete);

        // Assaa events
        GameEventDispatcher.UnSubscribe<AssaaPromptEvent>(this.OnAssaaPrompt);
        GameEventDispatcher.UnSubscribe<AssaaResponseEvent>(this.OnAssaaResponse);
        GameEventDispatcher.UnSubscribe<AssaaReorderCompleteEvent>(this.OnAssaaReorderComplete);
        GameEventDispatcher.UnSubscribe<AssaaProcessCompleteEvent>(this.OnAssaaProcessComplete);

        // Project events
        GameEventDispatcher.UnSubscribe<ProjectDeclarationStartEvent>(this.OnProjectDeclarationStart);
        GameEventDispatcher.UnSubscribe<ProjectDeclaredEvent>(this.OnProjectDeclared);
        GameEventDispatcher.UnSubscribe<ProjectDeclarationCompleteEvent>(this.OnProjectDeclarationComplete);
        GameEventDispatcher.UnSubscribe<BeloteDeclaredEvent>(this.OnBeloteDeclared);
        GameEventDispatcher.UnSubscribe<ProjectScoredEvent>(this.OnProjectScored);

        // Bidding events
        GameEventDispatcher.UnSubscribe<BiddingStartEvent>(this.OnBiddingStart);
        GameEventDispatcher.UnSubscribe<BidSubmittedEvent>(this.OnBidSubmitted);
        GameEventDispatcher.UnSubscribe<BiddingCompleteEvent>(this.OnBiddingComplete);
        GameEventDispatcher.UnSubscribe<BiddingRound2StartEvent>(this.OnBiddingRound2Start);
        GameEventDispatcher.UnSubscribe<MultiplierBiddingStartEvent>(this.OnMultiplierBiddingStart);

        // Card play events
        GameEventDispatcher.UnSubscribe<BeloteCard.Played>(this.OnCardPlayed);

        // Round/Turn events
        GameEventDispatcher.UnSubscribe<GameStage.NewRoundEvent>(this.OnNewRound);
        GameEventDispatcher.UnSubscribe<GameStage.NewTurnEvent>(this.OnNewTurn);
        GameEventDispatcher.UnSubscribe<GameStage.CardsCollectedEvent>(this.OnCardsCollected);
        GameEventDispatcher.UnSubscribe<GameStage.RoundEndScoreEvent>(this.OnRoundEndScore);
        GameEventDispatcher.UnSubscribe<GameStage.FoldWinnerEvent>(this.OnFoldWinner);

        // Sawa events
        GameEventDispatcher.UnSubscribe<SawaAvailableEvent>(this.OnSawaAvailable);
        GameEventDispatcher.UnSubscribe<SawaClaimedEvent>(this.OnSawaClaimed);

        Debug.Log("[GameEventLogger] === Event Unsubscriptions Complete ===");
    }

    //----------------------------------------------
    // EVENT HANDLERS
    //----------------------------------------------

    //----------------------------------------------
    // 1. RASSA EVENTS
    //----------------------------------------------
    private void OnRassaPrompt(RassaPromptEvent evt)
    {
        Debug.Log("╔════════════════════════════════════════════════════════════════╗");
        Debug.Log("║                    RASSA PROMPT EVENT                          ║");
        Debug.Log("╠════════════════════════════════════════════════════════════════╣");
        Debug.Log($"║ Player Being Asked: {evt.AskingPlayer?.Name ?? "Unknown"}");
        Debug.Log($"║ Position: {evt.AskingPlayer?.Position}");
        Debug.Log($"║ Team: {evt.AskingPlayer?.Team}");
        Debug.Log($"║ Round Number: {evt.RoundNumber}");
        Debug.Log("╚════════════════════════════════════════════════════════════════╝");
    }

    private void OnRassaResponse(RassaResponseEvent evt)
    {
        string choice = evt.UseRassa ? "✓ YES - Use Rassa" : "✗ NO - Random Deck";
        Debug.Log("╔════════════════════════════════════════════════════════════════╗");
        Debug.Log("║                    RASSA CHOICE MADE                           ║");
        Debug.Log("╠════════════════════════════════════════════════════════════════╣");
        Debug.Log($"║ Player: {evt.RespondingPlayer?.Name ?? "Unknown"}");
        Debug.Log($"║ Position: {evt.RespondingPlayer?.Position}");
        Debug.Log($"║ Team: {evt.RespondingPlayer?.Team}");
        Debug.Log($"║ Choice: {choice}");
        Debug.Log("╚════════════════════════════════════════════════════════════════╝");
    }

    private void OnRassaChoiceComplete(RassaChoiceCompleteEvent evt)
    {
        Debug.Log("╔════════════════════════════════════════════════════════════════╗");
        Debug.Log("║                RASSA CHOICE FINALIZED                          ║");
        Debug.Log("╠════════════════════════════════════════════════════════════════╣");
        Debug.Log($"║ Using Rassa: {(evt.UseRassa ? "YES" : "NO")}");
        Debug.Log($"║ Already Applied: {(evt.AlreadyApplied ? "YES" : "NO")}");
        Debug.Log("║ Status: Ready to deal cards");
        Debug.Log("╚════════════════════════════════════════════════════════════════╝");
    }

    //----------------------------------------------
    // 2. ASSAA EVENTS (ASSA)
    //----------------------------------------------
    private void OnAssaaPrompt(AssaaPromptEvent evt)
    {
        string promptType = evt.PromptNumber == 1 ? "Right Player" : "Teammate";
        Debug.Log("╔════════════════════════════════════════════════════════════════╗");
        Debug.Log("║                    ASSAA PROMPT EVENT                          ║");
        Debug.Log("╠════════════════════════════════════════════════════════════════╣");
        Debug.Log($"║ Prompt Type: {promptType} (#{evt.PromptNumber})");
        Debug.Log($"║ Player Being Asked: {evt.AskingPlayer?.Name ?? "Unknown"}");
        Debug.Log($"║ Position: {evt.AskingPlayer?.Position}");
        Debug.Log($"║ Team: {evt.AskingPlayer?.Team}");
        Debug.Log($"║ Rassa Chooser: {evt.RassaChooser?.Name ?? "Unknown"}");
        Debug.Log("╚════════════════════════════════════════════════════════════════╝");
    }

    private void OnAssaaResponse(AssaaResponseEvent evt)
    {
        string choice = evt.UseAssaa ? "✓ YES - Use Assaa" : "✗ NO - Decline Assaa";
        string promptType = evt.PromptNumber == 1 ? "Right Player" : "Teammate";
        Debug.Log("╔════════════════════════════════════════════════════════════════╗");
        Debug.Log("║                    ASSAA RESPONSE                              ║");
        Debug.Log("╠════════════════════════════════════════════════════════════════╣");
        Debug.Log($"║ Player: {evt.RespondingPlayer?.Name ?? "Unknown"}");
        Debug.Log($"║ Position: {evt.RespondingPlayer?.Position}");
        Debug.Log($"║ Team: {evt.RespondingPlayer?.Team}");
        Debug.Log($"║ Prompt Type: {promptType}");
        Debug.Log($"║ Choice: {choice}");
        Debug.Log("╚════════════════════════════════════════════════════════════════╝");
    }

    private void OnAssaaReorderComplete(AssaaReorderCompleteEvent evt)
    {
        Debug.Log("╔════════════════════════════════════════════════════════════════╗");
        Debug.Log("║                ASSAA CARD REORDERING COMPLETE                  ║");
        Debug.Log("╠════════════════════════════════════════════════════════════════╣");
        Debug.Log($"║ Player: {evt.ReorderingPlayer?.Name ?? "Unknown"}");
        Debug.Log($"║ Position: {evt.ReorderingPlayer?.Position}");
        Debug.Log($"║ Team: {evt.ReorderingPlayer?.Team}");
        Debug.Log($"║ Success: {(evt.Success ? "YES - Cards reordered" : "NO - Cancelled")}");
        
        if (evt.Success)
        {
            Debug.Log("║ Result: Deck has been reordered");
            Debug.Log("║ Note: New card order affects dealing");
        }
        else
        {
            Debug.Log("║ Result: Card reordering was cancelled");
        }
        
        Debug.Log("╚════════════════════════════════════════════════════════════════╝");
    }

    private void OnAssaaProcessComplete(AssaaProcessCompleteEvent evt)
    {
        Debug.Log("╔════════════════════════════════════════════════════════════════╗");
        Debug.Log("║                ASSAA PROCESS FINALIZED                         ║");
        Debug.Log("╠════════════════════════════════════════════════════════════════╣");
        Debug.Log($"║ Assaa Was Used: {(evt.AssaaWasUsed ? "YES" : "NO")}");
        Debug.Log("║ Status: Ready to continue with card dealing");
        Debug.Log("╚════════════════════════════════════════════════════════════════╝");
    }

    //----------------------------------------------
    // 3. PROJECT (MASHARIE3) EVENTS
    //----------------------------------------------
    private void OnProjectDeclarationStart(ProjectDeclarationStartEvent evt)
    {
        Debug.Log("╔════════════════════════════════════════════════════════════════╗");
        Debug.Log("║              PROJECT DECLARATION STARTED                       ║");
        Debug.Log("╠════════════════════════════════════════════════════════════════╣");
        Debug.Log($"║ Player: {evt.CurrentPlayer?.Name ?? "Unknown"}");
        Debug.Log($"║ Position: {evt.CurrentPlayer?.Position}");
        Debug.Log($"║ Team: {evt.CurrentPlayer?.Team}");
        Debug.Log($"║ Available Projects: {evt.AvailableProjects?.Count ?? 0}");
        
        if (evt.AvailableProjects != null && evt.AvailableProjects.Count > 0)
        {
            Debug.Log("║ Detected Projects:");
            foreach (var project in evt.AvailableProjects)
            {
                Debug.Log($"║   - {project.Type} ({project.GetPoints()} points)");
            }
        }
        Debug.Log("╚════════════════════════════════════════════════════════════════╝");
    }

    private void OnProjectDeclared(ProjectDeclaredEvent evt)
    {
        bool projectExists = evt.Project != null;
        Debug.Log("╔════════════════════════════════════════════════════════════════╗");
        Debug.Log("║                  PROJECT DECLARED                              ║");
        Debug.Log("╠════════════════════════════════════════════════════════════════╣");
        Debug.Log($"║ Player: {evt.Player?.Name ?? "Unknown"}");
        Debug.Log($"║ Position: {evt.Player?.Position}");
        Debug.Log($"║ Team: {evt.Player?.Team}");
        Debug.Log($"║ Project Type: {(projectExists ? evt.Project.Type.ToString() : "None")}");
        Debug.Log($"║ Project Exists: {(projectExists ? "YES" : "NO")}");
        
        if (projectExists)
        {
            Debug.Log($"║ Project Points: {evt.Project.GetPoints()}");
            Debug.Log($"║ Cards in Project: {evt.Project.Cards?.Count ?? 0}");
        }
        Debug.Log("╚════════════════════════════════════════════════════════════════╝");
    }

    private void OnProjectDeclarationComplete(ProjectDeclarationCompleteEvent evt)
    {
        Debug.Log("╔════════════════════════════════════════════════════════════════╗");
        Debug.Log("║           PROJECT DECLARATION COMPLETE                         ║");
        Debug.Log("╠════════════════════════════════════════════════════════════════╣");
        Debug.Log($"║ Valid Projects: {evt.ValidProjects?.Count ?? 0}");
        Debug.Log($"║ Cancelled Projects: {evt.CancelledProjects?.Count ?? 0}");
        
        if (evt.ValidProjects != null && evt.ValidProjects.Count > 0)
        {
            Debug.Log("║ Valid Projects:");
            foreach (var project in evt.ValidProjects)
            {
                Debug.Log($"║   - {project.Owner?.Name}: {project.Type} ({project.GetPoints()} points)");
            }
        }
        
        if (evt.CancelledProjects != null && evt.CancelledProjects.Count > 0)
        {
            Debug.Log("║ Cancelled Projects:");
            foreach (var project in evt.CancelledProjects)
            {
                Debug.Log($"║   - {project.Owner?.Name}: {project.Type}");
            }
        }
        Debug.Log("╚════════════════════════════════════════════════════════════════╝");
    }

    private void OnBeloteDeclared(BeloteDeclaredEvent evt)
    {
        Debug.Log("╔════════════════════════════════════════════════════════════════╗");
        Debug.Log("║                  ★ BELOTE DECLARED ★                          ║");
        Debug.Log("╠════════════════════════════════════════════════════════════════╣");
        Debug.Log($"║ Player: {evt.Player?.Name ?? "Unknown"}");
        Debug.Log($"║ Team: {evt.Player?.Team}");
        Debug.Log($"║ Points: 20");
        Debug.Log("╚════════════════════════════════════════════════════════════════╝");
    }

    private void OnProjectScored(ProjectScoredEvent evt)
    {
        Debug.Log("╔════════════════════════════════════════════════════════════════╗");
        Debug.Log("║                  PROJECTS SCORED                               ║");
        Debug.Log("╠════════════════════════════════════════════════════════════════╣");
        Debug.Log($"║ Team: {evt.Team}");
        Debug.Log($"║ Total Points Awarded: {evt.TotalPoints}");
        Debug.Log($"║ Number of Projects: {evt.Projects?.Count ?? 0}");
        
        if (evt.Projects != null && evt.Projects.Count > 0)
        {
            Debug.Log("║ Projects:");
            foreach (var project in evt.Projects)
            {
                Debug.Log($"║   - {project.Owner?.Name}: {project.Type} ({project.GetPoints()} points)");
            }
        }
        
        Debug.Log("╚════════════════════════════════════════════════════════════════╝");
    }

    //----------------------------------------------
    // 4. BIDDING EVENTS
    //----------------------------------------------
    private void OnBiddingStart(BiddingStartEvent evt)
    {
        Debug.Log("╔════════════════════════════════════════════════════════════════╗");
        Debug.Log("║                  BIDDING STARTED                               ║");
        Debug.Log("╠════════════════════════════════════════════════════════════════╣");
        Debug.Log($"║ Round: {evt.Round}");
        Debug.Log($"║ Current Bidder: {evt.CurrentBidder?.Name ?? "Unknown"}");
        Debug.Log($"║ Position: {evt.CurrentBidder?.Position}");
        Debug.Log($"║ Team: {evt.CurrentBidder?.Team}");
        
        if (evt.FaceUpCard != null)
        {
            Debug.Log($"║ Face-Up Card: {evt.FaceUpCard.Value} of {evt.FaceUpCard.Family}");
        }
        
        if (evt.HighestBid != null && !evt.HighestBid.IsPass)
        {
            Debug.Log($"║ Current Highest Bid: {evt.HighestBid}");
        }
        Debug.Log("╚════════════════════════════════════════════════════════════════╝");
    }

    private void OnBidSubmitted(BidSubmittedEvent evt)
    {
        Debug.Log("╔════════════════════════════════════════════════════════════════╗");
        Debug.Log("║                    BID SUBMITTED                               ║");
        Debug.Log("╠════════════════════════════════════════════════════════════════╣");
        Debug.Log($"║ Player: {evt.Player?.Name ?? "Unknown"}");
        Debug.Log($"║ Position: {evt.Player?.Position}");
        Debug.Log($"║ Team: {evt.Player?.Team}");
        Debug.Log($"║ Bid: {(evt.Bid != null ? evt.Bid.ToString() : "None")}");
        Debug.Log("╚════════════════════════════════════════════════════════════════╝");
    }

    private void OnBiddingComplete(BiddingCompleteEvent evt)
    {
        Debug.Log("╔════════════════════════════════════════════════════════════════╗");
        Debug.Log("║              ★★★ BIDDING COMPLETE ★★★                         ║");
        Debug.Log("╠════════════════════════════════════════════════════════════════╣");
        
        if (evt.WinningBidder != null && evt.WinningBid != null)
        {
            Debug.Log($"║ Winner: {evt.WinningBidder.Name}");
            Debug.Log($"║ Position: {evt.WinningBidder.Position}");
            Debug.Log($"║ Team: {evt.WinningBidder.Team}");
            Debug.Log($"║ Winning Bid: {evt.WinningBid}");
            Debug.Log($"║ Game Type: {(evt.SunDeclared ? "SUN (No Trump)" : "TRUMP")}");
            
            if (!evt.SunDeclared && evt.WinningBid.IsTrump)
            {
                Debug.Log($"║ Trump Suit: {evt.WinningBid.Suit}");
            }
            
            // Determine referee type based on bid
            string refereeType = "Unknown";
            if (evt.SunDeclared)
            {
                refereeType = "Sun";
            }
            else if (evt.WinningBid.IsTrump)
            {
                refereeType = $"Trump ({evt.WinningBid.Suit})";
            }
            Debug.Log($"║ Referee Type: {refereeType}");
        }
        else
        {
            Debug.Log("║ Result: No contract made - all players passed");
        }
        
        Debug.Log("╚════════════════════════════════════════════════════════════════╝");
    }

    private void OnBiddingRound2Start(BiddingRound2StartEvent evt)
    {
        Debug.Log("╔════════════════════════════════════════════════════════════════╗");
        Debug.Log("║              BIDDING ROUND 2 STARTED                           ║");
        Debug.Log("╠════════════════════════════════════════════════════════════════╣");
        Debug.Log($"║ Current Bidder: {evt.CurrentBidder?.Name ?? "Unknown"}");
        
        if (evt.TrumpTaker != null)
        {
            Debug.Log($"║ Trump Taker (Round 1): {evt.TrumpTaker.Name}");
        }
        
        if (evt.FaceUpCard != null)
        {
            Debug.Log($"║ Face-Up Card: {evt.FaceUpCard.Value} of {evt.FaceUpCard.Family}");
            Debug.Log($"║ Cannot Choose: {evt.FaceUpCard.Family} (face-up suit)");
        }
        
        Debug.Log("╚════════════════════════════════════════════════════════════════╝");
    }

    private void OnMultiplierBiddingStart(MultiplierBiddingStartEvent evt)
    {
        Debug.Log("╔════════════════════════════════════════════════════════════════╗");
        Debug.Log("║           MULTIPLIER BIDDING STARTED                           ║");
        Debug.Log("╠════════════════════════════════════════════════════════════════╣");
        Debug.Log($"║ Current Bidder: {evt.CurrentBidder?.Name ?? "Unknown"}");
        Debug.Log($"║ Trump Confirmer: {evt.TrumpConfirmer?.Name ?? "Unknown"}");
        Debug.Log($"║ Current Multiplier: {evt.CurrentMultiplier}x");
        Debug.Log($"║ Opposing Team Turn: {(evt.IsOpposingTeamTurn ? "YES" : "NO")}");
        Debug.Log("╚════════════════════════════════════════════════════════════════╝");
    }

    //----------------------------------------------
    // 5. CARD PLAY EVENTS
    //----------------------------------------------
    private void OnCardPlayed(BeloteCard.Played evt)
    {
        Player player = evt.Card?.Owner as Player;
        Debug.Log("╔════════════════════════════════════════════════════════════════╗");
        Debug.Log("║                    CARD PLAYED                                 ║");
        Debug.Log("╠════════════════════════════════════════════════════════════════╣");
        Debug.Log($"║ Player: {player?.Name ?? "Unknown"}");
        Debug.Log($"║ Position: {player?.Position}");
        Debug.Log($"║ Team: {player?.Team}");
        Debug.Log($"║ Card: {evt.Card?.Value} of {evt.Card?.Family}");
        
        if (Stage != null && Stage.CurrentFold != null)
        {
            Debug.Log($"║ Cards in Current Fold: {Stage.CurrentFold.Deck.Size}");
            
            // If fold is complete (4 cards), log the winner
            if (Stage.CurrentFold.Deck.Size == 4)
            {
                // Fold will be finalized shortly - we'll log winner in OnNewTurn
                Debug.Log("║ Status: Fold complete - determining winner...");
            }
        }
        Debug.Log("╚════════════════════════════════════════════════════════════════╝");
    }

    //----------------------------------------------
    // 6. ROUND/TURN EVENTS
    //----------------------------------------------
    private void OnNewRound(GameStage.NewRoundEvent evt)
    {
        if (evt.Start)
        {
            Debug.Log("╔════════════════════════════════════════════════════════════════╗");
            Debug.Log("║              ★★★ NEW ROUND STARTED ★★★                        ║");
            Debug.Log("╠════════════════════════════════════════════════════════════════╣");
            
            if (Stage != null)
            {
                Debug.Log($"║ Dealer: {Stage.Dealer?.Name ?? "Unknown"}");
                Debug.Log($"║ Bidder: {Stage.Bidder?.Name ?? "Unknown"}");
                Debug.Log($"║ Trump: {(Stage.Trump != null ? Stage.Trump.ToString() : "Sun (No Trump)")}");
                Debug.Log($"║ Round First Player: {Stage.RoundFirstPlayer?.Name ?? "Unknown"}");
                
                // Log all players' hand sizes (should be 8 after full deal)
                Debug.Log("║");
                Debug.Log("║ Cards Dealt to Each Player (Total: 8):");
                foreach (var player in Stage.Players)
                {
                    int handSize = player.Hand?.Size ?? 0;
                    string statusIcon = handSize == 8 ? "✓" : "⚠";
                    Debug.Log($"║   {statusIcon} {player.Name} ({player.Position}): {handSize} cards");
                }
            }
            
            Debug.Log("╚════════════════════════════════════════════════════════════════╝");
        }
        else
        {
            Debug.Log("╔════════════════════════════════════════════════════════════════╗");
            Debug.Log("║                   ROUND ENDED                                  ║");
            Debug.Log("╚════════════════════════════════════════════════════════════════╝");
        }
    }

    private void OnNewTurn(GameStage.NewTurnEvent evt)
    {
        Debug.Log("╔════════════════════════════════════════════════════════════════╗");
        Debug.Log("║                    NEW TURN                                    ║");
        Debug.Log("╠════════════════════════════════════════════════════════════════╣");
        Debug.Log($"║ Current Player: {evt.Current?.Name ?? "Unknown"}");
        Debug.Log($"║ Position: {evt.Current?.Position}");
        Debug.Log($"║ Team: {evt.Current?.Team}");
        Debug.Log($"║ Cards in Hand: {evt.Current?.Hand?.Size ?? 0}");
        Debug.Log("╚════════════════════════════════════════════════════════════════╝");
    }
    
    private void OnFoldWinner(GameStage.FoldWinnerEvent evt)
    {
        Debug.Log("╔════════════════════════════════════════════════════════════════╗");
        Debug.Log("║              ★★★ FOLD WINNER ★★★                              ║");
        Debug.Log("╠════════════════════════════════════════════════════════════════╣");
        Debug.Log($"║ Winner: {evt.Winner?.Name ?? "Unknown"}");
        Debug.Log($"║ Position: {evt.Winner?.Position}");
        Debug.Log($"║ Team: {evt.WinningTeam}");
        Debug.Log($"║ Fold Points: {evt.FoldPoints}");
        Debug.Log($"║ Cards in Fold: {evt.CardsInFold}");
        Debug.Log("║ Next Action: Winner leads the next fold");
        Debug.Log("╚════════════════════════════════════════════════════════════════╝");
    }

    private void OnCardsCollected(GameStage.CardsCollectedEvent evt)
    {
        Debug.Log("╔════════════════════════════════════════════════════════════════╗");
        Debug.Log("║                CARDS COLLECTED TO DECK                         ║");
        Debug.Log("╠════════════════════════════════════════════════════════════════╣");
        Debug.Log("║ All cards have been returned to the deck");
        Debug.Log("║ Deck will be shuffled for next round");
        Debug.Log("╚════════════════════════════════════════════════════════════════╝");
    }

    private void OnRoundEndScore(GameStage.RoundEndScoreEvent evt)
    {
        Debug.Log("╔════════════════════════════════════════════════════════════════╗");
        Debug.Log("║              ★★★ ROUND SCORE ★★★                              ║");
        Debug.Log("╠════════════════════════════════════════════════════════════════╣");
        Debug.Log("║ === RAW POINTS ===");
        Debug.Log($"║ Team 1: {evt.Team1RawPoints} points");
        Debug.Log($"║ Team 2: {evt.Team2RawPoints} points");
        Debug.Log("║");
        Debug.Log("║ === ROUND SCORE (÷10 and multiplier applied) ===");
        Debug.Log($"║ Team 1: +{evt.Team1RoundScore} points");
        Debug.Log($"║ Team 2: +{evt.Team2RoundScore} points");
        Debug.Log("║");
        Debug.Log($"║ Bidding Team: {evt.BiddingTeam}");
        Debug.Log($"║ Winning Team: {evt.WinningTeam}");
        Debug.Log($"║ Multiplier: {evt.Multiplier}x");
        Debug.Log($"║ Kaboot (All Tricks): {(evt.IsKaboot ? "YES" : "NO")}");
        Debug.Log("║");
        Debug.Log("╠════════════════════════════════════════════════════════════════╣");
        Debug.Log("║              ★★★ CUMULATIVE GAME SCORE ★★★                    ║");
        Debug.Log("╠════════════════════════════════════════════════════════════════╣");
        Debug.Log($"║ Team 1 Total: {evt.Team1CumulativeScore} points");
        Debug.Log($"║ Team 2 Total: {evt.Team2CumulativeScore} points");
        
        // Determine who is leading
        if (evt.Team1CumulativeScore > evt.Team2CumulativeScore)
        {
            int diff = evt.Team1CumulativeScore - evt.Team2CumulativeScore;
            Debug.Log($"║ Leading: Team 1 by {diff} points");
        }
        else if (evt.Team2CumulativeScore > evt.Team1CumulativeScore)
        {
            int diff = evt.Team2CumulativeScore - evt.Team1CumulativeScore;
            Debug.Log($"║ Leading: Team 2 by {diff} points");
        }
        else
        {
            Debug.Log("║ Status: TIED");
        }
        
        Debug.Log("╚════════════════════════════════════════════════════════════════╝");
    }

    //----------------------------------------------
    // 7. SAWA EVENTS
    //----------------------------------------------
    private void OnSawaAvailable(SawaAvailableEvent evt)
    {
        Debug.Log("╔════════════════════════════════════════════════════════════════╗");
        Debug.Log("║                  SAWA ELIGIBILITY                              ║");
        Debug.Log("╠════════════════════════════════════════════════════════════════╣");
        Debug.Log($"║ Player: {evt.Player?.Name ?? "Unknown"}");
        Debug.Log($"║ Position: {evt.Player?.Position}");
        Debug.Log($"║ Team: {evt.Player?.Team}");
        Debug.Log($"║ Eligible for Sawa: {(evt.IsAvailable ? "YES ✓" : "NO ✗")}");
        
        if (evt.IsAvailable)
        {
            Debug.Log("║ Status: Player can claim Sawa");
        }
        else
        {
            Debug.Log("║ Status: Sawa not available for this player");
        }
        
        Debug.Log("╚════════════════════════════════════════════════════════════════╝");
    }

    private void OnSawaClaimed(SawaClaimedEvent evt)
    {
        Debug.Log("╔════════════════════════════════════════════════════════════════╗");
        Debug.Log("║              ★★★ SAWA CLAIMED! ★★★                            ║");
        Debug.Log("╠════════════════════════════════════════════════════════════════╣");
        Debug.Log($"║ Player: {evt.Player?.Name ?? "Unknown"}");
        Debug.Log($"║ Position: {evt.Player?.Position}");
        Debug.Log($"║ Team: {evt.Player?.Team}");
        Debug.Log("║ Result: Trump changes to player's hand");
        Debug.Log("╚════════════════════════════════════════════════════════════════╝");
    }
}

