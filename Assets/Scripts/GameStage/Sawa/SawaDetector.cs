using System.Collections.Generic;
using UnityEngine;
using Pebble;

//-------------------------------------------------------
// SawaDetector
//-------------------------------------------------------
// Purpose:
//   Determines whether a player can claim "Sawa" - meaning they
//   are guaranteed to win all remaining tricks with their current hand.
//
// How it connects to other scripts:
//   - Used by `GameStage` to check if the current player can claim Sawa
//   - Analyzes the player's hand, the current fold state, and trump
//   - Uses Belote card comparison logic to determine winnability
//-------------------------------------------------------
public class SawaDetector
{
    //-------------------------------------------------------
    // Check if the given player can win all remaining tricks
    // 
    // Parameters:
    //   player - The player whose hand we're checking
    //   currentFold - The current fold (may be partially played)
    //   trump - The trump suit for this round (null for Sun)
    //   allPlayers - All players in the game
    //-------------------------------------------------------
    public static bool CanClaimSawa(Player player, Fold currentFold, Card32Family? trump, List<Player> allPlayers)
    {
        // Cannot claim Sawa if it's not the player's turn
        // This is redundant as we only check during their turn, but good safety check
        if (player.Hand.Empty)
        {
            return false;
        }

        // Get all cards still in play (not yet won by any team)
        List<BeloteCard> remainingCardsInOtherHands = GetRemainingCardsInOtherHands(player, allPlayers);
        
        // If there are cards in the current fold, we need to check if player can win THIS fold first
        if (currentFold.Deck.Size > 0 && currentFold.Deck.Size < allPlayers.Count)
        {
            // Player must be able to win the current fold to claim Sawa
            if (!CanWinCurrentFold(player, currentFold, trump, remainingCardsInOtherHands))
            {
                return false;
            }
        }

        // Now check if player can win all future folds
        // Strategy: For each remaining card in other players' hands, check if player has a better card
        return CanWinAllFutureFolds(player.Hand.Cards, remainingCardsInOtherHands, trump);
    }

    //-------------------------------------------------------
    // Get all cards that are still in other players' hands
    //-------------------------------------------------------
    private static List<BeloteCard> GetRemainingCardsInOtherHands(Player currentPlayer, List<Player> allPlayers)
    {
        List<BeloteCard> remainingCards = new List<BeloteCard>();
        
        foreach (Player player in allPlayers)
        {
            if (player != currentPlayer)
            {
                foreach (BeloteCard card in player.Hand.Cards)
                {
                    remainingCards.Add(card);
                }
            }
        }
        
        return remainingCards;
    }

    //-------------------------------------------------------
    // Check if player can win the current partially-played fold
    //-------------------------------------------------------
    private static bool CanWinCurrentFold(Player player, Fold currentFold, Card32Family? trump, List<BeloteCard> remainingCards)
    {
        // Get the current best card in the fold
        BeloteCard currentBest = currentFold.GetBest(trump);
        if (currentBest == null)
        {
            return true; // No cards played yet, player leads
        }

        // Check if current best is from player's partner
        Player bestPlayer = currentBest.Owner as Player;
        if (bestPlayer != null && bestPlayer.Team == player.Team)
        {
            return true; // Partner is winning, we can play anything
        }

        // Get the requested suit for this fold
        Card32Family? requestedFamily = currentFold.RequestedFamily;
        
        // Check if player has any card that can beat the current best
        foreach (BeloteCard card in player.Hand.Cards)
        {
            // If we must follow suit, check only those cards
            if (requestedFamily != null)
            {
                bool hasSuit = HasSuit(player.Hand.Cards, (Card32Family)requestedFamily);
                if (hasSuit && card.Family != requestedFamily)
                {
                    continue; // Must follow suit
                }
            }

            // Check if this card beats the current best
            if (BeloteCard.GetBestCard(card, currentBest, trump) == card)
            {
                return true; // Found a winning card
            }
        }

        return false; // Cannot win this fold
    }

    //-------------------------------------------------------
    // Check if player can win all future folds with their hand
    // This is a simplified heuristic - perfect analysis is complex
    //-------------------------------------------------------
    private static bool CanWinAllFutureFolds(List<BeloteCard> playerHand, List<BeloteCard> opponentCards, Card32Family? trump)
    {
        // Quick check: If player has no cards, they can't win anything
        if (playerHand.Count == 0)
        {
            return false;
        }

        // Quick check: If opponents have no cards, player wins by default
        if (opponentCards.Count == 0)
        {
            return true;
        }

        // Calculate the number of tricks remaining
        int tricksRemaining = playerHand.Count; // Each card in hand is one trick

        // Strategy: Check if player has enough "winning power" for all remaining tricks
        // We'll use a greedy simulation approach
        
        // Create working copies of the hands
        List<BeloteCard> workingHand = new List<BeloteCard>(playerHand);
        List<BeloteCard> workingOpponents = new List<BeloteCard>(opponentCards);

        // Simulate each remaining trick
        for (int trickNum = 0; trickNum < tricksRemaining; trickNum++)
        {
            // Find the best card player can lead with (or respond with)
            BeloteCard playerBestCard = GetStrongestCard(workingHand, trump);
            
            if (playerBestCard == null)
            {
                return false; // No cards left
            }

            // Check if any opponent card can beat the player's best card
            // This is a simplified check - in reality, suit following rules apply
            bool canBeBeat = false;
            BeloteCard opponentBestCard = null;

            foreach (BeloteCard opponentCard in workingOpponents)
            {
                // Check if opponent card beats player card
                if (BeloteCard.GetBestCard(opponentCard, playerBestCard, trump) == opponentCard)
                {
                    // If opponent has trump and player doesn't, or opponent has higher trump
                    if (opponentCard.Family == trump || playerBestCard.Family != trump)
                    {
                        canBeBeat = true;
                        opponentBestCard = opponentCard;
                        break;
                    }
                }
            }

            // If player card can be beaten by any opponent card, check if it's with trump
            if (canBeBeat)
            {
                // If player is leading with non-trump and opponent has trump, this could be problematic
                // However, if player has all remaining trumps or all highest cards, they're safe
                
                // Advanced check: Does player have all the highest cards in the suit they're leading?
                if (playerBestCard.Family != trump)
                {
                    // Check if player has unbeatable trumps
                    if (!HasUnbeatableTrumps(workingHand, workingOpponents, trump))
                    {
                        // Player might lose this trick
                        return false;
                    }
                }
                else
                {
                    // Player is leading trump but opponent has higher trump
                    return false;
                }
            }

            // Remove the cards that would be played this trick
            workingHand.Remove(playerBestCard);
            if (opponentBestCard != null)
            {
                workingOpponents.Remove(opponentBestCard);
            }
            else if (workingOpponents.Count > 0)
            {
                // Remove lowest opponent card (they would discard)
                BeloteCard lowestOpponent = GetWeakestCard(workingOpponents, trump);
                if (lowestOpponent != null)
                {
                    workingOpponents.Remove(lowestOpponent);
                }
            }
        }

        return true; // Player can win all tricks
    }

    //-------------------------------------------------------
    // Check if player has all the winning trumps
    //-------------------------------------------------------
    private static bool HasUnbeatableTrumps(List<BeloteCard> playerHand, List<BeloteCard> opponentCards, Card32Family? trump)
    {
        if (trump == null)
        {
            return false; // No trump in Sun games
        }

        // Get all player's trumps
        List<BeloteCard> playerTrumps = GetCardsOfSuit(playerHand, (Card32Family)trump);
        if (playerTrumps.Count == 0)
        {
            return false;
        }

        // Get all opponent trumps
        List<BeloteCard> opponentTrumps = GetCardsOfSuit(opponentCards, (Card32Family)trump);
        
        // If player has all remaining trumps, they're unbeatable
        if (opponentTrumps.Count == 0)
        {
            return true;
        }

        // Check if player's weakest trump beats opponent's strongest trump
        BeloteCard playerWeakestTrump = GetWeakestCard(playerTrumps, trump);
        BeloteCard opponentStrongestTrump = GetStrongestCard(opponentTrumps, trump);

        if (playerWeakestTrump != null && opponentStrongestTrump != null)
        {
            return BeloteCard.GetBestCard(playerWeakestTrump, opponentStrongestTrump, trump) == playerWeakestTrump;
        }

        return false;
    }

    //-------------------------------------------------------
    // Helper: Get the strongest card from a list
    //-------------------------------------------------------
    private static BeloteCard GetStrongestCard(List<BeloteCard> cards, Card32Family? trump)
    {
        if (cards.Count == 0) return null;

        BeloteCard strongest = cards[0];
        foreach (BeloteCard card in cards)
        {
            if (BeloteCard.GetBestCard(card, strongest, trump) == card)
            {
                strongest = card;
            }
        }
        return strongest;
    }

    //-------------------------------------------------------
    // Helper: Get the weakest card from a list
    //-------------------------------------------------------
    private static BeloteCard GetWeakestCard(List<BeloteCard> cards, Card32Family? trump)
    {
        if (cards.Count == 0) return null;

        BeloteCard weakest = cards[0];
        foreach (BeloteCard card in cards)
        {
            if (BeloteCard.GetBestCard(card, weakest, trump) == weakest)
            {
                weakest = card;
            }
        }
        return weakest;
    }

    //-------------------------------------------------------
    // Helper: Get all cards of a specific suit
    //-------------------------------------------------------
    private static List<BeloteCard> GetCardsOfSuit(List<BeloteCard> cards, Card32Family suit)
    {
        List<BeloteCard> result = new List<BeloteCard>();
        foreach (BeloteCard card in cards)
        {
            if (card.Family == suit)
            {
                result.Add(card);
            }
        }
        return result;
    }

    //-------------------------------------------------------
    // Helper: Check if hand has any card of the requested suit
    //-------------------------------------------------------
    private static bool HasSuit(List<BeloteCard> cards, Card32Family suit)
    {
        foreach (BeloteCard card in cards)
        {
            if (card.Family == suit)
            {
                return true;
            }
        }
        return false;
    }
}

