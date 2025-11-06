using System.Collections.Generic;
using Pebble;

//-------------------------------------------------------
// ProjectEvents
//-------------------------------------------------------
// Purpose:
//   Events related to the Projects (Masharie3) system.
//   Used to communicate between ProjectManager, UI, and GameStage.
//-------------------------------------------------------

//-------------------------------------------------------
// ProjectDeclarationStartEvent
//-------------------------------------------------------
// Sent when the project declaration phase begins
// (before the first trick of a round)
public class ProjectDeclarationStartEvent : PooledEvent
{
    public Player CurrentPlayer { get; set; }
    public List<Project> AvailableProjects { get; set; }

    public override void Reset()
    {
        CurrentPlayer = null;
        AvailableProjects = null;
    }
}

//-------------------------------------------------------
// ProjectDeclaredEvent
//-------------------------------------------------------
// Sent when a player declares a project
public class ProjectDeclaredEvent : PooledEvent
{
    public Player Player { get; set; }
    public Project Project { get; set; }

    public override void Reset()
    {
        Player = null;
        Project = null;
    }
}

//-------------------------------------------------------
// ProjectDeclarationCompleteEvent
//-------------------------------------------------------
// Sent when all players have finished declaring projects
// and the comparison/validation is complete
public class ProjectDeclarationCompleteEvent : PooledEvent
{
    public List<Project> ValidProjects { get; set; }      // Projects that will count
    public List<Project> CancelledProjects { get; set; }  // Projects that were cancelled due to ties

    public override void Reset()
    {
        ValidProjects = null;
        CancelledProjects = null;
    }
}

//-------------------------------------------------------
// BeloteDeclaredEvent
//-------------------------------------------------------
// Sent when a player declares Belote during gameplay
public class BeloteDeclaredEvent : PooledEvent
{
    public Player Player { get; set; }
    public Card32Family TrumpSuit { get; set; }

    public override void Reset()
    {
        Player = null;
        TrumpSuit = Card32Family.Clubs; // Default
    }
}

//-------------------------------------------------------
// ProjectScoredEvent
//-------------------------------------------------------
// Sent at the end of a round when project points are added
public class ProjectScoredEvent : PooledEvent
{
    public PlayerTeam Team { get; set; }
    public List<Project> Projects { get; set; }
    public int TotalPoints { get; set; }

    public override void Reset()
    {
        Team = PlayerTeam.Team1;
        Projects = null;
        TotalPoints = 0;
    }
}

