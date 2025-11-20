using GooglePlayGames.BasicApi;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CameraTrackGroundState : CameraState
{
    private IPlayer _player;
    private bool _checkForFreeFall = false;
    public CameraTrackGroundState(CameraStateMachine cameraMachine) : base(cameraMachine)
    {
    }

    public override void EnterState()
    {
        _checkForFreeFall = false;
        _player = _cameraManager.player;
        _player.EventAnnouncer.SubscribeToEvent(PlayerEvent.SwitchDirection, OnSwitchPlayerDirection);
        _player.EventAnnouncer.SubscribeToEvent(PlayerEvent.Land, OnPlayerLand);
        _player.EventAnnouncer.SubscribeToEvent(PlayerEvent.Airborne, OnPlayerAirborne);
        _player.EventAnnouncer.SubscribeToEvent(PlayerEvent.Fall, OnFall);
    }

    public override void ExitState()
    {
        _checkForFreeFall = false;
        _player.EventAnnouncer.UnsubscribeFromEvent(PlayerEvent.SwitchDirection, OnSwitchPlayerDirection);
        _player.EventAnnouncer.UnsubscribeFromEvent(PlayerEvent.Land, OnPlayerLand);
        _player.EventAnnouncer.UnsubscribeFromEvent(PlayerEvent.Airborne, OnPlayerAirborne);
        _player.EventAnnouncer.UnsubscribeFromEvent(PlayerEvent.Fall, OnFall);
    }

    public override void FixedUpdateState()
    {
        if (_checkForFreeFall)
        {
            if (_cameraManager.Targeter.PlayerTracker.IsOverExtended)
            {
                ChangeState(_cameraMachine.Factory.GetState(CameraStateType.Freefall));
            }
        }

        if(_cameraManager.dirChangeTimer > 0)
        {
            _cameraManager.dirChangeTimer -= Time.fixedDeltaTime;
            if (_cameraManager.dirChangeTimer < 0) _cameraManager.dirChangeTimer = 0;
        }

        _cameraManager.Targeter.UpdateTargets();
        _cameraManager.Mover.UpdatePosition(_cameraManager.Targeter);
    }

    private void OnSwitchPlayerDirection(IPlayer _)
    {
        if (_cameraManager.dirChangeTimer > 0) return;

        ChangeState(_cameraMachine.Factory.GetState(CameraStateType.ChangeDirection));
    }

    private void OnPlayerLand(IPlayer player)
    {
        _checkForFreeFall = false;
        var collision = player.LastLandCollision;
        var collidedTransformParent = collision.transform.parent;

        if (_cameraManager.currentGround != null && collidedTransformParent == _cameraManager.currentGround.transform)
        {
            return;
        }

        var collidedSeg = collidedTransformParent.GetComponent<GroundSegment>();
        _cameraManager.currentGround = collidedSeg.parentGround;
        _cameraManager.Targeter.OnEnterNewGround(collidedSeg);
    }

    private void OnPlayerAirborne(IPlayer _)
    {
        _checkForFreeFall = true;
    }

    private void OnFall(IPlayer _)
    {
        ChangeState(_cameraMachine.Factory.GetState(CameraStateType.Standby));
    }
    
}