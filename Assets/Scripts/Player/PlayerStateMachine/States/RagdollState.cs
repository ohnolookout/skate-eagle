internal class RagdollState : PlayerState
{
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

    public override void UpdateState()
    {
        DirectionCheck();
    }

    public override void FixedUpdateState()
    {
        _player.MomentumTracker.Update();
    }

    private void DirectionCheck()
    {
        bool lastDirection = _player.FacingForward;
        _player.FacingForward = _player.RagdollBody.velocity.x >= 0;
        if (_player.FacingForward != lastDirection)
        {
            _player.EventAnnouncer.InvokeAction(PlayerEvent.SwitchDirection);
        }
    }
}