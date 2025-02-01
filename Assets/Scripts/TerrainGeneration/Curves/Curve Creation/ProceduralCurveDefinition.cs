using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[Serializable]
public class ProceduralCurveDefinition : CurveDefinition
{
    #region Declarations
    public int _quantity, _maxConsecutive;
    public new ProceduralCurveSection[] curveSections;
    public ProceduralCurveSection[] Sections => curveSections;
    public string Name { get => name; set => name = value; }
    public int Quantity { get => _quantity; set => _quantity = value; }
    public int MaxConsecutive { get => _maxConsecutive; set => _maxConsecutive = value; }
    #endregion


    public ProceduralCurveDefinition(string name, ProceduralCurveSection[] definitions, int quantity = 1, int maxConsecutive = 2)
    {
        this.name = name;
        curveSections = definitions;
        _quantity = quantity;
        _maxConsecutive = maxConsecutive;
    }

    public ProceduralCurveDefinition()
    {
        name = "Default Curve";
        ProceduralCurveSection peak = new(LengthType.Medium, ShapeType.Roller, PitchType.Normal, SectionType.Peak);
        ProceduralCurveSection valley = new(LengthType.Medium, ShapeType.Roller, PitchType.Normal, SectionType.Valley);
        curveSections = new[] { peak, valley };
        _quantity = 1;
    }

    public void SetName(string newName)
    {
        name = newName;
    }

    public void MapGradeToCurveDefs(Grade grade)
    {
        foreach (ProceduralCurveSection definition in curveSections)
        {
            definition.AddGrade(grade);
        }
    }
}
