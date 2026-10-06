using System;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpManager : MonoBehaviour
{
    public static PowerUpManager Instance { get; private set; }

    public List<PowerUp> powerUps = new List<PowerUp>();

    public Action OnPowerUpAcquired;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        SetPowerUpsFromSave();
    }

    /// <summary>
    /// Sets the manager's powerUps list from the SaveSystem load data.
    /// Entries are re-linked to the ScriptableObjects in Resources/PowerUps
    /// (matched by powerUpName) so prerequisites stay intact, and the saved
    /// isAcquired states are applied. Falls back to the Resources catalog
    /// when the save has no PowerUps.
    /// </summary>
    public void SetPowerUpsFromSave()
    {
        PlayerSave save = SaveSystem.LoadGame();
        if (save != null && save.powerUps != null && save.powerUps.Count > 0)
        {
            PowerUp[] catalog = Resources.LoadAll<PowerUp>("PowerUps");
            List<PowerUp> resolved = new List<PowerUp>(save.powerUps.Count);

            foreach (PowerUp savedPowerUp in save.powerUps)
            {
                if (savedPowerUp == null || string.IsNullOrEmpty(savedPowerUp.powerUpName))
                {
                    continue;
                }

                PowerUp catalogEntry = null;
                foreach (PowerUp candidate in catalog)
                {
                    if (candidate != null && candidate.powerUpName == savedPowerUp.powerUpName)
                    {
                        catalogEntry = candidate;
                        break;
                    }
                }

                if (catalogEntry != null)
                {
                    catalogEntry.isAcquired = savedPowerUp.isAcquired;
                    resolved.Add(catalogEntry);
                }
                else
                {
                    // PowerUp no longer in Resources (or a deserialized copy):
                    // keep the save entry so no data is lost.
                    resolved.Add(savedPowerUp);
                }
            }

            // Include any new catalog PowerUps added after the save was created.
            foreach (PowerUp candidate in catalog)
            {
                if (candidate == null || string.IsNullOrEmpty(candidate.powerUpName))
                {
                    continue;
                }

                bool alreadyIncluded = resolved.Exists(p => p != null && p.powerUpName == candidate.powerUpName);
                if (!alreadyIncluded)
                {
                    resolved.Add(candidate);
                }
            }

            powerUps = resolved;
            return;
        }

        if (powerUps == null || powerUps.Count == 0)
        {
            powerUps = new List<PowerUp>(Resources.LoadAll<PowerUp>("PowerUps"));
        }
    }

    /// <summary>
    /// Tries to acquire a PowerUp by reference.
    /// Returns true on success, false otherwise.
    /// </summary>
    public bool TryAcquire(PowerUp powerUp)
    {
        if (powerUp == null)
        {
            Debug.LogWarning("TryAcquire called with null PowerUp.");
            return false;
        }

        if (!powerUps.Contains(powerUp))
        {
            Debug.LogWarning($"PowerUp '{powerUp.powerUpName}' is not managed by PowerUpManager.");
            return false;
        }

        bool acquired = powerUp.Acquire();
        if (acquired)
        {
            OnPowerUpAcquired?.Invoke();
        }
        return acquired;
    }

    /// <summary>
    /// Tries to acquire a PowerUp by name.
    /// Returns true on success, false otherwise.
    /// </summary>
    public bool TryAcquire(string powerUpName)
    {
        PowerUp powerUp = powerUps.Find(p => p != null && p.powerUpName == powerUpName);
        if (powerUp == null)
        {
            Debug.LogWarning($"PowerUp '{powerUpName}' not found.");
            return false;
        }

        bool acquired = powerUp.Acquire();
        if (acquired)
        {
            OnPowerUpAcquired?.Invoke();
        }
        return acquired;
    }

    public List<PowerUp> GetAcquiredPowerUps()
    {
        return powerUps.FindAll(p => p != null && p.isAcquired);
    }

    public List<PowerUp> GetAvailablePowerUps()
    {
        return powerUps.FindAll(p => p != null && !p.isAcquired && p.CanAcquire());
    }
}
