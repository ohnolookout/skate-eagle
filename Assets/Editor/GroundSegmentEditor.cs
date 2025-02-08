using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GroundSegment))]
public class GroundSegmentEditor: Editor
{
    GroundSegment _segment;
    SerializedObject _so;
    SerializedProperty _serializedCurve;
    CurveDefinition _curveDef;
    public void OnEnable()
    {
        _segment = target as GroundSegment;
        _curveDef = _segment.Curve.curveDefinition;
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
            _segment.RefreshCurve();
            _segment.TriggerGroundRecalculation(); 
        }

        if (GUILayout.Button("Delete"))
        {
            _segment.Delete();
            return;
        }

        DrawDefaultInspector();
    }

}
