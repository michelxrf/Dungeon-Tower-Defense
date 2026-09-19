using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Level Settings")]
    [SerializeField] private int _startingMoney;
    [SerializeField] private LevelSO _levelData;

    [Header("Audio")]
    [SerializeField] private AudioClip _winSound;
    [SerializeField] private AudioClip _loseSound;
    [SerializeField] private AudioClip _waveCompleteSound;

    private int _currentWaveIndex = 0;
    private int _money;
    private bool _isPaused = true;
    private List<TroopData> _playerTroopsHand = new List<TroopData>();

    public bool IsPaused => _isPaused;

    public Action<int> OnMoneyChanged;
    public Action OnWaveEnded;
    public Action OnLevelCompleted;
    public Action OnLevelFailed;
    public Action OnHandChanged;
    public Action OnPause;
    public Action OnResume;
    public Action OnTroopLeveledUp;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        PlayerSave playerData = SaveSystem.LoadGame();

        if (playerData != null)
        {
            _money = playerData.money;
            _currentWaveIndex = playerData.currentWave;
            _playerTroopsHand = playerData.playerTroopsHand.ToList<TroopData>();
        }
        else
        { 
            _money = _startingMoney;
        }

        OnMoneyChanged?.Invoke(_money);
    }

    public void AddNewTroop(TroopData troop)
    {
        if (troop == null)
        {
            Debug.LogWarning("AddNewTroop called with null troop data.");
            return;
        }

        int troopIndex = _playerTroopsHand.FindIndex(t => t.displayName == troop.displayName);
        if (troopIndex != -1)
        {
            _playerTroopsHand[troopIndex].LevelUp();
            OnTroopLeveledUp?.Invoke();
        }
        else
        {
            _playerTroopsHand.Add(troop);
        }
        OnHandChanged?.Invoke();
    }

    public List<TroopData> GetPlayerTroopsHand()
    {
        return _playerTroopsHand;
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


    public GameObject GetMob(int index)
    {
        if (index < 0 || index >= _levelData.waves[_currentWaveIndex].mobs.Length)
        {
            Debug.LogWarning($"Index {index} is out of bounds for the current wave's mobs.");
            return null;
        }
        return _levelData.waves[_currentWaveIndex].mobs[index];
    }

    public int GetTotalWaves()
    {
        return _levelData.waves.Length;
    }

    public int GetCurrentWaveIndex()
    {
        return _currentWaveIndex;
    }

    public void WaveFinished()
    {
        if (_currentWaveIndex + 1 >= _levelData.waves.Length)
        {
            Debug.Log("Level completed!");
            AudioManager.Instance.PlaySFX(_winSound);
            GameManager.Instance.LevelEnded(_levelData, _currentWaveIndex, _money);
            FindAnyObjectByType<WinScreen>().Show();
            OnLevelCompleted?.Invoke();
        }
        else
        {
            Debug.Log($"Wave {_currentWaveIndex} completed. Preparing next wave.");
            _currentWaveIndex++;
            AudioManager.Instance.PlaySFX(_waveCompleteSound);
            GameManager.Instance.WaveEnded(_levelData, _currentWaveIndex, _money);
            OnWaveEnded?.Invoke();
        }

        PauseGame();
    }

    public void PauseGame()
    {
        _isPaused = true;
        OnPause?.Invoke();
    }

    public void Unpause()
    {
        _isPaused = false;
        OnResume?.Invoke();
    }

    public void GameOver()
    {
        Debug.Log("Game Over!");
        AudioManager.Instance.PlaySFX(_loseSound);
        GameManager.Instance.LevelEnded(_levelData, _currentWaveIndex, _money);
        FindAnyObjectByType<GameOverScreen>().Show();
        PauseGame();
        OnLevelFailed?.Invoke();
    }

    public float GetLevelSpeedMultiplier()
    {
        return 1 + (_levelData.speedMultiplierPerWave * _currentWaveIndex);
    }
}
