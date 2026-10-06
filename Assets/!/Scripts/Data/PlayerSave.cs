/// <summary>
/// PLayer's save data. This is used to save the player's progress in the game.
/// </summary>
[System.Serializable]
public class PlayerSave
{
    public int money;
    public int knowledge;
    public LevelName currentLevel;
    public int currentWave;
    public TroopData[] playerTroopsHand;
    public System.Collections.Generic.List<PowerUp> powerUps;
    public int souls;

    /// <summary>
    /// Loads the default PowerUp catalog from the ScriptableObjects
    /// in Assets/Resources/PowerUps. Used for new saves.
    /// IMPORTANT: Never call this from a constructor or field initializer -
    /// Resources.LoadAll is not allowed during serialization (JsonUtility or
    /// MonoBehaviour serialization will trigger this error). Call it only
    /// from runtime code paths (Awake/Start, SaveSystem, PowerUpManager).
    /// </summary>
    public static System.Collections.Generic.List<PowerUp> LoadDefaultPowerUps()
    {
        PowerUp[] fromResources = UnityEngine.Resources.LoadAll<PowerUp>("PowerUps");
        return new System.Collections.Generic.List<PowerUp>(fromResources);
    }

    public PlayerSave(int money, int knowledge, LevelName currentLevel, int currentWave, TroopData[] playerTroopsHand, System.Collections.Generic.List<PowerUp> powerUps, int souls)
    {
        this.money = money;
        this.knowledge = knowledge;
        this.currentLevel = currentLevel;
        this.currentWave = currentWave;
        this.playerTroopsHand = playerTroopsHand;
        // Keep this constructor serialization-safe: no Resources.Load calls here.
        this.powerUps = powerUps ?? new System.Collections.Generic.List<PowerUp>();
        this.souls = souls;
    }

    public PlayerSave()
    {
        money = 0;
        knowledge = 0;
        currentLevel = LevelName.NotInitialized;
        currentWave = 0;
        playerTroopsHand = new TroopData[0];
        // Keep this constructor serialization-safe: no Resources.Load calls here.
        // New-save catalog init happens in SaveSystem.LoadGame (runtime, not
        // during serialization). See PlayerSave.LoadDefaultPowerUps.
        powerUps = new System.Collections.Generic.List<PowerUp>();
        souls = 0;
    }
}