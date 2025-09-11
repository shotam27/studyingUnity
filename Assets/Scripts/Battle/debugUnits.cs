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

    // Keep references to created enemy monster GameObjects
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
                // nothing extra to create; the monster GameObject itself will be the interactive visual
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

    [ContextMenu("Clear Debug Visuals")]
    public void ClearVisuals()
    {
        for (int i = createdEnemyMonsters.Count - 1; i >= 0; i--)
        {
            var go = createdEnemyMonsters[i];
            if (go != null) Destroy(go);
        }
        createdEnemyMonsters.Clear();
    }
}
