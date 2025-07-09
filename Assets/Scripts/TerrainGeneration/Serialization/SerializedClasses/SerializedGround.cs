using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializedGround : IDeserializable
{
    public Vector2 position;
    public string name;
    public List<SerializedGroundSegment> serializedSegmentList;
    public List<IDeserializable> serializedObjectList;

    public SerializedGround(string name, Vector2 position, List<IDeserializable> segmentList)
    {
        this.name = name;
        this.position = position;
        this.serializedObjectList = segmentList;
    }

    public SerializedGround(string name, Vector2 position, List<SerializedGroundSegment> segmentList)
    {
        this.name = name;
        this.position = position;
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

        foreach (var serializedSegment in serializedSegmentList)
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

        return ground;
    }
}
