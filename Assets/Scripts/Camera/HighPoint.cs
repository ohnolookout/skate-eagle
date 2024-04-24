using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct HighPoint
{
    private Vector3 _high, _trailingLow, _leadingLow;
    private float _distance;
    public Vector3 High => _high;
    public Vector3 TrailingLow => _trailingLow;
    public Vector3 LeadingLow => _leadingLow;
    public float Distance => _distance;

    public HighPoint(Vector3 highPoint, Vector3 trailing, Vector3 leading)
    {
        _high = highPoint;
        _trailingLow = trailing;
        _leadingLow = leading;
        _distance = Mathf.Max(_high.y - _trailingLow.y, _high.y - _leadingLow.y);
    }
}
