using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using static UnityEngine.Rendering.HableCurve;

[Serializable]
public class SerializedGround : IDeserializable
{
    public Vector2 position;
    public string name;
    public List<SerializedGroundSegment> segmentList;
    public List<IDeserializable> serializedObjectList;

    public SerializedGround(string name, Vector2 position, List<IDeserializable> segmentList)
    {
        this.name = name;
        this.position = position;
        this.serializedObjectList = segmentList;
        this.segmentList = new();

        foreach (var segment in segmentList)
        {
            this.segmentList.Add(segment as SerializedGroundSegment);
        }
    }

    public SerializedGround(string name, Vector2 position, List<SerializedGroundSegment> segmentList)
    {
        this.name = name;
        this.position = position;
        this.segmentList = segmentList;
        serializedObjectList = new List<IDeserializable>();
        foreach (var segment in segmentList)
        {
            serializedObjectList.Add(segment);
        }
    }

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
        ground.SegmentList = new();

        List<CurvePoint> allCurvePoints = new();

        foreach (var serializedSegment in segmentList)
        {
            var segment = groundManager.groundSpawner.AddEmptySegment(ground);
            serializedSegment.Deserialize(segment.gameObject, ground.gameObject);

            if (segment.NextLeftSegment != null)
            {
                segment.NextLeftSegment.NextRightSegment = segment;
            }
            ground.SegmentList.Add(segment);

            if (segment.IsStart)
            {
                groundManager.StartSegment = segment;
            }

            if (segment.IsFinish)
            {
                groundManager.FinishSegment = segment;
            }


            segment.gameObject.SetActive(false);
            segment.gameObject.SetActive(true);
        }

#if UNITY_EDITOR
        bool isFirstSegment = true;
        foreach(var segment in ground.SegmentList)
        {

            if (isFirstSegment)
            {
                CurvePoint curvePoint = LocalizedCurvePointFromSegment(segment, segment.Curve.CurvePoints[0]);
                ground.AddCurvePointEditObject(curvePoint);
            }

            for (int i = 1; i < segment.curve.CurvePoints.Count - 1; i++)
            {
                CurvePoint curvePoint = LocalizedCurvePointFromSegment(segment, segment.Curve.CurvePoints[i]);
                ground.AddCurvePointEditObject(curvePoint);
            }            

        }
#endif

        return ground;
    }

    private CurvePoint LocalizedCurvePointFromSegment(GroundSegment segment, CurvePoint curvePoint)
    {
        var worldPosition = segment.transform.TransformPoint(curvePoint.Position);
        var groundLocalzedPosition = segment.parentGround.transform.InverseTransformPoint(worldPosition);
        return new CurvePoint(groundLocalzedPosition, curvePoint.LeftTangent, curvePoint.RightTangent);
    }
}
