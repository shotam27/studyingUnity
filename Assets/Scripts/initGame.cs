using System.Collections;
using UnityEngine;
using SpeciesManagement;

/// <summary>
/// ゲーム初期化クラス
/// 初期モンスターの追加や基本設定を行う
/// </summary>
public class initGame : MonoBehaviour
{
    [Header("初期設定")]
    [SerializeField] private bool autoInitialize = true;
    [SerializeField] private float initDelay = 1.0f;

    [Header("初期モンスター設定")]
    [SerializeField] private string speciesName = "Flame Dragon";
    [SerializeField] private string[] monsterNames = { "Blaze", "ほのおくん" };
    [SerializeField] private int[] monsterLevels = { 5, 3 };

    private void Start()
    {
        if (autoInitialize)
        {
            StartCoroutine(InitializeGameWithDelay());
        }
    }

    /// <summary>
    /// 遅延付きでゲーム初期化を実行
    /// MonsterManagerとMonsterSpeciesManagerの準備を待つ
    /// </summary>
    private IEnumerator InitializeGameWithDelay()
    {
        Debug.Log("=== Game Initialization Started ===");
        
        // 必要なマネージャーの準備を待つ
        yield return new WaitForSeconds(initDelay);
        
        // マネージャーの存在確認
        while (MonsterManager.Instance == null || MonsterSpeciesManager.Instance == null)
        {
            Debug.Log("Waiting for managers to initialize...");
            yield return new WaitForSeconds(0.5f);
        }
        
        // 種族データが読み込まれるまで待機
        yield return new WaitUntil(() => MonsterManager.Instance.AllMonsterTypes.Count > 0);
        
        // 初期モンスターを追加
        AddInitialMonsters();
        
        Debug.Log("=== Game Initialization Completed ===");
    }

    /// <summary>
    /// 初期モンスターをパーティに追加
    /// </summary>
    [ContextMenu("Add Initial Monsters")]
    public void AddInitialMonsters()
    {
        Debug.Log("=== Adding Initial Monsters ===");
        
        if (MonsterManager.Instance == null)
        {
            Debug.LogError("MonsterManager not found!");
            return;
        }

        // Flame Dragon種族を検索
    Species flameDragonType = FindSpeciesByName(speciesName);
        
        if (flameDragonType == null)
        {
            Debug.LogError($"Species '{speciesName}' not found!");
            LogAvailableSpecies();
            return;
        }

    Debug.Log($"Found species: {flameDragonType.SpeciesName}");

        // 2体のFlameDragonを作成
        for (int i = 0; i < Mathf.Min(monsterNames.Length, monsterLevels.Length, 2); i++)
        {
            string name = monsterNames[i];
            int level = monsterLevels[i];
            
            var monster = MonsterManager.Instance.CreateAndAddMonster(flameDragonType, name, level);
            
            if (monster != null)
            {
                Debug.Log($"Successfully created monster: {monster.NickName} (Lv.{monster.Level})");
                Debug.Log($"  Stats: HP:{monster.MaxHP} ATK:{monster.ATK} DEF:{monster.DEF} SPD:{monster.SPD}");
            }
            else
            {
                Debug.LogError($"Failed to create monster: {name}");
            }
        }

        // パーティ状況をログ出力
        LogPartyStatus();
        
        Debug.Log("=== End Adding Initial Monsters ===");
    }

    /// <summary>
    /// 種族名で検索
    /// </summary>
    private Species FindSpeciesByName(string name)
    {
        var allTypes = MonsterManager.Instance.AllMonsterTypes;
        
        foreach (var type in allTypes)
        {
            if (type != null && type.SpeciesName != null && 
                type.SpeciesName.Equals(name, System.StringComparison.OrdinalIgnoreCase))
            {
                return type;
            }
        }
        
        return null;
    }

    /// <summary>
    /// 利用可能な種族をログ出力
    /// </summary>
    private void LogAvailableSpecies()
    {
        var allTypes = MonsterManager.Instance.AllMonsterTypes;
        Debug.Log($"Available species count: {allTypes.Count}");
        
            for (int i = 0; i < allTypes.Count; i++)
        {
            var type = allTypes[i];
            if (type != null)
            {
                Debug.Log($"  [{i}] {type.SpeciesName ?? "Unnamed"}");
            }
            else
            {
                Debug.Log($"  [{i}] NULL");
            }
        }
    }

    /// <summary>
    /// 現在のパーティ状況をログ出力
    /// </summary>
    private void LogPartyStatus()
    {
        var party = MonsterManager.Instance.PlayerMonsters;
        Debug.Log($"=== Party Status ===");
        Debug.Log($"Party Size: {party.Count}/{MonsterManager.Instance.MaxPartySize}");
        
        for (int i = 0; i < party.Count; i++)
        {
            var monster = party[i];
            Debug.Log($"  [{i}] {monster.NickName} (Lv.{monster.Level}) - {monster.MonsterType.SpeciesName}");
            Debug.Log($"      HP:{monster.CurrentHP}/{monster.MaxHP} ATK:{monster.ATK} DEF:{monster.DEF} SPD:{monster.SPD}");
        }
        
        Debug.Log($"=== End Party Status ===");
    }

    /// <summary>
    /// パーティをクリア（デバッグ用）
    /// </summary>
    [ContextMenu("Clear Party")]
    public void ClearParty()
    {
        if (MonsterManager.Instance == null) return;
        
        var partyCount = MonsterManager.Instance.PlayerMonsters.Count;
        
        // パーティクリア機能がある場合は使用、なければ手動削除
        var clearMethod = typeof(MonsterManager).GetMethod("ClearAllMonsters");
        if (clearMethod != null)
        {
            clearMethod.Invoke(MonsterManager.Instance, null);
        }
        else
        {
            // 手動でクリア
            while (MonsterManager.Instance.PlayerMonsters.Count > 0)
            {
                MonsterManager.Instance.RemoveMonster(MonsterManager.Instance.PlayerMonsters[0]);
            }
        }
        
        Debug.Log($"Cleared {partyCount} monsters from party");
    }

    /// <summary>
    /// 手動でゲーム初期化を実行
    /// </summary>
    [ContextMenu("Initialize Game Manually")]
    public void InitializeGameManually()
    {
        StartCoroutine(InitializeGameWithDelay());
    }
}
