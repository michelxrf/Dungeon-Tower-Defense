using UnityEngine;

public static class SaveSystem
{
    private static string saveFilePath = Application.persistentDataPath + "/savefile.json";

    public static void SaveGame(PlayerSave data)
    {
        string json = JsonUtility.ToJson(data);
        System.IO.File.WriteAllText(saveFilePath, json);
    }

    public static PlayerSave LoadGame()
    {
        if (System.IO.File.Exists(saveFilePath))
        {
            string json = System.IO.File.ReadAllText(saveFilePath);
            PlayerSave data = JsonUtility.FromJson<PlayerSave>(json);
            return data;
        }
        else
        { 
            Debug.LogWarning("Save file not found, returning null.");
            return null;
        }
    }

    public static void DeleteSave()
    {
        if (System.IO.File.Exists(saveFilePath))
        {
            System.IO.File.Delete(saveFilePath);
            Debug.Log("Save file deleted.");
        }
        else
        {
            Debug.LogWarning("No save file to delete.");
        }
    }
}
