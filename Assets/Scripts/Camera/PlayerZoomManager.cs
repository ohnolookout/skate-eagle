using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Com.LuisPedroFonseca.ProCamera2D;


public class PlayerZoomManager : MonoBehaviour
{
    #region Declarations
    private Camera _camera;
    private Transform _camContainer;
    private bool _doPlayerZoom = false,_doTransitionTargetSize = false;
    private float _targetSize, _transitionTargetSize, _defaultSize, _zoomYDelta = 0;
    private float _defaultYOffset;
    [SerializeField] private LevelManager _levelManager;
    public Action<Camera> OnZoomOut;
    public Action OnFinishZoomIn;
    public float ZoomYDelta => _zoomYDelta;
    public bool DoPlayerZoom { get => _doPlayerZoom; set => _doPlayerZoom = value; }
    public float TargetSize => _targetSize;
    private IPlayer _player => _levelManager.Player;
    #endregion

    #region Construct and Update
    private void Awake()
    {
        _camera = Camera.main;
        _camContainer = _camera.transform.parent;
        _defaultSize = _camera.orthographicSize;
        _targetSize = _defaultSize;
        _transitionTargetSize = _defaultSize;
        _defaultYOffset = ((CameraTargetUtility.MinYOffsetT + CameraTargetUtility.MaxYOffsetT)/2) * CameraTargetUtility.DefaultOrthoSize;
    }
    void Update()
    {
        if (_player == null)
        {
            return;
        }
        if (_doPlayerZoom)
        {
            PlayerZoom();
        }
        else if (!PlayerInsideYBuffer(0.8f) && _player.NormalBody.linearVelocityY >= 0)
        {
            _doPlayerZoom = true;
            OnZoomOut?.Invoke(_camera);
            PlayerZoom();
        }
        _camContainer.position = new(0, _zoomYDelta);
    }

    #endregion

    #region PlayerZoom

    private void PlayerZoom()
    {
        float change = PlayerZoomChange();

        _camera.orthographicSize += change;
        _zoomYDelta += change;
        _doPlayerZoom = !EndPlayerZoom();
    }

    private float PlayerZoomChange()
    {
        float camBottomY = _camera.ViewportToWorldPoint(new Vector3(0, 0, 0)).y;
        float change = (_player.NormalBody.position.y - camBottomY) / 1.8f;
        return change - _camera.orthographicSize;
    }

    private bool EndPlayerZoom()
    {
        return _camera.orthographicSize < _transitionTargetSize * 1.05f;
    }
    private bool PlayerInsideYBuffer(float buffer)
    {
        float camBufferY = _camera.transform.position.y + _camera.orthographicSize * buffer;
        return _player.NormalBody.position.y < camBufferY;
    }
    #endregion

    public void ResetZoom()
    {
        _camera.orthographicSize = _defaultSize;
        _zoomYDelta = 0;
        _doTransitionTargetSize = false;
        _doPlayerZoom = false;
    }

}
