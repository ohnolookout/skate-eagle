using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[Serializable]
public class CurveDefinition
{
    #region Declarations
    [HideInInspector] public string _name;
    public CurveSection[] _curveSections;
    public int _quantity, _maxConsecutive;
    public CurveSection[] Definitions => _curveSections;
    public CurveSection Peak => Definitions[1];
    public string Name { get => _name; set => _name = value; }
    public int Quantity { get => _quantity; set => _quantity = value; }
    public int MaxConsecutive { get => _maxConsecutive; set => _maxConsecutive = value; }
    #endregion


    public CurveDefinition(string name, CurveSection[] definitions, int quantity = 1, int maxConsecutive = 2)
    {
        _name = name;
        _curveSections = definitions;
        _quantity = quantity;
        _maxConsecutive = maxConsecutive;
    }

    public CurveDefinition()
    {
        _name = "Default Curve";
        ProceduralCurveSection valley = new(LengthType.Medium, ShapeType.Roller, PitchType.Normal, SectionType.Peak);
        ProceduralCurveSection peak = new(LengthType.Medium, ShapeType.Roller, PitchType.Normal, SectionType.Valley);
        _curveSections = new[] { valley, peak };
        _quantity = 1;
    }

    public void SetName(string newName)
    {
        _name = newName;
    }

    public void MapGradeToCurveDefs(Grade grade)
    {
        foreach (var definition in _curveSections)
        {
            definition.AddGrade(grade);
        }
    }
}
