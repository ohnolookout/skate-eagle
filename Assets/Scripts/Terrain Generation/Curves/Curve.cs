using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum CurveType { StartLine, FinishLine, Roller, SmallRoller, Custom, CustomFixed };
public class Curve
{
    private float length;
    private List<float> segmentLengths;
    public List<CurvePoint> curvePoints;
    public CurvePoint startPoint, endPoint;
    public CurveType curveType;
    private Vector3 lowPoint;

    public void GenerateCurveStats()
    {
        length = GetCurveLength();
        lowPoint = FindLowPoint();
        startPoint = curvePoints[0];
        endPoint = curvePoints[^1];
    }


    public CurvePoint GetPoint(int i)
    {
        return curvePoints[i];
    }

    public int Count
    {
        get
        {
            return curvePoints.Count;
        }
    }
    public float Length
    {
        get
        {
            return length;
        }
    }

    public CurveType Type
    {
        get
        {
            return curveType;
        }
    }
    public Vector3 LowPoint
    {
        get
        {
            return lowPoint;
        }
    }

    public CurvePoint StartPoint
    {
        get
        {
            return startPoint;
        }
    }

    public CurvePoint EndPoint
    {
        get
        {
            return endPoint;
        }
    }

    public List<float> SegmentLengths{
        get 
        {
            return segmentLengths;
        }
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

    private Vector3 FindLowPoint()
    {
        for (int i = 0; i < curvePoints.Count - 1; i++)
        {
            if (curvePoints[i].RightTangent.y < 0)
            {
                return BezierMath.GetPointAtT(curvePoints[i], curvePoints[i + 1], 0.5f);
            }
        }
        CurvePoint lowPoint = curvePoints[0];
        for (int i = 1; i < curvePoints.Count; i++)
        {
            if (curvePoints[i].ControlPoint.y < lowPoint.ControlPoint.y) lowPoint = curvePoints[i];
        }
        return lowPoint.ControlPoint;
    }


}
