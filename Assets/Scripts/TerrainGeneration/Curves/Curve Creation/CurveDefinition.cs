using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CurveDefinition
{
    public List<CurveSection> curveSections;
    public string name;
    public CurveDefinition(List<CurveSection> definitions)
    {
        name = "Default Curve";
        curveSections = new();
        foreach (var def in definitions)
        {
            curveSections.Add(def);
        }
    }
    public CurveDefinition()
    {
        name = "Default Curve";
        CurveSection peak = new(SectionType.Peak);
        CurveSection valley = new(SectionType.Valley);
        curveSections = new List<CurveSection> { peak, valley };
    }

    public void Log()
    {
        Debug.Log($"~~~Curve Definition~~~");
        Debug.Log($"Name: {name}");
        foreach (var section in curveSections)
        {
            section.Log();
        }
    }

}
