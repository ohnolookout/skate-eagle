internal class RagdollState : PlayerState
{
    public RagdollState(PlayerStateMachine playerMachine, PlayerStateFactory stateFactory) : base(playerMachine, stateFactory)
    {
    }

    public override void EnterState()
    {
        _player.IsRagdoll = true;
        _player.InputEvents.DisableInputs();
        _player.CancelAsyncTokens();
        _player.Trail.emitting = false;
        _player.EventAnnouncer.InvokeAction(PlayerEvent.Die);
    }

    public override void FixedUpdateState()
    {
        _player.MomentumTracker.Update();
    }

}