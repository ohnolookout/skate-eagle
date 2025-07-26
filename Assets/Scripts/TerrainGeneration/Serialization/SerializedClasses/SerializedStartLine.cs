using UnityEngine;

public class SerializedStartLine : IDeserializable
{
    public CurvePoint curvePoint;
    public float xOffset;
    private Vector3 _startPosition;
    public Vector3 StartPosition => _startPosition;
    public Vector3 StartPositionWithOffset => StartPosition + new Vector3(xOffset, 0, 0);

    public SerializedStartLine(StartLine startLine)
    {
        curvePoint = startLine.StartPoint;
        xOffset = startLine.XOffset;

        _startPosition = curvePoint.Object.transform.position;
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
