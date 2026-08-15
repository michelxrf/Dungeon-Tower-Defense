using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private int _maxHealth = 10;
    [SerializeField] private GameObject _floatingDamagePrefab;

    public void TakeDamage(int damage)
    {
        _maxHealth -= damage;

        if (_maxHealth <= 0)
        {
            Die();
        }
    }

    private void SpawnDamageIndicator(int damage)
    {
        GameObject damageIndicator = Instantiate(_floatingDamagePrefab, transform.position + Vector3.up, Quaternion.identity);
        damageIndicator.GetComponent<FloatingDamage>().Init(damage);

    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} has died.");
        Destroy(gameObject);
    }
}
