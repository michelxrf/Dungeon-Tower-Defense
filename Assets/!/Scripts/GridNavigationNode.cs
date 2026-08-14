using UnityEngine;

/// <summary>
/// A node for grid navigation, representing a point in the grid that can be used for pathfinding.
/// Also tracks agent occupancy and reservations to manage queueing and prevent agent overlapping.
/// </summary>
public class GridNavigationNode : MonoBehaviour
{
    [SerializeField] private NodeType _nodeType;
    [SerializeField] private Transform _transformOverride;

    private GridNavigationAgent _occupyingAgent;
    private GridNavigationAgent _reservingAgent;

    public NodeType NodeType
    {
        get => _nodeType;
        set => _nodeType = value;
    }

    public Vector3 Position => _transformOverride != null ? _transformOverride.position : transform.position;

    public GridNavigationAgent OccupyingAgent => _occupyingAgent;
    public GridNavigationAgent ReservingAgent => _reservingAgent;

    public bool IsOccupiedOrReserved => _occupyingAgent != null || _reservingAgent != null;

    public bool CanBeReservedBy(GridNavigationAgent agent)
    {
        if (agent == null) return false;
        bool occOK = _occupyingAgent == null || _occupyingAgent == agent;
        bool resOK = _reservingAgent == null || _reservingAgent == agent;
        return occOK && resOK;
    }

    public bool TryReserve(GridNavigationAgent agent)
    {
        if (CanBeReservedBy(agent))
        {
            _reservingAgent = agent;
            return true;
        }
        return false;
    }

    public void Occupy(GridNavigationAgent agent)
    {
        _occupyingAgent = agent;
        if (_reservingAgent == agent)
        {
            _reservingAgent = null;
        }
    }

    public void Release(GridNavigationAgent agent)
    {
        if (_occupyingAgent == agent)
        {
            _occupyingAgent = null;
        }
        if (_reservingAgent == agent)
        {
            _reservingAgent = null;
        }
    }
}

public enum NodeType
{
    Start,
    End,
    Normal
}


