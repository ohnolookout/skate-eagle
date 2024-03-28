using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TerrainManager : MonoBehaviour
{
    #region Variables
    private CameraOperator _camera;
    private LevelTerrain _terrain;
    [SerializeField] private GameObject _terrainPrefab;
    [SerializeField] private List<Rigidbody2D> _normalBodies, _ragdollBodies;
    private GroundColliderManager _colliderManager;
    const float _cameraBuffer = 25;
    private float _leadingTerrainX, _trailingTerrainX;
    private int _leadingTerrainIndex, _trailingTerrainIndex;
    private MinMaxCache _lowPointCache = new(ComparisonType.Least), _highPointCache = new(ComparisonType.Greatest);
    private bool _trackCollision = false;
    private Action<FinishScreenData> onFinish { get; set; }
    private Action onAttempt;
    public MinMaxCache LowPointCache { get => _lowPointCache; }
    public MinMaxCache HighPointCache { get => _highPointCache; }
    public List<Rigidbody2D> NormalBodies { get => _normalBodies; set => _normalBodies = value; }
    public List<Rigidbody2D> RagdollBodies { get => _ragdollBodies; set => _ragdollBodies = value; }
    public GroundColliderManager ColliderManager { get => _colliderManager; }
    #endregion

    #region Monobehaviors
    private void Awake()
    {
        _camera = Camera.main.GetComponent<CameraOperator>();
    }
    private void OnEnable()
    {
        onAttempt += () => { 
            _trackCollision = true;
        };
        onFinish += _ => { _trackCollision = false; };
        LevelManager.OnAttempt += onAttempt;
        LevelManager.OnFinish += onFinish;
    }

    private void OnDisable()
    {
        LevelManager.OnAttempt -= onAttempt;
        LevelManager.OnFinish -= onFinish;
        DeleteChildren();
    }

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
        UpdateCameraBounds();
        CheckTerrainBounds();
        if (_trackCollision)
        {
            _colliderManager.UpdateColliders();
        }
    }
    #endregion

    #region Initialization
    public Vector2 GenerateTerrain(Level level, Vector3 startPosition)
    {
        if (transform.childCount > 0)
        {
            DeleteChildren();
        }
        _terrain = Instantiate(_terrainPrefab, transform).GetComponent<LevelTerrain>();
        Vector2 finishPoint = TerrainGenerator.GenerateLevel(level, _terrain, startPosition);
        _colliderManager = new(_normalBodies, _ragdollBodies, _terrain);
        ActivateInitialSegments(3);
        return finishPoint;
    }
    private void ActivateInitialSegments(int activationCount)
    {
        _leadingTerrainIndex = -1;
        for (int i = 0; i < activationCount; i++)
        {
            ActivateLeadingSegment();
            if (i < 2)
            {
                _colliderManager.ColliderList[i].gameObject.SetActive(true);
            }
        }
    }
    public void DeleteChildren()
    {
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }
    #endregion

    #region Update Utilities
    private void UpdateCameraBounds()
    {
        if (_camera != null)
        {
            _leadingTerrainX = _camera.LeadingCorner.x + _cameraBuffer;
            _trailingTerrainX = _camera.TrailingCorner.x - _cameraBuffer;
        }
        else
        {
            _leadingTerrainX = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, 0)).x + _cameraBuffer;
            _trailingTerrainX = Camera.main.ViewportToWorldPoint(new Vector3(0, 1, 0)).x - _cameraBuffer;
        }
    }

    private void CheckTerrainBounds()
    {
        if (_terrain.SegmentList[_leadingTerrainIndex].StartsAfterX(_leadingTerrainX))
        {
            DeactivateLeadingSegment();
        }
        else if (_leadingTerrainIndex < _terrain.SegmentList.Count - 1)
        {
            //If the segment after the current leading segment starts before the leading edge of the camera + buffer,
            //Activate it and increase leadingSegmentIndex
            if (!_terrain.SegmentList[_leadingTerrainIndex + 1].StartsAfterX(_leadingTerrainX))
            {
                ActivateLeadingSegment();
            }
        }
        //Exit if trailingSegment index is outside the bounds of the segment array
        //Because player is on finishline segment.
        if (_trailingTerrainIndex >= _terrain.SegmentList.Count)
        {
            return;
        }
        //If the trailingSegment ends before the trailing edge of the camera + buffer,
        //Deactivate it and increment the trailing index.
        if (_terrain.SegmentList[_trailingTerrainIndex].EndsBeforeX(_trailingTerrainX))
        {
            DeactivateTrailingSegment();
            return;
        }
        if (_trailingTerrainIndex <= 0)
        {
            return;
        }
        //If the segment before the trailing segment index ends after the trailing edge of the camera + buffer,
        //Activate it and decrement the trailing index.
        if (!_terrain.SegmentList[_trailingTerrainIndex - 1].EndsBeforeX(_trailingTerrainX))
        {
            ActivateTrailingSegment();
        }
    }
    #endregion

    #region Activate/Deactivate
    private void DeactivateLeadingSegment()
    {
        _terrain.ActivateSegmentAtIndex(_leadingTerrainIndex, false);
        _leadingTerrainIndex--;
        _lowPointCache.RemoveLeading();
    }

    private void DeactivateTrailingSegment()
    {
        _terrain.ActivateSegmentAtIndex(_trailingTerrainIndex, false);
        _trailingTerrainIndex++;
        _lowPointCache.RemoveTrailing();
    }

    private void ActivateLeadingSegment()
    {
        _leadingTerrainIndex++;
        IGroundSegment addedSegment = _terrain.ActivateSegmentAtIndex(_leadingTerrainIndex, true);
        _lowPointCache.AddLeading(addedSegment.Curve.LowPoint);
    }

    private void ActivateTrailingSegment()
    {
        _trailingTerrainIndex--;
        IGroundSegment addedSegment = _terrain.ActivateSegmentAtIndex(_trailingTerrainIndex, true);
        _lowPointCache.AddTrailing(addedSegment.Curve.LowPoint);
    }
    #endregion    
}
