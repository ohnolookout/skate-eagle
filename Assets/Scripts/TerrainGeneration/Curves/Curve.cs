using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEditor;
using Com.LuisPedroFonseca.ProCamera2D.TopDownShooter;

[Serializable]
public class Curve
{
    #region Declarations
    public List<StandardCurveSection> curveSections;
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
    public Vector3 XYDelta => new Vector3(EndPoint.Position.x - StartPoint.Position.x, EndPoint.Position.y - StartPoint.Position.y);
    public List<float> SectionLengths => _sectionLengths;
    public List<StandardCurveSection> CurveSections { get => curveSections; set => curveSections = value; }
    #endregion

    #region Construction
    public Curve()
    {
        curveSections = new();
        _curvePoints = new();
    }

    public Curve(List<StandardCurveSection> curveSections)
    {
        this.curveSections = curveSections;
        _curvePoints = GetCurvePoints();
        UpdateCurveSections();
    }

    public Curve(List<CurvePoint> curvePoints)
    {
        _curvePoints = curvePoints;
    }
    public List<CurvePoint> GetCurvePoints()
    {
        List<CurvePoint> allPoints = curveSections[0].GetCurvePoints(new());
        for (int i = 1; i < curveSections.Count; i++)
        {
            var newPoints = curveSections[i].GetCurvePoints(allPoints[^1]);
            newPoints.RemoveAt(0);
            allPoints.AddRange(newPoints);
        }

        return allPoints;
    }

    public void UpdateCurveSections(Vector2? prevTang = null)
    {
        for (int i = 0; i < curveSections.Count; i++)
        {
            if (curveSections[i].Type == CurveDirection.Flat)
            {
                curveSections[i].UpdateCurvePoints();
                if (i > 0)
                {
                    curveSections[i - 1].SetEndPointTangent(curveSections[i].StartPoint.LeftTangent);
                    curveSections[i - 1].UpdateCurvePoints();
                }
                continue;
            }

            curveSections[i].UpdateCurvePoints(prevTang);
            prevTang = curveSections[i].EndPoint.RightTangent;
        }
        _curvePoints = GetCurvePoints();
        GenerateCurveStats();

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
            _sectionLengths.Add(BezierMath.Length(_curvePoints[i].Position, _curvePoints[i].RightTangent, _curvePoints[i + 1].LeftTangent, _curvePoints[i + 1].Position));
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

    public void DoDefaultHighLowPoints()
    {
        _highPoint = _curvePoints[0].Position;
        _lowPoint = _highPoint;

        for (int i = 0; i < _curvePoints.Count - 1; i++)
        {
            EvaluateHighLow(_curvePoints[i].Position);
        }
    }
    #endregion


}
