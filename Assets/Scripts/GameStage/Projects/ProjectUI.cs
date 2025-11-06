using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Pebble;

//-------------------------------------------------------
// ProjectUI
//-------------------------------------------------------
// Purpose:
//   Handles the UI for project declaration.
//   Displays buttons for available projects and handles player input.
//   Uses Unity's OnGUI for immediate mode GUI (can be replaced with UGUI later).
//-------------------------------------------------------
public class ProjectUI : MonoBehaviour
{
    //----------------------------------------------
    // Variables
    private ProjectManager m_projectManager;
    private GameStage m_gameStage;
    private Dictionary<Player, bool> m_playerPanelActive;      // Is panel active for this player?
    private Dictionary<Player, Dictionary<ProjectType, int>> m_playerProjectCounts; // Player -> Type -> Count

    // UI Layout
    private float m_buttonWidth = 150f;
    private float m_buttonHeight = 50f;
    private float m_buttonSpacing = 10f;
    private float m_panelPadding = 20f;

    // Colors
    private Color m_buttonColor = new Color(0.2f, 0.4f, 0.8f, 0.9f);
    private Color m_selectedColor = new Color(0.2f, 0.8f, 0.4f, 0.9f);
    private Color m_finishColor = new Color(0.8f, 0.4f, 0.2f, 0.9f);

    //----------------------------------------------
    // Initialization
    public void Init(GameStage gameStage, ProjectManager projectManager)
    {
        m_gameStage = gameStage;
        m_projectManager = projectManager;
        m_playerPanelActive = new Dictionary<Player, bool>();
        m_playerProjectCounts = new Dictionary<Player, Dictionary<ProjectType, int>>();

        // Subscribe to events
        GameEventDispatcher.Subscribe<ProjectDeclarationStartEvent>(OnDeclarationStart);
        GameEventDispatcher.Subscribe<BeloteCard.Played>(OnCardPlayed);
    }

    //----------------------------------------------
    // Shutdown
    public void Shutdown()
    {
        GameEventDispatcher.UnSubscribe<ProjectDeclarationStartEvent>(OnDeclarationStart);
        GameEventDispatcher.UnSubscribe<BeloteCard.Played>(OnCardPlayed);
    }
    
    // No Update() method needed - panels stay visible until first card is played

    //----------------------------------------------
    // OnDeclarationStart
    //----------------------------------------------
    private void OnDeclarationStart(ProjectDeclarationStartEvent evt)
    {
        Player player = evt.CurrentPlayer;
        
        Debug.Log($"[ProjectUI] === OnDeclarationStart for {player.Name} ({player.Position}) ===");
        
        // Ensure all dictionaries are properly initialized for this player
        if (!m_playerPanelActive.ContainsKey(player))
        {
            m_playerPanelActive[player] = false;
        }
        
        if (!m_playerProjectCounts.ContainsKey(player))
        {
            m_playerProjectCounts[player] = new Dictionary<ProjectType, int>();
        }
        
        // Reset counters to 0 at start of each round
        m_playerProjectCounts[player][ProjectType.Sara] = 0;
        m_playerProjectCounts[player][ProjectType.Khamsin] = 0;
        m_playerProjectCounts[player][ProjectType.Mia] = 0;
        m_playerProjectCounts[player][ProjectType.Arbamiya] = 0;
        
        Debug.Log($"[ProjectUI] Reset counters for {player.Name} - all back to 0");
        
        // AI players - auto-declare
        if (!(player is HumanPlayer))
        {
            Debug.Log($"[ProjectUI] {player.Name} is AI - scheduling auto-declaration");
            StartCoroutine(AutoDeclareForAIDelayed(player, evt.AvailableProjects));
            return;
        }

        // Human player - show panel for new round (stays until first card is played)
        Debug.Log($"[ProjectUI] ✅ Showing panel for HUMAN player {player.Name} ({player.Position})");
        m_playerPanelActive[player] = true;
        
        Debug.Log($"[ProjectUI] Panel state: Active={m_playerPanelActive[player]} - will hide when player plays first card");
    }
    
    //----------------------------------------------
    // OnCardPlayed
    //----------------------------------------------
    // Hide panel when player plays a card
    private void OnCardPlayed(BeloteCard.Played evt)
    {
        if (evt.Card == null || evt.Card.Owner == null)
            return;
            
        Player player = evt.Card.Owner as Player;
        if (player == null)
            return;
        
        // Hide this player's panel when they play a card
        if (m_playerPanelActive.ContainsKey(player) && m_playerPanelActive[player])
        {
            m_playerPanelActive[player] = false;
            Debug.Log($"[ProjectUI] {player.Name}'s panel hidden (played a card)");
        }
    }


    //----------------------------------------------
    // AutoDeclareForAIDelayed
    //----------------------------------------------
    // AI automatically declares all available projects after a short delay
    private System.Collections.IEnumerator AutoDeclareForAIDelayed(Player aiPlayer, List<Project> availableProjects)
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(0.5f, 2f));

        Debug.Log($"[ProjectUI] Auto-declaring projects for AI: {aiPlayer.Name}");

        // Declare all available projects
        foreach (var project in availableProjects)
        {
            m_projectManager.DeclareProject(aiPlayer, project.Type, project.Cards);
            
            // Update counter (though not visible for AI)
            if (m_playerProjectCounts.ContainsKey(aiPlayer))
            {
                m_playerProjectCounts[aiPlayer][project.Type]++;
            }
        }
    }

    //----------------------------------------------
    // OnGUI
    //----------------------------------------------
    // Renders project panels for all active players
    void OnGUI()
    {
        // Draw panels for all active human players
        foreach (var kvp in m_playerPanelActive)
        {
            Player player = kvp.Key;
            bool isActive = kvp.Value;
            
            if (!isActive || !(player is HumanPlayer))
                continue;

            DrawPlayerPanel(player);
        }
    }
    
    //----------------------------------------------
    // DrawPlayerPanel
    //----------------------------------------------
    // Draws the project panel for a specific player
    private void DrawPlayerPanel(Player player)
    {
        // Calculate panel size (4 project types + title, NO finish button)
        int projectButtonCount = 4;
        float panelWidth = m_buttonWidth + (m_panelPadding * 2);
        float panelHeight = (projectButtonCount + 1) * (m_buttonHeight + m_buttonSpacing) + (m_panelPadding * 2) + 20;

        // Position based on player position
        float panelX, panelY;
        GetPanelPosition(player.Position, panelWidth, panelHeight, out panelX, out panelY);

        // Draw panel background
        GUI.Box(new Rect(panelX, panelY, panelWidth, panelHeight), "");

        // Draw title with timer
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.fontSize = 16;
        titleStyle.fontStyle = FontStyle.Bold;

        string title = $"{player.Name} - Projects";
        
        GUI.Label(
            new Rect(panelX + m_panelPadding, panelY + m_panelPadding, m_buttonWidth, 25),
            title,
            titleStyle
        );

        // Draw project buttons
        float currentY = panelY + m_panelPadding + 35;

        DrawProjectButtonForPlayer(player, panelX, ref currentY, ProjectType.Sara, "20");
        DrawProjectButtonForPlayer(player, panelX, ref currentY, ProjectType.Khamsin, "50");
        DrawProjectButtonForPlayer(player, panelX, ref currentY, ProjectType.Mia, "100");
        DrawProjectButtonForPlayer(player, panelX, ref currentY, ProjectType.Arbamiya, "400");
    }
    
    //----------------------------------------------
    // GetPanelPosition
    //----------------------------------------------
    // Returns panel position based on player position
    private void GetPanelPosition(PlayerPosition position, float width, float height, out float x, out float y)
    {
        switch (position)
        {
            case PlayerPosition.South:
                x = (Screen.width - width) / 2;
                y = Screen.height - height - 20;
                break;
            case PlayerPosition.West:
                x = 20;
                y = (Screen.height - height) / 2;
                break;
            case PlayerPosition.North:
                x = (Screen.width - width) / 2;
                y = 20;
                break;
            case PlayerPosition.East:
                x = Screen.width - width - 20;
                y = (Screen.height - height) / 2;
                break;
            default:
                x = (Screen.width - width) / 2;
                y = (Screen.height - height) / 2;
                break;
        }
    }

    //----------------------------------------------
    // DrawProjectButtonForPlayer
    //----------------------------------------------
    // Draws a project button for a specific player
    private void DrawProjectButtonForPlayer(Player player, float panelX, ref float currentY, ProjectType type, string points)
    {
        int count = m_playerProjectCounts[player][type];
        Color originalColor = GUI.backgroundColor;
        
        // Color based on count (green if selected)
        GUI.backgroundColor = count > 0 ? m_selectedColor : m_buttonColor;

        string buttonLabel = $"{points}/{count}";
        
        if (GUI.Button(
            new Rect(panelX + m_panelPadding, currentY, m_buttonWidth, m_buttonHeight),
            buttonLabel))
        {
            OnProjectButtonClicked(player, type);
        }

        GUI.backgroundColor = originalColor;
        currentY += m_buttonHeight + m_buttonSpacing;
    }

    //----------------------------------------------
    // OnProjectButtonClicked
    //----------------------------------------------
    // Increment counter and immediately declare project
    private void OnProjectButtonClicked(Player player, ProjectType type)
    {
        m_playerProjectCounts[player][type]++;
        int count = m_playerProjectCounts[player][type];
        
        Debug.Log($"[ProjectUI] {player.Name} declared {type} (count: {count})");
        
        // Immediately declare this project
        m_projectManager.DeclareProject(player, type, new List<BeloteCard>());
    }

    //----------------------------------------------
    // ShowBeloteNotification
    //----------------------------------------------
    // Shows a notification when Belote is declared
    public static void ShowBeloteNotification(Player player)
    {
        Debug.Log($"[ProjectUI] *** BELOTE! {player.Name} declared Belote! ***");
        // TODO: Add visual/audio notification
    }
}

