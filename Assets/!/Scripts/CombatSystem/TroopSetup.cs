using UnityEngine;

public class TroopSetup : MonoBehaviour
{
    [SerializeField] private TroopSO _troopSO;

    public TroopSO TroopSO => _troopSO;
}
