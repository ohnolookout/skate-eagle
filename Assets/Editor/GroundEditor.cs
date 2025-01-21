using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;

[CustomEditor(typeof(Ground))]
public class GroundEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var ground = target.GetComponent<Ground>();
        if (GUILayout.Button("Add Segment"))
        {
            ground.AddSegment();
        }
    }
}
