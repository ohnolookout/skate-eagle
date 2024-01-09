using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LowpointCache
{
    private List<Vector3> _lowPoints;
    private Vector3 _lowestPoint;

    public LowpointCache()
    {
        _lowPoints = new();
        _lowestPoint = new Vector2(0, Single.PositiveInfinity);
    }
    public LowpointCache(List<Vector3> points)
    {
        _lowPoints = points;
        _lowestPoint = FindLowestPoint(_lowPoints);
    }

    public LowpointCache(List<Curve> curves)
    {
        _lowPoints = new();
        foreach(var curve in curves)
        {
            _lowPoints.Add(curve.LowPoint);
        }
        _lowestPoint = FindLowestPoint(_lowPoints);
    }

    public void AddTrailing(Vector3 point)
    {
        _lowPoints.Insert(0, point);
        UpdateLowestPointAfterAddition(point);
    }

    public void AddLeading(Vector3 point)
    {
        _lowPoints.Add(point);
        UpdateLowestPointAfterAddition(point);
    }

    public void RemoveTrailing()
    {
        if (_lowPoints.Count < 2)
        {
            Debug.LogWarning("Only one point in cache!");
            return;
        }
        Vector3 removedPoint = _lowPoints[0];
        _lowPoints.RemoveAt(0);
        UpdateLowestPointAfterRemoval(removedPoint);
    }
    public void RemoveLeading()
    {
        if (_lowPoints.Count < 2)
        {
            Debug.LogWarning("Only one point in cache!");
            return;
        }
        Vector3 removedPoint = _lowPoints[^1];
        _lowPoints.RemoveAt(_lowPoints.Count - 1);
        UpdateLowestPointAfterRemoval(removedPoint);
    }

    private void UpdateLowestPointAfterRemoval(Vector3 removedPoint)
    {
        if(removedPoint == _lowestPoint)
        {
            _lowestPoint = FindLowestPoint(_lowPoints);
        }
    }
    private void UpdateLowestPointAfterAddition(Vector3 addedPoint)
    {
        if (addedPoint.y <= _lowestPoint.y)
        {
            _lowestPoint = addedPoint;
        }
    }

    private static Vector3 FindLowestPoint(List<Vector3> points)
    {
        if(points.Count < 1)
        {
            Debug.LogWarning("No lowpoints in cache!");
            return new();
        }
        Vector3 lowPoint = points[0];
        for(int i = 1; i < points.Count; i++)
        {
            if(points[i].y < lowPoint.y)
            {
                lowPoint = points[i];
            }
        }
        return lowPoint;
    }

    public List<Vector3> LowPoints { get => _lowPoints; set => _lowPoints = value; }
    public Vector3 LowestPoint { get => _lowestPoint; set => _lowestPoint = value; }
}
