using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class MinMax<T> where T : ISortable
{
    #region Variables
    private List<T> _sortables = new();
    private int _currentIndex = 0;
    private bool _removeLastPointOnDelay = false;
    private Func<float, float, bool> _comparison,
        _least = (a, b) => a <= b,
        _greatest = (a, b) => a >= b;
    public Action<T> OnNewMinMax;
    public T CurrentValue => _sortables[_currentIndex];
    public int CurrentIndex => _currentIndex;
    public int Length => _sortables.Count;
    #endregion

    #region Constructors and Initialization
    public MinMax(ComparisonType type, List<T> sortables)
    {
        SetComparison(type);
        _sortables = sortables;
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
        if (_sortables.Count < 1)
        {
            Debug.LogWarning("No lowpoints in cache! Returning 0 index.");
            return 0;
        }

        int index = 0;

        for (int i = 1; i < _sortables.Count; i++)
        {
            if (_comparison(_sortables[i].SortKey, _sortables[index].SortKey))
            {
                index = i;
            }
        }

        return index;
    }
    #endregion

    #region Add/Remove
    public void AddTrailing(T newItem)
    {
        if (_removeLastPointOnDelay && _sortables.Count == 1)
        {
            _removeLastPointOnDelay = false;
            _sortables.RemoveAt(0);
        }
        _sortables.Insert(0, newItem);
        _currentIndex++;
        UpdateIndexAfterAddition(0);
    }

    public void AddLeading(T newItem)
    {
        if (_removeLastPointOnDelay && _sortables.Count == 1)
        {
            _removeLastPointOnDelay = false;
            _sortables.RemoveAt(0);
        }
        _sortables.Add(newItem);
        UpdateIndexAfterAddition(_sortables.Count - 1);
    }

    public void RemoveTrailing()
    {
        if (_sortables.Count < 2)
        {
            _removeLastPointOnDelay = true;
            return;
        }

        _sortables.RemoveAt(0);

        if (_currentIndex == 0)
        {
            UpdateIndexAfterRemoval(0);
        }
        else
        {
            _currentIndex--;
        }
    }
    public void RemoveLeading()
    {
        if (_sortables.Count < 2)
        {
            _removeLastPointOnDelay = true;
            return;
        }
        int removedIndex = _sortables.Count - 1;
        _sortables.RemoveAt(removedIndex);
        UpdateIndexAfterRemoval(removedIndex);
    }
    private void UpdateIndexAfterAddition(int addedIndex)
    {
        if (_sortables.Count == 1)
        {
            _currentIndex = 0;
            OnNewMinMax?.Invoke(_sortables[_currentIndex]);
        }
        if (_comparison(_sortables[addedIndex].SortKey, _sortables[_currentIndex].SortKey))
        {
            _currentIndex = addedIndex;
            OnNewMinMax?.Invoke(_sortables[_currentIndex]);
        }
    }

    private void UpdateIndexAfterRemoval(int removedIndex)
    {
        if (_currentIndex == removedIndex)
        {
            _currentIndex = FindCurrentIndex();
            OnNewMinMax?.Invoke(_sortables[_currentIndex]);
        }
    }
    #endregion
}
