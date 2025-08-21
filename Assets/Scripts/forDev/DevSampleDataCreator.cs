using UnityEngine;

namespace ForDev
{
    /// <summary>
    /// 開発用サンプルデータ作成ヘルパー
    /// MonsterSpeciesUI用の閲覧データを作成
    /// </summary>
    public class DevSampleDataCreator : MonoBehaviour
    {
        [Header("Sample Creation Settings")]
        [SerializeField] private bool createOnStart = true;
        [SerializeField] private int speciesCount = 8;
        [SerializeField] private int skillsPerSpecies = 3;

        private void Start()
        {
            if (createOnStart)
            {
                // MonsterManagerの初期化を待つ
                StartCoroutine(CreateDataAfterManagerInit());
            }
        }

        /// <summary>
        /// MonsterManagerの初期化後にデータ作成
        /// </summary>
        private System.Collections.IEnumerator CreateDataAfterManagerInit()
        {
            // MonsterManagerが初期化されるまで待機
            while (MonsterManager.Instance == null)
            {
                yield return null;
            }

            // 追加で1フレーム待機（確実に初期化完了を待つ）
            yield return null;

            CreateDevSampleData();
        }

        /// <summary>
        /// 開発用サンプルデータを作成
        /// </summary>
        [ContextMenu("Create Dev Sample Data")]
        public void CreateDevSampleData()
        {
            Debug.Log("=== Creating Dev Sample Data ===");

            if (MonsterManager.Instance == null)
            {
                Debug.LogError("MonsterManager not found, cannot create sample data");
                return;
            }

            // 既存のサンプル作成を試みる
            var existingCreator = FindObjectOfType<SampleDataCreator>();
            if (existingCreator != null)
            {
                var method = typeof(SampleDataCreator).GetMethod("CreateTemporaryMonsterTypes", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                method?.Invoke(existingCreator, null);
            }

            // 追加の開発用MonsterTypeを作成
            CreateAdditionalDevSpecies();

            Debug.Log("=== End Creating Dev Sample Data ===");
        }

        /// <summary>
        /// 追加の開発用MonsterTypeを作成
        /// </summary>
        private void CreateAdditionalDevSpecies()
        {
            // 開発用の種族データ
            var devSpeciesData = new[]
            {
                new { name = "Debug Dragon", hp = 120, atk = 80, def = 60, spd = 40, weak = WeaknessTag.Water, strong = StrongnessTag.Fire },
                new { name = "Test Slime", hp = 50, atk = 30, def = 25, spd = 35, weak = WeaknessTag.Earth, strong = StrongnessTag.Water },
                new { name = "Sample Golem", hp = 200, atk = 60, def = 100, spd = 10, weak = WeaknessTag.Water, strong = StrongnessTag.Earth },
                new { name = "Dev Fairy", hp = 40, atk = 50, def = 30, spd = 80, weak = WeaknessTag.Fire, strong = StrongnessTag.Air },
                new { name = "Verify Beast", hp = 90, atk = 70, def = 50, spd = 60, weak = WeaknessTag.Earth, strong = StrongnessTag.Fire },
                new { name = "Review Wyvern", hp = 110, atk = 90, def = 40, spd = 70, weak = WeaknessTag.Water, strong = StrongnessTag.Air },
                new { name = "Code Specter", hp = 70, atk = 60, def = 35, spd = 85, weak = WeaknessTag.Light, strong = StrongnessTag.Dark },
                new { name = "Bug Fixer", hp = 80, atk = 75, def = 55, spd = 50, weak = WeaknessTag.Dark, strong = StrongnessTag.Light }
            };

            var manager = MonsterManager.Instance;
            var existingCount = manager.AllMonsterTypes.Count;

            for (int i = 0; i < Mathf.Min(speciesCount, devSpeciesData.Length); i++)
            {
                var data = devSpeciesData[i];

                // MonsterTypeを作成
                var monsterType = ScriptableObject.CreateInstance<MonsterType>();
                
                // プライベートフィールドに値を設定（リフレクション使用）
                SetPrivateField(monsterType, "monsterTypeName", data.name);
                SetPrivateField(monsterType, "basicStatus", new BasicStatus(data.hp, data.atk, data.def, data.spd));
                SetPrivateField(monsterType, "weaknessTag", data.weak);
                SetPrivateField(monsterType, "strongnessTag", data.strong);

                // 基本スキルを作成
                var basicSkills = new System.Collections.Generic.List<Skill>();
                for (int j = 0; j < skillsPerSpecies; j++)
                {
                    var skill = CreateDevSkill($"{data.name}技{j+1}", 20 + j * 10, (SkillTag)(j % 4 + 1));
                    if (skill != null) basicSkills.Add(skill);
                }
                SetPrivateField(monsterType, "basicSkills", basicSkills);

                // MonsterManagerに追加（リフレクション使用）
                AddMonsterTypeToManager(monsterType);

                Debug.Log($"Created dev species: {data.name} (HP:{data.hp}, ATK:{data.atk})");
            }

            Debug.Log($"Added {speciesCount} dev species. Total: {manager.AllMonsterTypes.Count} (was {existingCount})");
        }

        /// <summary>
        /// 開発用スキルを作成
        /// </summary>
        private Skill CreateDevSkill(string name, int damage, SkillTag tag)
        {
            var skill = ScriptableObject.CreateInstance<Skill>();
            
            SetPrivateField(skill, "skillName", name);
            SetPrivateField(skill, "damage", damage);
            SetPrivateField(skill, "tag", tag);
            SetPrivateField(skill, "range", SkillRange.Single);
            SetPrivateField(skill, "shape", SkillShape.Point);
            SetPrivateField(skill, "description", $"Dev skill: {name}");

            return skill;
        }

        /// <summary>
        /// リフレクションでプライベートフィールドに値を設定
        /// </summary>
        private void SetPrivateField(object obj, string fieldName, object value)
        {
            try
            {
                var field = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field?.SetValue(obj, value);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Failed to set field {fieldName}: {ex.Message}");
            }
        }

        /// <summary>
        /// MonsterTypeをMonsterManagerに追加
        /// </summary>
        private void AddMonsterTypeToManager(MonsterType monsterType)
        {
            try
            {
                var manager = MonsterManager.Instance;
                var field = typeof(MonsterManager).GetField("allMonsterTypes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var list = field?.GetValue(manager) as System.Collections.Generic.List<MonsterType>;
                list?.Add(monsterType);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to add MonsterType to manager: {ex.Message}");
            }
        }

        /// <summary>
        /// サンプルモンスターを作成（各種族から1体ずつ）
        /// </summary>
        [ContextMenu("Create Sample Monsters")]
        public void CreateSampleMonsters()
        {
            if (MonsterManager.Instance == null)
            {
                Debug.LogError("MonsterManager not found");
                return;
            }

            var manager = MonsterManager.Instance;
            var allTypes = manager.AllMonsterTypes;

            Debug.Log($"Creating sample monsters from {allTypes.Count} species...");

            for (int i = 0; i < allTypes.Count && i < 5; i++) // 最大5体
            {
                var monsterType = allTypes[i];
                if (monsterType == null) continue;

                string sampleName = $"Sample{i+1}";
                int sampleLevel = Random.Range(3, 8);

                var monster = manager.CreateAndAddMonster(monsterType, sampleName, sampleLevel);
                if (monster != null)
                {
                    // ランダムでダメージを与える（状態の多様化）
                    if (Random.value < 0.3f) // 30%の確率
                    {
                        int damage = Random.Range(10, 30);
                        monster.TakeDamage(damage);
                    }

                    Debug.Log($"Created sample monster: {monster.NickName} (Lv.{monster.Level}, Type: {monsterType.MonsterTypeName})");
                }
            }

            Debug.Log("Sample monster creation completed");
        }
    }
}
