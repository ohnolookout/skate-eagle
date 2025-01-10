using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[Serializable]
public class ProceduralCurveDefinition : CurveDefinition
{
    #region Declarations
    [HideInInspector] public string _name;
    public HalfCurveDefinition[] _definitions;
    public int _quantity, _maxConsecutive;
    public HalfCurveDefinition[] Definitions => _definitions;
    public HalfCurveDefinition Peak => Definitions[1];
    public string Name { get => _name; set => _name = value; }
    public int Quantity { get => _quantity; set => _quantity = value; }
    public int MaxConsecutive { get => _maxConsecutive; set => _maxConsecutive = value; }
    #endregion


    public ProceduralCurveDefinition(string name, HalfCurveDefinition[] definitions, int quantity = 1, int maxConsecutive = 2)
    {
        _name = name;
        _definitions = definitions;
        _quantity = quantity;
        _maxConsecutive = maxConsecutive;
    }

    public ProceduralCurveDefinition()
    {
        _name = "Default Curve";
        HalfCurveDefinition valley = new(LengthType.Medium, ShapeType.Roller, SlopeType.Normal);
        HalfCurveDefinition peak = new(LengthType.Medium, ShapeType.Roller, SlopeType.Normal);
        _definitions = new[] { valley, peak };
        _quantity = 1;
    }

    public void SetName(string newName)
    {
        _name = newName;
    }

    public CurveParameters GetCurveParameters(Vector2 initialRightTangent)
    {

    }
}
