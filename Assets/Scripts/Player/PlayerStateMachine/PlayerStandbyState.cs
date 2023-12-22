using UnityEngine;
public class PlayerStandbyState : PlayerBaseState
{
    public PlayerStandbyState(PlayerStateMachine currentContext, PlayerStateFactory factory) : base(currentContext, factory)
    {
        _isRootState = true;
    }
    public override void EnterState()
    {
        _context.RunManager.EnterAttempt += _ => SwitchState(_factory.Active());
        _context.PlayerControls.Inputs.Player.Down.started += _ => _context.RunManager.StartAttempt();
    }
    public override void UpdateState()
    {
        
    }
    public override void FixedUpdateState()
    {
    }

    public override void ExitState()
    {
        _context.Animator.SetBool("OnBoard", true);
        _context.RigidEagle.bodyType = RigidbodyType2D.Dynamic;
        _context.RigidEagle.velocity += new Vector2(15, 0);
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
