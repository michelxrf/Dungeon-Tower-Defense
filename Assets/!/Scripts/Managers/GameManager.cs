using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private PlayerSave playerData;

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
        playerData = SaveSystem.LoadGame();
    }

    /// <summary>
    /// Saves and updates the player's progress when a wave ends
    /// </summary>
    /// <param name="level"></param>
    /// <param name="waveIndex"></param>
    /// <param name="money"></param>
    public void WaveEnded(LevelSO level, int waveIndex, int money)
    {
        playerData.currentLevel = level.levelName;
        playerData.currentWave = waveIndex;
        playerData.money = money;
        SaveSystem.SaveGame(playerData);
    }

    /// <summary>
    /// Saves resets and save game data when level ends
    /// </summary>
    public void LevelEnded(LevelSO level, int waveIndex, int money)
    {
        playerData.currentLevel = level.levelName;
        playerData.currentWave = 0; // Reset wave index for the next level
        playerData.knowledge = Mathf.FloorToInt(level.knowledgeMultiplier * waveIndex);
        playerData.money = 0;
        SaveSystem.SaveGame(playerData);
    }
}
