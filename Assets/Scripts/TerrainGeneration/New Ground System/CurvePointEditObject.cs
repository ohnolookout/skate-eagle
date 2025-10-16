using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

public class CurvePointEditObject : MonoBehaviour, ICameraTargetable, IObjectResync //Add CurvePointResync for LinkedTarget left and right targets
{
    #region Declarations
    private Ground _parentGround;
    public List<GameObject> zoomTargetObjects = new();
    public GameObject manualLeftTargetObject;
    public GameObject manualRightTargetObject;
    public CurvePoint CurvePoint => _parentGround.CurvePoints[transform.GetSiblingIndex()];
    public Ground ParentGround {
        get
        {
            if(_parentGround == null)
            {
                _parentGround = transform.parent.parent.GetComponent<Ground>();
            }
            return _parentGround;
        }
        set => _parentGround = value; }
    public GameObject Object => gameObject;
    public List<GameObject> ZoomTargetObjects { get => zoomTargetObjects; set => zoomTargetObjects = value; }
    public GameObject ManualLeftTargetObject { get => manualLeftTargetObject; set => manualLeftTargetObject = value; }
    public GameObject ManualRightTargetObject { get => manualRightTargetObject; set => manualRightTargetObject = value; }
    public LinkedCameraTarget LinkedCameraTarget { get => CurvePoint.LinkedCameraTarget; set => CurvePoint.LinkedCameraTarget = value; }
    public bool DoTargetLow { get => LinkedCameraTarget.doLowTarget; set => LinkedCameraTarget.doLowTarget = value; }

    public float LeftTangentMagnitude => CurvePoint.LeftTangent.magnitude;
    public float RightTangentMagnitude => CurvePoint.RightTangent.magnitude;
    public float LeftTangentAngle => CurvePoint.LeftTangent == Vector3.zero ? 0 : Vector3.SignedAngle(Vector3.right, CurvePoint.LeftTangent, Vector3.forward);
    public float RightTangentAngle => CurvePoint.RightTangent == Vector3.zero ? 0 : Vector3.SignedAngle(Vector3.right, CurvePoint.RightTangent, Vector3.forward);

    public CurvePointEditObject NextLeftCurvePointObject 
    { 
        get  
        {
            var index = transform.GetSiblingIndex();
            
            if (index != 0)
            {
                for(int i = index - 1; i >= 0; i--)
                {
                    if (ParentGround.CurvePointObjects[i].DoTargetLow)
                    {
                        return ParentGround.CurvePointObjects[i];
                    }
                }
            }

            return null;
        }
    }
    public CurvePointEditObject NextRightCurvePointObject
    {
        get
        {
            var index = transform.GetSiblingIndex();

            for (int i = index + 1; i < ParentGround.CurvePointObjects.Length; i++)
            {
                if (ParentGround.CurvePointObjects[i].DoTargetLow)
                {
                    return ParentGround.CurvePointObjects[i];
                }
            }

            return null;
        }
    }
    #endregion

    #region Monobehaviors
    void OnDrawGizmosSelected()
    {
    }

    #endregion

    #region Set Values
    public void SetCurvePoint(CurvePoint curvePoint)
    {        
        curvePoint.Object = gameObject; // Set the object reference in the CurvePoint
        transform.position = curvePoint.Position + ParentGround.transform.position;
        AddObjectToTarget();
    }

#if UNITY_EDITOR
    public void TangentsChanged(Vector3 updatedleftTang, Vector3 updatedRightTang)
    {
        CurvePoint.LeftTangent = updatedleftTang - transform.position;
        CurvePoint.RightTangent = updatedRightTang - transform.position;

    }

    public void LeftTangentChanged(Vector3 updatedTang)
    {
        Undo.RecordObject(this, "Tangent Changed");
        CurvePoint.LeftTangent = updatedTang - transform.position;

        if (CurvePoint.IsSymmetrical)
        {
            CurvePoint.RightTangent = -CurvePoint.LeftTangent; // If it's symmetrical, update the right tangent accordingly
        }
        else if (CurvePoint.TangentMode == ShapeTangentMode.Continuous)
        {
            var rightMagnitude = CurvePoint.RightTangent.magnitude;
            CurvePoint.RightTangent = -CurvePoint.LeftTangent.normalized * rightMagnitude; // Maintain the same magnitude for the left tangent
        }

    }

    public void RightTangentChanged(Vector3 updatedTang)
    {
        Undo.RecordObject(this, "Tangent Changed");
        CurvePoint.RightTangent = updatedTang - transform.position;

        if(CurvePoint.IsSymmetrical)
        {
            CurvePoint.LeftTangent = -CurvePoint.RightTangent; // If it's symmetrical, update the right tangent accordingly
        } else if (CurvePoint.TangentMode == ShapeTangentMode.Continuous)
        {
            var leftMagnitude = CurvePoint.LeftTangent.magnitude;
            CurvePoint.LeftTangent = -CurvePoint.RightTangent.normalized * leftMagnitude; // Maintain the same magnitude for the left tangent
        }
        
    }

    public void PositionChanged(Vector3 updatedPosition)
    {
        Undo.RecordObject(this, "Curve Point Position Changed");
        transform.position = updatedPosition;
        CurvePoint.Position = updatedPosition - ParentGround.transform.position;
    }

    public void TangentSettingsChanged(ShapeTangentMode mode, bool isSymmetrical)
    {
        Undo.RecordObject(this, "Tangents Changed");
        CurvePoint.TangentMode = mode;
        CurvePoint.IsSymmetrical = isSymmetrical;
        if (isSymmetrical)
        {
            CurvePoint.RightTangent = -CurvePoint.LeftTangent; // If it's symmetrical, update the right tangent accordingly
        }
        else if (CurvePoint.TangentMode == ShapeTangentMode.Continuous)
        {
            var rightMagnitude = CurvePoint.RightTangent.magnitude;
            CurvePoint.RightTangent = -CurvePoint.LeftTangent.normalized * rightMagnitude; // Maintain the same magnitude for the left tangent
        }
    }
#endif
#endregion

    #region Targeting
    public void AddObjectToTarget()
    {
        LinkedCameraTarget.targetTransform = transform;
        LinkedCameraTarget.SerializedPosition = transform.position;
        LinkedCameraTarget.parentObject = this;
    }

    public List<ObjectResync> GetObjectResyncs()
    {
        List<ObjectResync> resyncs = new();

        if (!LinkedCameraTarget.doLowTarget)
        {
            return resyncs;
        }

        var targets = new List<LinkedCameraTarget>();

        targets.Add(LinkedCameraTarget);

        targets.AddRange(LinkedCameraTarget.forceZoomTargets.Select(t => t.LinkedCameraTarget));

        foreach (var target in targets)
        {
            var resync = new ObjectResync(target.serializedObjectLocation);
            resync.resyncFunc = (obj) => 
            {
                target.targetTransform = obj.transform;
            };
            resyncs.Add(resync);
        }

        return resyncs;
    }
    #endregion
}



