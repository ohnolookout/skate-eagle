using UnityEngine;

public class PlayerAnimationManager
{
    private IPlayer _player;
    private Animator _animator;
    private bool _stomping = false;
    private const int _speedMax = 100, _speedMin = 20, _ySpeedMax = 15, _forceDeltaMin = 80, _forceDeltaMax = 200;

    public PlayerAnimationManager(IPlayer player, Animator animator)
    {
        _player = player;
        _animator = animator;
        _player.EventAnnouncer.SubscribeToEvent(PlayerEvent.Land, Land);
        _player.EventAnnouncer.SubscribeToEvent(PlayerEvent.Stomp, Stomp);
        _player.EventAnnouncer.SubscribeToEvent(PlayerEvent.Brake, Brake);
        _player.EventAnnouncer.SubscribeToEvent(PlayerEvent.Stand, Stand);
        _player.EventAnnouncer.SubscribeToEvent(PlayerEvent.Crouch, Crouch);
        _player.EventAnnouncer.SubscribeToEvent(PlayerEvent.Airborne, Airborne);
        _player.EventAnnouncer.SubscribeToEvent(PlayerEvent.Jump, Jump);
        _player.EventAnnouncer.SubscribeToEvent(PlayerEvent.Push, Push);
    }

    private void Push(IPlayer obj)
    {
        SetOnBoard(true);
        //Crouch(obj);
    }

    private void Stand(IPlayer obj)
    {
        _animator.SetBool("Crouched", false);
    }

    private void Crouch(IPlayer obj)
    {
        _animator.SetBool("Crouched", true);
    }

    private void Jump(IPlayer obj)
    {
        _animator.SetInteger("JumpCount", _player.Params.JumpCount);
        _animator.SetTrigger("Jump");
    }

    private void Airborne(IPlayer obj)
    {
        _animator.SetBool("Airborne", true);
    }

    private void Brake(IPlayer obj)
    {
        _animator.SetTrigger("Brake");
    }

    public void Land(IPlayer player)
    {
        _animator.SetBool("Airborne", false);
        _animator.SetFloat("forceDelta", MinMaxTo01(_player.MomentumTracker.ReboundMagnitude(TrackingType.PlayerNormal), _forceDeltaMin, _forceDeltaMax));
        if (_stomping)
        {
            _animator.SetTrigger("StompLand");
            _stomping = false;
        }
        else
        {
            _animator.SetTrigger("Land");
        }
    }

    public void UpdateSpeed()
    {
        _animator.SetFloat("Speed", MinMaxTo01(_player.NormalBody.linearVelocity.magnitude, _speedMin, _speedMax));
    }

    public void UpdateAirborneSpeed()
    {
        UpdateSpeed();
        _animator.SetFloat("YSpeed", MinMaxTo01(_player.NormalBody.linearVelocity.y, -_ySpeedMax, _ySpeedMax));
    }

    public void SetOnBoard(bool onBoard)
    {
        _animator.SetBool("OnBoard", onBoard);
    }

    public void Stomp(IPlayer player)
    {
        _stomping = true;
        _animator.SetTrigger("Stomp");
    }

    private float MinMaxTo01(float val, int min, int max)
    {
        return Mathf.Clamp01((val - min) / (max - min));
    }

}
