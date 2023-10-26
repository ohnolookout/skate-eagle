using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class SectionCache
{
    private static bool IsWithinRepetitionLimits(Sequence sequence, Dictionary<CurveDefinition, int> maxRepetitions)
    {
        CurveDefinition lastCurve = sequence.Curves[0];
        int repetitions = 1;
        for (int i = 1; i < sequence.Curves.Count; i++)
        {
            CurveDefinition currentCurve = sequence.Curves[i];
            if (currentCurve != lastCurve)
            {
                lastCurve = currentCurve;
                repetitions = 1;
                continue;
            }
            repetitions++;
            if (repetitions > maxRepetitions[currentCurve])
            {
                return false;
            }
        }
        return true;
    }

    private static bool IsWithinRepetitionLimits(List<CurveDefinition> sequence, Dictionary<CurveDefinition, int> maxRepetitions)
    {
        CurveDefinition lastCurve = sequence[0];
        int repetitions = 1;
        for (int i = 1; i < sequence.Count; i++)
        {
            CurveDefinition currentCurve = sequence[i];
            if (currentCurve != lastCurve)
            {
                lastCurve = currentCurve;
                repetitions = 1;
                continue;
            }
            repetitions++;
            if (repetitions > maxRepetitions[currentCurve])
            {
                return false;
            }
        }
        return true;
    }

    public static bool ValidateSection(LevelSection section)
    {
        return ValidateCurveList(section.GenerateSequence().Curves, section.Curves);
    }

    public static bool ValidateCurveList(List<CurveDefinition> sequence, List<CurveDefinition> possibleCurves)
    {
        var quantityDict = CurveQuantityDict(possibleCurves, out var curveCount);
        if(!AchievesAllQuantities(sequence, quantityDict))
        {
            Debug.Log("Sequence fails to achieve correct quantity of curves.");
            return false;
        }
        var repetitionDict = MaxRepetitionDict(possibleCurves);
        if(!IsWithinRepetitionLimits(sequence, repetitionDict))
        {
            Debug.Log("Sequence has too many repetitions of curve.");
            return false;
        }
        Debug.Log("Sequence is valid.");
        return true;
    }

    private static bool AchievesAllQuantities(List<CurveDefinition> sequence, Dictionary<CurveDefinition, int> quantityDict)
    {
        foreach(var curve in sequence)
        {
            quantityDict[curve]--;
            if(quantityDict[curve] < 0)
            {
                return false;
            }
        }
        foreach(var entry in quantityDict)
        {
            if(entry.Value < 0)
            {
                return false;
            }
        }

        return true;
    }

    public static Dictionary<CurveDefinition, int> MaxRepetitionDict(List<CurveDefinition> curves)
    {
        Dictionary<CurveDefinition, int> maxRepetitions = new();
        foreach (var curve in curves)
        {
            maxRepetitions[curve] = curve.MaxConsecutive;
        }
        return maxRepetitions;
    }

    public static Dictionary<CurveDefinition, int> MaxRepetitionDict(LevelSection section)
    {
        Dictionary<CurveDefinition, int> maxRepetitions = new();
        foreach (CurveDefinition curve in section.Curves)
        {
            maxRepetitions[curve] = curve.MaxConsecutive;
        }
        return maxRepetitions;
    }

    public static Dictionary<CurveDefinition, int> CurveQuantityDict(List<CurveDefinition> curves, out int curveCount)
    {
        Dictionary<CurveDefinition, int> curveQuantities = new();
        curveCount = 0;
        foreach (CurveDefinition curve in curves)
        {
            curveQuantities[curve] = curve.Quantity;
            curveCount += curve.Quantity;
        }
        return curveQuantities;
    }
}
