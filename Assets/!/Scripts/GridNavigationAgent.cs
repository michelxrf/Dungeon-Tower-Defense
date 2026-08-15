using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Agent that navigates along a path calculated by GridManager.
/// Uses MobVision for physical obstacle detection and node reservations
/// to prevent multiple agents from disputing the same grid node.
/// </summary>
public class GridNavigationAgent : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 3.0f;
    [SerializeField] private bool _autoStart = true;

    [Header("Vision & Obstacles")]
    [SerializeField] private MobVision _mobVision;

    [Header("Node Reservation Settings")]
    [Tooltip("The distance from the node at which the agent will consider itself to have reached the node. NEED TO BE HALF THE SIZE OF A GRID CELL")]
    [SerializeField] private float _nodeReservationDistance = 0.5f;

    [Header("Path Selection")]
    [SerializeField] private int _pathIndex = 0;
    [SerializeField] private bool _useRandomPath = false;

    private List<GridNavigationNode> _pathNodes;
    private Polyline _polyline;
    private float _currentDistance = 0f;
    private int _currentNodeIndex = 0;
    private bool _isMoving = false;

    // The next node this agent has successfully reserved.
    private GridNavigationNode _reservedNode;

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

        _reservedNode = null;
    }

    /// <summary>
    /// Starts or restarts following a path from GridManager.
    /// </summary>
    public void StartFollowingPath()
    {
        GridManager manager = GridManager.Instance != null
            ? GridManager.Instance
            : FindAnyObjectByType<GridManager>();

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

        Vector3[] controlPoints = _pathNodes
            .Select(n => n.Position)
            .ToArray();

        _polyline = new Polyline(controlPoints);

        if (_polyline == null || _polyline.TotalLength <= 0f)
        {
            Debug.LogWarning("[GridNavigationAgent] No valid path polyline built.");
            _isMoving = false;
            return;
        }

        manager.RegisterAgent(this);

        _currentDistance = 0f;
        _currentNodeIndex = 0;
        _reservedNode = null;
        _isMoving = true;

        transform.position = _polyline.EvaluateDistance(0f);

        // Reserve and occupy the starting node.
        manager.TryReserveNode(_pathNodes[0], this);
        manager.OccupyNode(_pathNodes[0], this);

        // Face the first direction of the path.
        UpdateRotation();
    }

    private void Update()
    {
        if (!_isMoving ||
            _polyline == null ||
            _polyline.TotalLength <= 0f ||
            _pathNodes == null ||
            _pathNodes.Count == 0)
        {
            return;
        }

        GridManager manager = GridManager.Instance;

        if (manager == null)
            return;

        // Determine whether the agent can enter the next node.
        if (!CanAdvanceToNextNode(manager))
            return;

        // MobVision handles physical obstacles and other agents.
        if (_mobVision == null)
        {
            _mobVision = GetComponentInChildren<MobVision>();
        }

        if (_mobVision != null && !_mobVision.IsPathClear())
        {
            return;
        }

        // Move forward.
        _currentDistance = Mathf.Min(
            _currentDistance + _moveSpeed * Time.deltaTime,
            _polyline.TotalLength
        );

        // Check node progression and occupation handoff.
        UpdateNodeOccupation(manager);

        // Position & Rotation update.
        UpdateTransform();

        // End of path handling.
        if (_currentDistance >= _polyline.TotalLength - 0.01f &&
            _currentNodeIndex >= _pathNodes.Count - 1)
        {
            manager.ReleaseNode(
                _pathNodes[_currentNodeIndex],
                this
            );

            _isMoving = false;
        }
    }

    /// <summary>
    /// Determines whether this agent is allowed to continue toward
    /// its next node.
    ///
    /// A node reservation is treated as the right-of-way system.
    /// Once the agent has reserved a node, another agent cannot claim it.
    /// </summary>
    private bool CanAdvanceToNextNode(GridManager manager)
    {
        // There is no next node.
        if (_currentNodeIndex + 1 >= _pathNodes.Count)
            return true;

        GridNavigationNode nextNode =
            _pathNodes[_currentNodeIndex + 1];

        float nextNodeDistance =
            _polyline.GetPointDistance(
                _currentNodeIndex + 1
            );

        // If we already own the next node, we have right of way.
        if (_reservedNode == nextNode)
            return true;

        // Start attempting to reserve the node when we are close enough.
        float reservationDistance =
            Mathf.Max(
                0f,
                nextNodeDistance - _nodeReservationDistance
            );

        if (_currentDistance < reservationDistance)
            return true;

        // Try to claim the node.
        bool reserved =
            manager.TryReserveNode(nextNode, this);

        if (!reserved)
        {
            // Another agent currently owns this node.
            //
            // Do not advance far enough to enter it.
            _currentDistance = Mathf.Min(
                _currentDistance,
                nextNodeDistance - 0.01f
            );

            return false;
        }

        // We now own the node.
        _reservedNode = nextNode;

        return true;
    }

    /// <summary>
    /// Updates which node this agent currently occupies as it travels along the polyline.
    /// Releases previous nodes so queued agents behind can advance.
    /// </summary>
    private void UpdateNodeOccupation(GridManager manager)
    {
        if (_currentNodeIndex + 1 >= _pathNodes.Count)
            return;

        float nextNodeDistance =
            _polyline.GetPointDistance(
                _currentNodeIndex + 1
            );

        // When the agent reaches the next node, transfer occupation.
        if (_currentDistance >= nextNodeDistance - 0.01f)
        {
            GridNavigationNode previousNode =
                _pathNodes[_currentNodeIndex];

            _currentNodeIndex++;

            GridNavigationNode currentNode =
                _pathNodes[_currentNodeIndex];

            manager.OccupyNode(
                currentNode,
                this
            );

            manager.ReleaseNode(
                previousNode,
                this
            );

            // We have now reached the node we reserved.
            if (_reservedNode == currentNode)
            {
                _reservedNode = null;
            }
        }
    }

    /// <summary>
    /// Updates the agent's position and rotation along the polyline.
    /// Rotation is always based on the direction of the path ahead,
    /// preventing the agent from looking backwards when stopped.
    /// </summary>
    private void UpdateTransform()
    {
        Vector3 targetPosition =
            _polyline.EvaluateDistance(_currentDistance);

        transform.position = targetPosition;

        UpdateRotation();
    }

    /// <summary>
    /// Calculates the forward direction from the polyline and rotates
    /// the agent toward the direction it will travel next.
    /// </summary>
    private void UpdateRotation()
    {
        if (_polyline == null || _polyline.TotalLength <= 0f)
            return;

        const float lookAheadDistance = 0.1f;

        float lookDistance =
            Mathf.Min(
                _currentDistance + lookAheadDistance,
                _polyline.TotalLength
            );

        Vector3 currentPosition =
            _polyline.EvaluateDistance(_currentDistance);

        Vector3 lookPosition =
            _polyline.EvaluateDistance(lookDistance);

        Vector3 direction =
            lookPosition - currentPosition;

        // If there is no path ahead, look backwards along the path
        // to determine the final forward direction.
        if (direction.sqrMagnitude <= 0.000001f)
        {
            float previousDistance =
                Mathf.Max(
                    0f,
                    _currentDistance - lookAheadDistance
                );

            Vector3 previousPosition =
                _polyline.EvaluateDistance(previousDistance);

            direction =
                currentPosition - previousPosition;
        }

        if (direction.sqrMagnitude <= 0.000001f)
            return;

        direction.Normalize();

        transform.rotation =
            Quaternion.LookRotation(
                direction,
                Vector3.up
            );
    }

    public void Stop()
    {
        _isMoving = false;
        ReleaseReservations();
    }
}