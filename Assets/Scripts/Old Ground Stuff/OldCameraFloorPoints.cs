using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class OldCameraFloorPoints
{
    public int firstSplineIndex; //The index of the point on the spline before the first floorPoint
    private SpriteShapeController groundControl;
    public Vector3[] floorPoints = new Vector3[2];

    //PROBLEM: GenerateMidpointsFromX, MoveRight, and MoveLeft all assume regular curves (every other has a normal trough).
    //This breaks floor points for irregular curves with > 2 points.
    //Solution: use more while loops to look for low points.
    //Wait until current point has tang with neg slope and next point has tang with pos slope.

    public void GenerateMidpointsFromX(SpriteShapeController controller, float targetX) //targetX is relative to world space and must be localized
    {
        groundControl = controller;
        Spline spline = controller.spline;
        //Find indices on spline that surround the targetX value beginning at the middle index of spline's points
        //Need to localize targetX?
        firstSplineIndex = GroundUtility.SplineIndexBeforeX(controller, targetX);
        //Check whether first index is beginning of trough (has tangent with y > 0)
        if (spline.GetRightTangent(firstSplineIndex).y > 0)
        {
            firstSplineIndex--;
        }
        //Assign firstMidpoint as the midpoint between those indices
        floorPoints[0] = GetMidpoint(controller, firstSplineIndex, firstSplineIndex + 1);
        //If firstMidpoint is to the right of targetX, then the current firstMidpoint becomes secondMidpoint
        //And a new firstMidpoint is located between the spline indices that are two than the indices around the targetX
        if (floorPoints[0].x > targetX)
        {
            floorPoints[1] = floorPoints[0];
            firstSplineIndex -= 2;
            floorPoints[0] = GetMidpoint(controller, firstSplineIndex, firstSplineIndex + 1);
        }
        //If firstMidpoint is <= targetX, then the splineIndices are incremented to find the second midpoint between the next pair of indices.
        else
        {
            floorPoints[1] = GetMidpoint(controller, firstSplineIndex + 2, firstSplineIndex + 3);
        }
        //If the first spline index is 0 (which corresponds to the bottom left corner point), the midpoint is hardcoded to an imaginary point 80 units to the left of the second midpoint.
        if (firstSplineIndex <= 0)
        {
            floorPoints[0] = new Vector3(floorPoints[1].x - 80, floorPoints[1].y);
        }
    }

    public Vector3 GetMidpoint(SpriteShapeController controller, int leftIndex, int rightIndex)
    {
        Spline spline = controller.spline;
        if(leftIndex < 0)
        {
            leftIndex = 0;
            rightIndex = 1;
        }
        Vector3 p0 = spline.GetPosition(leftIndex);
        Vector3 rt = p0 + spline.GetRightTangent(leftIndex);
        Vector3 p1 = spline.GetPosition(rightIndex);
        Vector3 lt = p1 + spline.GetLeftTangent(rightIndex);
        //Vector3 returnVect = controller.transform.TransformPoint(BezierUtility.BezierPoint(p0, rt, lt, p1, 0.5f));//WHY IS THIS BEING TRANSFORMED?
        //return returnVect;
        return BezierUtility.BezierPoint(p0, rt, lt, p1, 0.5f);
    }

    public Vector3 GetLowPoint(float targetX) //targetX is relative to world space and must be localized. lowPoint is returned in local space.
    {
        targetX -= groundControl.transform.position.x;
        if(targetX >= LeadingFloorX)
        {
            MoveMidpointsRight(groundControl);
        }
        else if(targetX <= TrailingFloorX)
        {
            MoveMidpointsLeft(groundControl);
        }
        if (floorPoints[0] == null)
        {
            return new Vector3(37, -20, 0);
        }
        //equation to calculate t
        Vector3 p0 = floorPoints[0];
        Vector3 p2 = floorPoints[1];
        float tangentVelocity = (p2.x - p0.x) / 4;
        Vector3 p1 = floorPoints[0] + new Vector3(tangentVelocity, 0, 0);
        Vector3 p3 = floorPoints[1] - new Vector3(tangentVelocity, 0, 0);
        float t = (targetX - p0.x) / (p2.x - p0.x);
        //Approximate point to use for closest point on curve calculation
        Vector3 approxPoint = new Vector3(
            targetX, (p0.y * (1 - t) + p2.y * t), 0);
        Vector3 lowPoint = BezierUtility.ClosestPointOnCurve(
            approxPoint, p0, p2, p1, p3, out t);
        return lowPoint;
    }


    public void MoveMidpointsRight(SpriteShapeController controller)
    {
        if (firstSplineIndex < controller.spline.GetPointCount() - 6) {
            firstSplineIndex += 2;
            floorPoints[0] = floorPoints[1];
            floorPoints[1] = GetMidpoint(controller, firstSplineIndex + 2, firstSplineIndex + 3);
        }
        //Increment the indices on the spline, change current second point to new firstpoint, calculate new secondpoint using incremented indices
        
    }
    public void MoveMidpointsLeft(SpriteShapeController controller)
    {
        if (firstSplineIndex > 2)
        {
            //Decrement the indices on the spline, change current first point to new second point, calculate new firstpoint using decremented indices
            firstSplineIndex -= 2;
            floorPoints[1] = floorPoints[0];
            floorPoints[0] = GetMidpoint(controller, firstSplineIndex, firstSplineIndex + 1);
        }
        //If the first spline index is 0 (which corresponds to the bottom left corner point), the midpoint is hardcoded to an imaginary point 80 units to the left of the second midpoint.
        else if (firstSplineIndex == 0)
        {
            floorPoints[1] = floorPoints[0];
            floorPoints[0] = new Vector3(floorPoints[1].x - 80, floorPoints[1].y);
        }
    }

    public void ShiftSpline(int delta)
    {
        firstSplineIndex += delta;
    }
    
    public float LeadingFloorX //leading and trailing x values are in local space.
    {
        get
        {
            return floorPoints[1].x;
        }
    }

    public float TrailingFloorX
    {
        get
        {
            return floorPoints[0].x;
        }
    }

}
