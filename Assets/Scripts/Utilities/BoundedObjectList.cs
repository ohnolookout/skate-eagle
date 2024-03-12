using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BoundedObjectList<T>
{

    private List<BoundedObject<T>> _allObjects;
    private List<T> _currentObjects;
    private Transform _trackedObject;
    private int _trailingDistance, _leadingDistance, _trailingIndex, _leadingIndex, _everyXFrames;
    public Action<T> OnTrailingObjectRemoved, OnTrailingObjectAdded, OnLeadingObjectRemoved, OnLeadingObjectAdded;
    //Trailing and leading indices mark the indices of the first and last objects in the _currentObjects list.

    public BoundedObjectList(List<BoundedObject<T>> allObjects, Transform trackedObject, int trailingDistance, int leadingDistance, int everyXFrames = 1)
    {
        _allObjects = allObjects;
        _trackedObject = trackedObject;
        _trailingDistance = trailingDistance;
        _leadingDistance = leadingDistance;
        _everyXFrames = everyXFrames;

#if UNITY_EDITOR
        if (!ValidateListOrder(_allObjects))
        {
            throw new Exception("Unordered list sent to BoundedObjectList");
        }

        if (_allObjects.Count < 1)
        {
            throw new Exception("BoundedObjectList must contain one or more elements");
        }
#endif

        FindInitialValues();
    }
    public void Update()
    {
        if (_everyXFrames == 1)
        {
            UpdateCurrentObjects();
        } else if(Time.frameCount % _everyXFrames == 0)
        {
            UpdateCurrentObjects();
        }
    }

    private void FindInitialValues()
    {
        float trailingX = _trackedObject.position.x - _trailingDistance;
        float leadingX = _trackedObject.position.x + _leadingDistance;

        _trailingIndex = 0;

        while (_trailingIndex < _allObjects.Count - 1 && _allObjects[_trailingIndex].Position.x < trailingX)
        {
            _trailingIndex++;
        }

        _leadingIndex = _trailingIndex;

        while(_leadingIndex < _allObjects.Count - 1 && _allObjects[_leadingIndex].Position.x < leadingX)
        {
            AddLeadingObject();
        }
    }

    //Could potentially cache this for player since it will be the same for high and low vals
    private void UpdateCurrentObjects()
    {
        float trailingX = _trackedObject.position.x - _trailingDistance;
        float leadingX = _trackedObject.position.x + _leadingDistance;

        if (_allObjects[_trailingIndex].Position.x < trailingX)
        {
            RemoveTrailingObject();
        } else if (_trailingIndex >= 0 && _allObjects[_trailingIndex - 1].Position.x > trailingX)
        {
            AddTrailingObject();
        }

        if (_allObjects[_leadingIndex].Position.x > leadingX)
        {
            RemoveLeadingObject();
        }
        else if (_leadingIndex < _allObjects.Count - 1 && _allObjects[_leadingIndex + 1].Position.x < leadingX)
        {
            AddLeadingObject();
        }
    }

    private void AddTrailingObject()
    {
        _trailingIndex--;
        T addedObj = _allObjects[_trailingIndex].Object;
        _currentObjects.Insert(0, addedObj);
        OnTrailingObjectAdded?.Invoke(addedObj);
        
    }

    private void RemoveTrailingObject()
    {
        _trailingIndex++;
        OnTrailingObjectRemoved?.Invoke(_currentObjects[0]);
        _currentObjects.RemoveAt(0);
    }

    private void AddLeadingObject()
    {
        _leadingIndex++;
        T addedObj = _allObjects[_leadingIndex].Object;
        _currentObjects.Add(addedObj);
        OnLeadingObjectAdded?.Invoke(addedObj);
    }

    private void RemoveLeadingObject()
    {
        _leadingIndex--;
        OnLeadingObjectRemoved?.Invoke(_currentObjects[_currentObjects.Count - 1]);
        _currentObjects.RemoveAt(_currentObjects.Count - 1);
    }

    private static bool ValidateListOrder(List<BoundedObject<T>> toValidate)
    {
        float lastX = toValidate[0].Position.x;
        foreach(var boundedObj in toValidate)
        {
            if(boundedObj.Position.x < lastX)
            {
                return false;
            }
            lastX = boundedObj.Position.x;
        }
        return true;
    }

}
