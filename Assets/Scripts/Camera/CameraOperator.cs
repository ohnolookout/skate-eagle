using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraOperator : MonoBehaviour, ICameraOperator
{
    #region Declarations
    [SerializeField] private Vector3 _offset = new(27, 23);
    [SerializeField] private float _lowPointXOffset = 10;
    private bool _isZooming = false;
    private CameraZoom _zoom;
    private CameraHighLowManager _highLowManager;
    private Vector3 _leadingCorner, _trailingCorner;
    private float _defaultSize;
    private ILevelManager _levelManager;
    private IPlayer _player;
    private Rigidbody2D _playerBody;
    private Camera _cam;
    private bool _isFinished = false;
    public Action OnFinishZoomIn { get; set; }
    public Action<ICameraOperator> OnZoomOut { get; set; }
    public Vector3 LeadingCorner { get => _leadingCorner; }
    public Vector3 TrailingCorner { get => _trailingCorner; }
    public float CurrentHighLowX => transform.position.x + _lowPointXOffset;
    public float ReverseHighLowX => _playerBody.transform.position.x - _lowPointXOffset - _offset.x;
    public Camera Camera { get => _cam; }
    public float DefaultSize { get => _defaultSize; }
    public CameraZoom Zoom => _zoom;

    new public GameObject gameObject => transform.gameObject;
    public Rigidbody2D PlayerBody => _playerBody;
    #endregion

    #region Monobehaviours
    void Awake()
    {
        //Assign levelmanager and destroy cameraOperator if levelmanager isn't valid
        _levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<ILevelManager>();

#if UNITY_EDITOR
        if (_levelManager == null || !_levelManager.HasPlayer || !_levelManager.HasTerrainManager)
        {
            Debug.LogWarning("Level manager, terrain manager, or player not found by camera operator. Camera will be static.");
            Destroy(this);
            return;
        }
#endif
        //Assign default camera components
        _cam = GetComponent<Camera>();
        _defaultSize = _cam.orthographicSize;
        _zoom = new(this);
        _zoom.OnZoomOut += OnZoomOut;
        _zoom.OnFinishZoomIn += OnFinishZoomIn;

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
        _highLowManager = new(this, _levelManager.GroundManager.Ground);
        _highLowManager.HighPoints.MinMax.OnNewMinMax += (_) => _zoom.UpdateHighLowZoom(_highLowManager);
        _highLowManager.LowPoints.MinMax.OnNewMinMax += (_) => _zoom.UpdateHighLowZoom(_highLowManager);
        _zoom.SubscribeToPlayerLanding(_player);
        UpdatePosition();
    }

    void Update()
    {
        //If player falls below bottom of screen, trigger fall
        if (transform.position.y - _cam.orthographicSize - 10 > _playerBody.position.y)
        {
            _levelManager.Fall();
        }
    }

    void FixedUpdate()
    {
        _highLowManager.UpdateCurrentHighLow(_isZooming);
        UpdatePosition();
        _isZooming = _zoom.UpdateZoom();
    }
    /*
    void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            return;
        }
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(_highLowManager.CurrentLowPoint, 1);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(_highLowManager.TargetLowPoint, 1);
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(_highLowManager.LastLowPoint, 1);
        Gizmos.color = Color.yellow;
        Vector3 topRight = _cam.ViewportToWorldPoint(new Vector3(1, 1, 0));
        Vector3 bottomLeft = _cam.ViewportToWorldPoint(new Vector3(0, 0, 0));
        float topY = topRight.y;
        float bottomY =bottomLeft.y;
        float leadingX = _highLowManager.LowPoints.PositionList.LeadingX;
        float trailingX = _highLowManager.LowPoints.PositionList.TrailingX;
        Gizmos.DrawLine(new(leadingX, bottomY), new(leadingX, topY));
        Gizmos.DrawLine(new(trailingX, bottomY), new(trailingX, topY));
        Gizmos.color = Color.magenta;
        float targetTopY = bottomY + _zoom.TargetSize * 2;
        Gizmos.DrawLine(new(bottomLeft.x, targetTopY), new(topRight.x, targetTopY));

        float bufferY = _cam.transform.position.y + _cam.orthographicSize * 0.8f;
        Gizmos.color = Color.white;
        Gizmos.DrawLine(new(bottomLeft.x, bufferY), new(topRight.x, bufferY));

        
        bufferY = _cam.transform.position.y + _cam.orthographicSize * 0.9f;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(new(bottomLeft.x, bufferY), new(topRight.x, bufferY));
        
    }
    */
    #endregion

    #region Position
    private void SetInitialPosition()
    {
        var startPosition = new Vector3(_levelManager.GroundManager.StartPoint.x + _offset.x, _levelManager.GroundManager.StartPoint.y + _offset.y, transform.position.z);
        //Set camera's position to player's position plus offset
        transform.position = startPosition; 
    }
    private void UpdatePosition()
    {
        //Add offset and zoom changes to low point y
        float cameraY = _highLowManager.CurrentLowPoint.y + _offset.y + _zoom.ZoomYDelta;

        float cameraX;

        //Freeze X on finish
        if (_isFinished)
        {
            cameraX = transform.position.x;
        }
        else
        {
            //Get x from player's position, offset, and the zoom Y delta adjusted by the aspect ratio to represent the x delta
            cameraX = _playerBody.position.x + _offset.x + (_zoom.ZoomYDelta * (1 / Camera.main.aspect));
        }
        //Assign updated x and y to camera's transform
        transform.position = new Vector3(cameraX, cameraY, transform.position.z);

        //Update leading and trailing corner values
        _leadingCorner = _cam.ViewportToWorldPoint(new Vector3(1, 1, 0));
        _trailingCorner = _cam.ViewportToWorldPoint(new Vector3(0, 1, 0));
    }
    #endregion
}
