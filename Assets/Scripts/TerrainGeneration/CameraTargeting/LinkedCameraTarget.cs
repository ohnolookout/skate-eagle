using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LinkedCameraTarget: IResyncable
{
    [SerializeField] private ResyncRef<LinkedCameraTarget> _prevTargetRef = new();
    [SerializeField] private ResyncRef<LinkedCameraTarget> _nextTargetRef = new();
    [SerializeReference] private List<ResyncRef<LinkedCameraTarget>> _forceZoomTargetRefs = new();
    [SerializeField] private ResyncRef<CurvePointEditObject> _parentObjectRef = new();


    public bool doLowTarget = false;
    public bool doUseManualOffset = false;
    public float manualYOffset = 0f;
    public float yOffset = 0f;
    public float orthoSize = 50f;
    public bool doUseManualOrthoSize = false;
    public float manualOrthoSize = 0f;
    public int[] serializedObjectLocation;
    public CurvePointEditObject _parentObject;
    public string UID { get; set; }
    public Vector3 Position
    {
        get
        {
            if (ParentObject != null)
            {
                return SerializedPosition = ParentObject.Object.transform.position;
            }
            else
            {
                return SerializedPosition;
            }
        }
    }

    public CurvePointEditObject ParentObject
    {
        get => _parentObjectRef.Value;
        set
        {
            _parentObject = value;
            _parentObjectRef.Value = value;
        }
    }
    public List<ResyncRef<LinkedCameraTarget>> ForceZoomTargetRefs { get => _forceZoomTargetRefs; set => _forceZoomTargetRefs = value; }
    public LinkedCameraTarget PrevTarget
    {
        get => _prevTargetRef.Value;
        set
        {
            _prevTargetRef.Value = value;
        }
    }
    public LinkedCameraTarget NextTarget
    {
        get => _nextTargetRef.Value;
        set
        {
            _nextTargetRef.Value = value;
        }
    }

    public Vector3 CamBottomPosition => Position - new Vector3(0, yOffset * CameraTargetUtility.DefaultOrthoSize);
    public Vector3 SerializedPosition;
    public LinkedCameraTarget()
    {
        serializedObjectLocation = new int[0];
    }

    public List<LinkedCameraTarget> GetZoomTargets()
    {
        List<LinkedCameraTarget> zoomTargets = new();
        foreach (var targetRef in _forceZoomTargetRefs)
        {
            zoomTargets.Add(targetRef.Value);
        }

        return zoomTargets;
    }

    public void SerializeResyncs()
    {
        if (_prevTargetRef != null)
        {
            _prevTargetRef = _prevTargetRef.FreshCopy();
        }

        if (_nextTargetRef != null)
        {
            _nextTargetRef = _nextTargetRef.FreshCopy();
        }

        if (_parentObjectRef != null)
        {
            _parentObjectRef = _parentObjectRef.FreshCopy();
        }

        for (int i = 0; i < _forceZoomTargetRefs.Count; i++)
        {
            if (_forceZoomTargetRefs[i] != null)
            {
                _forceZoomTargetRefs[i] = _forceZoomTargetRefs[i].FreshCopy();
            }
        }
    }

    public void RegisterResync()
    {
        LevelManager.ResyncHub.RegisterResync(this);
    }
}
