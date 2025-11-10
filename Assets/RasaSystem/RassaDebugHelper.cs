using UnityEngine;
using Pebble;

/// <summary>
/// Debug helper to test Rassa system
/// </summary>
public class RassaDebugHelper : MonoBehaviour
{
    [Header("Test References")]
    public RassaPromptUI rassaPromptUI;
    public RassaDeckManager rassaDeckManager;
    public RassaGameIntegration rassaIntegration;

    [Header("Test Button")]
    [Tooltip("Press T in Play mode to manually trigger Rassa prompt")]
    public KeyCode testKey = KeyCode.T;

    private void Update()
    {
        if (Input.GetKeyDown(testKey))
        {
            TestRassaPrompt();
        }
    }

    [ContextMenu("Test Rassa Prompt")]
    public void TestRassaPrompt()
    {
        Debug.Log("=== RASSA DEBUG TEST ===");

        // Check RassaPromptUI
        if (rassaPromptUI == null)
        {
            Debug.LogError("❌ RassaPromptUI is NULL!");
        }
        else
        {
            Debug.Log("✅ RassaPromptUI found");
            
            // Test showing the panel
            if (rassaPromptUI.promptPanel != null)
            {
                Debug.Log("✅ Prompt panel reference exists");
                Debug.Log($"Panel active state: {rassaPromptUI.promptPanel.activeSelf}");
                
                // Try to show it manually
                rassaPromptUI.promptPanel.SetActive(true);
                Debug.Log("🎯 Manually activated panel for testing");
            }
            else
            {
                Debug.LogError("❌ Prompt panel is NULL in RassaPromptUI!");
            }
        }

        // Check RassaDeckManager
        if (rassaDeckManager == null)
        {
            Debug.LogError("❌ RassaDeckManager is NULL!");
        }
        else
        {
            Debug.Log("✅ RassaDeckManager found");
            
            bool hasOrder = rassaDeckManager.HasSavedRassaOrder();
            Debug.Log($"Has saved Rassa order: {hasOrder}");
            
            if (hasOrder)
            {
                rassaDeckManager.DebugLogRassaOrder();
            }
        }

        // Check RassaGameIntegration
        if (rassaIntegration == null)
        {
            Debug.LogError("❌ RassaGameIntegration is NULL!");
        }
        else
        {
            Debug.Log("✅ RassaGameIntegration found");
            Debug.Log($"Rassa system enabled: {rassaIntegration.enableRassaSystem}");
        }

        Debug.Log("=== END DEBUG TEST ===");
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 100));
        GUILayout.Label("Rassa Debug Helper", GUI.skin.box);
        if (GUILayout.Button("Test Show Rassa Prompt (T key)"))
        {
            TestRassaPrompt();
        }
        GUILayout.EndArea();
    }
}


