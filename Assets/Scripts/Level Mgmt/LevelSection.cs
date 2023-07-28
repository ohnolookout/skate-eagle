using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[Serializable]
public class LevelSection
{
    [HideInInspector] public string _name;
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
        set
        {
            _startT = value;
        }
    }

    public GradeData Grade
    {
        get
        {
            return _grade;
        }
        set
        {
            _grade = value;
        }
    }

    public string Name
    {
        get
        {
            return _name;
        }
        set
        {
            _name = value;
        }
    }

    public bool Validate()
    {
        Name = $"{(StartT * 100)}%";
        if (StartT >= 1 || StartT < 0)
        {
            return false;
        }
        if (Curves.Count < 1)
        {
            return false;
        }
        return true;
    }

    public LevelSection DeepCopy()
    {
        GradeData grade = _grade.DeepCopy();
        List<CombinedCurveDefinition> curves = new();
        foreach (CombinedCurveDefinition combinedCurve in _curves)
        {
            curves.Add(combinedCurve.DeepCopy());
        }
        return new LevelSection(_startT, grade, curves);
    }
}
