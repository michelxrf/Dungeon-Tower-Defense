using System;
using UnityEngine;

[Serializable]
public class TroopData
{
    public string displayName;
    public string description;

    public GameObject prefab;
    public Sprite cardArt;

    public float health;
    public float damage;
    public float visualRange;
    public float attackInterval;
    public TroopSlotType troopSlotType;

    public TroopData(TroopSO troopSO)
    {
        displayName = troopSO.displayName;
        description = troopSO.description;
        prefab = troopSO.prefab;
        cardArt = troopSO.cardArt;
        health = troopSO.health;
        damage = troopSO.damage;
        visualRange = troopSO.visualRange;
        attackInterval = troopSO.attackInterval;
        troopSlotType = troopSO.troopSlotType;
    }
}
