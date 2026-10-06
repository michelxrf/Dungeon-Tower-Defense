using System.Collections.Generic;
using UnityEngine;

public static class SaveSystem
{
    private static string saveFilePath = Application.persistentDataPath + "/savefile.json";

    public static void SaveGame(PlayerSave data)
    {
        if (data == null)
        {
            Debug.LogWarning("SaveGame called with null data, aborting save.");
            return;
        }

        SyncPowerUpsFromManager(data);

        string json = JsonUtility.ToJson(data, true);
        System.IO.File.WriteAllText(saveFilePath, json);
    }

    public static PlayerSave LoadGame()
    {
        if (System.IO.File.Exists(saveFilePath))
        {
            string json = System.IO.File.ReadAllText(saveFilePath);
            PlayerSave data = JsonUtility.FromJson<PlayerSave>(json);
            if (data == null)
            {
                Debug.LogWarning("Save file could not be parsed, creating a new save file.");
                data = new PlayerSave();
                // Runtime path (not during serialization), safe to load catalog.
                data.powerUps = PlayerSave.LoadDefaultPowerUps();
                SaveGame(data);
                return data;
            }

            // Backward compatibility with saves created before powerUps/souls existed.
            if (data.playerTroopsHand == null)
            {
                data.playerTroopsHand = new TroopData[0];
            }
            if (data.powerUps == null || data.powerUps.Count == 0)
            {
                data.powerUps = PlayerSave.LoadDefaultPowerUps();
            }
            // data.souls defaults to 0 when missing from JSON, no fix-up needed.

            ApplyPowerUpsToManager(data);
            return data;
        }
        else
        { 
            Debug.Log("Save file not found, creating a new save file.");
            PlayerSave newData = new PlayerSave();
            // Runtime path (not during serialization), safe to load catalog.
            // SyncPowerUpsFromManager (inside SaveGame) will prefer the
            // manager's list when it has entries, otherwise this stays.
            newData.powerUps = PlayerSave.LoadDefaultPowerUps();
            SaveGame(newData);
            return newData;
        }
    }

    /// <summary>
    /// Copies the current PowerUp list from the PowerUpManager into the save data,
    /// so SaveGame always persists the latest powerups (and their isAcquired states).
    /// Souls are stored directly on PlayerSave, so there is nothing to sync for them.
    /// </summary>
    private static void SyncPowerUpsFromManager(PlayerSave data)
    {
        if (PowerUpManager.Instance == null)
        {
            return;
        }

        if (PowerUpManager.Instance.powerUps == null || PowerUpManager.Instance.powerUps.Count == 0)
        {
            // Don't clobber a save that already has PowerUps (e.g. a freshly
            // created PlayerSave initialized from Resources) with an empty manager list.
            if (data.powerUps == null || data.powerUps.Count == 0)
            {
                data.powerUps = PlayerSave.LoadDefaultPowerUps();
            }
            return;
        }

        data.powerUps = new List<PowerUp>(PowerUpManager.Instance.powerUps);
    }

    /// <summary>
    /// Restores the saved powerup acquisition states onto the PowerUpManager's
    /// ScriptableObject instances (matched by powerUpName, since JSON cannot
    /// reliably round-trip UnityEngine.Object references across sessions).
    /// </summary>
    private static void ApplyPowerUpsToManager(PlayerSave data)
    {
        if (PowerUpManager.Instance == null || PowerUpManager.Instance.powerUps == null)
        {
            return;
        }

        if (data.powerUps == null || data.powerUps.Count == 0)
        {
            return;
        }

        foreach (PowerUp savedPowerUp in data.powerUps)
        {
            if (savedPowerUp == null || string.IsNullOrEmpty(savedPowerUp.powerUpName))
            {
                continue;
            }

            PowerUp managed = PowerUpManager.Instance.powerUps.Find(p => p != null && p.powerUpName == savedPowerUp.powerUpName);
            if (managed != null)
            {
                managed.isAcquired = savedPowerUp.isAcquired;
            }
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
