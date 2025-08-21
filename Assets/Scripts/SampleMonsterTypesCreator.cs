using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Sample MonsterTypes Creator", menuName = "Game/Sample MonsterTypes Creator")]
public class SampleMonsterTypesCreator : ScriptableObject
{
    [ContextMenu("Create Sample MonsterTypes")]
    public void CreateSampleMonsterTypes()
    {
        // サンプルモンスタータイプを作成
        CreateMonsterType("スライム", new BasicStatus(80, 15, 8, 12), WeaknessTag.Fire, StrongnessTag.Physical, new string[] {"ファイアボール", "ライトヒール"});
        CreateMonsterType("ドラゴン", new BasicStatus(150, 35, 20, 8), WeaknessTag.Water, StrongnessTag.Fire, new string[] {"ファイアボール", "メガパンチ"});
        CreateMonsterType("フェニックス", new BasicStatus(120, 30, 15, 18), WeaknessTag.Water, StrongnessTag.Fire, new string[] {"ファイアボール", "ライトヒール"});
        CreateMonsterType("ゴブリン", new BasicStatus(60, 20, 10, 15), WeaknessTag.Light, StrongnessTag.Dark, new string[] {"メガパンチ", "エアカッター"});
        CreateMonsterType("アイスエレメンタル", new BasicStatus(100, 25, 18, 10), WeaknessTag.Fire, StrongnessTag.Water, new string[] {"ウォータースラッシュ", "アースクエイク"});
        
        Debug.Log("サンプルモンスタータイプを作成しました！Resources/MonsterTypesフォルダを確認してください。");
    }
    
    private void CreateMonsterType(string name, BasicStatus status, WeaknessTag weakness, StrongnessTag strongness, string[] skillNames)
    {
        var monsterType = CreateInstance<MonsterType>();
        monsterType.name = name;
        
        // Reflectionを使ってプライベートフィールドに値を設定
        var monsterTypeType = typeof(MonsterType);
        
        var nameField = monsterTypeType.GetField("monsterTypeName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var statusField = monsterTypeType.GetField("basicStatus", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var weaknessField = monsterTypeType.GetField("weaknessTag", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var strongnessField = monsterTypeType.GetField("strongnessTag", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var skillsField = monsterTypeType.GetField("basicSkills", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        nameField?.SetValue(monsterType, name);
        statusField?.SetValue(monsterType, status);
        weaknessField?.SetValue(monsterType, weakness);
        strongnessField?.SetValue(monsterType, strongness);
        
        // スキルを読み込んで設定
        var skills = new List<Skill>();
        foreach (string skillName in skillNames)
        {
            var skill = Resources.Load<Skill>($"Skills/{skillName}");
            if (skill != null)
            {
                skills.Add(skill);
            }
        }
        skillsField?.SetValue(monsterType, skills);
        
#if UNITY_EDITOR
        string path = $"Assets/Resources/MonsterTypes/{name}.asset";
        UnityEditor.AssetDatabase.CreateAsset(monsterType, path);
        UnityEditor.AssetDatabase.SaveAssets();
#endif
    }
}
