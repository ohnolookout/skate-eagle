using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public static class LevelManagement
{
    public static List<string> LevelNames()
    {
        return LevelPathsByName().Keys.ToList();
    }

    public static Dictionary<string, string> LevelPathsByName()
    {
        Dictionary<string, string> levelPathsByName = new();
        string[] levelGuids = AssetDatabase.FindAssets("t:Level", new string[] { "Assets/Levels" });
        string[] _levelPaths = new string[levelGuids.Length];
        for (int i = 0; i < levelGuids.Length; i++)
        {
            _levelPaths[i] = AssetDatabase.GUIDToAssetPath(levelGuids[i]);
        }
        foreach (string path in _levelPaths)
        {
            levelPathsByName[GetFilenameFromPath(path)] = path;
        }
        return levelPathsByName;
    }

    public static string GetFilenameFromPath(string path, string targetChar = "/")
    {
        int lastOccurrenceIndex = path.LastIndexOf(targetChar);

        if (lastOccurrenceIndex == -1)
        {
            // targetChar not found, return the entire path
            return path;
        }
        else
        {
            // Return the section of the path after the last occurrence of targetChar
            return path.Substring(lastOccurrenceIndex + 1);
        }
    }
    /*
    public static ScriptableLevelData2 DeepCopyLevel(ScriptableLevelData2 level)
    {
        string name = level.Name;
        float length = level.Length;
        MedalTimes medalTimes = DeepCopyMedalTimes(level.MedalTimes);
        List<LevelSection> levelSections = new();
        foreach (LevelSection section in level.LevelSections)
        {
            levelSections.Add(DeepCopyLevelSection(section));
        }
        ScriptableLevelData2 deepCopyData = ScriptableObject.CreateInstance<ScriptableLevelData2>();
        deepCopyData.ReassignValues(name, length, medalTimes, levelSections);
        return deepCopyData;
    }

    public static LevelSection DeepCopyLevelSection(LevelSection section)
    {
        GradeData grade = DeepCopyGradeData(section.Grade);
        List<CombinedCurveDefinition> curves = new();
        foreach (CombinedCurveDefinition combinedCurve in section.Curves)
        {
            curves.Add(DeepCopyCombinedCurve(combinedCurve));
        }
        return new LevelSection(section.StartT, grade, curves);
    }

    public static CombinedCurveDefinition DeepCopyCombinedCurve(CombinedCurveDefinition combinedCurve)
    {
        CurveDefinition valley = DeepCopySingleCurve(combinedCurve.Valley);
        CurveDefinition peak = DeepCopySingleCurve(combinedCurve.Peak);
        return new CombinedCurveDefinition(combinedCurve.Name, valley, peak, combinedCurve.Weight);
    }

    public static CurveDefinition DeepCopySingleCurve(CurveDefinition curve)
    {
        return new CurveDefinition(curve.Length, curve.Shape, curve.Slope, curve.Skew);
    }

    public static GradeData DeepCopyGradeData(GradeData gradeData)
    {
        return new GradeData(gradeData.MinClimb, gradeData.MaxClimb);
    }

    public static MedalTimes DeepCopyMedalTimes(MedalTimes medalTimes)
    {
        return new MedalTimes(medalTimes._bronzeTime, medalTimes._silverTime, medalTimes._goldTime, medalTimes._blueTime, medalTimes._redTime);
    }
    */


}
