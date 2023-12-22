using UnityEngine;
public class PlayerInactiveState : PlayerBaseState
{
    public PlayerInactiveState(PlayerStateMachine currentContext, PlayerStateFactory factory): base(currentContext, factory)
    {
        _isRootState = true;
    }
    public override void EnterState()
    {
        _context.RunManager.EnterStandby += _ => SwitchState(_factory.StandBy());
        _context.PlayerControls.Inputs.UI.Submit.started += _ => _context.RunManager.GoToStandby();
    }
    public override void UpdateState()
    {        
    }
    public override void FixedUpdateState()
    {
    }
    public override void ExitState()
    {
    }
    public override void CheckSwitchStates()
    {
    }
    public override void InitializeSubState()
    {
    }
    public override void CollisionEnter(Collision2D collision)
    {
    }
    public override void CollisionExit(Collision2D collision)
    {
    }
}
