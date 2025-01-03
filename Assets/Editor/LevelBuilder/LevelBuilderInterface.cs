using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class LevelBuilderInterface : EditorWindow
{
    private LevelTerrain _terrain;


    [MenuItem("Tools/Level Builder")]
    public static void ShowWindow()
    {
        GetWindow<LevelBuilderInterface>();
    }
    private void OnEnable()
    {
        _terrain = FindFirstObjectByType<LevelTerrain>();
    }
    private void OnGUI()
    {
        GUILayout.Label("Level Builder", EditorStyles.boldLabel);
        var addCurveButton = GUILayout.Button("Add Curve", GUILayout.ExpandWidth(false));
        var addStartLineButton = GUILayout.Button("Add Start Line", GUILayout.ExpandWidth(false));
        if (addCurveButton)
        {
            AddCurve();
        }

    }


    private void AddStartLine()
    {
        Debug.Log("Adding start");
        TerrainGenerator.GenerateStartSegment(_terrain, new(0,0));
    }

    private void AddCurve()
    {
        Debug.Log("Adding curve");
        var startPoint = new CurvePoint(new(0, 0), new(-1, 1), new(1, -1));
        var newCurve = CurveFactory.CurveFromDefinition(new CurveDefinition(), startPoint, -1, 0);
        TerrainGenerator.GenerateCompleteSegment(_terrain, newCurve);
    }
}
