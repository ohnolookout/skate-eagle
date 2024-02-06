using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerStateType
{
    Standby, Active, Pushing, Grounded, Airborne, Jumping, Stomping, Braking, Finished, Ragdoll, Crouched, Standing,
    
}
public class PlayerStateFactory
{
    private PlayerStateMachine _playerMachine;
    private Dictionary<PlayerStateType, PlayerState> _stateDict;

    public PlayerStateFactory(PlayerStateMachine machine)
    {
        _playerMachine = machine;
        _stateDict = new();
        _stateDict[PlayerStateType.Active] = new ActiveState(_playerMachine, this);
        _stateDict[PlayerStateType.Pushing] = new PushingState(_playerMachine, this);
        _stateDict[PlayerStateType.Airborne] = new AirborneState(_playerMachine, this);
        _stateDict[PlayerStateType.Grounded] = new GroundedState(_playerMachine, this);
        _stateDict[PlayerStateType.Braking] = new BrakingState(_playerMachine, this);
        _stateDict[PlayerStateType.Jumping] = new JumpingState(_playerMachine, this);
        _stateDict[PlayerStateType.Finished] = new FinishedState(_playerMachine, this);
        _stateDict[PlayerStateType.Ragdoll] = new RagdollState(_playerMachine, this);
        _stateDict[PlayerStateType.Standby] = new StandbyState(_playerMachine, this);
        _stateDict[PlayerStateType.Stomping] = new StompingState(_playerMachine, this);
        _stateDict[PlayerStateType.Crouched] = new CrouchedState(_playerMachine, this);
        _stateDict[PlayerStateType.Standing] = new StandingState(_playerMachine, this);
    }

    public PlayerState GetState(PlayerStateType type)
    {
        return _stateDict[type];
    }
}
