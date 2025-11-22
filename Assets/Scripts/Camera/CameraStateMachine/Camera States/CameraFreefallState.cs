using GooglePlayGames.BasicApi;
using UnityEngine;

public class CameraFreefallState : CameraState
{
    private bool _isGoingUp = false;
    public CameraFreefallState(CameraStateMachine cameraMachine) : base(cameraMachine)
    {
    }

    public override void EnterState()
    {
        Debug.Log("Entering freefall state.");
        _player.EventAnnouncer.SubscribeToEvent(PlayerEvent.Land, OnPlayerLand);
        _isGoingUp = _player.NormalBody.linearVelocityY > 0;
        _cameraManager.Targeter.SetYOffsetToPlayer(_player.Transform.position.y);
    }

    public override void ExitState()
    {
        _player.EventAnnouncer.UnsubscribeFromEvent(PlayerEvent.Land, OnPlayerLand);
    }

    public override void FixedUpdateState()
    {
        if (_isGoingUp)
        {
            _isGoingUp = _player.NormalBody.linearVelocityY > 0;
        }

        _cameraManager.Targeter.UpdateTargetsFreefall(_isGoingUp, _player.Airborne);
        _cameraManager.Mover.UpdatePosition(_cameraManager.Targeter);
    }

    private void OnPlayerLand(IPlayer player)
    {
        var collision = player.LastLandCollision;
        var collidedSeg = collision.transform.parent.GetComponent<GroundSegment>();

        if (collidedSeg == null)
        {
            return;
        }

        _cameraManager.StartContinuityTimer();
        _cameraManager.currentGround = collidedSeg.parentGround;
        _cameraManager.Targeter.OnEnterNewGround(collidedSeg, false);

        if (!_cameraManager.Targeter.PlayerTracker.IsOverExtended)
        {
            ChangeState(_cameraMachine.Factory.GetState(CameraStateType.TrackGround));
        }
    }
}