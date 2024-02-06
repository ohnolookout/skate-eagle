using UnityEngine;
using System.Threading.Tasks;
using System;
public class StompingState : PlayerState
{
    private float _originalRotationAccel;
    private float _timer = 0;
    private bool _stalling = true, _diving = false;
    private Action _boost;
    public StompingState(PlayerStateMachine playerMachine, PlayerStateFactory stateFactory) : base(playerMachine, stateFactory)
    {
        _originalRotationAccel = _player.Params.RotationAccel;
        _boost = () => _player.TriggerBoost(_player.FlipBoost, 1.8f);
    }

    public override void EnterState()
    {
        Debug.Log("Entering stomp");
        _player.CollisionManager.OnCollide += OnLand; 
        _player.Stomping = true;
        _player.JumpCount = 2;
        _player.StompCharge = 0;
        _player.Rigidbody.angularVelocity = 0;
        _player.Params.RotationAccel *= 1.5f;
        _timer = 0;
        _stalling = true;
        _diving = false;
    }

    public override void ExitState()
    {
        Debug.Log("Exiting stomp");
        _player.CollisionManager.OnCollide -= OnLand;
        _player.RotationAccel = _originalRotationAccel;
        _player.Stomping = false;
    }

    public override void FixedUpdateState()
    {
        if (_stalling)
        {
            StallPhase();
            return;
        }
        if (_diving)
        {
            DivePhase();
        }
    }

    private void OnLand(ColliderCategory _, float __)
    {
        _diving = false;
        _stalling = false;
        _player.DelayedFunc(_boost, 0.2f);
        ChangeState(_stateFactory.GetState(PlayerStateType.Grounded));
    }

    private void StallPhase()
    {
        if (_timer < 0.075f)
        {
            _player.Rigidbody.velocity -= new Vector2(_player.Rigidbody.velocity.x * 0.1f, _player.Rigidbody.velocity.y * 0.4f);
            _timer += Time.deltaTime;
        }
        else
        {
            _player.Rigidbody.centerOfMass = new Vector2(0, -2f);
            _stalling = false;
            _diving = true;
        }
    }

    private void DivePhase()
    {
        if (_player.Rigidbody.velocity.y > _player.StompSpeedLimit)
        {
            _player.Rigidbody.velocity -= new Vector2(0, 0.15f * Mathf.Abs(_player.Rigidbody.velocity.y));
            _player.Rigidbody.velocity = new Vector2(_player.Rigidbody.velocity.x, Mathf.Clamp(_player.Rigidbody.velocity.y, _player.StompSpeedLimit, -64));
        }
        else
        {
            _diving = false;
        }
    }

}