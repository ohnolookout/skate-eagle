using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
/*
public static class SectionCache
{
    private static bool IsWithinRepetitionLimits(Sequence sequence, Dictionary<ProceduralCurveDefinition, int> maxRepetitions)
    {
        ProceduralCurveDefinition lastCurve = sequence.Curves[0];
        int repetitions = 1;
        for (int i = 1; i < sequence.Curves.Count; i++)
        {
            ProceduralCurveDefinition currentCurve = sequence.Curves[i];
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

    private static bool IsWithinRepetitionLimits(List<ProceduralCurveDefinition> sequence, Dictionary<ProceduralCurveDefinition, int> maxRepetitions)
    {
        ProceduralCurveDefinition lastCurve = sequence[0];
        int repetitions = 1;
        for (int i = 1; i < sequence.Count; i++)
        {
            ProceduralCurveDefinition currentCurve = sequence[i];
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

    public static bool ValidateCurveList(List<ProceduralCurveDefinition> sequence, List<ProceduralCurveDefinition> possibleCurves)
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

    private static bool AchievesAllQuantities(List<ProceduralCurveDefinition> sequence, Dictionary<ProceduralCurveDefinition, int> quantityDict)
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

    public static Dictionary<ProceduralCurveDefinition, int> MaxRepetitionDict(List<ProceduralCurveDefinition> curves)
    {
        Dictionary<ProceduralCurveDefinition, int> maxRepetitions = new();
        foreach (var curve in curves)
        {
            maxRepetitions[curve] = curve.MaxConsecutive;
        }
        return maxRepetitions;
    }

    public static Dictionary<ProceduralCurveDefinition, int> MaxRepetitionDict(LevelSection section)
    {
        Dictionary<ProceduralCurveDefinition, int> maxRepetitions = new();
        foreach (ProceduralCurveDefinition curve in section.Curves)
        {
            maxRepetitions[curve] = curve.MaxConsecutive;
        }
        return maxRepetitions;
    }

    public static Dictionary<ProceduralCurveDefinition, int> CurveQuantityDict(List<ProceduralCurveDefinition> curves, out int curveCount)
    {
        Dictionary<ProceduralCurveDefinition, int> curveQuantities = new();
        curveCount = 0;
        foreach (ProceduralCurveDefinition curve in curves)
        {
            curveQuantities[curve] = curve.Quantity;
            curveCount += curve.Quantity;
        }
        return curveQuantities;
    }
}
*/