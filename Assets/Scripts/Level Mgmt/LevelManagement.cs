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
        string[] levelGuids = AssetDatabase.FindAssets("t:ScriptableLevelData2", new string[] { "Assets/Levels" });
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

}
