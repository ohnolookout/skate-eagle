using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Com.LuisPedroFonseca.ProCamera2D;


public class CameraZoom : MonoBehaviour
{
    #region Declarations
    private ProCamera2D _camera;
    private Transform _camContainer;
    private IPlayer _player;
    private bool _doPlayerZoom = false,_doTransitionTargetSize = false;
    private float _targetSize, _transitionTargetSize, _defaultSize, _zoomYDelta = 0;
    private float _defaultYOffset;
    public Action<ProCamera2D> OnZoomOut;
    public Action OnFinishZoomIn;
    public float ZoomYDelta => _zoomYDelta;
    public bool DoPlayerZoom { get => _doPlayerZoom; set => _doPlayerZoom = value; }
    public float TargetSize => _targetSize;
    #endregion

    #region Construct and Update
    private void Awake()
    {
        _camera = ProCamera2D.Instance;
        _camContainer = _camera.transform.parent;
        _defaultSize = _camera.GameCamera.orthographicSize;
        _targetSize = _defaultSize;
        _transitionTargetSize = _defaultSize;
        _defaultYOffset = _camera.GetOffsetY();

        LevelManager.OnPlayerCreated += OnPlayerCreated;
    }
    void Update()
    {
        if (_player == null)
        {
            return;
        }
        if (_doTransitionTargetSize)
        {
            TransitionTargetSize();
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

    public void SubscribeToPlayerLanding(IPlayer player)
    {
        player.EventAnnouncer.SubscribeToEvent(PlayerEvent.Land, EndZoomOut);
    }

    #endregion

    #region PlayerZoom

    private void EndZoomOut(IPlayer _)
    {
        _doPlayerZoom = false;
    }
    private void PlayerZoom()
    {
        float change = PlayerZoomChange();

        _camera.GameCamera.orthographicSize += change;
        _zoomYDelta += change;
        _doPlayerZoom = !EndPlayerZoom();
    }

    private float PlayerZoomChange()
    {
        float camBottomY = _camera.GameCamera.ViewportToWorldPoint(new Vector3(0, 0, 0)).y;
        float change = (_player.NormalBody.position.y - camBottomY) / 1.8f;
        return change - _camera.GameCamera.orthographicSize;
    }

    private bool EndPlayerZoom()
    {
        return _camera.GameCamera.orthographicSize < _transitionTargetSize * 1.05f;
    }
    private bool PlayerInsideYBuffer(float buffer)
    {
        float camBufferY = _camera.transform.position.y + _camera.GameCamera.orthographicSize * buffer;
        return _player.NormalBody.position.y < camBufferY;
    }
    #endregion

    #region HighLowZoom
    //Use adjacent lowpoints for highLowDistance rather than current lowpoint
    /*
    public void UpdateHighLowZoom(CameraHighLowManager highLowManager)
    {
        _transitionTargetSize = _targetSize;
        if (highLowManager.TargetHighPoint.Distance > _defaultSize * 1.4f)
        {
            _targetSize = highLowManager.TargetHighPoint.Distance * 0.72f;
        }
        else
        {
            _targetSize = _defaultSize;
        }
        _doTransitionTargetSize = true;
    }
    */
    //Base this on greater of time or distance to ensure smooth transition
    private void HighLowZoom()
    {
        float sizeChange = HighLowChange(_camera.GameCamera.orthographicSize);
        _zoomYDelta += sizeChange;
        _camera.GameCamera.orthographicSize += sizeChange;
        _transitionTargetSize = _camera.GameCamera.orthographicSize;

    }

    private const float _minZoom = 0.2f;
    private float HighLowChange(float currentSize)
    {
        float totalDifference = _targetSize - currentSize;
        if (_targetSize > currentSize)
        {
            return Mathf.Min(((totalDifference) / 60) * _minZoom + _minZoom, totalDifference);
        }
        return Mathf.Max(((totalDifference) / 60) * _minZoom - _minZoom, totalDifference);
    }

    private void TransitionTargetSize()
    {
        if(_transitionTargetSize != _targetSize)
        {
            _transitionTargetSize += HighLowChange(_transitionTargetSize);
        }
        else
        {
            _doTransitionTargetSize = false;
        }
    }
    #endregion

    private void OnPlayerCreated(IPlayer player)
    {
        _player = player;
    }

    public void ResetZoom()
    {
        _camera.GameCamera.orthographicSize = _defaultSize;
        _zoomYDelta = 0;
        _doTransitionTargetSize = false;
        _doPlayerZoom = false;
    }

}
