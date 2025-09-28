using UnityEngine;
using UnityEditor;
using TMPro;

[InitializeOnLoad]
public static class MonsterListItemFontUpdater
{
    static MonsterListItemFontUpdater()
    {
        // Auto-run on Editor startup or script reload
        Debug.Log("MonsterListItemFontUpdater: Auto-updating MonsterListItem font...");
        UpdateMonsterListItemFont();
    }

    [MenuItem("Tools/Update MonsterListItem Font to GenEiKiwamiGo")]
    public static void UpdateMonsterListItemFont()
    {
        // Load the MonsterListItem prefab from Assets/Prefabs/
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/MonsterListItem.prefab");
        if (prefab == null)
        {
            Debug.LogError("MonsterListItemFontUpdater: MonsterListItem prefab not found at 'Assets/Prefabs/MonsterListItem.prefab'.");
            return;
        }

        // Load the GenEiKiwamiGo font asset (from TextMesh Pro/Fonts/)
        TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Fonts/GenEiKiwamiGo SDF.asset");
        if (fontAsset == null)
        {
            Debug.LogError("MonsterListItemFontUpdater: GenEiKiwamiGo SDF font asset not found at 'Assets/TextMesh Pro/Fonts/GenEiKiwamiGo SDF.asset'. Please ensure the font is imported.");
            return;
        }

        // Load the prefab contents for editing
        string prefabPath = AssetDatabase.GetAssetPath(prefab);
        if (string.IsNullOrEmpty(prefabPath))
        {
            Debug.LogError("MonsterListItemFontUpdater: Could not get asset path for MonsterListItem prefab.");
            return;
        }

        GameObject prefabContents = PrefabUtility.LoadPrefabContents(prefabPath);

        // Find all TextMeshProUGUI components in the prefab
        TextMeshProUGUI[] tmpTexts = prefabContents.GetComponentsInChildren<TextMeshProUGUI>(true);
        if (tmpTexts.Length == 0)
        {
            Debug.LogWarning("MonsterListItemFontUpdater: No TextMeshProUGUI components found in MonsterListItem prefab.");
            PrefabUtility.UnloadPrefabContents(prefabContents);
            return;
        }

        // Update the font for each TextMeshProUGUI
        foreach (TextMeshProUGUI tmp in tmpTexts)
        {
            tmp.font = fontAsset;
            Debug.Log($"MonsterListItemFontUpdater: Updated font for TextMeshProUGUI '{tmp.name}' to GenEiKiwamiGo.");
        }

        // Save the modified prefab
        PrefabUtility.SaveAsPrefabAsset(prefabContents, prefabPath);
        PrefabUtility.UnloadPrefabContents(prefabContents);

        Debug.Log($"MonsterListItemFontUpdater: Successfully updated MonsterListItem prefab font to GenEiKiwamiGo. Modified {tmpTexts.Length} TextMeshProUGUI components.");

        // Also update MonsterUI components in the scene
        UpdateMonsterUIs(fontAsset);
    }

    private static void UpdateMonsterUIs(TMP_FontAsset fontAsset)
    {
        // Find all MonsterUI components in the scene
        var monsterUIs = Object.FindObjectsOfType<MonsterUI>(true);
        foreach (var ui in monsterUIs)
        {
            ui.myFont = fontAsset;
            Debug.Log($"MonsterListItemFontUpdater: Updated MonsterUI '{ui.name}' font to GenEiKiwamiGo.");
        }
    }
}