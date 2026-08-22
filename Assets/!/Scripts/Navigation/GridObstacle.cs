using UnityEngine;

/// <summary>
/// Component attached to objects that act as grid obstacles.
/// Detected by MobVision to halt GridNavigationAgents.
/// </summary>
public class GridObstacle : MonoBehaviour
{
    [SerializeField] private bool _isBlocking = true;

    public bool IsBlocking
    {
        get => _isBlocking && enabled && gameObject.activeInHierarchy;
        set => _isBlocking = value;
    }
}

