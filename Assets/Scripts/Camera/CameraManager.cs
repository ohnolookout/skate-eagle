using Com.LuisPedroFonseca.ProCamera2D;
using GooglePlayGames.BasicApi;
using System.Collections.Generic;
using Unity.Hierarchy;
using Unity.VisualScripting;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    private LinkedCameraTarget _currentLeftTarget;
    private LinkedCameraTarget _prevLeftTarget;
    private LinkedCameraTarget _nextLeftTarget;
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
            CheckHighPointExit();
        }

        MoveToTargetPos();

    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.orange;
        if(_currentHighPoint != null)
        {
            Gizmos.DrawSphere(_currentHighPoint.position, 2f);

            Gizmos.color = Color.darkOrange;
            if (_currentHighPoint.next != null)
            {
                Gizmos.DrawSphere(_currentHighPoint.next.position, 2f);
            }
        }

        Gizmos.color = Color.lightGreen;
        if(_currentLeftTarget != null)
        {
            Gizmos.DrawSphere(_currentLeftTarget.Position, 2f);
        }

        Gizmos.color = Color.darkOliveGreen;
        if(_nextLeftTarget != null)
        {
            Gizmos.DrawSphere(_nextLeftTarget.Position, 2f);
        }

        if(_player == null || _playerTransform == null)
        {
            return;
        }

        Gizmos.color = Color.blue;
        var directionalXOffset = _player.FacingForward ? _xOffset : -_xOffset;
        var targetX = _playerTransform.position.x + directionalXOffset;
        var bottomY = _camera.transform.position.y - _camera.orthographicSize;
        Gizmos.DrawLine(new Vector3(targetX, bottomY), new Vector3(targetX, bottomY + 5));

        Gizmos.color = Color.darkRed;
        Gizmos.DrawSphere(_targetPosition, 2f);

        Gizmos.color = Color.lightPink;
        Gizmos.DrawLine(_camera.transform.position, _targetPosition);
    }

    private void AddPlayer(IPlayer player)
    {
        _player = player;
        _playerTransform = player.Transform;
        _player.EventAnnouncer.SubscribeToEvent(PlayerEvent.SwitchDirection, OnSwitchPlayerDirection);
        _player.EventAnnouncer.SubscribeToEvent(PlayerEvent.Land, OnPlayerLand);
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
#if UNITY_EDITOR
        int searchCount = 0;
#endif
        Debug.Assert(_prevLeftTarget != null, "CameraManager: Prev left target is null.");
        while (_prevLeftTarget != null && _currentLeftTarget.Position.x > xPos)
        {
#if UNITY_EDITOR
            searchCount++;
            if(searchCount > 1)
            {
                Debug.LogWarning("Cam target search iterations > 1: " + searchCount);
            } 
#endif
            AssignPrevAndNextTargets(_currentLeftTarget, _prevLeftTarget, false);
        }

        Debug.Assert(_nextLeftTarget != null, "CameraManager: Next left target is null.");

        while (_nextLeftTarget != null && xPos > _nextLeftTarget.Position.x)
        {
#if UNITY_EDITOR
            searchCount++;
            if (searchCount > 1)
            {
                Debug.LogWarning("Cam target search iterations > 1: " + searchCount);
            }
#endif
            AssignPrevAndNextTargets(_currentLeftTarget, _nextLeftTarget, true);
        }

    }

    private void UpdateCurrentHighPoint()
    {
#if UNITY_EDITOR
        int searchCount = 0;
#endif
        if (_currentHighPoint == null)
        {
            Debug.LogWarning("CameraManager: Current high point is null. Assign a high point to ground.");
            return;
        }

        bool hasChanged = false;
        while(_currentHighPoint.previous != null && _playerTransform.position.x < _currentHighPoint.position.x)
        {
#if UNITY_EDITOR
            searchCount++;
            if (searchCount > 1)
            {
                Debug.LogWarning("Cam highpoint search iterations > 1: " + searchCount);
            }
#endif
            hasChanged = true;
            _currentHighPoint = _currentHighPoint.previous;
        }

        while(_currentHighPoint.next != null && _playerTransform.position.x > _currentHighPoint.next.position.x)
        {
#if UNITY_EDITOR
            searchCount++;
            if (searchCount > 1)
            {
                Debug.LogWarning("Cam highpoint search iterations > 1: " + searchCount);
            }
#endif
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
        if (_doCheckHighPointExit)
        {
            return;
        }

        UpdateCurrentHighPoint();

        float targetX;

        if(_currentHighPoint.next != null)
        {
            targetX = _currentHighPoint.position.x + ((_currentHighPoint.next.position.x - _currentHighPoint.position.x) / 2);            
        }
        else
        {
            targetX = _currentHighPoint.position.x;
        }
        UpdateCurrentTarget(targetX);
        var camParams = CameraTargetUtility.GetCamParams(targetX, _currentLeftTarget);
        _targetPosition = new Vector3(targetX, camParams.camBottomY + camParams.orthoSize);
        _targetOrthoSize = camParams.orthoSize;

        _doCheckHighPointExit = true;
        _xDampen = _defaultXDampen/4;
        _yDampen = _defaultYDampen/2;
        _zoomDampen = _defaultZoomDampen/2;

    }

    private void CheckHighPointExit()
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

    private void OnPlayerLand(IPlayer _)
    {
        var collision = _player.LastLandCollision;

        var collidedTransformParent = collision.transform.parent;

        if (_currentGround != null && collidedTransformParent.parent == _currentGround.transform)
        {
            return;
        }

        var playerPos = _player.NormalBody.position;

        var collidedSeg = collidedTransformParent.GetComponent<GroundSegment>();
        _currentGround = collidedSeg.parentGround;
        AssignPrevAndNextTargets(collidedSeg.FirstLeftTarget);
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
        _currentGround = null;
        _currentLeftTarget = level.SerializedStartLine.FirstCameraTarget;
        _prevLeftTarget = _currentLeftTarget.prevTarget;
        _nextLeftTarget = _currentLeftTarget.nextTarget;
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

    private void AssignPrevAndNextTargets(LinkedCameraTarget currentTarget, LinkedCameraTarget newTarget, bool moveRight)
    {
        _currentLeftTarget = newTarget;

        if (moveRight)
        {
            _prevLeftTarget = currentTarget;
            Debug.Log("Prev left target assigned to current target. New prev left target: " + _prevLeftTarget);
            if (newTarget.nextTarget != null)
            {
                _nextLeftTarget = newTarget.nextTarget;
            }
            else
            {
                var currentIndex = _currentGround.LowTargets.IndexOf(_currentLeftTarget);
                if (currentIndex >= 0 && currentIndex < _currentGround.LowTargets.Count - 1)
                {
                    _nextLeftTarget = _currentGround.LowTargets[currentIndex + 1];
                }
                else
                {
                    _nextLeftTarget = null;
                }
            }
        }
        else
        {
            _nextLeftTarget = currentTarget;

            if (newTarget.prevTarget != null)
            {
                _prevLeftTarget = newTarget.prevTarget;
            }
            else
            {
                var currentIndex = _currentGround.LowTargets.IndexOf(_currentLeftTarget);
                if (currentIndex > 0)
                {
                    _prevLeftTarget = _currentGround.LowTargets[currentIndex - 1];
                }
                else
                {
                    _prevLeftTarget = null;
                }
            }
        }
    }

    private void AssignPrevAndNextTargets(LinkedCameraTarget newTarget)
    {
        _currentLeftTarget = newTarget;

        if (newTarget.nextTarget != null)
        {
            _nextLeftTarget = newTarget.nextTarget;
        }
        else
        {
            var currentIndex = _currentGround.LowTargets.IndexOf(_currentLeftTarget);
            if (currentIndex < _currentGround.LowTargets.Count - 1)
            {
                _nextLeftTarget = _currentGround.LowTargets[currentIndex + 1];
            }
            else
            {
                _nextLeftTarget = null;
            }
        }

        if (newTarget.prevTarget != null)
        {
            _prevLeftTarget = newTarget.prevTarget;
        }
        else
        {
            var currentIndex = _currentGround.LowTargets.IndexOf(_currentLeftTarget);
            Debug.Log($"Index of current left target in ground low targets: {currentIndex}");
            if (currentIndex > 0)
            {
                _prevLeftTarget = _currentGround.LowTargets[currentIndex - 1];
            } else
            {
                _prevLeftTarget = null;
            }
        }
    }

}

