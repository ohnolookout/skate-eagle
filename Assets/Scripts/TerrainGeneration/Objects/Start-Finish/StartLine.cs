using UnityEngine;

public class StartLine : MonoBehaviour, ISerializable
{
    [SerializeField] private CurvePoint _curvePoint;
    [SerializeField] private float _xOffset = 0;
    [SerializeField] private Vector3 _serializedPosition;

    public CurvePoint CurvePoint { get => _curvePoint; set => _curvePoint = value; }
    public Vector3 StartPosition => _serializedPosition;
    public float XOffset {get => _xOffset; set => _xOffset = value; }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        if (_curvePoint != null && _curvePoint.Object != null)
        {
            Gizmos.DrawSphere(_curvePoint.Object.transform.position + new Vector3(XOffset, 0), 2f);
        } else if (_curvePoint != null){
            Gizmos.DrawSphere(_curvePoint.Position + new Vector3(XOffset, 0), 2f);
        }
        else
        {
            Gizmos.DrawSphere(_serializedPosition + new Vector3(XOffset, 0), 2f);
        }
    }

    public void SetStartLine(SerializedStartLine serializedStartLine)
    {
        _xOffset = serializedStartLine.xOffset;
        _serializedPosition = serializedStartLine.StartPosition;

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            Debug.Log("Setting start line curve point in editor mode.");
            _curvePoint = serializedStartLine.CurvePoint;
            Debug.Log("CurvePoint set to: " + _curvePoint);
        }
    }
#endif

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
