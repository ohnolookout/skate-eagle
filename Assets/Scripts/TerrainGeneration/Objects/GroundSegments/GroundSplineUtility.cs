using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

public static class GroundSplineUtility
{
    public static void GenerateSpline(Spline spline, List<CurvePoint> curvePoints, bool isOpen)
    {
        if (curvePoints.Count < 2)
        {
            Debug.LogError("Not enough curve points to generate a spline.");
            return;
        }

        FormatSpline(spline, isOpen);

        int startIndex = 0;

        for (int i = startIndex; i < curvePoints.Count; i++)
        {            
            AddCurvePointToSpline(spline, curvePoints[i], i, true);
        }

    }

    public static void CopySpline(Spline spline, List<CurvePoint> curvePoints, bool isOpen)
    {
        spline.Clear();
        spline.isOpenEnded = isOpen;

        for (int i = 0; i < curvePoints.Count; i++)
        {
            AddCurvePointToSpline(spline, curvePoints[i], i, false);
        }
    }

    public static void AddCornerPoints(List<CurvePoint> curvePoints)
    {

        if (curvePoints.Count < 2)
        {
            Debug.LogError("Not enough curve points to add corner points.");
            return;
        }

        if (curvePoints[0].Mode == ShapeTangentMode.Continuous)
        {
            curvePoints[0].Mode = ShapeTangentMode.Broken;
        }
        curvePoints[0].LeftTangent = new Vector3(0, 0);

        if (curvePoints[^1].Mode == ShapeTangentMode.Continuous)
        {
            curvePoints[^1].Mode = ShapeTangentMode.Broken;
        }
        curvePoints[^1].RightTangent = new Vector3(0, 0);

        var leftPosition = GetCornerPosition(curvePoints[0], true);
        var rightPosition = GetCornerPosition(curvePoints[^1], false);

        // Add left corner point
        CurvePoint leftCorner = new CurvePoint(leftPosition, ShapeTangentMode.Linear);
        curvePoints.Insert(0, leftCorner);

        // Add right corner point
        CurvePoint rightCorner = new CurvePoint(rightPosition, ShapeTangentMode.Linear);
        curvePoints.Add(rightCorner);
    }

    private static Vector3 GetCornerPosition(CurvePoint cornerPoint, bool isLeftCorner)
    {
        float radians = (cornerPoint.FloorAngle % 360) * Mathf.Deg2Rad;

        if (isLeftCorner)
        {
            return cornerPoint.Position + new Vector3(-Mathf.Sin(radians) * cornerPoint.FloorHeight, -Mathf.Cos(radians) * cornerPoint.FloorHeight);
        }
        else
        {
            return cornerPoint.Position + new Vector3(Mathf.Sin(radians) * cornerPoint.FloorHeight, -Mathf.Cos(radians) * cornerPoint.FloorHeight);
        }
    }


    private static void AddCurvePointToSpline(Spline spline, CurvePoint curvePoint, int index, bool doInsert) //Inserts curvePoint at a given index
    {
        if (doInsert)
        {
            spline.InsertPointAt(index, curvePoint.Position);
        }
        else
        {
            spline.SetPosition(index, curvePoint.Position);
        }
        var tangentMode = curvePoint.Mode == ShapeTangentMode.Linear ? ShapeTangentMode.Continuous : curvePoint.Mode;
        spline.SetTangentMode(index, tangentMode);
        spline.SetLeftTangent(index, curvePoint.LeftTangent);
        spline.SetRightTangent(index, curvePoint.RightTangent);
    }

    public static void FormatSpline(Spline spline, bool isOpenEnded)
    {
        spline.isOpenEnded = isOpenEnded;
        spline.Clear();
    }
}
