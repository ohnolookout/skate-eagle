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
        DoEnterStandby += () =>
        {
            _playerMachine.Player.InputEvents.OnDownPress += DoChangeState;
        };
        LevelManager.OnStandby += DoEnterStandby;
        DoChangeState += () => ChangeState(_stateFactory.GetState(PlayerStateType.Active), false);
        _player.DoLanding = false;
    }

    public override void ExitState()
    {
        _playerMachine.Player.InputEvents.OnDownPress -= DoChangeState;
        LevelManager.OnStandby -= DoEnterStandby;
        _player.AnimationManager.SetOnBoard(true);
        _player.NormalBody.bodyType = RigidbodyType2D.Dynamic;
        _player.NormalBody.linearVelocity += new Vector2(15, 0);
        _player.InputEvents.OnDownPress -= ExitState;
        _player.EventAnnouncer.InvokeAction(PlayerEvent.StartAttempt);
    }

}