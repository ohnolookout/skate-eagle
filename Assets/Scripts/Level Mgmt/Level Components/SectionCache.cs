using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class SectionCache
{
    public static List<CurveDefinition> DeepCopyCurveList(List<CurveDefinition> curveList)
    {
        List<CurveDefinition> copiedCurves = new();
        foreach (CurveDefinition combinedCurve in curveList)
        {
            copiedCurves.Add(combinedCurve.DeepCopy());
        }

        return copiedCurves;

    }

    public static List<Sequence> AllValidSections(Dictionary<CurveDefinition, int> curveQuantities, int remainingCurveCount)
    {
        if (remainingCurveCount == 1)
        {
            return ReturnRemainingCurves(curveQuantities.Keys.ToList());
        }
        List<Sequence> combinedBranches = new();
        foreach (CurveDefinition curve in curveQuantities.Keys.ToList())
        {
            if (curveQuantities[curve] <= 0) continue;

            List<Sequence> receivedBranch = CreateNewBranch(curve);
            combinedBranches = combinedBranches.Union(receivedBranch).ToList();
        }

        return combinedBranches;


        List<Sequence> ReturnRemainingCurves(List<CurveDefinition> curves)
        {
            List<Sequence> combinedBranches = new();
            foreach (CurveDefinition curve in curves)
            {
                if (curveQuantities[curve] <= 0) continue;

                Sequence newBranch = new();
                newBranch.Add(curve);
                combinedBranches.Add(newBranch);
            }
            return combinedBranches;
        }

        List<Sequence> CreateNewBranch(CurveDefinition curve)
        {
            curveQuantities[curve] -= 1;
            List<Sequence> receivedBranches = AllValidSections(curveQuantities, remainingCurveCount - 1);
            curveQuantities[curve] += 1;
            foreach (Sequence receivedBranch in receivedBranches)
            {
                receivedBranch.Curves.Insert(0, curve);
            }
            return receivedBranches;
        }
    }

    public static List<Sequence> TrimForMaxRepetitions(List<Sequence> listOfSequences, Dictionary<CurveDefinition, int> maxRepetitions)
    {
        List<Sequence> trimmedSequences = new();
        foreach (Sequence sequence in listOfSequences)
        {
            if (IsWithinRepetitionLimits(sequence, maxRepetitions))
            {
                trimmedSequences.Add(sequence);
            }
        }
        return trimmedSequences;
    }

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
