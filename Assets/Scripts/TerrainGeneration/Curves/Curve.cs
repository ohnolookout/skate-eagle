using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public enum CurveType { Procedural, Fixed, StartLine, FinishLine };
[Serializable]
public class Curve
{
    //private float length;
    private List<float> segmentLengths;
    public List<CurvePoint> curvePoints;
    public CurveDefinition curveDefinition;
    public CurveType curveType;
    private float length;
    private protected Vector3 _lowpoint, _highpoint;
    public Curve(CurveDefinition curveDef, CurvePoint startPoint)
    {
        curveDefinition = curveDef;
        curvePoints = CurvePointsFromDefinition(curveDefinition, -startPoint.LeftTangent);
        curveType = CurveType.Fixed;
        GenerateCurveStats();
    }

    public Curve(List<CurvePoint> curvePoints)
    {
        this.curvePoints = curvePoints;
        curveType = CurveType.Fixed;
        GenerateCurveStats();
    }

    public void RefreshCurve(Vector2 prevTang)
    {
        curvePoints = CurvePointsFromDefinition(curveDefinition, prevTang); 
        GenerateCurveStats();
    }

    public List<CurvePoint> CurvePointsFromDefinition(CurveDefinition curveDef, Vector2 prevTang)
    {
        List<CurvePoint> curvePoints = new();

        //Initialize startPoint with tangents of previous point
        CurvePoint startPoint = new(new(0, 0), -prevTang, prevTang);
        foreach(var section in curveDef.curveSections)
        {
            var sectionParams = section.GetSectionParameters(startPoint.RightTangent);

            if (curvePoints.Count == 0)
            {
                curvePoints = CalculateCurvePointPair(sectionParams, startPoint);
                _highpoint = curvePoints[0].ControlPoint;
                _lowpoint = _highpoint;
                EvaluateHighLow(BezierMath.GetMidpoint(curvePoints[0], curvePoints[1]));
            }
            else
            {
                List<CurvePoint> additionalCurvePoints = CalculateCurvePointPair(sectionParams, startPoint);

                curvePoints[^1] = additionalCurvePoints[0];
                curvePoints.Add(additionalCurvePoints[1]);
                EvaluateHighLow(BezierMath.GetMidpoint(additionalCurvePoints[0], additionalCurvePoints[1]));
            }
            startPoint = curvePoints[^1];
        }

        return curvePoints;
    }

    public List<CurvePoint> CalculateCurvePointPair(CurveSectionParameters sectionParams, CurvePoint startPoint)
    {
        List<CurvePoint> curvePoints = new();

        Vector3 prevTangent = -startPoint.LeftTangent.normalized;
        CurvePoint nextPoint = new();
        nextPoint.ControlPoint = startPoint.ControlPoint + new Vector3(sectionParams.Length, sectionParams.Climb);
        nextPoint.SetTangents(sectionParams.Pitch, 1);

        Vector3 middleVertex = BezierMath.CalculateThirdVertexFromCurvePoints(startPoint, nextPoint);
        float firstMaxMagnitude = (middleVertex - startPoint.ControlPoint).magnitude * 1.1f;
        float secondMaxMagnitude = (nextPoint.ControlPoint - middleVertex).magnitude * 1.1f;
        float firstMagnitude = Mathf.Min(Mathf.Max(sectionParams.Shape * firstMaxMagnitude, startPoint.LeftTangent.magnitude * 0.5f), startPoint.LeftTangent.magnitude * 3f);
        float secondMagnitude = sectionParams.Shape * secondMaxMagnitude;

        startPoint.RightTangent = prevTangent * firstMagnitude;
        nextPoint.LeftTangent = nextPoint.LeftTangent.normalized * secondMagnitude;
        curvePoints.Add(startPoint);
        curvePoints.Add(nextPoint);

        return curvePoints;
    }

    public void EvaluateHighLow(Vector3 newPoint)
    {
        if (newPoint.y >= _highpoint.y)
        {
            _highpoint = newPoint;
        }
        else if (newPoint.y <= _lowpoint.y)
        {
            _lowpoint = newPoint;
        }
    }
    public void GenerateCurveStats()
    {
        length = GetCurveLength();
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
    public CurvePoint StartPoint => curvePoints[0];
    public CurvePoint EndPoint => curvePoints[^1];
    public List<float> SegmentLengths => segmentLengths;


}
