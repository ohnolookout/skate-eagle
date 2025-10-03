using Com.LuisPedroFonseca.ProCamera2D;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public static class CameraTargetUtility
{
    //Min/Max Window
    public static Vector2 TaperVector = new Vector2(1, 1);
    public static float DefaultXBuffer = 55;
    public static float MinYOffsetT = 0.05f;
    public static float MaxYOffsetT = 0.7f;
    public static float HighYT = 0.6f;
    public static float MinSlopeDeltaForOffset = -.75f;
    public static float MaxSlopeDeltaForOffset = .75f;
    public static float DefaultAspectRatio = 16f / 9f;
    public static float DefaultOrthoSize = 50;
    public static float ZoomPointSearchWidth = DefaultOrthoSize * DefaultAspectRatio * 2;


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
            sectionPositionDict[cp] = GetAllSectionPositions(cp, ground, targetIndices[cp].leftIndex, targetIndices[cp].rightIndex);
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
                var maxYDelta = FindMaxYDelta(cp, sectionPositionDict[cp]);

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
    public static float FindMaxYDelta(CurvePoint cp, IEnumerable<Vector3> positions)
    {
        //Compare all points against all other points to find max y delta
        float maxYDelta = 0;

        var cpDelta = cp.Position.y - cp.LinkedCameraTarget.CamBottomPosition.y;
        if (cpDelta > MaxYOffsetT * DefaultOrthoSize)
        {
            maxYDelta = 2 * cpDelta;
        }

        foreach (var pos1 in positions)
        {
            foreach (var pos2 in positions)
            {
                var yDelta = Math.Abs(pos1.y - pos2.y);
                if (yDelta > maxYDelta)
                {
                    maxYDelta = yDelta;
                }
            }
        }

        return maxYDelta;
    }


    public static IEnumerable<Vector3> GetAllSectionPositions(CurvePoint cp, Ground ground, int start, int end)
    {
        //Leave current point in to account for large offsets.
        //Should i use offset points for all lowPoints?
        var allSectionPoints = ground.CurvePoints.GetRange(start, end - start);
        var filteredSectionPoints = allSectionPoints.Where(p => Mathf.Abs(p.Position.x - cp.Position.x) < ZoomPointSearchWidth);
        var sectionPositions = filteredSectionPoints.Select(p => 
            {
                if (p.LinkedCameraTarget.doLowTarget)
                {
                     return p.LinkedCameraTarget.CamBottomPosition;
                }

                return p.LinkedCameraTarget.TargetPosition;
            });

        var zoomPoints = ground.ZoomPoints.Where(z => z.Position.x > ground.CurvePoints[start].Position.x && z.Position.x < ground.CurvePoints[end].Position.x);

        foreach (var z in cp.LinkedCameraTarget.forceZoomTargets)
        {
            if(!zoomPoints.Contains(z))
            {
                zoomPoints = zoomPoints.Append(z);
            }
        }

        var zoomPositions = zoomPoints.Select(z => z.LinkedCameraTarget.TargetPosition);

        return sectionPositions.Concat(zoomPositions);
    }

    //Could rewrite to build sequentially instead of searching left and right each time for each point
    //Return a dict with tuplevalues of indices

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
    
    /// <summary>
    /// Checks if a target point is underneath a line that starts at startPoint
    /// and extends infinitely in the direction of slope.
    /// </summary>
    /// 
    public static bool IsPointBelowLine(Vector2 target, Vector2 startPoint, Vector2 slope, bool isBelow)
    {
        // Parametric form: line(t) = startPoint + slope * t
        // Solve for t such that lineX = target.X
        float t = (target.x - startPoint.x) / slope.x;

        // Compute Y on the line at that X
        float lineY = startPoint.y + slope.y * t;

        // Target is below if its Y is strictly less
        if (isBelow)
        {
            return target.y <= lineY;
        } else
        {
            return target.y >= lineY;
        }
    }

    public static float FindStartYForIntercept(float startX, Vector2 slope, Vector2 point)
    {
        float t = (point.x - startX) / slope.x;

        float yChange = slope.y * t;

        return point.y - yChange;
    }


}
