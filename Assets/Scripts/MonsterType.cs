using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New MonsterType", menuName = "Game/MonsterType")]
public class MonsterType : ScriptableObject
{
    [Header("基本情報")]
    [SerializeField] private string monsterTypeName;
    [SerializeField] private BasicStatus basicStatus;
    [SerializeField] private Sprite sprite;

    [Header("属性")]
    [SerializeField] private WeaknessTag weaknessTag;
    [SerializeField] private StrongnessTag strongnessTag;

    [Header("基本スキル")]
    [SerializeField] private List<Skill> basicSkills = new List<Skill>();

    // プロパティ
    public string MonsterTypeName => monsterTypeName;
    public BasicStatus BasicStatus => new BasicStatus(basicStatus); // コピーを返す
    public Sprite Sprite => sprite;
    public WeaknessTag WeaknessTag => weaknessTag;
    public StrongnessTag StrongnessTag => strongnessTag;
    public List<Skill> BasicSkills => new List<Skill>(basicSkills); // コピーを返す

    // バリデーション
    private void OnValidate()
    {
        if (basicStatus == null)
        {
            basicStatus = new BasicStatus(100, 10, 5, 10);
        }
    }

    // 弱点・強化チェック
    public bool IsWeakTo(SkillTag attackTag)
    {
        return (int)weaknessTag == (int)attackTag;
    }

    public bool IsStrongAgainst(SkillTag attackTag)
    {
        return (int)strongnessTag == (int)attackTag;
    }

    // ダメージ計算時の倍率を取得
    public float GetDamageMultiplier(SkillTag attackTag)
    {
        if (IsWeakTo(attackTag))
            return 1.5f; // 弱点は1.5倍
        else if (IsStrongAgainst(attackTag))
            return 0.5f; // 強化は0.5倍
        else
            return 1.0f; // 通常
    }
}
