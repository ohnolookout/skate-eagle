using System;
using UnityEngine;

[Serializable]
public class SerializedFinishLine: IDeserializable
{
    //public int flagPointIndex;
    public CurvePoint flagPoint;
    public int flagPointXOffset;
    //public int backstopPointIndex;
    public CurvePoint backstopPoint;
    public int backstopPointXOffset;
    public bool backstopIsActive;
    public Vector2 flagPosition;
    public Vector2 backstopPosition;
    public bool isForward;

    public SerializedFinishLine(int flagPointIndex, int flagPointXOffset,
                                int backstopPointIndex, int backstopPointXOffset,
                                bool backstopIsActive, Vector2 flagPosition, Vector2 backstopPosition)
    {
        //this.flagPointIndex = flagPointIndex;
        this.flagPointXOffset = flagPointXOffset;
        //this.backstopPointIndex = backstopPointIndex;
        this.backstopPointXOffset = backstopPointXOffset;
        this.backstopIsActive = backstopIsActive;
        this.flagPosition = flagPosition;
        this.backstopPosition = backstopPosition;
        isForward = flagPosition.x < backstopPosition.x;
    }

    public ISerializable Deserialize(GameObject targetObject, GameObject contextObject)
    {
        var finishLine = targetObject.GetComponent<FinishLine>();
        var groundManager = contextObject.GetComponent<GroundManager>();

        if (finishLine == null)
        {
            Debug.LogWarning("SerializedFinishLine: Deserialize called on a GameObject that does not have a FinishLine component.");
            return null;
        }

        if (groundManager == null)
        {
            Debug.LogWarning("SerializedFinishLine: Deserialize called with a context GameObject that does not have a GroundManager component.");
            return null;
        }

        finishLine.SetFinishLine(this);

        return finishLine;
    }
}
