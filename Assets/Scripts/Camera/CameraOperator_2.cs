using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraOperator_2 : MonoBehaviour, ICameraOperator
{
    private PositionalMinMax<PositionObject<Vector3>> _lowPoints, _highPoints;
    private float _lowPointBuffer, _highPointBuffer, _distanceBetweenLowPoints = 0;
    private const float DefaultLowPointBuffer = 100, DefaultHighPointBuffer = 50;
    [SerializeField] private Vector3 _offset = new(27, 23);
    [SerializeField] private float _lowPointXOffset = 10;
    private Vector3 _leadingCorner, _trailingCorner, _targetLowPoint, _lastLowPoint, _targetHighPoint, _currentLowPoint;
    private float _defaultSize, _zoomYDelta = 0;
    private bool _isZoomOut = false, _isZoomIn = false;
    private ILevelManager _levelManager;
    private IPlayer _player;
    private Rigidbody2D _playerBody;
    private Camera _cam;
    private bool _isFinished = false;
    public Action OnFinishZoomIn { get; set; }
    public Action<ICameraOperator> OnZoomOut { get; set; }
    public Vector3 LeadingCorner { get => _leadingCorner; }
    public Vector3 TrailingCorner { get => _trailingCorner; }
    public Vector3 Center { get => _cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0)); }
    public float ZoomYDelta { get => _zoomYDelta; }
    public float CurrentLowPointX => transform.position.x + _lowPointXOffset;
    public Camera Camera { get => _cam; }
    public float DefaultSize { get => _defaultSize; }
    public bool IsZoomOut { get => _isZoomOut; set => _isZoomOut = value; }
    public bool IsZoomIn { get => _isZoomIn; set => _isZoomIn = value; }

    new public GameObject gameObject => transform.gameObject;
    public float LowPointBuffer => _lowPointBuffer;
    public float HighPointBuffer => _highPointBuffer;
    
    #region Monobehaviours
    void Awake()
    {
        //Assign levelmanager and destroy cameraOperator if levelmanager isn't valid
        _levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<ILevelManager>();
        CheckLevelManager();

        //Assign default camera components
        _cam = GetComponent<Camera>();
        _defaultSize = _cam.orthographicSize;
        _lowPointBuffer = DefaultLowPointBuffer;
        _highPointBuffer = DefaultHighPointBuffer;

        //Get player, assign body, and set body to switch on game over
        _player = LevelManager.GetPlayer;
        _playerBody = _player.NormalBody;
        LevelManager.OnGameOver += _ => _playerBody = _player.RagdollBody;

        //Set isFinished to turn on on finish
        LevelManager.OnFinish += _ => _isFinished = true;
    }

    void Start()
    {
        //Create minmax caches for high and low points
        BuildHighLowCaches(_levelManager.TerrainManager.Terrain);

        //Assign current and past low points to cache's low point
        _targetLowPoint = _lowPoints.CurrentPoint;
        _lastLowPoint = _targetLowPoint;
        UpdatePosition();
    }

    void Update()
    {
        //If player falls below bottom of screen, trigger fall
        if (_cam.WorldToScreenPoint(_playerBody.position).y < 0)
        {
            _levelManager.Fall();
        }

        //Update zoom, position, and high/low caches unless level is finished
        UpdateZoom();

        if (!_isFinished)
        {
            UpdatePosition();
            UpdateHighLowCaches();
            
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(_currentLowPoint, 1);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(_targetLowPoint, 1);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(_lastLowPoint, 1);
    }
    #endregion

    #region Position
    private void UpdatePosition()
    {
        //Get new Y for low point by lerping between current and last low points.
        _currentLowPoint = LerpedLowPoint(_targetLowPoint, _lastLowPoint);

        //Add offset and zoom changes to low point y
        float cameraY = _currentLowPoint.y + _offset.y + _zoomYDelta;

        //Get x from player's position, offset, and the zoom Y delta adjusted by the aspect ratio to represent the x delta
        float cameraX = _playerBody.position.x + _offset.x + (_zoomYDelta * (1 / Camera.main.aspect));

        //Assign updated x and y to camera's transform
        transform.position = new Vector3(cameraX, cameraY, transform.position.z);

        //Update leading and trailing corner values
        _leadingCorner = _cam.ViewportToWorldPoint(new Vector3(1, 1, 0));
        _trailingCorner = _cam.ViewportToWorldPoint(new Vector3(0, 1, 0));
    }
    #endregion

    #region Zoom
    private void UpdateZoom()
    {
        //If not zooming in or out, check to start zoom out. If not zooming out, return.
        if (!_isZoomOut && !_isZoomIn)
        {
            StartZoomOutCheck();
            if (!_isZoomOut)
            {
                return;
            }
        }

        //If zooming out or in, update high/low buffers based on current camera bounds
        UpdateHighLowBuffers();

        if (_isZoomOut)
        {
            ZoomOut();
        } 
        else
        {
            ZoomIn();
        }
    }

    private void ZoomOut()
    {
        //Calculate zoom rate of change based on player y velocity
        float change = Mathf.Clamp(_playerBody.velocity.y, 0.5f, 99999) * 0.65f * Time.fixedDeltaTime;

        //Add change to size and y delta
        _cam.orthographicSize += change;
        _zoomYDelta += change;

        //Check to begin zoom in.
        StartZoomInCheck();
    }

    private void ZoomIn()
    {
        //Calculate zoom in rate of change based on player y velocity
        float change = Mathf.Clamp(_playerBody.velocity.y, -666, -1) * 0.5f * Time.fixedDeltaTime;

        //Add change to size and y delta
        _cam.orthographicSize += change;
        _zoomYDelta += change;

        //Check to end zoom in
        EndZoomInCheck();
    }

    private void StartZoomOutCheck()
    {
        //Set zoom out to true if player is above top of camera minus a scaled buffer
        if (!PlayerInYBounds()) {
            _isZoomOut = true;
            OnZoomOut?.Invoke(this);
        }
    }

    private void StartZoomInCheck()
    {
        //Set zoom in to true if player is in bounds and cam size is greater than default size
        if (PlayerInYBounds() && _cam.orthographicSize > _defaultSize)
        {
            _isZoomOut = false;
            _isZoomIn = true;
        }
    }

    private void EndZoomInCheck()
    {
        //End zoom in when camera size returns to default
        if(_cam.orthographicSize <= _defaultSize)
        {
            _isZoomIn = false;
            OnFinishZoomIn?.Invoke();
        }
    }

    private bool PlayerInYBounds()
    {
        //Return true if player is in bounds and cam size is greater than default size
        return _playerBody.position.y < LeadingCorner.y - Camera.main.orthographicSize * 0.2f;
    }

    #endregion

    private void CheckLevelManager()
    {

        if (_levelManager == null || !_levelManager.HasPlayer || !_levelManager.HasTerrainManager)
        {
            Debug.LogWarning("Level manager, terrain manager, or player not found by camera operator. Camera will be static.");
            Destroy(this);
            return;
        }
    }

    #region MinMax Management
    private void BuildHighLowCaches(LevelTerrain terrain)
    {
        //Get low and high point minmax caches from positional list factory
        PositionalListFactory<PositionObject<Vector3>>.CameraHighLowTrackers(this, terrain, out _lowPoints, out _highPoints);

        //Subscribe to new low/high point events in minmax caches
        _lowPoints.MinMax.OnNewMinMax += NewLowPoint;
        _highPoints.MinMax.OnNewMinMax += NewHighPoint;
    }

    private void UpdateHighLowCaches()
    {
        _lowPoints.Update();
        _highPoints.Update();
    }

    private void NewLowPoint(Vector3 newLow)
    {
        _lastLowPoint = _currentLowPoint;
        _targetLowPoint = newLow;
        _distanceBetweenLowPoints = Mathf.Abs(_lastLowPoint.x - _targetLowPoint.x);
    }
    private void NewHighPoint(Vector3 newHigh)
    {
        _targetHighPoint = newHigh;
    }

    private Vector3 LerpedLowPoint(Vector3 targetLow, Vector3 lastLow)
    {
        float currentXDistance = Mathf.Abs(CurrentLowPointX - lastLow.x);
        return new(CurrentLowPointX, Mathf.SmoothStep(lastLow.y, targetLow.y, Mathf.Clamp01(currentXDistance / _distanceBetweenLowPoints)));
    }

    private void UpdateHighLowBuffers()
    {
        UpdateHighPointBuffer();
        UpdateLowPointBuffer();
    }

    private void UpdateLowPointBuffer()
    {
        _lowPointBuffer = Mathf.Clamp(Camera.orthographicSize / DefaultSize, 1, 1.5f) * DefaultLowPointBuffer;
    }

    private void UpdateHighPointBuffer()
    {
        _highPointBuffer = Mathf.Clamp(Camera.orthographicSize / DefaultSize, 1, 1.5f) * DefaultHighPointBuffer;
    }

    #endregion
}
