using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GroundSegment))]
public class GroundSegmentEditor: Editor
{
    GroundSegment _segment;
    Curve _curve;
    List<CurveSection> _curveSections;
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



        if(EditorGUI.EndChangeCheck())
        {

        }
    }
}
