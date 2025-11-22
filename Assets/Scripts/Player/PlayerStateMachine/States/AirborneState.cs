using UnityEngine;
using System;
using System.Threading.Tasks;
public class AirborneState : PlayerState
{
    public AirborneState(PlayerStateMachine playerMachine, PlayerStateFactory stateFactory) : base(playerMachine, stateFactory)
    {

    }

    public override void EnterState()
    {
        _player.EventAnnouncer.InvokeAction(PlayerEvent.Airborne);
        _player.NormalBody.centerOfMass = new Vector2(0, 0f);
        _player.EventAnnouncer.SubscribeToAddCollision(OnLand);
        _params.RotationStart = _player.NormalBody.rotation;
        
        if (_player.Params.JumpCount == 1)
        {
            _player.InputEvents.OnJumpRelease += JumpRelease;
        } else
        {
            _player.Params.JumpCount = 1;
            _player.InputEvents.OnJumpPress += SecondJump;
        }
    }

    public override void ExitState()
    {
        _player.EventAnnouncer.UnsubscribeToAddCollision(OnLand);
        _player.InputEvents.OnJumpRelease -= JumpRelease;
        _player.InputEvents.OnJumpPress -= SecondJump;
        _player.JumpManager.CancelReleaseCheck();
    }

    public override void FixedUpdateState()
    {
        _player.AnimationManager.UpdateAirborneSpeed();
    }

    private void SecondJump()
    {
        _player.InputEvents.OnJumpPress -= SecondJump;
        _player.JumpManager.ScheduleSecondJump();
        _player.InputEvents.OnJumpRelease += JumpRelease;
    }

    private void JumpRelease()
    {
        _player.InputEvents.OnJumpRelease -= JumpRelease;
        _player.JumpManager.JumpRelease();
        if(_player.Params.JumpCount < _player.Params.JumpLimit && !_player.JumpManager.secondJumpScheduled)
        {
            _player.InputEvents.OnJumpPress += SecondJump;
        }
    }

    private void OnLand(Collision2D collision, MomentumTracker _, ColliderCategory __, TrackingType ___)
    {
        _player.LastLandCollision = collision;
        ChangeState(_stateFactory.GetState(PlayerStateType.Grounded));
    }

}