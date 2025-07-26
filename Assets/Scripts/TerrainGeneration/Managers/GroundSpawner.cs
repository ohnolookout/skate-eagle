using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.U2D;
using static UnityEngine.Rendering.HableCurve;

//Builds ground, ground segments, and start/finish objects at runtime
public class GroundSpawner : MonoBehaviour
{
    [SerializeField] private GroundManager _groundManager;
    [SerializeField] private GameObject _groundPrefab;
    [SerializeField] private GameObject _groundSegmentPrefab;
    [SerializeField] private GameObject _longSignPrefab;
    [SerializeField] private GameObject _squareSignPrefab;

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

//    //Add segment to start at last endpoint
//    public GroundSegment AddSegment(Ground ground, List<CurvePoint> curvePoints)
//    {

//#if UNITY_EDITOR
//        Undo.RegisterFullObjectHierarchyUndo(ground, "Add Segment");
//#endif

//        //Create new segment, set start point to end of current segment, and add to _segmentList
//        var newSegment = Instantiate(_groundSegmentPrefab, ground.transform, true).GetComponent<GroundSegment>();

//#if UNITY_EDITOR
//        Undo.RegisterCreatedObjectUndo(newSegment.gameObject, "Add Segment");
//#endif
//        newSegment.parentGround = ground;
//        newSegment.NextLeftSegment = ground.LastSegment;
//        Vector2 startPoint = new(0, 0);

//        if (newSegment.NextLeftSegment != null)
//        {
//            newSegment.NextLeftSegment.NextRightSegment = newSegment;
//            startPoint = newSegment.NextLeftSegment.EndPosition;
//        } else
//        {
//            startPoint = ground.transform.position;
//        }

//        newSegment.transform.position = startPoint;
//        ground.SegmentList.Add(newSegment);

//        ApplyCurvePointsToSegment(newSegment, curvePoints);
//        newSegment.PopulateDefaultTargets();

//        if (!newSegment.HasShadow)
//        {
//            newSegment.GetComponent<ShadowCaster2D>().enabled = false;
//        }

//        if(newSegment.NextLeftSegment != null)
//        {

//#if UNITY_EDITOR
//            Undo.RecordObject(newSegment.NextLeftSegment, "Remove collider end points");
//#endif

//            BuildCollider(newSegment.NextLeftSegment, curvePoints);
//        }

//        return newSegment;
//    }
    #endregion

    #region Build Segments

//    public void ApplyCurvePointsToSegment(GroundSegment segment, List<CurvePoint> curvePoints)
//    {
//#if UNITY_EDITOR
//        Undo.RegisterFullObjectHierarchyUndo(segment.gameObject, "Set edge");
//#endif
//        //Set splines to default formatting
//        GroundSplineUtility.FormatSpline(segment.Spline, segment.IsFloating);

//#if UNITY_EDITOR
//        Undo.RegisterFullObjectHierarchyUndo(segment.EdgeShapeController.gameObject, "Set edge");
//#endif

//        GroundSplineUtility.FormatSpline(segment.EdgeSpline, true);

//#if UNITY_EDITOR
//        Undo.RegisterFullObjectHierarchyUndo(segment, "Generating segment");
//#endif
//        segment.GetComponent<SpriteShapeRenderer>().enabled = !segment.IsFloating;
//        GroundSplineUtility.GenerateSpline(segment.Spline, curvePoints, segment.IsFloating);

//#if UNITY_EDITOR
//        Undo.RegisterFullObjectHierarchyUndo(segment.EdgeShapeController.gameObject, "Set edge");
//#endif
//        //GroundSplineUtility.InsertCurvePointsToOpenSpline(segment.EdgeSpline, curvePoints);
//        GroundSplineUtility.GenerateSpline(segment.EdgeSpline, curvePoints, true);

//        BuildCollider(segment, curvePoints);

//    }
//    public EdgeCollider2D BuildCollider(GroundSegment segment, List<CurvePoint> curvePoints, float resolution = 10)
//    {
//#if UNITY_EDITOR
//        Undo.RegisterCompleteObjectUndo(segment.Collider, "Edge Collider");
//        Undo.RegisterCompleteObjectUndo(segment.BottomCollider, "Bottom Collider");
//#endif

//        ColliderGenerator.BuildSegmentCollider(segment, curvePoints, segment.ColliderMaterial);
//        return segment.Collider;
//    }

    public GroundSegment AddEmptySegment(Ground ground)
    {
        var segment = Instantiate(_groundSegmentPrefab, ground.transform, true).GetComponent<GroundSegment>();
        ground.SegmentList.Add(segment);
        return segment;
    }
    #endregion

    #region Start/Finish
    public Vector2 SetStartPoint(Ground ground, CurvePoint curvePoint)
    {
        //segment.IsStart = true;
        var startPoint = ground.transform.TransformPoint(curvePoint.Position);
        return startPoint;
    }

    #endregion

    #region Objects
    public GameObject AddTutorialSign(bool isSquare)
    {
        var signPrefab = _longSignPrefab;

        if(isSquare)
        {
            signPrefab = _squareSignPrefab;
        }

        var sign = Instantiate(signPrefab, _groundManager.groundContainer.transform);
        return sign;
                
    }

    #endregion

}
