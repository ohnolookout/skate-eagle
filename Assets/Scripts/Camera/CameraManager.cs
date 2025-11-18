using UnityEngine;

public class CameraManager : MonoBehaviour
{
    private CompoundTarget _lookaheadTarget = new();
    private CompoundTarget _playerTarget = new();
    private LinkedHighPoint _currentHighPoint;
    private (float camBottomY, float orthoSize) _lastCamParams;
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
    //private float _groundChangeTimer = 0;
    //private const float _groundChangeTimeLimit = 1;
    private float _lastMaxY = float.PositiveInfinity;
    public const float minXOffset = 20;
    public const float maxXOffset = 110;
    private const float _minXOffsetVel = 0;
    private const float _maxXOffsetVel = 100;
    private float _dirChangeXOffsetTaper = 1;
    private float _dirChangeXOffsetTaperSpeed = .1f;
    private float _xOffset;
    private const float _defaultXDampen = 0.35f;
    private float _xDampen;
    private const float _defaultYDampen = 0.2f;
    private float _yDampen;
    private const float _defaultZoomDampen = 0.2f;
    private float _zoomDampen;
    private float _aspectRatio;
    private float _xBufferT = 0.7f;
    private const float _xOffsetDampen = 0.05f;
    private float _targetXOffset;
    private const float _maxYDampen = 0.2f;
    private const float _maxXDeltaPercent = .2f;
    private const float _maxYDeltaPercent = .15f;
    private const float _maxOrthoDeltaPercent = .04f;
    private float _lastXDelta = 0;
    private float _lastYDelta = 0;
    private float _lastOrthoDelta = 0;
    private float _maxXAccel = .3f;
    private float _maxYAccel = .3f;
    private float _maxOrthoAccel = 0.05f;

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

        //if(_groundChangeTimer > 0)
        //{
        //    _groundChangeTimer += Time.fixedDeltaTime;

        //    if(_groundChangeTimer > _groundChangeTimeLimit)
        //    {
        //        _groundChangeTimer = 0;
        //    }    
        //}

        if (!_doCheckHighPointExit)
        {
            UpdateTargetPos();
        }
        else
        {
            UpdateTargetDuringDirectionChange();
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
        var lookaheadX = playerX + _xOffset;
        var camX = playerX + (_xOffset / 2);

        UpdateTarget(lookaheadX, _lookaheadTarget);
        UpdateTarget(playerX, _playerTarget);
        UpdateCurrentHighPoint();

        var camParams = CameraTargetUtility.GetCamParams(lookaheadX, _lookaheadTarget.current);
        camParams = ProcessMaxY(lookaheadX, camParams);

        camParams = CheckPlayerZoom(camParams);
        Vector3 centerPosition = new(camX, camParams.camBottomY + camParams.orthoSize);

        _targetOrthoSize = camParams.orthoSize;
        _lastCamParams = camParams;
        _targetPosition = centerPosition;
    }

    private (float orthoSize, float camBottomY) ProcessMaxY(float lookaheadX, (float camBottomY, float orthoSize) camParams)
    {
        var playerMaxY = CameraTargetUtility.GetMaxCamY(_playerTransform.position, lookaheadX, _playerTarget.current);
        if (playerMaxY < camParams.camBottomY)
        {
            _doLerpLastMaxY = true;
            _lastMaxY = playerMaxY;
            //camParams.orthoSize += (camParams.camBottomY - playerMaxY) / 2;
            camParams.camBottomY = playerMaxY;
        }
        else if (_doLerpLastMaxY)
        {
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
        var directionalCoefficient = _player.FacingForward ? 1 : -1;
        var velOffsetT = (Mathf.Abs(_player.NormalBody.linearVelocity.x) - _minXOffsetVel) / (_maxXOffsetVel - _minXOffsetVel);
        _targetXOffset = Mathf.Lerp(minXOffset, maxXOffset, velOffsetT) * directionalCoefficient;

        if (_dirChangeXOffsetTaper < 1)
        {
            _dirChangeXOffsetTaper = Mathf.Clamp01(_dirChangeXOffsetTaper + (_dirChangeXOffsetTaperSpeed * (.25f + velOffsetT)));

            return Mathf.SmoothStep(_xOffset, _targetXOffset, _dirChangeXOffsetTaper);
        }

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
        var playerViewportPoint = _camera.WorldToViewportPoint(_player.NormalBody.position);

        //Find y delta
        var maxYDelta = _maxYDeltaPercent * _camera.orthographicSize;
        var newY = Mathf.SmoothStep(camPos.y, _targetPosition.y, _yDampen);
        var yDelta = FindDelta(camPos.y, newY, _targetPosition.y - camPos.y, _lastYDelta, maxYDelta, _maxYAccel);

        if(playerViewportPoint.y < .05f && yDelta < 0)
        {
            Debug.Log("Accelerating y delta due to low player screen position.");
            yDelta *= 1.25f;
        }  

        var maxXDelta = _maxXDeltaPercent * _camera.orthographicSize;
        var newX = Mathf.SmoothStep(camPos.x, _targetPosition.x, _xDampen);
        var xDelta = FindDelta(camPos.x, newX, _targetPosition.x - camPos.x, _lastXDelta, maxXDelta, _maxXAccel);

        if (playerViewportPoint.x < .1f && xDelta < 0)
        {
            Debug.Log("Accelerating x delta due to left player screen position.");
            xDelta *= 1.25f;
        }
        else if (playerViewportPoint.x > .9f && xDelta > 0)
        {
            xDelta *= 1.25f;
            Debug.Log("Accelerating x delta due to right player screen position.");
        }

        var maxOrthoDelta = _maxOrthoDeltaPercent * _camera.orthographicSize;
        var newOrthoSize = Mathf.SmoothStep(_camera.orthographicSize, _targetOrthoSize, _zoomDampen);
        var orthoDelta = FindDelta(_camera.orthographicSize, newOrthoSize, _targetOrthoSize - _camera.orthographicSize, _lastOrthoDelta, maxOrthoDelta, _maxOrthoAccel);


        _camera.transform.Translate(xDelta, yDelta, 0);
        _camera.orthographicSize += orthoDelta;

        _lastXDelta = xDelta;
        _lastYDelta = yDelta;
        _lastOrthoDelta = orthoDelta;

    }

    private float FindDelta(float currentVal, float targetVal, float totalDifference, float lastDelta, float maxDelta, float maxAccel, float accelModifier = 0)
    {

        var desiredDelta = targetVal - currentVal;
        var delta = desiredDelta;
        var desiredDeltaSign = Mathf.Sign(desiredDelta);

        if (Mathf.Abs(desiredDelta) > maxDelta)
        {
            delta = maxDelta * desiredDeltaSign;
        }

        var deltaAccel = delta - lastDelta;
        maxAccel += maxAccel * accelModifier;

        //Slow down as target is approached.
        if (lastDelta > maxDelta/3 && totalDifference / lastDelta < 6)
        {
            Debug.Log("Slowing down as target approached.");
            delta = lastDelta * 0.75f;
        } else if (Mathf.Abs(deltaAccel) > maxAccel)
        {
            delta = lastDelta + (maxAccel * Mathf.Sign(deltaAccel));
        }

        //Reduce overshoot
        if(Mathf.Sign(delta) != desiredDeltaSign)
        {
            delta *= 0.5f;
        };

        return delta;
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

    #region Direction Change Handling
    private void UpdateTargetDuringDirectionChange()
    {
        var newParams = CheckPlayerZoom(_lastCamParams);
        Vector3 centerPosition = new(_targetPosition.x, newParams.camBottomY + newParams.orthoSize);

        _targetOrthoSize = newParams.orthoSize;
        _targetPosition = centerPosition;
        _lastCamParams = newParams;

    }


    private void OnSwitchPlayerDirection(IPlayer player)
    {
        if (_doCheckHighPointExit)
        {
            _dirChangeXOffsetTaper = 0;
            return;
        }

        //if (_groundChangeTimer > 0)
        //{
        //    _dirChangeXOffsetTaper = 0;
        //    return;
        //}


        UpdateCurrentHighPoint();

        float targetX;


        if (_currentHighPoint.Next != null)
        {
            targetX = _currentHighPoint.position.x + ((_currentHighPoint.Next.position.x - _currentHighPoint.position.x) / 2);
        }
        else
        {
            targetX = _currentHighPoint.position.x;
        }
        UpdateTarget(targetX, _lookaheadTarget);
        var camParams = CameraTargetUtility.GetCamParams(targetX,_lookaheadTarget.current);
        camParams = ProcessMaxY(targetX, camParams);
        _targetPosition = new Vector3(targetX, camParams.camBottomY + camParams.orthoSize);
        _targetOrthoSize = camParams.orthoSize;
        _lastCamParams = camParams;

        _doCheckHighPointExit = true;

        var leftHighPointCameraPos = _camera.WorldToViewportPoint(_currentHighPoint.position);
        var leftHighPointInCamera = PointIsInCamera(leftHighPointCameraPos);
        var rightHighPointInCamera = true;
        var nextLowPointInCamera = true;

        if (leftHighPointInCamera)
        {
            rightHighPointInCamera = true;
            if(_currentHighPoint.Next != null)
            {
                var rightHighPointCameraPos = _camera.WorldToViewportPoint(_currentHighPoint.Next.position);
                rightHighPointInCamera = PointIsInCamera(leftHighPointCameraPos);
            }

            if (rightHighPointInCamera)
            {
                Vector3 nextLowPoint;
                if (_player.FacingForward)
                {
                    nextLowPoint = _playerTarget.next.Position;
                }
                else
                {
                    nextLowPoint = _playerTarget.current.Position;
                }

                var nextLowPointCameraPos = _camera.WorldToViewportPoint(nextLowPoint);
                nextLowPointInCamera = PointIsInCamera(nextLowPointCameraPos);
            }

        }

        if (leftHighPointInCamera && rightHighPointInCamera && nextLowPointInCamera)
        {
            _xDampen = _defaultXDampen / 6;
            _yDampen = _defaultYDampen / 6;
            _zoomDampen = _defaultZoomDampen / 4;
        }
        else
        {
            _xDampen = _defaultXDampen / 4;
        }
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
        _dirChangeXOffsetTaper = 0;
    }
    #endregion

    private bool PointIsInCamera(Vector3 viewportPoint)
    {
        return viewportPoint.x >= 0.02f
            && viewportPoint.x <= 0.98f
            && viewportPoint.y >= 0.03f
            && viewportPoint.y <= 0.9f;
    }
    private void OnPlayerLand(IPlayer _)
    {
        var collision = _player.LastLandCollision;

        var collidedTransformParent = collision.transform.parent;

        if (_currentGround != null && collidedTransformParent.parent == _currentGround.transform)
        {
            return;
        }

        Debug.Log("Updating current ground on player land.");
        var playerPos = _player.NormalBody.position;

        var collidedSeg = collidedTransformParent.GetComponent<GroundSegment>();
        _currentGround = collidedSeg.parentGround;
        //_groundChangeTimer = .01f;

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
        
        if (level.SerializedStartLine.CurvePoint != null)
        {
            _currentGround = level.SerializedStartLine.CurvePoint.ParentGround;
        }

        _lookaheadTarget.current = level.SerializedStartLine.FirstCameraTarget;
        _lookaheadTarget.prev = _lookaheadTarget.current.PrevTarget;
        _lookaheadTarget.next = _lookaheadTarget.current.NextTarget;

        _playerTarget.current = level.SerializedStartLine.FirstCameraTarget;
        _playerTarget.prev = _playerTarget.current.PrevTarget;
        _playerTarget.next = _playerTarget.current.NextTarget;

        _currentHighPoint = level.SerializedStartLine.FirstHighPoint;
        _doDirectionChangeDampen = false;
        _doCheckHighPointExit = false;
        //_groundChangeTimer = 0;
        _xDampen = _defaultXDampen;
        _yDampen = _defaultYDampen;
        _zoomDampen = _defaultZoomDampen;
        _xOffset = minXOffset;
        _doLerpLastMaxY = false;
        _lastMaxY = float.PositiveInfinity;
        _lastXDelta = 0;
        _lastYDelta = 0;
        _lastOrthoDelta = 0;
        _dirChangeXOffsetTaper = 1;
    }

    private (float camBottomY, float orthoSize) CheckPlayerZoom((float camBottomY, float orthoSize) camParams)
    {
        var playerY = _playerTransform.position.y;
        var yDist = playerY - camParams.camBottomY;
        var playerZoomSize = yDist / (1 + CameraTargetUtility.PlayerHighYT);

        camParams.orthoSize = Mathf.Max(playerZoomSize, camParams.orthoSize);

        return camParams;
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