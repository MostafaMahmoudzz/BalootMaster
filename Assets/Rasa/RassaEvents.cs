using Pebble;

//-------------------------------------------------------
// Rassa Events
//-------------------------------------------------------
// Purpose:
//   Events related to the Rassa system for communication
//   between game logic and UI systems.
//-------------------------------------------------------

/// <summary>
/// Event sent when the game needs to ask the player about using Rassa
/// </summary>
public class RassaPromptEvent : PooledEvent
{
    public Player AskingPlayer { get; set; }  // The player being asked (current bidder)
    public int RoundNumber { get; set; }      // Current round number

    public override void Reset()
    {
        AskingPlayer = null;
        RoundNumber = 0;
    }
}

/// <summary>
/// Event sent when the player responds to the Rassa prompt
/// </summary>
public class RassaResponseEvent : PooledEvent
{
    public bool UseRassa { get; set; }        // True if player wants to use Rassa
    public Player RespondingPlayer { get; set; }

    public override void Reset()
    {
        UseRassa = false;
        RespondingPlayer = null;
    }
}

/// <summary>
/// Event sent when the deck has been arranged with Rassa order
/// </summary>
public class RassaDeckArrangedEvent : PooledEvent
{
    public bool Success { get; set; }
    
    public override void Reset()
    {
        Success = false;
    }
}

