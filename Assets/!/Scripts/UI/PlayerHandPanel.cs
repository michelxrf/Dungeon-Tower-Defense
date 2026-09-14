using UnityEngine;

public class PlayerHandPanel : MonoBehaviour
{
    [SerializeField] private Transform _cardsContainer;
    [SerializeField] private GameObject _troopCardPrefab;


    private void Start()
    {
        LevelManager.Instance.OnHandChanged += UpdateHand;
        UpdateHand();
    }

    private void OnDestroy()
    {
        LevelManager.Instance.OnHandChanged -= UpdateHand;
    }

    private void UpdateHand()
    {
        foreach (Transform child in _cardsContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (TroopData troop in LevelManager.Instance.GetPlayerTroopsHand())
        {
            GameObject cardGO = Instantiate(_troopCardPrefab, _cardsContainer);
            TroopCard troopCard = cardGO.GetComponent<TroopCard>();
            troopCard.SetupCard(troop);
        }
    }

}
