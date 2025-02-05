using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class ICurveSection
{
    public abstract CurveSectionParameters GetSectionParameters(Vector2 prevTangent);
}
