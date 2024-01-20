using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateFactory
{
    private PlayerStateMachine _playerMachine;

    public PlayerState Grounded()
    {
        return new GroundedState(_playerMachine, this);
    }

    public PlayerState Active()
    {
        return new ActiveState(_playerMachine, this);
    }
}
