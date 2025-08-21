using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;

namespace SpeciesManagement
{
    /// <summary>
    /// MonsterType種族の管理を専門に行うクラス
    /// MonsterManagerとは分離して、種族データの管理に特化
    /// </summary>
    public class MonsterSpeciesManager : MonoBehaviour
    {
        [Header("Species Database")]
        [SerializeField] private List<MonsterType> registeredSpecies = new List<MonsterType>();
        [SerializeField] private string saveFileName = "monster_species_data.json";
        [SerializeField] private string webJsonFileName = "monster-species.json"; // Webページ用
        
        [Header("Settings")]
        [SerializeField] private bool autoSave = true;
        [SerializeField] private bool loadOnStart = true;
        [SerializeField] private bool useStreamingAssets = true; // StreamingAssetsからの読み込み
        
        // シングルトン
        public static MonsterSpeciesManager Instance { get; private set; }
        
        // イベント
        public System.Action<MonsterType> OnSpeciesAdded;
        public System.Action<MonsterType> OnSpeciesRemoved;
        public System.Action<MonsterType> OnSpeciesUpdated;
        public System.Action OnSpeciesListChanged;
        
        // プロパティ
        public List<MonsterType> AllSpecies => new List<MonsterType>(registeredSpecies);
        public int SpeciesCount => registeredSpecies.Count;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                
                if (loadOnStart)
                {
                    LoadSpeciesData();
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        #region CRUD Operations
        
        /// <summary>
        /// 新しい種族を登録
        /// </summary>
        public bool AddSpecies(MonsterType species)
        {
            if (species == null)
            {
                Debug.LogError("Cannot add null species");
                return false;
            }
            
            // 重複チェック
            if (IsSpeciesRegistered(species))
            {
                Debug.LogWarning($"Species {species.MonsterTypeName} is already registered");
                return false;
            }
            
            registeredSpecies.Add(species);
            OnSpeciesAdded?.Invoke(species);
            OnSpeciesListChanged?.Invoke();
            
            if (autoSave) SaveSpeciesData();
            
            Debug.Log($"Added species: {species.MonsterTypeName}");
            return true;
        }
        
        /// <summary>
        /// 種族を削除
        /// </summary>
        public bool RemoveSpecies(MonsterType species)
        {
            if (species == null) return false;
            
            bool removed = registeredSpecies.Remove(species);
            if (removed)
            {
                OnSpeciesRemoved?.Invoke(species);
                OnSpeciesListChanged?.Invoke();
                
                if (autoSave) SaveSpeciesData();
                
                Debug.Log($"Removed species: {species.MonsterTypeName}");
            }
            
            return removed;
        }
        
        /// <summary>
        /// 種族情報を更新
        /// </summary>
        public void UpdateSpecies(MonsterType species)
        {
            if (species == null || !registeredSpecies.Contains(species)) return;
            
            OnSpeciesUpdated?.Invoke(species);
            
            if (autoSave) SaveSpeciesData();
            
            Debug.Log($"Updated species: {species.MonsterTypeName}");
        }
        
        #endregion
        
        #region Search & Filter
        
        /// <summary>
        /// 種族が登録済みかチェック
        /// </summary>
        public bool IsSpeciesRegistered(MonsterType species)
        {
            return registeredSpecies.Contains(species) || 
                   registeredSpecies.Any(s => s.MonsterTypeName == species.MonsterTypeName);
        }
        
        /// <summary>
        /// 名前で種族を検索
        /// </summary>
        public MonsterType GetSpeciesByName(string name)
        {
            return registeredSpecies.FirstOrDefault(s => s.MonsterTypeName == name);
        }
        
        /// <summary>
        /// IDで種族を検索
        /// </summary>
        public MonsterType GetSpeciesById(int id)
        {
            return (id >= 0 && id < registeredSpecies.Count) ? registeredSpecies[id] : null;
        }
        
        /// <summary>
        /// 弱点タグで種族をフィルタリング
        /// </summary>
        public List<MonsterType> GetSpeciesByWeakness(WeaknessTag weakness)
        {
            return registeredSpecies.Where(s => s.WeaknessTag == weakness).ToList();
        }
        
        /// <summary>
        /// 強さタグで種族をフィルタリング
        /// </summary>
        public List<MonsterType> GetSpeciesByStrength(StrongnessTag strength)
        {
            return registeredSpecies.Where(s => s.StrongnessTag == strength).ToList();
        }
        
        /// <summary>
        /// ステータス範囲で種族をフィルタリング
        /// </summary>
        public List<MonsterType> GetSpeciesByStatRange(int minHP, int maxHP)
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
            try
            {
                string jsonPath = GetJsonFilePath();
                
                if (File.Exists(jsonPath))
                {
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
                LoadFromResources(); // フォールバック
            }
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
                    var monsterType = CreateMonsterTypeFromJson(speciesData);
                    if (monsterType != null)
                    {
                        registeredSpecies.Add(monsterType);
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
        /// JSON用のSpeciesDataからMonsterTypeを作成
        /// </summary>
        private MonsterType CreateMonsterTypeFromJson(SpeciesJsonData data)
        {
            try
            {
                var monsterType = ScriptableObject.CreateInstance<MonsterType>();
                
                // 基本情報設定
                monsterType.MonsterTypeName = data.name;
                
                // BasicStatus作成
                var basicStatus = ScriptableObject.CreateInstance<BasicStatus>();
                basicStatus.MaxHP = data.basicStatus.maxHP;
                basicStatus.ATK = data.basicStatus.atk;
                basicStatus.DEF = data.basicStatus.def;
                basicStatus.SPD = data.basicStatus.spd;
                monsterType.BasicStatus = basicStatus;
                
                // Enum変換
                if (System.Enum.TryParse<WeaknessTag>(data.weakness, out var weakness))
                    monsterType.WeaknessTag = weakness;
                
                if (System.Enum.TryParse<StrongnessTag>(data.strength, out var strength))
                    monsterType.StrongnessTag = strength;
                
                // その他の設定（デフォルト値）
                monsterType.CaptureRate = 0.5f;
                monsterType.GrowthRate = 1.0f;
                
                return monsterType;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to create MonsterType from JSON data: {ex.Message}");
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
                    id = species.name ?? species.MonsterTypeName?.Replace(" ", "_").ToLower(),
                    name = species.MonsterTypeName,
                    description = $"A {species.MonsterTypeName} species",
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
            MonsterType[] loadedTypes = Resources.LoadAll<MonsterType>("MonsterTypes");
            registeredSpecies.Clear();
            registeredSpecies.AddRange(loadedTypes);
            
            Debug.Log($"Loaded {registeredSpecies.Count} species from Resources");
            OnSpeciesListChanged?.Invoke();
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
                
                if (string.IsNullOrEmpty(species.MonsterTypeName))
                {
                    issues.Add($"Species at index {i} has no name");
                }
                
                if (species.BasicStatus == null)
                {
                    issues.Add($"Species {species.MonsterTypeName} has no BasicStatus");
                }
                
                // 重複チェック
                for (int j = i + 1; j < registeredSpecies.Count; j++)
                {
                    if (registeredSpecies[j]?.MonsterTypeName == species.MonsterTypeName)
                    {
                        issues.Add($"Duplicate species name: {species.MonsterTypeName}");
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
            registeredSpecies.Clear();
            
            // サンプルデータ作成
            var sampleSpecies = new[]
            {
                CreateSampleMonsterType("Flame Dragon", 150, 120, 80, 70, WeaknessTag.Ice, StrongnessTag.Fire),
                CreateSampleMonsterType("Forest Wolf", 80, 75, 55, 90, WeaknessTag.Fire, StrongnessTag.Earth),
                CreateSampleMonsterType("Crystal Golem", 200, 60, 120, 30, WeaknessTag.Dark, StrongnessTag.Light),
                CreateSampleMonsterType("Thunder Bird", 100, 90, 50, 110, WeaknessTag.Earth, StrongnessTag.Electric),
                CreateSampleMonsterType("Ice Bear", 140, 85, 90, 40, WeaknessTag.Fire, StrongnessTag.Ice)
            };
            
            foreach (var species in sampleSpecies)
            {
                registeredSpecies.Add(species);
            }
            
            OnSpeciesListChanged?.Invoke();
            Debug.Log($"Created {sampleSpecies.Length} sample species");
        }
        
        /// <summary>
        /// サンプル用MonsterTypeを作成
        /// </summary>
        private MonsterType CreateSampleMonsterType(string name, int hp, int atk, int def, int spd, 
            WeaknessTag weakness, StrongnessTag strength)
        {
            var monsterType = ScriptableObject.CreateInstance<MonsterType>();
            monsterType.MonsterTypeName = name;
            
            var basicStatus = ScriptableObject.CreateInstance<BasicStatus>();
            basicStatus.MaxHP = hp;
            basicStatus.ATK = atk;
            basicStatus.DEF = def;
            basicStatus.SPD = spd;
            
            monsterType.BasicStatus = basicStatus;
            monsterType.WeaknessTag = weakness;
            monsterType.StrongnessTag = strength;
            monsterType.CaptureRate = 0.5f;
            monsterType.GrowthRate = 1.0f;
            
            return monsterType;
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
