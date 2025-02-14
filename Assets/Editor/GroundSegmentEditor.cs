using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/*
[CustomEditor(typeof(GroundSegment))]
public class GroundSegmentEditor: Editor
{
    GroundSegment _segment;
    SerializedObject _so;
    SerializedProperty _serializedCurve;
    public void OnEnable()
    {
        _segment = target as GroundSegment;
        _so = new(target);
        _serializedCurve = _so.FindProperty("_curve");
    }
    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(_serializedCurve, true);

        _so.ApplyModifiedProperties();
        _so.Update();

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RegisterFullObjectHierarchyUndo(_segment, "Curve Change");
            _segment.RefreshCurve();
            _segment.parentGround.RecalculateSegments(_segment);
        }
        
        if (GUILayout.Button("Duplicate"))
        {
            _segment.parentGround.DuplicateSegment(_segment);
        }

        if (GUILayout.Button("Reset"))
        {
            _segment.Reset();
        }

        if (GUILayout.Button("Delete"))
        {
            _segment.Delete();
            return;
        }

        DrawDefaultInspector();
    }

}
*/