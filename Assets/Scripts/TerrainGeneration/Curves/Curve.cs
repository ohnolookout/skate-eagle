using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum CurveType { StartLine, FinishLine, Custom };
public class Curve
{
    //private float length;
    private List<float> segmentLengths;
    public List<CurvePoint> curvePoints;
    public CurvePoint startPoint, endPoint;
    public CurveType curveType;
    private float length;
    private protected Vector3 _lowpoint, _highpoint;

    public void GenerateCurveStats()
    {
        length = GetCurveLength();
        startPoint = curvePoints[0];
        endPoint = curvePoints[^1];
    }
    private float GetCurveLength()
    {
        segmentLengths = new();
        float length = 0;
        for (int i = 0; i < curvePoints.Count - 1; i++)
        {
            segmentLengths.Add(BezierMath.Length(curvePoints[i].ControlPoint, curvePoints[i].RightTangent, curvePoints[i + 1].LeftTangent, curvePoints[i + 1].ControlPoint));
            length += segmentLengths[^1];
        }
        return length;
    }

    public CurvePoint GetPoint(int i)
    {
        return curvePoints[i];
    }

    public int Count { get => curvePoints.Count; }
    public CurveType Type { get => curveType; }
    public Vector3 Lowpoint { get => _lowpoint; }
    public Vector3 Highpoint => _highpoint;
    public CurvePoint StartPoint { get => startPoint; }
    public CurvePoint EndPoint { get => endPoint; }
    public List<float> SegmentLengths { get => segmentLengths; }


}
