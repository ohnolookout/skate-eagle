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

    public Sequence GenerateSequence()
    {
        Sequence sequence = new();
        if (Curves.Count == 1)
        {
            for(int i = 0; i < Curves[0].Quantity; i++)
            {
                sequence.Add(Curves[0]);
            }
            return sequence;
        }
        int totalCount = 0;
        Dictionary<CurveDefinition, int> curveQuantities = new();
        List<CurveDefinition> possibleCurves = new();
        //Build dictionary of curve quantities to decrement as curves are added as well as a total count of all curves
        //and a list of possible curves that will be adjusted as curves become available or unavailable to generate.
        foreach(CurveDefinition curve in _curves)
        {
            curveQuantities[curve] = curve.Quantity;
            totalCount += curve.Quantity;
            possibleCurves.Add(curve);
        }
        CurveDefinition lastCurve = new CurveDefinition();
        int consecCount = 1;
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
            }
            if (currentCurve == lastCurve)
            {
                consecCount++;
                if (consecCount >= currentCurve.MaxConsecutive)
                {
                    possibleCurves.Remove(currentCurve);
                }
            } else if (curveQuantities.ContainsKey(lastCurve) && !possibleCurves.Contains(lastCurve))
            {
                possibleCurves.Add(lastCurve);
                consecCount = 1;
            }
            lastCurve = currentCurve;
            currentCount++;
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
                //Debug.Log($"{curve.Name} is part of consecutive chain. Adjusting cycles needed.");
                cyclesNeeded = Mathf.CeilToInt(1 + (curveQuantity - ((float)curve.MaxConsecutive - consecCount)) / curve.MaxConsecutive);
            }
            else
            {
                cyclesNeeded = Mathf.CeilToInt((float)curveQuantity / curve.MaxConsecutive);
            }
            //Debug.Log($"Cycles needed for {curve.Name} to expend quantity {curveQuantity}: {cyclesNeeded}");
            if (cyclesNeeded > totalRemaining - curveQuantity)
            {
                return curve;
            } 
        }
        if (possibleCurves.Count > 0)
        {
            return possibleCurves[random.Next(possibleCurves.Count)];
        }
        else if (curveQuantities.Count > 0)
        {
            Debug.Log("!!!!Ran out of possible curves!!!!");
            return curveQuantities.Keys.ToList()[0];
        }
        Debug.Log("!!!!Ran out of all curves!!!!");
        return null;
    }

    public void NextCurveTest()
    {
        HalfCurveDefinition valley = new(LengthType.Medium, ShapeType.Roller, SlopeType.Normal, SkewType.Center);
        HalfCurveDefinition peak1 = new(LengthType.Short, ShapeType.HardPeak, SlopeType.Steep, SkewType.Center);
        HalfCurveDefinition peak2 = new(LengthType.Medium, ShapeType.Roller, SlopeType.Normal, SkewType.Center);
        HalfCurveDefinition peak3 = new(LengthType.Long, ShapeType.SoftTable, SlopeType.Gentle, SkewType.Center);
        CurveDefinition curve1 = new("curve1", valley, peak1, 5, 2);
        CurveDefinition curve2 = new("curve2", valley, peak2, 1, 1);
        CurveDefinition curve3 = new("curve3", valley, peak2, 1, 1);
        CurveDefinition lastCurve = new();
        int consecCount = 0;

        List<CurveDefinition> possibleCurves = new() { curve1, curve2, curve3 };
        Dictionary<CurveDefinition, int> curveQuantities = new();
        int totalRemaining = 0;

        foreach (CurveDefinition curve in possibleCurves)
        {
            curveQuantities[curve] = curve.Quantity;
            totalRemaining += curve.Quantity;
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

}
