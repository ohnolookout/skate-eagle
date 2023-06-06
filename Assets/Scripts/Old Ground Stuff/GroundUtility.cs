using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public static class GroundUtility
{

    public static void InsertCurve(SpriteShapeController controller, OldCurve curve, int index) //Inserts curve into the spline beginning at the given index
    {
        for (int i = 0; i < curve.Count; i++)
        {
            InsertCurvePoint(controller, curve.GetPoint(i), index);
            index++;
        }
        controller.spline.SetTangentMode(index, ShapeTangentMode.Broken);
        controller.spline.SetRightTangent(index, new Vector3(0, -1));

    }

    public static void InsertCurve2(SpriteShapeController controller, Curve curve, int index) //Inserts curve into the spline beginning at the given index
    {
        for (int i = 0; i < curve.Count; i++)
        {
            InsertCurvePoint(controller, curve.GetPoint(i), index);
            index++;
        }
        controller.spline.SetTangentMode(index, ShapeTangentMode.Broken);
        controller.spline.SetRightTangent(index, new Vector3(0, -1));

    }

    public static void AddCurveToList(OldCurve curve, List<CurvePoint> list)
    {
        for (int i = 0; i < curve.Count; i++)
        {
            list.Add(curve.GetPoint(i));
        }
    }

    public static OldCurve CreateNewCurve(CurvePoint lastPoint, bool includeStart = false)
    {
        OldCurve curve;
        int hillType = Random.Range(0, 2);
        //Get array of array of points the create curves depending on the random value.
        if (hillType == 0)
        {
            curve = OldCurveModes.RollerCurve(lastPoint, includeStart);
        }
        else if (hillType == 1)
        {
            curve = OldCurveModes.SmallRollerCurve(lastPoint, includeStart);
        }
        else
        {
            curve = OldCurveModes.SmallRollerCurve(lastPoint, includeStart);
        }
        return curve;
    }

    public static List<CurvePoint> GenerateLevelList(float targetLength, CurvePoint firstPoint, out float generatedLength)
    {
        List<CurvePoint> curvePoints = new List<CurvePoint>();
        float length = 0;
        CurvePoint lastPoint = firstPoint;
        while (length < targetLength)
        {
            OldCurve curve = CreateNewCurve(lastPoint);
            AddCurveToList(curve, curvePoints);
            length += curve.CurveLength;
            lastPoint = curve.GetPoint(curve.Count - 1);
        }
        generatedLength = length;
        OldCurve finishCurve = OldCurveModes.FinishLine(lastPoint);
        AddCurveToList(finishCurve, curvePoints);
        curvePoints.Reverse();
        curvePoints.Add(firstPoint);
        return curvePoints;
    }

    public static void InsertCurvePoint(SpriteShapeController controller, CurvePoint curvePoint, int index) //Inserts curvePoint at a given index
    {
        Spline spline = controller.spline;
        spline.InsertPointAt(index, curvePoint.ControlPoint);
        spline.SetTangentMode(index, ShapeTangentMode.Continuous);
        spline.SetLeftTangent(index, curvePoint.LeftTangent);
        spline.SetRightTangent(index, curvePoint.RightTangent);
    }

        public static Vector2 GetBezierCurveTangent(Vector2 firstPoint, Vector2 rightTangent, Vector2 secondPoint, Vector2 leftTangent, float t)
    {
        t = Mathf.Clamp01(t);
        Vector2 tangent = 3 * (1 - t) * (1 - t) * (rightTangent - firstPoint) +
            6 * (1 - t) * t * (leftTangent - rightTangent) +
            3 * t * t * (secondPoint - leftTangent);
        return tangent;
    }

    public static int SplineIndexBeforeX(SpriteShapeController controller, float targetX) //targetX is world and must be localized
    {
        Spline spline = controller.spline;
        targetX -= controller.transform.position.x;
        //Finds the indices of the two control point around a given x value beggining at the current index
        //If the target X is greater than the x at the current index, looks to the next index to the right
        //Otherwise looks left
        //Returns an array of two ints. 0 is left index, 1 is right index.
        int currentIndex = (spline.GetPointCount() - 1) / 2;
        float currentX = spline.GetPosition(currentIndex).x;
        int index = currentIndex;
        if (currentX <= targetX)
        {
            for (int i = currentIndex; i < spline.GetPointCount(); i++)
            {
                if (spline.GetPosition(i).x > targetX)
                {
                    index = i - 1;
                    break;
                }
            }
        }
        else
        {
            for (int i = currentIndex; i >= 0; i--)
            {
                if (spline.GetPosition(i).x < targetX)
                {
                    index = i;
                    break;
                }
            }
        }
        return index;
    }

    public static CurvePoint SplineToCurvePoint(SpriteShapeController controller, int index)
    {
        Spline spline = controller.spline;
        CurvePoint curvePoint = new CurvePoint();
        curvePoint.ControlPoint = spline.GetPosition(index);
        curvePoint.LeftTangent = spline.GetLeftTangent(index);
        curvePoint.RightTangent = spline.GetRightTangent(index);
        return curvePoint;
    }

    public static Vector2 GetSlopeAtX(SpriteShapeController controller, float targetX) //targetX is world and must be localized.
    {
        Spline spline = controller.spline;
        Transform transform = controller.transform;
        int index = SplineIndexBeforeX(controller, targetX);
        targetX -= transform.position.x;
        // Get the two control points that bracket the target x coordinate
        Vector2 firstPoint = spline.GetPosition(index);
        Vector2 rightTangent = firstPoint + (Vector2)spline.GetRightTangent(index);
        Vector2 secondPoint = spline.GetPosition(index + 1);
        Vector2 leftTangent = secondPoint + (Vector2)spline.GetLeftTangent(index + 1);
        float t = (targetX - firstPoint.x) / (secondPoint.x - firstPoint.x);// Calculate the slope of the spline at the target x coordinate
        return transform.TransformPoint(GetBezierCurveTangent(firstPoint, rightTangent, secondPoint, leftTangent, t)) - transform.position;
    }

    public static Vector3 GetMidpoint(CurvePoint firstPoint, CurvePoint secondPoint, float t)
    {
        return BezierUtility.BezierPoint(firstPoint.ControlPoint, firstPoint.RightTangent + firstPoint.ControlPoint, secondPoint.LeftTangent + secondPoint.ControlPoint, secondPoint.ControlPoint, t);
    }

    public static void UpdateRightCorners(SpriteShapeController controller, float lowerY)
    {
        Spline spline = controller.spline;
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
    public static void UpdateLeftCorners(SpriteShapeController controller, float lowerY)
    {
        Spline spline = controller.spline;
        spline.SetPosition(0, new Vector3(spline.GetPosition(1).x, spline.GetPosition(1).y - lowerY));
        spline.SetTangentMode(0, ShapeTangentMode.Linear);
        spline.SetLeftTangent(0, new Vector3(0, 1));
        spline.SetRightTangent(0, new Vector3(1, 0));
        spline.SetTangentMode(1, ShapeTangentMode.Broken);
        spline.SetLeftTangent(1, new Vector2(0, -1));
    }

    public static void UpdateCorners(SpriteShapeController controller, float lowerY)
    {
        UpdateRightCorners(controller, lowerY);
        UpdateLeftCorners(controller, lowerY);
    }

    public static void FormatSpline(Spline spline)
    {
        spline.isOpenEnded = false;
        while (spline.GetPointCount() > 2)
        {
            spline.RemovePointAt(2);
        }
    }
}
