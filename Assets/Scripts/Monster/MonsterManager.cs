using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif
using SpeciesManagement;

public class MonsterManager : MonoBehaviour
{
    [Header("MonsterType管理")]
    [SerializeField] private List<Species> allMonsterTypes = new List<Species>();
    
    [Header("Monster個体管理")]
    [SerializeField] private List<Monster> playerMonsters = new List<Monster>();
    [SerializeField] private int maxPartySize = 6;

    // シングルトンパターン
    public static MonsterManager Instance { get; private set; }

    // プロパティ - MonsterSpeciesManagerと連携
    public List<Species> AllMonsterTypes 
    { 
        get 
        {
            // MonsterSpeciesManagerが存在する場合はそちらを優先
            if (MonsterSpeciesManager.Instance != null)
            {
                return MonsterSpeciesManager.Instance.AllSpecies;
            }
            return new List<Species>(allMonsterTypes);
        }
    }
    public List<Monster> PlayerMonsters => new List<Monster>(playerMonsters);
    public int MaxPartySize => maxPartySize;

    private void Awake()
    {
        // シングルトン設定
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // MonsterSpeciesManagerが存在しない場合は作成
            if (SpeciesManagement.MonsterSpeciesManager.Instance == null)
            {
                Debug.Log("Creating MonsterSpeciesManager instance");
                var speciesManagerGO = new GameObject("MonsterSpeciesManager");
                speciesManagerGO.AddComponent<SpeciesManagement.MonsterSpeciesManager>();
                DontDestroyOnLoad(speciesManagerGO);
            }
            
            LoadMonsterTypesFromResources();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #region Species管理

    // ResourcesフォルダからすべてのSpeciesを読み込み
    private void LoadMonsterTypesFromResources()
    {
        Debug.Log("=== Species Loading Debug ===");
        Debug.Log($"Resources folder path: Assets/Resources/MonsterTypes");
        
        Species[] loadedTypes = Resources.LoadAll<Species>("MonsterTypes");
        Debug.Log($"Found {loadedTypes.Length} Species assets in Resources");
        
        allMonsterTypes.Clear();
        allMonsterTypes.AddRange(loadedTypes);
        
        foreach (var type in loadedTypes)
        {
            Debug.Log($"Loaded Species: {type.name} - {(type.SpeciesName ?? "NULL NAME")} ");
        }
        
        Debug.Log($"Loaded {allMonsterTypes.Count} Species from Resources");
    Debug.Log("=== End Species Loading Debug ===");
    }

    // MonsterTypeを名前で検索
    public Species GetMonsterTypeByName(string typeName)
    {
        return allMonsterTypes.FirstOrDefault(type => type.SpeciesName == typeName);
    }

    // MonsterTypeをIDで検索（配列インデックス）
    public Species GetMonsterTypeByID(int id)
    {
        if (id >= 0 && id < allMonsterTypes.Count)
            return allMonsterTypes[id];
        return null;
    }

    // ランダムなMonsterTypeを取得
    public Species GetRandomMonsterType()
    {
        if (allMonsterTypes.Count == 0) return null;
        int randomIndex = Random.Range(0, allMonsterTypes.Count);
        return allMonsterTypes[randomIndex];
    }

    #endregion

    #region Monster個体管理

    // 新しいモンスターを作成してパーティに追加
    public Monster CreateAndAddMonster(Species monsterType, string nickName = "", int level = 1)
    {
        Debug.Log($"=== CreateAndAddMonster Debug ===");
        Debug.Log($"Species: {(monsterType?.name ?? "NULL")} ");
        Debug.Log($"NickName: {nickName}");
        Debug.Log($"Level: {level}");
        
        if (monsterType == null) 
        {
            Debug.LogError("Species is NULL! Cannot create monster.");
            return null;
        }
        
        Monster newMonster = new Monster(monsterType, nickName, level);
        Debug.Log($"Created monster: {newMonster.NickName}");
        
        bool added = AddMonster(newMonster);
        Debug.Log($"Monster added to party: {added}");
    Debug.Log($"=== End CreateAndAddMonster Debug ===");
        
        return added ? newMonster : null;
    }

    // モンスターをパーティに追加
    public bool AddMonster(Monster monster)
    {
        if (monster == null || playerMonsters.Count >= maxPartySize) 
            return false;
        
        playerMonsters.Add(monster);
        Debug.Log($"Added {monster.NickName} to party. Party size: {playerMonsters.Count}");
        return true;
    }

    // モンスターをパーティから削除
    public bool RemoveMonster(Monster monster)
    {
        bool removed = playerMonsters.Remove(monster);
        if (removed)
        {
            Debug.Log($"Removed {monster.NickName} from party. Party size: {playerMonsters.Count}");
        }
        return removed;
    }

    // モンスターを名前で検索
    public Monster GetMonsterByName(string nickName)
    {
        return playerMonsters.FirstOrDefault(monster => monster.NickName == nickName);
    }

    // 生きているモンスターのみ取得
    public List<Monster> GetAliveMonsters()
    {
        return playerMonsters.Where(monster => !monster.IsDead).ToList();
    }

    // 死んでいるモンスターのみ取得
    public List<Monster> GetDeadMonsters()
    {
        return playerMonsters.Where(monster => monster.IsDead).ToList();
    }

    // パーティの空きスロット数
    public int GetAvailableSlots()
    {
        return maxPartySize - playerMonsters.Count;
    }

    // パーティが満員かチェック
    public bool IsPartyFull()
    {
        return playerMonsters.Count >= maxPartySize;
    }

    #endregion

    #region ユーティリティ

    // ランダムなモンスターを生成
    public Monster GenerateRandomMonster(int minLevel = 1, int maxLevel = 10)
    {
        Debug.Log($"=== GenerateRandomMonster Debug ===");
        Debug.Log($"Available Species: {allMonsterTypes.Count}");
        
        Species randomType = GetRandomMonsterType();
        if (randomType == null) 
        {
            Debug.LogError("No Species available! Cannot generate random monster.");
            return null;
        }
        
        Debug.Log($"Selected Species: {randomType.name}");
        
        int randomLevel = Random.Range(minLevel, maxLevel + 1);
        string randomName = GenerateRandomName(randomType.SpeciesName);
        
        Debug.Log($"Generated: {randomName} (Lv.{randomLevel})");
        Debug.Log($"=== End GenerateRandomMonster Debug ===");
        
        return new Monster(randomType, randomName, randomLevel);
    }

    // ランダムな名前生成（簡易版）
    private string GenerateRandomName(string baseTypeName)
    {
        string[] prefixes = { "小さな", "大きな", "強い", "速い", "賢い", "古い", "若い", "美しい" };
        string[] suffixes = { "君", "ちゃん", "さん", "様", "王", "姫", "長老", "戦士" };
        
        if (Random.Range(0, 2) == 0)
        {
            return prefixes[Random.Range(0, prefixes.Length)] + baseTypeName;
        }
        else
        {
            return baseTypeName + suffixes[Random.Range(0, suffixes.Length)];
        }
    }

    // すべてのモンスターを全回復
    public void HealAllMonsters()
    {
        foreach (Monster monster in playerMonsters)
        {
            monster.FullHeal();
        }
        Debug.Log("All monsters have been fully healed!");
    }

    // パーティの平均レベル取得
    public float GetAverageLevel()
    {
        if (playerMonsters.Count == 0) return 0;
        return (float)playerMonsters.Average(monster => monster.Level);
    }

    // デバッグ用：パーティ情報をログ出力
    [ContextMenu("Debug: Print Party Info")]
    public void DebugPrintPartyInfo()
    {
        Debug.Log($"=== Party Info ===");
        Debug.Log($"Party Size: {playerMonsters.Count}/{maxPartySize}");
        Debug.Log($"Average Level: {GetAverageLevel():F1}");
        
        for (int i = 0; i < playerMonsters.Count; i++)
        {
            Monster monster = playerMonsters[i];
            Debug.Log($"[{i}] {monster.ToString()}");
        }
    }

    #endregion

    #region Visual creation

    // Create a Monster data object (adds to party) and a corresponding GameObject with a SpriteRenderer.
    // speciesIdentifier: can be Species.SpeciesName or the JSON id from StreamingAssets.
    // position: world position to place the visual. If null, placed at origin.
    public GameObject CreateMonsterGameObject(string nickName, string speciesIdentifier, int level = 1, Vector3? position = null, Transform parent = null)
    {
        // 1) Try to resolve Species ScriptableObject by SpeciesName first
        Species species = GetMonsterTypeByName(speciesIdentifier);

        // 2) If not found, try to parse StreamingAssets monster-species.json to match id->name and then resolve
        if (species == null)
        {
            try
            {
                string path = System.IO.Path.Combine(Application.streamingAssetsPath, "monster-species.json");
                if (System.IO.File.Exists(path))
                {
                    string raw = System.IO.File.ReadAllText(path);
                    // JsonUtility cannot parse a top-level array, so wrap it
                    string wrapped = "{\"items\":" + raw + "}";
                    var wrapper = JsonUtility.FromJson<StreamingSpeciesWrapper>(wrapped);
                    if (wrapper != null && wrapper.items != null)
                    {
                        StreamingSpeciesEntry matched = null;
                        // try match by id first
                        foreach (var it in wrapper.items)
                        {
                            if (!string.IsNullOrEmpty(it.id) && it.id == speciesIdentifier)
                            {
                                matched = it;
                                // found, try resolve by name
                                species = GetMonsterTypeByName(it.name);
                                break;
                            }
                        }

                        // if still not found, try match by name in JSON
                        if (species == null)
                        {
                            foreach (var it in wrapper.items)
                            {
                                if (!string.IsNullOrEmpty(it.name) && it.name == speciesIdentifier)
                                {
                                    matched = it;
                                    species = GetMonsterTypeByName(it.name);
                                    break;
                                }
                            }
                        }

                        // If we have a JSON entry but no Species ScriptableObject, create a runtime Species
                        if (species == null && matched != null)
                        {
                            try
                            {
                                var runtimeSpecies = ScriptableObject.CreateInstance<Species>();
                                var sType = typeof(Species);

                                // set private fields via reflection
                                var fName = sType.GetField("monsterTypeName", BindingFlags.NonPublic | BindingFlags.Instance);
                                if (fName != null) fName.SetValue(runtimeSpecies, matched.name);

                                var fBasic = sType.GetField("basicStatus", BindingFlags.NonPublic | BindingFlags.Instance);
                                if (fBasic != null && matched.basicStatus != null)
                                {
                                    var bs = new BasicStatus(matched.basicStatus.maxHP, matched.basicStatus.atk, matched.basicStatus.def, matched.basicStatus.spd);
                                    fBasic.SetValue(runtimeSpecies, bs);
                                }

                                // try load sprite from Resources/Images/{name}
                                var fSprite = sType.GetField("sprite", BindingFlags.NonPublic | BindingFlags.Instance);
                                if (fSprite != null)
                                {
                                    Sprite sp = null;
                                    try { sp = Resources.Load<Sprite>("Images/" + matched.name); } catch { sp = null; }
                                    fSprite.SetValue(runtimeSpecies, sp);
                                }

                                species = runtimeSpecies;
                                Debug.Log($"CreateMonsterGameObject: created runtime Species for '{matched.name}' from StreamingAssets JSON.");
                            }
                            catch (System.Exception ex)
                            {
                                Debug.LogWarning($"CreateMonsterGameObject: failed to create runtime Species: {ex.Message}");
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"CreateMonsterGameObject: failed reading StreamingAssets JSON: {ex.Message}");
            }
        }

        if (species == null)
        {
            // If running in the Editor, try to auto-generate missing Species assets from StreamingAssets JSON
            #if UNITY_EDITOR
            try
            {
                Debug.Log($"CreateMonsterGameObject: Species '{speciesIdentifier}' not found. Attempting to generate Species assets from StreamingAssets (Editor-only)...");
                // call the editor generator via menu command to avoid assembly dependency
                UnityEditor.EditorApplication.ExecuteMenuItem("Tools/Generate Species From StreamingAssets");
                // reload resources list
                LoadMonsterTypesFromResources();
                // retry resolving by name
                species = GetMonsterTypeByName(speciesIdentifier);
                if (species == null)
                {
                    Debug.LogError($"CreateMonsterGameObject: Species '{speciesIdentifier}' still not found after generating assets.");
                    return null;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"CreateMonsterGameObject: failed to auto-generate Species assets in Editor: {ex.Message}");
                return null;
            }
            #else
            Debug.LogError($"CreateMonsterGameObject: Species '{speciesIdentifier}' not found (Resources/Species or StreamingAssets). Aborting.");
            return null;
            #endif
        }

        // Create Monster data and add to manager
        Monster monster = CreateAndAddMonster(species, nickName, level);
        if (monster == null)
        {
            Debug.LogError("CreateMonsterGameObject: failed to create Monster data (party may be full).");
            return null;
        }

        // Create visual GameObject
        GameObject go = new GameObject($"MonsterGO_{monster.NickName}");
        if (parent != null) go.transform.SetParent(parent, false);
        else
        {
            // ensure a root exists
            var root = GameObject.Find("MonstersRoot");
            if (root == null)
            {
                root = new GameObject("MonstersRoot");
            }
            go.transform.SetParent(root.transform, false);
        }

        Vector3 pos = position ?? Vector3.zero;
        go.transform.position = pos;

        // Add SpriteRenderer if species has a Sprite
        var sr = go.AddComponent<SpriteRenderer>();
        if (species.Sprite != null)
        {
            sr.sprite = species.Sprite;
        }
        else
        {
            // fallback: try Resources/Images/{SpeciesName}
            string candidate = species.SpeciesName;
            var resSprite = Resources.Load<Sprite>("Images/" + candidate);
            if (resSprite != null) sr.sprite = resSprite;
            else
            {
                // fallback: create a simple placeholder sprite so visuals are visible in editor/play
                Debug.LogWarning($"CreateMonsterGameObject: no sprite found for species '{species.SpeciesName}'. Creating placeholder sprite.");
                try
                {
                    int size = 32;
                    Texture2D tex = new Texture2D(size, size, TextureFormat.ARGB32, false);
                    Color fill = new Color(0.8f, 0.2f, 0.8f, 1f); // magenta-ish placeholder
                    Color[] cols = new Color[size * size];
                    for (int i = 0; i < cols.Length; i++) cols[i] = fill;
                    tex.SetPixels(cols);
                    tex.Apply();

                    Sprite placeholder = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
                    sr.sprite = placeholder;
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"CreateMonsterGameObject: failed to create placeholder sprite: {ex.Message}");
                }
            }
        }

        // Optional small component to link data <-> view
        var link = go.AddComponent<MonsterViewLink>();
        link.monster = monster;

        return go;
    }

    // Helper classes for JSON parsing of StreamingAssets monster-species.json
    [System.Serializable]
    private class StreamingSpeciesWrapper { public StreamingSpeciesEntry[] items; }

    [System.Serializable]
    private class StreamingSpeciesEntry
    {
        public string id;
        public string name;
        public string description;
        public StreamingBasicStatus basicStatus;
    }

    [System.Serializable]
    private class StreamingBasicStatus
    {
        public int maxHP;
        public int atk;
        public int def;
        public int spd;
    }

    // Simple component to attach to visuals to keep a reference to the Monster data object
    private class MonsterViewLink : MonoBehaviour
    {
        public Monster monster;
    }

    #endregion
}
