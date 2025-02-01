using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using static System.Collections.Specialized.BitVector32;
[Serializable]
public class LevelSection
{
    public string _name;
    public Grade _grade;
    public List<ProceduralCurveDefinition> _curves = new();
    private static System.Random random = new();

    public LevelSection()
    {
        _name = "New Section";
        _grade = new();
        _curves.Add(new ProceduralCurveDefinition());
    }

    public LevelSection(string name, Grade grade, List<ProceduralCurveDefinition> curves)
    {
        _name = name;
        _grade = grade;
        _curves = curves;
    }

    public bool Validate()
    {
        foreach (ProceduralCurveDefinition curve in Curves)
        {
            NameCurve(curve);
        }
        if (Curves.Count < 1)
        {
            return false;
        }
        return true;
    }

    private void NameCurve(ProceduralCurveDefinition curve)
    {
        string name = "Custom curve";

        if (curve.curveSections[0].GetType() == typeof(ProceduralCurveSection))
        {
            var proceduralSection = (ProceduralCurveSection)curve.curveSections[0];
            name = $"{proceduralSection.LengthType} {proceduralSection.PitchType} {proceduralSection.ShapeType}";
        }

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
        Dictionary<ProceduralCurveDefinition, int> curveQuantities = new();
        List<ProceduralCurveDefinition> possibleCurves = new();
        //Build dictionary of curve quantities to decrement as curves are added as well as a total count of all curves
        //and a list of possible curves that will be adjusted as curves become available or unavailable to generate.
        foreach(ProceduralCurveDefinition curve in _curves)
        {
            curveQuantities[curve] = curve.Quantity;
            totalCount += curve.Quantity;
            possibleCurves.Add(curve);
        }
        ProceduralCurveDefinition lastCurve = new ProceduralCurveDefinition();
        int consecCount = 1;
        int currentCount = 0;
        while (totalCount > currentCount)
        {
            ProceduralCurveDefinition currentCurve = GetNextCurve(curveQuantities, possibleCurves, lastCurve, consecCount, totalCount - currentCount);
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

    private ProceduralCurveDefinition GetNextCurve(Dictionary<ProceduralCurveDefinition, int> curveQuantities, List<ProceduralCurveDefinition> possibleCurves,
        ProceduralCurveDefinition lastCurve, int consecCount, int totalRemaining)
    {
        foreach(ProceduralCurveDefinition curve in possibleCurves)
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

    public void LogCurveList(List<ProceduralCurveDefinition> curveList)
    {
        string curveNames = "Curves: ";
        foreach(ProceduralCurveDefinition curve in curveList)
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
        foreach (ProceduralCurveDefinition curve in _curves)
        {
            curveNames += curve.Name + " ";
        }
        Debug.Log(curveNames);
    }

    public List<ProceduralCurveDefinition> Curves => _curves;
    public Grade Grade { get => _grade; set => _grade = value; }
    public string Name { get => _name; set => _name = value; }
}
