using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class CurveDefinition
{
    public List<ICurveSection> curveSections;
    public string name;

}
