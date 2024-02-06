using UnityEngine;
using System;

public class PushingState : PlayerState
{
    private Action OnJump, OnAirborne;
    public PushingState(PlayerStateMachine playerMachine, PlayerStateFactory stateFactory) : base(playerMachine, stateFactory)
    {
        OnJump += () => ChangeState(_stateFactory.GetState(PlayerStateType.Jumping));
        OnAirborne += () => ChangeState(_stateFactory.GetState(PlayerStateType.Airborne));
        OnAirborne += () => _player.JumpCount = 1;
    }

    public override void EnterState()
    {
        _player.Rigidbody.centerOfMass = new Vector2(0, -2f);
        _player.DoLanding = false;
        Debug.Log("Entering pushing");
        _animator.SetBool("Airborne", false);
        _player.InputEvents.OnJumpPress += OnJump;
        _player.CollisionManager.OnAirborne += OnAirborne;
        _player.JumpCount = 0;
    }

    public override void ExitState()
    {
        _player.DoLanding = true;
        _player.InputEvents.OnJumpPress -= OnJump;
        _player.CollisionManager.OnAirborne -= OnAirborne;
    }

    public override void FixedUpdateState()
    {
        _animator.SetFloat("Speed", _body.velocity.magnitude);
    }
}