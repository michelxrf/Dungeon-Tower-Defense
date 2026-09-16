using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to detect obstacles that prevent mob from moving forward: traps, player defenses, other mobs.
/// OverlapBox-based replacement for <see cref="MobVision"/> that requires no Collider or trigger.
/// Queries the physics scene on an interval and caches any <see cref="GridObstacle"/> in front.
/// </summary>
public class ObstacleDetection : MonoBehaviour
{
    [SerializeField] private float _range = 0.5f;
    [SerializeField] private float _queryInterval = 0.1f;
    [SerializeField] private LayerMask _obstacleLayerMask = ~0;

    private readonly List<GridObstacle> _obstacles = new();
    private float _queryTimer;
    private GridObstacle _selfObstacle;

    private void Awake()
    {
        _selfObstacle = GetComponentInParent<GridObstacle>();
    }

    private void OnEnable()
    {
        // Query immediately on enable so IsPathClear() is valid on the first frame.
        _queryTimer = _queryInterval;
    }

    private void Update()
    {
        _queryTimer += Time.deltaTime;

        if (_queryTimer >= _queryInterval)
        {
            _queryTimer = 0f;
            QueryObstacles();
        }
    }

    private void QueryObstacles()
    {
        _obstacles.Clear();

        // Cube of side _range sitting directly ahead, touching this transform.
        Vector3 halfExtents = Vector3.one * (_range * 0.5f);
        Vector3 center = transform.position + transform.forward * (_range * 0.5f);

        Collider[] hits = Physics.OverlapBox(
            center,
            halfExtents,
            transform.rotation,
            _obstacleLayerMask,
            QueryTriggerInteraction.Collide);

        foreach (Collider hit in hits)
        {
            if (!hit.TryGetComponent(out GridObstacle obstacle))
            {
                // Obstacle component may live on a parent (e.g. root mob object).
                obstacle = hit.GetComponentInParent<GridObstacle>();
                if (obstacle == null)
                {
                    continue;
                }
            }

            // Ignore our own obstacle so we never block ourselves.
            if (obstacle == _selfObstacle)
            {
                continue;
            }

            if (!obstacle.IsBlocking)
            {
                continue;
            }

            if (!_obstacles.Contains(obstacle))
            {
                _obstacles.Add(obstacle);
            }
        }
    }

    /// <summary>
    /// Check if there are any obstacles in the detection range
    /// </summary>
    /// <returns>Returns true if the path is clear, false otherwise</returns>
    public bool IsPathClear()
    {
        return _obstacles.Count == 0;
    }

    public List<GridObstacle> GetObstacles()
    {
        return _obstacles;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 halfExtents = Vector3.one * (_range * 0.5f);
        Vector3 center = transform.position + transform.forward * (_range * 0.5f);

        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.TRS(center, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, halfExtents * 2f);
    }
}
