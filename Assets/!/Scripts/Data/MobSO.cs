using UnityEngine;

[CreateAssetMenu(fileName = "Mob Data", menuName = "New Mob Data")]
public class MobSO : ScriptableObject
{
    public string displayName;
    public string description;
    public GameObject prefab;
    public float Health;
    public float Damage;
    public float AttackFrequency;
    public bool IsRanged;
    public float Range;
}
