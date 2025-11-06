using System.Collections.Generic;
using System.Linq;
using Pebble;
using UnityEngine;

//-------------------------------------------------------
// ProjectDetector
//-------------------------------------------------------
// Purpose:
//   Scans a player's hand and detects all possible projects.
//   Handles all project types: Sara, Khamsin, Mia, Arba'miya.
//   (Belote is detected during gameplay separately)
//-------------------------------------------------------
public static class ProjectDetector
{
    //----------------------------------------------
    // DetectAllProjects
    //----------------------------------------------
    // Scans a player's hand and returns all valid projects
    // Only returns the BEST project of each type to avoid conflicts
    public static List<Project> DetectAllProjects(Player player, bool isSunRound)
    {
        List<Project> allProjects = new List<Project>();

        if (player == null || player.Hand == null || player.Hand.Empty)
            return allProjects;

        // Check for sequences (Sara, Khamsin, Mia)
        List<Project> sequenceProjects = DetectSequences(player);
        
        // Check for 4 of a kind (Mia)
        List<Project> fourOfKindProjects = DetectFourOfKind(player);
        
        // Check for 4 Aces (Mia in Hukm, Arba'miya in Sun)
        Project acesProject = DetectFourAces(player, isSunRound);

        // Combine all projects
        allProjects.AddRange(sequenceProjects);
        allProjects.AddRange(fourOfKindProjects);
        if (acesProject != null)
            allProjects.Add(acesProject);

        // Only keep the best project per type to simplify UI
        allProjects = FilterBestProjects(allProjects);

        return allProjects;
    }

    //----------------------------------------------
    // DetectSequences
    //----------------------------------------------
    // Detects Sara (3), Khamsin (4), and Mia (5) consecutive cards
    private static List<Project> DetectSequences(Player player)
    {
        List<Project> sequences = new List<Project>();

        // Group cards by suit
        var cardsBySuit = player.Hand.Cards
            .GroupBy(c => c.Family)
            .Where(g => g.Count() >= 3); // Need at least 3 for Sara

        foreach (var suitGroup in cardsBySuit)
        {
            // Sort cards by rank
            var sortedCards = suitGroup
                .OrderBy(c => GetCardRankForSequence(c.Value))
                .ToList();

            // Find longest consecutive sequence
            List<BeloteCard> currentSequence = new List<BeloteCard>();
            
            for (int i = 0; i < sortedCards.Count; i++)
            {
                if (currentSequence.Count == 0)
                {
                    currentSequence.Add(sortedCards[i]);
                }
                else
                {
                    int lastRank = GetCardRankForSequence(currentSequence[currentSequence.Count - 1].Value);
                    int currentRank = GetCardRankForSequence(sortedCards[i].Value);

                    // Check if consecutive
                    if (currentRank == lastRank + 1)
                    {
                        currentSequence.Add(sortedCards[i]);
                    }
                    else
                    {
                        // Break in sequence - check if we have a valid project
                        AddSequenceProject(sequences, currentSequence, player);
                        currentSequence.Clear();
                        currentSequence.Add(sortedCards[i]);
                    }
                }
            }

            // Check final sequence
            AddSequenceProject(sequences, currentSequence, player);
        }

        return sequences;
    }

    //----------------------------------------------
    // AddSequenceProject
    //----------------------------------------------
    // Helper to add a sequence project if it's valid
    private static void AddSequenceProject(List<Project> projects, List<BeloteCard> sequence, Player player)
    {
        if (sequence.Count >= 5)
        {
            // Mia (100) - 5 consecutive
            projects.Add(new Project(ProjectType.Mia, sequence.Take(5).ToList(), player));
        }
        else if (sequence.Count == 4)
        {
            // Khamsin (50) - 4 consecutive
            projects.Add(new Project(ProjectType.Khamsin, sequence, player));
        }
        else if (sequence.Count == 3)
        {
            // Sara (20) - 3 consecutive
            projects.Add(new Project(ProjectType.Sara, sequence, player));
        }
    }

    //----------------------------------------------
    // DetectFourOfKind
    //----------------------------------------------
    // Detects 4 cards of the same rank (10, J, Q, K only) = Mia (100)
    private static List<Project> DetectFourOfKind(Player player)
    {
        List<Project> fourOfKind = new List<Project>();

        // Valid ranks for 4 of a kind: Ten, Jack, Queen, King
        Card32Value[] validRanks = { Card32Value.Ten, Card32Value.Jack, Card32Value.Queen, Card32Value.King };

        foreach (var rank in validRanks)
        {
            var matchingCards = player.Hand.Cards
                .Where(c => c.Value == rank)
                .ToList();

            if (matchingCards.Count == 4)
            {
                fourOfKind.Add(new Project(ProjectType.Mia, matchingCards, player));
            }
        }

        return fourOfKind;
    }

    //----------------------------------------------
    // DetectFourAces
    //----------------------------------------------
    // Detects 4 Aces: Mia (100) in Hukm, Arba'miya (400) in Sun
    private static Project DetectFourAces(Player player, bool isSunRound)
    {
        var aces = player.Hand.Cards
            .Where(c => c.Value == Card32Value.Ace)
            .ToList();

        if (aces.Count == 4)
        {
            ProjectType type = isSunRound ? ProjectType.Arbamiya : ProjectType.Mia;
            return new Project(type, aces, player);
        }

        return null;
    }

    //----------------------------------------------
    // FilterBestProjects
    //----------------------------------------------
    // Keeps only the best project per type (highest points)
    private static List<Project> FilterBestProjects(List<Project> projects)
    {
        // Group by type and keep the best one
        var bestProjects = projects
            .GroupBy(p => p.Type)
            .Select(g => g.OrderByDescending(p => p.GetPoints()).First())
            .ToList();

        return bestProjects;
    }

    //----------------------------------------------
    // GetCardRankForSequence
    //----------------------------------------------
    // Returns sequential rank for detecting consecutive cards
    // Note: In Baloot, the sequence is 7-8-9-10-J-Q-K-A
    private static int GetCardRankForSequence(Card32Value value)
    {
        switch (value)
        {
            case Card32Value.Seven: return 7;
            case Card32Value.Eight: return 8;
            case Card32Value.Nine: return 9;
            case Card32Value.Ten: return 10;
            case Card32Value.Jack: return 11;
            case Card32Value.Queen: return 12;
            case Card32Value.King: return 13;
            case Card32Value.Ace: return 14;
            default: return 0;
        }
    }

    //----------------------------------------------
    // CanDeclareBelote
    //----------------------------------------------
    // Checks if a player can declare Belote (has both K and Q of trump)
    public static bool CanDeclareBelote(Player player, Card32Family trumpSuit)
    {
        bool hasKing = player.Hand.Cards.Any(c => c.Value == Card32Value.King && c.Family == trumpSuit);
        bool hasQueen = player.Hand.Cards.Any(c => c.Value == Card32Value.Queen && c.Family == trumpSuit);
        
        return hasKing && hasQueen;
    }

    //----------------------------------------------
    // ShouldDeclareBeloteNow
    //----------------------------------------------
    // Checks if playing this card should trigger a Belote declaration
    public static bool ShouldDeclareBeloteNow(BeloteCard cardPlayed, Player player, Card32Family trumpSuit)
    {
        // Check if the card played is K or Q of trump
        if (cardPlayed.Family != trumpSuit)
            return false;

        if (cardPlayed.Value != Card32Value.King && cardPlayed.Value != Card32Value.Queen)
            return false;

        // Check if player still has the other card in hand (before the play)
        Card32Value neededCard = (cardPlayed.Value == Card32Value.King) 
            ? Card32Value.Queen 
            : Card32Value.King;

        bool hasOtherCard = player.Hand.Cards.Any(c => 
            c.Value == neededCard && 
            c.Family == trumpSuit && 
            c != cardPlayed);

        return hasOtherCard;
    }
}

