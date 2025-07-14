using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

public static class TileCreator
{
    [MenuItem("Assets/Create/2D/Tile")]
    public static void CreateTile()
    {
        Tile tile = ScriptableObject.CreateInstance<Tile>();
        string path = AssetDatabase.GenerateUniqueAssetPath("Assets/New Tile.asset");
        AssetDatabase.CreateAsset(tile, path);
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = tile;
    }
}
