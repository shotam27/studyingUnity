using System.IO;
using UnityEngine;
using UnityEditor;

public static class SpeciesAssetGenerator
{
    private const string ResourcesFolder = "Assets/Resources";
    private const string MonsterTypesFolder = "Assets/Resources/MonsterTypes";
    private const string ImagesFolderResources = "Images"; // Resources/Images/{name}

    [MenuItem("Tools/Generate Species From StreamingAssets")]
    public static void GenerateSpeciesAssets()
    {
        // ensure folders
        if (!Directory.Exists(ResourcesFolder)) Directory.CreateDirectory(ResourcesFolder);
        if (!Directory.Exists(MonsterTypesFolder)) Directory.CreateDirectory(MonsterTypesFolder);

        string path = Path.Combine(Application.streamingAssetsPath, "monster-species.json");
        if (!File.Exists(path))
        {
            Debug.LogError($"SpeciesAssetGenerator: StreamingAssets file not found: {path}");
            return;
        }

        string raw = File.ReadAllText(path);
        string wrapped = "{\"items\":" + raw + "}";
        var wrapper = JsonUtility.FromJson<StreamingSpeciesWrapper>(wrapped);
        if (wrapper == null || wrapper.items == null)
        {
            Debug.LogError("SpeciesAssetGenerator: failed to parse JSON or no items found.");
            return;
        }

        int created = 0, updated = 0, skipped = 0;
        foreach (var it in wrapper.items)
        {
            if (string.IsNullOrEmpty(it.name)) { skipped++; continue; }

            string fileName = SanitizeFileName(it.name);
            string assetPath = Path.Combine(MonsterTypesFolder, fileName + ".asset");

            Species asset = AssetDatabase.LoadAssetAtPath<Species>(assetPath);
            bool isNew = false;
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<Species>();
                isNew = true;
            }

            // Use SerializedObject to set private fields
            SerializedObject so = new SerializedObject(asset);
            var propName = so.FindProperty("monsterTypeName");
            if (propName != null) propName.stringValue = it.name;

            var propBasic = so.FindProperty("basicStatus");
            if (propBasic != null && it.basicStatus != null)
            {
                var pMax = propBasic.FindPropertyRelative("maxHP");
                var pAtk = propBasic.FindPropertyRelative("atk");
                var pDef = propBasic.FindPropertyRelative("def");
                var pSpd = propBasic.FindPropertyRelative("spd");
                if (pMax != null) pMax.intValue = it.basicStatus.maxHP;
                if (pAtk != null) pAtk.intValue = it.basicStatus.atk;
                if (pDef != null) pDef.intValue = it.basicStatus.def;
                if (pSpd != null) pSpd.intValue = it.basicStatus.spd;
            }

            // Try assign sprite from Resources/Images/{name} first, then search project assets for a matching Sprite
            var propSprite = so.FindProperty("sprite");
            if (propSprite != null)
            {
                Sprite sp = null;
                // try Resources/Images/{name}
                try { sp = Resources.Load<Sprite>(Path.Combine(ImagesFolderResources, it.name)); } catch { sp = null; }
                if (sp == null)
                {
                    try { sp = Resources.Load<Sprite>(Path.Combine(ImagesFolderResources, SanitizeFileName(it.name))); } catch { sp = null; }
                }

                // If still null, search the project for a Sprite asset with the same name (or sanitized name)
                if (sp == null)
                {
                    string[] searchNames = new[] { it.name, SanitizeFileName(it.name) };
                    foreach (var nameTry in searchNames)
                    {
                        if (string.IsNullOrEmpty(nameTry)) continue;
                        // Find sprite assets by name
                        string[] guids = AssetDatabase.FindAssets($"t:Sprite {nameTry}");
                        if (guids != null && guids.Length > 0)
                        {
                            string assetPathFound = AssetDatabase.GUIDToAssetPath(guids[0]);
                            var loaded = AssetDatabase.LoadAssetAtPath<Sprite>(assetPathFound);
                            if (loaded != null)
                            {
                                sp = loaded;
                                break;
                            }
                        }
                        // Try searching textures by name and load as Sprite if importer has sprite mode
                        if (sp == null)
                        {
                            string[] texGuids = AssetDatabase.FindAssets(nameTry);
                            foreach (var g in texGuids)
                            {
                                string p = AssetDatabase.GUIDToAssetPath(g);
                                var maybeSprite = AssetDatabase.LoadAssetAtPath<Sprite>(p);
                                if (maybeSprite != null)
                                {
                                    sp = maybeSprite;
                                    break;
                                }
                            }
                            if (sp != null) break;
                        }
                    }
                }

                propSprite.objectReferenceValue = sp;
            }

            so.ApplyModifiedProperties();

            if (isNew)
            {
                AssetDatabase.CreateAsset(asset, assetPath);
                created++;
            }
            else
            {
                EditorUtility.SetDirty(asset);
                updated++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"SpeciesAssetGenerator: Created={created} Updated={updated} Skipped={skipped}");
    }

    private static string SanitizeFileName(string input)
    {
        if (string.IsNullOrEmpty(input)) return "unnamed";
        foreach (var c in Path.GetInvalidFileNameChars())
        {
            input = input.Replace(c, '_');
        }
        input = input.Replace(' ', '_');
        return input;
    }

    // JSON helper classes
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
}
