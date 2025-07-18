using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Ground : MonoBehaviour, ISerializable
{
    #region Declarations
    [SerializeField] private List<GroundSegment> _segmentList;
    [SerializeField] GameObject _segmentPrefab;
    [SerializeField] PhysicsMaterial2D _colliderMaterial;
    [SerializeField] private bool _isFloating = false;
    [SerializeField] private bool _isInverted = false;
    [SerializeField] private bool _hasShadow = true;
    private List<CurvePointObject> _curvePointEditObjects = new();    
    [SerializeField] private GameObject _curvePointEditObjectPrefab;
    [SerializeField] private GameObject _curvePointParent;
    [SerializeField] private List<CurvePoint> _curvePoints = new();
    [SerializeField] private List<LinkedCameraTarget> _linkedCameraTargets = new();
    //Add dictionary that maps CurvePointObjects to Splinepoints

    public List<GroundSegment> SegmentList { get => _segmentList; set => _segmentList = value; }
    public PhysicsMaterial2D ColliderMaterial { get => _colliderMaterial; set => _colliderMaterial = value; }
    public CurvePoint StartPoint => _segmentList[0].Curve.StartPoint;
    public CurvePoint EndPoint => _segmentList[^1].Curve.EndPoint;
    public bool IsFloating { get => _isFloating; set => _isFloating = value; }
    public bool IsInverted { get => _isInverted; set => _isInverted = value; }
    public bool HasShadow { get => _hasShadow; set => _hasShadow = value; }
    public GroundSegment LastSegment => _segmentList.Count > 0 ? _segmentList[^1] : null;
    public List<CurvePoint> CurvePoints => _curvePoints;
    public List<CurvePointObject> CurvePointEditObjects => _curvePointEditObjects;
    public List<LinkedCameraTarget> LinkedCameraTargets => _linkedCameraTargets;
    #endregion

    public IDeserializable Serialize()
    {
        var name = gameObject.name;
        var position = transform.position;
        SerializeLevelUtility.SegmentsFromCurvePoints(this, out var editSegment, out var runtimeSegments);

        return new SerializedGround(name, position, runtimeSegments, editSegment, _curvePoints);
    }
#if UNITY_EDITOR
    public void AddCurvePointEditObject(CurvePoint curvePoint) 
    {
        var point = Instantiate(_curvePointEditObjectPrefab, _curvePointParent.transform).GetComponent<CurvePointObject>();
        point.groundTransform = transform;
        CurvePoints.Add(curvePoint);
        _curvePointEditObjects.Add(point);
        point.SetCurvePoint(curvePoint);
        point.OnCurvePointChange += OnCurvePointChanged;
    }

    private void OnCurvePointChanged(CurvePointObject point)
    {
        //Update corresponding splinepoints on curvePoint change
    }
#endif
}


#if UNITY_EDITOR

[CustomEditor(typeof(Ground))]
public class GroundEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var ground = (Ground)target;
    }
    public void OnSceneGUI()
    {
        var ground = (Ground)target;

        foreach(var point in ground.CurvePointEditObjects)
        {
            CurvePointObjectEditor.DrawCurvePointHandles(point);
        }
    }
}

#endif