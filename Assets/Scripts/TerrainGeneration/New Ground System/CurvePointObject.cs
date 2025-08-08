using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.U2D;

public class CurvePointObject : MonoBehaviour, ICameraTargetable, IObjectResync //Add CurvePointResync for LinkedTarget left and right targets
{
    #region Declarations
    public CurvePoint curvePoint;
    private Ground _parentGround;
    public List<GameObject> rightTargetObjects = new();
    public List<GameObject> leftTargetObjects = new();
    public CurvePoint CurvePoint => curvePoint;
    public Ground ParentGround { get => _parentGround; set => _parentGround = value; }
    public List<GameObject> RightTargetObjects { get => rightTargetObjects; set => rightTargetObjects = value; }
    public List<GameObject> LeftTargetObjects { get => leftTargetObjects; set => leftTargetObjects = value; }
    public LinkedCameraTarget LinkedCameraTarget { get => curvePoint.LinkedCameraTarget; set => curvePoint.LinkedCameraTarget = value; }
    public bool DoTargetLow { get => LinkedCameraTarget.doTargetLow; set => LinkedCameraTarget.doTargetLow = value; }
    public bool DoTargetHigh { get => LinkedCameraTarget.doTargetHigh; set => LinkedCameraTarget.doTargetHigh = value; }
    public CurvePointObject NextLeftCurvePointObject 
    { 
        get  
        {
            var index = ParentGround.CurvePointObjects.IndexOf(this);
            
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
    public CurvePointObject NextRightCurvePointObject
    {
        get
        {
            var index = ParentGround.CurvePointObjects.IndexOf(this);

            for(int i = index + 1; i < ParentGround.CurvePointObjects.Count; i++)
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
        LinkedCameraTarget.DrawTargets();
    }

    #endregion
    public void SetCurvePoint(CurvePoint curvePoint)
    {
        this.curvePoint = curvePoint;
        this.curvePoint.Object = gameObject; // Set the object reference in the CurvePoint
        transform.position = this.curvePoint.Position + ParentGround.transform.position;
        LinkedCameraTarget.Target = CameraTargetUtility.GetTarget(CameraTargetType.CurvePointLow, transform);
    }

    public void TangentsChanged(Vector3 updatedleftTang, Vector3 updatedRightTang)
    {
        curvePoint.LeftTangent = updatedleftTang - transform.position;
        curvePoint.RightTangent = updatedRightTang - transform.position;

    }

    public void LeftTangentChanged(Vector3 updatedTang)
    {
        curvePoint.LeftTangent = updatedTang - transform.position;

        if (curvePoint.IsSymmetrical)
        {
            curvePoint.RightTangent = -curvePoint.LeftTangent; // If it's symmetrical, update the right tangent accordingly
        }
        else if (curvePoint.Mode == ShapeTangentMode.Continuous)
        {
            var rightMagnitude = curvePoint.RightTangent.magnitude;
            curvePoint.RightTangent = -curvePoint.LeftTangent.normalized * rightMagnitude; // Maintain the same magnitude for the left tangent
        }

    }

    public void RightTangentChanged(Vector3 updatedTang)
    {

        curvePoint.RightTangent = updatedTang - transform.position;

        if(curvePoint.IsSymmetrical)
        {
            curvePoint.LeftTangent = -curvePoint.RightTangent; // If it's symmetrical, update the right tangent accordingly
        } else if (curvePoint.Mode == ShapeTangentMode.Continuous)
        {
            var leftMagnitude = curvePoint.LeftTangent.magnitude;
            curvePoint.LeftTangent = -curvePoint.RightTangent.normalized * leftMagnitude; // Maintain the same magnitude for the left tangent
        }
        
    }

    public void PositionChanged(Vector3 updatedPosition)
    {
        transform.position = updatedPosition;
        curvePoint.Position = updatedPosition - ParentGround.transform.position;
    }

    public void TangentSettingsChanged(ShapeTangentMode mode, bool isSymmetrical)
    {
        curvePoint.Mode = mode;
        curvePoint.IsSymmetrical = isSymmetrical;
        if (isSymmetrical)
        {
            curvePoint.RightTangent = -curvePoint.LeftTangent; // If it's symmetrical, update the right tangent accordingly
        }
        else if (curvePoint.Mode == ShapeTangentMode.Continuous)
        {
            var rightMagnitude = curvePoint.RightTangent.magnitude;
            curvePoint.RightTangent = -curvePoint.LeftTangent.normalized * rightMagnitude; // Maintain the same magnitude for the left tangent
        }
    }

    public void GenerateTarget()
    {
        LinkedCameraTarget.Target = CameraTargetUtility.GetTarget(CameraTargetType.CurvePointLow, transform);
    }

    public void PopulateDefaultTargets() //Figure out if I need to run this on deserialization
    {
        GenerateTarget();

        if (!LinkedCameraTarget.doTargetLow && !LinkedCameraTarget.doTargetHigh)
        {
            leftTargetObjects = new();
            rightTargetObjects = new();
            LinkedCameraTarget.LeftTargets = new();
            LinkedCameraTarget.RightTargets = new();
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

        if (!LinkedCameraTarget.doTargetLow)
        {
            return resyncs;
        }

        var targets = new List<LinkedCameraTarget>();

        targets.AddRange(LinkedCameraTarget.LeftTargets);
        targets.AddRange(LinkedCameraTarget.RightTargets);

        foreach (var target in targets)
        {
            var resync = new ObjectResync(target.SerializedLocation);
            resync.resyncFunc = (obj) => 
            {
                target.Target.TargetTransform = obj.transform;

                if (LinkedCameraTarget.LeftTargets.Contains(target))
                {
                    LeftTargetObjects.Add(obj);
                }

                if (LinkedCameraTarget.RightTargets.Contains(target))
                {
                    RightTargetObjects.Add(obj);
                }
            };
            resyncs.Add(resync);
        }

        return resyncs;
    }
}



