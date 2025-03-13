using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;

/*
[CustomEditor(typeof(Ground))]
public class GroundEditor : Editor
{
    Ground ground;
    public void OnEnable()
    {
        ground = target as Ground;
    }
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Add Segment"))
        {
            ground.AddSegment(new CurveDefinition());
        }
        if(GUILayout.Button("Remove Segment"))
        {
            ground.RemoveSegment();
        }
        if (GUILayout.Button("Insert Segment At Index"))
        {
            ground.InsertSegment(new CurveDefinition(), 8);
        }
        if (GUILayout.Button("Reset Segment List"))
        {
            bool clear = EditorUtility.DisplayDialog("Clear Segment List", "Are you sure you want to clear all segments?", "Yes", "No");
            if (!clear)
            {
                return;
            }
            ground.ResetSegmentList();
        }

        DrawDefaultInspector();
    }
}
*/
