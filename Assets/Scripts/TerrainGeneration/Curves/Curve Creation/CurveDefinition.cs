using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
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

    public float TotalLength()
    {
        float totalLength = 0;
        foreach (var section in curveSections)
        {
            totalLength += section.length;
        }
        return totalLength;
    }

    public float TotalClimb()
    {
        float totalClimb = 0;
        foreach (var section in curveSections)
        {
            totalClimb += section.climb;
        }
        return totalClimb;
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
*/