using System.Collections.Generic;
using UnityEngine;

public class StartLine : MonoBehaviour, ISerializable, ICurvePointResync
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
        startPoint.LinkedCameraTarget.doTargetLow = true;
        _curvePoint = startPoint;
        _xOffset = xOffset;
    }

    public List<CurvePointResync> GetCurvePointResyncs()
    {
        List<CurvePointResync> resyncs = new();

        if (_curvePoint != null)
        {
            var resync = new CurvePointResync(_curvePoint);
            resync.resyncFunc = (point) => { _curvePoint = point; };
            resyncs.Add(resync);
        }

        return resyncs;
    }

    public IDeserializable Serialize()
    {
        return new SerializedStartLine(this);
    }
}
