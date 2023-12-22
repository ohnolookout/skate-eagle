using UnityEngine;
public class PlayerStoppedState : PlayerBaseState
{
    public PlayerStoppedState(PlayerStateMachine currentContext, PlayerStateFactory factory) : base(currentContext, factory)
    {
        _isRootState = true;
    }
    public override void EnterState()
    {
        _context.PlayerControls.Inputs.UI.Submit.started += _ => _context.RunManager.RestartGame();
    }
    public override void UpdateState()
    {

    }
    public override void FixedUpdateState()
    {

    }
    public override void ExitState()
    {
        _context.PlayerControls.Inputs.UI.Submit.started -= _ => _context.RunManager.RestartGame();
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
