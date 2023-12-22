using UnityEngine;
public abstract class PlayerBaseState
{
    protected PlayerStateMachine _context;
    protected PlayerStateFactory _factory;
    protected PlayerBaseState _currentSubstate;
    protected PlayerBaseState _currentSuperstate;
    protected bool _isRootState = false;
    protected bool _isRootMovement = false;

    public PlayerBaseState(PlayerStateMachine currentContext, PlayerStateFactory stateFactory)
    {
        _context = currentContext;
        _factory = stateFactory;
    }
    public abstract void EnterState();
    public abstract void UpdateState();
    public abstract void FixedUpdateState();
    public abstract void ExitState();
    public abstract void CheckSwitchStates();
    public abstract void InitializeSubState();
    public abstract void CollisionEnter(Collision2D collision);
    public abstract void CollisionExit(Collision2D collision);

    public void UpdateStates() 
    {
        UpdateState();
        if(_currentSubstate != null)
        {
            _currentSubstate.UpdateStates();
        }
    }
    protected void SwitchState(PlayerBaseState newState)
    {
        ExitState();

        newState.EnterState();
        if (_isRootState)
        {
            _context.CurrentState = newState;
        } else if (_currentSuperstate != null)
        {
            _currentSuperstate.SetSubstate(newState);
        }
    }
    protected void SetSuperState(PlayerBaseState newSuperstate)
    {
        _currentSuperstate = newSuperstate;
    }

    protected void SetSubstate(PlayerBaseState newSubstate)
    {
        _currentSubstate = newSubstate;
        newSubstate.SetSuperState(this);
    }
}
