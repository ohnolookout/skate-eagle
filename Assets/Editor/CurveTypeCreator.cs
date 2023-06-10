using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CurveTypeCreator : EditorWindow
{
    private float xDeltaMin, xDeltaMax, lengthToVelocityRatioMin, lengthToVelocityRatioMax, slopeMin, slopeMax;
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
        lengthToVelocityRatioMin = EditorGUILayout.FloatField("Length:Velocity Min", lengthToVelocityRatioMin);
        lengthToVelocityRatioMax = EditorGUILayout.FloatField("Length:Velocity Max", lengthToVelocityRatioMax);
        slopeMin = EditorGUILayout.FloatField("Slope Min", slopeMin);
        slopeMax = EditorGUILayout.FloatField("Slope Max", slopeMax);
        curveIndex = EditorGUILayout.Popup(curveIndex, savedCurves);
        groundSpawner = GameObject.FindGameObjectWithTag("GroundSpawner").GetComponent<GroundSpawner>();
        currentCurveParameters = new(xDeltaMin, xDeltaMax, lengthToVelocityRatioMin, lengthToVelocityRatioMax, slopeMin, slopeMax);
        if (GUILayout.Button("Generate"))
        {
            GenerateCustomCurve();
        }
        if (GUILayout.Button("Generate Min"))
        {
            GenerateFixedCustomCurve(true);
        }
        if (GUILayout.Button("Generate Max"))
        {
            GenerateFixedCustomCurve(false);
        }

    }

    private void GenerateCustomCurve()
    {
        if (groundSpawner == null)
        {
            Debug.LogError("Error: No GroundSpawner assigned to CurveTypeCreatorTool");
            return;
        }

        //groundSpawner.GenerateTestSegment(currentCurveParameters);
    }

    private void GenerateFixedCustomCurve(bool useMinimumParameters)
    {
        if (groundSpawner == null)
        {
            Debug.LogError("Error: No GroundSpawner assigned to CurveTypeCreatorTool");
            return;
        }

        //groundSpawner.GenerateTestSegment(currentCurveParameters);
    }
}
