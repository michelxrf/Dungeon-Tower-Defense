using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A Catmull-Rom spline representation created from a set of control points.
/// </summary>
public class CatmullRomSpline
{
    private readonly Vector3[] _controlPoints;
    private readonly List<Vector3> _sampledPoints = new List<Vector3>();
    private readonly List<float> _sampledDistances = new List<float>();
    private float _totalLength;

    public Vector3[] ControlPoints => _controlPoints;
    public IReadOnlyList<Vector3> SampledPoints => _sampledPoints;
    public float TotalLength => _totalLength;

    public CatmullRomSpline(Vector3[] controlPoints, int resolutionPerSegment = 10)
    {
        if (controlPoints == null || controlPoints.Length == 0)
        {
            _controlPoints = Array.Empty<Vector3>();
            _totalLength = 0f;
            return;
        }

        _controlPoints = controlPoints;
        BakeSpline(resolutionPerSegment);
    }

    private void BakeSpline(int resolutionPerSegment)
    {
        _sampledPoints.Clear();
        _sampledDistances.Clear();
        _totalLength = 0f;

        if (_controlPoints.Length == 1)
        {
            _sampledPoints.Add(_controlPoints[0]);
            _sampledDistances.Add(0f);
            return;
        }

        int count = _controlPoints.Length;
        for (int i = 0; i < count - 1; i++)
        {
            Vector3 p0 = (i == 0) ? _controlPoints[0] - (_controlPoints[1] - _controlPoints[0]) : _controlPoints[i - 1];
            Vector3 p1 = _controlPoints[i];
            Vector3 p2 = _controlPoints[i + 1];
            Vector3 p3 = (i + 2 < count) ? _controlPoints[i + 2] : _controlPoints[i + 1] + (_controlPoints[i + 1] - _controlPoints[i]);

            int steps = (i == count - 2) ? resolutionPerSegment + 1 : resolutionPerSegment;
            for (int step = 0; step < steps; step++)
            {
                float t = (float)step / resolutionPerSegment;
                Vector3 point = EvaluateCatmullRom(p0, p1, p2, p3, t);

                if (_sampledPoints.Count > 0)
                {
                    float dist = Vector3.Distance(_sampledPoints[_sampledPoints.Count - 1], point);
                    _totalLength += dist;
                }

                _sampledPoints.Add(point);
                _sampledDistances.Add(_totalLength);
            }
        }
    }

    /// <summary>
    /// Returns the sampled distance along the spline corresponding to control point at controlPointIndex.
    /// </summary>
    public float GetControlPointDistance(int controlPointIndex, int resolutionPerSegment = 10)
    {
        if (_sampledDistances == null || _sampledDistances.Count == 0) return 0f;
        if (controlPointIndex <= 0) return 0f;
        if (_controlPoints == null || controlPointIndex >= _controlPoints.Length) return _totalLength;

        int sampleIndex = controlPointIndex * resolutionPerSegment;
        if (sampleIndex < _sampledDistances.Count)
        {
            return _sampledDistances[sampleIndex];
        }
        return _totalLength;
    }

    public static Vector3 EvaluateCatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        return 0.5f * (
            (2f * p1) +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t3
        );
    }

    /// <summary>
    /// Gets a position along the spline by normalized time t (0 to 1).
    /// </summary>
    public Vector3 Evaluate(float normalizedTime)
    {
        if (_sampledPoints.Count == 0) return Vector3.zero;
        if (_sampledPoints.Count == 1) return _sampledPoints[0];

        float targetDistance = Mathf.Clamp01(normalizedTime) * _totalLength;
        return EvaluateDistance(targetDistance);
    }

    /// <summary>
    /// Gets a position along the spline by absolute distance from start.
    /// </summary>
    public Vector3 EvaluateDistance(float distance)
    {
        if (_sampledPoints.Count == 0) return Vector3.zero;
        if (_sampledPoints.Count == 1) return _sampledPoints[0];

        distance = Mathf.Clamp(distance, 0f, _totalLength);

        for (int i = 0; i < _sampledDistances.Count - 1; i++)
        {
            if (distance <= _sampledDistances[i + 1])
            {
                float d0 = _sampledDistances[i];
                float d1 = _sampledDistances[i + 1];
                float segmentLength = d1 - d0;
                float t = segmentLength > 0.0001f ? (distance - d0) / segmentLength : 0f;
                return Vector3.Lerp(_sampledPoints[i], _sampledPoints[i + 1], t);
            }
        }

        return _sampledPoints[_sampledPoints.Count - 1];
    }
}

