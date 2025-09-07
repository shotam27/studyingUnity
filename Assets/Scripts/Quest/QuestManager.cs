using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;
using System.Text;

/// <summary>
/// クエストデータと簡易マネージャ
/// - Quest (名前, ランク, 出てくるモンスター, 報酬)
/// - シングルトンでアクセス可能
/// </summary>
public class QuestManager : MonoBehaviour
{
    // シングルトン
    public static QuestManager Instance { get; private set; }

    [Header("Registered Quests")]
    [SerializeField] private List<Quest> registeredQuests = new List<Quest>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Load quests from StreamingAssets on start. If not present, create sample quests and save them.
        LoadOrCreateQuestsFromStreamingAssets();
    }

    #region CRUD

    public IReadOnlyList<Quest> AllQuests => registeredQuests.AsReadOnly();

    public bool AddQuest(Quest quest)
    {
        if (quest == null) return false;
        if (registeredQuests.Any(q => q.Name == quest.Name)) return false;
        registeredQuests.Add(quest);
        return true;
    }

    public bool RemoveQuest(Quest quest)
    {
        if (quest == null) return false;
        return registeredQuests.Remove(quest);
    }

    public Quest GetQuestByName(string name)
    {
        return registeredQuests.FirstOrDefault(q => q.Name == name);
    }

    public List<Quest> GetQuestsByRank(int rank)
    {
        return registeredQuests.Where(q => q.Rank == rank).ToList();
    }

    public Quest GetRandomQuest(int minRank = int.MinValue, int maxRank = int.MaxValue)
    {
        var candidates = registeredQuests.Where(q => q.Rank >= minRank && q.Rank <= maxRank).ToList();
        if (candidates.Count == 0) return null;
        return candidates[Random.Range(0, candidates.Count)];
    }

    #endregion

    #region Utilities

    [ContextMenu("Create Sample Quests")]
    private void CreateSampleQuests()
    {
        registeredQuests.Clear();

        // 例: Flame Dragon を出すクエスト（Species がプロジェクトに存在することを想定）
        var flame = MonsterManager.Instance?.GetMonsterTypeByName("Flame Dragon");
        var slime = MonsterManager.Instance?.GetMonsterTypeByName("Slime");

        var q1 = new Quest
        {
            Name = "炎の洞窟討伐",
            Rank = 2,
            Monsters = new List<Species>() { flame },
            Rewards = new List<Reward>() { new Reward { ItemName = "Gold", Quantity = 100 }, new Reward { ItemName = "Flame Scale", Quantity = 1 } }
        };

        var q2 = new Quest
        {
            Name = "スライム退治",
            Rank = 1,
            Monsters = new List<Species>() { slime },
            Rewards = new List<Reward>() { new Reward { ItemName = "Gold", Quantity = 20 }, new Reward { ItemName = "Slime Jelly", Quantity = 2 } }
        };

        if (q1 != null) registeredQuests.Add(q1);
        if (q2 != null) registeredQuests.Add(q2);

        Debug.Log($"Created {registeredQuests.Count} sample quests");

        // Save the created sample quests into StreamingAssets/quests.json so the editor/web tools can edit them.
        try
        {
            SaveQuestsToStreamingAssets();
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"Failed to save sample quests to StreamingAssets: {ex.Message}");
        }
    }

    private void LoadOrCreateQuestsFromStreamingAssets()
    {
        var path = Path.Combine(Application.streamingAssetsPath, "quests.json");
        if (File.Exists(path))
        {
            try
            {
                LoadQuestsFromJsonFile(path);
                Debug.Log($"Loaded {registeredQuests.Count} quests from {path}");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Failed to load quests.json: {ex.Message}. Creating sample quests instead.");
                CreateSampleQuests();
            }
        }
        else
        {
            CreateSampleQuests();
            Debug.Log($"No quests.json found at {path}. Created and saved sample quests.");
        }
    }

    private void LoadQuestsFromJsonFile(string filePath)
    {
        var json = File.ReadAllText(filePath, Encoding.UTF8);
        var wrapper = JsonUtility.FromJson<QuestJsonList>(json);
        registeredQuests.Clear();
        if (wrapper == null || wrapper.Quests == null) return;
        foreach (var qj in wrapper.Quests)
        {
            var q = new Quest { Name = qj.Name, Rank = qj.Rank, Rewards = qj.Rewards ?? new List<Reward>() };
            if (qj.Monsters != null)
            {
                foreach (var mname in qj.Monsters)
                {
                    var sp = MonsterManager.Instance?.GetMonsterTypeByName(mname);
                    if (sp != null) q.Monsters.Add(sp);
                    else Debug.LogWarning($"Species '{mname}' not found while loading quest '{q.Name}'");
                }
            }
            registeredQuests.Add(q);
        }
    }

    private void SaveQuestsToStreamingAssets()
    {
        var path = Path.Combine(Application.streamingAssetsPath, "quests.json");
        var wrapper = new QuestJsonList { Quests = new List<QuestJson>() };
        foreach (var q in registeredQuests)
        {
            var qj = new QuestJson { Name = q.Name, Rank = q.Rank, Rewards = q.Rewards ?? new List<Reward>() };
            qj.Monsters = new List<string>();
            if (q.Monsters != null)
            {
                foreach (var s in q.Monsters)
                {
                    qj.Monsters.Add(s?.SpeciesName ?? "");
                }
            }
            wrapper.Quests.Add(qj);
        }
        var json = JsonUtility.ToJson(wrapper, true);
        // Ensure directory exists
        Directory.CreateDirectory(Application.streamingAssetsPath);
        File.WriteAllText(path, json, Encoding.UTF8);
        Debug.Log($"Saved {registeredQuests.Count} quests to {path}");
    }

    [ContextMenu("Debug: Print All Quests")]
    private void DebugPrintQuests()
    {
        Debug.Log($"Registered Quests: {registeredQuests.Count}");
        for (int i = 0; i < registeredQuests.Count; i++)
        {
            var q = registeredQuests[i];
            string mons = q.Monsters != null ? string.Join(", ", q.Monsters.ConvertAll(m => m?.SpeciesName ?? "null")) : "none";
            string rewards = q.Rewards != null ? string.Join(", ", q.Rewards.ConvertAll(r => $"{r.ItemName} x{r.Quantity}")) : "none";
            Debug.Log($"[{i}] {q.Name} (Rank:{q.Rank}) Monsters: {mons} Rewards: {rewards}");
        }
    }

    #endregion

    
}

[System.Serializable]
public class Quest
{
    public string Name;
    public int Rank = 1; // クエストランク
    public List<Species> Monsters = new List<Species>();
    public List<Reward> Rewards = new List<Reward>();
}

[System.Serializable]
public class Reward
{
    public string ItemName;
    public int Quantity = 1;
}

// JSON-friendly DTOs for (de)serializing quests where Species are represented by name
[System.Serializable]
public class QuestJson
{
    public string Name;
    public int Rank = 1;
    public List<string> Monsters = new List<string>();
    public List<Reward> Rewards = new List<Reward>();
}

[System.Serializable]
public class QuestJsonList
{
    public List<QuestJson> Quests = new List<QuestJson>();
}
