using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
