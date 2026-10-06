using UnityEngine;
using UnityEngine.UI;

public class MenuScreenSwitcher : MonoBehaviour
{
    [Header("Screens (CanvasGroup based)")]
    [SerializeField] private CanvasGroup levelSelectorScreen;
    [SerializeField] private CanvasGroup powerUpsScreen;

    [Header("Buttons")]
    [SerializeField] private Button levelSelectButton;
    [SerializeField] private Button powerUpButton;

    [Header("Defaults")]
    [SerializeField] private bool showLevelSelectorOnStart = true;

    private void Awake()
    {
        EnsureCanvasGroup(ref levelSelectorScreen, nameof(levelSelectorScreen));
        EnsureCanvasGroup(ref powerUpsScreen, nameof(powerUpsScreen));

        if (levelSelectButton != null)
        {
            levelSelectButton.onClick.RemoveListener(ShowLevelSelectorScreen);
            levelSelectButton.onClick.AddListener(ShowLevelSelectorScreen);
        }

        if (powerUpButton != null)
        {
            powerUpButton.onClick.RemoveListener(ShowPowerUpsScreen);
            powerUpButton.onClick.AddListener(ShowPowerUpsScreen);
        }
    }

    private void Start()
    {
        if (showLevelSelectorOnStart)
            ShowLevelSelectorScreen();
        else
            ShowPowerUpsScreen();
    }

    private void OnDestroy()
    {
        if (levelSelectButton != null)
            levelSelectButton.onClick.RemoveListener(ShowLevelSelectorScreen);

        if (powerUpButton != null)
            powerUpButton.onClick.RemoveListener(ShowPowerUpsScreen);
    }

    public void ShowLevelSelectorScreen()
    {
        Show(levelSelectorScreen);
        Hide(powerUpsScreen);
    }

    public void ShowPowerUpsScreen()
    {
        Show(powerUpsScreen);
        Hide(levelSelectorScreen);
    }

    private void Show(CanvasGroup canvasGroup)
    {
        if (canvasGroup == null) return;
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    private void Hide(CanvasGroup canvasGroup)
    {
        if (canvasGroup == null) return;
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private void EnsureCanvasGroup(ref CanvasGroup canvasGroup, string fieldName)
    {
        if (canvasGroup == null) return;

        // If a GameObject was assigned without a CanvasGroup, add one.
        if (canvasGroup.GetComponent<CanvasGroup>() == null)
        {
            Debug.LogWarning($"{fieldName} is missing a CanvasGroup, adding one.", this);
        }
    }
}
