using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
}
