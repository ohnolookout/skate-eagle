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

    public List<CurveDefinition> GenerateSequence()
    {
        Dictionary<CurveDefinition, int> curveQuantities = new();
        int totalCount = 0;
        List<CurveDefinition> possibleCurves = new();
        List<CurveDefinition> sequence = new();
        //Build dictionary of curve quantities to decrement as curves are added as well as a total count of all curves
        //and a list of possible curves that will be adjusted as curves become available or unavailable to generate.
        foreach(CurveDefinition curve in _curves)
        {
            curveQuantities[curve] = curve.Quantity;
            totalCount += curve.Quantity;
            possibleCurves.Add(curve);
        }
        CurveDefinition lastCurve = null;
        int consecCount = 0;
        int currentCount = 0;
        while (totalCount > currentCount)
        {
            CurveDefinition currentCurve = GetNextCurve(curveQuantities, possibleCurves, lastCurve, consecCount, totalCount - currentCount);
            sequence.Add(currentCurve);
            curveQuantities[currentCurve]--;
            if (curveQuantities[currentCurve] <= 0)
            {
                possibleCurves.Remove(currentCurve);
                curveQuantities.Remove(currentCurve);
                consecCount = 0;
            }else if (currentCurve == lastCurve)
            {
                consecCount++;
                if(consecCount > currentCurve.MaxConsecutive)
                {
                    possibleCurves.Remove(currentCurve);
                }
            } else if (!possibleCurves.Contains(lastCurve) && curveQuantities.ContainsKey(lastCurve))
            {
                possibleCurves.Add(lastCurve);
                consecCount = 0;
            }

            lastCurve = currentCurve;
            currentCount++;
            totalCount--;
        }
        return sequence;
    }

    private CurveDefinition GetNextCurve(Dictionary<CurveDefinition, int> curveQuantities, List<CurveDefinition> possibleCurves,
        CurveDefinition lastCurve, int consecCount, int totalRemaining)
    {
        foreach(CurveDefinition curve in possibleCurves)
        {
            if (curveQuantities[curve] <= curve.MaxConsecutive)
            {
                continue;
            }
            int cyclesNeeded;
            int curveQuantity = curveQuantities[curve];
            if(curve == lastCurve)
            {
                cyclesNeeded = Mathf.CeilToInt(1 + (curveQuantity - (curve.MaxConsecutive - consecCount)) / curve.MaxConsecutive);
            }
            else
            {
                cyclesNeeded = Mathf.CeilToInt(curveQuantity / curve.MaxConsecutive);
            }
            if (cyclesNeeded > totalRemaining - curveQuantity)
            {
                return curve;
            } 
        }

        return possibleCurves[random.Next(possibleCurves.Count)];
    }

    //Return list of possible curves for next curve that won't violate max consecutive rules
    private List<CurveDefinition> VetPossibleCurves(Dictionary<CurveDefinition, int> curveQuantities, List<CurveDefinition> possibleCurves, 
        CurveDefinition currentCurve, CurveDefinition lastCurve, int consecCount, int remainingCount)
    {
        int aboveConsecLimit = 0;
        foreach (var curve in possibleCurves)
        {
            if (curveQuantities[curve] <= curve.MaxConsecutive)
            {
                continue;
            }
            aboveConsecLimit++;
            //If more than two curve types have more quantity remaining than their max consecutive curves, return possible curves list.
            if(aboveConsecLimit > 2)
            {
                return possibleCurves;
            }
            if(remainingCount - curveQuantities[curve] < curveQuantities[curve] / curve.MaxConsecutive)
            {
                return new List<CurveDefinition>(){ curve};
            }
            //if remainingCount < curveQuantities[curve]/curve.MaxConsecutive, return list of only this curve.
        }

        return null;
    }

    public void CacheValidSections()
    {
        if (!Validate())
        {
            throw new Exception("Section must contain at least one curve type");
        }
        if (_curves.Count == 1)
        {
            _cachedSequences = new();
            Sequence newSequence = new();
            for (int i = 0; i < _curves[0].Quantity; i++)
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
        if (_cachedSequences.Count < 1)
        {
            throw new Exception("No valid sequences exist with current parameters.");
        }
    }

    public void LogSectionCache()
    {
        Debug.Log($"Logging {_cachedSequences.Count} sections");
        for (int i = 0; i < _cachedSequences.Count; i++)
        {
            Debug.Log($"Sequence {i}:");
            foreach (CurveDefinition curve in _cachedSequences[i].Curves)
            {
                Debug.Log($"{curve.Name}");
            }
        }
    }

    public void LogCurveList(List<CurveDefinition> curveList)
    {
        string curveNames = "Curves: ";
        foreach(CurveDefinition curve in curveList)
        {
            curveNames += curve.Name + " ";
        }
        Debug.Log(curveNames);
    }
    public void Log()
    {
        Validate();
        Debug.Log($"Logging section {_name}...");
        string curveNames = "Curves: ";
        foreach (CurveDefinition curve in _curves)
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

    public List<Sequence> Sequences
    {
        get
        {
            return _cachedSequences;
        }
    }

    public Sequence RandomCurveSequence
    {
        get
        {
            LogCurveList(GenerateSequence());
            if (_cachedSequences.Count > 0)
            {
                return _cachedSequences[random.Next(_cachedSequences.Count)];
            }
            CacheValidSections();
            if (_cachedSequences.Count > 0)
            {
                return _cachedSequences[random.Next(_cachedSequences.Count)];
            }
            Debug.Log("No sequences cached!");
            return null;
        }
    }

}
