using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEditor;

[Serializable]
public class Curve
{
    #region Declarations
    public CurveDefinition curveDefinition;
    [SerializeField][HideInInspector] private List<float> _sectionLengths;
    [SerializeField][HideInInspector] private List<CurvePoint> _curvePoints;
    [SerializeField][HideInInspector] private float length;
    [SerializeField][HideInInspector] private protected Vector3 _lowPoint, _highPoint;
    public List<CurvePoint> CurvePoints { get => _curvePoints; set => _curvePoints = value; }
    public int Count { get => _curvePoints.Count; }
    public Vector3 LowPoint { get => _lowPoint; set => _lowPoint = value; }
    public Vector3 HighPoint { get => _highPoint; set => _highPoint = value; }
    public CurvePoint StartPoint => _curvePoints[0];
    public CurvePoint EndPoint => _curvePoints[^1];
    public List<float> SectionLengths => _sectionLengths;
    #endregion

    #region Construction
    public Curve(CurveDefinition curveDef, Vector2 prevTang)
    {
        curveDefinition = curveDef;
        _curvePoints = CurvePointsFromDefinition(curveDefinition, prevTang);
        GenerateCurveStats();
    }

    public Curve(List<CurvePoint> curvePoints)
    {
        _curvePoints = curvePoints;
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
                _highPoint = curvePoints[0].ControlPoint;
                _lowPoint = _highPoint;
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
    #endregion

    #region Curve Stats

    private void EvaluateHighLow(Vector3 newPoint)
    {
        if (newPoint.y > _highPoint.y)
        {
            _highPoint = newPoint;
        }
        else if (newPoint.y < _lowPoint.y)
        {
            _lowPoint = newPoint;
        }
    }
    private void GenerateCurveStats()
    {
        length = GetCurveLength();
    }
    private float GetCurveLength()
    {
        _sectionLengths = new();
        float length = 0;
        for (int i = 0; i < _curvePoints.Count - 1; i++)
        {
            _sectionLengths.Add(BezierMath.Length(_curvePoints[i].ControlPoint, _curvePoints[i].RightTangent, _curvePoints[i + 1].LeftTangent, _curvePoints[i + 1].ControlPoint));
            length += _sectionLengths[^1];
        }
        return length;
    }

    public CurvePoint GetPoint(int i)
    {
        return _curvePoints[i];
    }

    public void LogCurvePoints()
    {
        foreach (var point in _curvePoints)
        {
            point.Log();
        }
    }

    public void RecalculateDefaultHighLowPoints()
    {
        _highPoint = _curvePoints[0].ControlPoint;
        _lowPoint = _highPoint;

        for ( int i = 0; i < _curvePoints.Count - 1; i++)
        {
            EvaluateHighLow(BezierMath.GetMidpoint(_curvePoints[i], _curvePoints[i + 1]));
        }
    }
    #endregion


}
