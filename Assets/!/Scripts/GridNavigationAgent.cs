using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Agent that navigates along a path calculated by GridManager.
/// Manages node reservations and proximity spacing so agents form a queue and never overlap,
/// even when traversing intersecting/overlapping grid paths.
/// </summary>
public class GridNavigationAgent : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 3.0f;
    [SerializeField] private bool _autoStart = true;

    [Header("Vision & Obstacles")]
    [SerializeField] private MobVision _mobVision;

    [Header("Queue & Collision Settings")]
    [SerializeField] private float _stoppingDistance = 0.8f;
    [SerializeField] private float _nodeReservationBuffer = 0.2f;

    [Header("Path Selection")]
    [SerializeField] private int _pathIndex = 0;
    [SerializeField] private bool _useRandomPath = false;

    private List<GridNavigationNode> _pathNodes;
    private Polyline _polyline;
    private float _currentDistance = 0f;
    private int _currentNodeIndex = 0;
    private bool _isMoving = false;

    public float MoveSpeed
    {
        get => _moveSpeed;
        set => _moveSpeed = value;
    }

    public int PathIndex
    {
        get => _pathIndex;
        set => _pathIndex = value;
    }

    public bool UseRandomPath
    {
        get => _useRandomPath;
        set => _useRandomPath = value;
    }

    public bool IsMoving => _isMoving;

    private void Start()
    {
        if (_autoStart)
        {
            StartFollowingPath();
        }
    }

    private void OnDisable()
    {
        ReleaseReservations();
    }

    private void OnDestroy()
    {
        ReleaseReservations();
    }

    private void ReleaseReservations()
    {
        if (GridManager.Instance != null)
        {
            GridManager.Instance.UnregisterAgent(this);
        }
    }

    /// <summary>
    /// Starts or restarts following a path from GridManager.
    /// </summary>
    public void StartFollowingPath()
    {
        GridManager manager = GridManager.Instance != null ? GridManager.Instance : FindAnyObjectByType<GridManager>();

        if (manager == null)
        {
            Debug.LogWarning("[GridNavigationAgent] GridManager instance not found in scene.");
            _isMoving = false;
            return;
        }

        if (_useRandomPath)
        {
            _pathNodes = manager.GetRandomPathNodes(out _pathIndex);
        }
        else
        {
            _pathNodes = manager.GetPathNodes(_pathIndex);
        }

        if (_pathNodes == null || _pathNodes.Count == 0)
        {
            Debug.LogWarning("[GridNavigationAgent] No valid path nodes found to navigate.");
            _isMoving = false;
            return;
        }

        Vector3[] controlPoints = _pathNodes.Select(n => n.Position).ToArray();
        _polyline = new Polyline(controlPoints);

        if (_polyline == null || _polyline.TotalLength <= 0f)
        {
            Debug.LogWarning("[GridNavigationAgent] No valid path spline built.");
            _isMoving = false;
            return;
        }

        manager.RegisterAgent(this);

        _currentDistance = 0f;
        _currentNodeIndex = 0;
        _isMoving = true;

        transform.position = _polyline.EvaluateDistance(0f);

        // Try to reserve start node
        manager.TryReserveNode(_pathNodes[0], this);
        manager.OccupyNode(_pathNodes[0], this);
    }

    private void Update()
    {
        if (!_isMoving || _polyline == null || _polyline.TotalLength <= 0f || _pathNodes == null || _pathNodes.Count == 0) return;

        GridManager manager = GridManager.Instance;
        if (manager == null) return;

        // Determine target distance bound based on node reservation and proximity to agent ahead
        float maxAllowedDistance = GetMaxAllowedDistance(manager);

        if (_currentDistance < maxAllowedDistance)
        {
            float nextDistance = Mathf.Min(_currentDistance + _moveSpeed * Time.deltaTime, maxAllowedDistance);
            _currentDistance = nextDistance;
        }

        // Check node progression and occupation handoff
        UpdateNodeOccupation(manager);

        // Position & Rotation update
        Vector3 targetPosition = _polyline.EvaluateDistance(_currentDistance);
        
        if(transform.position != targetPosition)
        {
            transform.rotation = Quaternion.LookRotation(targetPosition - transform.position, Vector3.up);
            transform.position = targetPosition;
        }

        // End of path handling
        if (_currentDistance >= _polyline.TotalLength - 0.01f && _currentNodeIndex >= _pathNodes.Count - 1)
        {
            manager.ReleaseNode(_pathNodes[_currentNodeIndex], this);
            _isMoving = false;
        }
    }

    /// <summary>
    /// Calculates how far along the spline this agent is allowed to advance without colliding with another agent or entering an unreserved node.
    /// </summary>
    private float GetMaxAllowedDistance(GridManager manager)
    {
        // 0. MobVision Obstacle Constraint
        if (_mobVision == null)
        {
            _mobVision = GetComponentInChildren<MobVision>();
        }

        if (_mobVision != null && !_mobVision.IsPathClear())
        {
            return _currentDistance;
        }

        float maxDist = _polyline.TotalLength;

        // 1. Node Reservation Constraint
        if (_currentNodeIndex + 1 < _pathNodes.Count)
        {
            GridNavigationNode nextNode = _pathNodes[_currentNodeIndex + 1];
            float nextNodeDistance = _polyline.GetPointDistance(_currentNodeIndex + 1);

            // Attempt to reserve the next node when within buffer distance
            bool reserved = true;
            if (_currentDistance >= nextNodeDistance - _nodeReservationBuffer - _stoppingDistance)
            {
                reserved = manager.TryReserveNode(nextNode, this);
            }

            if (!reserved)
            {
                // Stop before entering the unreserved node
                float capAtNode = Mathf.Max(0f, nextNodeDistance - _stoppingDistance);
                maxDist = Mathf.Min(maxDist, capAtNode);
            }
        }

        // 2. Proximity & Queue Constraint against other active agents
        Vector3 currentPos = transform.position;
        Vector3 moveDir = _polyline.EvaluateDistance(Mathf.Min(_currentDistance + 0.1f, _polyline.TotalLength)) - currentPos;
        bool isMovingForward = moveDir.sqrMagnitude > 0.0001f;

        if (isMovingForward)
        {
            moveDir.Normalize();

            foreach (GridNavigationAgent other in manager.ActiveAgents)
            {
                if (other == null || other == this || !other.gameObject.activeInHierarchy) continue;

                Vector3 toOther = other.transform.position - currentPos;
                float distToOther = toOther.magnitude;

                if (distToOther < _stoppingDistance * 1.5f && distToOther > 0.001f)
                {
                    // Check if the other agent is ahead of us along our path
                    if (Vector3.Dot(moveDir, toOther / distToOther) > 0.3f)
                    {
                        float safeDistance = Mathf.Max(0f, _currentDistance + (distToOther - _stoppingDistance));
                        maxDist = Mathf.Min(maxDist, safeDistance);
                    }
                }
            }
        }

        return maxDist;
    }

    /// <summary>
    /// Updates which node this agent currently occupies as it travels along the spline.
    /// Releases previous nodes so queued agents behind can advance.
    /// </summary>
    private void UpdateNodeOccupation(GridManager manager)
    {
        if (_currentNodeIndex + 1 >= _pathNodes.Count) return;

        float nextNodeDistance = _polyline.GetPointDistance(_currentNodeIndex + 1);

        // When close enough to next node, transfer occupation and release previous node
        if (_currentDistance >= nextNodeDistance - 0.1f)
        {
            GridNavigationNode prevNode = _pathNodes[_currentNodeIndex];
            _currentNodeIndex++;
            GridNavigationNode currentNode = _pathNodes[_currentNodeIndex];

            manager.OccupyNode(currentNode, this);
            manager.ReleaseNode(prevNode, this);
        }
    }

    public void Stop()
    {
        _isMoving = false;
        ReleaseReservations();
    }
}



