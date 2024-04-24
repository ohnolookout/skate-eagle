using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum ListSection { Trailing, Leading}
public abstract class PositionalList<T> where T : IPosition
{
    #region Variables
    private protected List<T> _allObjects;
    private protected List<T> _currentObjects;
    private protected float _trailingX, _leadingX;
    private protected int _trailingIndex, _leadingIndex;
    public Action<T, ListSection> OnObjectRemoved, OnObjectAdded;
    private protected Func<float> _updateTrailingX, _updateLeadingX;
    //_trailingIndex is exclusive and _leadingIndex is inclusive.


    public List<T> CurrentObjects { get => _currentObjects; }
    public List<T> AllObjects { get => _allObjects; }
    public int NextTrailingIndex { get => _trailingIndex; }
    public int NextLeadingIndex { get => _leadingIndex; }
    public int CurrentTrailingIndex => _trailingIndex + 1;
    public int CurrentLeadingIndex => _leadingIndex - 1;
    public float TrailingX { get => _trailingX; set => _trailingX = value; }
    public float LeadingX { get => _leadingX; set => _leadingX = value; }
    #endregion

    #region Constructor and Initialization
    public PositionalList(List<T> allObjects, Func<float> updateTrailing, Func<float> updateLeading)
    {
        _allObjects = allObjects;
        _currentObjects = new();
        _updateTrailingX = updateTrailing;
        _updateLeadingX = updateLeading;
        _trailingX = _updateTrailingX();
        _leadingX = _updateLeadingX();

        Initialize();
    }
    public void Initialize()
    {

#if UNITY_EDITOR
        ValidateConstruction();
#endif

        FindInitialValues();
    }
    public void FindInitialValues()
    {
        FindInitialTrailing();
        FindInitialLeading();
    }

    private void FindInitialTrailing()
    {
        _trailingIndex = 0;
        if (NextTrailingPosition().x > _trailingX)
        {
            _trailingIndex = -1;
            return;
        }
        while (NextTrailingIndex < _allObjects.Count - 1 && NextTrailingPosition().x < _trailingX)
        {
            _trailingIndex++;
        }
        if (NextTrailingPosition().x >= _trailingX)
        {
            AddTrailingObject();
        }
    }

    private void FindInitialLeading()
    {
        if (CurrentObjects.Count > 0)
        {
            _leadingIndex = _trailingIndex + 2;
        }
        else
        {
            _leadingIndex = _trailingIndex + 1;
        }
        while (NextLeadingIndex < _allObjects.Count && NextLeadingPosition().x <= _leadingX)
        {
            AddLeadingObject();
        }
    }
    #endregion

    #region Update
    public void Update()
    {
        UpdateTargetXValues();
        UpdateCurrentObjects();
    }

    public void UpdateTargetXValues() 
    {
        _trailingX = _updateTrailingX();
        _leadingX = _updateLeadingX();
    }

    protected bool UpdateCurrentObjects()
    {
        bool objectsChanged = false;

        if (CurrentTrailingIndex < _allObjects.Count && CurrentTrailingPosition().x < _trailingX)
        {
            RemoveTrailingObject();
            objectsChanged = true;
        }
        else if (NextTrailingIndex >= 0 && NextTrailingIndex < _allObjects.Count && NextTrailingPosition().x >= _trailingX)
        {
            AddTrailingObject();
            objectsChanged = true;
        }

        if (CurrentLeadingIndex >= 0 && CurrentLeadingIndex < _allObjects.Count && CurrentLeadingPosition().x > _leadingX)
        {
            RemoveLeadingObject();
            objectsChanged = true;
        }
        else if (NextLeadingIndex < _allObjects.Count && NextLeadingPosition().x <= _leadingX)
        {
            AddLeadingObject();
            objectsChanged = true;
        }

        return objectsChanged;
    }

    public void ChangeUpdateFuncs(Func<float> newTrailing, Func<float> newLeading)
    {
        _updateTrailingX = newTrailing;
        _updateLeadingX = newLeading;

        UpdateTargetXValues();

        bool objectsChanged = true;
        while (objectsChanged)
        {
            objectsChanged = UpdateCurrentObjects();
        }
    }
    #endregion

    #region Add/Remove
    public virtual void AddTrailingObject()
    {
        T addedObj = _allObjects[_trailingIndex];
        _currentObjects.Insert(0, addedObj);
        OnObjectAdded?.Invoke(addedObj, ListSection.Trailing);
        _trailingIndex--;

    }

    public virtual void RemoveTrailingObject()
    {
        OnObjectRemoved?.Invoke(_currentObjects[0], ListSection.Trailing);
        _currentObjects.RemoveAt(0);
        _trailingIndex++;
    }

    public virtual void AddLeadingObject()
    {
        T addedObj = _allObjects[_leadingIndex];
        _currentObjects.Add(addedObj);
        OnObjectAdded?.Invoke(addedObj, ListSection.Leading);
        _leadingIndex++;
    }

    public virtual void RemoveLeadingObject()
    {
        OnObjectRemoved?.Invoke(_currentObjects[_currentObjects.Count - 1], ListSection.Leading);
        _currentObjects.RemoveAt(_currentObjects.Count - 1);
        _leadingIndex--;
    }
    #endregion

    #region Validation
    public void ValidateConstruction()
    {
        if (_allObjects.Count < 1)
        {
            throw new Exception("BoundedObjectList must contain one or more elements");
        }

        if (!ValidateListOrder(_allObjects))
        {
            throw new Exception("Unordered list sent to BoundedObjectList");
        }
    }
    private static bool ValidateListOrder(List<T> toValidate)
    {
        Vector3 lastPosition = toValidate[0].Position;
        foreach (var boundedObj in toValidate)
        {
            if (boundedObj.Position.x < lastPosition.x)
            {
                Debug.Log($"Invalid list order. {boundedObj.Position} comes before {lastPosition}");
                return false;
            }
            lastPosition = boundedObj.Position;
        }
        return true;
    }
    #endregion

    #region Abstract Funcs
    public abstract Vector3 NextLeadingPosition();
    public abstract Vector3 NextTrailingPosition();
    public abstract Vector3 CurrentLeadingPosition();
    public abstract Vector3 CurrentTrailingPosition();
    #endregion

}
