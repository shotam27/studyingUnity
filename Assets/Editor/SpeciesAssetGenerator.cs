using System.IO;
using UnityEngine;
using UnityEditor;

public static class SpeciesAssetGenerator
{
    private const string ResourcesFolder = "Assets/Resources";
    private const string MonsterTypesFolder = "Assets/Resources/MonsterTypes";
    private const string MonsterPrefabsFolder = "Assets/Resources/MonsterTypes/Prefabs";
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
            
            // Prepare variable for sprite lookup and later prefab creation
            Sprite sprite = null;

            // Try assign sprite from Resources/Images/{name} first, then search project assets for a matching Sprite
            var propSprite = so.FindProperty("sprite");
            if (propSprite != null)
            {
                // try Resources/Images/{name}
                try
                {
                    string resPath = Path.Combine(ImagesFolderResources, it.name);
                    Debug.Log($"SpeciesAssetGenerator: Trying Resources.Load<Sprite>('{resPath}') for '{it.name}'");
                    sprite = Resources.Load<Sprite>(resPath);
                    Debug.Log(sprite == null ? $"SpeciesAssetGenerator: Resources.Load returned null for '{resPath}'" : $"SpeciesAssetGenerator: Resources.Load found sprite '{sprite.name}' at '{resPath}'");
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"SpeciesAssetGenerator: Resources.Load threw for '{it.name}': {ex.Message}");
                    sprite = null;
                }
                if (sprite == null)
                {
                    try
                    {
                        string resPath2 = Path.Combine(ImagesFolderResources, SanitizeFileName(it.name));
                        Debug.Log($"SpeciesAssetGenerator: Trying Resources.Load<Sprite>('{resPath2}') (sanitized) for '{it.name}'");
                        sprite = Resources.Load<Sprite>(resPath2);
                        Debug.Log(sprite == null ? $"SpeciesAssetGenerator: Resources.Load returned null for '{resPath2}'" : $"SpeciesAssetGenerator: Resources.Load found sprite '{sprite.name}' at '{resPath2}'");
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"SpeciesAssetGenerator: Resources.Load(sanitized) threw for '{it.name}': {ex.Message}");
                        sprite = null;
                    }
                }

                // If still null, search the project for a Sprite asset with the same name (or sanitized name)
                if (sprite == null)
                {
                    string[] searchNames = new[] { it.name, SanitizeFileName(it.name) };
                    foreach (var nameTry in searchNames)
                    {
                        if (string.IsNullOrEmpty(nameTry)) continue;
                        // Find sprite assets by name
                        string[] guids = AssetDatabase.FindAssets($"t:Sprite {nameTry}");
                        Debug.Log($"SpeciesAssetGenerator: AssetDatabase.FindAssets for 't:Sprite {nameTry}' returned { (guids==null?0:guids.Length)} results.");
                        if (guids != null && guids.Length > 0)
                        {
                            string assetPathFound = AssetDatabase.GUIDToAssetPath(guids[0]);
                            Debug.Log($"SpeciesAssetGenerator: Found GUID {guids[0]} -> path '{assetPathFound}'");
                            var loaded = AssetDatabase.LoadAssetAtPath<Sprite>(assetPathFound);
                            if (loaded != null)
                            {
                                Debug.Log($"SpeciesAssetGenerator: Loaded Sprite '{loaded.name}' from '{assetPathFound}'");
                                sprite = loaded;
                                break;
                            }
                            else
                            {
                                Debug.Log($"SpeciesAssetGenerator: LoadAssetAtPath<Sprite> returned null for '{assetPathFound}'");
                            }
                        }
                        // Try searching textures by name and load as Sprite if importer has sprite mode
                        if (sprite == null)
                        {
                            string[] texGuids = AssetDatabase.FindAssets(nameTry);
                            Debug.Log($"SpeciesAssetGenerator: AssetDatabase.FindAssets('{nameTry}') returned { (texGuids==null?0:texGuids.Length)} results for generic search.");
                            foreach (var g in texGuids)
                            {
                                string p = AssetDatabase.GUIDToAssetPath(g);
                                Debug.Log($"SpeciesAssetGenerator: Checking asset at '{p}'");
                                var maybeSprite = AssetDatabase.LoadAssetAtPath<Sprite>(p);
                                if (maybeSprite != null)
                                {
                                    Debug.Log($"SpeciesAssetGenerator: Found sprite '{maybeSprite.name}' at '{p}' via generic search.");
                                    sprite = maybeSprite;
                                    break;
                                }
                            }
                            if (sprite != null) break;
                        }
                    }
                }

                propSprite.objectReferenceValue = sprite;
            }

            so.ApplyModifiedProperties();

            // Ensure prefab folder exists and create/update a prefab for this species with the Command component
            try
            {
                if (!Directory.Exists(MonsterPrefabsFolder)) Directory.CreateDirectory(MonsterPrefabsFolder);
                string prefabPath = Path.Combine(MonsterPrefabsFolder, fileName + ".prefab");

                // If sprite exists, create or update prefab to include SpriteRenderer + Collider2D + Command
                if (sprite != null)
                {
                    // Use PrefabUtility APIs to modify/create prefab contents
                    var prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
                    bool wasCreated = false;
                    if (prefabRoot == null)
                    {
                        prefabRoot = new GameObject(fileName);
                        wasCreated = true;
                    }

                    // Ensure SpriteRenderer
                    var sr = prefabRoot.GetComponent<SpriteRenderer>();
                    if (sr == null) sr = prefabRoot.AddComponent<SpriteRenderer>();
                    sr.sprite = sprite;

                    // Ensure a 2D collider so OnMouseDown works (Command uses Collider2D)
                    var col = prefabRoot.GetComponent<Collider2D>();
                    if (col == null) prefabRoot.AddComponent<CircleCollider2D>();

                    // Ensure Command component is attached
                    var cmd = prefabRoot.GetComponent<Command>();
                    if (cmd == null) prefabRoot.AddComponent<Command>();

                    // Save or replace prefab
                    PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
                    if (wasCreated) GameObject.DestroyImmediate(prefabRoot);

                    Debug.Log($"SpeciesAssetGenerator: Created/Updated prefab at '{prefabPath}' with Command and sprite '{sprite.name}'.");
                }
                else
                {
                    // No sprite: still ensure prefab exists with Command (empty visuals)
                    if (!File.Exists(prefabPath))
                    {
                        var tempGO = new GameObject(fileName);
                        tempGO.AddComponent<Command>();
                        PrefabUtility.SaveAsPrefabAsset(tempGO, prefabPath);
                        GameObject.DestroyImmediate(tempGO);
                        Debug.Log($"SpeciesAssetGenerator: Created empty prefab at '{prefabPath}' with Command (no sprite available).");
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"SpeciesAssetGenerator: failed to create/update prefab for '{it.name}': {ex.Message}");
            }

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
