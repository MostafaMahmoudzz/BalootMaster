using System.Collections.Generic;
using System.Linq;
using Pebble;
using UnityEngine;

//-------------------------------------------------------
// ProjectManager
//-------------------------------------------------------
// Purpose:
//   Manages the complete lifecycle of Projects (Masharie3):
//   - Detection in player hands
//   - Declaration phase before first trick
//   - Comparison between players
//   - Scoring at round end
//   - Belote tracking during gameplay
//-------------------------------------------------------
public class ProjectManager
{
    //----------------------------------------------
    // Variables
    private GameStage m_gameStage;
    private bool m_isSunRound;
    private Card32Family? m_trumpSuit;
    
    // Project declarations
    private Dictionary<Player, List<Project>> m_playerProjects;        // All detected projects per player
    private Dictionary<Player, List<Project>> m_declaredProjects;      // Projects declared by each player
    private List<Project> m_validProjects;                             // Projects that passed comparison
    private List<Project> m_cancelledProjects;                         // Projects cancelled due to ties
    
    // Belote tracking
    private Dictionary<Player, bool> m_beloteDeclared;                 // Has player declared Belote?
    private List<Project> m_beloteProjects;                            // Belote projects for scoring
    
    // Declaration state
    private bool m_declarationPhaseActive;
    private const float PANEL_DISPLAY_TIME = 15f; // Panels visible for 15 seconds at round start

    //----------------------------------------------
    // Properties
    public bool DeclarationPhaseActive 
    { 
        get { return m_declarationPhaseActive; } 
    }

    //----------------------------------------------
    // Constructor
    public ProjectManager(GameStage gameStage)
    {
        m_gameStage = gameStage;
        m_playerProjects = new Dictionary<Player, List<Project>>();
        m_declaredProjects = new Dictionary<Player, List<Project>>();
        m_validProjects = new List<Project>();
        m_cancelledProjects = new List<Project>();
        m_beloteDeclared = new Dictionary<Player, bool>();
        m_beloteProjects = new List<Project>();
        m_declarationPhaseActive = false;
    }

    //----------------------------------------------
    // StartRound
    //----------------------------------------------
    // Called at the start of each round to reset state
    public void StartRound(bool isSunRound, Card32Family? trumpSuit)
    {
        Debug.Log($"[ProjectManager] === STARTING NEW ROUND === Sun: {isSunRound}, Trump: {trumpSuit}");
        
        m_isSunRound = isSunRound;
        m_trumpSuit = trumpSuit;
        
        // Clear all project data from previous round
        m_playerProjects.Clear();
        m_declaredProjects.Clear();
        m_validProjects.Clear();
        m_cancelledProjects.Clear();
        m_beloteDeclared.Clear();
        m_beloteProjects.Clear();
        m_declarationPhaseActive = false;

        // Initialize Belote tracking for all players
        foreach (var player in m_gameStage.Players)
        {
            m_beloteDeclared[player] = false;
        }
        
        Debug.Log("[ProjectManager] Round state reset complete - ready for declaration phase");
    }

    //----------------------------------------------
    // StartDeclarationPhase
    //----------------------------------------------
    // Shows project panels for all players at round start
    // Game continues normally - panels are just UI overlays
    public void StartDeclarationPhase()
    {
        Debug.Log("[ProjectManager] === SHOWING PROJECT PANELS FOR NEW ROUND ===");
        
        m_declarationPhaseActive = true;
        
        // Detect projects for all players (for reference)
        DetectAllPlayerProjects();

        // Send event to show panels for ALL players
        int eventsSent = 0;
        foreach (var player in m_gameStage.Players)
        {
            ProjectDeclarationStartEvent evt = Pools.Claim<ProjectDeclarationStartEvent>();
            evt.CurrentPlayer = player;
            evt.AvailableProjects = m_playerProjects.ContainsKey(player) ? m_playerProjects[player] : new List<Project>();
            GameEventDispatcher.SendEvent(evt);
            eventsSent++;
            Debug.Log($"[ProjectManager] Sent ProjectDeclarationStartEvent to {player.Name} ({player.Position})");
        }
        
        Debug.Log($"[ProjectManager] Sent {eventsSent} declaration events - game continues immediately");
    }

    //----------------------------------------------
    // DetectAllPlayerProjects
    //----------------------------------------------
    // Scans all players' hands and detects available projects
    private void DetectAllPlayerProjects()
    {
        foreach (var player in m_gameStage.Players)
        {
            List<Project> projects = ProjectDetector.DetectAllProjects(player, m_isSunRound);
            m_playerProjects[player] = projects;
            m_declaredProjects[player] = new List<Project>();

            Debug.Log($"[ProjectManager] {player.Name} has {projects.Count} available projects:");
            foreach (var project in projects)
            {
                Debug.Log($"  - {project}");
            }
        }
    }

    //----------------------------------------------
    // Update
    //----------------------------------------------
    // Not needed - game doesn't wait for declarations
    public void Update()
    {
        // Declaration phase doesn't block game anymore
    }

    //----------------------------------------------
    // DeclareProject
    //----------------------------------------------
    // Called when a player declares a project (can be called multiple times for same type)
    public void DeclareProject(Player player, ProjectType projectType, List<BeloteCard> cards)
    {
        if (!m_declarationPhaseActive)
        {
            Debug.LogWarning("[ProjectManager] Cannot declare project - declaration phase not active");
            return;
        }

        if (projectType == ProjectType.None)
        {
            Debug.LogWarning("[ProjectManager] Cannot declare None project");
            return;
        }

        // Create project instance
        Project project = new Project(projectType, cards, player);

        // Add to declared projects (allow duplicates of same type)
        if (!m_declaredProjects.ContainsKey(player))
        {
            m_declaredProjects[player] = new List<Project>();
        }

        m_declaredProjects[player].Add(project);
        Debug.Log($"[ProjectManager] {player.Name} declared {project.Type} (total: {m_declaredProjects[player].Count(p => p.Type == projectType)})");

        // Send event
        ProjectDeclaredEvent evt = Pools.Claim<ProjectDeclaredEvent>();
        evt.Player = player;
        evt.Project = project;
        GameEventDispatcher.SendEvent(evt);
    }

    //----------------------------------------------
    // EndDeclarationPhase
    //----------------------------------------------
    // Called at round end to compare and validate projects
    private void EndDeclarationPhase()
    {
        Debug.Log("[ProjectManager] Round ending - comparing declared projects");
        
        m_declarationPhaseActive = false;

        // Compare and validate projects
        CompareProjects();
    }

    //----------------------------------------------
    // CompareProjects
    //----------------------------------------------
    // Compares all declared projects and determines which are valid
    private void CompareProjects()
    {
        m_validProjects.Clear();
        m_cancelledProjects.Clear();

        // Collect all declared projects (except Belote)
        List<Project> allDeclared = new List<Project>();
        foreach (var kvp in m_declaredProjects)
        {
            var nonBelote = kvp.Value.Where(p => p.Type != ProjectType.Belote);
            allDeclared.AddRange(nonBelote);
        }

        if (allDeclared.Count == 0)
        {
            Debug.Log("[ProjectManager] No projects declared");
            return;
        }

        // Group by type and compare
        var projectsByType = allDeclared.GroupBy(p => p.Type);

        foreach (var typeGroup in projectsByType)
        {
            var projectsOfType = typeGroup.ToList();

            if (projectsOfType.Count == 1)
            {
                // Only one player declared this type - automatically valid
                m_validProjects.Add(projectsOfType[0]);
                Debug.Log($"[ProjectManager] Valid project: {projectsOfType[0].Owner.Name} - {projectsOfType[0]}");
            }
            else
            {
                // Multiple players declared same type - compare
                Project bestProject = projectsOfType[0];
                bool isTied = false;

                for (int i = 1; i < projectsOfType.Count; i++)
                {
                    int comparison = Project.CompareProjects(bestProject, projectsOfType[i]);
                    
                    if (comparison < 0)
                    {
                        // New best
                        bestProject = projectsOfType[i];
                        isTied = false;
                    }
                    else if (comparison == 0)
                    {
                        // Tied - all cancelled
                        isTied = true;
                    }
                }

                if (isTied)
                {
                    // All projects of this type are cancelled
                    m_cancelledProjects.AddRange(projectsOfType);
                    Debug.Log($"[ProjectManager] Cancelled {projectsOfType.Count} projects of type {typeGroup.Key} due to tie");
                }
                else
                {
                    // Best project wins
                    m_validProjects.Add(bestProject);
                    Debug.Log($"[ProjectManager] Valid project: {bestProject.Owner.Name} - {bestProject}");
                }
            }
        }
    }

    //----------------------------------------------
    // OnCardPlayed
    //----------------------------------------------
    // Called when a card is played - checks for Belote declaration
    public void OnCardPlayed(BeloteCard card, Player player)
    {
        // Only check in trump rounds
        if (m_isSunRound || !m_trumpSuit.HasValue)
            return;

        // Skip if already declared
        if (m_beloteDeclared[player])
            return;

        // Check if this triggers Belote
        if (ProjectDetector.ShouldDeclareBeloteNow(card, player, m_trumpSuit.Value))
        {
            DeclareBelote(player);
        }
    }

    //----------------------------------------------
    // DeclareBelote
    //----------------------------------------------
    // Declares Belote for a player
    private void DeclareBelote(Player player)
    {
        Debug.Log($"[ProjectManager] {player.Name} declared Belote!");
        
        m_beloteDeclared[player] = true;

        // Create Belote project
        List<BeloteCard> beloteCards = player.Hand.Cards
            .Where(c => c.Family == m_trumpSuit.Value && 
                       (c.Value == Card32Value.King || c.Value == Card32Value.Queen))
            .ToList();

        Project beloteProject = new Project(ProjectType.Belote, beloteCards, player);
        m_beloteProjects.Add(beloteProject);

        // Send event
        BeloteDeclaredEvent evt = Pools.Claim<BeloteDeclaredEvent>();
        evt.Player = player;
        evt.TrumpSuit = m_trumpSuit.Value;
        GameEventDispatcher.SendEvent(evt);
    }

    //----------------------------------------------
    // ScoreProjects
    //----------------------------------------------
    // Called at round end - compares declared projects and adds points
    public void ScoreProjects(Score score, Player biddingTeamPlayer)
    {
        Debug.Log("[ProjectManager] Round ending - comparing and scoring projects");

        // NOW compare projects (at round end, not at declaration time)
        EndDeclarationPhase();

        // Score valid projects
        Dictionary<PlayerTeam, List<Project>> projectsByTeam = new Dictionary<PlayerTeam, List<Project>>();

        // Initialize
        projectsByTeam[PlayerTeam.Team1] = new List<Project>();
        projectsByTeam[PlayerTeam.Team2] = new List<Project>();

        // Collect valid projects by team
        foreach (var project in m_validProjects)
        {
            projectsByTeam[project.Owner.Team].Add(project);
        }

        // Add Belote projects
        foreach (var beloteProject in m_beloteProjects)
        {
            projectsByTeam[beloteProject.Owner.Team].Add(beloteProject);
        }

        // Add points to score
        foreach (var kvp in projectsByTeam)
        {
            PlayerTeam team = kvp.Key;
            List<Project> projects = kvp.Value;
            int totalPoints = projects.Sum(p => p.GetPoints());

            if (totalPoints > 0)
            {
                score.AddScore(team, totalPoints);
                Debug.Log($"[ProjectManager] {team} scored {totalPoints} points from {projects.Count} projects");

                // Send event
                ProjectScoredEvent evt = Pools.Claim<ProjectScoredEvent>();
                evt.Team = team;
                evt.Projects = projects;
                evt.TotalPoints = totalPoints;
                GameEventDispatcher.SendEvent(evt);
            }
        }
    }

    //----------------------------------------------
    // GetPlayerProjects
    //----------------------------------------------
    // Returns the available projects for a player
    public List<Project> GetPlayerProjects(Player player)
    {
        if (m_playerProjects.ContainsKey(player))
            return m_playerProjects[player];
        return new List<Project>();
    }

    //----------------------------------------------
    // GetDeclaredProjects
    //----------------------------------------------
    // Returns the declared projects for a player
    public List<Project> GetDeclaredProjects(Player player)
    {
        if (m_declaredProjects.ContainsKey(player))
            return m_declaredProjects[player];
        return new List<Project>();
    }
}

