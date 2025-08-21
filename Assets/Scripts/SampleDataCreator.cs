using UnityEngine;

public class SampleDataCreator : MonoBehaviour
{
    [Header("サンプルデータ作成")]
    [SerializeField] private bool createSampleMonstersOnStart = true;
    [SerializeField] private int sampleMonsterCount = 5;

    private void Start()
    {
        if (createSampleMonstersOnStart)
        {
            // MonsterTypeが無い場合は動的生成してからモンスター作成
            if (MonsterManager.Instance.AllMonsterTypes.Count == 0)
            {
                Debug.Log("No MonsterTypes found, creating temporary ones...");
                CreateTemporaryMonsterTypes();
            }
            CreateSampleMonsters();
        }
    }

    private void CreateTemporaryMonsterTypes()
    {
        Debug.Log("=== Creating Temporary MonsterTypes ===");
        
        // 基本スキルを作成
        var basicSkill = CreateBasicSkill();
        
        // 複数のモンスタータイプを作成
        var slimeType = CreateBasicMonsterType("Slime", basicSkill, new BasicStatus(80, 15, 8, 12));
        var dragonType = CreateBasicMonsterType("Dragon", basicSkill, new BasicStatus(150, 35, 20, 8));
        var goblinType = CreateBasicMonsterType("Goblin", basicSkill, new BasicStatus(60, 20, 10, 15));
        var elementalType = CreateBasicMonsterType("Elemental", basicSkill, new BasicStatus(100, 25, 18, 10));
        var phoenixType = CreateBasicMonsterType("Phoenix", basicSkill, new BasicStatus(120, 30, 15, 18));
        
        // MonsterManagerに直接追加
        var manager = MonsterManager.Instance;
        var allTypesField = typeof(MonsterManager).GetField("allMonsterTypes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var typesList = (System.Collections.Generic.List<MonsterType>)allTypesField.GetValue(manager);
        
        typesList.Clear();
        typesList.Add(slimeType);
        typesList.Add(dragonType);
        typesList.Add(goblinType);
        typesList.Add(elementalType);
        typesList.Add(phoenixType);
        
        Debug.Log($"Created {typesList.Count} temporary MonsterTypes");
        Debug.Log("=== End Creating Temporary MonsterTypes ===");
    }
    
    private Skill CreateBasicSkill()
    {
        var skill = ScriptableObject.CreateInstance<Skill>();
        
        // Reflectionでプライベートフィールドに値を設定
        var skillType = typeof(Skill);
        var skillNameField = skillType.GetField("skillName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var tagField = skillType.GetField("tag", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var damageField = skillType.GetField("damage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var rangeField = skillType.GetField("range", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var shapeField = skillType.GetField("shape", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var descriptionField = skillType.GetField("description", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        skillNameField?.SetValue(skill, "Basic Attack");
        tagField?.SetValue(skill, SkillTag.Physical);
        damageField?.SetValue(skill, 20);
        rangeField?.SetValue(skill, SkillRange.Single);
        shapeField?.SetValue(skill, SkillShape.Point);
        descriptionField?.SetValue(skill, "A basic physical attack");
        
        return skill;
    }
    
    private MonsterType CreateBasicMonsterType(string name, Skill basicSkill, BasicStatus status)
    {
        var monsterType = ScriptableObject.CreateInstance<MonsterType>();
        
        // Reflectionでプライベートフィールドに値を設定
        var monsterTypeType = typeof(MonsterType);
        var nameField = monsterTypeType.GetField("monsterTypeName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var statusField = monsterTypeType.GetField("basicStatus", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var weaknessField = monsterTypeType.GetField("weaknessTag", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var strongnessField = monsterTypeType.GetField("strongnessTag", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var skillsField = monsterTypeType.GetField("basicSkills", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        nameField?.SetValue(monsterType, name);
        statusField?.SetValue(monsterType, status);
        weaknessField?.SetValue(monsterType, WeaknessTag.Fire);
        strongnessField?.SetValue(monsterType, StrongnessTag.Physical);
        skillsField?.SetValue(monsterType, new System.Collections.Generic.List<Skill> { basicSkill });
        
        return monsterType;
    }

    [ContextMenu("Create Sample Monsters")]
    public void CreateSampleMonsters()
    {
        Debug.Log("=== SampleDataCreator Debug ===");
        
        var manager = MonsterManager.Instance;
        if (manager == null)
        {
            Debug.LogError("MonsterManager.Instance is NULL!");
            return;
        }
        
        Debug.Log($"MonsterManager found. Available MonsterTypes: {manager.AllMonsterTypes.Count}");
        
        // サンプルモンスターの名前リスト
        string[] sampleNames = {
            "Pika", "Slimey", "Dragon-chan", "Fenny", "Goblin",
            "Ice-chan", "Fire", "Thunder", "Earth", "Windy"
        };

        int successCount = 0;
        for (int i = 0; i < sampleMonsterCount; i++)
        {
            Debug.Log($"--- Creating monster {i + 1}/{sampleMonsterCount} ---");
            
            var randomType = manager.GetRandomMonsterType();
            if (randomType != null)
            {
                string nickname = sampleNames[Random.Range(0, sampleNames.Length)];
                int level = Random.Range(1, 11); // レベル1-10
                
                Debug.Log($"Attempting to create: {nickname} (Type: {randomType.name}, Level: {level})");
                
                var monster = manager.CreateAndAddMonster(randomType, nickname, level);
                
                if (monster != null)
                {
                    successCount++;
                    
                    // ランダムでダメージを与える（デモ用）
                    if (Random.Range(0f, 1f) < 0.3f) // 30%の確率でダメージ
                    {
                        int damage = Random.Range(10, 50);
                        monster.TakeDamage(damage);
                        Debug.Log($"Applied {damage} damage to {monster.NickName}");
                    }
                }
                else
                {
                    Debug.LogError($"Failed to create monster {nickname}");
                }
            }
            else
            {
                Debug.LogError($"No MonsterType available for monster {i + 1}");
            }
        }

        Debug.Log($"Successfully created {successCount}/{sampleMonsterCount} sample monsters!");
        Debug.Log($"Total monsters in party: {manager.PlayerMonsters.Count}");
        Debug.Log("=== End SampleDataCreator Debug ===");
    }

    [ContextMenu("Clear All Monsters")]
    public void ClearAllMonsters()
    {
        // PlayerMonstersのコピーを作成してから削除
        var allMonsters = new System.Collections.Generic.List<Monster>(MonsterManager.Instance.PlayerMonsters);
        
        foreach (var monster in allMonsters)
        {
            MonsterManager.Instance.RemoveMonster(monster);
        }
        
        Debug.Log("全てのモンスターを削除しました。");
    }
}
