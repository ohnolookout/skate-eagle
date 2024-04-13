using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionalEdgeCollider : IDoublePosition, IPosition
{
    Vector3 _startPosition, _endPosition;
    EdgeCollider2D _collider;

    public PositionalEdgeCollider(EdgeCollider2D collider)
    {
        _collider = collider;
        _startPosition = _collider.points[0];
        _endPosition = _collider.points[^1];
    }

    public Vector3 Position => _startPosition;

    public Vector3 StartPosition => _startPosition;

    public Vector3 EndPosition => _endPosition;

    public EdgeCollider2D Collider => _collider;
}
