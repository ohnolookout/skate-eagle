using UnityEngine;
public class PlayerFinishedState : PlayerBaseState
{
    public PlayerFinishedState(PlayerStateMachine currentContext, PlayerStateFactory factory) : base(currentContext, factory)
    {
        _isRootState = true;
    }
    public override void EnterState()
    {
        _context.Animator.SetTrigger("Brake");
    }
    public override void UpdateState()
    {
    }
    public override void FixedUpdateState()
    {
        _context.RigidEagle.velocity -= _context.RigidEagle.velocity * 0.08f;
        if (_context.RigidEagle.velocity.x < 10f && _context.Animator.GetBool("OnBoard"))
        {
            Dismount();
        }
    }
    public override void ExitState()
    {
        _context.RigidEagle.velocity = new Vector2(0, 0);
    }
    public override void CheckSwitchStates()
    {
        if (Mathf.Abs(_context.RigidEagle.velocity.x) < 1 && Mathf.Abs(_context.RigidEagle.velocity.y) < 1)
        {
            SwitchState(_factory.Stopped());
        }
    }
    public override void CollisionEnter(Collision2D collision)
    {
    }
    public override void CollisionExit(Collision2D collision)
    {
    }
    public override void InitializeSubState()
    {
    }
    public void Dismount()
    {
        _context.Animator.SetBool("OnBoard", false);
    }
}
