using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI representation of a single <see cref="PowerUp"/> inside the store.
/// Populated at runtime via Initialize — see PowerUpStore.
/// </summary>
public class PowerUpUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text powerUpNameText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private Button buyButton;

    private PowerUp powerUp;

    private void Awake()
    {
        ResolveReferences();
    }

    private void OnEnable()
    {
        SubscribeToManager();
        RefreshButtonState();
    }

    private void OnDisable()
    {
        UnsubscribeFromManager();
    }

    private void OnDestroy()
    {
        UnsubscribeFromManager();
    }

    /// <summary>
    /// Initializes the UI elements based on the PowerUp passed as a parameter.
    /// Sets up the button interactability (enough souls) and click behaviour (acquire).
    /// </summary>
    public void Initialize(PowerUp powerUpToDisplay)
    {
        powerUp = powerUpToDisplay;
        ResolveReferences();

        if (powerUp == null)
        {
            Debug.LogWarning("PowerUpUI.Initialize called with null PowerUp.", this);
            return;
        }

        if (powerUpNameText != null)
        {
            powerUpNameText.text = powerUp.powerUpName;
        }

        if (priceText != null)
        {
            priceText.text = powerUp.cost.ToString();
        }

        if (buyButton != null)
        {
            buyButton.onClick.RemoveListener(OnBuyButtonClicked);
            buyButton.onClick.AddListener(OnBuyButtonClicked);
        }

        SubscribeToManager();
        RefreshButtonState();
    }

    /// <summary>
    /// Checks the player current amount of souls and makes the button
    /// interactable only if the player has enough souls (and the power up
    /// is not already acquired). Re-evaluated every time
    /// PowerUpManager.OnPowerUpAcquired is invoked.
    /// </summary>
    public void RefreshButtonState()
    {
        if (buyButton == null || powerUp == null)
        {
            return;
        }

        if (powerUp.isAcquired)
        {
            buyButton.interactable = false;
            return;
        }

        PlayerSave save = SaveSystem.LoadGame();
        int souls = save != null ? save.souls : 0;
        buyButton.interactable = souls >= powerUp.cost;
    }

    private void OnBuyButtonClicked()
    {
        if (powerUp == null || powerUp.isAcquired)
        {
            return;
        }

        PlayerSave save = SaveSystem.LoadGame();
        if (save == null)
        {
            save = new PlayerSave();
        }

        if (save.souls < powerUp.cost)
        {
            RefreshButtonState();
            return;
        }

        bool acquired = TryAcquireThroughManager(powerUp);
        if (!acquired)
        {
            return;
        }

        // Reduce the power up cost from player souls and persist.
        save.souls = Mathf.Max(0, save.souls - powerUp.cost);

        // Keep the serialized save list in sync for the case where
        // PowerUpManager is absent (SaveGame syncs from the manager when present).
        if (save.powerUps != null)
        {
            PowerUp savedEntry = save.powerUps.Find(p => p != null && p.powerUpName == powerUp.powerUpName);
            if (savedEntry != null)
            {
                savedEntry.isAcquired = true;
            }
        }

        SaveSystem.SaveGame(save);

        // TryAcquire already fired OnPowerUpAcquired which refreshes every
        // PowerUpUI, but refresh locally too (covers the no-manager fallback).
        RefreshButtonState();
    }

    private bool TryAcquireThroughManager(PowerUp target)
    {
        if (PowerUpManager.Instance != null && PowerUpManager.Instance.powerUps != null)
        {
            // Reference coming from the save file is a deserialized copy, so it
            // may not be the managed instance. Resolve by name when needed.
            if (PowerUpManager.Instance.powerUps.Contains(target))
            {
                return PowerUpManager.Instance.TryAcquire(target);
            }

            PowerUp managed = PowerUpManager.Instance.powerUps.Find(p => p != null && p.powerUpName == target.powerUpName);
            if (managed != null)
            {
                bool acquiredManaged = PowerUpManager.Instance.TryAcquire(managed);
                if (acquiredManaged)
                {
                    target.isAcquired = true;
                }
                return acquiredManaged;
            }
        }

        // No manager (or power up not managed): acquire directly.
        return target.Acquire();
    }

    private void ResolveReferences()
    {
        if (powerUpNameText == null)
        {
            Transform nameTransform = transform.Find("PowerUpName");
            if (nameTransform != null)
            {
                powerUpNameText = nameTransform.GetComponent<TMP_Text>();
            }
        }

        if (priceText == null)
        {
            Transform priceTransform = transform.Find("Price/Price");
            if (priceTransform != null)
            {
                priceText = priceTransform.GetComponent<TMP_Text>();
            }
        }

        if (buyButton == null)
        {
            Transform buttonTransform = transform.Find("Button");
            if (buttonTransform != null)
            {
                buyButton = buttonTransform.GetComponent<Button>();
            }
            else
            {
                buyButton = GetComponentInChildren<Button>(true);
            }
        }
    }

    private void SubscribeToManager()
    {
        if (PowerUpManager.Instance != null)
        {
            PowerUpManager.Instance.OnPowerUpAcquired -= RefreshButtonState;
            PowerUpManager.Instance.OnPowerUpAcquired += RefreshButtonState;
        }
    }

    private void UnsubscribeFromManager()
    {
        if (PowerUpManager.Instance != null)
        {
            PowerUpManager.Instance.OnPowerUpAcquired -= RefreshButtonState;
        }
    }
}
