using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TerrainManager : MonoBehaviour
{
    #region Declarations
    private LevelTerrain _terrain;
    [SerializeField] private GameObject _terrainPrefab;
    [SerializeField] private List<Rigidbody2D> _normalBodies, _ragdollBodies;
    private GroundColliderManager _colliderManager;
    private Vector2 _finishPoint;
    const float _cameraBuffer = 25;
    private bool _trackCollision = false;
    public Action<Vector2> OnActivateFinish;
    private DoublePositionalList<IGroundSegment> _positionalSegmentList;
    public LevelTerrain Terrain { get => _terrain; }
    #endregion

    #region Monobehaviors

    private void Start()
    {
        if (_terrain == null)
        {
            Debug.LogWarning("No terrain found by terrain manager. Destroying terrain manager.");
            DestroyImmediate(gameObject);
            return;
        }
        _colliderManager.OnActivateLastSegment += _terrain.ActivateFinishObjects;
    }

    void Update()
    {
        _positionalSegmentList.Update();
        if (_trackCollision)
        {
            _colliderManager.Update();
        }
    }
    private void OnEnable()
    {
        LevelManager.OnAttempt += () => _trackCollision = true;
        LevelManager.OnFinish += _ => _trackCollision = false;
    }

    private void OnDisable()
    {
        DeleteChildren();
    }

    #endregion

    #region Initialization
    public Vector2 GenerateTerrain(Level level, Vector3 startPosition)
    {
        if (transform.childCount > 0)
        {
            DeleteChildren();
        }

        InitializeTerrain(level, startPosition);

        _colliderManager = new(_normalBodies, _ragdollBodies, _terrain);

        InitializePositionalList(_terrain, _cameraBuffer, _cameraBuffer);
        
        return _finishPoint;
    }

    private void InitializePositionalList(LevelTerrain terrain, float trailingBuffer, float leadingBuffer)
    {
        _positionalSegmentList = GetPositionalSegmentList(terrain, trailingBuffer, leadingBuffer);
        ActivateInitialSegments(_positionalSegmentList);
        AssignPositionalEvents(_positionalSegmentList);
    }

    private void InitializeTerrain(Level level, Vector3 startPosition)
    {
        _terrain = Instantiate(_terrainPrefab, transform).GetComponent<LevelTerrain>();

        TerrainGenerator.GenerateLevel(level, _terrain, startPosition, out _finishPoint);

        _terrain.SegmentList[^1].OnActivate += OnFinishActivation;
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
    #endregion

    #region PositionalList
    private static DoublePositionalList<IGroundSegment> GetPositionalSegmentList(LevelTerrain terrain, float trailingBuffer, float leadingBuffer)
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
