using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CameraZoom
{
    #region Declarations
    private Camera _cam;
    private ICameraOperator _cameraOperator;
    private bool _doPlayerZoom = false,_doTransitionTargetSize = false;
    private float _finalTargetSize, _currentTargetSize, _zoomYDelta = 0;
    public Action<ICameraOperator> OnZoomOut;
    public Action OnFinishZoomIn;
    public float ZoomYDelta => _zoomYDelta;
    public bool DoPlayerZoom { get => _doPlayerZoom; set => _doPlayerZoom = value; }
    public float TargetSize => _finalTargetSize;
    #endregion

    #region Construct and Update
    public CameraZoom(ICameraOperator cameraOperator)
    {
        _cameraOperator = cameraOperator;
        _cam = cameraOperator.Camera;
        _finalTargetSize = cameraOperator.DefaultSize;
        _currentTargetSize = _finalTargetSize;
    }

    public void SubscribeToPlayerLanding(IPlayer player)
    {
        player.EventAnnouncer.SubscribeToEvent(PlayerEvent.Land, (_) => _doPlayerZoom = false);
    }

    public bool UpdateZoom()
    {
        if (_doTransitionTargetSize)
        {
            TransitionTargetSize();
        }
        if (_doPlayerZoom)
        {
            PlayerZoom();
        }
        else if (!PlayerInsideYBuffer(0.8f) && _cameraOperator.PlayerBody.velocityY >= 0)
        {
            _doPlayerZoom = true;
            OnZoomOut?.Invoke(_cameraOperator);
            PlayerZoom();
        } else if (_cam.orthographicSize != _finalTargetSize)
        {
            HighLowZoom();
        }
        else
        {
            return false;
        }
        return true;
    }
    #endregion

    #region PlayerZoom
    private void PlayerZoom()
    {
        float change = PlayerZoomChange();

        _cam.orthographicSize += change;
        _zoomYDelta += change;
        _doPlayerZoom = !EndPlayerZoom();
    }

    private float PlayerZoomChange()
    {
        float camBottomY = _cam.ViewportToWorldPoint(new Vector3(0, 0, 0)).y;
        float change = (_cameraOperator.PlayerBody.position.y - camBottomY) / 1.8f;
        return change - _cam.orthographicSize;
    }

    private bool EndPlayerZoom()
    {
        return _cam.orthographicSize < _currentTargetSize * 1.05f;
    }
    private bool PlayerInsideYBuffer(float buffer)
    {

        float camBufferY = _cam.transform.position.y + _cam.orthographicSize * buffer;
        return _cameraOperator.PlayerBody.position.y < camBufferY;
    }
    #endregion

    #region HighLowZoom
    //Use adjacent lowpoints for highLowDistance rather than current lowpoint
    public void UpdateHighLowZoom(CameraHighLowManager highLowManager)
    {
        _currentTargetSize = _finalTargetSize;
        if (highLowManager.TargetHighPoint.Distance > _cameraOperator.DefaultSize * 1.4f)
        {
            _finalTargetSize = highLowManager.TargetHighPoint.Distance * 0.72f;
        }
        else
        {
            _finalTargetSize = _cameraOperator.DefaultSize;
        }
        _doTransitionTargetSize = true;
    }

    //Base this on greater of time or distance to ensure smooth transition
    private void HighLowZoom()
    {
        float sizeChange = HighLowChange(_cam.orthographicSize);
        _zoomYDelta += sizeChange;
        _cam.orthographicSize += sizeChange;
        _currentTargetSize = _cam.orthographicSize;

    }

    private const float _minZoom = 0.2f;
    private float HighLowChange(float currentSize)
    {
        float totalDifference = _finalTargetSize - currentSize;
        if (_finalTargetSize > currentSize)
        {
            return Mathf.Min(((totalDifference) / 60) * _minZoom + _minZoom, totalDifference);
        }
        return Mathf.Max(((totalDifference) / 60) * _minZoom - _minZoom, totalDifference);
    }

    private void TransitionTargetSize()
    {
        if(_currentTargetSize != _finalTargetSize)
        {
            _currentTargetSize += HighLowChange(_currentTargetSize);
        }
        else
        {
            _doTransitionTargetSize = false;
        }
    }
    #endregion

}
