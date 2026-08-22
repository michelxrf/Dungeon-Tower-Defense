using System.Collections.Generic;
using UnityEngine;


public class Attack : MonoBehaviour
{
    [SerializeField] private int _damage = 10;
    [SerializeField] private float _attackFrequency = 1f;

    private List<Health> _targetsInRange = new List<Health>();
    private Collider _collider;

    private void Awake()
    {
        _collider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Health targetHealth = other.GetComponent<Health>();
        if (targetHealth != null && !_targetsInRange.Contains(targetHealth))
        {
            _targetsInRange.Add(targetHealth);
            Debug.Log($"Target entered range: {targetHealth.name}");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Health targetHealth = other.GetComponent<Health>();
        if (targetHealth != null && _targetsInRange.Contains(targetHealth))
        {
            _targetsInRange.Remove(targetHealth);
            Debug.Log($"Target left range: {targetHealth.name}");
        }
    }

    private void Start()
    {
        InvokeRepeating(nameof(PerformAttack), 0f, _attackFrequency);
    }

    private void PerformAttack()
    {
        if (_targetsInRange.Count > 0)
        {
            Health target = _targetsInRange[0];
            do
            {
                if(target ==  null)
                    _targetsInRange.RemoveAt(0);

            } while (_targetsInRange.Count > 0 && _targetsInRange[0] == null);

            if (target != null)
            {
                target.TakeDamage(_damage);
                Debug.Log($"Attacked {target.name} for {_damage} damage.");
            }
        }
    }
}
