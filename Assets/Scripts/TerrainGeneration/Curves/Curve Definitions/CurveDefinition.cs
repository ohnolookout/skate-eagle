using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CurveDefinition
{
    public abstract CurveParameters GetCurveParameters(Vector2 initialRightTangent);
}
