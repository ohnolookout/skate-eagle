using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class StartLine : MonoBehaviour, ISerializable
{
    [SerializeField] private float _xOffset = 0;
    [SerializeField] private Vector3 _camStartPosition = new();
    [SerializeField] private float _camOrthoSize = 50;
    private LinkedHighPoint _firstHighPoint;
    private ResyncRef<CurvePoint> _curvePointRef = new();
    private ResyncRef<LinkedCameraTarget> _firstCamTargetRef = new();
    private ResyncRef<Ground> _groundRef = new();
    public string UID { get; set; }

    public Vector3 StartPosition => CurvePoint.WorldPosition;
    public Vector3 StartPositionWithOffset => StartPosition + new Vector3(_xOffset, 0, 0);
    public GameObject GameObject => gameObject;
    public CurvePoint CurvePoint { get => _curvePointRef.Value; set => _curvePointRef.Value = value; }
    public Ground Ground { get => _groundRef.Value; set => _groundRef.Value = value; }
    public float XOffset { get => _xOffset; set => _xOffset = value; }
    public Vector3 CamStartPosition { get => _camStartPosition; set => _camStartPosition = value; }
    public float CamOrthoSize { get => _camOrthoSize; set => _camOrthoSize = value; }
    public LinkedCameraTarget FirstCameraTarget { get => _firstCamTargetRef.Value; set => _firstCamTargetRef.Value = value; }    
    public LinkedHighPoint FirstHighPoint { get => _firstHighPoint; set => _firstHighPoint = value; }
    public ResyncRef<CurvePoint> CurvePointRef { get => _curvePointRef; set => _curvePointRef = value; }
    public ResyncRef<LinkedCameraTarget> FirstCamTargetRef { get => _firstCamTargetRef; set => _firstCamTargetRef = value; }
    public ResyncRef<Ground> GroundRef { get => _groundRef; set => _groundRef = value; }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(CurvePoint.WorldPosition + new Vector3(XOffset, 0), 2f);
    }

    public void SetStartLine(SerializedStartLine serializedStartLine)
    {
        _xOffset = serializedStartLine.xOffset;
        _camStartPosition = serializedStartLine.CamStartPosition;
        _camOrthoSize = serializedStartLine.CamOrthoSize;
        _firstHighPoint = serializedStartLine.FirstHighPoint;
        CurvePointRef = serializedStartLine.curvePointRef;
        FirstCamTargetRef = serializedStartLine.firstCameraTargetRef;
        GroundRef = serializedStartLine.groundRef;
        UID = serializedStartLine.uid;
    }

    public void SetStartLine(CurvePoint startPoint, float xOffset = 0)
    {
#if UNITY_EDITOR
        Undo.RecordObject(this, "Set Start Point");
#endif
        CurvePoint = startPoint;
        _xOffset = xOffset;

        if (CurvePoint.CPObject != null)
        {
            Ground = CurvePoint.CPObject.ParentGround;
            Debug.Log("Ground saved to startline: " + Ground.name);
        }
    }

    public IDeserializable Serialize()
    {
        return new SerializedStartLine(this);
    }

    public void Clear()
    {
        CurvePoint = null;
        _xOffset = 0;
    }

    public void Refresh(GroundManager _ = null)
    {
        return;
    }

    public bool IsParentGround(GameObject obj)
    {
        return obj.GetComponent<Ground>() == Ground;
    }

    public void RegisterResync()
    {
        LevelManager.ResyncHub.RegisterResync(this);
    }
}
