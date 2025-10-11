using Com.LuisPedroFonseca.ProCamera2D;
using System.Collections.Generic;
using Unity.Hierarchy;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CameraManager : MonoBehaviour
{
    private LinkedCameraTarget _currentLeftTarget;
    private Camera _camera;
    public bool doLogPosition = false;
    private bool _doUpdate = true;
    private IPlayer _player;
    private Transform _playerTransform;
    private bool _doPlayerZoom = false;
    private Ground _currentGround;
    private float _xOffset = CameraTargetUtility.DefaultPlayerXOffset + CameraTargetUtility.DefaultTargetXOffset;

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

    }

    void Update()
    {
        if (_doUpdate)
        {
            UpdateCameraPos();
        }

    }

    private void OnDrawGizmosSelected()
    {

    }

    private void AddPlayer(IPlayer player)
    {
        _player = player;
        _playerTransform = player.Transform;
        //_player.EventAnnouncer.SubscribeToEvent(PlayerEvent.SwitchDirection, OnSwitchPlayerDirection);
        _player.EventAnnouncer.SubscribeToAddCollision(OnPlayerCollide);
        UpdateCameraPos();
    }

    private void UpdateCameraPos()
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

        _camera.orthographicSize = adjustedOrthoSize;
        _camera.transform.position = new(camX, camParams.camBottomY + adjustedOrthoSize);
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

    }

    private float CheckPlayerZoom(float targetOrthoSize, float camBottomY)
    {
        var playerY = _playerTransform.position.y;
        var yDist = playerY - camBottomY;
        var playerZoomSize = yDist / (1 + CameraTargetUtility.PlayerHighYT);

        if (playerZoomSize > targetOrthoSize)
        {
            Debug.Log("Player Y: " + playerY + " Cam bottom Y: " + camBottomY + " Cam top y: " + (camBottomY + playerZoomSize * 2));
        }
        return Mathf.Max(playerZoomSize, targetOrthoSize);
    }

}

