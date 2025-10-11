using Com.LuisPedroFonseca.ProCamera2D;
using System.Collections.Generic;
using Unity.Hierarchy;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CameraManager : MonoBehaviour
{
    private LinkedCameraTarget _currentLeftTarget;
    private Camera _camera;
    private Vector3 _targetPosition;
    private float _targetOrthoSize;
    public bool doLogPosition = false;
    private bool _doUpdate = true;
    private IPlayer _player;
    private Transform _playerTransform;
    private bool _doPlayerZoom = false;
    private Ground _currentGround;
    private float _xOffset = CameraTargetUtility.DefaultPlayerXOffset + CameraTargetUtility.DefaultTargetXOffset;
    private const float _defaultXDampen = 0.15f;
    private float _xDampen;
    private float _xDampenOnDirectionChange = 0.01f;
    private const float _defaultYDampen = 0.15f;
    private float _yDampenOnDirectionChange = 0.03f;
    private float _yDampen;
    private const float _defaultZoomDampen = 0.1f;
    private float _zoomDampenOnDirectionChange = 0.02f;
    private float _zoomDampen;
    private bool _doDirectionChangeDampen = false;

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

    }

    void Update()
    {
        if (_doUpdate)
        {
            UpdateTargetPos();
            MoveToTargetPos();
        }

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
        var directionalXOffset = _player.FacingForward ? _xOffset : -_xOffset;        
        var targetX = _playerTransform.position.x + directionalXOffset;
        var camX = _playerTransform.position.x + (directionalXOffset / 2);

        UpdateCurrentTarget(targetX);
        var camParams = CameraTargetUtility.GetCamParams(targetX, _currentLeftTarget);

        var adjustedOrthoSize = CheckPlayerZoom(camParams.orthoSize, camParams.camBottomY);
        Vector3 centerPosition = new(camX, camParams.camBottomY + adjustedOrthoSize);

        _targetOrthoSize = adjustedOrthoSize;
        _targetPosition = centerPosition;
    }

    private void MoveToTargetPos()
    {
        if (_doDirectionChangeDampen)
        {
            Debug.Log("Doing direction change dampen");
            _xDampen = Mathf.SmoothStep(_xDampen, _defaultXDampen, 0.15f);
            _yDampen = Mathf.SmoothStep(_yDampen, _defaultYDampen, 0.15f);
            _zoomDampen = Mathf.SmoothStep(_zoomDampen, _defaultZoomDampen, 0.15f);
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

    private void OnSwitchPlayerDirection(IPlayer player)
    {
        _doDirectionChangeDampen = true;
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
        var leftTarget = _currentGround.FindNearestLeftLowPoint(_player.NormalBody.position, collidedSeg);

        if (leftTarget.nextTarget == null)
        {
            _currentLeftTarget = leftTarget;
        }
        else {
            var leftDistance = Mathf.Abs(leftTarget.Position.x - playerPos.x);
            var rightDistance = Mathf.Abs(leftTarget.nextTarget.Position.x - playerPos.x);

            _currentLeftTarget = leftDistance < rightDistance ? leftTarget : leftTarget.nextTarget;
        }

    }

    private void FreezeCamera()
    {
        _doUpdate = false;
        _doPlayerZoom = false;
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
        _doPlayerZoom = false;
        _doDirectionChangeDampen = false;
        _xDampen = _defaultXDampen;

    }

    private float CheckPlayerZoom(float targetOrthoSize, float camBottomY)
    {
        var playerY = _playerTransform.position.y;
        var yDist = playerY - camBottomY;
        var playerZoomSize = yDist / (1 + CameraTargetUtility.PlayerHighYT);

        return Mathf.Max(playerZoomSize, targetOrthoSize);
    }

}

