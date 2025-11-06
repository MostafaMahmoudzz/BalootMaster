using System.Collections.Generic;
using System.Linq;
using Pebble;

//-------------------------------------------------------
// Project
//-------------------------------------------------------
// Purpose:
//   Represents a single project (Mashroo3) declared by a player.
//   Contains the type, cards involved, and comparison logic.
//-------------------------------------------------------
public class Project
{
    //----------------------------------------------
    // Properties
    public ProjectType Type { get; set; }
    public List<BeloteCard> Cards { get; set; }
    public Player Owner { get; set; }
    
    // For comparison: the highest-ranking card in the sequence
    public BeloteCard HighestCard 
    { 
        get 
        { 
            if (Cards == null || Cards.Count == 0) return null;
            return Cards.OrderByDescending(c => GetCardRankForComparison(c.Value)).First();
        } 
    }

    //----------------------------------------------
    // Constructor
    public Project(ProjectType type, List<BeloteCard> cards, Player owner)
    {
        Type = type;
        Cards = new List<BeloteCard>(cards);
        Owner = owner;
    }

    //----------------------------------------------
    // GetPoints
    //----------------------------------------------
    // Returns the point value of this project
    public int GetPoints()
    {
        return Type switch
        {
            ProjectType.Sara => 20,
            ProjectType.Khamsin => 50,
            ProjectType.Mia => 100,
            ProjectType.Arbamiya => 400,
            ProjectType.Belote => 20,
            _ => 0
        };
    }

    //----------------------------------------------
    // CompareProjects
    //----------------------------------------------
    // Compares two projects and returns the winner
    // Returns 1 if project1 wins, -1 if project2 wins, 0 if tied
    public static int CompareProjects(Project project1, Project project2)
    {
        // Null safety checks FIRST (before accessing any properties)
        if (project1 == null && project2 == null) return 0;
        if (project1 == null) return -1;
        if (project2 == null) return 1;

        // Belote is never compared
        if (project1.Type == ProjectType.Belote || project2.Type == ProjectType.Belote)
            return 0;

        // Compare by points first (higher points = better)
        int points1 = project1.GetPoints();
        int points2 = project2.GetPoints();
        
        if (points1 > points2) return 1;
        if (points1 < points2) return -1;

        // Same type - compare highest card (with null safety)
        if (project1.HighestCard == null && project2.HighestCard == null) return 0;
        if (project1.HighestCard == null) return -1;
        if (project2.HighestCard == null) return 1;
        
        int card1Rank = GetCardRankForComparison(project1.HighestCard.Value);
        int card2Rank = GetCardRankForComparison(project2.HighestCard.Value);

        if (card1Rank > card2Rank) return 1;
        if (card1Rank < card2Rank) return -1;

        // Exact tie - all projects cancelled
        return 0;
    }

    //----------------------------------------------
    // GetCardRankForComparison
    //----------------------------------------------
    // Returns a comparable rank value for card comparison
    // Higher value = higher rank
    private static int GetCardRankForComparison(Card32Value value)
    {
        switch (value)
        {
            case Card32Value.Seven: return 1;
            case Card32Value.Eight: return 2;
            case Card32Value.Nine: return 3;
            case Card32Value.Ten: return 4;
            case Card32Value.Jack: return 5;
            case Card32Value.Queen: return 6;
            case Card32Value.King: return 7;
            case Card32Value.Ace: return 8;
            default: return 0;
        }
    }

    //----------------------------------------------
    // ToString
    //----------------------------------------------
    public override string ToString()
    {
        string typeStr = Type switch
        {
            ProjectType.Sara => "Sara (20)",
            ProjectType.Khamsin => "Khamsin (50)",
            ProjectType.Mia => "Mia (100)",
            ProjectType.Arbamiya => "Arba'miya (400)",
            ProjectType.Belote => "Belote (20)",
            _ => "None"
        };

        if (Cards != null && Cards.Count > 0)
        {
            string cardsStr = string.Join(", ", Cards.Select(c => $"{c.Value} of {c.Family}"));
            return $"{typeStr}: {cardsStr}";
        }

        return typeStr;
    }
}

