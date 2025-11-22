using System;
using System.Collections;
using UnityEngine;

public abstract class PlayerState
{
    protected IPlayer _player;
    protected Rigidbody2D _body;
    protected InputEventController _inputEvents;
    protected ICollisionManager _collisionManager;
    protected PlayerParameters _params;
    protected PlayerStateMachine _playerMachine;
    protected PlayerStateFactory _stateFactory;
    protected PlayerState _substate, _superstate;
    protected bool _isRootState = false;
    public PlayerState Substate => _substate;

    public PlayerState(PlayerStateMachine playerMachine, PlayerStateFactory stateFactory)
    {
        _playerMachine = playerMachine;
        _stateFactory = stateFactory;
        _player = playerMachine.Player;
        _body = _player.NormalBody;
        _collisionManager = _player.CollisionManager;
        _inputEvents = _player.InputEvents;
        _params = _player.Params;
    }

    public virtual void EnterState() { }
    public virtual void ExitState() { }
    public virtual void UpdateState() { }
    public virtual void FixedUpdateState() { }


    public void InitializeSubstate(PlayerState newSubstate)
    {
        if(_substate != null)
        {
            _substate.ChangeState(newSubstate);
            return;
        }
        SetSubstate(newSubstate);
        _substate.EnterState();
    }

    public void UpdateStates()
    {
        UpdateState();
        if (_substate != null)
        {
            _substate.UpdateStates();
        }
    }

    public void FixedUpdateStates()
    {
        FixedUpdateState();
        if (_substate != null)
        {
            _substate.FixedUpdateStates();
        }
    }

    public void ExitStates()
    {
        ExitState();
        if (_substate != null)
        {
            _substate.ExitStates();
        }
    }

    public void ChangeState(PlayerState newState, bool maintainSubstates = true)
    {
        ExitState();
        newState.EnterState();
        if (!maintainSubstates)
        {
            _playerMachine.CurrentState = newState;
            return;
        }
        if (_isRootState)
        {
            _playerMachine.CurrentState = newState;
            if(_substate != null)
            {
                newState.SetSubstate(_substate);
            }
        }
        else if(_superstate != null)
        {
            _superstate.SetSubstate(newState);
        }
    }
    public void SetSubstate(PlayerState substate)
    {
        _substate = substate;
        _substate.SetSuperstate(this);
    }

    public void SetSuperstate(PlayerState superstate)
    {
        _superstate = superstate;
    }
}