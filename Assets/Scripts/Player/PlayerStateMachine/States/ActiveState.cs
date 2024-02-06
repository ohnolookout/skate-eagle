using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class ActiveState : PlayerState
{
    public bool doRotate = false;
    private Vector2 _rotationInput = new(0,0);
    private Vector2 _finishPoint;
    public bool crouched = false;
    private Transform _playerTransform;
    private Rigidbody2D _playerBody;
    private bool _checkFinish = false;
    private const int _dampenThreshold = 180;

    public ActiveState(PlayerStateMachine playerMachine, PlayerStateFactory stateFactory) : base(playerMachine, stateFactory)
    {
        _playerTransform = _player.Transform;
        _playerBody = _player.Rigidbody;
        LevelManager.OnActivateFinish += DoActivateFinish;
        _isRootState = true;
    }


    public override void EnterState()
    {
        StartCrouch();
        _player.InputEvents.OnDownPress += StartCrouch;
        _player.InputEvents.OnDownRelease += StopCrouch;
        _player.InputEvents.OnRotate += StartRotate;
        _player.InputEvents.OnRotateRelease += StopRotate;
        _collisionManager.OnCollide += CheckForBodyCollision;
        InitializeSubstate(_stateFactory.GetState(PlayerStateType.Pushing));
    }
    public override void ExitState()
    {
        _player.InputEvents.OnDownPress -= StartCrouch;
        _player.InputEvents.OnDownRelease -= StopCrouch;
        _player.InputEvents.OnRotate -= StartRotate;
        _player.InputEvents.OnRotateRelease -= StopRotate;
        _collisionManager.OnCollide -= CheckForBodyCollision;
    }

    public override void UpdateState()
    {
        DirectionCheck();
        if (_checkFinish)
        {
            FinishCheck();
        }
        if (_substate != null)
        {
            _substate.UpdateStates();
        }
    }

    public override void FixedUpdateState()
    {
        _player.MomentumTracker.Update();
        if (crouched && !_player.Stomping)
        {
            _playerBody.AddForce(new Vector2(0, -_player.DownForce * 20));
        }
        if (doRotate)
        {
            _playerBody.AddTorque(-_player.RotationAccel * _rotationInput.x);
        }
        if (_substate != null)
        {
            _substate.FixedUpdateStates();
        }
    }

    public override void OnCollisionEnter(Collision2D collision)
    {
        base.OnCollisionEnter(collision);
    }

    public override void OnCollisionExit(Collision2D collision)
    {
        base.OnCollisionExit(collision);
    }

    private void StartRotate(Vector2 rotation)
    {
        doRotate = true;
        _rotationInput = rotation;
    }

    private void StopRotate()
    {
        doRotate = false;
        _rotationInput = new(0, 0);
    }

    private void StartCrouch()
    {
        crouched = true;
        _player.Animator.SetBool("Crouched", true);
        _player.Animator.SetTrigger("Crouch");
    }
    private void StopCrouch()
    {
        _player.Animator.SetBool("Crouched", false);
        _player.Animator.SetTrigger("Stand Up");
        crouched = false;
    }
    private void DirectionCheck()
    {
        bool lastDirection = _player.FacingForward;
        _player.FacingForward = _playerBody.velocity.x >= 0;
        if (_player.FacingForward != lastDirection)
        {
            //What is this animator bool used for?
            _player.Animator.SetBool("FacingForward", _player.FacingForward);
            _playerTransform.localScale = new Vector3(-_playerTransform.localScale.x, _playerTransform.localScale.y, _playerTransform.localScale.z);
        }
    }
    private void FinishCheck()
    {
        if (_playerTransform.position.x >= _finishPoint.x && _player.CollisionManager.BothWheelsCollided)
        {
            if (crouched)
            {
                StopCrouch();
            }
            ChangeState(_stateFactory.GetState(PlayerStateType.Braking), false);
        }
    }
    public async void DampenLanding()
    {

        _player.Rigidbody.angularVelocity = Mathf.Clamp(_player.Rigidbody.angularVelocity, -300, 300);
        float timer = 0;
        int underThresholdCount = 0;
        //Add && playerdampen to conditional
        while (timer < 0.2f && underThresholdCount < 2)
        {
            if (Mathf.Abs(_player.Rigidbody.angularVelocity) > 60)
            {
                _player.Rigidbody.angularVelocity *= 0.3f;
            }
            if (_player.Rigidbody.angularVelocity < _dampenThreshold)
            {
                underThresholdCount++;
            }
            else
            {
                underThresholdCount = 0;
            }
            timer += Time.deltaTime;
            await Task.Yield();
        }
    }
    private void CheckForBodyCollision(ColliderCategory colliderCategory, float _)
    {
        if (colliderCategory == ColliderCategory.Body)
        {

            ChangeState(_stateFactory.GetState(PlayerStateType.Ragdoll), false);
        }
    }

    public void DoActivateFinish(Vector2 finishPoint)
    {
        _finishPoint = finishPoint;
        _checkFinish = true;
        LevelManager.OnActivateFinish -= DoActivateFinish;
    }
}
