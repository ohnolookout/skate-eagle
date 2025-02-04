using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FixedCurveDefinition : CurveDefinition
{
    public FixedCurveDefinition(List<FixedCurveSection> definitions)
    {
        curveSections = new();
        foreach (var def in definitions)
        {
            curveSections.Add(def);
        }
    }
    public FixedCurveDefinition()
    {
        name = "Default Curve";
        FixedCurveSection peak = new(SectionType.Peak);
        FixedCurveSection valley = new(SectionType.Valley);
        curveSections = new List<ICurveSection> { peak, valley };
    }

}
