using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializedGround
{
    public Vector2 position;
    public string name;
    public List<SerializedGroundSegment> segmentList;

    public SerializedGround(Ground ground)
    {
        name = ground.gameObject.name;
        position = ground.transform.position;
        segmentList = new List<SerializedGroundSegment>();
        foreach (GroundSegment segment in ground.SegmentList)
        {
            segmentList.Add(new SerializedGroundSegment(segment));
        }
    }
}
