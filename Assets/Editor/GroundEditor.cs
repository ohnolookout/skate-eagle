using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;

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
        if(GUILayout.Button("Remove Segment At Index"))
        {
            ground.RemoveSegment(8);
        }
        if (GUILayout.Button("Insert Segment At Index"))
        {
            ground.InsertSegment(new CurveDefinition(), 8);
        }
    }
}
