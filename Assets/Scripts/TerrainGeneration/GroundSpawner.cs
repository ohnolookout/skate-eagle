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

    #region Add/Remove Segments
    public Ground AddGround()
    {
        Debug.Log("Adding ground");
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

        if (newSegment.PreviousSegment != null)
        {
            newSegment.PreviousSegment.NextSegment = newSegment;
            startPoint = newSegment.PreviousSegment.EndPosition;
        } else
        {
            startPoint = ground.transform.position;
        }

        newSegment.transform.position = startPoint;
        ground.SegmentList.Add(newSegment);

        ApplyCurveToSegment(newSegment, newSegment.Curve);
        newSegment.PopulateDefaultTargets();

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
#if UNITY_EDITOR
        Undo.RegisterFullObjectHierarchyUndo(segment.gameObject, "Set edge");
#endif
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
        Undo.RegisterCompleteObjectUndo(segment.Collider, "Edge Collider");
        Undo.RegisterCompleteObjectUndo(segment.BottomCollider, "Bottom Collider");
#endif

        ColliderGenerator.BuildSegmentCollider(segment, segment.ColliderMaterial);
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

    public void SetFinishLine(Vector2 flagPosition, Vector2 backstopPosition, bool backstopIsActive = true)
    {
#if UNITY_EDITOR
        Undo.RegisterFullObjectHierarchyUndo(_groundManager.FinishLine.gameObject, "Set finish line");
#endif
        _groundManager.FinishLine.gameObject.SetActive(true);
        _groundManager.FinishLine.Backstop.gameObject.SetActive(backstopIsActive);
        _groundManager.FinishLine.SetFinishLine(flagPosition, backstopPosition);
    }

    #endregion

}
