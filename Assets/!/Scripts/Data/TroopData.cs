using UnityEngine;

public class TroopData
{
    public string displayName;
    public string description;

    public GameObject prefab;
    public Sprite cardArt;

    public float speed;
    public float health;
    public float damage;
    public float attackFrequency;

    public TroopData(TroopSO troopSO)
    {
        displayName = troopSO.displayName;
        description = troopSO.description;
        prefab = troopSO.prefab;
        cardArt = troopSO.cardArt;
        speed = troopSO.speed;
        health = troopSO.health;
        damage = troopSO.damage;
        attackFrequency = troopSO.attackFrequency;
    }
}
