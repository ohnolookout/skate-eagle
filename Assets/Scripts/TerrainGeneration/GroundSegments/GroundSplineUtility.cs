using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

public static class GroundSplineUtility
{


    public static void GenerateSpline(Spline spline, Curve curve, int floorHeight)
    {
        InsertCurveToSpline(spline, curve, 1);
        UpdateCorners(spline, floorHeight);
    }

    public static void GenerateSpline(Spline spline, List<SplineControlPoint> splinePoints, bool isOpen)
    {
        spline.Clear();
        spline.isOpenEnded = isOpen;

        foreach (var point in splinePoints)
        {
            InsertSplinePointToSpline(spline, point, spline.GetPointCount());
        }
    }



    public static void UpdateCorners(Spline spline, float lowerY)
    {
        UpdateRightCorners(spline, lowerY);
        UpdateLeftCorners(spline, lowerY);
    }

    public static void UpdateRightCorners(Spline spline, float lowerY)
    {
        //Reassigns the lower right corner (last index on the spline) to the same x as the preceding point and the y of the preceding point - the lowerBoundY buffer.
        int lastIndex = spline.GetPointCount() - 1;
        spline.SetPosition(lastIndex, new Vector3(spline.GetPosition(lastIndex - 1).x, spline.GetPosition(lastIndex - 1).y - lowerY));
        spline.SetTangentMode(lastIndex, ShapeTangentMode.Linear);
        spline.SetLeftTangent(lastIndex, new Vector3(-1, 0));
        spline.SetRightTangent(lastIndex, new Vector3(0, 1));
        //Resets the corner point's tangent mode in case it was changed.
        spline.SetTangentMode(lastIndex - 1, ShapeTangentMode.Broken);
        spline.SetRightTangent(lastIndex - 1, new Vector2(0, -1));
    }
    public static void UpdateLeftCorners(Spline spline, float lowerY)
    {

        spline.SetPosition(0, new Vector3(spline.GetPosition(1).x, spline.GetPosition(1).y - lowerY));
        spline.SetTangentMode(0, ShapeTangentMode.Linear);
        spline.SetLeftTangent(0, new Vector3(0, 1));
        spline.SetRightTangent(0, new Vector3(1, 0));
        spline.SetTangentMode(1, ShapeTangentMode.Broken);
        spline.SetLeftTangent(1, new Vector2(0, -1));
    }


    public static void InsertCurveToOpenSpline(Spline spline, Curve curve)
    {
        CopyCurvePointToSpline(spline, curve.GetPoint(0), 0);
        CopyCurvePointToSpline(spline, curve.GetPoint(1), 1);
        for (int i = 2; i < curve.Count; i++)
        {
            InsertCurvePointToSpline(spline, curve.GetPoint(i), i);
        }
    }

    public static void InsertCurveToSpline(Spline spline, Curve curve, int index) //Inserts curve into the spline beginning at the given index
    {
        for (int i = 0; i < curve.Count; i++)
        {
            InsertCurvePointToSpline(spline, curve.GetPoint(i), index);
            index++;
        }
        spline.SetTangentMode(index, ShapeTangentMode.Broken);
        spline.SetRightTangent(index, new Vector3(0, -1));

    }
    public static void InsertCurvePointToSpline(Spline spline, CurvePoint curvePoint, int index) //Inserts curvePoint at a given index
    {
        spline.InsertPointAt(index, curvePoint.ControlPoint);
        spline.SetTangentMode(index, ShapeTangentMode.Continuous);
        spline.SetLeftTangent(index, curvePoint.LeftTangent);
        spline.SetRightTangent(index, curvePoint.RightTangent);
    }

    public static void CopyCurvePointToSpline(Spline spline, CurvePoint curvePoint, int index) //Inserts curvePoint at a given index
    {
        spline.SetPosition(index, curvePoint.ControlPoint);
        spline.SetTangentMode(index, ShapeTangentMode.Continuous);
        spline.SetLeftTangent(index, curvePoint.LeftTangent);
        spline.SetRightTangent(index, curvePoint.RightTangent);
    }

    public static void InsertSplinePointToSpline(Spline spline, SplineControlPoint splineControlPoint, int index) //Inserts curvePoint at a given index
    {
        spline.InsertPointAt(index, splineControlPoint.position);
        spline.SetTangentMode(index, splineControlPoint.mode);
        spline.SetLeftTangent(index, splineControlPoint.leftTangent);
        spline.SetRightTangent(index, splineControlPoint.rightTangent);
    }

    public static void FormatSpline(Spline spline, bool isOpenEnded)
    {
        spline.isOpenEnded = isOpenEnded;
        while (spline.GetPointCount() > 2)
        {
            spline.RemovePointAt(2);
        }
        spline.SetPosition(0, new Vector3(-5, -5));
        spline.SetPosition(1, new Vector3(5, 5));
    }
}
