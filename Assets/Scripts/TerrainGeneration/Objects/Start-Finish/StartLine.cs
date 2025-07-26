using UnityEngine;

public class StartLine : MonoBehaviour, ISerializable
{
    [SerializeField] private CurvePoint _startPoint;
    [SerializeField] private float _xOffset = 0;
    [SerializeField] private Vector3 _startPosition;

    public CurvePoint StartPoint { get => _startPoint; set => _startPoint = value; }
    public Vector3 StartPosition => _startPosition;
    public float XOffset {get => _xOffset; set => _xOffset = value; }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        if (_startPoint != null && _startPoint.Object != null)
        {
            Gizmos.DrawSphere(_startPoint.Object.transform.position + new Vector3(XOffset, 0), 2f);
        }
    }

    public void SetStartLine(SerializedStartLine serializedStartLine)
    {
        _xOffset = serializedStartLine.xOffset;
        _startPosition = serializedStartLine.StartPosition;
    }
    
    public void SetStartLine(CurvePoint startPoint, float xOffset = 0)
    {
        if(_startPoint != null)
        {
            _startPoint.IsStart = false; // Reset previous start point
        }

        startPoint.IsStart = true; // Set new start point
        _startPoint = startPoint;
        _xOffset = xOffset;
    }


    public IDeserializable Serialize()
    {
        return new SerializedStartLine(this);
    }
}
