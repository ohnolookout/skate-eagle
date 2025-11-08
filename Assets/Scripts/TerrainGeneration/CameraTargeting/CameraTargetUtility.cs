using AYellowpaper.SerializedCollections.Editor.Data;
using Com.LuisPedroFonseca.ProCamera2D;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

public static class CameraTargetUtility
{
    #region Constants

    public const float MinYOffsetT = 0.15f;
    public const float MaxYOffsetT = 0.6f;
    public const float HighYT = 0.4f;
    public const float PlayerHighYT = 0.7f;
    public const float MinSlopeDeltaForOffset = -.75f;
    public const float MaxSlopeDeltaForOffset = .75f;
    public const float DefaultAspectRatio = 16f / 9f;
    public const float DefaultOrthoSize = 50;
    //private static float maxSlope = (DefaultOrthoSize * 1.6f)/ CameraManager.maxXOffset; //Max slope to keep target within camera view

    #endregion
    #region Runtime Methods
    public static (float camBottomY, float orthoSize) GetCamParams(float posX, LinkedCameraTarget leftTarget)
    {
        if (leftTarget == null)
        {
            Debug.LogWarning("No target added...");
            return (-DefaultOrthoSize, DefaultOrthoSize);
        }

        float camBottomY;
        float orthoSize;
        if (leftTarget.NextTarget == null || leftTarget.Position.x > posX)
        {
            //Use target bottom y if there is no next target or if target is to the left of found target (bc found target is leftmost target in the chain)
            camBottomY = leftTarget.CamBottomPosition.y;
            orthoSize = leftTarget.orthoSize;
        }
        else
        {
            var t = (posX - leftTarget.Position.x) / (leftTarget.NextTarget.Position.x - leftTarget.Position.x);
            t = Mathf.Clamp01(t);
            orthoSize = Mathf.SmoothStep(leftTarget.orthoSize, leftTarget.NextTarget.orthoSize, t);
            camBottomY = Mathf.SmoothStep(leftTarget.CamBottomPosition.y, leftTarget.NextTarget.CamBottomPosition.y, t);
        }
        return (camBottomY, orthoSize);


    }

    #endregion

#if UNITY_EDITOR
    public static void BuildGroundCameraTargets(Ground ground)
    {
        var lowTargets = ground.GetLowTargets();


        var cpTargets = ground.CurvePoints.Select(cp => cp.LinkedCameraTarget).ToList();

        if (ground.ManualLeftTargetObj != null)
        {
            cpTargets.Insert(0, ground.ManualLeftCamTarget);
        }

        if (ground.ManualRightTargetObj != null)
        {
            cpTargets.Add(ground.ManualRightCamTarget);
        }


        Dictionary<LinkedCameraTarget, (IEnumerable<Vector3> allPositions, IEnumerable<Vector3> midpointPositions)> sectionPositionDict = new();
        if (lowTargets.Count() == 0)
        {
            Debug.LogWarning("No low points found for ground: " + ground.name);
            return;
        }

        var targetIndices = BuildAllPrevAndNextTargets(cpTargets, lowTargets, ground);

        //Calculate y offset for each low point
        foreach (var target in lowTargets)
        {
            var zoomPoints = ground.GetZoomPoints();
            sectionPositionDict[target] = (GetSectionPositions(target, cpTargets, zoomPoints, targetIndices[target].currentIndex, targetIndices[target].prevIndex, targetIndices[target].nextIndex, false),
                GetSectionPositions(target, cpTargets, zoomPoints, targetIndices[target].currentIndex, targetIndices[target].prevIndex, targetIndices[target].nextIndex, true));

            if (IsManualEndpoint(target, ground))
            {
                continue;
            }

            SetYOffsets(target, sectionPositionDict[target].allPositions);
        }

        //Find max y delta for each low point and convert to orthographic size
        foreach (var target in lowTargets)
        {
            //Don't calculate for low points that aren't in ground.CurvePoints
            if (IsManualEndpoint(target, ground))
            {
                continue;
            }

            var midpointPositions = sectionPositionDict[target].midpointPositions.Concat(new[] { target.CamBottomPosition });
            var indices = targetIndices[target];
            var prevIndex = indices.prevIndex;
            var nexIndex = indices.nextIndex;

            if(target.doUseManualOrthoSize)
            {
                target.orthoSize = Mathf.Max(target.manualOrthoSize, DefaultOrthoSize);
            } else
            {
                var maxYDelta = FindMaxYDelta(ground, target, midpointPositions);

                //Translate maxYDelta to orthographic size
                target.orthoSize = Mathf.Max(maxYDelta / (1 + HighYT), DefaultOrthoSize);
            }
        }

        ground.HighTargets = BuildHighPointList(ground);

    }

    #region Build Prev/Next Targets
    public static Dictionary<LinkedCameraTarget, (int currentIndex, int prevIndex, int nextIndex)> 
        BuildAllPrevAndNextTargets(List<LinkedCameraTarget> cpTargets, IEnumerable<LinkedCameraTarget> lowPoints, Ground ground)
    {
        Dictionary<LinkedCameraTarget, (int currentIndex, int prevIndex, int nextIndex)> targetIndices = new();   

        if (!lowPoints.Any())
        {
            return targetIndices;
        }

        var prevTarget = lowPoints.First();
        var currentTarget = prevTarget;
        LinkedCameraTarget nextTarget = null;
        int currentIndex = cpTargets.IndexOf(currentTarget);
        var prevIndex = 0;
        int nextIndex = 0;

        currentTarget.PrevTarget = null;

        for (int i = 1; i < lowPoints.Count(); i++)
        {
            nextTarget = lowPoints.ElementAt(i);
            nextIndex = cpTargets.IndexOf(nextTarget);

            //Set prev and next targets for current and next CP
            if (!IsManualEndpoint(currentTarget, ground))
            {
                currentTarget.NextTarget = nextTarget;
            }
            if (!IsManualEndpoint(nextTarget, ground))
            {
                nextTarget.PrevTarget = currentTarget;
            }

            //Set prev and next indices for currentCP
            targetIndices[currentTarget] = (currentIndex, prevIndex, nextIndex);

            //Advance all variables
            prevTarget = currentTarget;
            currentTarget = nextTarget;
            prevIndex = currentIndex;
            currentIndex = nextIndex;
        }

        targetIndices[currentTarget] = (currentIndex, prevIndex, cpTargets.Count - 1);

        return targetIndices;
    }

    public static IEnumerable<Vector3> GetSectionPositions(LinkedCameraTarget target, List<LinkedCameraTarget> cpTargets, List<LinkedCameraTarget> allZoomPoints, int currentIndex, int prev, int next, bool doUseMidpoints)
    {
        List<LinkedCameraTarget> allSectionPoints;

        if (doUseMidpoints)
        {
            var startIndex = prev + ((currentIndex - prev) / 2);
            var endIndex = currentIndex + ((next - currentIndex) / 2);
            allSectionPoints = cpTargets.GetRange(startIndex, endIndex - startIndex + 1);
        }
        else
        {
            allSectionPoints = cpTargets.GetRange(prev, next - prev + 1);
        }

        var sectionPositions = allSectionPoints.Select(p =>
        {
            return p.Position;
        });

        var zoomPoints = allZoomPoints.Where(z => z.Position.x > cpTargets[prev].Position.x && z.Position.x < cpTargets[next].Position.x);
        zoomPoints.Concat(target.GetZoomTargets());
        zoomPoints = zoomPoints.Distinct();
        var zoomPositions = zoomPoints.Select(z => z.Position);

        return sectionPositions.Concat(zoomPositions);
    }

    #endregion

    #region Y Offsets
    public static void SetYOffsets(LinkedCameraTarget target, IEnumerable<Vector3> positions)
    {
        if (target.doUseManualOffset)
        {
            target.yOffset = target.manualYOffset;
            return;
        }


        var prevPos = target.PrevTarget != null ? target.PrevTarget.Position - new Vector3(0, MinYOffsetT * DefaultOrthoSize) : target.Position - new Vector3(1, MinYOffsetT * DefaultOrthoSize);
        var nextPos = target.NextTarget != null ? target.NextTarget.Position - new Vector3(0, MinYOffsetT * DefaultOrthoSize) : target.Position - new Vector3(-1, MinYOffsetT * DefaultOrthoSize);
        var targetAdjustedPos = target.Position - new Vector3(0, MinYOffsetT * DefaultOrthoSize);

        var lowestYIntercept = float.PositiveInfinity;
        foreach (var pos in positions)
        {
            if (pos == target.Position || pos == prevPos || pos == nextPos)
            {
                continue;
            }
            //Calculate slope from previous or next point
            //Find where line intercepts at cp x position

            float slope;
            var adjustedPos = pos - new Vector3(0, MinYOffsetT * DefaultOrthoSize);
            if (adjustedPos == prevPos || adjustedPos == nextPos)
            {
                continue;
            }
            if (pos.x < target.Position.x)
            {
                slope = (adjustedPos.y - prevPos.y) / (adjustedPos.x - prevPos.x);
            }
            else
            {
                slope = (adjustedPos.y - nextPos.y) / (nextPos.x - adjustedPos.x);
            }

            var yIntercept = adjustedPos.y + slope * Mathf.Abs(target.Position.x - adjustedPos.x);

            lowestYIntercept = Mathf.Min(lowestYIntercept, yIntercept);
        }

        var slopeFromPrev = (targetAdjustedPos.y - prevPos.y) / (targetAdjustedPos.x - prevPos.x);
        var slopeToNext = (targetAdjustedPos.y - nextPos.y) / (nextPos.x - targetAdjustedPos.x);

        var leftTargetPos = target.Position + new Vector3(-CameraManager.minXOffset, -MinYOffsetT * DefaultOrthoSize);
        var rightTargetPos = target.Position + new Vector3(CameraManager.minXOffset, -MinYOffsetT * DefaultOrthoSize);

        var interceptFromLeftXOffset = leftTargetPos.y + slopeToNext * Mathf.Abs(targetAdjustedPos.x - leftTargetPos.x);
        var interceptFromRightXOffset = rightTargetPos.y + slopeFromPrev * Mathf.Abs(targetAdjustedPos.x - rightTargetPos.x);

        lowestYIntercept = Mathf.Min(lowestYIntercept, interceptFromLeftXOffset, interceptFromRightXOffset);

        var lowestPointOffsetT = (target.Position.y - lowestYIntercept) / DefaultOrthoSize;

        var slopeDelta = slopeFromPrev - slopeToNext;
        var slopeDeltaT = Mathf.Clamp01((slopeDelta - MinSlopeDeltaForOffset) / (MaxSlopeDeltaForOffset - MinSlopeDeltaForOffset));

        var slopeOffsetT = MinYOffsetT + (MaxYOffsetT - MinYOffsetT) * slopeDeltaT;        
        target.yOffset = Mathf.Max(slopeOffsetT, lowestPointOffsetT, MinYOffsetT);

    }
    #endregion

    #region Finding Y Delta
    public static float FindMaxYDelta(Ground ground, LinkedCameraTarget target, IEnumerable<Vector3> positions)
    {

        float maxYDelta = 0;

        var cpDelta = target.Position.y - target.CamBottomPosition.y;
        if (cpDelta > MaxYOffsetT * DefaultOrthoSize)
        {
            maxYDelta = (1 + HighYT) * cpDelta;
        }

        var camBottomPos = target.CamBottomPosition;
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
        if (target.NextTarget == null || target.Position.x > posX)
        {
            //Use target bottom y if there is no next target or if target is to the left of found target (bc found target is leftmost target in the chain)
            camBottomY = target.CamBottomPosition.y;
        }
        else
        {
            var t = (posX - target.Position.x) / (target.NextTarget.Position.x - target.Position.x);
            camBottomY = Mathf.Lerp(target.CamBottomPosition.y, target.NextTarget.CamBottomPosition.y, t);
        }

        return new(posX, camBottomY);

    }
    #endregion

    #region Locating Targets
    public static LinkedCameraTarget FindNearestLeftTarget(float posX, Ground ground)
    {
        if (ground.LowTargets.Count == 0)
        {
            Debug.LogWarning($"Ground {ground.name} has no low targets. Add some.");
            return null;
        }

        var lowPoints = ground.LowTargets;
        int currentIndex = lowPoints.Count() / 2;

        while (currentIndex < ground.LowTargets.Count - 1 && lowPoints[currentIndex + 1].Position.x < posX)
        {
            currentIndex++;
        }

        while (currentIndex > 0 && lowPoints[currentIndex].Position.x > posX)
        {
            currentIndex--;
        }

        return lowPoints[currentIndex];
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

            //Add position if high point found or if at end of ground and still going up
            if (highPointFound || (i == ground.CurvePoints.Count - 2 && stillGoingUp))
            {
                LinkedHighPoint highPoint = new(highPosition + ground.transform.position);

                if(result.Count > 0)
                {
                    result[^1].Next = highPoint;
                    highPoint.Previous = result.Count > 0 ? result[^1] : null;
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

        Vector3 coarseHighPoint = FindHighestY(leftPoint, rightPoint, 0, 1, coarseIncrement, goingUp, out float startFineT, out highPointFound, out stillGoingUp);
        
        if (!highPointFound)
        {
            return coarseHighPoint;
        }

        var fineHighPoint = FindHighestY(leftPoint, rightPoint, startFineT, startFineT + coarseIncrement, fineIncrement, stillGoingUp, out startFineT, out highPointFound, out stillGoingUp);

        return fineHighPoint;

    }


    private static Vector3 FindHighestY(CurvePoint leftPoint, CurvePoint rightPoint, float startT, float endT, float increment, bool goingUp,
        out float highestT, out bool highPointFound, out bool stillGoingUp)
    {
        Vector3 highestY = new(0, float.NegativeInfinity);
        float t = startT;
        highPointFound = false;

        while (t < endT + increment)
        {
            var point = BezierMath.Lerp(leftPoint, rightPoint, t);

            if (point.y > highestY.y)
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

        highestT = Mathf.Clamp(t, startT, endT);
        return highestY;

    }

    #endregion

    #region Manual End Targets

    public static CurvePointEditObject ValidateEndTargetObj(Ground ground, GameObject obj)
    {
        if (obj != null)
        {
            var cpObj = obj.GetComponent<CurvePointEditObject>();
            if (cpObj != null && cpObj.LinkedCameraTarget.doLowTarget && cpObj.ParentGround != ground)
            {
                return cpObj;
            }
            else
            {
                return null;
            }
        }
        else
        {
           return null;
        }
    }
    public static bool IsManualEndpoint(LinkedCameraTarget target, Ground ground)
    {
        return (ground.ManualLeftCamTarget != null && target == ground.ManualLeftCamTarget) ||
               (ground.ManualRightCamTarget != null && target == ground.ManualRightCamTarget);
    }
    #endregion
#endif
}
