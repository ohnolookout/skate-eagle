using Com.LuisPedroFonseca.ProCamera2D;
using GooglePlayGames.BasicApi;
using System.Collections.Generic;
using Unity.Android.Types;
using Unity.Hierarchy;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class CameraManager : MonoBehaviour
{
    private CompoundTarget _lookaheadTarget = new();
    private CompoundTarget _playerTarget = new();
    //private LinkedCameraTarget _currentLookaheadLeftTarget;
    //private LinkedCameraTarget _prevLookaheadLeftTarget;
    //private LinkedCameraTarget _nextLookaheadLeftTarget;
    //private LinkedCameraTarget _currentPlayerLeftTarget;
    //private LinkedCameraTarget _prevPlayerLeftTarget;
    //private LinkedCameraTarget _nextPlayerLeftTarget;
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
    private bool _doLerpLastMaxY = false;
    private float _lastMaxY = float.PositiveInfinity;
    public const float minXOffset = 20;
    public const float maxXOffset = 110;
    private const float _minOffsetVel = 0;
    private const float _maxOffsetVel = 100;
    private float _xOffset;
    private const float _defaultXDampen = 0.35f;
    private float _xDampen;
    private float _xDampenOnDirectionChange = 0.005f;
    private const float _defaultYDampen = 0.2f;
    private float _yDampenOnDirectionChange = 0.2f;
    private float _yDampen;
    private const float _defaultZoomDampen = 0.2f;
    private float _zoomDampenOnDirectionChange = 0.04f;
    private float _zoomDampen;
    private float _aspectRatio;
    private float _xBufferT = 0.7f;
    private const float _xOffsetDampen = 0.05f;
    private float _targetXOffset;
    private const float _maxYDampen = 0.2f;

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
            if (_currentHighPoint.Next != null)
            {
                Gizmos.DrawSphere(_currentHighPoint.Next.position, 2f);
            }
        }

        Gizmos.color = Color.lightGreen;
        if(_lookaheadTarget.current != null)
        {
            Gizmos.DrawSphere(_lookaheadTarget.current.Position, 2f);
        }

        Gizmos.color = Color.darkOliveGreen;
        if(_lookaheadTarget.next != null)
        {
            Gizmos.DrawSphere(_lookaheadTarget.next.Position, 2f);
        }

        if(_player == null || _playerTransform == null)
        {
            return;
        }

        var lowerTargetDelta = new Vector3(0, 2);

        Gizmos.color = Color.lightPink;
        if (_lookaheadTarget.current != null)
        {
            Gizmos.DrawSphere(_playerTarget.current.Position - lowerTargetDelta, 2f);
        }

        Gizmos.color = Color.rebeccaPurple;
        if (_lookaheadTarget.next != null)
        {
            Gizmos.DrawSphere(_playerTarget.next.Position - lowerTargetDelta, 2f);
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
        var playerX = _playerTransform.position.x;
        var directionalXOffset = _player.FacingForward ? _xOffset : -_xOffset;
        var lookaheadX = playerX + directionalXOffset;
        var camX = playerX + (directionalXOffset / 2);

        UpdateTarget(lookaheadX, _lookaheadTarget);
        UpdateTarget(playerX, _playerTarget);
        UpdateCurrentHighPoint();

        var camParams = CameraTargetUtility.GetCamParams(lookaheadX, _lookaheadTarget.current);   
        camParams = ProcessMaxY(playerX, lookaheadX, camParams);

        var adjustedOrthoSize = CheckPlayerZoom(camParams.orthoSize, camParams.camBottomY);
        Vector3 centerPosition = new(camX, camParams.camBottomY + adjustedOrthoSize);

        _targetOrthoSize = adjustedOrthoSize;
        _targetPosition = centerPosition;
    }

    private (float orthoSize, float camBottomY) ProcessMaxY(float playerX, float lookaheadX, (float camBottomY, float orthoSize) camParams)
    {
        var playerMaxY = CameraTargetUtility.GetMaxCamY(playerX, lookaheadX, _playerTarget.current);

        if (playerMaxY < camParams.camBottomY)
        {
            _doLerpLastMaxY = true;
            _lastMaxY = playerMaxY;
            camParams.orthoSize += (camParams.camBottomY - playerMaxY) / 2;
            camParams.camBottomY = playerMaxY;
        }
        else if (_doLerpLastMaxY)
        {
            Debug.Log("Lerping from last maxY");
            _lastMaxY = Mathf.SmoothStep(_lastMaxY, camParams.camBottomY, _maxYDampen);

            if (camParams.camBottomY > _lastMaxY + 2)
            {
                camParams.orthoSize += (camParams.camBottomY - _lastMaxY) / 2;
                camParams.camBottomY = _lastMaxY;
            }
            else
            {
                _lastMaxY = float.PositiveInfinity;
                _doLerpLastMaxY = false;
            }
        }

        return camParams;
    }

    private float GetXOffset()
    {
        var velOffsetT = (_player.NormalBody.linearVelocity.magnitude - _minOffsetVel) / (_maxOffsetVel - _minOffsetVel);
        _targetXOffset = Mathf.Lerp(minXOffset, maxXOffset, velOffsetT);
        return Mathf.SmoothStep(_xOffset, _targetXOffset, _xOffsetDampen);
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


    private void UpdateTarget(float xPos, CompoundTarget target)
    {
#if UNITY_EDITOR
        int searchCount = 0;
#endif
        //Debug.Assert(_prevLeftTarget != null, "CameraManager: Prev left target is null.");
        while (target.prev != null && target.current.Position.x > xPos)
        {
#if UNITY_EDITOR
            searchCount++;
            if(searchCount > 1)
            {
                Debug.LogWarning("Cam target search iterations > 1: " + searchCount);
            } 
#endif
            AssignPrevAndNextTargets(target, target.prev, false);
        }

        //Debug.Assert(_nextLeftTarget != null, "CameraManager: Next left target is null.");

        while (target.next != null && xPos > target.next.Position.x)
        {
#if UNITY_EDITOR
            searchCount++;
            if (searchCount > 1)
            {
                Debug.LogWarning("Cam target search iterations > 1: " + searchCount);
            }
#endif
            AssignPrevAndNextTargets(target, target.next, true);
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
        while(_currentHighPoint.Previous != null && _playerTransform.position.x < _currentHighPoint.position.x)
        {
#if UNITY_EDITOR
            searchCount++;
            if (searchCount > 1)
            {
                Debug.LogWarning("Cam highpoint search iterations > 1: " + searchCount);
            }
#endif
            hasChanged = true;
            _currentHighPoint = _currentHighPoint.Previous;
        }

        while(_currentHighPoint.Next != null && _playerTransform.position.x > _currentHighPoint.Next.position.x)
        {
#if UNITY_EDITOR
            searchCount++;
            if (searchCount > 1)
            {
                Debug.LogWarning("Cam highpoint search iterations > 1: " + searchCount);
            }
#endif
            hasChanged = true;
            _currentHighPoint = _currentHighPoint.Next;
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

        if(_currentHighPoint.Next != null)
        {
            targetX = _currentHighPoint.position.x + ((_currentHighPoint.Next.position.x - _currentHighPoint.position.x) / 2);            
        }
        else
        {
            targetX = _currentHighPoint.position.x;
        }
        UpdateTarget(targetX, _lookaheadTarget);
        var camParams = CameraTargetUtility.GetCamParams(targetX,_lookaheadTarget.current);
        var maxCamY = CameraTargetUtility.GetMaxCamY(_playerTransform.position.x, targetX, _playerTarget.current);
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
        AssignPrevAndNextTargets(_lookaheadTarget, collidedSeg.FirstLeftTarget);
        AssignPrevAndNextTargets(_playerTarget, collidedSeg.FirstLeftTarget);
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
        _currentGround = level.SerializedStartLine.groundRef.Value;

        _lookaheadTarget.current = level.SerializedStartLine.FirstCameraTarget;
        _lookaheadTarget.prev = _lookaheadTarget.current.PrevTarget;
        _lookaheadTarget.next = _lookaheadTarget.current.NextTarget;

        _playerTarget.current = level.SerializedStartLine.FirstCameraTarget;
        _playerTarget.prev = _playerTarget.current.PrevTarget;
        _playerTarget.next = _playerTarget.current.NextTarget;

        _currentHighPoint = level.SerializedStartLine.FirstHighPoint;
        _doDirectionChangeDampen = false;
        _doCheckHighPointExit = false;
        _xDampen = _defaultXDampen;
        _yDampen = _defaultYDampen;
        _zoomDampen = _defaultZoomDampen;
        _xOffset = minXOffset;
        _doLerpLastMaxY = false;
        _lastMaxY = float.PositiveInfinity;
}

    private float CheckPlayerZoom(float targetOrthoSize, float camBottomY)
    {
        var playerY = _playerTransform.position.y;
        var yDist = playerY - camBottomY;
        var playerZoomSize = yDist / (1 + CameraTargetUtility.PlayerHighYT);

        return Mathf.Max(playerZoomSize, targetOrthoSize);
    }

    private void AssignPrevAndNextTargets(CompoundTarget currentTarget, LinkedCameraTarget newTarget, bool moveRight)
    {
        currentTarget.current = newTarget;

        if (moveRight)
        {
            currentTarget.prev = currentTarget.current;
            if (newTarget.NextTarget != null)
            {
                currentTarget.next = newTarget.NextTarget;
            }
            else
            {
                var currentIndex = _currentGround.LowTargets.IndexOf(currentTarget.current);
                if (currentIndex >= 0 && currentIndex < _currentGround.LowTargets.Count - 1)
                {
                    currentTarget.next = _currentGround.LowTargets[currentIndex + 1];
                }
                else
                {
                    currentTarget.next = null;
                }
            }
        }
        else
        {
            currentTarget.next = currentTarget.current;

            if (newTarget.PrevTarget != null)
            {
                currentTarget.prev = newTarget.PrevTarget;
            }
            else
            {
                var currentIndex = _currentGround.LowTargets.IndexOf(currentTarget.current);
                if (currentIndex > 0)
                {
                    currentTarget.prev = _currentGround.LowTargets[currentIndex - 1];
                }
                else
                {
                    currentTarget.prev = null;
                }
            }
        }
    }

    private void AssignPrevAndNextTargets(CompoundTarget currentTarget, LinkedCameraTarget newTarget)
    {
        currentTarget.current = newTarget;

        if (newTarget.NextTarget != null)
        {
            currentTarget.next = newTarget.NextTarget;
        }
        else
        {
            var currentIndex = _currentGround.LowTargets.IndexOf(currentTarget.current);
            if (currentIndex < _currentGround.LowTargets.Count - 1)
            {
                currentTarget.next = _currentGround.LowTargets[currentIndex + 1];
            }
            else
            {
                currentTarget.next = null;
            }
        }

        if (newTarget.PrevTarget != null)
        {
            currentTarget.prev = newTarget.PrevTarget;
        }
        else
        {
            var currentIndex = _currentGround.LowTargets.IndexOf(currentTarget.current);
            if (currentIndex > 0)
            {
                currentTarget.prev = _currentGround.LowTargets[currentIndex - 1];
            } else
            {
                currentTarget.prev = null;
            }
        }
    }

}

public class CompoundTarget
{
    public LinkedCameraTarget current;
    public LinkedCameraTarget prev;
    public LinkedCameraTarget next;
}