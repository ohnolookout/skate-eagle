using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
[Serializable]
public class LevelSection
{
    public string _name;
    public Grade _grade;
    public List<CurveDefinition> _curves = new();
    [HideInInspector] public List<Sequence> _cachedSequences = new();
    private static System.Random random = new();

    public LevelSection()
    {
        _name = "New Section";
        _grade = new();
        _curves.Add(new CurveDefinition());
    }

    public LevelSection(string name, Grade grade, List<CurveDefinition> curves)
    {
        _name = name;
        _grade = grade;
        _curves = curves;
    }

    public LevelSection(string name, Grade grade, List<CurveDefinition> curves, List<Sequence> cachedSequences)
    {
        _name = name;
        _grade = grade;
        _curves = curves;
        _cachedSequences = cachedSequences;
    }


    public bool Validate()
    {
        foreach (CurveDefinition curve in Curves)
        {
            NameCurve(curve);
        }
        if (Curves.Count < 1)
        {
            return false;
        }
        return true;
    }

    private void NameCurve(CurveDefinition curve)
    {
        string name = $"{curve.Peak.Length} {curve.Peak.Slope} {curve.Peak.Shape} {curve.Peak.Skew}";
        curve.Name = name;
    }

    public LevelSection DeepCopy()
    {
        Grade grade = _grade.DeepCopy();
        List<CurveDefinition> curves = new();
        List<Sequence> sequencesToCache = new();
        foreach (CurveDefinition curve in _curves)
        {
            curves.Add(curve.DeepCopy());
        }
        foreach (Sequence cachedSequence in _cachedSequences)
        {
            Sequence newSequence = new();
            foreach(CurveDefinition curve in cachedSequence.Curves)
            {
                newSequence.Add(curve.DeepCopy());
            }
            sequencesToCache.Add(newSequence);
        }
        return new LevelSection(_name, grade, curves, sequencesToCache);
    }

    public void CacheValidSections()
    {
        if(!Validate())
        {
            throw new Exception("Section must contain at least one curve type");
        }
        if(_curves.Count == 1)
        {
            _cachedSequences = new();
            Sequence newSequence = new();
            for(int i = 0; i < _curves[0].Quantity; i++)
            {
                newSequence.Add(_curves[0]);
            }
            _cachedSequences.Add(newSequence);
        }
        Dictionary<CurveDefinition, int> curveQuantities = SectionCache.CurveQuantityDict(_curves, out int curveCount);
        _cachedSequences = SectionCache.AllValidSections(curveQuantities, curveCount);
        if (_curves.Count > 1)
        {
            Dictionary<CurveDefinition, int> maxRepetitions = SectionCache.MaxRepetitionDict(this);
            _cachedSequences = SectionCache.TrimForMaxRepetitions(_cachedSequences, maxRepetitions);
        }
        if(_cachedSequences.Count < 1)
        {
            throw new Exception("No valid sequences exist with current parameters.");
        }
    }

    public void LogSectionCache()
    {
        Debug.Log($"Logging {_cachedSequences.Count} sections");
        for(int i = 0; i < _cachedSequences.Count; i++)
        {
            Debug.Log($"Sequence {i}:");
            foreach(CurveDefinition curve in _cachedSequences[i].Curves)
            {
                Debug.Log($"{curve.Name}");
            }
        }
    }

    public void Log()
    {
        Validate();
        Debug.Log($"Logging section {_name}...");
        string curveNames = "Curves: ";
        foreach(CurveDefinition curve in _curves)
        {
            curveNames += curve.Name + " ";
        }
        Debug.Log(curveNames);
    }

    public List<CurveDefinition> Curves
    {
        get
        {
            return _curves;
        }
    }


    public Grade Grade
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

    public Sequence RandomCurveSequence
    {
        get
        {
            return _cachedSequences[random.Next(_cachedSequences.Count)];
        }
    }

}
