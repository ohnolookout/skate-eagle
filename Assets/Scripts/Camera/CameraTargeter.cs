using GooglePlayGames.BasicApi;
using UnityEditor.Build;
using UnityEngine;

public class CameraTargeter
{
    private LinkedTargetTracker _lookaheadTracker = new();
    private LinkedTargetTracker _playerTracker = new();
    private HighPointTracker _highPointTracker = new();
    private Vector3 _targetPosition;
    private float _targetOrthoSize;
    private float _xOffset;
    private CameraManager _cameraManager;
    public bool doLerpLastMaxY = false;
    public float lastMaxY = float.PositiveInfinity;
    public const float minXOffset = 20;
    public const float maxXOffset = 110;
    public const float minXOffsetVel = 0;
    public const float maxXOffsetVel = 100;
    public float dirChangeXOffsetTaper = 1;
    public float dirChangeXOffsetTaperSpeed = .1f;
    public float xOffset;
    public float aspectRatio;
    public float xBufferT = 0.7f;
    public const float xOffsetDampen = 0.05f;
    public float targetXOffset;
    private float _playerX;
    private float _lookaheadX;
    private float _camX;
    public LinkedTargetTracker LookaheadTracker => _lookaheadTracker;
    public LinkedTargetTracker PlayerTracker => _playerTracker;
    public HighPointTracker HighPointTracker => _highPointTracker;
    public Vector3 TargetPosition => _targetPosition;
    public float TargetOrthoSize => _targetOrthoSize;


    public CameraTargeter(CameraManager cameraManager)
    {
        _cameraManager = cameraManager;
        xOffset = minXOffset;
    }
    public void UpdateTargets()
    {
        if (_cameraManager.Player == null)
        {
            return;
        }

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

    public void UpdateLookaheadTarget(float targetX)
    {
        _lookaheadX = targetX;
        _lookaheadTracker.Update(_lookaheadX);
    }

    public void SetCamX(float camX)
    {
        _camX = camX;
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

    private (float orthoSize, float camBottomY) ProcessMaxY(float lookaheadX, (float camBottomY, float orthoSize) camParams)
    {
        var playerMaxY = CameraTargetUtility.GetMaxCamY(_cameraManager.PlayerTransform.position, lookaheadX, _playerTracker.Current);
        if (playerMaxY < camParams.camBottomY)
        {
            doLerpLastMaxY = true;
            lastMaxY = playerMaxY;
            //camParams.orthoSize += (camParams.camBottomY - playerMaxY) / 2;
            camParams.camBottomY = playerMaxY;
        }
        else if (doLerpLastMaxY)
        {
            lastMaxY = Mathf.SmoothStep(lastMaxY, camParams.camBottomY, CameraMover.maxYDampen);

            if (camParams.camBottomY > lastMaxY + 2)
            {
                camParams.orthoSize += (camParams.camBottomY - lastMaxY) / 2;
                camParams.camBottomY = lastMaxY;
            }
            else
            {
                lastMaxY = float.PositiveInfinity;
                doLerpLastMaxY = false;
            }
        }

        return camParams;
    }

    private float GetXOffset()
    {
        var directionalCoefficient = _cameraManager.Player.FacingForward ? 1 : -1;
        var velOffsetT = (Mathf.Abs(_cameraManager.Player.NormalBody.linearVelocity.x) - minXOffsetVel) / (maxXOffsetVel - minXOffsetVel);
        targetXOffset = Mathf.Lerp(minXOffset, maxXOffset, velOffsetT) * directionalCoefficient;

        if (dirChangeXOffsetTaper < 1)
        {
            dirChangeXOffsetTaper = Mathf.Clamp01(dirChangeXOffsetTaper + (dirChangeXOffsetTaperSpeed * (.25f + velOffsetT)));

            return Mathf.SmoothStep(xOffset, targetXOffset, dirChangeXOffsetTaper);
        }

        return Mathf.SmoothStep(xOffset, targetXOffset, xOffsetDampen);

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

    public void UpdateXValues()
    {

        _xOffset = GetXOffset();
        _playerX = _cameraManager.PlayerTransform.position.x;
        _lookaheadX = _playerX + _xOffset;
        _camX = _playerX + (_xOffset / 2);
    }

    public bool UpdateTargetsDuringDirectionChange()
    {
        var newParams = CheckPlayerZoom(_targetPosition.y - TargetOrthoSize, TargetOrthoSize);
        Vector3 centerPosition = new(_targetPosition.x, newParams.camBottomY + newParams.orthoSize);
        return _highPointTracker.Update(_cameraManager.PlayerTransform.position.x);
    }

    public void OnEnterNewGround(GroundSegment collidedSegment)
    {
        _lookaheadTracker.EnterGround(collidedSegment, true, _cameraManager.Player.FacingForward);
        _playerTracker.EnterGround(collidedSegment, true, _cameraManager.Player.FacingForward);
        _highPointTracker.SetHighPoint(collidedSegment.StartHighPoint);
        UpdateTargets();
    }

    public void ResetTrackers(SerializedStartLine startLine)
    {
        xOffset = minXOffset;
        dirChangeXOffsetTaper = 1;
        _lookaheadTracker.SetStartingTarget(startLine);
        _playerTracker.SetStartingTarget(startLine);
        _highPointTracker.SetHighPoint(startLine.FirstHighPoint);
    }
 
    public void OnExitDirectionChange()
    {
        dirChangeXOffsetTaper = 1;
    }
}
