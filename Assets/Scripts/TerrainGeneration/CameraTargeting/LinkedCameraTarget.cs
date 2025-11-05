using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LinkedCameraTarget: IResyncable
{
    [SerializeReference] public List<CurvePoint> forceZoomTargets = new();
    [SerializeReference] public LinkedCameraTarget prevTarget;
    [SerializeReference] public LinkedCameraTarget nextTarget;
    [SerializeField] private ResyncRef<LinkedCameraTarget> _prevTargetRef = new();
    [SerializeField] private ResyncRef<LinkedCameraTarget> _nextTargetRef = new();
    [SerializeReference] private List<ResyncRef<CurvePoint>> _forceZoomTargetRefs = new();
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
    public ICameraTargetable parentObject;
    public string UID { get; set; }
    public Vector3 Position
    {
        get
        {
            if (parentObject != null)
            {
                return SerializedPosition = parentObject.Object.transform.position;
            }
            else
            {
                return SerializedPosition;
            }
        }
    }

    public Vector3 CamBottomPosition => Position - new Vector3(0, yOffset * CameraTargetUtility.DefaultOrthoSize);
    public Vector3 SerializedPosition;
    public LinkedCameraTarget()
    {
        serializedObjectLocation = new int[0];
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
