using UnityEngine;
using Sirenix.OdinInspector;

//----------------------------------------------
// RoundInfoTracker
//----------------------------------------------
// Purpose:
//   Displays current round information, dealer, and dealing order
//   in the Unity Inspector for debugging and monitoring purposes.
//
// How it connects to other scripts:
//   - Finds the BeloteGame instance in the scene
//   - Reads GameStage data to display current round and dealer info
//----------------------------------------------
public class RoundInfoTracker : MonoBehaviour
{
    [Title("Round Information")]
    [ShowInInspector, ReadOnly, PropertyOrder(0)]
    [InfoBox("The current round number (starts at 1, increases when round ends or all players pass)")]
    public int CurrentRound
    {
        get
        {
            GameStage stage = GetGameStage();
            if (stage == null) return 0;
            
            return stage.CurrentRound;
        }
    }

    [Title("Dealer Information")]
    [ShowInInspector, ReadOnly, PropertyOrder(1)]
    [LabelText("Current Dealer")]
    public string CurrentDealer
    {
        get
        {
            GameStage stage = GetGameStage();
            if (stage == null || stage.Dealer == null) 
                return "Not assigned yet";
            
            return $"{stage.Dealer.Name} ({stage.Dealer.Position}) - Team {stage.Dealer.Team}";
        }
    }

    [ShowInInspector, ReadOnly, PropertyOrder(2)]
    [LabelText("Next Dealer")]
    public string NextDealer
    {
        get
        {
            GameStage stage = GetGameStage();
            if (stage == null || stage.Dealer == null) 
                return "Not assigned yet";
            
            // Get the right player of current dealer (anti-clockwise)
            var getRightPlayerMethod = typeof(GameStage).GetMethod("GetRightPlayer", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (getRightPlayerMethod != null)
            {
                Player nextDealer = (Player)getRightPlayerMethod.Invoke(stage, new object[] { stage.Dealer });
                if (nextDealer != null)
                {
                    return $"{nextDealer.Name} ({nextDealer.Position}) - Team {nextDealer.Team}";
                }
            }
            
            return "Unknown";
        }
    }

    [Title("Dealing Order (Anti-Clockwise)")]
    [ShowInInspector, ReadOnly, PropertyOrder(3)]
    [LabelText("Card Distribution Pattern")]
    [MultiLineProperty(6)]
    public string PlayersDealtTo
    {
        get
        {
            GameStage stage = GetGameStage();
            if (stage == null || stage.Players == null || stage.Players.Count == 0) 
                return "No players";
            
            if (stage.Dealer == null)
                return "Dealer not assigned yet";
                
            if (stage.RoundFirstPlayer == null)
                return "Round not started yet";
            
            string result = $"Dealer: {stage.Dealer.Name} deals to the player on their RIGHT first\n";
            result += $"Distribution Order (Anti-clockwise):\n";
            
            // Get the dealing order starting from RoundFirstPlayer
            var getRightPlayerMethod = typeof(GameStage).GetMethod("GetRightPlayer", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (getRightPlayerMethod != null)
            {
                Player currentPlayer = stage.RoundFirstPlayer;
                for (int i = 0; i < stage.Players.Count; i++)
                {
                    string marker = (i == 0) ? "▶" : " ";
                    string note = (i == 0) ? " ← First to receive cards" : "";
                    result += $"{marker} {i + 1}. {currentPlayer.Name} ({currentPlayer.Position}) - Team {currentPlayer.Team}{note}\n";
                    currentPlayer = (Player)getRightPlayerMethod.Invoke(stage, new object[] { currentPlayer });
                    
                    if (currentPlayer == stage.RoundFirstPlayer)
                        break;
                }
            }
            
            // Add example pattern
            result += $"\nPattern: {stage.Dealer.Name} → ";
            var getRightPlayer = typeof(GameStage).GetMethod("GetRightPlayer", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (getRightPlayer != null)
            {
                Player p = stage.RoundFirstPlayer;
                for (int i = 0; i < stage.Players.Count; i++)
                {
                    result += $"{p.Name}";
                    if (i < stage.Players.Count - 1) result += " → ";
                    p = (Player)getRightPlayer.Invoke(stage, new object[] { p });
                    if (p == stage.RoundFirstPlayer) break;
                }
            }
            
            return result;
        }
    }

    [Title("Additional Info")]
    [ShowInInspector, ReadOnly, PropertyOrder(4)]
    [LabelText("First Player This Round")]
    public string FirstPlayer
    {
        get
        {
            GameStage stage = GetGameStage();
            if (stage == null || stage.RoundFirstPlayer == null) 
                return "Not assigned yet";
            
            return $"{stage.RoundFirstPlayer.Name} ({stage.RoundFirstPlayer.Position}) - Team {stage.RoundFirstPlayer.Team}";
        }
    }

    [ShowInInspector, ReadOnly, PropertyOrder(5)]
    [LabelText("Current Bidder")]
    public string CurrentBidder
    {
        get
        {
            GameStage stage = GetGameStage();
            if (stage == null || stage.Bidder == null) 
                return "No contract made yet";
            
            return $"{stage.Bidder.Name} ({stage.Bidder.Position}) - Team {stage.Bidder.Team}";
        }
    }

    [ShowInInspector, ReadOnly, PropertyOrder(6)]
    [LabelText("Trump Suit")]
    public string TrumpSuit
    {
        get
        {
            GameStage stage = GetGameStage();
            if (stage == null) 
                return "N/A";
            
            if (stage.Trump == null)
                return "No trump (Sun contract or not set)";
            
            return stage.Trump.ToString();
        }
    }

    [Title("All Players")]
    [ShowInInspector, ReadOnly, PropertyOrder(7)]
    [MultiLineProperty(5)]
    public string AllPlayers
    {
        get
        {
            GameStage stage = GetGameStage();
            if (stage == null || stage.Players == null || stage.Players.Count == 0) 
                return "No players";
            
            string result = "";
            foreach (Player player in stage.Players)
            {
                string isDealer = (stage.Dealer == player) ? "🃏 DEALER" : "";
                string isCurrent = (stage.CurrentPlayer == player) ? "▶ CURRENT" : "";
                result += $"{player.Name} ({player.Position}) - {player.Team} {isDealer} {isCurrent}\n";
            }
            
            return result;
        }
    }

    // Cache the game stage reference
    private BeloteGame m_cachedGame;

    private GameStage GetGameStage()
    {
        // Find BeloteGame in the scene
        if (m_cachedGame == null)
        {
            m_cachedGame = FindObjectOfType<BeloteGame>();
        }

        if (m_cachedGame == null)
            return null;

        // Access the GameStage through the public Stage property
        return m_cachedGame.Stage;
    }

    // Clear cache when component is disabled
    private void OnDisable()
    {
        m_cachedGame = null;
    }

    // Provide a button to manually refresh the display
    [Button("Refresh Information", ButtonSizes.Medium)]
    [PropertyOrder(100)]
    private void RefreshInfo()
    {
        m_cachedGame = null;
        // The getters will automatically refresh when the inspector redraws
    }

    [InfoBox("Add this component to any GameObject in your scene to monitor round and dealer information. " +
             "The information updates automatically as you play.", InfoMessageType.Info)]
    [PropertyOrder(-1)]
    [Button("How to Use", ButtonSizes.Large)]
    private void ShowHelp()
    {
        Debug.Log(@"
========================================
RoundInfoTracker - How to Use
========================================

This component displays real-time information about:
1. Current Round Number - Starts at 1, increases when:
   - A game round ends (all cards played)
   - All players pass in bidding rounds 1 and 2

2. Current Dealer - The player who is dealing this round

3. Next Dealer - The player who will deal next round
   (always the player to the right, anti-clockwise)

4. Dealing Order - Shows which players receive cards and in what order

5. Additional game state information

SETUP:
- This component should already be working if attached to a GameObject
- Make sure there's a BeloteGame object in your scene
- The information will update automatically during play

NOTE: Some information won't be available until the game starts.
========================================
        ");
    }
}

