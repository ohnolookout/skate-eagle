using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class SerializedGround : IDeserializable
{
    public Vector2 position;
    public Quaternion rotation;
    public string name;
    public bool isInverted = false;
    public bool hasShadow = true;
    public FloorType floorType;
    public int floorHeight;
    public int floorAngle;
    public List<SerializedGroundSegment> segmentList;
    public List<CurvePoint> curvePoints;
    public List<CurvePoint> lowPoints;
    public List<CurvePoint> zoomPoints;
    public CurvePoint manualLeftCamTarget;
    public CurvePoint manualRightCamTarget;
    public List<ICameraTargetable> cameraTargets; //List of camera targets for this ground
    public bool IsFloating => floorType == FloorType.Floating;

    #region Serialization
    public SerializedGround(Ground ground)
    {
        //Save world position for each curve point
        foreach (var curvePointObj in ground.CurvePointObjects)
        {
            curvePointObj.CurvePoint.WorldPosition = curvePointObj.transform.position;
            curvePointObj.CurvePoint.name = curvePointObj.name;
        }

        name = ground.gameObject.name;
        position = ground.transform.position;
        curvePoints = ground.CurvePoints;
        lowPoints = ground.CurvePoints.Where(cp => cp.LinkedCameraTarget.doLowTarget).ToList();

        manualLeftCamTarget = ground.ManualLeftCamTarget != null ? ground.ManualLeftCamTarget.CurvePoint : null;
        manualRightCamTarget = ground.ManualRightCamTarget != null ? ground.ManualRightCamTarget.CurvePoint : null;

        if (lowPoints.Count == 0)
        {
            Debug.LogWarning($"SerializedGround: Ground {ground.name} has no lowpoint camera targets. Add some.");
        }
        else
        {
            if (manualLeftCamTarget != null)
            {
                lowPoints[0].LinkedCameraTarget.prevTarget = manualLeftCamTarget.LinkedCameraTarget;
            }

            if (manualRightCamTarget != null)
            {
                lowPoints[^1].LinkedCameraTarget = manualRightCamTarget.LinkedCameraTarget;
            }
        }

        isInverted = ground.IsInverted;
        hasShadow = ground.HasShadow;
        floorType = ground.FloorType;
        floorHeight = ground.StartFloorHeight;
        floorAngle = ground.StartFloorAngle;
        SetFloorPoints(ground);

        SerializeLevelUtility.SerializeGroundSegments(this);
    }

    public void SetFloorPoints(Ground ground)
    {
        if(ground == null || ground.CurvePoints == null || ground.CurvePoints.Count < 2)
        {
            return;
        }

        ground.CurvePoints[0].FloorPointType = FloorPointType.Set;
        ground.CurvePoints[^1].FloorPointType = FloorPointType.Set;

        switch (ground.FloorType)
        {
            case FloorType.Flat:
                SetFlatFloorPoints(ground);
                break;
            case FloorType.Slanted:
                SetSlantedFloorPoints(ground);
                break;
            case FloorType.Segmented:
                SetSegmentedFloorPoints(ground);
                break;
        }
    }

    public void SetFlatFloorPoints(Ground ground)
    {
        var yValue = ground.CurvePoints[0].Position.y - ground.StartFloorHeight;

        ground.CurvePoints[0].FloorPosition = new Vector3(ground.CurvePoints[0].WorldPosition.x, yValue, 0);
        ground.CurvePoints[^1].FloorPosition = new Vector3(ground.CurvePoints[^1].WorldPosition.x, yValue, 0);
    }

    public void SetSlantedFloorPoints(Ground ground)
    {
        ground.CurvePoints[0].FloorPosition = GroundSplineUtility.GetFloorPosition(ground.CurvePoints[0]);
        ground.CurvePoints[^1].FloorPosition = GroundSplineUtility.GetFloorPosition(ground.CurvePoints[^1]);

    }

    public void SetSegmentedFloorPoints(Ground ground)
    {
        foreach (var cp in ground.CurvePoints)
        {
            if (cp.FloorPointType != FloorPointType.None)
            {
                cp.FloorPosition = GroundSplineUtility.GetFloorPosition(cp);
            }
        }
    }

    #endregion

    #region Deserialization
    public ISerializable Deserialize(GameObject targetObject, GameObject contextObject)
    {
        var ground = targetObject.GetComponent<Ground>();
        var groundManager = contextObject.GetComponent<GroundManager>();

        if (ground == null)
        {
            Debug.LogWarning("SerializedGround: Deserialize called on a GameObject that does not have a Ground component.");
            return null;
        }

        if (groundManager == null)
        {
            Debug.LogWarning("SerializedGround: Deserialize called with a context GameObject that does not have a GroundManager component.");
            return null;
        }

        ground.name = name;
        ground.IsInverted = isInverted;
        ground.HasShadow = hasShadow;
        ground.FloorType = floorType;
        ground.StartFloorHeight = floorHeight;
        ground.StartFloorAngle = floorAngle;

        DeserializeRuntimeSegments(groundManager, ground);

#if UNITY_EDITOR

        foreach (var curvePoint in curvePoints)
        {
            ground.SetCurvePoint(curvePoint);
        }

        ground.LowPoints = ground.CurvePoints.Where( cp => cp.LinkedCameraTarget.doLowTarget).ToList();
#endif




        return ground;
    }

    public void DeserializeRuntimeSegments(GroundManager groundManager, Ground ground)
    {
        foreach (var serializedSegment in segmentList)
        {
            var segment = groundManager.groundSpawner.AddEmptySegment(ground);
            serializedSegment.Deserialize(segment, ground);
            segment.gameObject.SetActive(false);
            segment.gameObject.SetActive(true);
        }
    }
    #endregion
}
