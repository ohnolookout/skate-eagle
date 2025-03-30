using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StraightCurveSection : ICurveSection
{
    public CurveSectionType CurveType => CurveSectionType.Straight;
    public List<CurvePoint> GetCurvePoints()
    {
        throw new System.NotImplementedException();
    }
}
