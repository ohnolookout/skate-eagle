using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[Serializable]
public class CurveDefinition
{
    [HideInInspector] public string _name;
    public HalfCurveDefinition[] _definitions;
    public int _quantity, _maxConsecutive;


    public CurveDefinition(string name, HalfCurveDefinition[] definitions, int quantity = 1, int maxConsecutive = 2)
    {
        _name = name;
        _definitions = definitions;
        _quantity = quantity;
        _maxConsecutive = maxConsecutive;
    }

    public CurveDefinition()
    {
        _name = "Default Curve";
        HalfCurveDefinition valley = new(LengthType.Medium, ShapeType.Roller, SlopeType.Normal, SkewType.Center);
        HalfCurveDefinition peak = new(LengthType.Medium, ShapeType.Roller, SlopeType.Normal, SkewType.Center);
        _definitions = new[] { valley, peak };
        _quantity = 1;
    }

    public void SetName(string newName)
    {
        _name = newName;
    }

    public HalfCurveDefinition[] Array
    {
        get
        {
            return _definitions;
        }
    }

    public HalfCurveDefinition[] Definitions
    {
        get
        {
            return _definitions;
        }
    }

    public HalfCurveDefinition Valley
    {
        get
        {
            return Definitions[0];
        }
    }

    public HalfCurveDefinition Peak
    {
        get
        {
            return Definitions[1];
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
