using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private GameObject _floatingDamagePrefab;
    [SerializeField] private int _maxHealth = 10;
    
    private int _currentHealth = 10;

    public Action<float, float> OnHealthChanged;

    private void Awake()
    {
        _currentHealth = _maxHealth;
    }

    private void Start()
    {
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
    }

    public void TakeDamage(int damage)
    {
        _currentHealth -= damage;
        SpawnDamageIndicator(damage);
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    private void SpawnDamageIndicator(int damage)
    {
        GameObject damageIndicator = Instantiate(_floatingDamagePrefab);
        damageIndicator.GetComponent<RectTransform>().position = transform.position;
        damageIndicator.GetComponent<FloatingDamage>().Init(damage);

    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} has died.");
        Destroy(gameObject);
    }
}
