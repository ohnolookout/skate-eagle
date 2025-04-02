using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public static class DeepCopy
{
    /*
    public static CurveDefinition CopyCurveDefinition(CurveDefinition defToCopy)
    {
        List<CurveSection> copiedSections = new();

        foreach(var sectionToCopy in defToCopy.curveSections)
        {
            copiedSections.Add(CopyCurveSection(sectionToCopy));
        }

        return new CurveDefinition(copiedSections);
    }
    */

    public static List<ICurveSection> CopyCurveSectionList(List<ICurveSection> sectionsToCopy)
    {
        List<ICurveSection> copiedSections = new();
        foreach (ICurveSection sectionToCopy in sectionsToCopy)
        {
            copiedSections.Add(CopyCurveSection(sectionToCopy));
        }
        return copiedSections;
    }

    public static ICurveSection CopyCurveSection(ICurveSection sectionToCopy)
    {
        switch (sectionToCopy.CurveType)
        {
            case CurveSectionType.Straight:
                return new StraightCurveSection();
            case CurveSectionType.Custom:
                return new CustomCurveSection();
            case CurveSectionType.Standard:
                return CopyStandardCurveSection(sectionToCopy as StandardCurveSection);
            default:
                return new StandardCurveSection(CurveDirection.Valley);
        }
    }

    private static StandardCurveSection CopyStandardCurveSection(StandardCurveSection sectionToCopy)
    {
        return new(sectionToCopy.XYDelta, sectionToCopy.Height, sectionToCopy.Skew, sectionToCopy.Shape, sectionToCopy.Type);
    }

    public static CurveSection CopyCurveSection(CurveSection sectionToCopy)
    {
        return new(sectionToCopy.sectionType, sectionToCopy.length, sectionToCopy.shape, sectionToCopy.pitch, sectionToCopy.climb);
    }
    /*
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
            List<ProceduralCurveDefinition> curves = new();
            List<Sequence> sequencesToCache = new();
            foreach (ProceduralCurveDefinition curve in sectionToCopy.Curves)
            {
                curves.Add(CopyCurveDefinition(curve));
            }
            return new LevelSection(sectionToCopy.Name, grade, curves);
        }

        public static ProceduralCurveDefinition CopyCurveDefinition(ProceduralCurveDefinition curveToCopy)
        {
            List<ProceduralCurveSection> copiedDefs = new();
            for(int i = 0; i < curveToCopy.curveSections.Count; i++)
            {
                copiedDefs.Add(curveToCopy.curveSections[i] as ProceduralCurveSection);
            }
            return new ProceduralCurveDefinition(curveToCopy.Name, copiedDefs, curveToCopy.Quantity);
        }

        public static ProceduralCurveSection CopyHalfCurve(ProceduralCurveSection halfCurveToCopy)
        {
            return new ProceduralCurveSection(halfCurveToCopy.LengthType, halfCurveToCopy.ShapeType, halfCurveToCopy.PitchType, halfCurveToCopy.SectionType);
        }

        public static Grade CopyGrade(Grade gradeToCopy)
        {
            return new Grade(gradeToCopy.ClimbMin, gradeToCopy.ClimbMax);
        }
        */
}