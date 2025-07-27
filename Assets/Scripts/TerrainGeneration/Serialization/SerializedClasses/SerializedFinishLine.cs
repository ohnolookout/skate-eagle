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

    public SerializedFinishLine(FinishLine finishLine)
    {
        flagPoint = finishLine.FlagPoint;
        flagPointXOffset = finishLine.FlagXOffset;
        backstopPoint = finishLine.BackstopPoint;
        backstopPointXOffset = finishLine.BackstopXOffset;
        backstopIsActive = finishLine.BackstopIsActive;
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
