using UnityEngine;

public class TroopSlot : MonoBehaviour
{
    [SerializeField] private GameObject _freeIndicator;
    [SerializeField] private TroopSlotType _troopSlotType;

    private bool _isOccupied = false;
    public bool IsOccupied => _isOccupied;

    public TroopSlotType GetTroopSlotType()
    {
        return _troopSlotType;
    }

    private TroopData _troopData = null;

    private void Start()
    {
        HideFreeSlot();
    }

    public void SetTroopData(TroopData troopData)
    {
        _troopData = troopData;
        _isOccupied = true;

        GameObject instantiatedTroop = Instantiate(_troopData.prefab, transform.position, Quaternion.identity, transform);
        instantiatedTroop.GetComponent<TroopSetup>().Setup(_troopData);
    }

    public void ClearTroopData()
    {
        _troopData = null;
        _isOccupied = false;

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void ShowFreeSlot()
    {
        if (_freeIndicator != null)
        {
            _freeIndicator.SetActive(_troopData == null);
        }
    }

    public void HideFreeSlot()
    {
        if (_freeIndicator != null)
        {
            _freeIndicator.SetActive(false);
        }
    }
}
