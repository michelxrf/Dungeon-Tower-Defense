using UnityEngine;

public class TroopSetup : MonoBehaviour
{
    [SerializeField] private TroopSO _troopSO;
    
    public TroopData GetTroopData()
    {
        if (_troopSO == null)
        {
            return null;
        }

        // OnValidate / edit mode runs before LevelManager.Awake(), so Instance can be null.
        // Fall back to the raw SO values so editor preview still works.
        var instance = LevelManager.Instance;
        if (instance == null)
        {
            return new TroopData(_troopSO);
        }

        var hand = instance.GetPlayerTroopsHand();
        if (hand == null)
        {
            return new TroopData(_troopSO);
        }

        var match = hand.Find(t => t != null && t.displayName == _troopSO.displayName);
        return match ?? new TroopData(_troopSO);
    }
}