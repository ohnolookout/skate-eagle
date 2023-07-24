using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[Serializable]
public class CombinedCurveDefinition
{
    public string _name;
    public CurveDefinition _valleyDefinition, _peakDefinition;
    public int _weight;

    public CombinedCurveDefinition(string name, CurveDefinition valleyDefinition, CurveDefinition peakDefinition, int weight = 1)
    {
        _name = name;
        _valleyDefinition = valleyDefinition;
        _peakDefinition = peakDefinition;
        _weight = weight;
    }

    public CombinedCurveDefinition()
    {
        _name = "Default Curve";
        _valleyDefinition = new(LengthType.Medium, ShapeType.Roller, SlopeType.Normal, SkewType.Center);
        _peakDefinition = new(LengthType.Medium, ShapeType.Roller, SlopeType.Normal, SkewType.Center);
        _weight = 1;
    }

    public void SetName(string newName)
    {
        _name = newName;
        Debug.Log($"Name changed to {_name}");
    }

    public CurveDefinition[] DefinitionsAsArray
    {
        get
        {
            return new CurveDefinition[2] { _valleyDefinition, _peakDefinition };
        }
    }

    public CurveDefinition Valley
    {
        get
        {
            return _valleyDefinition;
        }
    }

    public CurveDefinition Peak
    {
        get
        {
            return _peakDefinition;
        }
    }

    public string Name
    {
        get
        {
            return _name;
        }
        set
        {
            _name = value;
        }
    }

    public int Weight
    {
        get
        {
            return _weight;
        }
        set
        {
            _weight = value;
        }
    }

}
