using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class LevelBuilderInterface : EditorWindow
{
    private Terrain _terrain;


    [MenuItem("Tools/Level Builder")]
    public static void ShowWindow()
    {
        GetWindow<LevelBuilderInterface>();
    }
    private void OnEnable()
    {
        _terrain = FindFirstObjectByType<Terrain>();
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
        List<FixedHalfCurve> halfCurves = new()
        {
            new(),
            new()
        };
        halfCurves[0].Type = HalfCurveType.Valley;
        //_terrain.GenerateCompleteSegment(halfCurves);
    }
}
