using UnityEngine;

[CreateAssetMenu(fileName = "Player Troop", menuName = "New Player Defense")]
public class TroopSO : ScriptableObject
{
    public string displayName;
    public string description;
    public TroopSlotType troopSlotType;

    public GameObject prefab;
    public Sprite cardArt;

    public float speed;
    public float health;
    public float damage;
    public float attackFrequency;
    public int cost;
}
