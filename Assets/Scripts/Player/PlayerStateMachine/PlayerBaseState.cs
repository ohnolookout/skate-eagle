public abstract class PlayerBaseState
{
    protected PlayerStateMachine _context;
    protected PlayerStateFactory _factory;

    public PlayerBaseState(PlayerStateMachine currentContext, PlayerStateFactory stateFactory)
    {
        _context = currentContext;
        _factory = stateFactory;
    }
    public abstract void EnterState();
    public abstract void UpdateState();
    public abstract void ExitState();
    public abstract void CheckSwitchStates();
    public abstract void InitializeSubState();

    void UpdateStates() 
    {
        
    }
    protected void SwitchState(PlayerBaseState newState)
    {
        ExitState();

        newState.EnterState();

        _context.CurrentState = newState;
    }
    protected void SetSuperState()
    {

    }

    protected void SetSubstate()
    {

    }
}
