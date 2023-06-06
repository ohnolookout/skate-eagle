using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum CurveType { StartLine, FinishLine, Roller, SmallRoller };
public class Curve
{
    private readonly float length;
    private readonly List<float> segmentLengths;
    private readonly List<CurvePoint> curvePoints;
    private readonly CurvePoint startPoint, endPoint;
    private readonly CurveType curveType;
    private readonly Vector3 lowPoint;

    public Curve(CurveType type, CurvePoint? start = null)
    {
        if(start is null)
        {
            if(type != CurveType.StartLine)
            {
                throw new Exception("Must specify CurveType.StartLine when not providing starting CurvePoint for new Curve.");
            }
            curvePoints = CurveUtility.GenerateStartLine();
        } else
        {
            startPoint = (CurvePoint)start;
            curvePoints = CurveUtility.GenerateCurveList(type, startPoint);
        }
        curveType = type;
        length = CurveUtility.GetCurveLength(curvePoints, out segmentLengths);
        lowPoint = CurveUtility.FindLowPoint(curvePoints);
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
}
