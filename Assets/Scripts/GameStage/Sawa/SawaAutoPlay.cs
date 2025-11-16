using System.Collections.Generic;
using UnityEngine;
using Pebble;

//-------------------------------------------------------
// SawaAutoPlay
//-------------------------------------------------------
// Purpose:
//   Handles the automatic playing out of remaining tricks when
//   a player claims "Sawa". This simulates playing all remaining
//   cards and awards the appropriate tricks to the claiming player.
//
// How it connects to other scripts:
//   - Called by `GameStage` when a player claims Sawa
//   - Simulates card plays and fold wins
//   - Awards points to the claiming player's team
//-------------------------------------------------------
public class SawaAutoPlay
{
    //-------------------------------------------------------
    // Automatically resolve all remaining tricks in favor of the claiming player
    // 
    // Parameters:
    //   claimingPlayer - The player who claimed Sawa
    //   currentFold - The current fold (may be partially played)
    //   trump - The trump suit for this round
    //   allPlayers - All players in the game
    //   pastFolds - Array of past folds per team (to add completed folds to)
    //   gameStage - Reference to the game stage for accessing deck and other state
    //
    // Returns:
    //   The player who will lead the next action (should be null as round ends)
    //-------------------------------------------------------
    public static void AutoResolveRemainingTricks(
        Player claimingPlayer, 
        Fold currentFold, 
        Card32Family? trump, 
        List<Player> allPlayers,
        List<Fold>[] pastFolds,
        GameStage gameStage)
    {
        Debug.Log($"[SawaAutoPlay] {claimingPlayer.Name} claimed Sawa! Auto-resolving remaining tricks...");
        
        PlayerTeam claimingTeam = claimingPlayer.Team;
        int tricksAutoResolved = 0;
        int pointsGained = 0;

        // Step 1: If there's a partially completed fold, complete it in favor of claiming player
        if (currentFold.Deck.Size > 0 && currentFold.Deck.Size < allPlayers.Count)
        {
            Debug.Log($"[SawaAutoPlay] Completing current partially-played fold ({currentFold.Deck.Size} cards)");
            CompleteFoldForPlayer(claimingPlayer, currentFold, trump, allPlayers, pastFolds, ref tricksAutoResolved, ref pointsGained);
        }

        // Step 2: Play out all remaining tricks automatically
        int remainingTricks = claimingPlayer.Hand.Size;
        Debug.Log($"[SawaAutoPlay] Auto-resolving {remainingTricks} remaining tricks");

        for (int i = 0; i < remainingTricks; i++)
        {
            // Create a new fold for this trick
            Fold autoFold = new Fold();
            
            // Move all cards from all players to this fold (simulating the trick)
            foreach (Player player in allPlayers)
            {
                if (player.Hand.Size > 0)
                {
                    BeloteCard card = player.Hand.Cards[0]; // Take first card (any card will do)
                    player.Hand.MoveCardTo(card, autoFold.Deck);
                }
            }

            // Finalize this fold - claiming player wins
            autoFold.Finalize(trump);
            
            // Override winner to be the claiming player (they win all tricks)
            // Actually, we need to award it to whoever would logically win
            // But since Sawa means they CAN win all, we set them as winner
            autoFold.Winner = claimingPlayer;
            
            int foldPoints = autoFold.Points;
            pointsGained += foldPoints;
            
            Debug.Log($"[SawaAutoPlay] Trick {i + 1}/{remainingTricks}: {autoFold.Deck.Size} cards, {foldPoints} points");

            // Add to past folds for the claiming team
            pastFolds[(int)claimingTeam].Add(autoFold);
            tricksAutoResolved++;
        }

        Debug.Log($"[SawaAutoPlay] === Sawa Complete ===");
        Debug.Log($"[SawaAutoPlay] {claimingPlayer.Name} ({claimingTeam}) won {tricksAutoResolved} tricks automatically");
        Debug.Log($"[SawaAutoPlay] Total points gained: {pointsGained}");
        Debug.Log($"[SawaAutoPlay] Plus 10 de der will be awarded to {claimingTeam}");
    }

    //-------------------------------------------------------
    // Complete a partially-played fold in favor of the claiming player
    //-------------------------------------------------------
    private static void CompleteFoldForPlayer(
        Player claimingPlayer, 
        Fold currentFold, 
        Card32Family? trump, 
        List<Player> allPlayers,
        List<Fold>[] pastFolds,
        ref int tricksAutoResolved,
        ref int pointsGained)
    {
        // Find which players still need to play in this fold
        List<Player> playersWhoPlayed = new List<Player>();
        foreach (BeloteCard card in currentFold.Deck.Cards)
        {
            if (card.Owner is Player player)
            {
                playersWhoPlayed.Add(player);
            }
        }

        // Complete the fold by playing remaining players' cards
        foreach (Player player in allPlayers)
        {
            if (!playersWhoPlayed.Contains(player) && player.Hand.Size > 0)
            {
                BeloteCard card = player.Hand.Cards[0]; // Play first available card
                player.Hand.MoveCardTo(card, currentFold.Deck);
            }
        }

        // Finalize the fold
        currentFold.Finalize(trump);
        
        // Set the winner to the claiming player (they claimed they can win all)
        currentFold.Winner = claimingPlayer;
        
        int foldPoints = currentFold.Points;
        pointsGained += foldPoints;

        Debug.Log($"[SawaAutoPlay] Completed partial fold: {currentFold.Deck.Size} cards, {foldPoints} points");

        // Move this fold to past folds
        Fold completedFold = new Fold();
        currentFold.MoveTo(completedFold);
        pastFolds[(int)claimingPlayer.Team].Add(completedFold);
        tricksAutoResolved++;
    }
}

