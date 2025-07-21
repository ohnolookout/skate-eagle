using UnityEngine;

public class StartLine : MonoBehaviour, ISerializable
{
    [SerializeField] private CurvePoint _startPoint;
    [SerializeField] private float _xOffset = 0;

    public CurvePoint StartPoint { get => _startPoint; set => _startPoint = value; }
    public Vector3 StartPosition => new(_startPoint.Position.x + XOffset, _startPoint.Position.y);
    public float XOffset {get => _xOffset; set => _xOffset = value; }

    public void SetStartLine(SerializedStartLine serializedStartLine)
    {
        _startPoint = serializedStartLine.curvePoint;
        _xOffset = serializedStartLine.xOffset;
    }
    
    public void SetStartLine(CurvePoint startPoint, float xOffset = 0)
    {
        _startPoint = startPoint;
        _xOffset = xOffset;
    }

    public IDeserializable Serialize()
    {
        return new SerializedStartLine(_startPoint, _xOffset);
    }
}
