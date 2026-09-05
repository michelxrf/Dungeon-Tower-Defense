using System.Collections;
using TMPro;
using UnityEngine;

public class MobSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _mobPrefab;

    private bool _inCooldown = false;
    private Collider _collider;
    private bool _isSpawnAreaOccupied;
    private int _spawnCount = 0;
    private bool _finishedSpawning = false;
    private int _livingMobs = 0;

    private void Update()
    {
        SpawnMobs();
    }

    private void Start()
    {
        LevelManager.Instance.OnWaveEnded += () => _finishedSpawning = false;
    }

    private void SpawnMobs()
    {
        if (!_isSpawnAreaOccupied && !_inCooldown && !_finishedSpawning)
        {
            GameObject mob = LevelManager.Instance.GetMob(_spawnCount);
            if (mob == null)
            {
                _finishedSpawning = true;
                StartCoroutine(NotifyWaveDestroyed());
                return;
            }

            SpawnMob(mob);
        }
    }


    private void SpawnMob(GameObject mobPrefab)
    {
        if (_mobPrefab == null) return;
        GameObject mobInstance = Instantiate(_mobPrefab, transform.position, Quaternion.identity);
        _spawnCount++;
        _livingMobs++;
        mobInstance.GetComponent<Health>().OnDeath += () => _livingMobs--;
        StartCoroutine(WaitCooldown(PerformanceSettings.MobSpawnInterval));
    }

    IEnumerator WaitCooldown(float waitTime)
    {
        _inCooldown = true;
        yield return new WaitForSeconds(waitTime);
        _inCooldown = false;
    }

    IEnumerator WaitForSpawnAreaClear(float waitTime)
    {
        _isSpawnAreaOccupied = Physics.OverlapSphere(transform.position, 0.5f, LayerMask.GetMask("EnemyPawn"), QueryTriggerInteraction.Ignore).Length == 0;
        yield return new WaitForSeconds(waitTime);
        StartCoroutine(WaitForSpawnAreaClear(waitTime));
    }

    IEnumerator NotifyWaveDestroyed()
    {
        yield return new WaitUntil(() => _finishedSpawning && _livingMobs <= 0);
        _spawnCount = 0;
        LevelManager.Instance.WaveFinished();
    }
}
