using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CurveSection
{
    public abstract CurveSectionParameters GetSectionParameters(Vector2 prevTangent);
}
