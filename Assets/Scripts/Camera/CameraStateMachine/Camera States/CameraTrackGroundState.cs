using GooglePlayGames.BasicApi;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CameraTrackGroundState : CameraState
{
    public CameraTrackGroundState(CameraStateMachine cameraMachine) : base(cameraMachine)
    {
    }

    public override void EnterState()
    {
        Debug.Log("Entering track ground state.");
        _player.EventAnnouncer.SubscribeToEvent(PlayerEvent.SwitchDirection, OnSwitchPlayerDirection);
        _player.EventAnnouncer.SubscribeToEvent(PlayerEvent.Land, OnPlayerLand);
    }

    public override void ExitState()
    {
        _player.EventAnnouncer.UnsubscribeFromEvent(PlayerEvent.SwitchDirection, OnSwitchPlayerDirection);
        _player.EventAnnouncer.UnsubscribeFromEvent(PlayerEvent.Land, OnPlayerLand);
    }

    public override void FixedUpdateState()
    {
        //Debug.Log($"Player airborne: {_player.Airborne}");
        if (_player.Airborne)
        {
            if (_cameraManager.Targeter.PlayerTracker.IsOverExtended)
            {
                ChangeState(_cameraMachine.Factory.GetState(CameraStateType.Freefall));
            }
        }

        _cameraManager.Targeter.UpdateTargetsGroundTracking();
        _cameraManager.Mover.UpdatePosition(_cameraManager.Targeter);
    }

    private void OnSwitchPlayerDirection(IPlayer _)
    {
        if (_cameraManager.ContinuityTimer > 0)
        {
            return;
        }

        ChangeState(_cameraMachine.Factory.GetState(CameraStateType.ChangeDirection));
    }

    private void OnPlayerLand(IPlayer player)
    {
        var collision = player.LastLandCollision;
        var collidedTransformParent = collision.transform.parent;

        if (_cameraManager.currentGround != null && collidedTransformParent == _cameraManager.currentGround.transform)
        {
            return;
        }

        var collidedSeg = collidedTransformParent.GetComponent<GroundSegment>();
        _cameraManager.currentGround = collidedSeg.parentGround;
        _cameraManager.Targeter.OnEnterNewGround(collidedSeg, true);
    }
            
}