using UnityEngine.U2D;
using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class CameraOperator : MonoBehaviour, ICameraOperator
{
    [SerializeField] private Vector3 offset;
    private Vector3 leadingCorner, trailingCorner;
    private float defaultSize, zoomYDelta = 0, camY, targetY = 0;
    private bool _cameraZoomOut = false, _cameraZoomIn = false;
    private ILevelManager _levelManager;
    private LowpointCache _lowPoints;
    private IPlayer _player;
    private IEnumerator transitionYCoroutine, zoomOutRoutine, zoomInRoutine;
    private Camera cam;
    private bool _isFinished = false;
    public Action OnFinishZoomIn { get; set; }
    public Action<ICameraOperator> OnZoomOut { get; set; }

    void Awake()
    {
        AssignComponents();
        LevelManager.OnFinish += _ => _isFinished = true;
    }
    void Start()
    {
        if (_levelManager == null || !_levelManager.HasPlayer || !_levelManager.HasTerrainManager)
        {
            Debug.LogWarning("Level manager, terrain manager, or player not found by camera operator. Camera will be static.");
            Destroy(this);
            return;
        }
        camY = _lowPoints.LowestPoint.y;
        UpdatePosition();
    }

    void Update()
    {
        if (cam.WorldToScreenPoint(_player.Rigidbody.position).y < 0) _levelManager.Fall();
        UpdateZoom();
        if (!_isFinished)
        {
            UpdatePosition();
        }
    }

    private void AssignComponents()
    {
        defaultSize = Camera.main.orthographicSize;
        _levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<ILevelManager>();
        if (_levelManager == null)
        {
            Debug.LogWarning("No level manager found by camera operator. Camera will be static.");
            return;
        }
        if (_levelManager.HasTerrainManager)
        {
            _lowPoints = _levelManager.TerrainManager.Lowpoints;
            transitionYCoroutine = TransitionLowY(_lowPoints.LowestPoint);
        }
        if (_levelManager.HasPlayer)
        {
            _player = _levelManager.Player;
        }
        cam = GetComponent<Camera>();
    }

    private void UpdateZoom()
    {
        if (_cameraZoomOut || _player.Rigidbody.position.y < LeadingCorner.y - Camera.main.orthographicSize * 0.2f)
        {
            return;
        }

        if (_cameraZoomIn && _player.Rigidbody.velocity.y > 0)
        {
            StopCoroutine(zoomInRoutine);
            _cameraZoomIn = false;
        }
        else if (!_cameraZoomIn)
        {
            zoomOutRoutine = ZoomOut();
            StartCoroutine(zoomOutRoutine);
        }
    }

    private void UpdatePosition()
    {
        if (_lowPoints.LowestPoint.y != targetY)
        {
            StopCoroutine(transitionYCoroutine);
            transitionYCoroutine = TransitionLowY(_lowPoints.LowestPoint);
            StartCoroutine(transitionYCoroutine);
        }
        float cameraX = _levelManager.Player.Rigidbody.position.x + offset.x + (zoomYDelta * (1 / Camera.main.aspect));
        float cameraY = camY + offset.y + zoomYDelta;
        transform.position = new Vector3(cameraX, cameraY, transform.position.z);
        leadingCorner = cam.ViewportToWorldPoint(new Vector3(1, 1, 0));
        trailingCorner = cam.ViewportToWorldPoint(new Vector3(0, 1, 0));
    }
    private IEnumerator TransitionLowY(Vector3 endPoint)
    {
        float startBirdX = _player.Rigidbody.position.x;
        float distance = Mathf.Clamp(Mathf.Abs(endPoint.x - (_levelManager.Player.Rigidbody.position.x + 20)), 15, 100);
        float startY = camY;
        targetY = endPoint.y;
        float t = 0;
        while (Mathf.Abs(camY - endPoint.y) > 0.2)
        {
            t = Mathf.Clamp01(Mathf.Abs(_player.Rigidbody.position.x - startBirdX) / distance);
            camY = Mathf.SmoothStep(startY, targetY, t);
            yield return null;
        }
    }

    private IEnumerator ZoomOut()
    {
        OnZoomOut?.Invoke(this);

        _cameraZoomOut = true;
        while (_player.Rigidbody.position.y > LeadingCorner.y - cam.orthographicSize * 0.2f
            || _player.Rigidbody.velocity.y > 0)
        {
            float change = Mathf.Clamp(_levelManager.Player.Rigidbody.velocity.y, 0.5f, 99999) * 0.65f * Time.fixedDeltaTime;
            cam.orthographicSize += change;
            zoomYDelta += change;
            yield return new WaitForFixedUpdate();
        }
        _cameraZoomOut = false;
        if (_cameraZoomIn) StopCoroutine(zoomInRoutine);
        zoomInRoutine = ZoomIn();
        StartCoroutine(zoomInRoutine);
    }

    private IEnumerator ZoomIn()
    {
        _cameraZoomIn = true;
        while (cam.orthographicSize > defaultSize)
        {
            float change = Mathf.Clamp(_levelManager.Player.Rigidbody.velocity.y, -666, -1) * 0.5f * Time.fixedDeltaTime;
            cam.orthographicSize += change;
            zoomYDelta += change;
            yield return new WaitForFixedUpdate();
        }
        _cameraZoomIn = false;
        OnFinishZoomIn?.Invoke();
    }
    public Vector3 LeadingCorner { get => leadingCorner; }
    public Vector3 TrailingCorner { get => trailingCorner; }
    public Vector3 Center { get => cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0)); }
    public float ZoomYDelta { get => zoomYDelta; }
    public Camera Camera { get => cam; }
    public float DefaultSize { get => defaultSize; }
    public bool CameraZoomOut { get => _cameraZoomOut; set => _cameraZoomOut = value; }
    public bool CameraZoomIn { get => _cameraZoomIn; set => _cameraZoomIn = value; }
}