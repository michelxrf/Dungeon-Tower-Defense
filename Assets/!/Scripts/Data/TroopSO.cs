using UnityEngine;

[CreateAssetMenu(fileName = "TroopData", menuName = "New Troop Data")]
public class TroopSO : ScriptableObject
{
    public string displayName;
    public string description;

    public GameObject prefab;
    public Sprite cardArt;

    public float speed;
    public float health;
    public float damage;
    public float attackFrequency;
    public int cost;
}
