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
    private ResyncRef<CurvePoint> _curvePointRef = new();
    private ResyncRef<LinkedCameraTarget> _firstCamTargetRef = new();
    private ResyncRef<LinkedHighPoint> _firstHighPointRef = new();
    public string UID { get; set; }

    public Vector3 StartPosition => _curvePoint.WorldPosition;
    public Vector3 StartPositionWithOffset => StartPosition + new Vector3(_xOffset, 0, 0);
    public GameObject GameObject => gameObject;

    public CurvePoint CurvePoint
    {
        get => _curvePoint;
        set
        {
            _curvePointRef.Value = value;
            _curvePoint = value;
        }
    }
    public float XOffset { get => _xOffset; set => _xOffset = value; }
    public Vector3 CamStartPosition { get => _camStartPosition; set => _camStartPosition = value; }
    public float CamOrthoSize { get => _camOrthoSize; set => _camOrthoSize = value; }
    public LinkedCameraTarget FirstCameraTarget
    {
        get => _firstCameraTarget;
        set
        {
            _firstCamTargetRef.Value = value;
            _firstCameraTarget = value;
        }
    }
    public LinkedHighPoint FirstHighPoint
    {
        get => _firstHighPoint;
        set
        {
            _firstHighPointRef.Value = value;
            _firstHighPoint = value;
        }
    }

    public ResyncRef<CurvePoint> CurvePointRef { get => _curvePointRef; set => _curvePointRef = value; }
    public ResyncRef<LinkedCameraTarget> FirstCamTargetRef { get => _firstCamTargetRef; set => _firstCamTargetRef = value; }
    public ResyncRef<LinkedHighPoint> FirstHighPointRef { get => _firstHighPointRef; set => _firstHighPointRef = value; }

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
        _curvePointRef = serializedStartLine.curvePointRef;
        _firstCamTargetRef = serializedStartLine.firstCameraTargetRef;
        _firstHighPointRef = serializedStartLine.firstHighPointRef;
    }

    public void SetStartLine(CurvePoint startPoint, float xOffset = 0)
    {
#if UNITY_EDITOR
        Undo.RecordObject(this, "Set Start Point");
#endif
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

        if (FirstCameraTarget != null)
        {
            var resync = new ObjectResync(FirstCameraTarget.serializedObjectLocation);
            resync.resyncFunc = (obj) => { FirstCameraTarget.parentObject = obj.GetComponent<ICameraTargetable>(); };
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

    public void RegisterResync()
    {
        if(_curvePoint != null)
        {
            _curvePointRef.Value = _curvePoint;
        }
        if(_firstCameraTarget != null)
        {
            _firstCamTargetRef.Value = _firstCameraTarget;
        }
        if(_firstHighPoint != null)
        {
            _firstHighPointRef.Value = _firstHighPoint;
        }
        LevelManager.ResyncHub.RegisterResync(this);
    }
}
