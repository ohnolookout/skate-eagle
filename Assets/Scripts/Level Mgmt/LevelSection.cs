using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[Serializable]
public class LevelSection
{
    [HideInInspector]
    public string _name;
    public float _startT;
    public GradeData _grade;
    public List<CombinedCurveDefinition> _curves = new();

    public LevelSection()
    {
        _startT = 0;
        _name = "0%";
        _grade = new();
        _curves.Add(new CombinedCurveDefinition());
    }

    public LevelSection(float startT, GradeData grade, List<CombinedCurveDefinition> curves)
    {
        _startT = startT;
        _name = $"{(_startT * 100)}%";
        _grade = grade;
        _curves = curves;
    }

    public List<CombinedCurveDefinition> Curves
    {
        get
        {
            return _curves;
        }
    }

    public float StartT
    {
        get
        {
            return _startT;
        }
    }

    public GradeData Grade
    {
        get
        {
            return _grade;
        }
    }
}
