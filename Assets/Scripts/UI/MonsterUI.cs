using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MonsterUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI atkText;
    [SerializeField] private TextMeshProUGUI defText;
    [SerializeField] private TextMeshProUGUI spdText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Image monsterImage;
    [SerializeField] private Transform skillsParent;
    [SerializeField] private GameObject skillItemPrefab;

    [Header("Auto Generate")]
    [SerializeField] private bool autoGenerateUI = true;

    [Header("Font")]
    [Tooltip("Assign the GN-Koharuiro_Sunray SDF TMP Font Asset here")]
    [SerializeField] private TMP_FontAsset myFont;

    private Monster currentMonster;

    private void Awake()
    {
        if (autoGenerateUI)
        {
            GenerateUI();
        }
        // Apply assigned TMP font to existing/generated TMP texts
        ApplyDefaultFont();
    }

    private void ApplyDefaultFont()
    {
        if (myFont == null) return;
        // Apply to explicit serialized fields
        if (nameText != null) nameText.font = myFont;
        if (levelText != null) levelText.font = myFont;
        if (hpText != null) hpText.font = myFont;
        if (atkText != null) atkText.font = myFont;
        if (defText != null) defText.font = myFont;
        if (spdText != null) spdText.font = myFont;
        if (statusText != null) statusText.font = myFont;
        // Apply to any TMP children (covers generated elements)
        var all = GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var t in all) t.font = myFont;
    }

    [ContextMenu("Generate UI")]
    public void GenerateUI()
    {
        // 既存のUI要素をクリア（手動で設定されている場合は保持）
        if (nameText == null) nameText = CreateTextElement("NameText", "Monster Name", new Vector2(0, 200));
        if (levelText == null) levelText = CreateTextElement("LevelText", "Lv.1", new Vector2(0, 160));
        if (hpText == null) hpText = CreateTextElement("HPText", "HP: 100/100", new Vector2(0, 120));
        if (atkText == null) atkText = CreateTextElement("ATKText", "ATK: 10", new Vector2(0, 80));
        if (defText == null) defText = CreateTextElement("DEFText", "DEF: 5", new Vector2(0, 40));
        if (spdText == null) spdText = CreateTextElement("SPDText", "SPD: 10", new Vector2(0, 0));
        if (statusText == null) statusText = CreateTextElement("StatusText", "Alive", new Vector2(0, -40));
        
        // モンスター画像
        if (monsterImage == null) monsterImage = CreateImageElement("MonsterImage", new Vector2(-200, 100));
        
        // スキルリスト用ScrollView
        if (skillsParent == null) 
        {
            var scrollView = CreateScrollView("SkillsScrollView", new Vector2(200, 0));
            skillsParent = scrollView.transform.Find("Viewport/Content");
        }
        
        // スキルアイテムプレハブ
        if (skillItemPrefab == null) skillItemPrefab = CreateSkillItemPrefab();
    }

    private TextMeshProUGUI CreateTextElement(string name, string text, Vector2 position)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(transform, false);
        
        var textComponent = textObj.AddComponent<TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.fontSize = 18;
        textComponent.color = Color.white;
        textComponent.alignment = TextAlignmentOptions.Left;
        
        var rectTransform = textObj.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = new Vector2(200, 30);
        
    // add simple background
    return textComponent;
    }

    private Image CreateImageElement(string name, Vector2 position)
    {
        GameObject imageObj = new GameObject(name);
        imageObj.transform.SetParent(transform, false);
        
        var imageComponent = imageObj.AddComponent<Image>();
        imageComponent.color = Color.gray;
        
        var rectTransform = imageObj.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = new Vector2(100, 100);
        
        return imageComponent;
    }

    private GameObject CreateScrollView(string name, Vector2 position)
    {
        GameObject scrollViewObj = new GameObject(name);
        scrollViewObj.transform.SetParent(transform, false);
        
        // RectTransformを最初に追加
        var rectTransform = scrollViewObj.AddComponent<RectTransform>();
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = new Vector2(180, 150);
        
        var scrollRect = scrollViewObj.AddComponent<ScrollRect>();
        var image = scrollViewObj.AddComponent<Image>();
        image.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        
        // Viewport
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollViewObj.transform, false);
        var viewportRect = viewport.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;
        viewport.AddComponent<Mask>();
        viewport.AddComponent<Image>();
        
        // Content
        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        var contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0, 200);
        
        var verticalLayoutGroup = content.AddComponent<VerticalLayoutGroup>();
        verticalLayoutGroup.spacing = 5;
        verticalLayoutGroup.padding = new RectOffset(5, 5, 5, 5);
        
        var contentSizeFitter = content.AddComponent<ContentSizeFitter>();
        contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        scrollRect.content = contentRect;
        scrollRect.viewport = viewportRect;
        scrollRect.vertical = true;
        scrollRect.horizontal = false;
        
        return scrollViewObj;
    }

    private GameObject CreateSkillItemPrefab()
    {
        GameObject skillItem = new GameObject("SkillItemPrefab");
        var rectTransform = skillItem.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(160, 25);
        
        var textComponent = skillItem.AddComponent<TextMeshProUGUI>();
        textComponent.text = "Skill Name";
        textComponent.fontSize = 14;
        textComponent.color = Color.white;
    return skillItem;
    }


    public void DisplayMonster(Monster monster)
    {
        currentMonster = monster;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (currentMonster == null) return;

        // 基本情報表示
    nameText.text = $"{currentMonster.NickName} ({currentMonster.MonsterType.SpeciesName})";
        levelText.text = $"Lv.{currentMonster.Level}";
        
        // ステータス表示
        hpText.text = $"HP: {currentMonster.CurrentHP}/{currentMonster.MaxHP}";
        atkText.text = $"ATK: {currentMonster.ATK}";
        defText.text = $"DEF: {currentMonster.DEF}";
        spdText.text = $"SPD: {currentMonster.SPD}";
        
        // 状態表示
        statusText.text = currentMonster.IsDead ? "Dead" : "Alive";
        statusText.color = currentMonster.IsDead ? Color.red : Color.green;
        
        // スプライト表示
        if (monsterImage != null && currentMonster.MonsterType.Sprite != null)
        {
            monsterImage.sprite = currentMonster.MonsterType.Sprite;
        }
        
        // スキル表示
        DisplaySkills();
    }

    private void DisplaySkills()
    {
        // 既存のスキルUIを削除
        foreach (Transform child in skillsParent)
        {
            Destroy(child.gameObject);
        }

        // スキルを表示
        foreach (var skill in currentMonster.LearnedSkills)
        {
            if (skillItemPrefab != null)
            {
                var skillItem = Instantiate(skillItemPrefab, skillsParent);
                var skillText = skillItem.GetComponent<TextMeshProUGUI>();
                if (skillText != null)
                {
                    skillText.text = $"{skill.SkillName} ({skill.Tag})";
                }
            }
        }
    }
}
