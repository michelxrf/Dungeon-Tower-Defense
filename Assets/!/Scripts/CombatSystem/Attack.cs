using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TargetAquisition))]
public class Attack : MonoBehaviour
{
    private int _damage = 10;

    // Private for now, will be changed later (e.g. driven by stats).
    private float _cooldownTime = 1f;

    private TargetAquisition _targetAquisition;
    private float _cooldownTimer;
    private bool _isOnCooldown;

    private void Awake()
    {
        _targetAquisition = GetComponent<TargetAquisition>();
    }

    private void Start()
    {
        SyncDamageFromSetup();
        TryPerformAttack();
    }

    private void OnDisable()
    {
        UnsubscribeFromTargetAcquired();
    }

    private void OnDestroy()
    {
        UnsubscribeFromTargetAcquired();
    }

    private void Update()
    {
        if (LevelManager.Instance != null && LevelManager.Instance.IsPaused) return;

        if (!_isOnCooldown) return;

        _cooldownTimer -= Time.deltaTime;
        if (_cooldownTimer <= 0f)
        {
            _isOnCooldown = false;
            TryPerformAttack();
        }
    }

    private void TryPerformAttack()
    {
        if (_isOnCooldown) return;
        if (LevelManager.Instance != null && LevelManager.Instance.IsPaused) return;
        if (_targetAquisition == null) return;

        Health target = GetClosestTarget();
        if (target != null)
        {
            PerformAttack(target);
        }
        else
        {
            // No target available: wait for TargetAquisition to acquire one,
            // then attempt to attack immediately.
            SubscribeToTargetAcquired();
        }
    }

    private void PerformAttack(Health target)
    {
        RotateTowardTarget(target.transform);
        target.TakeDamage(_damage);
        Debug.Log($"Attacked {target.name} for {_damage} damage.");

        StartCooldown();
    }

    private void RotateTowardTarget(Transform targetTransform)
    {
        if (targetTransform == null) return;

        Vector3 direction = targetTransform.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f) return;

        transform.rotation = Quaternion.LookRotation(direction);
    }

    private void StartCooldown()
    {
        // Unsubscribe as soon as the cooldown starts.
        UnsubscribeFromTargetAcquired();

        _isOnCooldown = true;
        _cooldownTimer = _cooldownTime;
    }

    private Health GetClosestTarget()
    {
        List<GameObject> targets = _targetAquisition.GetTargetsInRange();
        if (targets == null || targets.Count == 0) return null;

        Health closest = null;
        float closestSqrDistance = float.MaxValue;
        Vector3 selfPosition = transform.position;

        foreach (GameObject targetObject in targets)
        {
            if (targetObject == null) continue;
            if (!targetObject.TryGetComponent(out Health health)) continue;

            float sqrDistance = (targetObject.transform.position - selfPosition).sqrMagnitude;
            if (sqrDistance < closestSqrDistance)
            {
                closestSqrDistance = sqrDistance;
                closest = health;
            }
        }

        return closest;
    }

    private void SubscribeToTargetAcquired()
    {
        // Guard against double subscription.
        _targetAquisition.OnTargetsAcquired -= HandleTargetAcquired;
        _targetAquisition.OnTargetsAcquired += HandleTargetAcquired;
    }

    private void UnsubscribeFromTargetAcquired()
    {
        if (_targetAquisition == null) return;
        _targetAquisition.OnTargetsAcquired -= HandleTargetAcquired;
    }

    private void HandleTargetAcquired()
    {
        TryPerformAttack();
    }

    private void SyncDamageFromSetup()
    {
        if (TryGetComponent(out TroopSetup troopSetup) && troopSetup.TroopSO != null)
        {
            _damage = troopSetup.TroopSO.damage;
        }
    }
}
