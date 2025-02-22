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

    public SerializedGround(string name, Vector2 position, List<SerializedGroundSegment> segmentList)
    {
        this.name = name;
        this.position = position;
        this.segmentList = segmentList;
    }
}
