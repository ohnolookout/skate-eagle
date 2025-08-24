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

    public static void AddFloorPoints(List<CurvePoint> curvePoints, List<Vector3> floorPositions)
    {

        if (curvePoints.Count < 2)
        {
            Debug.LogError("Not enough curve points to add corner points.");
            return;
        }

        if (curvePoints[0].TangentMode == ShapeTangentMode.Continuous)
        {
            curvePoints[0].TangentMode = ShapeTangentMode.Broken;
        }
        curvePoints[0].LeftTangent = new Vector3(0, 0);

        if (curvePoints[^1].TangentMode == ShapeTangentMode.Continuous)
        {
            curvePoints[^1].TangentMode = ShapeTangentMode.Broken;
        }
        curvePoints[^1].RightTangent = new Vector3(0, 0);

        //Try refactoring to add all floor points onto end in reverse order. Not sure if it will be consequential.
        floorPositions.Reverse();

        for (int i = 0; i < floorPositions.Count; i++)
        {
            CurvePoint floorPoint = new CurvePoint(floorPositions[i], ShapeTangentMode.Linear);

            curvePoints.Add(floorPoint);
        }
    }

    public static Vector3 GetFloorPosition(CurvePoint cornerPoint)
    {
        return GetPositionFromAngle(cornerPoint.Position, cornerPoint.FloorHeight, cornerPoint.FloorAngle);
    }

    public static Vector3 GetPositionFromAngle(Vector3 position, int floorHeight, int floorAngle)
    {
        float radians = (floorAngle % 360) * Mathf.Deg2Rad;
        return position + new Vector3(Mathf.Sin(radians) * floorHeight, -Mathf.Cos(radians) * floorHeight);
    }


    public static void GetAngleAndMagFromPosition(Vector3 curvePointPos, Vector3 targetPos, out float angle, out float magnitude)
    {
        var vector = targetPos - curvePointPos;
        magnitude = vector.magnitude;
        // Angle (0 = down)
        angle = Mathf.Atan2(vector.x, -vector.y) * Mathf.Rad2Deg;

        // Normalize angle to [0, 360)
        if (angle < 0) angle += 360f;
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
        var tangentMode = curvePoint.TangentMode == ShapeTangentMode.Linear ? ShapeTangentMode.Continuous : curvePoint.TangentMode;
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
