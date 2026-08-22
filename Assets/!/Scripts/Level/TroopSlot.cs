using UnityEngine;

public class TroopSlot : MonoBehaviour
{
    [SerializeField] private GameObject _freeIndicator;

    private TroopData _troopData = null;

    private void Start()
    {
        HideFreeSlot();
    }

    public void SetTroopData(TroopData troopData)
    {
        _troopData = troopData;

        GameObject instantiatedTroop = Instantiate(_troopData.prefab, transform.position, Quaternion.identity, transform);
        instantiatedTroop.GetComponent<TroopSetup>().Setup(_troopData);
    }

    public void ClearTroopData()
    {
        _troopData = null;
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
