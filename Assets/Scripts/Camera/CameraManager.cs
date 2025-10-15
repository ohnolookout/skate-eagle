using Com.LuisPedroFonseca.ProCamera2D;
using GooglePlayGames.BasicApi;
using System.Collections.Generic;
using Unity.Hierarchy;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    private LinkedCameraTarget _currentLeftTarget;
    private LinkedHighPoint _currentHighPoint;
    private Camera _camera;
    private Vector3 _targetPosition;
    private float _targetOrthoSize;
    public bool doLogPosition = false;
    private bool _doUpdate = true;
    private IPlayer _player;
    private Transform _playerTransform;
    private Ground _currentGround;
    private bool _doCheckHighPointExit = false;
    private bool _doDirectionChangeDampen = false;
    public const float minXOffset = 20;
    public const float maxXOffset = 110;
    private const float _minOffsetVel = 0;
    private const float _maxOffsetVel = 100;
    private float _xOffset;
    private const float _defaultXDampen = 0.35f;
    private float _xDampen;
    private float _xDampenOnDirectionChange = 0.005f;
    private const float _defaultYDampen = 0.2f;
    private float _yDampenOnDirectionChange = 0.05f;
    private float _yDampen;
    private const float _defaultZoomDampen = 0.2f;
    private float _zoomDampenOnDirectionChange = 0.03f;
    private float _zoomDampen;
    private float _aspectRatio;
    private float _xBufferT = 0.7f;
    private const float _xOffsetDampen = 0.05f;
    private float _targetXOffset;

    void Awake()
    {
        _camera = Camera.main;
        FreezeCamera();

        LevelManager.OnPlayerCreated += AddPlayer;
        LevelManager.OnLanding += GoToStartPosition;

        //Freeze events
        LevelManager.OnStandby += UnfreezeCamera;
        LevelManager.OnFall += FreezeCamera;
        LevelManager.OnCrossFinish += FreezeCamera;
        LevelManager.OnGameOver += FreezeCamera;

        _xDampen = _defaultXDampen;
        _yDampen = _defaultYDampen;
        _zoomDampen = _defaultZoomDampen;
        _aspectRatio = _camera.aspect;
        _xOffset = minXOffset;

    }

    void FixedUpdate()
    {
        if (!_doUpdate)
        {
            return;
        }

        if (!_doCheckHighPointExit)
        {
            UpdateTargetPos();
        }
        else
        {
            UpdateCurrentHighPoint();
            CheckDirectionChange();
        }

        MoveToTargetPos();

    }

    private void OnDrawGizmosSelected()
    {

    }

    private void AddPlayer(IPlayer player)
    {
        _player = player;
        _playerTransform = player.Transform;
        _player.EventAnnouncer.SubscribeToEvent(PlayerEvent.SwitchDirection, OnSwitchPlayerDirection);
        _player.EventAnnouncer.SubscribeToAddCollision(OnPlayerCollide);
        UpdateTargetPos();
    }

    private void UpdateTargetPos()
    {
        if (_player == null)
        {
            return;
        }
        _xOffset = GetXOffset();
        var directionalXOffset = _player.FacingForward ? _xOffset : -_xOffset;
        var targetX = _playerTransform.position.x + directionalXOffset;
        var camX = _playerTransform.position.x + (directionalXOffset / 2);

        UpdateCurrentTarget(targetX);
        UpdateCurrentHighPoint();
        var camParams = CameraTargetUtility.GetCamParams(targetX, _currentLeftTarget);

        var adjustedOrthoSize = CheckPlayerZoom(camParams.orthoSize, camParams.camBottomY);
        Vector3 centerPosition = new(camX, camParams.camBottomY + adjustedOrthoSize);

        _targetOrthoSize = adjustedOrthoSize;
        _targetPosition = centerPosition;
    }

    private float GetXOffset()
    {
        var velOffsetT = (_player.NormalBody.linearVelocity.magnitude - _minOffsetVel) / (_maxOffsetVel - _minOffsetVel);
        _targetXOffset = Mathf.Lerp(minXOffset, maxXOffset, velOffsetT);
        return Mathf.Lerp(_xOffset, _targetXOffset, _xOffsetDampen);
    }

    private void MoveToTargetPos()
    {
        if (_doDirectionChangeDampen)
        {
            _xDampen = Mathf.SmoothStep(_xDampen, _defaultXDampen, 0.1f);
            _yDampen = Mathf.SmoothStep(_yDampen, _defaultYDampen, 0.1f);
            _zoomDampen = Mathf.SmoothStep(_zoomDampen, _defaultZoomDampen, 0.1f);
            if (Mathf.Abs(_defaultXDampen - _xDampen) < 0.02 && Mathf.Abs(_defaultYDampen - _yDampen) < 0.02)
            {
                _xDampen = _defaultXDampen;
                _yDampen = _defaultYDampen;
                _zoomDampen = _defaultZoomDampen;
                _doDirectionChangeDampen = false;
            }
        }
        var camPos = _camera.transform.position;
        var newY = Mathf.SmoothStep(camPos.y, _targetPosition.y, _yDampen);
        var newX = Mathf.SmoothStep(camPos.x, _targetPosition.x, _xDampen);
        var newOrthoSize = Mathf.SmoothStep(_camera.orthographicSize, _targetOrthoSize, _zoomDampen);

        _camera.transform.position = new(newX, newY);
        _camera.orthographicSize = newOrthoSize;
    }


    private void UpdateCurrentTarget(float xPos)
    {
        if(xPos < _currentLeftTarget.Position.x && _currentLeftTarget.prevTarget != null)
        {
            while (_currentLeftTarget.prevTarget != null && _currentLeftTarget.Position.x > xPos)
            {
                _currentLeftTarget = _currentLeftTarget.prevTarget;
            }

            return;
        }

        while (_currentLeftTarget.nextTarget != null && xPos > _currentLeftTarget.nextTarget.Position.x)
        {
            _currentLeftTarget = _currentLeftTarget.nextTarget;
        }

    }

    private void UpdateCurrentHighPoint()
    {
        if (_currentHighPoint == null)
        {
            Debug.LogWarning("CameraManager: Current high point is null. Assign a high point to the starting ground.");
            return;
        }

        bool hasChanged = false;
        while(_currentHighPoint.previous != null && _playerTransform.position.x < _currentHighPoint.position.x)
        {
            hasChanged = true;
            _currentHighPoint = _currentHighPoint.previous;
        }

        while(_currentHighPoint.next != null && _playerTransform.position.x > _currentHighPoint.next.position.x)
        {
            hasChanged = true;
            _currentHighPoint = _currentHighPoint.next;
        }

        if(hasChanged == true && _doCheckHighPointExit == true)
        {
            ExitDirectionChange();
        }
    }

    private void OnSwitchPlayerDirection(IPlayer player)
    {
        var targetX = _currentHighPoint.position.x + ((_currentHighPoint.next.position.x - _currentHighPoint.position.x)/2);
        UpdateCurrentTarget(targetX);
        var camParams = CameraTargetUtility.GetCamParams(targetX, _currentLeftTarget);
        _targetPosition = new Vector3(targetX, camParams.camBottomY + camParams.orthoSize);
        _targetOrthoSize = camParams.orthoSize;

        _doCheckHighPointExit = true;
        _xDampen = _defaultXDampen/4;
        _yDampen = _defaultYDampen/3;
        _zoomDampen = _defaultZoomDampen/3;

    }

    private void CheckDirectionChange()
    {
        var playerX = _playerTransform.position.x;
        var camDist = Mathf.Abs(playerX - _camera.transform.position.x);

        
        if (camDist < _camera.orthographicSize * _aspectRatio * _xBufferT)
        { 
            return;
        }

        ExitDirectionChange();
    }

    private void ExitDirectionChange()
    {
        _doDirectionChangeDampen = true;
        _doCheckHighPointExit = false;
        _xDampen = _xDampenOnDirectionChange;
        _yDampen = _yDampenOnDirectionChange;
        _zoomDampen = _zoomDampenOnDirectionChange;

    }

    private void OnPlayerCollide(Collision2D collision, MomentumTracker _, ColliderCategory __, TrackingType ___)
    {
        if (!_player.Airborne)
        {
            return;
        }

        var collidedTransformParent = collision.transform.parent;

        if (collidedTransformParent == _currentGround.transform)
        {
            return;
        }

        var playerPos = _player.NormalBody.position;

        var collidedSeg = collision.gameObject.GetComponent<GroundSegment>();
        _currentGround = collidedSeg.parentGround;
        _currentLeftTarget = collidedSeg.StartTarget;
        _currentHighPoint = collidedSeg.StartHighPoint;

        UpdateTargetPos();

    }

    private void FreezeCamera()
    {
        _doUpdate = false;
    }

    private void UnfreezeCamera()
    {
        _doUpdate = true;
    }

    private void GoToStartPosition(Level level, PlayerRecord _)
    {
        FreezeCamera();
        Camera.main.transform.position = level.SerializedStartLine.CamStartPosition;
        Camera.main.orthographicSize = level.SerializedStartLine.CamOrthoSize;
        _currentLeftTarget = level.SerializedStartLine.FirstCameraTarget;
        _currentHighPoint = level.SerializedStartLine.FirstHighPoint;
        _doDirectionChangeDampen = false;
        _doCheckHighPointExit = false;
        _xDampen = _defaultXDampen;
        _yDampen = _defaultYDampen;
        _zoomDampen = _defaultZoomDampen;
        _xOffset = minXOffset;

    }

    private float CheckPlayerZoom(float targetOrthoSize, float camBottomY)
    {
        var playerY = _playerTransform.position.y;
        var yDist = playerY - camBottomY;
        var playerZoomSize = yDist / (1 + CameraTargetUtility.PlayerHighYT);

        return Mathf.Max(playerZoomSize, targetOrthoSize);
    }

}

