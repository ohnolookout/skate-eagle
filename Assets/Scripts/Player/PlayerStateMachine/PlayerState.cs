using System.Collections;
using UnityEngine;

public class PlayerState
{

    protected IPlayer player;
    protected PlayerStateMachine playerMachine;

    public PlayerState(IPlayer player, PlayerStateMachine playerMachine)
    {
        this.player = player;
        this.playerMachine = playerMachine;
    }

    public virtual void EnterState()
    {

    }

    public virtual void ExitState()
    {

    }

    public virtual void Update()
    {

    }

    public virtual void FixedUpdate()
    {

    }
}