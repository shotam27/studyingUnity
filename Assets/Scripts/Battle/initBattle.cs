using UnityEngine;

public class initBattle : MonoBehaviour
{
    [Header("Tile Grid")]
    public GameObject tilePrefab;
    public int rows = 20;
    public int cols = 20;
    public Vector2 spacing = new Vector2(1f, 1f);
    public bool centerGrid = true;
    [Tooltip("Parent transform for generated tiles. If null, this GameObject will be used.")]
    public Transform parentForTiles;

    [Header("Cleanup")]
    [Tooltip("Prefix used for created tile objects so ClearGeneratedTiles() can remove them safely")]
    public string tileNamePrefix = "Tile_";

    [Header("Runtime")]
    public bool generateOnStart = false;

    private void Start()
    {
        // Only auto-generate at runtime (Play mode)
        if (generateOnStart && Application.isPlaying)
        {
            GenerateGrid();
        }
    }

    [ContextMenu("Generate Tile Grid")]
    public void GenerateGrid()
    {
        // Only generate tiles during Play mode to avoid modifying prefab assets in the Editor
        if (!Application.isPlaying)
        {
            Debug.LogWarning("initBattle: GenerateGrid can only be run during Play mode.");
            return;
        }

        if (tilePrefab == null)
        {
            Debug.LogWarning("initBattle: tilePrefab is not assigned.");
            return;
        }

        Transform parent = parentForTiles != null ? parentForTiles : this.transform;

        // If the chosen parent is not part of a valid scene (e.g. part of a prefab asset), create a scene root under this GameObject
        if (!parent.gameObject.scene.IsValid())
        {
            Debug.LogWarning("initBattle: specified parent is not in a scene (likely a Prefab asset). Creating a scene root under this GameObject for generated tiles.");
            var existing = transform.Find("BattleTilesRoot");
            if (existing != null) parent = existing;
            else
            {
                var root = new GameObject("BattleTilesRoot");
                root.transform.SetParent(this.transform, false);
                parent = root.transform;
            }
        }

        // Optionally clear previous generated tiles
        ClearGeneratedTiles();

        // Compute offset to center grid
        float offsetX = 0f, offsetY = 0f;
        if (centerGrid)
        {
            offsetX = -((cols - 1) * spacing.x) / 2f;
            offsetY = -((rows - 1) * spacing.y) / 2f;
        }

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                Vector3 localPos = new Vector3(offsetX + x * spacing.x, offsetY + y * spacing.y, 0f);
                var go = GameObject.Instantiate(tilePrefab);
                if (go == null) continue;
                go.transform.SetParent(parent, false);
                go.transform.localPosition = localPos;
                // Ensure Tile component exists and set grid coordinates
                var tileComp = go.GetComponent<Tile>() ?? go.AddComponent<Tile>();
                tileComp.SetGridPosition(x, y);

                // Name tile as x{X}y{Y}
                go.name = $"x{tileComp.gridX}y{tileComp.gridY}";
            }
        }

        Debug.Log($"initBattle: Generated {rows * cols} tiles under '{parent.name}'.");
    }

    [ContextMenu("Clear Generated Tiles")]
    public void ClearGeneratedTiles()
    {
        Transform parent = parentForTiles != null ? parentForTiles : this.transform;
        // Collect to avoid modifying collection while iterating: find children that have a Tile component
        var toDestroy = new System.Collections.Generic.List<GameObject>();
        foreach (Transform child in parent)
        {
            if (child.GetComponent<Tile>() != null) toDestroy.Add(child.gameObject);
        }
        for (int i = 0; i < toDestroy.Count; i++)
        {
            #if UNITY_EDITOR
            if (!Application.isPlaying) UnityEditor.Undo.DestroyObjectImmediate(toDestroy[i]);
            else Destroy(toDestroy[i]);
            #else
            Destroy(toDestroy[i]);
            #endif
        }
        if (toDestroy.Count > 0) Debug.Log($"initBattle: Cleared {toDestroy.Count} generated tiles.");
    }

    // Helper that uses PrefabUtility in editor to preserve prefab connection, fallback to Instantiate at runtime
    private GameObject PrefabUtilitySafeInstantiate(GameObject prefab, Transform parent)
    {
        if (prefab == null) return null;
        #if UNITY_EDITOR
        // Try to use PrefabUtility to preserve prefab connection, but ensure the returned object is a scene instance.
        var instObj = UnityEditor.PrefabUtility.InstantiatePrefab(prefab);
        var instGo = instObj as GameObject;
        if (instGo != null && instGo.scene.IsValid())
        {
            // Good: we got a scene instance.
            return instGo;
        }

        // Fallback: create a runtime instance which is guaranteed to be a scene object.
        var runtimeInst = GameObject.Instantiate(prefab);
        // Register with Undo for editor convenience
        if (!Application.isPlaying)
        {
            UnityEditor.Undo.RegisterCreatedObjectUndo(runtimeInst, "Instantiate Tile");
        }
        return runtimeInst;
        #else
        return GameObject.Instantiate(prefab);
        #endif
    }
}
