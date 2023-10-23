using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DeepCopy
{
    public static Level CopyLevel(Level levelToCopy)
    {
        string name = levelToCopy.Name;
        MedalTimes medalTimes = CopyMedalTimes(levelToCopy.MedalTimes);
        List<LevelSection> levelSections = CopyLevelSections(levelToCopy.LevelSections);
        Level newLevel = ScriptableObject.CreateInstance<Level>();
        newLevel.ReassignValues(name, medalTimes, levelSections);
        return newLevel;
    }

    public static MedalTimes CopyMedalTimes(MedalTimes medalTimes)
    {
        return new MedalTimes(medalTimes.Bronze, medalTimes.Silver, medalTimes.Gold, medalTimes.Blue, medalTimes.Red);

    }

    public static List<LevelSection> CopyLevelSections(List<LevelSection> sectionsToCopy)
    {
        List<LevelSection> sections = new();
        foreach (LevelSection section in sectionsToCopy)
        {
            sections.Add(CopySection(section));
        }
        return sections;
    }

    public static LevelSection CopySection(LevelSection sectionToCopy)
    {
        Grade grade = CopyGrade(sectionToCopy.Grade);
        List<CurveDefinition> curves = new();
        List<Sequence> sequencesToCache = new();
        foreach (CurveDefinition curve in sectionToCopy.Curves)
        {
            curves.Add(CopyCurveDefinition(curve));
        }
        foreach (Sequence cachedSequence in sectionToCopy.Sequences)
        {
            Sequence newSequence = new();
            foreach (CurveDefinition curve in cachedSequence.Curves)
            {
                newSequence.Add(CopyCurveDefinition(curve));
            }
            sequencesToCache.Add(newSequence);
        }
        return new LevelSection(sectionToCopy.Name, grade, curves, sequencesToCache);
    }

    public static CurveDefinition CopyCurveDefinition(CurveDefinition curveToCopy)
    {
        HalfCurveDefinition valley = CopyHalfCurve(curveToCopy.Valley);
        HalfCurveDefinition peak = CopyHalfCurve(curveToCopy.Peak);
        return new CurveDefinition(curveToCopy.Name, valley, peak, curveToCopy.Quantity);
    }

    public static HalfCurveDefinition CopyHalfCurve(HalfCurveDefinition halfCurveToCopy)
    {
        return new HalfCurveDefinition(halfCurveToCopy.Length, halfCurveToCopy.Shape, halfCurveToCopy.Slope, halfCurveToCopy.Skew);
    }

    public static Grade CopyGrade(Grade gradeToCopy)
    {
        return new Grade(gradeToCopy.MinClimb, gradeToCopy.MaxClimb);
    }
}
