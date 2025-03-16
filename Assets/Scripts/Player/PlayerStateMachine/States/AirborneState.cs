using UnityEngine;
using System;
using System.Threading.Tasks;
public class AirborneState : PlayerState
{
    private int _downPressCount = 0;
    private Action _decreaseDownPressCount;
    public AirborneState(PlayerStateMachine playerMachine, PlayerStateFactory stateFactory) : base(playerMachine, stateFactory)
    {
        _decreaseDownPressCount = () => Mathf.Clamp(_downPressCount - 1, 0, 3);

    }

    public override void EnterState()
    {
        _downPressCount = 0;
        _player.EventAnnouncer.InvokeAction(PlayerEvent.Airborne);
        _player.NormalBody.centerOfMass = new Vector2(0, 0f);
        _player.CollisionManager.OnCollide += OnLand;
        _params.RotationStart = _player.NormalBody.rotation;
        
        if (_player.Params.StompCharge >= _player.Params.StompThreshold)
        {
            _player.InputEvents.OnDownPress += DownPress;
        }

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
        _player.CollisionManager.OnCollide -= OnLand;
        _player.InputEvents.OnJumpRelease -= JumpRelease;
        _player.InputEvents.OnJumpPress -= SecondJump;
        _player.InputEvents.OnDownPress -= DownPress;
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
        if(_player.Params.JumpCount < _player.Params.JumpLimit)
        {
            _player.InputEvents.OnJumpPress += SecondJump;
        }
    }

    private void OnLand(ColliderCategory _, float __)
    {
        ChangeState(_stateFactory.GetState(PlayerStateType.Grounded));
    }

    private void DownPress()
    {
        _downPressCount++;
        if (_downPressCount > 1)
        {
            ChangeState(_stateFactory.GetState(PlayerStateType.Stomping));
        }
        else
        {
            PlayerAsyncUtility.DelayedFunc(_decreaseDownPressCount, 0.25f);
        }
    }
}