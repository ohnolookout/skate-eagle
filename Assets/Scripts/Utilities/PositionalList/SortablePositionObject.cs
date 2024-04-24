using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SortablePositionObject<T> : IPosition, ISortable
{
    private Vector3 _position;
    private T _value;
    private float _sortKey;
    public SortablePositionObject(T value, Vector3 position, float sortKey)
    {
        _value = value;
        _position = position;
        _sortKey = sortKey;
    }
    public Vector3 Position => _position;
    public T Value => _value;
    public float SortKey { get => _sortKey;}
}
