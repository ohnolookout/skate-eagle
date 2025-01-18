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
        return new LevelSection(sectionToCopy.Name, grade, curves);
    }

    public static CurveDefinition CopyCurveDefinition(CurveDefinition curveToCopy)
    {
        ProceduralCurveSection[] copiedDefs = new ProceduralCurveSection[curveToCopy.Definitions.Length];
        for(int i = 0; i < curveToCopy.Definitions.Length; i++)
        {
            copiedDefs[i] = curveToCopy.Definitions[i];
        }
        return new CurveDefinition(curveToCopy.Name, copiedDefs, curveToCopy.Quantity);
    }

    public static ProceduralCurveSection CopyHalfCurve(ProceduralCurveSection halfCurveToCopy)
    {
        return new ProceduralCurveSection(halfCurveToCopy.LengthType, halfCurveToCopy.ShapeType, halfCurveToCopy.PitchType, halfCurveToCopy.SectionType);
    }

    public static Grade CopyGrade(Grade gradeToCopy)
    {
        return new Grade(gradeToCopy.MinClimb, gradeToCopy.MaxClimb);
    }
}
