using System;
using UnityEngine;

[Serializable]
public class TroopData
{
    public string displayName;
    public string description;

    public GameObject prefab;
    public Sprite cardArt;
    public int cost;
    public int health;
    public int damage;
    public float visualRange;
    public float attackInterval;
    public TroopSlotType troopSlotType;

    public int level;
    public int healthUpPerLevel;
    public int damageUpPerLevel;
    public float rangeUpPerLevel;
    public float attackIntervalDownPerLevel;

    public TroopData(TroopSO troopSO)
    {
        displayName = troopSO.displayName;
        description = troopSO.description;
        cost = troopSO.cost;
        prefab = troopSO.prefab;
        cardArt = troopSO.cardArt;
        health = troopSO.health;
        damage = troopSO.damage;
        visualRange = troopSO.visualRange;
        attackInterval = troopSO.attackInterval;
        troopSlotType = troopSO.troopSlotType;
        level = troopSO.level;
        healthUpPerLevel = troopSO.healthUpPerLevel;
        damageUpPerLevel = troopSO.damageUpPerLevel;
        rangeUpPerLevel = troopSO.rangeUpPerLevel;
        attackIntervalDownPerLevel = troopSO.attackIntervalDownPerLevel;
    }

    public void LevelUp()
    {
        health += healthUpPerLevel;
        damage += damageUpPerLevel;
        visualRange += rangeUpPerLevel;
        attackInterval = Mathf.Max(0.1f, attackInterval - attackIntervalDownPerLevel);
        level++;
    }
}
