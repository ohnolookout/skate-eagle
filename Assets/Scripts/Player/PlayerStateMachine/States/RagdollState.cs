internal class RagdollState : PlayerState
{
    public RagdollState(PlayerStateMachine playerMachine, PlayerStateFactory stateFactory) : base(playerMachine, stateFactory)
    {
    }

    public override void EnterState()
    {
        _player.Die();
    }

    public override void ExitState()
    {
    }

    public override void FixedUpdateState()
    {
        _player.MomentumTracker.Update();
    }

    public override void UpdateState()
    {
    }
}