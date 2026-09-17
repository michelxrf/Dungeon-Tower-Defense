using UnityEngine;

public class LevelCameraSetup : MonoBehaviour
{

    [SerializeField] private Transform _topLeftCorner;
    [SerializeField] private Transform _topRightCorner;
    [SerializeField] private Transform _bottomLeftCorner;
    [SerializeField] private Transform _bottomRightCorner;

    [SerializeField] private float margin = 1.05f;
    
    private Camera _targetCamera;

    private void Awake()
    {
        _targetCamera = GetComponent<Camera>();
    }

    private void Start()
    {
        FitCameraToLevel();
    }

    private void FitCameraToLevel()
    {
        Vector3[] points =
        {
            _topLeftCorner.position,
            _topRightCorner.position,
            _bottomLeftCorner.position,
            _bottomRightCorner.position
        };

        Bounds bounds = new Bounds(points[0], Vector3.zero);

        foreach (Vector3 point in points)
            bounds.Encapsulate(point);

        // Centraliza a câmera no level
        Vector3 cameraPosition = _targetCamera.transform.position;

        cameraPosition.x = _bottomLeftCorner.position.x;
        cameraPosition.y = _bottomLeftCorner.position.x * 1.5f;
        cameraPosition.z = bounds.center.z;

        _targetCamera.transform.position = cameraPosition;

        // Calcula o tamanho necessário
        float sizeByHeight = bounds.size.z / 3f;
        float sizeByWidth = bounds.size.x / (3f * _targetCamera.aspect);

        _targetCamera.orthographicSize =
            Mathf.Max(sizeByHeight, sizeByWidth) * margin;
    }
}