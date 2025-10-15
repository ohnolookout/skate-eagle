using Com.LuisPedroFonseca.ProCamera2D;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class CameraTargetUtility
{
    #region Constants

    public const float MinYOffsetT = 0.15f;
    public const float MaxYOffsetT = 0.7f;
    public const float HighYT = 0.3f;
    public const float PlayerHighYT = 0.7f;
    public const float MinSlopeDeltaForOffset = -.75f;
    public const float MaxSlopeDeltaForOffset = .75f;
    public const float DefaultAspectRatio = 16f / 9f;
    public const float DefaultOrthoSize = 50;

    #endregion

    public static void BuildGroundCameraTargets(Ground ground)
    {
        var lowPoints = ground.CurvePoints.Where(cp => cp.LinkedCameraTarget != null && cp.LinkedCameraTarget.doLowTarget);

        Dictionary<CurvePoint, (IEnumerable<Vector3> allPositions, IEnumerable<Vector3> midpointPositions)> sectionPositionDict = new();
        if (lowPoints.Count() == 0)
        {
            Debug.LogWarning("No low points found for ground: " + ground.name);
            return;
        }

        var targetIndices = BuildAllPrevAndNextTargets(ground, lowPoints);

        //Calculate y offset for each low point
        foreach (var cp in lowPoints)
        {
            sectionPositionDict[cp] = (GetSectionPositions(cp, ground, targetIndices[cp].currentIndex, targetIndices[cp].prevIndex, targetIndices[cp].nextIndex, false),
                GetSectionPositions(cp, ground, targetIndices[cp].currentIndex, targetIndices[cp].prevIndex, targetIndices[cp].nextIndex, true));
            SetYOffsets(cp, sectionPositionDict[cp].allPositions);
        }

        //Find max y delta for each low point and convert to orthographic size
        foreach (var cp in lowPoints)
        {
            var midpointPositions = sectionPositionDict[cp].midpointPositions.Concat(new[] { cp.LinkedCameraTarget.CamBottomPosition });
            var indices = targetIndices[cp];
            var prevIndex = indices.prevIndex;
            var nexIndex = indices.nextIndex;

            if(cp.LinkedCameraTarget.doUseManualZoomOrthoSize)
            {
                cp.LinkedCameraTarget.orthoSize = Mathf.Max(cp.LinkedCameraTarget.manualZoomOrthoSize, DefaultOrthoSize);
            } else
            {
                var maxYDelta = FindMaxYDelta(ground, cp, midpointPositions);

                //Translate maxYDelta to orthographic size
                cp.LinkedCameraTarget.orthoSize = Mathf.Max(maxYDelta / (1 + HighYT), DefaultOrthoSize);
            }
        }

        ground.HighPoints = BuildHighPointList(ground);

    }

    #region Building Targets
    public static Dictionary<CurvePoint, (int currentIndex, int prevIndex, int nextIndex)> BuildAllPrevAndNextTargets(Ground ground, IEnumerable<CurvePoint> lowPoints)
    {
        Dictionary<CurvePoint, (int currentIndex, int prevIndex, int nextIndex)> targetIndices = new();   

        if (!lowPoints.Any())
        {
            return targetIndices;
        }

        var prevCP = lowPoints.First();
        var currentCP = prevCP;
        CurvePoint nextCP = null;
        var prevIndex = 0;
        int currentIndex = ground.CurvePoints.IndexOf(currentCP);
        int nextIndex = 0;

        if (ground.ManualLeftCamTarget != null && ground.ManualLeftCamTarget.LinkedCameraTarget != null)
        {
            currentCP.LinkedCameraTarget.prevTarget = ground.ManualLeftCamTarget.LinkedCameraTarget;
        }
        else
        {
            currentCP.LinkedCameraTarget.prevTarget = null;
        }

        for (int i = 1; i < lowPoints.Count(); i++)
        {
            nextCP = lowPoints.ElementAt(i);
            nextIndex = ground.CurvePoints.IndexOf(nextCP);
            
            //Set prev and next targets for current and next CP
            currentCP.LinkedCameraTarget.nextTarget = nextCP.LinkedCameraTarget;
            nextCP.LinkedCameraTarget.prevTarget = currentCP.LinkedCameraTarget;

            //Set prev and next indices for currentCP
            targetIndices[currentCP] = (currentIndex, prevIndex, nextIndex);

            //Advance all variables
            prevCP = currentCP;
            currentCP = nextCP;
            prevIndex = currentIndex;
            currentIndex = nextIndex;
        }

        targetIndices[currentCP] = (currentIndex, prevIndex, ground.CurvePoints.Count - 1);

        if (ground.ManualRightCamTarget != null && ground.ManualRightCamTarget.LinkedCameraTarget != null)
        {
            currentCP.LinkedCameraTarget.nextTarget = ground.ManualRightCamTarget.LinkedCameraTarget;
        } else
        {
            currentCP.LinkedCameraTarget.nextTarget = null;
        }

        return targetIndices;
    }
    public static IEnumerable<Vector3> GetSectionPositions(CurvePoint cp, Ground ground, int currentIndex, int prev, int next, bool doUseMidpoints)
    {
        List<CurvePoint> allSectionPoints;
        if (doUseMidpoints)
        {
            var startIndex = prev + ((currentIndex - prev) / 2);
            var endIndex = currentIndex + ((next - currentIndex) / 2);
            allSectionPoints = ground.CurvePoints.GetRange(startIndex, endIndex - startIndex + 1);
        }
        else
        {
            allSectionPoints = ground.CurvePoints.GetRange(prev, next - prev + 1);
        }

        var sectionPositions = allSectionPoints.Select(p =>
        {
            return p.LinkedCameraTarget.Position;
        });

        var zoomPoints = ground.ZoomPoints.Where(z => z.Position.x > ground.CurvePoints[prev].Position.x && z.Position.x < ground.CurvePoints[next].Position.x);
        zoomPoints.Concat(cp.LinkedCameraTarget.forceZoomTargets);
        zoomPoints = zoomPoints.Distinct();
        var zoomPositions = zoomPoints.Select(z => z.LinkedCameraTarget.Position);

        return sectionPositions.Concat(zoomPositions);
    }

    #endregion

    #region Y Offsets
    public static void SetYOffsets(CurvePoint cp, IEnumerable<Vector3> positions)
    {
        if (cp.LinkedCameraTarget.doUseManualOffsets)
        {
            cp.LinkedCameraTarget.yOffset = cp.LinkedCameraTarget.manualYOffset;
            return;
        }

        var lowerPointOffsetT = 0f;
        foreach (var pos in positions)
        {
            var offsetPosY = pos.y - MinYOffsetT * DefaultOrthoSize;
            if (offsetPosY < cp.Position.y - (lowerPointOffsetT * DefaultOrthoSize))
            {
                lowerPointOffsetT = (cp.Position.y - offsetPosY) / DefaultOrthoSize;
            }
        }

        var prevPos = cp.LinkedCameraTarget.prevTarget != null ? cp.LinkedCameraTarget.prevTarget.Position : cp.Position + Vector3.left;
        var nextPos = cp.LinkedCameraTarget.nextTarget != null ? cp.LinkedCameraTarget.nextTarget.Position : cp.Position + Vector3.right;

        var slopeFromPrev = (cp.Position.y - prevPos.y) / (cp.Position.x - prevPos.x);
        var slopeToNext = (nextPos.y - cp.Position.y) / (nextPos.x - cp.Position.x);

        var slopeDelta = slopeFromPrev - slopeToNext;
        var slopeDeltaT = Mathf.Clamp01((slopeDelta - MinSlopeDeltaForOffset) / (MaxSlopeDeltaForOffset - MinSlopeDeltaForOffset));

        var slopeOffsetT = MinYOffsetT + (MaxYOffsetT - MinYOffsetT) * slopeDeltaT;
        cp.LinkedCameraTarget.yOffset = Mathf.Max(slopeOffsetT, lowerPointOffsetT);


    }
    #endregion

    #region Finding Y Delta
    public static float FindMaxYDelta(Ground ground, CurvePoint cp, IEnumerable<Vector3> positions)
    {

        float maxYDelta = 0;

        var cpDelta = cp.Position.y - cp.LinkedCameraTarget.CamBottomPosition.y;
        if (cpDelta > MaxYOffsetT * DefaultOrthoSize)
        {
            maxYDelta = (1 + HighYT) * cpDelta;
        }

        var camBottomPos = cp.LinkedCameraTarget.CamBottomPosition;
        foreach (var pos in positions)
        {
            var camBottomIntercept = GetCamBottomIntercept(pos.x, ground);

            maxYDelta = Mathf.Max(maxYDelta, pos.y - camBottomIntercept.y);
        }

        return maxYDelta;
    }
    public static Vector3 GetCamBottomIntercept(float posX, Ground ground)
    {
        if(ground.CurvePoints.Count == 0)
        {
            Debug.LogWarning("No curve points found for ground: " + ground.name);
            return new(posX, DefaultOrthoSize);
        }

        var target = FindNearestLeftTarget(posX, ground);

        if (target == null)
        {
            Debug.LogWarning("No low target found...");
            return new(posX, DefaultOrthoSize);
        }
                
        float camBottomY;
        if (target.nextTarget == null || target.Position.x > posX)
        {
            //Use target bottom y if there is no next target or if target is to the left of found target (bc found target is leftmost target in the chain)
            camBottomY = target.CamBottomPosition.y;
        }
        else
        {
            var t = (posX - target.Position.x) / (target.nextTarget.Position.x - target.Position.x);
            camBottomY = Mathf.Lerp(target.CamBottomPosition.y, target.nextTarget.CamBottomPosition.y, t);
        }

        return new(posX, camBottomY);

    }
    #endregion

    #region Locating Targets
    public static (float camBottomY, float orthoSize) GetCamParams(float posX, LinkedCameraTarget leftTarget)
    {
        if(leftTarget == null)
        {
            Debug.LogWarning("No target added...");
            return (-DefaultOrthoSize, DefaultOrthoSize);
        }

        float camBottomY;
        float orthoSize;
        if (leftTarget.nextTarget == null || leftTarget.Position.x > posX)
        {
            //Use target bottom y if there is no next target or if target is to the left of found target (bc found target is leftmost target in the chain)
            camBottomY = leftTarget.CamBottomPosition.y;
            orthoSize = leftTarget.orthoSize;
        }
        else
        {
            var t = (posX - leftTarget.Position.x) / (leftTarget.nextTarget.Position.x - leftTarget.Position.x);
            t = Mathf.Clamp01(t);
            orthoSize = Mathf.SmoothStep(leftTarget.orthoSize, leftTarget.nextTarget.orthoSize, t);
            camBottomY = Mathf.SmoothStep(leftTarget.CamBottomPosition.y, leftTarget.nextTarget.CamBottomPosition.y, t);
        }
        return (camBottomY, orthoSize);

    }
    public static LinkedCameraTarget FindNearestLeftTarget(float posX, Ground ground)
    {
        if (ground.LowPoints.Count == 0)
        {
            Debug.LogWarning($"Ground {ground.name} has no low targets. Add some.");
            return null;
        }

        var startTarget = ground.LowPoints[ground.LowPoints.Count() / 2].LinkedCameraTarget;

        if (ground.LowPoints.Count == 1)
        {
            return startTarget;
        }

        return FindNearestLeftTarget(posX, startTarget);
    }
    public static LinkedCameraTarget FindNearestLeftTarget(float posX, LinkedCameraTarget startTarget)
    {
        var currentTarget = startTarget;

        bool lookRight = posX > currentTarget.Position.x;

        if (lookRight)
        {
            while (currentTarget.nextTarget != null)
            {
                if (currentTarget.nextTarget.Position.x > posX)
                {
                    break;
                }
                currentTarget = currentTarget.nextTarget;
            }
        }
        else
        {
            while (currentTarget.prevTarget != null && currentTarget.Position.x > posX)
            {
                currentTarget = currentTarget.prevTarget;
            }
        }
        
        return currentTarget;
    }
    #endregion

    #region High Points
    public static List<LinkedHighPoint> BuildHighPointList(Ground ground)
    {
        List<LinkedHighPoint> result = new();
        bool goingUp = false;
        bool stillGoingUp = false;

        for (int i = 0; i < ground.CurvePoints.Count - 1; i++)
        {
                       var leftPoint = ground.CurvePoints[i];
            var rightPoint = ground.CurvePoints[i + 1];
            bool highPointFound = false;

            var highPosition = FindHighPoint(leftPoint, rightPoint, goingUp, out highPointFound, out stillGoingUp);

            if (highPointFound)
            {
                LinkedHighPoint highPoint = new(highPosition);

                if(result.Count > 0)
                {
                    result[^1].next = highPoint;
                    highPoint.previous = result.Count > 0 ? result[^1] : null;
                }
                result.Add(highPoint);
                highPointFound = false;
            }

            goingUp = stillGoingUp;
        }

        return result;
    }

    private static Vector3 FindHighPoint(CurvePoint leftPoint, CurvePoint rightPoint, bool goingUp, out bool highPointFound, out bool stillGoingUp)
    {
        float coarseIncrement = 0.1f;
        float fineIncrement = 0.001f;

        Vector3 coarseHighPoint = FindHighestY(leftPoint, rightPoint, 0, 1, coarseIncrement, goingUp, out float newT, out highPointFound, out stillGoingUp);
        
        if (!highPointFound)
        {
            return coarseHighPoint;
        }

        var fineHighPoint = FindHighestY(leftPoint, rightPoint, newT, newT + coarseIncrement, fineIncrement, stillGoingUp, out newT, out highPointFound, out stillGoingUp);

        return fineHighPoint;

    }


    private static Vector3 FindHighestY(CurvePoint leftPoint, CurvePoint rightPoint, float startT, float endT, float increment, bool goingUp,
        out float highestT, out bool highPointFound, out bool stillGoingUp)
    {
        Vector3 highestY = new(0, float.NegativeInfinity);
        float t = startT;
        highPointFound = false;

        while (t <= endT)
        {
            var point = BezierMath.Lerp(leftPoint, rightPoint, t);
            if(point.y > highestY.y)
            {
                goingUp = true;
                highestY = point;
            }
            else
            {
                if (goingUp)
                {
                    highestY = point;
                    t -= increment;
                    highPointFound = true;
                    break;
                }
                goingUp = false;
            }

            t += increment;
        }

        if (highPointFound)
        {
            var secondToLastPoint = BezierMath.Lerp(leftPoint, rightPoint, endT - increment);
            stillGoingUp = secondToLastPoint.y < rightPoint.Position.y;
        }
        else
        {
            stillGoingUp = goingUp;
        }

        highestT = t;
        return highestY;

    }

    #endregion
}
