using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerStateType { Standby, Grounded, Airborne, Jumping, Stomping, Braking, Finished, Ragdoll}
public class PlayerStateMachine
{
    public PlayerState CurrentState { get; set; }
    public Dictionary<PlayerStateType, PlayerState> _stateDict;
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

    public void UpdateCurrentState()
    {
        CurrentState.UpdateState();
    }

    public void FixedUpdateCurrentState()
    {
        CurrentState.FixedUpdateState();
    }
}
