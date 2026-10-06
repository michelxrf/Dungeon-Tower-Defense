using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Populates the PowerUp store (Viewport/Content) from the player save.
/// Attach to the Store GameObject in the Menus scene (Canvas/PowerUpsScreen/Store).
/// </summary>
public class PowerUpStore : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject powerUpPrefab;
    [SerializeField] private Transform contentRoot;

    private void Start()
    {
        ResolveReferences();
        PopulateStore();
    }

    private void ResolveReferences()
    {
        if (powerUpPrefab == null)
        {
            // Fallback: try loading the known prefab location.
            powerUpPrefab = Resources.Load<GameObject>("PowerUp");
        }

        if (contentRoot == null)
        {
            Transform found = transform.Find("Viewport/Content");
            if (found != null)
            {
                contentRoot = found;
            }
        }
    }

    private void PopulateStore()
    {
        if (powerUpPrefab == null)
        {
            Debug.LogWarning("PowerUpStore: powerUpPrefab is not assigned.", this);
            return;
        }

        if (contentRoot == null)
        {
            Debug.LogWarning("PowerUpStore: contentRoot (Viewport/Content) is not assigned.", this);
            return;
        }

        List<PowerUp> allPowerUps = GetAllPowerUps();
        Debug.Log("PowerUpStore: Found " + allPowerUps.Count + " PowerUps to display.", this);
        if (allPowerUps == null || allPowerUps.Count == 0)
        {
            Debug.LogWarning("PowerUpStore: no PowerUps found to display.", this);
            return;
        }

        // Clear placeholders / previous entries so we don't duplicate.
        for (int i = contentRoot.childCount - 1; i >= 0; i--)
        {
            Transform child = contentRoot.GetChild(i);
            if (child != null)
            {
                Destroy(child.gameObject);
            }
        }

        foreach (PowerUp powerUp in allPowerUps)
        {
            if (powerUp == null || powerUp.isAcquired)
            {
                continue;
            }

            GameObject entry = Instantiate(powerUpPrefab, contentRoot);
            PowerUpUI ui = entry.GetComponent<PowerUpUI>();
            if (ui == null)
            {
                Debug.LogWarning("PowerUpStore: PowerUp prefab is missing PowerUpUI.", entry);
                continue;
            }

            ui.Initialize(powerUp);
        }
    }

    /// <summary>
    /// Gets the list of all PowerUps via the SaveSystem.
    /// Falls back to the PowerUpManager / Resources when no save exists yet.
    /// </summary>
    private List<PowerUp> GetAllPowerUps()
    {
        PlayerSave save = SaveSystem.LoadGame();
        if (save != null && save.powerUps != null && save.powerUps.Count > 0)
        {
            return save.powerUps;
        }

        if (PowerUpManager.Instance != null && PowerUpManager.Instance.powerUps != null && PowerUpManager.Instance.powerUps.Count > 0)
        {
            return PowerUpManager.Instance.powerUps;
        }

        PowerUp[] fromResources = Resources.LoadAll<PowerUp>("PowerUps");
        if (fromResources != null && fromResources.Length > 0)
        {
            return new List<PowerUp>(fromResources);
        }

        return new List<PowerUp>();
    }
}
