using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomCurveSection : ICurveSection
{
    public CurveSectionType CurveType => CurveSectionType.Custom;
    public List<CurvePoint> GetCurvePoints(CurvePoint startPoint)
    {
        throw new System.NotImplementedException();
    }
}
