using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manager class for handling all grid data like navigation and state.
/// Calculates and stores all valid simple paths from Start to End nodes.
/// Manages agent registration and node/cell reservations to support queuing and prevent agent overlap.
/// </summary>
public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [SerializeField] private bool _drawGizmos = true;
    [SerializeField] private Color[] _pathColors = new Color[]
    {
        Color.cyan,
        Color.magenta,
        Color.yellow,
        Color.green,
        Color.red,
        new Color(1f, 0.5f, 0f) // Orange
    };
    [SerializeField] private Color _nodeColor = Color.white;
    [SerializeField] private int _maxPathCount = 100;

    private Dictionary<GridNavigationNode, Vector2Int> _gridNodes = new Dictionary<GridNavigationNode, Vector2Int>();
    private Dictionary<Vector2Int, List<GridNavigationNode>> _positionToNodes = new Dictionary<Vector2Int, List<GridNavigationNode>>();
    private Grid _grid;

    private List<CatmullRomSpline> _pathSplines = new List<CatmullRomSpline>();
    private List<List<GridNavigationNode>> _allCalculatedPaths = new List<List<GridNavigationNode>>();
    private List<GridNavigationAgent> _activeAgents = new List<GridNavigationAgent>();
    private bool _pathCalculated = false;

    public IReadOnlyList<CatmullRomSpline> PathSplines
    {
        get
        {
            if (!_pathCalculated || _pathSplines == null || _pathSplines.Count == 0)
            {
                CalculateAndStoreAllPaths();
            }
            return _pathSplines;
        }
    }

    public CatmullRomSpline PathSpline
    {
        get
        {
            var splines = PathSplines;
            return splines != null && splines.Count > 0 ? splines[0] : null;
        }
    }

    public IReadOnlyList<List<GridNavigationNode>> AllCalculatedPaths => _allCalculatedPaths;
    public List<GridNavigationNode> CalculatedPathNodes => (_allCalculatedPaths != null && _allCalculatedPaths.Count > 0) ? _allCalculatedPaths[0] : null;
    public IReadOnlyList<GridNavigationAgent> ActiveAgents => _activeAgents;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _grid = GetComponent<Grid>();
        if (!_pathCalculated)
        {
            CalculateAndStoreAllPaths();
        }
    }

    public void RegisterAgent(GridNavigationAgent agent)
    {
        if (agent != null && !_activeAgents.Contains(agent))
        {
            _activeAgents.Add(agent);
        }
    }

    public void UnregisterAgent(GridNavigationAgent agent)
    {
        if (agent != null)
        {
            _activeAgents.Remove(agent);
            ClearNodeReservationsForAgent(agent);
        }
    }

    /// <summary>
    /// Searches the whole level for all navigation nodes and stores data for further use.
    /// Maps 3D world positions to 2D grid coordinates on the XZ plane.
    /// </summary>
    public void InitializeGridDictionary()
    {
        _gridNodes.Clear();
        _positionToNodes.Clear();

        if (_grid == null)
        {
            _grid = GetComponent<Grid>();
        }

        GridNavigationNode[] nodes = FindObjectsByType<GridNavigationNode>();

        foreach (GridNavigationNode node in nodes)
        {
            if (node == null) continue;

            Vector3Int cellPosition = _grid != null ? _grid.WorldToCell(node.transform.position) : Vector3Int.RoundToInt(node.transform.position);
            Vector2Int gridPosition = new Vector2Int(cellPosition.x, cellPosition.z);

            _gridNodes[node] = gridPosition;

            if (!_positionToNodes.TryGetValue(gridPosition, out List<GridNavigationNode> listAtPos))
            {
                listAtPos = new List<GridNavigationNode>();
                _positionToNodes[gridPosition] = listAtPos;
            }
            listAtPos.Add(node);
        }
    }

    /// <summary>
    /// Calculates all non-self-overlapping paths from Start to End nodes and stores them as Catmull-Rom splines.
    /// </summary>
    public List<CatmullRomSpline> CalculateAndStoreAllPaths()
    {
        InitializeGridDictionary();

        _pathSplines.Clear();
        _allCalculatedPaths.Clear();

        GridNavigationNode startNode = _gridNodes.Keys.FirstOrDefault(n => n != null && n.NodeType == NodeType.Start);
        GridNavigationNode endNode = _gridNodes.Keys.FirstOrDefault(n => n != null && n.NodeType == NodeType.End);

        if (startNode == null || endNode == null)
        {
            Debug.LogWarning("[GridManager] Start node or End node missing in scene.");
            _pathCalculated = true;
            return _pathSplines;
        }

        _allCalculatedPaths = FindAllPaths(startNode, endNode, _maxPathCount);

        foreach (List<GridNavigationNode> pathNodes in _allCalculatedPaths)
        {
            if (pathNodes != null && pathNodes.Count > 0)
            {
                Vector3[] controlPoints = pathNodes.Select(n => n.Position).ToArray();
                _pathSplines.Add(new CatmullRomSpline(controlPoints));
            }
        }

        if (_pathSplines.Count == 0)
        {
            Debug.LogWarning("[GridManager] No valid path found from Start to End.");
        }

        _pathCalculated = true;
        return _pathSplines;
    }

    /// <summary>
    /// Compatibility method for single path calculation call.
    /// </summary>
    public CatmullRomSpline CalculateAndStorePath()
    {
        var splines = CalculateAndStoreAllPaths();
        return splines != null && splines.Count > 0 ? splines[0] : null;
    }

    /// <summary>
    /// Safely gets a specific path spline by index.
    /// </summary>
    public CatmullRomSpline GetPathSpline(int index)
    {
        var splines = PathSplines;
        if (splines == null || splines.Count == 0) return null;
        index = Mathf.Clamp(index, 0, splines.Count - 1);
        return splines[index];
    }

    /// <summary>
    /// Gets a random path spline from the stored paths.
    /// </summary>
    public CatmullRomSpline GetRandomPathSpline()
    {
        var splines = PathSplines;
        if (splines == null || splines.Count == 0) return null;
        return splines[UnityEngine.Random.Range(0, splines.Count)];
    }

    /// <summary>
    /// Gets the list of GridNavigationNodes for a path index.
    /// </summary>
    public List<GridNavigationNode> GetPathNodes(int index)
    {
        if (!_pathCalculated || _allCalculatedPaths == null || _allCalculatedPaths.Count == 0)
        {
            CalculateAndStoreAllPaths();
        }
        if (_allCalculatedPaths == null || _allCalculatedPaths.Count == 0) return null;
        index = Mathf.Clamp(index, 0, _allCalculatedPaths.Count - 1);
        return _allCalculatedPaths[index];
    }

    /// <summary>
    /// Gets the list of GridNavigationNodes for a random path.
    /// </summary>
    public List<GridNavigationNode> GetRandomPathNodes(out int chosenIndex)
    {
        if (!_pathCalculated || _allCalculatedPaths == null || _allCalculatedPaths.Count == 0)
        {
            CalculateAndStoreAllPaths();
        }
        if (_allCalculatedPaths == null || _allCalculatedPaths.Count == 0)
        {
            chosenIndex = -1;
            return null;
        }
        chosenIndex = UnityEngine.Random.Range(0, _allCalculatedPaths.Count);
        return _allCalculatedPaths[chosenIndex];
    }

    /// <summary>
    /// Checks if a node and its grid cell location are free of other agents, and reserves it for the specified agent if free.
    /// </summary>
    public bool TryReserveNode(GridNavigationNode node, GridNavigationAgent agent)
    {
        if (node == null || agent == null) return false;

        if (!node.CanBeReservedBy(agent)) return false;

        // Check if any other node sharing the same grid position is occupied or reserved by another agent
        if (_gridNodes.TryGetValue(node, out Vector2Int gridPos))
        {
            if (IsPositionOccupiedOrReserved(gridPos, agent)) return false;
        }

        return node.TryReserve(agent);
    }

    /// <summary>
    /// Checks if any navigation node at the specified 2D grid position is occupied or reserved by an agent other than excludeAgent.
    /// </summary>
    public bool IsPositionOccupiedOrReserved(Vector2Int gridPos, GridNavigationAgent excludeAgent)
    {
        if (_positionToNodes.TryGetValue(gridPos, out List<GridNavigationNode> nodesAtPos))
        {
            foreach (GridNavigationNode node in nodesAtPos)
            {
                if (node == null) continue;
                if ((node.OccupyingAgent != null && node.OccupyingAgent != excludeAgent) ||
                    (node.ReservingAgent != null && node.ReservingAgent != excludeAgent))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void OccupyNode(GridNavigationNode node, GridNavigationAgent agent)
    {
        if (node != null && agent != null)
        {
            node.Occupy(agent);
        }
    }

    public void ReleaseNode(GridNavigationNode node, GridNavigationAgent agent)
    {
        if (node != null && agent != null)
        {
            node.Release(agent);
        }
    }

    public void ClearNodeReservationsForAgent(GridNavigationAgent agent)
    {
        if (agent == null) return;
        foreach (var node in _gridNodes.Keys)
        {
            if (node != null)
            {
                node.Release(agent);
            }
        }
    }

    /// <summary>
    /// Returns only direct cardinal neighbors (North, South, East, West) in grid space.
    /// Diagonals are strictly excluded.
    /// </summary>
    public GridNavigationNode[] GetNodeNeighbors(GridNavigationNode node)
    {
        if (node == null) return Array.Empty<GridNavigationNode>();

        if (_gridNodes.Count == 0)
        {
            InitializeGridDictionary();
        }

        if (!_gridNodes.TryGetValue(node, out Vector2Int cellPos))
        {
            return Array.Empty<GridNavigationNode>();
        }

        List<GridNavigationNode> neighbors = new List<GridNavigationNode>();

        Vector2Int[] cardinalDirections = new Vector2Int[]
        {
            new Vector2Int(0, 1),   // Up / North (+Z)
            new Vector2Int(0, -1),  // Down / South (-Z)
            new Vector2Int(-1, 0),  // Left / West (-X)
            new Vector2Int(1, 0)    // Right / East (+X)
        };

        foreach (Vector2Int dir in cardinalDirections)
        {
            Vector2Int neighborPos = cellPos + dir;
            if (_positionToNodes.TryGetValue(neighborPos, out List<GridNavigationNode> nodesAtPos))
            {
                foreach (GridNavigationNode neighbor in nodesAtPos)
                {
                    if (neighbor != null && neighbor != node && IsNodeTraversable(neighbor))
                    {
                        neighbors.Add(neighbor);
                    }
                }
            }
        }

        return neighbors.ToArray();
    }

    private bool IsNodeTraversable(GridNavigationNode node)
    {
        if (node == null) return false;
        NodeType type = node.NodeType;
        return type == NodeType.Normal || type == NodeType.Start || type == NodeType.End;
    }

    /// <summary>
    /// Finds all simple (non-self-overlapping) paths from startNode to endNode using DFS backtracking.
    /// Allows paths to bifurcate into multiple valid branches.
    /// </summary>
    private List<List<GridNavigationNode>> FindAllPaths(GridNavigationNode startNode, GridNavigationNode endNode, int maxPaths)
    {
        if (startNode == null || endNode == null) return new List<List<GridNavigationNode>>();

        List<List<GridNavigationNode>> results = new List<List<GridNavigationNode>>();
        HashSet<GridNavigationNode> visited = new HashSet<GridNavigationNode>();
        List<GridNavigationNode> currentPath = new List<GridNavigationNode>();

        void DFS(GridNavigationNode current)
        {
            if (results.Count >= maxPaths) return;

            visited.Add(current);
            currentPath.Add(current);

            if (current == endNode)
            {
                results.Add(new List<GridNavigationNode>(currentPath));
            }
            else
            {
                GridNavigationNode[] neighbors = GetNodeNeighbors(current);
                foreach (GridNavigationNode neighbor in neighbors)
                {
                    if (neighbor != null && !visited.Contains(neighbor))
                    {
                        DFS(neighbor);
                    }
                }
            }

            currentPath.RemoveAt(currentPath.Count - 1);
            visited.Remove(current);
        }

        DFS(startNode);

        // Sort paths by length (shortest paths first)
        results.Sort((a, b) => a.Count.CompareTo(b.Count));

        return results;
    }

    private void OnDrawGizmos()
    {
        if (!_drawGizmos) return;

        if (!Application.isPlaying && (!_pathCalculated || _pathSplines == null || _pathSplines.Count == 0))
        {
            CalculateAndStoreAllPaths();
        }

        if (_pathSplines == null || _pathSplines.Count == 0) return;

        for (int p = 0; p < _pathSplines.Count; p++)
        {
            CatmullRomSpline spline = _pathSplines[p];
            if (spline == null || spline.SampledPoints == null || spline.SampledPoints.Count == 0) continue;

            Color pathColor = (_pathColors != null && _pathColors.Length > 0)
                ? _pathColors[p % _pathColors.Length]
                : Color.HSVToRGB((float)p / _pathSplines.Count, 1f, 1f);

            Gizmos.color = pathColor;
            IReadOnlyList<Vector3> points = spline.SampledPoints;
            for (int i = 0; i < points.Count - 1; i++)
            {
                Gizmos.DrawLine(points[i], points[i + 1]);
            }

            Gizmos.color = _nodeColor;
            if (spline.ControlPoints != null)
            {
                foreach (Vector3 cp in spline.ControlPoints)
                {
                    Gizmos.DrawWireSphere(cp, 0.15f);
                }
            }
        }
    }
}




