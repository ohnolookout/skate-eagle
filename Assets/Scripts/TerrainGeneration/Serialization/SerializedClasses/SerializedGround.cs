using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializedGround : IDeserializable
{
    public Vector2 position;
    public Quaternion rotation;
    public string name;
    public bool isFloating = false;
    public bool isInverted = false;
    public bool hasShadow = true;
    public FloorType floorType;
    public int floorHeight;
    public int floorAngle;
    public SerializedGroundSegment editorSegment; //Single segment for editing
    public List<SerializedGroundSegment> segmentList; //Divided segments for runtime
    public List<CurvePoint> curvePoints;
    public List<ICameraTargetable> cameraTargets; //List of camera targets for this ground

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
        isInverted = ground.IsInverted;
        isFloating = ground.IsFloating;
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

        ground.CurvePoints[0].HasFloorPosition = true;
        ground.CurvePoints[^1].HasFloorPosition = true;

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
            if (cp.HasFloorPosition)
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
        ground.IsFloating = isFloating;
        ground.HasShadow = hasShadow;
        ground.FloorType = floorType;
        ground.StartFloorHeight = floorHeight;
        ground.StartFloorAngle = floorAngle;

        if (Application.isPlaying)
        {
            DeserializeRuntimeSegments(groundManager, ground);
        }
        else
        {
            DeserializeEditSegment(groundManager, ground);
        }

#if UNITY_EDITOR

        foreach (var curvePoint in curvePoints)
        {
            ground.SetCurvePoint(curvePoint);
        }
#endif

        return ground;
    }

    public void DeserializeEditSegment(GroundManager groundManager, Ground ground)
    {
        var segment = groundManager.groundSpawner.AddEmptySegment(ground);
        editorSegment.Deserialize(segment, ground);
    }

    private void DeserializeRuntimeSegments(GroundManager groundManager, Ground ground)
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
