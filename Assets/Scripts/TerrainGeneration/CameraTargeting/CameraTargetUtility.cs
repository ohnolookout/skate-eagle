using Com.LuisPedroFonseca.ProCamera2D;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class CameraTargetUtility
{
    //Min/Max Window
    public const float DefaultTargetXOffset = 55;
    public const float DefaultPlayerXOffset = 45;
    public const float MinYOffsetT = 0.05f;
    public const float MaxYOffsetT = 0.7f;
    public const float HighYT = 0.3f;
    public const float PlayerHighYT = 0.7f;
    public const float MinSlopeDeltaForOffset = -.75f;
    public const float MaxSlopeDeltaForOffset = .75f;
    public const float DefaultAspectRatio = 16f / 9f;
    public const float DefaultOrthoSize = 50;
    public const float PointSearchWidth = DefaultOrthoSize * DefaultAspectRatio * 2;


    public static void BuildGroundCameraTargets(Ground ground)
    {
        var lowPoints = ground.CurvePoints.Where(cp => cp.LinkedCameraTarget != null && cp.LinkedCameraTarget.doLowTarget);

        Dictionary<CurvePoint, IEnumerable<Vector3>> sectionPositionDict = new();
        if (lowPoints.Count() == 0)
        {
            Debug.LogWarning("No low points found for ground: " + ground.name);
            return;
        }

        var targetIndices = BuildAllPrevAndNextTargets(ground, lowPoints);

        //Calculate y offset for each low point
        foreach (var cp in lowPoints)
        {
            sectionPositionDict[cp] = GetSectionPositions(cp, ground, targetIndices[cp].prevIndex, targetIndices[cp].nextIndex);
            SetYOffsets(cp, sectionPositionDict[cp]);
        }

        //Find max y delta for each low point and convert to orthographic size
        foreach (var cp in lowPoints)
        {
            sectionPositionDict[cp] = sectionPositionDict[cp].Concat(new[] { cp.LinkedCameraTarget.CamBottomPosition });
            var indices = targetIndices[cp];
            var prevIndex = indices.prevIndex;
            var nexIndex = indices.nextIndex;

            if(cp.LinkedCameraTarget.doUseManualZoomOrthoSize)
            {
                cp.LinkedCameraTarget.OrthoSize = Mathf.Max(cp.LinkedCameraTarget.manualZoomOrthoSize, DefaultOrthoSize);
            } else
            {
                var maxYDelta = FindMaxYDelta(ground, cp, sectionPositionDict[cp]);

                //Translate maxYDelta to orthographic size
                cp.LinkedCameraTarget.OrthoSize = Mathf.Max(maxYDelta / (1 + HighYT), DefaultOrthoSize);
            }
        }

    }

    public static void SetYOffsets(CurvePoint cp, IEnumerable<Vector3> positions)
    {
        if (cp.LinkedCameraTarget.doUseManualOffsets)
        {
            cp.LinkedCameraTarget.YOffset = cp.LinkedCameraTarget.manualYOffset;
            return;
        }

        var lowerPointOffsetT = 0f;
        foreach (var pos in positions)
        {
            var offsetPosY = pos.y - MinYOffsetT * DefaultOrthoSize;
            if (offsetPosY < cp.Position.y - (lowerPointOffsetT * DefaultOrthoSize))
            {
                lowerPointOffsetT = (cp.Position.y - offsetPosY)/DefaultOrthoSize;
            }
        }

        var prevPos = cp.LinkedCameraTarget.prevTarget != null ? cp.LinkedCameraTarget.prevTarget.Position : cp.Position + Vector3.left;
        var nextPos = cp.LinkedCameraTarget.nextTarget != null ? cp.LinkedCameraTarget.nextTarget.Position : cp.Position + Vector3.right;

        var slopeFromPrev = (cp.Position.y - prevPos.y) / (cp.Position.x - prevPos.x);
        var slopeToNext = (nextPos.y - cp.Position.y)/(nextPos.x - cp.Position.x);

        var slopeDelta = slopeFromPrev - slopeToNext;
        var slopeDeltaT = Mathf.Clamp01((slopeDelta - MinSlopeDeltaForOffset) / (MaxSlopeDeltaForOffset - MinSlopeDeltaForOffset));

        var slopeOffsetT = MinYOffsetT + (MaxYOffsetT - MinYOffsetT) * slopeDeltaT;
        cp.LinkedCameraTarget.YOffset = Mathf.Max(slopeOffsetT, lowerPointOffsetT);


    }
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

    public static IEnumerable<Vector3> GetSectionPositions(CurvePoint cp, Ground ground, int start, int end)
    {
        var allSectionPoints = ground.CurvePoints.GetRange(start, end - start + 1);
        var sectionPositions = allSectionPoints.Select(p =>
        {
            return p.LinkedCameraTarget.Position;
        });

        var zoomPoints = ground.ZoomPoints.Where(z => z.Position.x > ground.CurvePoints[start].Position.x && z.Position.x < ground.CurvePoints[end].Position.x);
        zoomPoints.Concat(cp.LinkedCameraTarget.forceZoomTargets);
        zoomPoints = zoomPoints.Distinct();
        var zoomPositions = zoomPoints.Select(z => z.LinkedCameraTarget.Position);

        return sectionPositions.Concat(zoomPositions);
    }
    public static Dictionary<CurvePoint, (int prevIndex, int nextIndex)> BuildAllPrevAndNextTargets(Ground ground, IEnumerable<CurvePoint> lowPoints)
    {
        Dictionary<CurvePoint, (int prevIndex, int nextIndex)> targetIndices = new();   

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
            targetIndices[currentCP] = (prevIndex, nextIndex);

            //Advance all variables
            prevCP = currentCP;
            currentCP = nextCP;
            prevIndex = currentIndex;
            currentIndex = nextIndex;
        }

        targetIndices[currentCP] = (prevIndex, ground.CurvePoints.Count - 1);

        if (ground.ManualRightCamTarget != null && ground.ManualRightCamTarget.LinkedCameraTarget != null)
        {
            currentCP.LinkedCameraTarget.nextTarget = ground.ManualRightCamTarget.LinkedCameraTarget;
        } else
        {
            currentCP.LinkedCameraTarget.nextTarget = null;
        }

        return targetIndices;
    }

    public static Vector3 GetCamBottomIntercept(float posX, Ground ground)
    {
        if(ground.CurvePoints.Count == 0)
        {
            Debug.LogWarning("No curve points found for ground: " + ground.name);
            return new(posX, DefaultOrthoSize);
        }

        var target = FindNearestLeftTarget(posX, ground);
                
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
            orthoSize = leftTarget.OrthoSize;
        }
        else
        {
            var t = (posX - leftTarget.Position.x) / (leftTarget.nextTarget.Position.x - leftTarget.Position.x);
            t = Mathf.Clamp01(t);
            orthoSize = Mathf.SmoothStep(leftTarget.OrthoSize, leftTarget.nextTarget.OrthoSize, t);
            camBottomY = Mathf.SmoothStep(leftTarget.CamBottomPosition.y, leftTarget.nextTarget.CamBottomPosition.y, t);
        }
        return (camBottomY, orthoSize);

    }
    public static LinkedCameraTarget FindNearestLeftTarget(float posX, Ground ground)
    {
        if (ground.LowPoints.Count == 0)
        {
            Debug.LogWarning($"Ground {ground.name} has now low targets. Add some.");
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

}
