public class PlayerInactiveState : PlayerBaseState
{
    public PlayerInactiveState(PlayerStateMachine currentContext, PlayerStateFactory factory): base(currentContext, factory)
    {

    }
    public override void EnterState()
    {
        _context._runManager.EnterStandby += _ => NextState();
    }
    public override void UpdateState()
    {
    }
    public override void ExitState()
    {
    }
    public override void CheckSwitchStates()
    {
    }
    public override void InitializeSubState()
    {
    }

    private void NextState()
    {
        SwitchState(_factory.StandBy());
    }
}
