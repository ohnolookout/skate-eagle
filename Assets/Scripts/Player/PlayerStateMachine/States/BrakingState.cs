using UnityEngine;
public class BrakingState : PlayerState
{
    private Rigidbody2D _playerBody;
    private bool _onBoard = true;
    public BrakingState(PlayerStateMachine playerMachine, PlayerStateFactory stateFactory) : base(playerMachine, stateFactory)
    {
        _playerBody = _player.Rigidbody;
        _isRootState = true;
    }

    //Figure out why sound isn't working.
    public override void EnterState()
    {
        Debug.Log("Entering BrakingState");
        _player.Animator.SetTrigger("Brake");
        _player.InputEvents.DisableInputs();
        _player.SlowToStop();

    }

    public override void FixedUpdateState()
    {
        _playerBody.velocity -= _playerBody.velocity * 0.08f;
        CheckForDismount();
        if (Mathf.Abs(_playerBody.velocity.x) < 1)
        {
            _playerBody.velocity = new Vector2(0, 0);
            ChangeState(_stateFactory.GetState(PlayerStateType.Finished));
            return;
        }
        
            
    }

    private void CheckForDismount()
    {
        if (_playerBody.velocity.x < 10f && _onBoard)
        {
            _onBoard = false;
            _player.Dismount();
        }
    }


}