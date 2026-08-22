using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used to detect obstacles that prevent mob from moving forward: traps, player defenses, other mobs
/// </summary>
public class MobVision : MonoBehaviour
{
    private List<GridObstacle> _obstacles = new List<GridObstacle>();
    private Collider _collider;

    private void Awake()
    {
        _collider = GetComponent<Collider>();
    }

    /// <summary>
    /// Register obstacle entering detection range
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        GridObstacle newObstacle = other.GetComponent<GridObstacle>();
        if (newObstacle != null && other != _collider)
        {
            _obstacles.Add(newObstacle);
            Debug.Log($"Obstacle detected: {newObstacle.name}");
        }
    }

    /// <summary>
    /// Register obstacle leaving detection range
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerExit(Collider other)
    {
        GridObstacle obstacle = other.GetComponent<GridObstacle>();
        if (obstacle != null && _obstacles.Contains(obstacle))
        {
            _obstacles.Remove(obstacle);
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
}


