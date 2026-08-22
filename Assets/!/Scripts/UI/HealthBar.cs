using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Health _healthComponent;

    [SerializeField] private Image _fill;

    private void Start()
    {
        _healthComponent.OnHealthChanged += UpdateHealthBar;
    }

    private void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        _fill.fillAmount = currentHealth / maxHealth;
    }

    private void LateUpdate()
    {
        // prevent rotation of the health bar
        transform.rotation = Quaternion.Euler(0, 90f, 0);
    }
}
