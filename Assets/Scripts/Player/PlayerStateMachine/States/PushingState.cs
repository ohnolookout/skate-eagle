using UnityEngine;
using System;

public class PushingState : PlayerState
{
    private Action OnJump, OnAirborne;
    public PushingState(PlayerStateMachine playerMachine, PlayerStateFactory stateFactory) : base(playerMachine, stateFactory)
    {
        OnJump += () => _player.JumpManager.Jump();
        OnJump += () => ChangeState(_stateFactory.GetState(PlayerStateType.Airborne));
        OnAirborne += () => ChangeState(_stateFactory.GetState(PlayerStateType.Airborne));
    }

    public override void EnterState()
    {
        _player.EventAnnouncer.InvokeAction(PlayerEvent.Push);
        _player.NormalBody.centerOfMass = new Vector2(0, -2f);
        _player.DoLanding = false;
        _player.InputEvents.OnJumpPress += OnJump;
        _player.CollisionManager.OnAirborne += OnAirborne;
        _player.Params.JumpCount = 0;
    }

    public override void ExitState()
    {
        _player.DoLanding = true;
        _player.InputEvents.OnJumpPress -= OnJump;
        _player.CollisionManager.OnAirborne -= OnAirborne;
    }

    public override void FixedUpdateState()
    {
        _player.AnimationManager.UpdateSpeed();
    }
}