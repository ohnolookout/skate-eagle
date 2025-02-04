using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(IGroundSegment))]
public class IGroundSegmentEditor: Editor
{
    IGroundSegment segment;
    FixedCurve curve;
    List<FixedCurveSection> curveSections;
    public void OnEnable()
    {
        segment = target as IGroundSegment;
        if (segment.Curve.Type == CurveType.Fixed)
        {
            curve = segment.Curve as FixedCurve;
            curveSections = new List<FixedCurveSection>();
            foreach (var section in curve.curveDefinition.curveSections)
            {
                curveSections.Add(section as FixedCurveSection);
            }
        }
    }
    public override void OnInspectorGUI()
    {
        if(curve == null)
        {
            return;
        }
        EditorGUI.BeginChangeCheck();
        //Access curve defintion
        //Access curve sections
        //Need to cast all as fixed



        if(EditorGUI.EndChangeCheck())
        {

        }
    }
}
