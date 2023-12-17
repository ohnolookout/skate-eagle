using UnityEngine;
public class PlayerStandbyState : PlayerBaseState
{
    public PlayerStandbyState(PlayerStateMachine currentContext, PlayerStateFactory factory) : base(currentContext, factory)
    {

    }
    public override void EnterState()
    {
        _context.runManager.EnterAttempt += _ => NextState();
    }
    public override void UpdateState()
    {
        //If controller down, trigger EnterActive;
        if (_context.playerController.down)
        {
            _context.runManager.StartAttempt();
        }
    }

    //Remove StartAttempt from playerScript
    public override void ExitState()
    {
        _context.animator.SetBool("OnBoard", true);
        _context.rigidEagle.bodyType = RigidbodyType2D.Dynamic;
        _context.rigidEagle.velocity += new Vector2(15, 0);
    }
    public override void CheckSwitchStates()
    {
        
    }
    public override void InitializeSubState()
    {
    }

    private void NextState()
    {
        SwitchState(_factory.Active());
    }
}
