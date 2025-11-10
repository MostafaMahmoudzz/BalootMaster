using Pebble;

//-------------------------------------------------------
// Assaa Events
//-------------------------------------------------------
// Purpose:
//   Events related to the Assaa system for communication
//   between game logic and UI systems.
//   Assaa is activated AFTER Rassa is chosen (YES).
//-------------------------------------------------------

/// <summary>
/// Event sent when asking a player about using Assaa
/// </summary>
public class AssaaPromptEvent : PooledEvent
{
    public Player AskingPlayer { get; set; }  // The player being asked
    public Player RassaChooser { get; set; }  // The player who chose Rassa
    public int PromptNumber { get; set; }     // 1 = right player, 2 = teammate

    public override void Reset()
    {
        AskingPlayer = null;
        RassaChooser = null;
        PromptNumber = 0;
    }
}

/// <summary>
/// Event sent when a player responds to Assaa prompt
/// </summary>
public class AssaaResponseEvent : PooledEvent
{
    public bool UseAssaa { get; set; }        // True if player wants to use Assaa
    public Player RespondingPlayer { get; set; }
    public int PromptNumber { get; set; }     // Which prompt this was (1 or 2)

    public override void Reset()
    {
        UseAssaa = false;
        RespondingPlayer = null;
        PromptNumber = 0;
    }
}

/// <summary>
/// Event sent when Assaa card reordering should be shown
/// </summary>
public class AssaaReorderPromptEvent : PooledEvent
{
    public Player ReorderingPlayer { get; set; }  // The player who will reorder cards
    public BeloteDeck Deck { get; set; }          // The deck to reorder

    public override void Reset()
    {
        ReorderingPlayer = null;
        Deck = null;
    }
}

/// <summary>
/// Event sent when card reordering is complete
/// </summary>
public class AssaaReorderCompleteEvent : PooledEvent
{
    public bool Success { get; set; }
    public Player ReorderingPlayer { get; set; }

    public override void Reset()
    {
        Success = false;
        ReorderingPlayer = null;
    }
}

/// <summary>
/// Event sent when the entire Assaa process is complete
/// </summary>
public class AssaaProcessCompleteEvent : PooledEvent
{
    public bool AssaaWasUsed { get; set; }
    
    public override void Reset()
    {
        AssaaWasUsed = false;
    }
}

