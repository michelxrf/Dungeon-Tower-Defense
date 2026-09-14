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

    public PlayerSave(int money, int knowledge, LevelName currentLevel, int currentWave, TroopData[] playerTroopsHand)
    {
        this.money = money;
        this.knowledge = knowledge;
        this.currentLevel = currentLevel;
        this.currentWave = currentWave;
        this.playerTroopsHand = playerTroopsHand;
    }

    public PlayerSave()
    {
        money = 0;
        knowledge = 0;
        currentLevel = LevelName.NotInitialized;
        currentWave = 0;
        playerTroopsHand = new TroopData[0];
    }
}