using UnityEngine;
using System;
public class JumpingState : PlayerState
{
    public Action<ColliderCategory, float> OnLand;
    public JumpingState(PlayerStateMachine playerMachine, PlayerStateFactory stateFactory) : base(playerMachine, stateFactory)
    {
    }

    public override void EnterState()
    {
        _player.Animator.SetTrigger("Jump");
        _player.Params.JumpMultiplier = 1 - (_params.JumpCount * 0.25f);
        _player.OnJump?.Invoke(_player);;
        if (_body.velocity.y < 0)
        {
            _body.velocity = new Vector2(_body.velocity.x, 0);
        }
        if (_params.JumpCount == 0)
        {
            _body.angularVelocity *= 0.1f;
            _body.centerOfMass = new Vector2(0, 0.0f);
        }
        _body.AddForce(new Vector2(0, _params.JumpForce * 1000 * _params.JumpMultiplier));
        ChangeState(_stateFactory.GetState(PlayerStateType.Airborne));
    }

    public override void ExitState()
    {
        _player.Params.JumpCount++;
        _player.Params.JumpStartTime = Time.time;
        _player.CheckForJumpRelease = true;
    }
}