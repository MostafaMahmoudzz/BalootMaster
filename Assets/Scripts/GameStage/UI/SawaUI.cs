using UnityEngine;
using UnityEngine.UI;
using Pebble;

//-------------------------------------------------------
// SawaUI
//-------------------------------------------------------
// Purpose:
//   Displays the "Sawa" button when the current player can
//   claim Sawa (guaranteed to win all remaining tricks).
//   Handles button clicks and dispatches Sawa claim events.
//
// How it connects to other scripts:
//   - Listens to `SawaAvailableEvent` from `GameStage`
//   - Dispatches `SawaClaimedEvent` when button is clicked
//   - Manages UI visibility and positioning
//
// Setup:
//   1. Create a Button in your Unity Canvas
//   2. Assign it to the SawaButton field in the Inspector
//   3. The script will handle show/hide automatically
//-------------------------------------------------------
public class SawaUI : MonoBehaviour
{
    [Header("Manual Setup - Assign Your Button Here")]
    [Tooltip("Drag your custom Sawa button from the Canvas here")]
    public Button SawaButton;

    private Player m_currentPlayer;
    private bool m_isAvailable = false;

    //-------------------------------------------------------
    void Awake()
    {
        Debug.Log("[SawaUI] Awake called - initializing");
        
        // Subscribe to Sawa events IMMEDIATELY in Awake
        GameEventDispatcher.Subscribe<SawaAvailableEvent>(OnSawaAvailable);
        GameEventDispatcher.Subscribe<GameStage.NewRoundEvent>(OnNewRound);
        
        // Try to find button if not assigned
        if (SawaButton == null)
        {
            Debug.Log("[SawaUI] Button not assigned, trying to find 'SawaButton' in scene...");
            GameObject buttonObj = GameObject.Find("SawaButton");
            if (buttonObj != null)
            {
                SawaButton = buttonObj.GetComponent<Button>();
                if (SawaButton != null)
                {
                    Debug.Log("[SawaUI] Found SawaButton in scene!");
                }
            }
        }
        
        // Setup the button if assigned or found
        if (SawaButton != null)
        {
            // Add click listener
            SawaButton.onClick.AddListener(OnButtonClicked);
            Debug.Log("[SawaUI] Button assigned and listener added");
            
            // IMPORTANT: Disable and hide the button at start
            SawaButton.interactable = false;
            SawaButton.gameObject.SetActive(false);
            Debug.Log("[SawaUI] Button disabled and hidden at start");
        }
        else
        {
            Debug.LogWarning("[SawaUI] No button assigned or found! Please create a button named 'SawaButton' in your Canvas.");
        }
        
        Debug.Log("[SawaUI] Awake complete - ready to receive events");
    }

    //-------------------------------------------------------
    void Start()
    {
        Debug.Log("[SawaUI] Start called");
    }

    //-------------------------------------------------------
    void OnDestroy()
    {
        // Unsubscribe from events
        GameEventDispatcher.UnSubscribe<SawaAvailableEvent>(OnSawaAvailable);
        GameEventDispatcher.UnSubscribe<GameStage.NewRoundEvent>(OnNewRound);

        // Remove button listener
        if (SawaButton != null)
        {
            SawaButton.onClick.RemoveListener(OnButtonClicked);
        }
    }

    //-------------------------------------------------------
    // Show the Sawa button
    void ShowButton()
    {
        if (SawaButton != null)
        {
            SawaButton.gameObject.SetActive(true);
            SawaButton.interactable = true;
            Debug.Log("[SawaUI] Sawa button shown and enabled");
        }
        else
        {
            Debug.LogWarning("[SawaUI] Cannot show button - no button assigned!");
        }
    }

    //-------------------------------------------------------
    // Hide the Sawa button
    void HideButton()
    {
        if (SawaButton != null)
        {
            SawaButton.interactable = false;
            SawaButton.gameObject.SetActive(false);
            Debug.Log("[SawaUI] Sawa button hidden and disabled");
        }
    }

    //-------------------------------------------------------
    // Handle Sawa availability event
    void OnSawaAvailable(SawaAvailableEvent evt)
    {
        m_currentPlayer = evt.Player;
        m_isAvailable = evt.IsAvailable;

        Debug.Log($"[SawaUI] OnSawaAvailable called - Player: {evt.Player?.Name}, Available: {evt.IsAvailable}");

        if (m_isAvailable)
        {
            ShowButton();
        }
        else
        {
            HideButton();
        }
    }

    //-------------------------------------------------------
    // Handle button click
    void OnButtonClicked()
    {
        if (!m_isAvailable || m_currentPlayer == null)
        {
            Debug.LogWarning("[SawaUI] Button clicked but Sawa is not available!");
            return;
        }

        Debug.Log($"[SawaUI] {m_currentPlayer.Name} clicked Sawa button!");

        // Dispatch Sawa claimed event
        SawaClaimedEvent evt = Pools.Claim<SawaClaimedEvent>();
        evt.Player = m_currentPlayer;
        GameEventDispatcher.SendEvent(evt);

        // Hide button immediately
        HideButton();
        m_isAvailable = false;
    }

    //-------------------------------------------------------
    // Handle new round event - hide button
    void OnNewRound(GameStage.NewRoundEvent evt)
    {
        if (!evt.Start)
        {
            // Round ended, hide button
            HideButton();
            m_isAvailable = false;
        }
    }

    //-------------------------------------------------------
    // Optional: Draw button using Unity's OnGUI as a fallback
    // This shows if no button is assigned in Inspector
    void OnGUI()
    {
        // Only draw OnGUI button if no button is assigned
        if (SawaButton == null && m_isAvailable && m_currentPlayer != null)
        {
            // Calculate button position (centered at bottom)
            float buttonWidth = 200f;
            float buttonHeight = 50f;
            float x = (Screen.width - buttonWidth) / 2f;
            float y = Screen.height - 200f;

            // Draw a styled button
            GUI.backgroundColor = new Color(0.2f, 0.7f, 0.2f);
            GUI.contentColor = Color.white;
            
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 20;
            buttonStyle.fontStyle = FontStyle.Bold;

            if (GUI.Button(new Rect(x, y, buttonWidth, buttonHeight), "صوا (Sawa) - No Button Assigned", buttonStyle))
            {
                Debug.Log("[SawaUI] OnGUI fallback button clicked!");
                OnButtonClicked();
            }
        }
    }
}

