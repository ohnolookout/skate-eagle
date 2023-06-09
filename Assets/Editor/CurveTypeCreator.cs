using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CurveTypeCreator : EditorWindow
{

    string curveTypeName = "";
    CurveParameters parameters;
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
        parameters = EditorGUILayout.ObjectField("", parameters, typeof(CurveParameters), false) as CurveParameters;
        /*parameters.xDeltaMin = EditorGUILayout.FloatField("X Delta Min", parameters.xDeltaMin);
        parameters.xDeltaMax = EditorGUILayout.FloatField("X Delta Max", parameters.xDeltaMax);
        parameters.yDeltaMin = EditorGUILayout.FloatField("Y Delta Min", parameters.yDeltaMin);
        parameters.yDeltaMax = EditorGUILayout.FloatField("Y Delta Max", parameters.yDeltaMax);
        parameters.xVelocityMin = EditorGUILayout.FloatField("X Velocity Min", parameters.xVelocityMin);
        parameters.xVelocityMax = EditorGUILayout.FloatField("X Velocity Max", parameters.xVelocityMax);
        parameters.slopeMin = EditorGUILayout.FloatField("Slope Min", parameters.slopeMin);
        parameters.slopeMax = EditorGUILayout.FloatField("Slope Min", parameters.slopeMax);*/
        curveIndex = EditorGUILayout.Popup(curveIndex, savedCurves);
        //groundSpawner = GameObject.FindGameObjectWithTag("GroundSpawner").GetComponent<GroundSpawner>();

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

        //groundSpawner.GenerateTestSegment(parameters);
    }
}
