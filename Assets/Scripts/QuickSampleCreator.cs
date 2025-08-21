using UnityEngine;

public class QuickSampleCreator : MonoBehaviour
{
    [ContextMenu("Create Quick Sample Data")]
    public void CreateQuickSampleData()
    {
        // 基本スキルを作成
        var basicSkill = CreateBasicSkill();
        
        // モンスタータイプを作成
        var slimeType = CreateBasicMonsterType("Slime", basicSkill);
        var dragonType = CreateBasicMonsterType("Dragon", basicSkill);
        
        // MonsterManagerに手動でモンスターを追加
        var manager = MonsterManager.Instance;
        
        // サンプルモンスター5体を作成
        manager.CreateAndAddMonster(slimeType, "Slimey", 3);
        manager.CreateAndAddMonster(dragonType, "Drago", 5);
        manager.CreateAndAddMonster(slimeType, "Bouncy", 2);
        manager.CreateAndAddMonster(dragonType, "Flame", 7);
        manager.CreateAndAddMonster(slimeType, "Gooey", 4);
        
        Debug.Log("Quick sample monsters created!");
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
        
        skillNameField?.SetValue(skill, "Basic Attack");
        tagField?.SetValue(skill, SkillTag.Physical);
        damageField?.SetValue(skill, 20);
        rangeField?.SetValue(skill, SkillRange.Single);
        shapeField?.SetValue(skill, SkillShape.Point);
        
        return skill;
    }
    
    private MonsterType CreateBasicMonsterType(string name, Skill basicSkill)
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
        statusField?.SetValue(monsterType, new BasicStatus(100, 15, 8, 12));
        weaknessField?.SetValue(monsterType, WeaknessTag.Fire);
        strongnessField?.SetValue(monsterType, StrongnessTag.Physical);
        skillsField?.SetValue(monsterType, new System.Collections.Generic.List<Skill> { basicSkill });
        
        return monsterType;
    }
}
