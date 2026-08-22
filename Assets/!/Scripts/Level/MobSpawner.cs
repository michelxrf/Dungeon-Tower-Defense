using UnityEngine;

public class MobSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _mobPrefab;
    [SerializeField] private float _spawnInterval = 2f;

    private float _spawnTimer;
    private Collider _collider;
    private bool _isSpawnAreaOccupied;

    private void Update()
    {
        _spawnTimer += Time.deltaTime;

        if (_spawnTimer >= _spawnInterval)
        {
            _spawnTimer = 0f;

            if (Physics.OverlapSphere(transform.position, 0.5f, LayerMask.GetMask("EnemyPawn"), QueryTriggerInteraction.Ignore).Length == 0)
            {
                _isSpawnAreaOccupied = false;
                SpawnMob();
            }
            else
            {
                _isSpawnAreaOccupied = true;
            }
        }
    }

    private void SpawnMob()
    {
        if (_mobPrefab == null) return;
        GameObject mobInstance = Instantiate(_mobPrefab, transform.position, Quaternion.identity);  
    }
}
