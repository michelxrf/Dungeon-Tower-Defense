using System;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Level Settings")]
    [SerializeField] private int _startingMoney;

    private int _money;

    public Action<int> OnMoneyChanged;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        _money = _startingMoney;
        OnMoneyChanged?.Invoke(_money);
    }

    public int GetCurrentMoney()
    {
        return _money;
    }

    public void AddMoney(int amount)
    {
        _money += amount;
        OnMoneyChanged?.Invoke(_money);
    }

    public void RemoveMoney(int amount)
    {
        _money -= amount;
        OnMoneyChanged?.Invoke(_money);
    }
}
