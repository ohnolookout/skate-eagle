using UnityEngine;

public class RagdollState : PlayerState
{
    private bool _doCheckFall = true;
    public RagdollState(PlayerStateMachine playerMachine, PlayerStateFactory stateFactory) : base(playerMachine, stateFactory)
    {
    }

    public override void EnterState()
    {
        _player.IsRagdoll = true;
        _player.InputEvents.DisableInputs();
        _player.BoostTokenSource.Cancel();
        _player.Trail.emitting = false;
        _player.EventAnnouncer.InvokeAction(PlayerEvent.Die);
    }

    public override void FixedUpdateState()
    {
        DirectionCheck();

        if (_doCheckFall && _player.RagdollBody.position.y < _player.KillPlaneY)
        {
            _doCheckFall = false;
            _player.EventAnnouncer.InvokeAction(PlayerEvent.Fall);
        }

        _player.MomentumTracker.Update();
    }

    private void DirectionCheck()
    {
        bool lastDirection = _player.FacingForward;
        _player.FacingForward = _player.RagdollBody.linearVelocity.x >= 0;
        if (_player.FacingForward != lastDirection)
        {
            _player.EventAnnouncer.InvokeAction(PlayerEvent.SwitchDirection);
        }
    }
}