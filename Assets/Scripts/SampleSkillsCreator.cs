using UnityEngine;

[CreateAssetMenu(fileName = "Sample Skills Creator", menuName = "Game/Sample Skills Creator")]
public class SampleSkillsCreator : ScriptableObject
{
    [ContextMenu("Create Sample Skills")]
    public void CreateSampleSkills()
    {
        // サンプルスキルデータを作成
        CreateSkill("ファイアボール", SkillTag.Fire, 30, SkillRange.Single, SkillShape.Point);
        CreateSkill("ウォータースラッシュ", SkillTag.Water, 25, SkillRange.Line, SkillShape.Line);
        CreateSkill("アースクエイク", SkillTag.Earth, 40, SkillRange.Area, SkillShape.Circle);
        CreateSkill("エアカッター", SkillTag.Air, 20, SkillRange.Cross, SkillShape.Cross);
        CreateSkill("ライトヒール", SkillTag.Light, -20, SkillRange.Self, SkillShape.Point);
        CreateSkill("ダークブラスト", SkillTag.Dark, 35, SkillRange.All, SkillShape.Circle);
        CreateSkill("メガパンチ", SkillTag.Physical, 45, SkillRange.Single, SkillShape.Point);
        CreateSkill("マジックミサイル", SkillTag.Magical, 28, SkillRange.Single, SkillShape.Point);
        
        Debug.Log("サンプルスキルを作成しました！Resources/Skillsフォルダを確認してください。");
    }
    
    private void CreateSkill(string skillName, SkillTag tag, int damage, SkillRange range, SkillShape shape)
    {
        var skill = CreateInstance<Skill>();
        skill.name = skillName;
        
        // Reflectionを使ってプライベートフィールドに値を設定
        var skillType = typeof(Skill);
        
        var tagField = skillType.GetField("tag", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var damageField = skillType.GetField("damage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var rangeField = skillType.GetField("range", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var shapeField = skillType.GetField("shape", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var skillNameField = skillType.GetField("skillName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var descriptionField = skillType.GetField("description", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        tagField?.SetValue(skill, tag);
        damageField?.SetValue(skill, damage);
        rangeField?.SetValue(skill, range);
        shapeField?.SetValue(skill, shape);
        skillNameField?.SetValue(skill, skillName);
        descriptionField?.SetValue(skill, $"{skillName}の説明");
        
#if UNITY_EDITOR
        string path = $"Assets/Resources/Skills/{skillName}.asset";
        UnityEditor.AssetDatabase.CreateAsset(skill, path);
        UnityEditor.AssetDatabase.SaveAssets();
#endif
    }
}
