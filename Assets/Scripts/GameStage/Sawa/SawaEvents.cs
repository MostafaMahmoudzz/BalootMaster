using Pebble;

//-------------------------------------------------------
// SawaEvents
//-------------------------------------------------------
// Purpose:
//   Event classes for the Sawa system. These events are
//   dispatched when Sawa becomes available or is claimed.
//
// How it connects to other scripts:
//   - Dispatched by `GameStage` and listened to by UI components
//   - Used to show/hide Sawa button and handle claiming
//-------------------------------------------------------

/// <summary>
/// Event sent when Sawa becomes available or unavailable for a player
/// </summary>
public class SawaAvailableEvent : PooledEvent
{
    public Player Player { get; set; }
    public bool IsAvailable { get; set; }

    public override void Reset()
    {
        Player = null;
        IsAvailable = false;
    }
}

/// <summary>
/// Event sent when a player claims Sawa
/// </summary>
public class SawaClaimedEvent : PooledEvent
{
    public Player Player { get; set; }

    public override void Reset()
    {
        Player = null;
    }
}

