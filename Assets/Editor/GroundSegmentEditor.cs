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
        Debug.Log("OnEnable");
        _segment = target as GroundSegment;
        _so = new(target);
        _serializedCurve = _so.FindProperty("_curve");
        Debug.Log($"_segment.Curve.Type: {_segment.Curve.Type}");
        /*
        if (_segment.Curve.Type == CurveType.Fixed)
        {
            Debug.Log("Fixed curve found. Assigning curve.");
            _curve = _segment.Curve as FixedCurve;
            _curveSections = new List<FixedCurveSection>();
            foreach (var section in _curve.curveDefinition.curveSections)
            {
                _curveSections.Add(section as FixedCurveSection);
            }
        }*/
    }
    public override void OnInspectorGUI()
    {
        if (_segment.Curve.Type != CurveType.Fixed)
        {
            Debug.Log("Curve is not fixed");
            return;
        }
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(_serializedCurve, true);



        if(EditorGUI.EndChangeCheck())
        {

        }
    }
}
