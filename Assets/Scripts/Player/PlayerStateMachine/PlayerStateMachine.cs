using UnityEngine;
using System;
using System.Threading.Tasks;


public class PlayerStateMachine
{
    public PlayerState CurrentState { get; set; }
    private PlayerStateFactory _stateFactory;
    public bool doJump;
    public IPlayer Player { get; set; }

    public PlayerStateMachine(IPlayer player)
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

    
}
