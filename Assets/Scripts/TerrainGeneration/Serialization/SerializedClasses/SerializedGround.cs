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
    public SerializedGroundSegment editorSegment; //Single segment for editing
    public List<SerializedGroundSegment> segmentList; //Divided segments for runtime
    public List<CurvePoint> curvePointList;
    //public List<IDeserializable> serializedObjectList;

    public SerializedGround(string name, Vector2 position, List<SerializedGroundSegment> segmentList, SerializedGroundSegment editSegment, List<CurvePoint> curvePointList)
    {
        this.name = name;
        this.position = position;
        this.segmentList = new();
        this.curvePointList = curvePointList;

        foreach (var segment in segmentList)
        {
            this.segmentList.Add(segment as SerializedGroundSegment);
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
        //curvePointList = SerializeLevelUtility.GenerateCurvePointListFromGround(this);

        foreach (var curvePoint in curvePointList)
        {
            ground.AddCurvePointEditObject(curvePoint);
        }
#endif

        return ground;
    }

}
