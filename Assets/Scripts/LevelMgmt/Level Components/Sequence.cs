using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Sequence
{
    public List<CurveDefinition> _curves;

    public Sequence()
    {
        _curves = new();
    }
    public void Add(CurveDefinition curve)
    {
        _curves.Add(curve);
    }

    public List<CurveDefinition> Curves
    {
        get
        {
            return _curves;
        }
        set
        {
            _curves = value;
        }
    }
}
