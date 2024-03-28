using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public enum ComparisonType { Least, Greatest}
public class MinMaxCache
{
    #region Variables
    private List<Vector3> _points = new();
    private int _currentIndex = 0;
    private Func<float, float, bool> _comparison, 
        _least = ( a, b ) => a <= b, 
        _greatest = (a, b) => a >= b;
    public Action<Vector3> OnNewMinMax;
    private Vector3 _defaultVector = new();
    public Vector3 CurrentPoint
    {
        get
        {
            if (_points.Count > 0)
            {
                return _points[_currentIndex];
            }
            return _defaultVector;
        }
    }
    public int CurrentIndex => _currentIndex;
    public int Length => _points.Count;
    #endregion

    #region Constructors and Initialization
    public MinMaxCache(ComparisonType type)
    {
        SetComparison(type);
    }
    public MinMaxCache(ComparisonType type, List<Vector3> points)
    {
        SetComparison(type);
        _points = points;
        _currentIndex = FindCurrentIndex();
    }

    private void SetComparison(ComparisonType type)
    {
        if (type == ComparisonType.Least)
        {
            _comparison = _least;
        }
        else
        {
            _comparison = _greatest;
        }
    }

    private int FindCurrentIndex()
    {
        if (_points.Count < 1)
        {
            Debug.LogWarning("No lowpoints in cache! Returning 0 index.");
            return 0;
        }

        int index = 0;

        for (int i = 1; i < _points.Count; i++)
        {
            if (_comparison(_points[i].y, _points[index].y))
            {
                index = i;
            }
        }

        return index;
    }
    #endregion

    #region Add/Remove
    public void AddTrailing(Vector3 point)
    {
        _points.Insert(0, point);
        _currentIndex++;
        UpdateIndexAfterAddition(0);
    }

    public void AddLeading(Vector3 point)
    {
        _points.Add(point);
        UpdateIndexAfterAddition(_points.Count - 1);
    }

    public void RemoveTrailing()
    {
        if (_points.Count < 2)
        {
            Debug.LogWarning("Only one point in cache!");
            return;
        }

        _points.RemoveAt(0);

        if(_currentIndex == 0)
        {
            UpdateIndexAfterRemoval(0);
        } else
        {
            _currentIndex--;
        }
    }
    public void RemoveLeading()
    {
        if(_points.Count < 1)
        {
            return;
        } else if (_points.Count < 2)
        {
            Debug.LogWarning("Only one point in cache!");
            _defaultVector = _points[_currentIndex];
        }

        int removedIndex = _points.Count - 1;
        _points.RemoveAt(removedIndex);
        UpdateIndexAfterRemoval(removedIndex);        
    }
    private void UpdateIndexAfterAddition(int addedIndex)
    {
        if(_points.Count == 1)
        {
            _currentIndex = 0;
            OnNewMinMax?.Invoke(_points[_currentIndex]);
        }
        if (_comparison(_points[addedIndex].y, _points[_currentIndex].y))
        {
            _currentIndex = addedIndex;
            OnNewMinMax?.Invoke(_points[_currentIndex]);
        }
    }

    private void UpdateIndexAfterRemoval(int removedIndex)
    {
        if (_currentIndex == removedIndex)
        {
            Debug.Log("Current index removed. Finding new index...");
            _currentIndex = FindCurrentIndex();
            OnNewMinMax?.Invoke(_points[_currentIndex]);
        }
    }
    #endregion
}
