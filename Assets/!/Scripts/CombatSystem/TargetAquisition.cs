using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ShowDetection))]
public class TargetAquisition : MonoBehaviour
{
    public Action OnTargetsAcquired;

    [SerializeField] private TroopSetup _troopSetup;
    [SerializeField] private float _range = 10f;
    [SerializeField] private float _queryInterval = 0.25f;
    [SerializeField] private LayerMask _targetLayerMask;

    private readonly List<GameObject> _targetsInRange = new();
    private float _queryTimer;

    private void Start()
    {
        SetupStats();
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.OnTroopLeveledUp += SetupStats;
        }
    }

    private void OnValidate()
    {
        SetupStats();
    }

    private void OnDestroy()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.OnTroopLeveledUp -= SetupStats;
        }
    }

    private void SetupStats()
    {
        if (_troopSetup != null)
        {
            TroopData troopData = _troopSetup.GetTroopData();
            if (troopData != null)
            {
                _range = troopData.visualRange;
            }
        }
        if (TryGetComponent(out ShowDetection showDetection))
        {
            showDetection.Setup(_range);
        }
    }

    private void Update()
    {
        _queryTimer += Time.deltaTime;

        if (_queryTimer >= _queryInterval)
        {
            _queryTimer = 0f;
            QueryTargets();
        }
    }

    private void QueryTargets()
    {
        int previousCount = _targetsInRange.Count;
        _targetsInRange.Clear();

        Collider[] hits = Physics.OverlapSphere(transform.position, _range, _targetLayerMask);

        foreach (Collider hit in hits)
        {
            if (hit.TryGetComponent(out Health _))
            {
                _targetsInRange.Add(hit.gameObject);
            }
        }

        if (previousCount == 0 && _targetsInRange.Count > 0)
        {
            OnTargetsAcquired?.Invoke();
        }
    }

    public List<GameObject> GetTargetsInRange()
    {
        return _targetsInRange;
    }
}
