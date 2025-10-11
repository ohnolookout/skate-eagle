using System;
using UnityEngine;

[Serializable]
public class SerializedStartLine : IDeserializable
{
    public float xOffset;
    [SerializeField] private CurvePoint _curvePoint;
    [SerializeField] private Vector3 _camStartPosition = new();
    [SerializeField] private float _camOrthoSize = 50;
    [SerializeField] private LinkedCameraTarget _firstCameraTarget;
    public Vector3 StartPosition => _curvePoint.WorldPosition;
    public Vector3 StartPositionWithOffset => StartPosition + new Vector3(xOffset, 0, 0);
    public CurvePoint CurvePoint => _curvePoint;
    public Vector3 CamStartPosition => _camStartPosition;
    public float CamOrthoSize => _camOrthoSize;
    public LinkedCameraTarget FirstCameraTarget => _firstCameraTarget;

    public SerializedStartLine(StartLine startLine)
    {
        xOffset = startLine.XOffset;
        startLine.CurvePoint.LinkedCameraTarget.doLowTarget = true;

        _curvePoint = startLine.CurvePoint;
        _curvePoint.SaveWorldPosition();
        _camStartPosition = startLine.CamStartPosition;
        _camOrthoSize = startLine.CamOrthoSize;
        _firstCameraTarget = startLine.FirstCameraTarget;
                
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
