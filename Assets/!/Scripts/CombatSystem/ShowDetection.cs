using UnityEngine;

/// <summary>
/// Attached to a sphere mesh used to visualize a detection range.
/// Toggles the mesh visibility and resizes the sphere via <see cref="Setup"/>.
/// </summary>
[RequireComponent(typeof(MeshRenderer))]
public class ShowDetection : MonoBehaviour
{
    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private bool _hideOnAwake = true;

    private float _radius = 1f;
    private MeshFilter _meshFilter;

    private void Awake()
    {
        if (_meshRenderer == null)
        {
            _meshRenderer = GetComponent<MeshRenderer>();
        }

        _meshFilter = GetComponent<MeshFilter>();

        if (_hideOnAwake)
        {
            Hide();
        }
    }

    /// <summary>
    /// Resizes the sphere mesh so its world radius matches <paramref name="radius"/>.
    /// Works with any sphere mesh by normalizing against the mesh's base radius
    /// (Unity's default sphere has a radius of 0.5).
    /// </summary>
    /// <param name="radius">Desired world-space radius.</param>
    public void Setup(float radius)
    {
        _radius = Mathf.Max(radius, 0.01f);

        float baseRadius = 0.5f;
        if (_meshFilter == null)
        {
            _meshFilter = GetComponent<MeshFilter>();
        }

        if (_meshFilter != null && _meshFilter.sharedMesh != null)
        {
            baseRadius = _meshFilter.sharedMesh.bounds.extents.x;
            if (Mathf.Approximately(baseRadius, 0f))
            {
                baseRadius = 0.5f;
            }
        }

        float scale = _radius / baseRadius;
        _meshRenderer.transform.localScale = Vector3.one * scale;
    }

    /// <summary>
    /// Shows the detection sphere mesh.
    /// </summary>
    public void Show()
    {
        SetVisible(true);
    }

    /// <summary>
    /// Hides the detection sphere mesh.
    /// </summary>
    public void Hide()
    {
        SetVisible(false);
    }

    /// <summary>
    /// Toggles the detection sphere mesh on or off.
    /// </summary>
    /// <param name="visible">True to show, false to hide.</param>
    public void Toggle(bool visible)
    {
        SetVisible(visible);
    }

    /// <summary>
    /// Flips the current visibility state.
    /// </summary>
    public void Toggle()
    {
        SetVisible(!IsVisible);
    }

    public bool IsVisible => _meshRenderer != null && _meshRenderer.enabled;

    private void SetVisible(bool visible)
    {
        if (_meshRenderer == null)
        {
            _meshRenderer = GetComponent<MeshRenderer>();
        }

        if (_meshRenderer != null)
        {
            _meshRenderer.enabled = visible;
        }
    }
}
