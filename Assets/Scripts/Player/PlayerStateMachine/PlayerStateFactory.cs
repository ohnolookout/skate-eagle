using System.Collections.Generic;
public class PlayerStateFactory
{
    PlayerStateMachine _context;
    enum StateType { Inactive, Standby, Active, Dead, Fallen, Finished }
    Dictionary<StateType, PlayerBaseState> _states = new();
    

    public PlayerStateFactory(PlayerStateMachine currentContext)
    {
        _context = currentContext;
        _states[StateType.Inactive] = new PlayerInactiveState(_context, this);
        _states[StateType.Standby] = new PlayerStandbyState(_context, this);
        _states[StateType.Active] = new PlayerActiveState(_context, this);
        _states[StateType.Dead] = new PlayerDeadState(_context, this);
        _states[StateType.Fallen] = new PlayerFallenState(_context, this);
        _states[StateType.Finished] = new PlayerFinishedState(_context, this);
    }

    public PlayerBaseState Inactive()
    {
        return _states[StateType.Inactive];
    }

    public PlayerBaseState StandBy()
    {
        return _states[StateType.Standby];
    }

    public PlayerBaseState Active()
    {
        return _states[StateType.Active];
    }

    public PlayerBaseState Dead()
    {
        return _states[StateType.Dead];
    }

    public PlayerBaseState Fallen()
    {
        return _states[StateType.Fallen];
    }

    public PlayerBaseState Finished()
    {
        return _states[StateType.Finished];
    }
}
