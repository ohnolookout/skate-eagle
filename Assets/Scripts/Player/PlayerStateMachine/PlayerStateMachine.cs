using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerStateMachine
{
    public PlayerState CurrentState { get; set; }
    private PlayerStateFactory _stateFactory; 
    public IPlayer Player { get; set; }

    public PlayerStateMachine(IPlayer player, PlayerStateType startingState = PlayerStateType.Standby)
    {
        Player = player;
        _stateFactory = new(this);
    }

    public void InitializeState(PlayerStateType startingState = PlayerStateType.Standby)
    {
        CurrentState = _stateFactory.GetState(startingState);
        CurrentState.EnterState();
    }

    public void ExitStates()
    {
        CurrentState.ExitStates();
    }

    public void UpdateCurrentStates()
    {
        CurrentState.UpdateStates();
    }

    public void FixedUpdateCurrentStates()
    {
        CurrentState.FixedUpdateStates();
    }
    public void UpdateCurrentState()
    {
        CurrentState.UpdateState();
    }

    public void FixedUpdateCurrentState()
    {
        CurrentState.FixedUpdateState();
    }

    public void OnCollisionEnter(Collision2D collision)
    {
        CurrentState.OnCollisionEnter(collision);
    }

    public void OnCollisionExit(Collision2D collision)
    {
        CurrentState.OnCollisionExit(collision);
    }
}
