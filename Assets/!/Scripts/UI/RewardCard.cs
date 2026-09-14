using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RewardCard : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image _cardArt;
    [SerializeField] private TMP_Text _cardName;
    [SerializeField] private TMP_Text _cost;

    private bool _selected = false;
    private TroopData _data;
    private TroopSO _troopSO;
    private CardRewardScreen _cardRewardScreen;

    public void SetupCard(TroopSO data, CardRewardScreen cardRewardScreen)
    {
        _cardRewardScreen = cardRewardScreen;
        _troopSO = data;

        if (data != null)
        {
            if (data.cardArt != null)
            {
                _cardArt.sprite = data.cardArt;
            }
            _cardName.text = data.displayName;
            _cost.text = _troopSO.cost.ToString();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_selected)
        {
            _cardRewardScreen.SelectReward(_data, gameObject);
        }
    }
}
