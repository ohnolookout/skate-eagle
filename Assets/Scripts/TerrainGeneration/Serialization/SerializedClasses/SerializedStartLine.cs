using UnityEngine;

public class SerializedStartLine : IDeserializable
{
    public CurvePoint curvePoint;
    public float xOffset;

    public SerializedStartLine(CurvePoint curvePoint, float xOffset)
    {
        this.curvePoint = curvePoint;
        this.xOffset = xOffset;
    }

    public ISerializable Deserialize(GameObject targetObject, GameObject contextObject)
    {
        var startLine = targetObject.GetComponent<StartLine>();
        var groundManager = contextObject.GetComponent<GroundManager>();

        if (startLine == null)
        {
            Debug.LogWarning("SerializedFinishLine: Deserialize called on a GameObject that does not have a FinishLine component.");
            return null;
        }

        if (groundManager == null)
        {
            Debug.LogWarning("SerializedFinishLine: Deserialize called with a context GameObject that does not have a GroundManager component.");
            return null;
        }

        startLine.SetStartLine(this);

        return startLine;
    }
}
