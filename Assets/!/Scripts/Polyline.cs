using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a path made of straight line segments between a set of points.
/// </summary>
public class Polyline
{
    private readonly Vector3[] _points;
    private readonly List<float> _pointDistances = new List<float>();
    private float _totalLength;

    public Vector3[] Points => _points;
    public IReadOnlyList<float> PointDistances => _pointDistances;
    public float TotalLength => _totalLength;

    public Polyline(Vector3[] points)
    {
        if (points == null || points.Length == 0)
        {
            _points = Array.Empty<Vector3>();
            _totalLength = 0f;
            return;
        }

        _points = points;
        BakePath();
    }

    private void BakePath()
    {
        _pointDistances.Clear();
        _totalLength = 0f;

        if (_points.Length == 0)
            return;

        // First point is always at distance 0.
        _pointDistances.Add(0f);

        for (int i = 1; i < _points.Length; i++)
        {
            _totalLength += Vector3.Distance(
                _points[i - 1],
                _points[i]
            );

            _pointDistances.Add(_totalLength);
        }
    }

    /// <summary>
    /// Gets a position along the polyline by normalized distance (0 to 1).
    /// Movement is distributed according to the actual length of the path.
    /// </summary>
    public Vector3 Evaluate(float normalizedDistance)
    {
        if (_points.Length == 0)
            return Vector3.zero;

        if (_points.Length == 1)
            return _points[0];

        normalizedDistance = Mathf.Clamp01(normalizedDistance);

        return EvaluateDistance(normalizedDistance * _totalLength);
    }

    /// <summary>
    /// Gets a position along the polyline by absolute distance from the start.
    /// </summary>
    public Vector3 EvaluateDistance(float distance)
    {
        if (_points.Length == 0)
            return Vector3.zero;

        if (_points.Length == 1)
            return _points[0];

        if (_totalLength <= 0f)
            return _points[0];

        distance = Mathf.Clamp(distance, 0f, _totalLength);

        // Find the segment containing the requested distance.
        for (int i = 0; i < _pointDistances.Count - 1; i++)
        {
            float segmentStart = _pointDistances[i];
            float segmentEnd = _pointDistances[i + 1];

            if (distance <= segmentEnd)
            {
                float segmentLength = segmentEnd - segmentStart;

                float t = segmentLength > 0.0001f
                    ? (distance - segmentStart) / segmentLength
                    : 0f;

                return Vector3.Lerp(
                    _points[i],
                    _points[i + 1],
                    t
                );
            }
        }

        return _points[_points.Length - 1];
    }

    /// <summary>
    /// Returns the distance along the polyline corresponding to a point index.
    /// </summary>
    public float GetPointDistance(int pointIndex)
    {
        if (_pointDistances.Count == 0)
            return 0f;

        if (pointIndex <= 0)
            return 0f;

        if (pointIndex >= _points.Length)
            return _totalLength;

        return _pointDistances[pointIndex];
    }
}