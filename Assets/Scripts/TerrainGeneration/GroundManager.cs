using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GroundManager : MonoBehaviour
{
    #region Declarations
    private Ground _ground;
    [SerializeField] private GameObject _terrainPrefab;
    [SerializeField] private GameObject _finishFlagPrefab;
    [SerializeField] private GameObject _backstopPrefab;
    [SerializeField] private GameObject _finishFlag;
    [SerializeField] private GameObject _backstop;
    public IGroundSegment finishSegment;
    public IGroundSegment startSegment;
    [SerializeField] private List<Rigidbody2D> _normalBodies, _ragdollBodies;
    private Vector2 _startPoint = new(400f, -150f);
    private Vector2 _finishPoint = new(0, 0);
    const float _cameraBuffer = 25;
    public Action<Vector2> OnActivateFinish;
    private DoublePositionalList<IGroundSegment> _positionalSegmentList;
    public Ground Ground { get => _ground; }
    public Vector2 StartPoint { get => _startPoint; set => _startPoint = value; }
    public Vector2 FinishPont { get => _finishPoint; set => _finishPoint = value; }
    #endregion

    #region Monobehaviors
    private void Awake()
    {
    }
    private void Start()
    {
        finishSegment.OnActivate += OnFinishActivation;
        if (_ground == null)
        {
            Debug.LogWarning("No terrain found by terrain manager. Destroying terrain manager.");
            DestroyImmediate(gameObject);
            return;
        }
        
    }

    void Update()
    {
        _positionalSegmentList.Update();
    }

    private void OnDisable()
    {
        DeleteChildren();
    }

    #endregion

    #region Initialization
    public Vector2 GenerateGround(Level level)
    {
        if (transform.childCount > 0)
        {
            DeleteChildren();
        }

        InitializeTerrain(level);

        InitializePositionalList(_ground, _cameraBuffer, _cameraBuffer);
        
        return _finishPoint;
    }

    private void InitializePositionalList(Ground terrain, float trailingBuffer, float leadingBuffer)
    {
        _positionalSegmentList = GetPositionalSegmentList(terrain, trailingBuffer, leadingBuffer);
        ActivateInitialSegments(_positionalSegmentList);
        AssignPositionalEvents(_positionalSegmentList);
    }

    private void InitializeTerrain(Level level)
    {
        _ground = Instantiate(_terrainPrefab, transform).GetComponent<Ground>();

        GroundGenerator.GenerateLevel(level, this, _ground);
    }

    private void OnFinishActivation(IGroundSegment segment)
    {
        segment.OnActivate -= OnFinishActivation;
        OnActivateFinish?.Invoke(_finishPoint);
    }
    public void DeleteChildren()
    {
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }

    public void SetStartPoint(IGroundSegment segment, int curvePointIndex)
    {
        startSegment = segment;
        _startPoint = segment.gameObject.transform.TransformPoint(segment.Curve.GetPoint(curvePointIndex).ControlPoint);
        Debug.Log("Start point set to " + _startPoint);
    }

    public void SetFinishPoint(IGroundSegment segment, int finishPointIndex)
    {
        //If finishSegment has already been assigned, make isFinish false on old segment and destroy finish objects
        if (finishSegment != null)
        {
            finishSegment.IsFinish = false;
            DestroyImmediate(_finishFlag);
            DestroyImmediate(_backstop);
        }

        finishSegment = segment;
        segment.IsFinish = true;

        //Add finish flag to designated point in GroundSegment. Mark point as finishPoint.        
        _finishPoint = segment.gameObject.transform.TransformPoint(segment.Curve.GetPoint(finishPointIndex).ControlPoint);
        _finishFlag = Instantiate(_finishFlagPrefab, _finishPoint, transform.rotation, segment.gameObject.transform);

        //Add backstop to endpoint of GroundSegment
        _backstop = Instantiate(_backstopPrefab, segment.EndPosition - new Vector3(75, 0), transform.rotation, segment.gameObject.transform);

    }
    #endregion

    #region PositionalList
    private static DoublePositionalList<IGroundSegment> GetPositionalSegmentList(Ground terrain, float trailingBuffer, float leadingBuffer)
    {
        return DoublePositionalListFactory<IGroundSegment>.CameraOperatorTracker(terrain.SegmentList, Camera.main.GetComponent<ICameraOperator>(), trailingBuffer, leadingBuffer);
    }

    private static void ActivateInitialSegments(DoublePositionalList<IGroundSegment> segmentList)
    {
        foreach (var segment in segmentList.CurrentObjects)
        {
            segment.gameObject.SetActive(true);
        }
    }

    private static void AssignPositionalEvents(DoublePositionalList<IGroundSegment> positionalList)
    {
        positionalList.OnObjectAdded += (obj, _) => obj.gameObject.SetActive(true);
        positionalList.OnObjectRemoved += (obj, _) => obj.gameObject.SetActive(false);
    }
    #endregion
}
