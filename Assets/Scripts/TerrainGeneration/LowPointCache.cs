using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LowpointCache
{
    private List<Vector3> _points;
    private Vector3 _lowestPoint;

    public LowpointCache()
    {
        _points = new();
        _lowestPoint = new Vector2(0, Single.PositiveInfinity);
    }
    public LowpointCache(List<Vector3> points)
    {
        _points = points;
        _lowestPoint = FindLowestPoint(_points);
    }

    public LowpointCache(List<Curve> curves)
    {
        _points = new();
        foreach(var curve in curves)
        {
            _points.Add(curve.LowPoint);
        }
        _lowestPoint = FindLowestPoint(_points);
    }

    public void AddTrailing(Vector3 point)
    {
        _points.Insert(0, point);
        UpdateLowestPointAfterAddition(point);
    }

    public void AddLeading(Vector3 point)
    {
        _points.Add(point);
        UpdateLowestPointAfterAddition(point);
    }

    public void RemoveTrailing()
    {
        if (_points.Count < 2)
        {
            Debug.LogWarning("Only one point in cache!");
            return;
        }
        Vector3 removedPoint = _points[0];
        _points.RemoveAt(0);
        UpdateLowestPointAfterRemoval(removedPoint);
    }
    public void RemoveLeading()
    {
        if (_points.Count < 2)
        {
            Debug.LogWarning("Only one point in cache!");
            return;
        }
        Vector3 removedPoint = _points[^1];
        _points.RemoveAt(_points.Count - 1);
        UpdateLowestPointAfterRemoval(removedPoint);
    }

    private void UpdateLowestPointAfterRemoval(Vector3 removedPoint)
    {
        if(removedPoint == _lowestPoint)
        {
            _lowestPoint = FindLowestPoint(_points);
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
    public Vector3 LowestPoint { get => _lowestPoint; set => _lowestPoint = value; }
}
