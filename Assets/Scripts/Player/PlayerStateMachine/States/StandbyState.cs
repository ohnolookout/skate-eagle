using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class StandbyState : PlayerState
{
    public Action DoChangeState, DoEnterStandby;
    public StandbyState(PlayerStateMachine playerMachine, PlayerStateFactory stateFactory) : base(playerMachine, stateFactory)
    {
        _isRootState = true;
    }

    public override void EnterState()
    {
        DoEnterStandby += () => _playerMachine.Player.InputEvents.OnDownPress += DoChangeState;
        if(_player.Params.StompCharge > 0)
        {
            DoEnterStandby += () => _player.OnStartWithStomp?.Invoke(_player);
        }
        LevelManager.OnStandby += DoEnterStandby;
        DoChangeState += () => ChangeState(_stateFactory.GetState(PlayerStateType.Active), false);
        _player.DoLanding = false;
    }

    public override void ExitState()
    {
        _playerMachine.Player.InputEvents.OnDownPress -= DoChangeState;
        LevelManager.OnStandby -= DoEnterStandby;
        _player.Animator.SetBool("OnBoard", true);
        _player.NormalBody.bodyType = RigidbodyType2D.Dynamic;
        _player.NormalBody.velocity += new Vector2(15, 0);
        _player.InputEvents.OnDownPress -= ExitState;
        _player.OnStartAttempt?.Invoke();
    }

    public override void FixedUpdateState()
    {
    }

    public override void UpdateState()
    {
    }
}