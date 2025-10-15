using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class StartLine : MonoBehaviour, ISerializable, IObjectResync
{
    [SerializeField] private CurvePoint _curvePoint;
    [SerializeField] private float _xOffset = 0;
    [SerializeField] private Vector3 _camStartPosition = new();
    [SerializeField] private float _camOrthoSize = 50;
    private LinkedCameraTarget _firstCameraTarget;
    private LinkedHighPoint _firstHighPoint;
    public GameObject GameObject => gameObject;

    public CurvePoint CurvePoint { get => _curvePoint; set => _curvePoint = value; }
    public float XOffset {get => _xOffset; set => _xOffset = value; }
    public Vector3 CamStartPosition { get => _camStartPosition; set => _camStartPosition = value; }
    public float CamOrthoSize { get => _camOrthoSize; set => _camOrthoSize = value; }
    public LinkedCameraTarget FirstCameraTarget { get => _firstCameraTarget; set => _firstCameraTarget = value; }
    public LinkedHighPoint FirstHighPoint { get => _firstHighPoint; set => _firstHighPoint = value; }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(_curvePoint.WorldPosition + new Vector3(XOffset, 0), 2f);
    }

    public void SetStartLine(SerializedStartLine serializedStartLine)
    {
        _xOffset = serializedStartLine.xOffset;
        _curvePoint = serializedStartLine.CurvePoint;
        _camStartPosition = serializedStartLine.CamStartPosition;
        _camOrthoSize = serializedStartLine.CamOrthoSize;
        _firstCameraTarget = serializedStartLine.FirstCameraTarget;
        _firstHighPoint = serializedStartLine.FirstHighPoint;
    }

    public void SetStartLine(CurvePoint startPoint, float xOffset = 0)
    {
#if UNITY_EDITOR
        Undo.RecordObject(this, "Set Start Point");
#endif
        startPoint.LinkedCameraTarget.doLowTarget = true;
        _curvePoint = startPoint;
        _xOffset = xOffset;
    }

    public List<ObjectResync> GetObjectResyncs()
    {
        List<ObjectResync> resyncs = new();

        if (_curvePoint != null)
        {
            var resync = new ObjectResync(_curvePoint.LinkedCameraTarget.serializedObjectLocation);
            resync.resyncFunc = (obj) => { _curvePoint.Object = obj; };
            resyncs.Add(resync);
        }

        if (FirstCameraTarget != null && FirstCameraTarget.parentObject != null)
        {
            var resync = new ObjectResync(FirstCameraTarget.serializedObjectLocation);
            resync.resyncFunc = (obj) => { FirstCameraTarget.parentObject.Object.GetComponent<CurvePoint>().Object = obj; };
            resyncs.Add(resync);
        }

        return resyncs;
    }

    public IDeserializable Serialize()
    {
        return new SerializedStartLine(this);
    }

    public void Clear()
    {
        _curvePoint = null;
        _xOffset = 0;
    }

    public void Refresh(GroundManager _ = null)
    {
        return;
    }

#if UNITY_EDITOR
    public bool IsParentGround(GameObject obj)
    {
        if (_curvePoint.Object == null)
        {
            return false;
        }

        var cpObj = _curvePoint.Object.GetComponent<CurvePointEditObject>();
        return obj.GetComponent<Ground>() == cpObj.ParentGround;
    }

#endif
}
