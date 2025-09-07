#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;

public static class TileRenamer
{
    [MenuItem("Tools/Rename tiles 0_0 -> x0y0 (Selected Parent)")]
    public static void RenameTilesUnderSelectedParent()
    {
        if (Selection.activeTransform == null)
        {
            EditorUtility.DisplayDialog("Rename Tiles", "Please select a parent GameObject in the Hierarchy.", "OK");
            return;
        }
        int count = RenameUnderTransform(Selection.activeTransform);
        EditorUtility.DisplayDialog("Rename Tiles", $"Renamed {count} GameObjects under '{Selection.activeTransform.name}'.", "OK");
    }

    [MenuItem("Tools/Rename tiles 0_0 -> x0y0 (Entire Scene)")]
    public static void RenameTilesInScene()
    {
        int total = 0;
        var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (var root in roots)
        {
            total += RenameUnderTransform(root.transform);
        }
        EditorUtility.DisplayDialog("Rename Tiles", $"Renamed {total} GameObjects in the scene.", "OK");
    }

    private static int RenameUnderTransform(Transform parent)
    {
        int changed = 0;
        var regex = new Regex("^(\\d+)_(\\d+)$");
        foreach (Transform t in parent)
        {
            var m = regex.Match(t.name);
            if (m.Success)
            {
                string newName = $"x{m.Groups[1].Value}y{m.Groups[2].Value}";
                Undo.RecordObject(t.gameObject, "Rename Tile");
                t.gameObject.name = newName;
                changed++;
            }
            changed += RenameUnderTransform(t);
        }
        return changed;
    }
}
#endif
