using System;
using Unity.VisualScripting;
using UnityEngine;

public class CardRewardScreen : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int _numberOfCardsToDisplay = 3;

    [Header("References")]
    [SerializeField] private CardInGameSettings _cardInGameSettings;
    [SerializeField] private GameObject _cardPrefab;
    [SerializeField] private Transform _cardContainer;
    [SerializeField] private GameObject _confirmButton;
    [SerializeField] private CanvasGroup _canvasGroup;

    private TroopData _selectedReward;

    public Action OnRewardPicked;

    private void Start()
    {
        LevelManager.Instance.OnWaveEnded += ShowCardRewards;
        ShowCardRewards();
    }

    public void ShowCardRewards()
    {
        DisplayCardRewards();
        _canvasGroup.alpha = 1;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
        _confirmButton.SetActive(false);
    }

    public void HideCardRewards()
    {
        _canvasGroup.alpha = 0;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }

    public void ConfirmSelection()
    {
        // Logic to confirm the selected card and proceed
        _selectedReward = null;
        HideCardRewards();
        OnRewardPicked?.Invoke();
        LevelManager.Instance.AddNewTroop(_selectedReward);
        LevelManager.Instance.Unpause();
    }

    public void SelectReward(TroopData selectedReward, GameObject selectedGameObject)
    {
        _selectedReward = selectedReward;
        ClearSelection();

        _confirmButton.SetActive(true);
        LeanTween.scale(selectedGameObject, new Vector3(1.2f, 1.2f, 1.2f), 0.3f).setEase(LeanTweenType.easeOutBack);
    }

    private void ClearSelection()
    {
        _confirmButton.SetActive(false);
        foreach (Transform child in _cardContainer)
        {
            RewardCard rewardCard = child.GetComponent<RewardCard>();
            if (rewardCard != null)
            {
                LeanTween.scale(child.gameObject, new Vector3(1f, 1f, 1f), 0.3f).setEase(LeanTweenType.easeOutBack);
            }
        }
    }

    private void DisplayCardRewards()
    {
        // Clear existing cards
        foreach (Transform child in _cardContainer)
        {
            Destroy(child.gameObject);
        }

        Debug.Log($"Displaying card rewards {_numberOfCardsToDisplay}");
        // Display new cards
        for (int i = 0; i < _numberOfCardsToDisplay; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, _cardInGameSettings.Troops.Length);

            TroopSO troopSO = _cardInGameSettings.Troops[randomIndex];
            GameObject cardInstance = Instantiate(_cardPrefab, _cardContainer);
            RewardCard rewardCard = cardInstance.GetComponent<RewardCard>();
            rewardCard.SetupCard(troopSO, this);
        }
    }
}
