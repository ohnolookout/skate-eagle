using UnityEngine;

public class StartLine : MonoBehaviour, ISerializable
{
    [SerializeField] private CurvePoint _curvePoint;
    [SerializeField] private float _xOffset = 0;

    public CurvePoint CurvePoint { get => _curvePoint; set => _curvePoint = value; }
    public float XOffset {get => _xOffset; set => _xOffset = value; }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(_curvePoint.WorldPosition + new Vector3(XOffset, 0), 2f);
    }

    public void SetStartLine(SerializedStartLine serializedStartLine)
    {
        _xOffset = serializedStartLine.xOffset;
        _curvePoint = serializedStartLine.CurvePoint;
    }

    public void SetStartLine(CurvePoint startPoint, float xOffset = 0)
    {
        if(_curvePoint != null)
        {
            _curvePoint.IsStart = false; // Reset previous start point
        }

        startPoint.LinkedCameraTarget.doTargetLow = true;
        startPoint.IsStart = true; // Set new start point
        _curvePoint = startPoint;
        _xOffset = xOffset;
    }


    public IDeserializable Serialize()
    {
        return new SerializedStartLine(this);
    }
}
