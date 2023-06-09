using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CurveTypeCreator : EditorWindow
{
    private float xDeltaMin, xDeltaMax, yDeltaMin, yDeltaMax, xVelocityMin, xVelocityMax, slopeMin, slopeMax;
    string curveTypeName = "";
    CurveTypeSettings currentCurveSettings;
    CurveParameters currentCurveParameters;
    GroundSpawner groundSpawner;
    string[] savedCurves = new[] { "Flat", "Bad Curve" };
    int curveIndex = 0;

    [MenuItem("Tools/CurveTypeCreator")]
    public static void ShowWindow()
    {
        GetWindow(typeof(CurveTypeCreator));
    }

    private void OnEnable()
    {
    }

    private void OnGUI()
    {
        GUILayout.Label("Curve Type Creator", EditorStyles.boldLabel);

        curveTypeName = EditorGUILayout.TextField("Curve Type Name", curveTypeName);
        currentCurveSettings = EditorGUILayout.ObjectField("", currentCurveSettings, typeof(CurveTypeSettings), false) as CurveTypeSettings;
        xDeltaMin = EditorGUILayout.FloatField("X Delta Min", xDeltaMin);
        xDeltaMax = EditorGUILayout.FloatField("X Delta Max", xDeltaMax);
        yDeltaMin = EditorGUILayout.FloatField("Y Delta Min", yDeltaMin);
        yDeltaMax = EditorGUILayout.FloatField("Y Delta Max", yDeltaMax);
        xVelocityMin = EditorGUILayout.FloatField("X Velocity Min", xVelocityMin);
        xVelocityMax = EditorGUILayout.FloatField("X Velocity Max", xVelocityMax);
        slopeMin = EditorGUILayout.FloatField("Slope Min", slopeMin);
        slopeMax = EditorGUILayout.FloatField("Slope Max", slopeMax);
        curveIndex = EditorGUILayout.Popup(curveIndex, savedCurves);
        groundSpawner = GameObject.FindGameObjectWithTag("GroundSpawner").GetComponent<GroundSpawner>();
        currentCurveParameters = new(xDeltaMin, xDeltaMax, yDeltaMin, yDeltaMax, xVelocityMin, xVelocityMax, slopeMin, slopeMax);
        if (GUILayout.Button("Generate"))
        {
            GenerateCustomCurve();
        }

    }

    private void GenerateCustomCurve()
    {
        if (groundSpawner == null)
        {
            Debug.LogError("Error: No GroundSpawner assigned to CurveTypeCreatorTool");
            return;
        }

        groundSpawner.GenerateTestSegment(currentCurveParameters);
    }
}
