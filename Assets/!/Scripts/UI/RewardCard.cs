using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RewardCard : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image _cardArt;
    [SerializeField] private TMP_Text _cardName;
    [SerializeField] private TMP_Text _cost;
    [SerializeField] private TMP_Text _level;

    private bool _selected = false;
    private TroopData _data;
    private TroopSO _troopSO;
    private CardRewardScreen _cardRewardScreen;

    public void SetupCard(TroopSO data, CardRewardScreen cardRewardScreen)
    {
        _cardRewardScreen = cardRewardScreen;
        _troopSO = data;
        _data = data != null ? new TroopData(data) : null;

        if (data != null)
        {
            if (data.cardArt != null)
            {
                _cardArt.sprite = _data.cardArt;
            }
            _cardName.text = _data.displayName;
            _cost.text = _data.cost.ToString();

            if(LevelManager.Instance.GetPlayerTroopsHand().Exists(t => t.displayName == _data.displayName))
            {
                _level.text = "Lv: " + (LevelManager.Instance.GetPlayerTroopsHand().Find(t => t.displayName == _data.displayName).level + 1).ToString();
            }
            else
            {
                _level.text = "Lv: 1";
            }
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
