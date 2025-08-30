using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MyMonsterUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Transform monsterListParent;
    [SerializeField] private GameObject monsterListItemPrefab;
    [SerializeField] private MonsterUI monsterDetailUI;
    [SerializeField] private Button refreshButton;
    [SerializeField] private Button createRandomButton;
    [SerializeField] private TextMeshProUGUI partyInfoText;

    private List<GameObject> monsterListItems = new List<GameObject>();

    private void Start()
    {
        // ボタンのイベント設定
        if (refreshButton != null)
            refreshButton.onClick.AddListener(RefreshMonsterList);
        
        if (createRandomButton != null)
            createRandomButton.onClick.AddListener(CreateRandomMonster);

        // 初期表示
        RefreshMonsterList();
    }

    private void RefreshMonsterList()
    {
        Debug.Log("=== RefreshMonsterList Debug ===");
        
        // 既存のリストアイテムを削除
        foreach (var item in monsterListItems)
        {
            if (item != null)
                Destroy(item);
        }
        monsterListItems.Clear();
        Debug.Log("Cleared existing monster list items");

        // パーティ情報更新
        UpdatePartyInfo();

        // モンスターリスト表示
        if (MonsterManager.Instance == null)
        {
            Debug.LogError("MonsterManager.Instance is NULL!");
            return;
        }
        
        var monsters = MonsterManager.Instance.PlayerMonsters;
        Debug.Log($"Found {monsters.Count} monsters in party");
        
        for (int i = 0; i < monsters.Count; i++)
        {
            Debug.Log($"Creating list item for monster {i}: {monsters[i].NickName}");
            CreateMonsterListItem(monsters[i]);
        }
        
        Debug.Log($"Created {monsterListItems.Count} monster list items");
        Debug.Log("=== End RefreshMonsterList Debug ===");
    }

    private void CreateMonsterListItem(Monster monster)
    {
        try
        {
            Debug.Log($"--- CreateMonsterListItem for {monster?.NickName ?? "<null>"} ---");

            if (monsterListItemPrefab == null)
            {
                Debug.LogError("monsterListItemPrefab is NULL!");
                return;
            }

            if (monsterListParent == null)
            {
                Debug.LogError("monsterListParent is NULL!");
                return;
            }

            var listItem = Instantiate(monsterListItemPrefab, monsterListParent) as GameObject;
            if (listItem == null)
            {
                Debug.LogError("Instantiate returned null or non-GameObject for monsterListItemPrefab");
                return;
            }

            monsterListItems.Add(listItem);
            Debug.Log($"Instantiated list item: {listItem.name}");

        // Attach a debug background to visualize the RectTransform bounds of the list item
        try
        {
            var debugBg = listItem.GetComponent<DebugBackground>();
            if (debugBg == null)
            {
                debugBg = listItem.AddComponent<DebugBackground>();
                // choose a semi-transparent green so it doesn't obscure the text
                debugBg.color = new Color(0f, 0.6f, 0f, 0.18f);
                debugBg.padding = new Vector2(4f, 4f);
                debugBg.autoCreate = true;
                // try to create/update immediately so the background appears in Edit/Play
                debugBg.CreateOrUpdateBackground();
                Debug.Log("Added DebugBackground to list item for visual bounds");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"Failed to add DebugBackground: {ex.Message}");
        }

        // リストアイテムの設定
        // try to find an existing Button (root first, then children)
        var button = listItem.GetComponent<Button>() ?? listItem.GetComponentInChildren<Button>();
        var text = listItem.GetComponentInChildren<TextMeshProUGUI>();

        // If no Button exists on the prefab or its children, add one.
        // Avoid adding an Image to a GameObject that already has another Graphic (e.g., TextMeshProUGUI).
        if (button == null)
        {
            // If the root already has a Graphic (Image/TextMeshProUGUI/etc), create a child click area
            var existingGraphic = listItem.GetComponent<Graphic>();
            if (existingGraphic == null)
            {
                // safe to add Image+Button to root
                var img = listItem.AddComponent<Image>();
                img.color = new Color(1f, 1f, 1f, 0f); // transparent
                img.raycastTarget = true;
                button = listItem.AddComponent<Button>();
                if (button != null) button.targetGraphic = img;
                Debug.Log("Added Button + transparent Image to list item root for click handling");
            }
            else
            {
                // create a stretched child to host the Image+Button so we don't conflict with existing Graphic
                var clickArea = new GameObject("CLICK_AREA", typeof(RectTransform));
                clickArea.transform.SetParent(listItem.transform, false);
                var rt = clickArea.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = Vector2.zero;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;

                var img = clickArea.AddComponent<Image>();
                img.color = new Color(1f, 1f, 1f, 0f);
                img.raycastTarget = true;

                button = clickArea.AddComponent<Button>();
                if (button != null) button.targetGraphic = img;
                Debug.Log("Added child CLICK_AREA with transparent Image+Button for click handling");
            }
        }

        if (text != null)
        {
            string statusIcon = monster.IsDead ? "X" : "O";
            text.text = $"{statusIcon} {monster.NickName} (Lv.{monster.Level})";
            Debug.Log($"Set text: {text.text}");
        }
        else
        {
            Debug.LogError("TextMeshProUGUI component not found in list item!");
        }

            if (button != null)
            {
                if (button.onClick != null)
                {
                    button.onClick.AddListener(() => SelectMonster(monster));
                    Debug.Log("Added button click listener");
                }
                else
                {
                    Debug.LogError("Button.onClick is null on the created/found Button");
                }
            }
            else
            {
                Debug.LogError("Button component not found in list item!");
            }

            Debug.Log($"--- End CreateMonsterListItem ---");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Exception in CreateMonsterListItem: {ex}\nMonster: {monster?.NickName ?? "<null>"}");
        }
    }

    private void SelectMonster(Monster monster)
    {
        if (monsterDetailUI != null)
        {
            monsterDetailUI.gameObject.SetActive(true);
            monsterDetailUI.DisplayMonster(monster);
        }
    }

    private void CreateRandomMonster()
    {
        Debug.Log("=== CreateRandomMonster Debug ===");
        
        // MonsterTypeが無い場合はサンプル種族を作成
        if (MonsterManager.Instance.AllMonsterTypes.Count == 0)
        {
            Debug.Log("No MonsterTypes available, trying to create sample species...");
            if (SpeciesManagement.MonsterSpeciesManager.Instance != null)
            {
                SpeciesManagement.MonsterSpeciesManager.Instance.CreateSampleSpecies();
            }
        }
        
        var randomMonster = MonsterManager.Instance.GenerateRandomMonster();
        if (randomMonster != null)
        {
            MonsterManager.Instance.AddMonster(randomMonster);
            RefreshMonsterList();
            Debug.Log($"Successfully created random monster: {randomMonster.NickName}");
        }
        else
        {
            Debug.LogError("Failed to create random monster");
        }
        
        Debug.Log("=== End CreateRandomMonster Debug ===");
    }

    private void UpdatePartyInfo()
    {
        if (partyInfoText != null)
        {
            var manager = MonsterManager.Instance;
            int total = manager.PlayerMonsters.Count;
            int alive = manager.GetAliveMonsters().Count;
            int dead = manager.GetDeadMonsters().Count;
            float avgLevel = manager.GetAverageLevel();

            partyInfoText.text = $"Party: {total} (Alive: {alive}, Dead: {dead})\nAvg Level: {avgLevel:F1}";
        }
    }
}
