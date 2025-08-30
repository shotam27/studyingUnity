using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;

namespace SpeciesManagement
{
    /// <summary>
    /// Species種族の管理を専門に行うクラス
    /// MonsterManagerとは分離して、種族データの管理に特化
    /// </summary>
    public class MonsterSpeciesManager : MonoBehaviour
    {
        [Header("Species Database")]
    [SerializeField] private List<Species> registeredSpecies = new List<Species>();
        [SerializeField] private string saveFileName = "monster_species_data.json";
        [SerializeField] private string webJsonFileName = "monster-species.json"; // Webページ用
        
        [Header("Settings")]
        [SerializeField] private bool autoSave = true;
        [SerializeField] private bool loadOnStart = true;
        [SerializeField] private bool useStreamingAssets = true; // StreamingAssetsからの読み込み
        
        // シングルトン
        public static MonsterSpeciesManager Instance { get; private set; }
        
    // イベント
    public System.Action<Species> OnSpeciesAdded;
    public System.Action<Species> OnSpeciesRemoved;
    public System.Action<Species> OnSpeciesUpdated;
        public System.Action OnSpeciesListChanged;
        
    // プロパティ
    public List<Species> AllSpecies => new List<Species>(registeredSpecies);
        public int SpeciesCount => registeredSpecies.Count;
        
        private void Awake()
        {
            Debug.Log("=== MonsterSpeciesManager Awake ===");
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Debug.Log("MonsterSpeciesManager instance created");
                
                if (loadOnStart)
                {
                    Debug.Log("Loading species data on start");
                    LoadSpeciesData();
                }
                else
                {
                    Debug.Log("LoadOnStart is disabled, creating sample data");
                    CreateDefaultSampleSpecies();
                }
            }
            else
            {
                Debug.Log("MonsterSpeciesManager instance already exists, destroying duplicate");
                Destroy(gameObject);
            }
            Debug.Log("=== End MonsterSpeciesManager Awake ===");
        }
        
        #region CRUD Operations
        
        /// <summary>
        /// 新しい種族を登録
        /// </summary>
    public bool AddSpecies(Species species)
        {
            if (species == null)
            {
                Debug.LogError("Cannot add null species");
                return false;
            }
            
            // 重複チェック
            if (IsSpeciesRegistered(species))
            {
                Debug.LogWarning($"Species {species.SpeciesName} is already registered");
                return false;
            }
            
            registeredSpecies.Add(species);
            OnSpeciesAdded?.Invoke(species);
            OnSpeciesListChanged?.Invoke();
            
            if (autoSave) SaveSpeciesData();
            
            Debug.Log($"Added species: {species.SpeciesName}");
            return true;
        }
        
        /// <summary>
        /// 種族を削除
        /// </summary>
    public bool RemoveSpecies(Species species)
        {
            if (species == null) return false;
            
            bool removed = registeredSpecies.Remove(species);
            if (removed)
            {
        OnSpeciesRemoved?.Invoke(species);
                OnSpeciesListChanged?.Invoke();
                
                if (autoSave) SaveSpeciesData();
                
        Debug.Log($"Removed species: {species.SpeciesName}");
            }
            
            return removed;
        }
        
        /// <summary>
        /// 種族情報を更新
        /// </summary>
        public void UpdateSpecies(Species species)
        {
            if (species == null || !registeredSpecies.Contains(species)) return;

            OnSpeciesUpdated?.Invoke(species);

            if (autoSave) SaveSpeciesData();

            Debug.Log($"Updated species: {species.SpeciesName}");
        }
        
        #endregion
        
        #region Search & Filter
        
        /// <summary>
        /// 種族が登録済みかチェック
        /// </summary>
        public bool IsSpeciesRegistered(Species species)
        {
            return registeredSpecies.Contains(species) || 
                   registeredSpecies.Any(s => s.SpeciesName == species.SpeciesName);
        }
        
        /// <summary>
        /// 名前で種族を検索
        /// </summary>
        public Species GetSpeciesByName(string name)
        {
            return registeredSpecies.FirstOrDefault(s => s.SpeciesName == name);
        }
        
        /// <summary>
        /// IDで種族を検索
        /// </summary>
        public Species GetSpeciesById(int id)
        {
            return (id >= 0 && id < registeredSpecies.Count) ? registeredSpecies[id] : null;
        }
        
        /// <summary>
        /// 弱点タグで種族をフィルタリング
        /// </summary>
        public List<Species> GetSpeciesByWeakness(WeaknessTag weakness)
        {
            return registeredSpecies.Where(s => s.WeaknessTag == weakness).ToList();
        }
        
        /// <summary>
        /// 強さタグで種族をフィルタリング
        /// </summary>
        public List<Species> GetSpeciesByStrength(StrongnessTag strength)
        {
            return registeredSpecies.Where(s => s.StrongnessTag == strength).ToList();
        }
        
        /// <summary>
        /// ステータス範囲で種族をフィルタリング
        /// </summary>
        public List<Species> GetSpeciesByStatRange(int minHP, int maxHP)
        {
            return registeredSpecies.Where(s => 
                s.BasicStatus != null && 
                s.BasicStatus.MaxHP >= minHP && 
                s.BasicStatus.MaxHP <= maxHP
            ).ToList();
        }
        
        #endregion
        
        #region Data Persistence
        
        /// <summary>
        /// 種族データを保存
        /// </summary>
        public void SaveSpeciesData()
        {
            try
            {
                var saveData = new SpeciesSaveData
                {
                    speciesNames = registeredSpecies.Select(s => s.name).ToArray(),
                    saveTimestamp = System.DateTime.Now.ToBinary()
                };
                
                string json = JsonUtility.ToJson(saveData, true);
                string path = System.IO.Path.Combine(Application.persistentDataPath, saveFileName);
                System.IO.File.WriteAllText(path, json);
                
                Debug.Log($"Species data saved to: {path}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to save species data: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 種族データを読み込み
        /// </summary>
        public void LoadSpeciesData()
        {
            Debug.Log("=== LoadSpeciesData Debug ===");
            try
            {
                string jsonPath = GetJsonFilePath();
                Debug.Log($"JSON file path: {jsonPath}");
                
                if (File.Exists(jsonPath))
                {
                    Debug.Log("JSON file found, loading from JSON");
                    string json = File.ReadAllText(jsonPath);
                    LoadFromJsonString(json);
                    Debug.Log($"Loaded species data from: {jsonPath}");
                }
                else
                {
                    Debug.Log("No JSON species data found, loading from Resources as fallback");
                    LoadFromResources();
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to load species data: {ex.Message}");
                Debug.Log("Creating default sample species as fallback");
                CreateDefaultSampleSpecies();
            }
            Debug.Log("=== End LoadSpeciesData Debug ===");
        }
        
        /// <summary>
        /// JSONファイルパスを取得
        /// </summary>
        private string GetJsonFilePath()
        {
            if (useStreamingAssets)
            {
                // StreamingAssetsフォルダから読み込み（Webページからの更新を想定）
                return Path.Combine(Application.streamingAssetsPath, webJsonFileName);
            }
            else
            {
                // persistentDataPathから読み込み（アプリ内部の保存データ）
                return Path.Combine(Application.persistentDataPath, saveFileName);
            }
        }
        
        /// <summary>
        /// JSON文字列から種族データを読み込み
        /// </summary>
    public void LoadFromJsonString(string json)
        {
            try
            {
                var speciesArray = JsonHelper.FromJsonArray<SpeciesJsonData>(json);
                
                registeredSpecies.Clear();
                
                foreach (var speciesData in speciesArray)
                {
                    var speciesObj = CreateSpeciesFromJson(speciesData);
                    if (speciesObj != null)
                    {
                        registeredSpecies.Add(speciesObj);
                    }
                }
                
                Debug.Log($"Loaded {registeredSpecies.Count} species from JSON");
                OnSpeciesListChanged?.Invoke();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to parse JSON species data: {ex.Message}");
            }
        }
        
        /// <summary>
        /// JSON用のSpeciesDataからSpeciesを作成
        /// </summary>
        private Species CreateSpeciesFromJson(SpeciesJsonData data)
        {
            try
            {
                var speciesObj = ScriptableObject.CreateInstance<Species>();
                
                // リフレクションでプライベートフィールドに値を設定
                var speciesType = typeof(Species);
                var nameField = speciesType.GetField("monsterTypeName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var statusField = speciesType.GetField("basicStatus", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var weaknessField = speciesType.GetField("weaknessTag", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var strengthField = speciesType.GetField("strongnessTag", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                // BasicStatus作成
                var basicStatus = new BasicStatus(
                    data.basicStatus.maxHP,
                    data.basicStatus.atk,
                    data.basicStatus.def,
                    data.basicStatus.spd
                );
                
                // フィールドに値を設定
                nameField?.SetValue(speciesObj, data.name);
                statusField?.SetValue(speciesObj, basicStatus);
                
                // Enum変換
                if (System.Enum.TryParse<WeaknessTag>(data.weakness, out var weakness))
                    weaknessField?.SetValue(speciesObj, weakness);
                
                if (System.Enum.TryParse<StrongnessTag>(data.strength, out var strength))
                    strengthField?.SetValue(speciesObj, strength);
                
                return speciesObj;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to create Species from JSON data: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// 種族データをJSON形式で保存（Webページ用）
        /// </summary>
        public void SaveToWebJson()
        {
            try
            {
                var speciesArray = registeredSpecies.Select(species => new SpeciesJsonData
                {
                    id = species.name ?? species.SpeciesName?.Replace(" ", "_").ToLower(),
                    name = species.SpeciesName,
                    description = $"A {species.SpeciesName} species",
                    basicStatus = new BasicStatusJson
                    {
                        maxHP = species.BasicStatus?.MaxHP ?? 100,
                        atk = species.BasicStatus?.ATK ?? 50,
                        def = species.BasicStatus?.DEF ?? 50,
                        spd = species.BasicStatus?.SPD ?? 50
                    },
                    weakness = species.WeaknessTag.ToString(),
                    strength = species.StrongnessTag.ToString(),
                    rarity = "Common", // デフォルト値
                    category = "Beast", // デフォルト値
                    createdAt = System.DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                }).ToArray();
                
                string json = JsonHelper.ToJsonArray(speciesArray, true);
                
                // StreamingAssetsに保存
                string streamingPath = Path.Combine(Application.streamingAssetsPath, webJsonFileName);
                Directory.CreateDirectory(Path.GetDirectoryName(streamingPath));
                File.WriteAllText(streamingPath, json);
                
                Debug.Log($"Species data saved to web JSON: {streamingPath}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to save web JSON: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Resources フォルダから初期データを読み込み
        /// </summary>
        private void LoadFromResources()
        {
            Debug.Log("=== LoadFromResources Debug ===");
                Species[] loadedTypes = Resources.LoadAll<Species>("MonsterTypes");
                Debug.Log($"Found {loadedTypes.Length} Species in Resources");
            
                registeredSpecies.Clear();
                registeredSpecies.AddRange(loadedTypes);
            
            // Resourcesに何もない場合はサンプルデータを作成
            if (registeredSpecies.Count == 0)
            {
                Debug.Log("No Species found in Resources, creating default sample species");
                CreateDefaultSampleSpecies();
            }
            else
            {
                Debug.Log($"Loaded {registeredSpecies.Count} species from Resources");
                OnSpeciesListChanged?.Invoke();
            }
            Debug.Log("=== End LoadFromResources Debug ===");
        }
        
        #endregion
        
        #region Validation
        
        /// <summary>
        /// 種族データの整合性をチェック
        /// </summary>
        public void ValidateSpeciesData()
        {
            var issues = new List<string>();
            
            for (int i = 0; i < registeredSpecies.Count; i++)
            {
                var species = registeredSpecies[i];
                
                if (species == null)
                {
                    issues.Add($"Species at index {i} is null");
                    continue;
                }
                
                if (string.IsNullOrEmpty(species.SpeciesName))
                {
                    issues.Add($"Species at index {i} has no name");
                }
                
                if (species.BasicStatus == null)
                {
                    issues.Add($"Species {species.SpeciesName} has no BasicStatus");
                }
                
                // 重複チェック
                for (int j = i + 1; j < registeredSpecies.Count; j++)
                {
                    if (registeredSpecies[j]?.SpeciesName == species.SpeciesName)
                    {
                        issues.Add($"Duplicate species name: {species.SpeciesName}");
                    }
                }
            }
            
            if (issues.Count > 0)
            {
                Debug.LogWarning($"Species validation found {issues.Count} issues:\n" + string.Join("\n", issues));
            }
            else
            {
                Debug.Log("Species data validation passed");
            }
        }
        
        #endregion
        
        #region Context Menu Actions
        
        [ContextMenu("Validate All Species")]
        public void ValidateAllSpecies()
        {
            ValidateSpeciesData();
        }
        
        [ContextMenu("Save Species Data")]
        public void SaveSpeciesDataManual()
        {
            SaveSpeciesData();
        }
        
        [ContextMenu("Save to Web JSON")]
        public void SaveToWebJsonManual()
        {
            SaveToWebJson();
        }
        
        [ContextMenu("Load Species Data")]
        public void LoadSpeciesDataManual()
        {
            LoadSpeciesData();
        }
        
        [ContextMenu("Create Sample Species")]
        public void CreateSampleSpecies()
        {
            CreateDefaultSampleSpecies();
        }
        
        [ContextMenu("Clear All Species")]
        public void ClearAllSpecies()
        {
            registeredSpecies.Clear();
            OnSpeciesListChanged?.Invoke();
            Debug.Log("All species cleared");
        }
        
        /// <summary>
        /// サンプル種族データを作成
        /// </summary>
        private void CreateDefaultSampleSpecies()
        {
            Debug.Log("=== CreateDefaultSampleSpecies Debug ===");
            registeredSpecies.Clear();
            
            // サンプルデータ作成
            var sampleSpecies = new[]
            {
                CreateSampleMonsterType("Flame Dragon", 150, 120, 80, 70, WeaknessTag.Water, StrongnessTag.Fire),
                CreateSampleMonsterType("Forest Wolf", 80, 75, 55, 90, WeaknessTag.Fire, StrongnessTag.Earth),
                CreateSampleMonsterType("Crystal Golem", 200, 60, 120, 30, WeaknessTag.Dark, StrongnessTag.Light),
                CreateSampleMonsterType("Thunder Bird", 100, 90, 50, 110, WeaknessTag.Earth, StrongnessTag.Air),
                CreateSampleMonsterType("Ice Bear", 140, 85, 90, 40, WeaknessTag.Fire, StrongnessTag.Water)
            };
            
            foreach (var species in sampleSpecies)
            {
                if (species != null)
                {
                    registeredSpecies.Add(species);
                    Debug.Log($"Created sample species: {species.SpeciesName}");
                }
                else
                {
                    Debug.LogError("Failed to create sample species");
                }
            }
            
            OnSpeciesListChanged?.Invoke();
            Debug.Log($"Created {sampleSpecies.Length} sample species, total registered: {registeredSpecies.Count}");
            Debug.Log("=== End CreateDefaultSampleSpecies Debug ===");
        }
        
        /// <summary>
        /// サンプル用Speciesを作成
        /// </summary>
        private Species CreateSampleMonsterType(string name, int hp, int atk, int def, int spd, 
            WeaknessTag weakness, StrongnessTag strength)
        {
            try
            {
                Debug.Log($"Creating sample Species: {name}");
                var monsterType = ScriptableObject.CreateInstance<Species>();
                
                // リフレクションでプライベートフィールドに値を設定
                var monsterTypeType = typeof(Species);
                var nameField = monsterTypeType.GetField("monsterTypeName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var statusField = monsterTypeType.GetField("basicStatus", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var weaknessField = monsterTypeType.GetField("weaknessTag", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var strengthField = monsterTypeType.GetField("strongnessTag", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                var basicStatus = new BasicStatus(hp, atk, def, spd);
                
                nameField?.SetValue(monsterType, name);
                statusField?.SetValue(monsterType, basicStatus);
                weaknessField?.SetValue(monsterType, weakness);
                strengthField?.SetValue(monsterType, strength);
                
                Debug.Log($"Successfully created Species: {name} with HP:{hp} ATK:{atk} DEF:{def} SPD:{spd}");
                return monsterType;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to create sample Species '{name}': {ex.Message}");
                return null;
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// 種族データ保存用の構造体
    /// </summary>
    [System.Serializable]
    public class SpeciesSaveData
    {
        public string[] speciesNames;
        public long saveTimestamp;
    }
    
    /// <summary>
    /// JSON用の種族データ構造体（Webページ連携用）
    /// </summary>
    [System.Serializable]
    public class SpeciesJsonData
    {
        public string id;
        public string name;
        public string description;
        public BasicStatusJson basicStatus;
        public string weakness;
        public string strength;
        public string rarity;
        public string category;
        public string createdAt;
        public string updatedAt;
    }
    
    /// <summary>
    /// JSON用の基本ステータス構造体
    /// </summary>
    [System.Serializable]
    public class BasicStatusJson
    {
        public int maxHP;
        public int atk;
        public int def;
        public int spd;
    }
    
    /// <summary>
    /// JSON配列のシリアライズヘルパー
    /// </summary>
    public static class JsonHelper
    {
        public static T[] FromJsonArray<T>(string json)
        {
            string wrappedJson = "{\"array\":" + json + "}";
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(wrappedJson);
            return wrapper.array;
        }
        
        public static string ToJsonArray<T>(T[] array, bool prettyPrint = false)
        {
            Wrapper<T> wrapper = new Wrapper<T> { array = array };
            string json = JsonUtility.ToJson(wrapper, prettyPrint);
            return json.Substring(9, json.Length - 10); // Remove {"array": and }
        }
        
        [System.Serializable]
        private class Wrapper<T>
        {
            public T[] array;
        }
    }
}
