using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoublePositionObject<T> : IPosition, IDoublePosition
{
    private Vector3 _position, _startPosition, _endPosition;
    private T _value;
    public DoublePositionObject(T value, Vector3 position, Vector3 startPosition, Vector3 endPosition)
    {
        _value = value;
        _position = position;
        _startPosition = startPosition;
        _endPosition = endPosition;
    }

    public DoublePositionObject(T value, Vector3 startPosition, Vector3 endPosition)
    {
        _value = value;
        _position = startPosition;
        _startPosition = startPosition;
        _endPosition = endPosition;
    }

    public Vector3 Position { get => _position; set => _position = value; }
    public Vector3 StartPosition => _startPosition;
    public Vector3 EndPosition => _endPosition;
    public T Value => _value;
}
