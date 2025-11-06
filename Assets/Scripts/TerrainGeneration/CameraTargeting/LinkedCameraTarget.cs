using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LinkedCameraTarget: IResyncable
{
    [SerializeReference] public LinkedCameraTarget _prevTarget;
    [SerializeReference] public LinkedCameraTarget _nextTarget;
    [SerializeField] private ResyncRef<LinkedCameraTarget> _prevTargetRef = new();
    [SerializeField] private ResyncRef<LinkedCameraTarget> _nextTargetRef = new();
    [SerializeReference] private List<ResyncRef<LinkedCameraTarget>> _forceZoomTargetRefs = new();
    [SerializeField] private ResyncRef<ICameraTargetable> _parentObjectRef = new();


    public bool doLowTarget = false;
    public bool doUseManualOffset = false;
    public float manualYOffset = 0f;
    public float yOffset = 0f;
    public float orthoSize = 50f;
    public bool doUseManualOrthoSize = false;
    public float manualOrthoSize = 0f;
    public int[] serializedObjectLocation;
    //public Transform targetTransform;
    public ICameraTargetable _parentObject;
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

    public ICameraTargetable ParentObject
    {
        get => _parentObject;
        set
        {
            _parentObject = value;
            _parentObjectRef.Value = value;
        }
    }
    public List<ResyncRef<LinkedCameraTarget>> ForceZoomTargetRefs { get => _forceZoomTargetRefs; set => _forceZoomTargetRefs = value; }
    public LinkedCameraTarget PrevTarget
    {
        get => _prevTarget;
        set
        {
            _prevTargetRef.Value = value;
            _prevTarget = value;
        }
    }
    public LinkedCameraTarget NextTarget
    {
        get => _nextTarget;
        set
        {
            _nextTargetRef.Value = value;
            _nextTarget = value;
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
    public void RegisterResync()
    {
        //if(prevTarget != null)
        //{
        //    _prevTargetRef.Value = prevTarget;
        //}
        //if (nextTarget != null)
        //{
        //    _nextTargetRef.Value = nextTarget;
        //}

        //if(forceZoomTargets.Count > 0)
        //{
        //    _forceZoomTargetRefs = new();
        //    foreach(var point in forceZoomTargets)
        //    {
        //        _forceZoomTargetRefs.Add(new ResyncRef<CurvePoint>(point));
        //    }
        //}

        //if(parentObject != null)
        //{
        //    _parentObjectRef.Value = parentObject;
        //}

        LevelManager.ResyncHub.RegisterResync(this);
    }
}
