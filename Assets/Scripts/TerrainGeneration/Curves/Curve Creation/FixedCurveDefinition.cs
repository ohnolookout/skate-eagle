using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedCurveDefinition : CurveDefinition
{
    public FixedCurveDefinition(FixedCurveSection[] definitions)
    {
        curveSections = definitions;
    }
    public FixedCurveDefinition()
    {
        name = "Default Curve";
        FixedCurveSection peak = new(SectionType.Peak);
        FixedCurveSection valley = new(SectionType.Valley);
        curveSections = new[] { peak, valley };
    }
}
