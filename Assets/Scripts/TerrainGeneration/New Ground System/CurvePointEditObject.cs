using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

public class CurvePointEditObject : MonoBehaviour, ICameraTargetable, IObjectResync //Add CurvePointResync for LinkedTarget left and right targets
{
    #region Declarations
    private Ground _parentGround;
    public List<GameObject> rightTargetObjects = new();
    public List<GameObject> leftTargetObjects = new();
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
    public List<GameObject> ZoomTargetObjects { get => zoomTargetObjects; set => zoomTargetObjects = value; }
    public GameObject ManualLeftTargetObject { get => manualLeftTargetObject; set => manualLeftTargetObject = value; }
    public GameObject ManualRightTargetObject { get => manualRightTargetObject; set => manualRightTargetObject = value; }
    public List<GameObject> RightTargetObjects { get => rightTargetObjects; set => rightTargetObjects = value; }
    public List<GameObject> LeftTargetObjects { get => leftTargetObjects; set => leftTargetObjects = value; }
    public LinkedCameraTarget LinkedCameraTarget { get => CurvePoint.LinkedCameraTarget; set => CurvePoint.LinkedCameraTarget = value; }
    public bool DoTargetLow { get => LinkedCameraTarget.doLowTarget; set => LinkedCameraTarget.doLowTarget = value; }
    public bool DoTargetHigh { get => LinkedCameraTarget.doZoomTarget; set => LinkedCameraTarget.doZoomTarget = value; }

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
        if (!DoTargetLow && !DoTargetHigh)
        {
            return;
        }
    }

    #endregion

    #region Set Values
    public void SetCurvePoint(CurvePoint curvePoint)
    {
        curvePoint.Object = gameObject; // Set the object reference in the CurvePoint
        transform.position = curvePoint.Position + ParentGround.transform.position;
        GenerateTarget();
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
    public void GenerateTarget()
    {
        LinkedCameraTarget.TargetTransform = transform;
        LinkedCameraTarget.SerializedPosition = transform.position;
    }

    public void PopulateDefaultTargets() //Figure out if I need to run this on deserialization
    {
        GenerateTarget();

        if (!LinkedCameraTarget.doLowTarget && !LinkedCameraTarget.doZoomTarget)
        {
            leftTargetObjects = new();
            rightTargetObjects = new();
            //LinkedCameraTarget.LeftTargets = new();
            //LinkedCameraTarget.RightTargets = new();
            return;
        }

        if (NextLeftCurvePointObject != null && !leftTargetObjects.Contains(NextLeftCurvePointObject.gameObject))
        {
            leftTargetObjects.Add(NextLeftCurvePointObject.gameObject);
        }

        if (NextRightCurvePointObject != null && !rightTargetObjects.Contains(NextRightCurvePointObject.gameObject))
        {
            rightTargetObjects.Add(NextRightCurvePointObject.gameObject);
        }

        leftTargetObjects = CleanTargetObjectList(leftTargetObjects);
        rightTargetObjects = CleanTargetObjectList(rightTargetObjects);
    }

    private List<GameObject> CleanTargetObjectList(List<GameObject> targetObjects)
    {
        targetObjects = targetObjects.Distinct().ToList(); // Remove duplicates

        for (int i = 0; i < targetObjects.Count; i++)
        {
            var targetable = targetObjects[i].GetComponentInChildren<ICameraTargetable>();
            if (targetable == null || !targetable.DoTargetLow)
            {
                targetObjects.RemoveAt(i);
                i--; // Adjust index after removal
            }
        }

        return targetObjects;
    }

    public List<ObjectResync> GetObjectResyncs()
    {
        List<ObjectResync> resyncs = new();

        if (!LinkedCameraTarget.doLowTarget)
        {
            return resyncs;
        }

        var targets = new List<LinkedCameraTarget>();

        targets.AddRange(LinkedCameraTarget.forceZoomTargets.Select(t => t.LinkedCameraTarget));
        
        if(LinkedCameraTarget.prevTarget != null)
        {
            targets.Add(LinkedCameraTarget.prevTarget);
        }
        if(LinkedCameraTarget.nextTarget != null)
        {
            targets.Add(LinkedCameraTarget.nextTarget);
        }
        //targets.AddRange(LinkedCameraTarget.LeftTargets);
        //targets.AddRange(LinkedCameraTarget.RightTargets);

        foreach (var target in targets)
        {
            var resync = new ObjectResync(target.SerializedObjectLocation);
            resync.resyncFunc = (obj) => 
            {
                target.TargetTransform = obj.transform;
            };
            resyncs.Add(resync);
        }

        return resyncs;
    }
    #endregion
}



