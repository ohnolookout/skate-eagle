using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class StartLine : MonoBehaviour, ISerializable, IObjectResync
{
    [SerializeField] private CurvePoint _curvePoint;
    [SerializeField] private float _xOffset = 0;
    public GameObject GameObject => gameObject;

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
#if UNITY_EDITOR
        Undo.RecordObject(this, "Set Start Point");
#endif
        startPoint.LinkedCameraTarget.doTargetLow = true;
        _curvePoint = startPoint;
        _xOffset = xOffset;
    }

    public List<ObjectResync> GetObjectResyncs()
    {
        List<ObjectResync> resyncs = new();

        if (_curvePoint != null)
        {
            var resync = new ObjectResync(_curvePoint.LinkedCameraTarget.SerializedLocation);
            resync.resyncFunc = (obj) => { _curvePoint.Object = obj; };
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
