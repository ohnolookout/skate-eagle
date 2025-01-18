using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[Serializable]
public class CurveDefinition
{
    #region Declarations
    [HideInInspector] public string _name;
    public ProceduralCurveSection[] _definitions;
    public int _quantity, _maxConsecutive;
    public ProceduralCurveSection[] Definitions => _definitions;
    public ProceduralCurveSection Peak => Definitions[1];
    public string Name { get => _name; set => _name = value; }
    public int Quantity { get => _quantity; set => _quantity = value; }
    public int MaxConsecutive { get => _maxConsecutive; set => _maxConsecutive = value; }
    #endregion


    public CurveDefinition(string name, ProceduralCurveSection[] definitions, int quantity = 1, int maxConsecutive = 2)
    {
        _name = name;
        _definitions = definitions;
        _quantity = quantity;
        _maxConsecutive = maxConsecutive;
    }

    public CurveDefinition()
    {
        _name = "Default Curve";
        ProceduralCurveSection valley = new(LengthType.Medium, ShapeType.Roller, PitchType.Normal, SectionType.Peak);
        ProceduralCurveSection peak = new(LengthType.Medium, ShapeType.Roller, PitchType.Normal, SectionType.Valley);
        _definitions = new[] { valley, peak };
        _quantity = 1;
    }

    public void SetName(string newName)
    {
        _name = newName;
    }
}
