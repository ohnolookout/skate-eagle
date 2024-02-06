using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class StandbyState : PlayerState
{
    public Action DoChangeState;
    public StandbyState(PlayerStateMachine playerMachine, PlayerStateFactory stateFactory) : base(playerMachine, stateFactory)
    {
        _isRootState = true;
    }

    public override void EnterState()
    {
        DoChangeState += () => ChangeState(new ActiveState(_playerMachine, _stateFactory));
        _playerMachine.Player.InputEvents.OnDownPress += DoChangeState;
        _player.DoLanding = false;
    }

    public override void ExitState()
    {
        _playerMachine.Player.InputEvents.OnDownPress -= DoChangeState;
        _player.Animator.SetBool("OnBoard", true);
        _player.Rigidbody.bodyType = RigidbodyType2D.Dynamic;
        _player.Rigidbody.velocity += new Vector2(15, 0);
        _player.InputEvents.OnDownPress -= ExitState;
        _player.DoStart();
    }

    public override void FixedUpdateState()
    {
    }

    public override void UpdateState()
    {
    }
}