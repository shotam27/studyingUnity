using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Debug helper: creates sample player/enemy monsters and places simple visual markers on tiles.
/// Usage: attach to a GameObject in the Battle scene and call CreateDebugMonsters() from inspector or Start.
/// </summary>
public class debugUnits : MonoBehaviour
{
    [Header("Species / creation")]
    public string playerSpeciesName = "ソードウサギ"; // species name as used by Species.SpeciesName
    public string[] playerNickNames = new string[] { "うさ１", "うさ２" };
    public int playerLevel = 1;

    public string enemySpeciesName = "いもむしくん";
    public int enemyCount = 2;
    public int enemyLevel = 1;

    [Header("Placement")]
    public int playerColumn = 1; // x coordinate for player units (grid X)
    public int enemyColumn = 6;  // x coordinate for enemy units (grid X)
    public int startRowForPlayers = 1; // starting y coordinate
    public int startRowForEnemies = 1;

    [Header("Visuals")]
    public GameObject unitVisualPrefab; // optional prefab to instantiate as a marker; if null a simple cube+text will be created
    public Transform unitsParent;

    [Header("Auto-create")]
    public bool createOnStart = true; // if true, CreateDebugUnits() will be invoked when the scene starts
    [Tooltip("Seconds to wait for MonsterManager.Instance. Set to 0 or negative to wait indefinitely.")]
    public float startWaitTimeout = 30f;

    // Keep references to created visuals so they can be cleared
    private List<GameObject> createdVisuals = new List<GameObject>();
    private List<GameObject> createdEnemyMonsters = new List<GameObject>();

    [ContextMenu("Create Debug Monsters")]
    public void CreateDebugUnits()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("debugUnits: CreateDebugUnits should be run in Play mode.");
            return;
        }

        if (MonsterManager.Instance == null)
        {
            Debug.LogError("MonsterManager.Instance not found.");
            return;
        }

        ClearVisuals();
        createdEnemyMonsters.Clear();

        // Ensure unitsParent exists (parent for generated monster visuals)
        if (unitsParent == null)
        {
            var go = new GameObject("DebugMonstersRoot");
            go.transform.SetParent(this.transform, false);
            unitsParent = go.transform;
        }

        // Create player monsters and place them at playerColumn, startRowForPlayers + i
        for (int i = 0; i < playerNickNames.Length; i++)
        {
            string nick = playerNickNames[i];
            int row = startRowForPlayers + i;
            string tileName = $"x{playerColumn}y{row}";
            var tileGO = GameObject.Find(tileName);
            Vector3 pos = tileGO != null ? tileGO.transform.position : new Vector3(playerColumn, 0f, row);

            // Create the Monster GameObject (resolves species by name internally)
            var createdGO = MonsterManager.Instance.CreateMonsterGameObject(nick, playerSpeciesName, playerLevel, pos, unitsParent);
            if (createdGO == null)
            {
                Debug.LogError($"debugUnits: failed to create player monster '{nick}' species '{playerSpeciesName}'");
            }
            else
            {
                // create a simple marker for quick debug visibility (also tracked in createdVisuals)
                PlaceVisualAtGrid(nick, playerColumn, row, true);
            }
        }

        // Create enemy monsters (store created GameObjects) and place at enemyColumn, startRowForEnemies + i
        for (int i = 0; i < enemyCount; i++)
        {
            string nick = enemySpeciesName + "_e" + (i + 1);
            int row = startRowForEnemies + i;
            string tileName = $"x{enemyColumn}y{row}";
            var tileGO = GameObject.Find(tileName);
            Vector3 pos = tileGO != null ? tileGO.transform.position : new Vector3(enemyColumn, 0f, row);

            var createdGO = MonsterManager.Instance.CreateMonsterGameObject(nick, enemySpeciesName, enemyLevel, pos, unitsParent);
            if (createdGO == null)
            {
                Debug.LogError($"debugUnits: failed to create enemy monster '{nick}' species '{enemySpeciesName}'");
            }
            else
            {
                createdEnemyMonsters.Add(createdGO);
            }

            // still create a simple marker for quick debug visibility
            PlaceVisualAtGrid(nick, enemyColumn, row, false);
        }

        Debug.Log($"debugUnits: Created {playerNickNames.Length} player monsters and {createdEnemyMonsters.Count} enemies.");
    }

    // Start coroutine waits for MonsterManager to be available, then creates debug monsters if requested.
    private IEnumerator Start()
    {
        if (!createOnStart) yield break;

        // wait until MonsterManager.Instance exists (respect configured timeout)
        float elapsed = 0f;
        if (startWaitTimeout <= 0f)
        {
            // wait indefinitely
            while (MonsterManager.Instance == null)
            {
                yield return null;
            }
        }
        else
        {
            while (MonsterManager.Instance == null && elapsed < startWaitTimeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (MonsterManager.Instance == null)
            {
                Debug.LogWarning($"debugUnits: MonsterManager not ready after {startWaitTimeout} seconds; CreateDebugUnits skipped.");
                yield break;
            }
        }

        CreateDebugUnits();
    }

    void PlaceVisualAtGrid(string label, int gridX, int gridY, bool isPlayer)
    {
        // Find tile GameObject named x{gridX}y{gridY}
        string tileName = $"x{gridX}y{gridY}";
        var tileGO = GameObject.Find(tileName);
        Vector3 pos;
        if (tileGO == null)
        {
            Debug.LogWarning($"Tile '{tileName}' not found. Placing marker at fallback position for {label}.");
            // fallback to estimated grid position (assuming 1 unit per cell)
            pos = new Vector3(gridX, 0f, gridY);
        }
        else
        {
            pos = tileGO.transform.position;
        }
        GameObject vis = null;
        if (unitVisualPrefab != null)
        {
            vis = Instantiate(unitVisualPrefab, unitsParent);
            vis.transform.position = pos + new Vector3(0f, 0f, -0.1f);
        }
        else
        {
            // Create a simple marker
            vis = GameObject.CreatePrimitive(PrimitiveType.Cube);
            vis.name = "MonsterVis_" + label;
            vis.transform.SetParent(unitsParent, false);
            vis.transform.position = pos + new Vector3(0f, 0f, -0.1f);
            vis.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
            var rend = vis.GetComponent<Renderer>();
            rend.material = new Material(Shader.Find("Standard"));
            rend.material.color = isPlayer ? Color.cyan : Color.magenta;

            // Add a text label using 3D TextMesh if available
            var textGO = new GameObject("Label");
            textGO.transform.SetParent(vis.transform, false);
            textGO.transform.localPosition = new Vector3(0f, 0.6f, 0f);
            var tm = textGO.AddComponent<TextMesh>();
            tm.text = label;
            tm.characterSize = 0.15f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = Color.black;
        }

        createdVisuals.Add(vis);
    }

    [ContextMenu("Clear Debug Visuals")]
    public void ClearVisuals()
    {
        for (int i = createdVisuals.Count-1; i >= 0; i--)
        {
            var go = createdVisuals[i];
            if (go != null) Destroy(go);
        }
        createdVisuals.Clear();
    }
}
