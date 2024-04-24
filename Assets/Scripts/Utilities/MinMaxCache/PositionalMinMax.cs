using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionalMinMax<T> where T : IPosition, ISortable
{
    private PositionalList<T> _positionalList;
    private MinMax<T> _minMax;
    public MinMax<T> MinMax => _minMax;
    public PositionalList<T> PositionList { get => _positionalList; }
    public T CurrentMinMax => _minMax.CurrentValue;
    public PositionalMinMax(PositionalList<T> positionalList, ComparisonType comparisonType)
    {
        _positionalList = positionalList;
        List<T> inboundObjects = new();
        foreach(var inboundObject in _positionalList.CurrentObjects)
        {
            inboundObjects.Add(inboundObject);
        }

        _minMax = new(comparisonType, inboundObjects);
        _positionalList.OnObjectAdded += ParseAddedObject;
        _positionalList.OnObjectRemoved += ParseRemovedObject;

    }

    public void Update()
    {
        _positionalList.Update();
    }

    private void ParseRemovedObject(T removedObject, ListSection section)
    {
        if (section == ListSection.Trailing)
        {
            _minMax.RemoveTrailing();
        }
        else
        {
            _minMax.RemoveLeading();
        }
            
    }

    private void ParseAddedObject(T addedObject, ListSection section)
    {
        if (section == ListSection.Trailing)
        {
            _minMax.AddTrailing(addedObject);
        }
        else
        {
            _minMax.AddLeading(addedObject);
        }
    }
}
