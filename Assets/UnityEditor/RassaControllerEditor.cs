using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(RassaController))]
public class RassaControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RassaController controller = (RassaController)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Setup Helpers", EditorStyles.boldLabel);

        if (GUILayout.Button("Initialize All Card Info Components"))
        {
            controller.InitializeAllCards();
            EditorUtility.DisplayDialog("Rassa Setup", "Card info components initialized!", "OK");
        }

        EditorGUILayout.HelpBox(
            "This will add CardInfoComponent to all buttons and set their card values automatically (32 cards total).",
            MessageType.Info
        );

        EditorGUILayout.Space();

        if (GUILayout.Button("Load Card Order from PlayerPrefs"))
        {
            controller.LoadFromPlayerPrefs();
        }
    }
}
#endif

