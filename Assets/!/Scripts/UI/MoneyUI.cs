using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class MoneyUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _money;

    private void Start()
    {
        if(LevelManager.Instance != null )
        {
            UpdateMoney(LevelManager.Instance.GetCurrentMoney());
            LevelManager.Instance.OnMoneyChanged += UpdateMoney;
        }
    }

    public void UpdateMoney(int money)
    {
        _money.text = money.ToString();
    }

    private void OnDestroy()
    {
        if (LevelManager.Instance != null)
            LevelManager.Instance.OnMoneyChanged += UpdateMoney;
    }
}
