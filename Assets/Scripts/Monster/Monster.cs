using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Monster
{
    [Header("個体情報")]
    [SerializeField] private string nickName;
    [SerializeField] private Species monsterType;
    [SerializeField] private int level;

    [Header("習得スキル")]
    [SerializeField] private List<Skill> learnedSkills = new List<Skill>();

    [Header("現在のステータス")]
    [SerializeField] private int currentHP;
    [SerializeField] private bool isDead = false;

    // プロパティ
    public string NickName => nickName;
    public Species MonsterType => monsterType;
    public int Level => level;
    public List<Skill> LearnedSkills => new List<Skill>(learnedSkills); // コピーを返す
    public int CurrentHP => currentHP;
    public bool IsDead => isDead;

    // 計算されたステータス（レベル補正付き）
    public int MaxHP => CalculateMaxHP();
    public int ATK => CalculateATK();
    public int DEF => CalculateDEF();
    public int SPD => CalculateSPD();

    // コンストラクタ
    public Monster(Species type, string nickName = "", int level = 1)
    {
        this.monsterType = type;
        this.nickName = string.IsNullOrEmpty(nickName) ? type.SpeciesName : nickName;
        this.level = Mathf.Max(1, level);
        
        // 基本スキルを習得
    if (type != null && type.BasicSkills != null)
        {
            learnedSkills.AddRange(type.BasicSkills);
        }
        
        // HP初期化
        this.currentHP = MaxHP;
        this.isDead = false;
    }

    // レベル補正されたステータス計算
    private int CalculateMaxHP()
    {
        if (monsterType == null) return 1;
        float baseHP = monsterType.BasicStatus.MaxHP;
        return Mathf.RoundToInt(baseHP * (1.0f + (level - 1) * 0.1f));
    }

    private int CalculateATK()
    {
        if (monsterType == null) return 1;
        float baseATK = monsterType.BasicStatus.ATK;
        return Mathf.RoundToInt(baseATK * (1.0f + (level - 1) * 0.08f));
    }

    private int CalculateDEF()
    {
        if (monsterType == null) return 1;
        float baseDEF = monsterType.BasicStatus.DEF;
        return Mathf.RoundToInt(baseDEF * (1.0f + (level - 1) * 0.06f));
    }

    private int CalculateSPD()
    {
        if (monsterType == null) return 1;
        float baseSPD = monsterType.BasicStatus.SPD;
        return Mathf.RoundToInt(baseSPD * (1.0f + (level - 1) * 0.05f));
    }

    // スキル管理
    public bool LearnSkill(Skill skill)
    {
        if (skill == null || learnedSkills.Contains(skill))
            return false;
        
        learnedSkills.Add(skill);
        return true;
    }

    public bool ForgetSkill(Skill skill)
    {
        return learnedSkills.Remove(skill);
    }

    public bool HasSkill(Skill skill)
    {
        return learnedSkills.Contains(skill);
    }

    // HP管理
    public void TakeDamage(int damage)
    {
        currentHP = Mathf.Max(0, currentHP - damage);
        if (currentHP <= 0)
        {
            isDead = true;
        }
    }

    public void Heal(int healAmount)
    {
        if (isDead) return;
        
        currentHP = Mathf.Min(MaxHP, currentHP + healAmount);
    }

    public void FullHeal()
    {
        currentHP = MaxHP;
        isDead = false;
    }

    // レベルアップ
    public void LevelUp()
    {
        level++;
        // HPを最大値に合わせて調整（割合維持）
        float hpRatio = (float)currentHP / CalculateMaxHP();
        currentHP = Mathf.RoundToInt(MaxHP * hpRatio);
    }

    public void SetLevel(int newLevel)
    {
        if (newLevel < 1) return;
        
        float hpRatio = (float)currentHP / MaxHP;
        level = newLevel;
        currentHP = Mathf.RoundToInt(MaxHP * hpRatio);
    }

    // 弱点・強化チェック（MonsterTypeに委任）
    public bool IsWeakTo(SkillTag attackTag)
    {
        return monsterType?.IsWeakTo(attackTag) ?? false;
    }

    public bool IsStrongAgainst(SkillTag attackTag)
    {
        return monsterType?.IsStrongAgainst(attackTag) ?? false;
    }

    public float GetDamageMultiplier(SkillTag attackTag)
    {
        return monsterType?.GetDamageMultiplier(attackTag) ?? 1.0f;
    }

    // デバッグ用
    public override string ToString()
    {
        return $"{nickName} (Lv.{level}) HP:{currentHP}/{MaxHP} ATK:{ATK} DEF:{DEF} SPD:{SPD}";
    }
}
