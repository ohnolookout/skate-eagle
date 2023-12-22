using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StompState : PlayerBaseState
{
    private float _originalRotationAccel;
    private float _stompTimer;
    private Rigidbody2D _rigidbody;
    private bool _goingDown, _accelerating;
    public StompState(PlayerStateMachine currentContext, PlayerStateFactory factory) : base(currentContext, factory)
    {
        _isRootMovement = true;
    }
    public override void EnterState()
    {
        _stompTimer = 0;
        _goingDown = false;
        _accelerating = false;
        _context.CollisionTracker.OnCollide += _ => SwitchState(_factory.Grounded());
        _context.Stomping = true;
        _rigidbody = _context.RigidEagle;
        _rigidbody.angularVelocity = 0;
        _originalRotationAccel = _context.RotationAccel;
        _context.RotationAccel *= 1.5f;
    }
    public override void UpdateState()
    {

    }
    public override void FixedUpdateState()
    {
        if (!_goingDown)
        {
            PauseAtTheTop();
        }
        if (_accelerating)
        {
            Accelerate();
        }

    }
    public override void ExitState()
    {
        _context.TriggerBoost(_context.FlipBoost, 1.8f, 0.2f);
        _context.CollisionTracker.OnCollide -= _ => SwitchState(_factory.Grounded());
        _context.RotationAccel = _originalRotationAccel;
        _context.Stomping = false;
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

    public void PauseAtTheTop()
    {
        _stompTimer += Time.deltaTime;
        if (_stompTimer < 0.075f)
        {
            _rigidbody.velocity -= new Vector2(_rigidbody.velocity.x * 0.1f, _rigidbody.velocity.y * 0.4f);
            _stompTimer += Time.deltaTime;
            return;
        }
        _goingDown = true;
        _accelerating = true;
        _rigidbody.centerOfMass = new Vector2(0, -2f);
    }

    public void Accelerate()
    {
        if (_rigidbody.velocity.y > _context.StompSpeedLimit)
        {
            _rigidbody.velocity -= new Vector2(0, 0.15f * Mathf.Abs(_rigidbody.velocity.y));
            _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, Mathf.Clamp(_rigidbody.velocity.y, _context.StompSpeedLimit, -64));
            return;
        }
        _accelerating = false;
    }
}
