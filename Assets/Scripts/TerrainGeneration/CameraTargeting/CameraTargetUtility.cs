using Com.LuisPedroFonseca.ProCamera2D;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.HableCurve;

public static class CameraTargetUtility
{
    //Min/Max Window
    public const float DefaultXBuffer = 55;
    public const float MinYOffsetT = 0.05f;
    public const float MaxYOffsetT = 0.7f;
    public const float HighYT = 0.3f;
    public const float MinSlopeDeltaForOffset = -.75f;
    public const float MaxSlopeDeltaForOffset = .75f;
    public const float DefaultAspectRatio = 16f / 9f;
    public const float DefaultOrthoSize = 50;
    public const float PointSearchWidth = DefaultOrthoSize * DefaultAspectRatio * 2;


    public static void BuildGroundCameraTargets(Ground ground)
    {
        var lowPoints = ground.CurvePoints.Where(cp => cp.LinkedCameraTarget != null && cp.LinkedCameraTarget.doLowTarget);
        ground.LowPoints = lowPoints.ToList();

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
            sectionPositionDict[cp] = GetSectionPositions(cp, ground, targetIndices[cp].leftIndex, targetIndices[cp].rightIndex);
            SetYOffsets(cp, sectionPositionDict[cp]);
        }

        //Find max y delta for each low point and convert to orthographic size
        foreach (var cp in lowPoints)
        {
            sectionPositionDict[cp] = sectionPositionDict[cp].Concat(new[] { cp.LinkedCameraTarget.CamBottomPosition });
            var indices = targetIndices[cp];
            var leftIndex = indices.leftIndex;
            var rightIndex = indices.rightIndex;

            if(cp.LinkedCameraTarget.doUseManualZoomOrthoSize)
            {
                cp.LinkedCameraTarget.zoomOrthoSize = Mathf.Max(cp.LinkedCameraTarget.manualZoomOrthoSize, DefaultOrthoSize);
            } else
            {
                var maxYDelta = FindMaxYDelta(ground, cp, sectionPositionDict[cp]);

                //Translate maxYDelta to orthographic size
                cp.LinkedCameraTarget.zoomOrthoSize = Mathf.Max(maxYDelta / (1 + HighYT), DefaultOrthoSize);
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

        var prevPos = cp.LinkedCameraTarget.prevTarget != null ? cp.LinkedCameraTarget.prevTarget.TargetPosition : cp.Position + Vector3.left;
        var nextPos = cp.LinkedCameraTarget.nextTarget != null ? cp.LinkedCameraTarget.nextTarget.TargetPosition : cp.Position + Vector3.right;

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
            var camBottomIntercept = GetCamBottomIntercept(pos, ground);

            maxYDelta = Mathf.Max(maxYDelta, pos.y - camBottomIntercept.y);
        }

        return maxYDelta;
    }

    public static IEnumerable<Vector3> GetSectionPositions(CurvePoint cp, Ground ground, int start, int end)
    {
        var allSectionPoints = ground.CurvePoints.GetRange(start, end - start);
        var sectionPositions = allSectionPoints.Select(p =>
        {
            return p.LinkedCameraTarget.TargetPosition;
        });

        var zoomPoints = ground.ZoomPoints.Where(z => z.Position.x > ground.CurvePoints[start].Position.x && z.Position.x < ground.CurvePoints[end].Position.x);
        zoomPoints.Concat(cp.LinkedCameraTarget.forceZoomTargets);
        zoomPoints = zoomPoints.Distinct();
        var zoomPositions = zoomPoints.Select(z => z.LinkedCameraTarget.TargetPosition);

        return sectionPositions.Concat(zoomPositions);
    }
    public static Dictionary<CurvePoint, (int leftIndex, int rightIndex)> BuildAllPrevAndNextTargets(Ground ground, IEnumerable<CurvePoint> lowPoints)
    {
        Dictionary<CurvePoint, (int leftIndex, int rightIndex)> targetIndices = new();   

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

    public static Vector3 GetCamBottomIntercept(Vector3 target, Ground ground, int startIndex = -1)
    {
        if(ground.CurvePoints.Count == 0)
        {
            Debug.LogWarning("No curve points found for ground: " + ground.name);
            return target + Vector3.down * DefaultOrthoSize;
        }

        var leftIndex = FindNearestLeftLowPointIndex(target, ground, startIndex);

        CurvePoint rightPoint = null;
        CurvePoint leftPoint = null;

        var foundPoint = ground.LowPoints[leftIndex];
        
        if(leftIndex < ground.CurvePoints.Count - 1 && foundPoint.Position.x < target.x)
        {
            leftPoint = foundPoint;
            rightPoint = ground.LowPoints[leftIndex + 1];
        } else
        {
            if (foundPoint.Position.x > target.x)
            {
                leftPoint = ground.ManualLeftCamTarget != null ? ground.ManualLeftCamTarget.CurvePoint : null;
                rightPoint = foundPoint;
            } else
            {
                leftPoint = foundPoint;
                rightPoint = ground.ManualRightCamTarget != null ? ground.ManualRightCamTarget.CurvePoint : null;
            }

        }

        float camBottomY;
        if (rightPoint == null)
        {
            camBottomY = leftPoint.LinkedCameraTarget.CamBottomPosition.y;
        }
        else if (leftPoint == null)
        {
            camBottomY = rightPoint.LinkedCameraTarget.CamBottomPosition.y;
        }
        else
        {
            var t = (target.x - leftPoint.Position.x) / (rightPoint.Position.x - leftPoint.Position.x);
            camBottomY = Mathf.Lerp(leftPoint.LinkedCameraTarget.CamBottomPosition.y, rightPoint.LinkedCameraTarget.CamBottomPosition.y, t);
        }

        return new(target.x, camBottomY);

    }

    public static int FindNearestLeftLowPointIndex(Vector3 target, Ground ground, int startIndex = -1)
    {
        int leftIndex;

        if (startIndex == -1)
        {
            leftIndex = ground.LowPoints.Count / 2;
        }
        else
        {
            leftIndex = Mathf.Clamp(startIndex, 0, ground.LowPoints.Count - 1);
        }

        if (leftIndex == ground.LowPoints.Count - 1)
        {
            return leftIndex;
        }

        bool lookRight = target.x > ground.LowPoints[leftIndex].Position.x;

        if (lookRight)
        {
            while (leftIndex < ground.LowPoints.Count - 2)
            {
                if (ground.LowPoints[leftIndex + 1].Position.x > target.x)
                {
                    break;
                }
                leftIndex++;
            }
        }
        else
        {
            while (leftIndex > 0 && ground.LowPoints[leftIndex].Position.x > target.x)
            {
                leftIndex--;
            }
        }
        
        return leftIndex;
    }

}
