#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace Pebble
{
    public class MenuCreateBase
    {
        public static void Create<T>(string newAssetName) where T : ScriptableObject
        {
            T asset = ScriptableObject.CreateInstance<T>();

            string path = SelectionHelpers.GetSelectionAssetPath();

            AssetDatabase.CreateAsset(asset, path + "/" + newAssetName + ".asset");
            AssetDatabase.SaveAssets();

            Selection.activeObject = asset;

            EditorUtility.FocusProjectWindow();
        }
    }

    // Use like this

    /* class MenuCreateMyClass : MenuCreateBase
    {
        [MenuItem("Assets/Create/The/Path/You/Want")]
        public static void Create()
        {
            Create<MyClass>("MyClass or name you want");
        }
    } */
}

#endif