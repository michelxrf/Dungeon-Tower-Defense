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

    public PlayerSave(int money, int knowledge, LevelName currentLevel, int currentWave)
    {
        this.money = money;
        this.knowledge = knowledge;
        this.currentLevel = currentLevel;
        this.currentWave = currentWave;
    }

    public PlayerSave()
    {
        money = 0;
        knowledge = 0;
        currentLevel = LevelName.NotInitialized;
        currentWave = 0;
    }
}