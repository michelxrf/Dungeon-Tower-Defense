using UnityEngine;

[CreateAssetMenu(fileName = "Player Troop", menuName = "New Player Defense")]
public class TroopSO : ScriptableObject
{
    public string displayName;
    public string description;
    public TroopSlotType troopSlotType;

    public GameObject prefab;
    public Sprite cardArt;

    public float health;
    public float visualRange;
    public int damage;
    public float attackInterval;
    public int cost;
}
