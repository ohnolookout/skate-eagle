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
    public GroundSpawner groundSpawner;
    private List<Ground> _grounds;
    public GameObject groundContainer;
    [SerializeField] private List<Rigidbody2D> _normalBodies, _ragdollBodies;
    private Vector2 _startPoint = new(400f, -150f);
    private Vector2 _finishPoint = new(0, 0);
    const float _cameraBuffer = 25;
    private Action<Vector2> _onActivateFinish;
    private DoublePositionalList<GroundSegment> _positionalSegmentList;
    public Ground Ground { get => _ground; }
    public Vector2 StartPoint { get => _startPoint; set => _startPoint = value; }
    public Vector2 FinishPoint { get => _finishPoint; set => _finishPoint = value; }
    public Action<Vector2> OnActivateFinish;
    #endregion

    #region Monobehaviors

    void Update()
    {
        //_positionalSegmentList.Update();
    }

    private void OnDisable()
    {
        ClearGround();
    }

    #endregion

    #region Initialization
    public Vector2 GenerateGround(Level level)
    {
        if (transform.childCount > 0)
        {
            ClearGround();
        }

        //InitializeTerrain(level);

        //InitializePositionalList(_ground, _cameraBuffer, _cameraBuffer);
        
        return _finishPoint;
    }

    private void InitializePositionalList(Ground terrain, float trailingBuffer, float leadingBuffer)
    {
        _positionalSegmentList = GetPositionalSegmentList(terrain, trailingBuffer, leadingBuffer);
        ActivateInitialSegments(_positionalSegmentList);
        AssignPositionalEvents(_positionalSegmentList);
    }
    /*
    private void InitializeTerrain(Level level)
    {
        _ground = Instantiate(_terrainPrefab, transform).GetComponent<Ground>();

        //GroundGenerator.GenerateLevel(level, this, _ground);
    }
    private void OnFinishActivation(GroundSegment segment)
    {
        segment.OnActivateFinish -= OnFinishActivation;
        _onActivateFinish?.Invoke(_finishPoint);
    }
    */
    public void ClearGround()
    {
        groundSpawner.ClearStartFinishObjects();

        while (groundContainer.transform.childCount > 0)
        {
            DestroyImmediate(groundContainer.transform.GetChild(0).gameObject);
        }
    }
    #endregion

    #region PositionalList
    private static DoublePositionalList<GroundSegment> GetPositionalSegmentList(Ground terrain, float trailingBuffer, float leadingBuffer)
    {
        return DoublePositionalListFactory<GroundSegment>.CameraOperatorTracker(terrain.SegmentList, Camera.main.GetComponent<ICameraOperator>(), trailingBuffer, leadingBuffer);
    }

    private static void ActivateInitialSegments(DoublePositionalList<GroundSegment> segmentList)
    {
        foreach (var segment in segmentList.CurrentObjects)
        {
            segment.gameObject.SetActive(true);
        }
    }

    private static void AssignPositionalEvents(DoublePositionalList<GroundSegment> positionalList)
    {
        positionalList.OnObjectAdded += (obj, _) => obj.gameObject.SetActive(true);
        positionalList.OnObjectRemoved += (obj, _) => obj.gameObject.SetActive(false);
    }
    #endregion
}
