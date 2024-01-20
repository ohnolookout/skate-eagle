using System.Collections;
using UnityEngine;

public class PlayerState
{
    protected IPlayer _player;
    protected PlayerStateMachine _playerMachine;
    protected PlayerStateFactory _stateFactory;
    protected PlayerState _substate, _superstate;
    protected bool _isRootState = false;

    public PlayerState(PlayerStateMachine playerMachine, PlayerStateFactory stateFactory)
    {
        _playerMachine = playerMachine;
        _stateFactory = stateFactory;
    }

    public virtual void EnterState()
    {

    }

    public virtual void ExitState()
    {

    }

    public virtual void UpdateState()
    {

    }

    public virtual void FixedUpdateState()
    {

    }

    public void UpdateStates()
    {
        UpdateState();
        if(_substate != null)
        {
            _substate.UpdateState();
        }
    }

    public void ExitStates()
    {
        if(_substate != null)
        {
            _substate.ExitState();
        }
        ExitState();
    }

    public void ChangeState(PlayerState newState)
    {
        ExitState();
        newState.EnterState();
        if (_isRootState)
        {
            _playerMachine.ChangeState(newState);
        }
        else if(_superstate != null)
        {
            _superstate.SetSubstate(newState);
        }
    }

    public void SetSubstate(PlayerState substate)
    {
        if(_substate != null)
        {
            _substate.ExitState();
        }
        _substate = substate;
        _substate.EnterState();
        _substate.SetSuperstate(this);
    }

    public void SetSuperstate(PlayerState superstate)
    {
        _superstate = superstate;
    }
}