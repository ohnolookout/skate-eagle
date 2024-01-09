using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateMachine
{
    public PlayerState CurrentState { get; set; }
    public IPlayer player { get; set; }
    public void Initialize(IPlayer player, PlayerState startingState)
    {
        CurrentState = startingState;
        this.player = player;
        CurrentState.EnterState();
    }
    public void ChangeState(PlayerState newState)
    {
        CurrentState.ExitState();
        CurrentState = newState;
        CurrentState.EnterState();
    }
}
