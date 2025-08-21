using System;
using UnityEngine;

[Serializable]
public enum SkillTag
{
    None,
    Fire,
    Water,
    Earth,
    Air,
    Light,
    Dark,
    Physical,
    Magical,
    Healing,
    Support,
    Debuff
}

[Serializable]
public enum SkillRange
{
    Self,
    Single,
    Adjacent,
    Cross,
    Line,
    Area,
    All
}

[Serializable]
public enum SkillShape
{
    Point,
    Line,
    Cross,
    Square,
    Circle,
    Cone
}

[CreateAssetMenu(fileName = "New Skill", menuName = "Game/Skill")]
public class Skill : ScriptableObject
{
    [SerializeField] private SkillTag tag;
    [SerializeField] private int damage;
    [SerializeField] private SkillRange range;
    [SerializeField] private SkillShape shape;
    [SerializeField] private string skillName;
    [SerializeField] private string description;

    public SkillTag Tag => tag;
    public int Damage => damage;
    public SkillRange Range => range;
    public SkillShape Shape => shape;
    public string SkillName => skillName;
    public string Description => description;
}
