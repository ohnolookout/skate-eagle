using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic;

public class CameraTargeter
{
    #region Declarations
    // Partner classes
    private CameraManager _cameraManager;
    private LinkedTargetTracker _lookaheadTracker = new();
    private LinkedTargetTracker _playerTracker = new();
    private HighPointTracker _highPointTracker = new();
    private RunningAverager _xVelAvg = new(RunningAvgCount);
    private RunningAverager _yVelAvg = new(RunningAvgCount/2);

    //Outputs
    private Vector3 _targetPosition;
    private float _targetOrthoSize;

    //Private vars
    private float _xOffset;
    private float _yOffset;
    private bool _doLerpLastMaxY = false;
    private float _lastMaxY = float.PositiveInfinity;
    private float _dirChangeXOffsetTaper = 1;
    private float _dirChangeXOffsetTaperSpeed = .1f;
    private float _velOffsetT = 0;
    private float _targetXOffset;
    private float _targetYOffset;
    private float _playerX;
    private float _lookaheadX;
    private float _camX;

    //Constants
    public const float MinXOffset = 20;
    private const float MaxXOffset = 110;
    private const float MinXOffsetVel = 20;
    private const float MaxXOffsetVel = 200;
    private const float MaxXOffsetVelTStep = 0.1f;
    private const float XOffsetDampen = 0.1f;
    private const float MinYOffsetT = 0f;
    private const float MaxYOffsetT = 0.2f;
    private const float MinYOffsetVel = 0;
    private const float MaxYOffsetVel = 80;
    private const float YOffsetDampen = 0.1f;
    private const int RunningAvgCount = 30;
    //Getters/setters
    public LinkedTargetTracker LookaheadTracker => _lookaheadTracker;
    public LinkedTargetTracker PlayerTracker => _playerTracker;
    public HighPointTracker HighPointTracker => _highPointTracker;
    public Vector3 TargetPosition => _targetPosition;
    public float TargetOrthoSize => _targetOrthoSize;
    public float XOffset => _xOffset;
    public float YOffset => _yOffset;
    #endregion

    #region Constructor
    public CameraTargeter(CameraManager cameraManager)
    {
        _cameraManager = cameraManager;
        _xOffset = MinXOffset;
    }
    #endregion

    #region Updating Targets
    public void UpdateTargetsGroundTracking()
    {
        if (_cameraManager.Player == null)
        {
            return;
        }

        _xVelAvg.AddSpeed(_cameraManager.Player.NormalBody.linearVelocityX);
        _yVelAvg.AddSpeed(_cameraManager.Player.NormalBody.linearVelocityY);

        UpdateXValues();

        _lookaheadTracker.Update(_lookaheadX);
        _playerTracker.Update(_playerX); 
        _highPointTracker.Update(_cameraManager.PlayerTransform.position.x);

        var camParams = CameraTargetUtility.GetCamParams(_lookaheadX, _lookaheadTracker.Current);
        camParams = ProcessMaxY(_lookaheadX, camParams);

        camParams = CheckPlayerZoom(camParams);
        Vector3 centerPosition = new(_camX, camParams.camBottomY + camParams.orthoSize);

        _targetPosition = centerPosition;
        _targetOrthoSize = camParams.orthoSize;
    }

    public bool UpdateTargetsDirectionChange()
    {
        _xVelAvg.AddSpeed(_cameraManager.Player.NormalBody.linearVelocityX);
        _yVelAvg.AddSpeed(_cameraManager.Player.NormalBody.linearVelocityY);
        var newParams = CheckPlayerZoom(_targetPosition.y - TargetOrthoSize, TargetOrthoSize);
        Vector3 centerPosition = new(_targetPosition.x, newParams.camBottomY + newParams.orthoSize);
        return _highPointTracker.Update(_cameraManager.PlayerTransform.position.x);
    }

    public void UpdateTargetsFreefall(bool goingUp, bool airborne)
    {
        _xVelAvg.AddSpeed(_cameraManager.Player.NormalBody.linearVelocityX);
        _yVelAvg.AddSpeed(_cameraManager.Player.NormalBody.linearVelocityY);
        UpdateXValues();
        _yOffset = GetYOffset(goingUp, airborne);
        _targetPosition = new(_playerX + (_xOffset/2), _cameraManager.PlayerTransform.position.y + _yOffset);
    }

    public void UpdateLookaheadTarget(float targetX)
    {
        _lookaheadX = targetX;
        _lookaheadTracker.Update(_lookaheadX);
    }

    public void UpdateParams()
    {
        var camParams = CameraTargetUtility.GetCamParams(_lookaheadX, _lookaheadTracker.Current);
        camParams = ProcessMaxY(_lookaheadX, camParams);

        camParams = CheckPlayerZoom(camParams);
        Vector3 centerPosition = new(_camX, camParams.camBottomY + camParams.orthoSize);

        _targetPosition = centerPosition;
        _targetOrthoSize = camParams.orthoSize;
    }

    public void SetCamX(float camX)
    {
        _camX = camX;
    }

    public void SetYOffset(float yOffset)
    {
        _yOffset = yOffset;
    }

    public void SetYOffsetToPlayer(float playerY)
    {
        _yOffset = _targetPosition.y - playerY;
    }

    #endregion

    #region Update Calculations
    public void UpdateXValues()
    {
        _xOffset = GetXOffset();
        _playerX = _cameraManager.PlayerTransform.position.x;
        _lookaheadX = _playerX + _xOffset;
        _camX = _playerX + (_xOffset / 2);
    }

    private float GetXOffset()
    {
        var directionalCoefficient = _cameraManager.Player.FacingForward ? 1 : -1;
        var xVel = _xVelAvg.RunningAverage;
        var velOffsetT = (Mathf.Abs(xVel) - MinXOffsetVel) / (MaxXOffsetVel - MinXOffsetVel);

        _targetXOffset = Mathf.SmoothStep(MinXOffset, MaxXOffset, velOffsetT) * directionalCoefficient;

        if (_dirChangeXOffsetTaper < 1)
        {
            _dirChangeXOffsetTaper = Mathf.Clamp01(_dirChangeXOffsetTaper + (_dirChangeXOffsetTaperSpeed * (.25f + velOffsetT)));
            return Mathf.SmoothStep(_xOffset, _targetXOffset, _dirChangeXOffsetTaper);
        }

        return _targetXOffset;

    }

    private float GetYOffset(bool goingUp, bool airborne)
    {
        var adjustedVelocity = _yVelAvg.RunningAverage;
        if (goingUp)
        {
            adjustedVelocity /= 3;
        } else
        {
            if (adjustedVelocity > 0) return Mathf.SmoothStep(_yOffset, _targetYOffset, YOffsetDampen);

            adjustedVelocity *= 2;
        }

        var minYOffset = MinYOffsetT * _cameraManager.Camera.orthographicSize * 2;
        var maxYOffset = MaxYOffsetT * _cameraManager.Camera.orthographicSize * 2;
        var directionalCoefficient = goingUp ? 1 : -1;
        var velOffsetT = (Mathf.Abs(adjustedVelocity) - MinYOffsetVel) / (MaxYOffsetVel - MinYOffsetVel);

        _targetYOffset = Mathf.Lerp(minYOffset, maxYOffset, velOffsetT) * directionalCoefficient;

        if (!airborne)
        {
            _targetYOffset *= -1;
        }
        return Mathf.SmoothStep(_yOffset, _targetYOffset, YOffsetDampen);
    }

    private (float orthoSize, float camBottomY) ProcessMaxY(float lookaheadX, (float camBottomY, float orthoSize) camParams)
    {
        var playerMaxY = CameraTargetUtility.GetMaxCamY(_cameraManager.PlayerTransform.position, lookaheadX, _playerTracker.Current);
        if (playerMaxY < camParams.camBottomY)
        {
            _doLerpLastMaxY = true;
            _lastMaxY = playerMaxY;
            camParams.camBottomY = playerMaxY;
        }
        else if (_doLerpLastMaxY)
        {
            _lastMaxY = Mathf.SmoothStep(_lastMaxY, camParams.camBottomY, CameraMover.MaxYDampen);

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

    private (float camBottomY, float orthoSize) CheckPlayerZoom((float camBottomY, float orthoSize) camParams)
    {
        return CheckPlayerZoom(camParams.camBottomY, camParams.orthoSize);
    }
    private (float camBottomY, float orthoSize) CheckPlayerZoom(float camBottomY, float orthoSize)
    {
        var playerY = _cameraManager.PlayerTransform.position.y;
        var yDist = playerY - camBottomY;
        var playerZoomSize = yDist / (1 + CameraTargetUtility.PlayerHighYT);

        orthoSize = Mathf.Max(playerZoomSize, orthoSize);

        return (camBottomY, orthoSize);
    }

    #endregion

    #region Events

    public void ResetTrackers(SerializedStartLine startLine)
    {
        _xOffset = MinXOffset;
        _dirChangeXOffsetTaper = 1;
        _dirChangeXOffsetTaperSpeed = .1f;
        _velOffsetT = 0;
        _doLerpLastMaxY = false;
        _lastMaxY = float.PositiveInfinity;
        _lookaheadTracker.SetStartingTarget(startLine);
        _playerTracker.SetStartingTarget(startLine);
        _highPointTracker.SetHighPoint(startLine.FirstHighPoint);
        _xVelAvg = new(RunningAvgCount);
        _yVelAvg = new(RunningAvgCount / 2);
        SetTargets(startLine.CamStartPosition, startLine.CamOrthoSize);
    }

    public void SetTargets(Vector3 position, float orthoSize)
    {
        _targetPosition = position;
        _targetOrthoSize = orthoSize;
    }

    public void OnEnterNewGround(GroundSegment collidedSegment, bool doContinuity)
    {
        _lookaheadTracker.EnterGround(collidedSegment, doContinuity, _cameraManager.Player.FacingForward);
        _playerTracker.EnterGround(collidedSegment, doContinuity, _cameraManager.Player.FacingForward);
        _highPointTracker.SetHighPoint(collidedSegment.StartHighPoint);
        UpdateTargetsGroundTracking();
    }
 
    public void OnExitDirectionChange()
    {
        _dirChangeXOffsetTaper = 1;
    }

    #endregion

    
}
