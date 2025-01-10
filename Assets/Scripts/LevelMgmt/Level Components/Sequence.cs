using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Sequence
{
    public List<ProceduralCurveDefinition> _curves;

    public Sequence()
    {
        _curves = new();
    }
    public void Add(ProceduralCurveDefinition curve)
    {
        _curves.Add(curve);
    }

    public List<ProceduralCurveDefinition> Curves
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
