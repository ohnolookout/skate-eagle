using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static UnityEngine.Rendering.HableCurve;

//Builds ground, ground segments, and start/finish objects at runtime
public class GroundSpawner : MonoBehaviour
{
    [SerializeField] private GroundManager _groundManager;
    [SerializeField] private GameObject _groundPrefab;
    [SerializeField] private GameObject _groundSegmentPrefab;
    [SerializeField] private GameObject _finishFlagPrefab;
    [SerializeField] private GameObject _backstopPrefab;
    private GameObject _finishFlag;
    private GameObject _backstop;

    #region Add/Remove Segments
    public Ground AddGround()
    {
        var groundObj = Instantiate(_groundPrefab, _groundManager.groundContainer.transform);
        groundObj.name = "Ground " + (_groundManager.groundContainer.transform.childCount - 1);

#if UNITY_EDITOR
        Undo.RegisterCreatedObjectUndo(groundObj, "Add Ground");
#endif

        return groundObj.GetComponent<Ground>();
    }

    //Add segment to start at last endpoint
    public GroundSegment AddSegment(Ground ground, Curve curve)
    {
        //Debug.Log("Adding segment with curve: ");
        //curve.LogCurvePoints();


#if UNITY_EDITOR
        Undo.RegisterFullObjectHierarchyUndo(ground, "Add Segment");
#endif

        //Create new segment, set start point to end of current segment, and add to _segmentList
        var newSegment = Instantiate(_groundSegmentPrefab, ground.transform, true).GetComponent<GroundSegment>();

#if UNITY_EDITOR
        Undo.RegisterCreatedObjectUndo(newSegment.gameObject, "Add Segment");
#endif
        newSegment.Curve = curve;
        newSegment.parentGround = ground;
        newSegment.PreviousSegment = ground.LastSegment;
        Vector2 startPoint = new(0, 0);

        if (ground.LastSegment != null)
        {
            ground.LastSegment.NextSegment = newSegment;
            startPoint = ground.LastSegment.EndPosition;
        }

        newSegment.transform.position = startPoint;
        ground.SegmentList.Add(newSegment);

        ApplyCurveToSegment(newSegment, newSegment.Curve);
        newSegment.DoDefaultHighLowPoints();

        if (!newSegment.HasShadow)
        {
            newSegment.GetComponent<ShadowCaster2D>().enabled = false;
        }

        if(newSegment.PreviousSegment != null)
        {

#if UNITY_EDITOR
            Undo.RecordObject(newSegment.PreviousSegment, "Remove collider end points");
#endif

            BuildCollider(newSegment.PreviousSegment);
        }

        return newSegment;
    }
    #endregion

    #region Build Segments

    public void ApplyCurveToSegment(GroundSegment segment, Curve curve)
    {
        //Set splines to default formatting
        GroundSplineUtility.FormatSpline(segment.Spline, segment.IsFloating);

#if UNITY_EDITOR
        Undo.RegisterFullObjectHierarchyUndo(segment.EdgeShapeController.gameObject, "Set edge");
#endif

        GroundSplineUtility.FormatSpline(segment.EdgeSpline, true);

#if UNITY_EDITOR
        Undo.RegisterFullObjectHierarchyUndo(segment, "Generating segment");
#endif
        if (segment.IsFloating)
        {
            GroundSplineUtility.InsertCurveToOpenSpline(segment.Spline, curve);
        }
        else
        {
            GroundSplineUtility.GenerateSpline(segment.Spline, curve, segment.floorHeight);
        }

#if UNITY_EDITOR
        Undo.RegisterFullObjectHierarchyUndo(segment.EdgeShapeController.gameObject, "Set edge");
#endif

        GroundSplineUtility.InsertCurveToOpenSpline(segment.EdgeSpline, curve);

        BuildCollider(segment);

    }
    public EdgeCollider2D BuildCollider(GroundSegment segment, float resolution = 10)
    {
#if UNITY_EDITOR
        Undo.RegisterCompleteObjectUndo(segment.Collider, "Add Collider");
#endif

        CurveCollider.BuildSegmentCollider(segment, segment.ColliderMaterial);
        return segment.Collider;
    }

    public GroundSegment AddEmptySegment(Ground ground)
    {
        return Instantiate(_groundSegmentPrefab, ground.transform, true).GetComponent<GroundSegment>();
    }
    #endregion

    #region Start/Finish
    public Vector2 SetStartPoint(GroundSegment segment, int curvePointIndex)
    {
        segment.IsStart = true;
        var startPoint = segment.transform.TransformPoint(segment.Curve.GetPoint(curvePointIndex).Position);
        return startPoint;
    }

    public Vector2 SetFinishPoint(GroundSegment segment, int finishPointIndex)
    {
#if UNITY_EDITOR
        Undo.RegisterFullObjectHierarchyUndo(segment, "Set Finish Point");
        //If finishSegment has already been assigned, make isFinish false on old segment and destroy finish objects
        if (_finishFlag != null)
        {
            Undo.DestroyObjectImmediate(_finishFlag);
        }
        if (_backstop != null)
        {
            Undo.DestroyObjectImmediate(_backstop);
        }
#endif

        segment.IsFinish = true;

        //Add finish flag to designated point in GroundSegment.        
        var finishPoint = segment.transform.TransformPoint(segment.Curve.GetPoint(finishPointIndex).Position);
        finishPoint += new Vector3(50, 0, 0);

        _finishFlag = Instantiate(_finishFlagPrefab, finishPoint, transform.rotation, segment.gameObject.transform);

#if UNITY_EDITOR
        Undo.RegisterCreatedObjectUndo(_finishFlag, "Add Flag");
#endif

        //Add backstop to endpoint of GroundSegment
        _backstop = Instantiate(_backstopPrefab, segment.EndPosition - new Vector3(75, 0), transform.rotation, segment.gameObject.transform);

#if UNITY_EDITOR
        Undo.RegisterCreatedObjectUndo(_backstop, "Add Backstop");
#endif

        return finishPoint;
    }

    public void ClearStartFinishObjects()
    {
        _finishFlag = null;
        _backstop = null;
    }
    #endregion

}
