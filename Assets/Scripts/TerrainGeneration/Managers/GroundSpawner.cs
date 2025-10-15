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
    [SerializeField] private GameObject _arrowTextSign;
    [SerializeField] private GameObject _arrowSquareSign;
    [SerializeField] private GameObject _rotateTextSign;
    [SerializeField] private GameObject _rotateSquareSign;


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

    #endregion

    #region Build Segments

    public GroundSegment AddEmptySegment(Ground ground)
    {
        var segment = Instantiate(_groundSegmentPrefab, ground.transform, true).GetComponent<GroundSegment>();
        ground.SegmentList.Add(segment);
        return segment;
    }
    #endregion

    #region Start/Finish
    public Vector2 SetStartPoint(Ground ground, LinkedCameraTarget curvePoint)
    {
        //segment.IsStart = true;
        var startPoint = ground.transform.TransformPoint(curvePoint.Position);
        return startPoint;
    }

    #endregion

    #region Objects
    public GameObject AddTutorialSign(SignType type)
    {
        GameObject signPrefab;
        switch (type)
        {
            case SignType.ArrowSquare:
                signPrefab = _arrowSquareSign;
                break;
            case SignType.ArrowText:
                signPrefab = _arrowTextSign;
                break;
            case SignType.RotateSquare:
                signPrefab = _rotateSquareSign;
                break;
            case SignType.RotateText:
                signPrefab = _rotateTextSign;
                break;
            default:
                signPrefab = _arrowSquareSign;
                break;
        }

        var sign = Instantiate(signPrefab, _groundManager.groundContainer.transform);
        return sign;
                
    }

    #endregion

}
