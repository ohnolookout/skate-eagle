using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionalMinMax<T> where T : IPosition
{
    private PositionalList<T> _positionalList;
    private MinMaxCache _minMaxCache;
    public PositionalMinMax(PositionalList<T> positionalList, ComparisonType comparisonType)
    {
        _positionalList = positionalList;
        List<Vector3> currentPoints = new();
        foreach(var positionObject in _positionalList.CurrentObjects)
        {
            currentPoints.Add(positionObject.Position);
        }

        _minMaxCache = new(comparisonType, currentPoints);
        _positionalList.OnObjectAdded += ParseAddedObject;
        _positionalList.OnObjectRemoved += ParseRemovedObject;

    }

    public void Update()
    {
        _positionalList.Update();
    }

    private void ParseRemovedObject(T removedObject, ListSection section)
    {
        if(section == ListSection.Trailing)
        {
            _minMaxCache.RemoveTrailing();
        }
        else
        {
            _minMaxCache.RemoveLeading();
        }
            
    }

    private void ParseAddedObject(T addedObject, ListSection section)
    {
        if (section == ListSection.Trailing)
        {
            _minMaxCache.AddTrailing(addedObject.Position);
        }
        else
        {
            _minMaxCache.AddLeading(addedObject.Position);
        }
    }

    public MinMaxCache MinMax { get => _minMaxCache; }
    public PositionalList<T> PositionList { get => _positionalList; }
}
