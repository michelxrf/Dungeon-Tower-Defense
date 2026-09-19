using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private GameObject _floatingDamagePrefab;
    [SerializeField] private int _maxHealth = 10;
    
    private int _currentHealth = 10;

    public Action<float, float> OnHealthChanged;
    public Action OnDeath;

    [SerializeField] private AudioClip _hurtSound;
    [SerializeField] private AudioClip _deathSound;

    private void Awake()
    {
        _currentHealth = _maxHealth;
    }

    private void Start()
    {
        SetupStats();
        LevelManager.Instance.OnTroopLeveledUp += SetupStats;
    }

    private void SetupStats()
    {
        if (TryGetComponent(out TroopSetup troopSetup) && troopSetup.GetTroopData() != null)
        {
            _maxHealth = troopSetup.GetTroopData().health;
            _currentHealth = _maxHealth;
        }
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
        else
        {
            AudioManager.Instance.PlaySFX(_hurtSound);
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
        OnDeath?.Invoke();
        AudioManager.Instance.PlaySFX(_deathSound);
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        LevelManager.Instance.OnTroopLeveledUp -= SetupStats;
    }
}
