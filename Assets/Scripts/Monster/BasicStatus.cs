using System;
using UnityEngine;

[Serializable]
public class BasicStatus
{
    [SerializeField] private int maxHP;
    [SerializeField] private int atk;
    [SerializeField] private int def;
    [SerializeField] private int spd;

    public int MaxHP => maxHP;
    public int ATK => atk;
    public int DEF => def;
    public int SPD => spd;

    public BasicStatus(int maxHP, int atk, int def, int spd)
    {
        this.maxHP = maxHP;
        this.atk = atk;
        this.def = def;
        this.spd = spd;
    }

    public BasicStatus(BasicStatus other)
    {
        this.maxHP = other.maxHP;
        this.atk = other.atk;
        this.def = other.def;
        this.spd = other.spd;
    }
}
