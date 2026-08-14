using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attached to an agent (or child object with a visionCollider trigger) to detect GridObstacles in 3D space.
/// </summary>
public class MobVision : MonoBehaviour
{
    [SerializeField] private Collider _visionCollider;
    [SerializeField] private LayerMask _obstacleLayerMask = ~0;

    private readonly HashSet<GridObstacle> _detectedObstacles = new HashSet<GridObstacle>();
    private readonly Collider[] _overlapBuffer = new Collider[16];

    public Collider VisionCollider => _visionCollider;

    public bool HasObstacleInVision
    {
        get
        {
            // Remove destroyed, disabled, or non-blocking obstacles from trigger set
            _detectedObstacles.RemoveWhere(o => o == null || !o.IsBlocking);

            if (_detectedObstacles.Count > 0)
            {
                return true;
            }

            // Perform 3D physics overlap check as a reliable fallback
            return CheckPhysicsOverlap();
        }
    }

    public IReadOnlyCollection<GridObstacle> DetectedObstacles => _detectedObstacles;

    private void Awake()
    {
        if (_visionCollider == null)
        {
            _visionCollider = GetComponent<Collider>();
        }

        // Ensure a Kinematic Rigidbody exists so Unity 3D trigger events fire
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = GetComponentInParent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
                rb.isKinematic = true;
                rb.useGravity = false;
            }
        }
    }

    private bool CheckPhysicsOverlap()
    {
        if (_visionCollider == null) return false;

        int numColliders = 0;

        if (_visionCollider is BoxCollider box)
        {
            Vector3 center = box.transform.TransformPoint(box.center);
            Vector3 halfExtents = Vector3.Scale(box.size, box.transform.lossyScale) * 0.5f;
            numColliders = Physics.OverlapBoxNonAlloc(center, halfExtents, _overlapBuffer, box.transform.rotation, _obstacleLayerMask, QueryTriggerInteraction.Collide);
        }
        else if (_visionCollider is SphereCollider sphere)
        {
            Vector3 center = sphere.transform.TransformPoint(sphere.center);
            float radius = sphere.radius * Mathf.Max(sphere.transform.lossyScale.x, sphere.transform.lossyScale.y, sphere.transform.lossyScale.z);
            numColliders = Physics.OverlapSphereNonAlloc(center, radius, _overlapBuffer, _obstacleLayerMask, QueryTriggerInteraction.Collide);
        }
        else
        {
            numColliders = Physics.OverlapBoxNonAlloc(_visionCollider.bounds.center, _visionCollider.bounds.extents, _overlapBuffer, Quaternion.identity, _obstacleLayerMask, QueryTriggerInteraction.Collide);
        }

        for (int i = 0; i < numColliders; i++)
        {
            Collider col = _overlapBuffer[i];
            if (col == null || col == _visionCollider) continue;

            // Ignore self and child colliders
            if (col.transform.IsChildOf(transform.root)) continue;

            GridObstacle obstacle = col.GetComponentInParent<GridObstacle>();
            if (obstacle != null && obstacle.IsBlocking)
            {
                _detectedObstacles.Add(obstacle);
                return true;
            }
        }

        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;
        if (other.transform.IsChildOf(transform.root)) return;

        GridObstacle obstacle = other.GetComponentInParent<GridObstacle>();
        if (obstacle != null && obstacle.IsBlocking)
        {
            _detectedObstacles.Add(obstacle);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == null) return;

        GridObstacle obstacle = other.GetComponentInParent<GridObstacle>();
        if (obstacle != null)
        {
            _detectedObstacles.Remove(obstacle);
        }
    }
}


