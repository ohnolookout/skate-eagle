using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[Serializable]
public class CurveDefinition
{
    [HideInInspector] public string _name;
    public HalfCurveDefinition _valleyDefinition, _peakDefinition;
    public int _quantity, _maxConsecutive;

    public CurveDefinition(string name, HalfCurveDefinition valleyDefinition, HalfCurveDefinition peakDefinition, int quantity = 1, int maxConsecutive = 2)
    {
        _name = name;
        _valleyDefinition = valleyDefinition;
        _peakDefinition = peakDefinition;
        _quantity = quantity;
        _maxConsecutive = 2;
    }

    public CurveDefinition()
    {
        _name = "Default Curve";
        _valleyDefinition = new(LengthType.Medium, ShapeType.Roller, SlopeType.Normal, SkewType.Center);
        _peakDefinition = new(LengthType.Medium, ShapeType.Roller, SlopeType.Normal, SkewType.Center);
        _quantity = 1;
    }

    public void SetName(string newName)
    {
        _name = newName;
    }

    public HalfCurveDefinition[] DefinitionsAsArray
    {
        get
        {
            return new HalfCurveDefinition[2] { _valleyDefinition, _peakDefinition };
        }
    }

    public HalfCurveDefinition Valley
    {
        get
        {
            return _valleyDefinition;
        }
    }

    public HalfCurveDefinition Peak
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
            return _quantity;
        }
        set
        {
            _quantity = value;
        }
    }

    public int Quantity
    {
        get
        {
            return _quantity;
        }
        set
        {
            _quantity = value;
        }
    }

    public int MaxConsecutive
    {
        get
        {
            return _maxConsecutive;
        }
        set
        {
            _maxConsecutive = value;
        }
    }
}
