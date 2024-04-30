using UnityEngine.U2D;
using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
[RequireComponent(typeof(Camera))]
public class CameraOperator_OLD : MonoBehaviour
{
    [SerializeField] private Vector3 _offset = new(27, 23);
    private Vector3 _leadingCorner, _trailingCorner;
    private float _defaultSize, _zoomYDelta = 0, _camY, _targetY = 0;
    private bool _cameraZoomOut = false, _cameraZoomIn = false;
    private ILevelManager _levelManager;
    private MinMaxCache _lowPoints;
    private IPlayer _player;
    private Rigidbody2D _playerBody;
    private IEnumerator _transitionYCoroutine, _zoomOutRoutine, _zoomInRoutine;
    private Camera _cam;
    private bool _isFinished = false;
    public Action OnFinishZoomIn { get; set; }
    public Action<CameraOperator_OLD> OnZoomOut { get; set; }
    public Vector3 LeadingCorner { get => _leadingCorner; }
    public Vector3 TrailingCorner { get => _trailingCorner; }
    public Vector3 Center { get => _cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0)); }
    public float ZoomYDelta { get => _zoomYDelta; }
    public Camera Camera { get => _cam; }
    public float DefaultSize { get => _defaultSize; }
    public bool IsZoomOut { get => _cameraZoomOut; set => _cameraZoomOut = value; }
    public bool CameraZoomIn { get => _cameraZoomIn; set => _cameraZoomIn = value; }
    new public GameObject gameObject => transform.gameObject;

    void Awake()
    {
        AssignComponents();
        LevelManager.OnFinish += _ => _isFinished = true;
        LevelManager.OnGameOver += _ => _playerBody = _player.RagdollBody;
        _playerBody = _player.NormalBody;
    }
    void Start()
    {
        if (_levelManager == null || !_levelManager.HasPlayer || !_levelManager.HasTerrainManager)
        {
            Debug.LogWarning("Level manager, terrain manager, or player not found by camera operator. Camera will be static.");
            Destroy(this);
            return;
        }

        if (_levelManager.HasTerrainManager)
        {
            //_lowPoints = _levelManager.TerrainManager.LowPointCache;
            _transitionYCoroutine = TransitionLowY(_lowPoints.CurrentPoint);
        }

        _camY = _lowPoints.CurrentPoint.y;
        UpdatePosition();
    }

    void Update()
    {
        if (_cam.WorldToScreenPoint(_playerBody.position).y < 0)
        {
            _levelManager.Fall();
        }
        UpdateZoom();
        if (!_isFinished)
        {
            UpdatePosition();
        }
    }

    private void AssignComponents()
    {
        _defaultSize = Camera.main.orthographicSize;
        _levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<ILevelManager>();
        if (_levelManager == null)
        {
            Debug.LogWarning("No level manager found by camera operator. Camera will be static.");
            return;
        }
        if (_levelManager.HasPlayer)
        {
            _player = LevelManager.GetPlayer;
        }
        _cam = GetComponent<Camera>();
    }

    private void UpdateZoom()
    {
        if (_cameraZoomOut || _playerBody.position.y < LeadingCorner.y - Camera.main.orthographicSize * 0.2f)
        {
            return;
        }

        if (_cameraZoomIn && _playerBody.velocity.y > 0)
        {
            StopCoroutine(_zoomInRoutine);
            _cameraZoomIn = false;
        }
        else if (!_cameraZoomIn)
        {
            _zoomOutRoutine = ZoomOut();
            StartCoroutine(_zoomOutRoutine);
        }
    }

    private void UpdatePosition()
    {
        if (_lowPoints.CurrentPoint.y != _targetY)
        {
            StopCoroutine(_transitionYCoroutine);
            _transitionYCoroutine = TransitionLowY(_lowPoints.CurrentPoint);
            StartCoroutine(_transitionYCoroutine);
        }
        float cameraX = _playerBody.position.x + _offset.x + (_zoomYDelta * (1 / Camera.main.aspect));
        float cameraY = _camY + _offset.y + _zoomYDelta;
        transform.position = new Vector3(cameraX, cameraY, transform.position.z);
        _leadingCorner = _cam.ViewportToWorldPoint(new Vector3(1, 1, 0));
        _trailingCorner = _cam.ViewportToWorldPoint(new Vector3(0, 1, 0));
    }
    private IEnumerator TransitionLowY(Vector3 endPoint)
    {
        float startBirdX = _playerBody.position.x;
        float distance = Mathf.Clamp(Mathf.Abs(endPoint.x - (_playerBody.position.x + 20)), 15, 100);
        float startY = _camY;
        _targetY = endPoint.y;
        float t = 0;
        while (Mathf.Abs(_camY - endPoint.y) > 0.2)
        {
            t = Mathf.Clamp01(Mathf.Abs(_playerBody.position.x - startBirdX) / distance);
            _camY = Mathf.SmoothStep(startY, _targetY, t);
            yield return null;
        }
    }

    private IEnumerator ZoomOut()
    {
        OnZoomOut?.Invoke(this);

        _cameraZoomOut = true;
        while (_playerBody.position.y > LeadingCorner.y - _cam.orthographicSize * 0.2f
            || _playerBody.velocity.y > 0)
        {
            float change = Mathf.Clamp(_playerBody.velocity.y, 0.5f, 99999) * 0.65f * Time.fixedDeltaTime;
            _cam.orthographicSize += change;
            _zoomYDelta += change;
            yield return new WaitForFixedUpdate();
        }
        _cameraZoomOut = false;
        if (_cameraZoomIn) StopCoroutine(_zoomInRoutine);
        _zoomInRoutine = ZoomIn();
        StartCoroutine(_zoomInRoutine);
    }

    private IEnumerator ZoomIn()
    {
        _cameraZoomIn = true;
        while (_cam.orthographicSize > _defaultSize)
        {
            float change = Mathf.Clamp(_playerBody.velocity.y, -666, -1) * 0.5f * Time.fixedDeltaTime;
            _cam.orthographicSize += change;
            _zoomYDelta += change;
            yield return new WaitForFixedUpdate();
        }
        _cameraZoomIn = false;
        OnFinishZoomIn?.Invoke();
    }
}