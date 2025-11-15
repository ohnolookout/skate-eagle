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
    public string UID;
    public bool isInverted = false;
    public bool hasShadow = true;
    public FloorType floorType;
    public int floorHeight;
    public int floorAngle;
    public int lastObjCount;
    public List<SerializedGroundSegment> segmentList;
    public List<CurvePoint> curvePoints;
    public List<LinkedCameraTarget> lowTargets;
    public List<CurvePoint> zoomPoints;
    public List<LinkedHighPoint> highPoints;
    public LinkedCameraTarget manualLeftCamTarget;
    public LinkedCameraTarget manualRightCamTarget;
    public List<ICameraTargetable> cameraTargets; //List of camera targets for this ground
    public ResyncRef<CurvePointEditObject> leftEndTargetObjRef = new();
    public ResyncRef<CurvePointEditObject> rightEndTargetObjRef = new();
    public ResyncRef<LinkedCameraTarget> leftEndCamTargetRef = new();
    public ResyncRef<LinkedCameraTarget> rightEndCamTargetRef = new();
    public List<ResyncRef<CurvePointEditObject>> zoomPointRefs = new();
    public List<LinkedHighPoint> highTargets = new();
    public bool IsFloating => floorType == FloorType.Floating;

    #region Serialization
    public SerializedGround(Ground ground)
    {
        //Save world position for each curve point
        foreach (var curvePointObj in ground.CurvePointObjects)
        {
            curvePointObj.CurvePoint.name = curvePointObj.name;
        }

        name = ground.gameObject.name;
        UID = ground.UID;
        position = ground.transform.position;
        curvePoints = ground.CurvePoints;

        lastObjCount = ground.lastCPObjCount;
        lowTargets = ground.GetLowTargets();
        highPoints = ground.HighTargets;

        manualLeftCamTarget = ground.ManualLeftCamTarget;
        manualRightCamTarget = ground.ManualRightCamTarget;

        isInverted = ground.IsInverted;
        hasShadow = ground.HasShadow;
        floorType = ground.FloorType;
        floorHeight = ground.StartFloorHeight;
        floorAngle = ground.StartFloorAngle;
        SetFloorPoints(ground);

        CameraTargetUtility.BuildGroundCameraTargets(ground);

        //Make copies of all target refs

        foreach(var cp in curvePoints)
        {
            cp.SerializeResyncs();
        }

        leftEndCamTargetRef = ground.LeftEndCamTargetRef.FreshCopy();
        rightEndCamTargetRef = ground.RightEndCamTargetRef.FreshCopy();
        leftEndTargetObjRef = ground.LeftEndTargetObjRef.FreshCopy();
        rightEndTargetObjRef = ground.RightEndTargetObjRef.FreshCopy();
        zoomPointRefs = ground.ZoomPointRefs;
        foreach(var z in zoomPointRefs)
        {
            z.FreshCopy();
        }

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

        var endPointHeight = ground.CurvePoints[^1].Position.y - yValue;
        ground.CurvePoints[0].FloorPosition = GroundSplineUtility.GetPositionFromAngle(ground.CurvePoints[0].Position, ground.StartFloorHeight, 0);
        ground.CurvePoints[^1].FloorPosition = GroundSplineUtility.GetPositionFromAngle(ground.CurvePoints[^1].Position, (int)endPointHeight, 0);
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
        ground.UID = UID;
        ground.transform.position = position;
        ground.IsInverted = isInverted;
        ground.HasShadow = hasShadow;
        ground.FloorType = floorType;
        ground.StartFloorHeight = floorHeight;
        ground.StartFloorAngle = floorAngle;
        ground.lastCPObjCount = lastObjCount;

        ground.LeftEndCamTargetRef = leftEndCamTargetRef;
        ground.RightEndCamTargetRef = rightEndCamTargetRef;
        ground.LeftEndTargetObjRef = leftEndTargetObjRef;
        ground.RightEndTargetObjRef = rightEndTargetObjRef;
        ground.ZoomPointRefs = zoomPointRefs;

        DeserializeSegments(groundManager, ground);
        ground.LowTargets = lowTargets;
        ground.HighTargets = highPoints;

        //if(manualLeftCamTarget != null && manualLeftCamTarget.serializedObjectLocation.Count() > 1)
        if (manualLeftCamTarget != null)
        {
            ground.ManualLeftCamTarget = manualLeftCamTarget;
        }
        else
        {
            ground.ManualLeftCamTarget = null;
        }

        //if(manualRightCamTarget != null && manualRightCamTarget.serializedObjectLocation.Count() > 1)

        if (manualRightCamTarget != null)
        {
            ground.ManualRightCamTarget = manualRightCamTarget;
        }
        else
        {
            ground.ManualRightCamTarget = null;
        }


        foreach (var curvePoint in curvePoints)
        {
            curvePoint.RegisterResync();
#if UNITY_EDITOR
            ground.SetCurvePoint(curvePoint);
#endif
        }
        return ground;
    }

    public void DeserializeSegments(GroundManager groundManager, Ground ground)
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
