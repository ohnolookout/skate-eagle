using UnityEngine;

// Manages all player animation state changes and triggers based on player events
public class PlayerAnimationManager
{
    private IPlayer _player;
    private Animator _animator;
    // Tracks if the player is currently stomping to trigger the correct landing animation
    private bool _stomping = false;
    // Animation parameter limits for normalization
    private const int _speedMax = 100, _speedMin = 20, _ySpeedMax = 15, _forceDeltaMin = 20, _forceDeltaMax = 120;

    // Subscribes to relevant player events to update animation state
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

    // Called when the player pushes off or mounts the board
    private void Push(IPlayer obj)
    {
        SetOnBoard(true);
    }

    // Called when the player stands up from crouch
    private void Stand(IPlayer obj)
    {
        _animator.SetBool("Crouched", false);
    }

    // Called when the player crouches
    private void Crouch(IPlayer obj)
    {
        _animator.SetBool("Crouched", true);
    }

    // Called when the player jumps
    private void Jump(IPlayer obj)
    {
        _animator.SetInteger("JumpCount", _player.Params.JumpCount);
        _animator.SetTrigger("Jump");
    }

    // Called when the player becomes airborne
    private void Airborne(IPlayer obj)
    {
        _animator.SetBool("Airborne", true);
    }

    // Called when the player brakes
    private void Brake(IPlayer obj)
    {
        _animator.SetTrigger("Brake");
    }

    // Called when the player lands on the ground
    public void Land(IPlayer player)
    {
        _animator.SetBool("Airborne", false);
        // Set forceDelta parameter based on landing force
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

    // Updates the player's speed parameter for ground movement
    public void UpdateSpeed()
    {
        _animator.SetFloat("Speed", MinMaxTo01(_player.NormalBody.linearVelocity.magnitude, _speedMin, _speedMax));
    }

    // Updates the player's speed and vertical speed parameters while airborne
    public void UpdateAirborneSpeed()
    {
        UpdateSpeed();
        _animator.SetFloat("YSpeed", MinMaxTo01(_player.NormalBody.linearVelocity.y, -_ySpeedMax, _ySpeedMax));
    }

    // Sets the OnBoard animation parameter
    public void SetOnBoard(bool onBoard)
    {
        _animator.SetBool("OnBoard", onBoard);
    }

    // Called when the player initiates a stomp
    public void Stomp(IPlayer player)
    {
        _stomping = true;
        _animator.SetTrigger("Stomp");
    }

    // Normalizes a value between min and max to a 0-1 range for animation parameters
    private float MinMaxTo01(float val, int min, int max)
    {
        return Mathf.Clamp01((val - min) / (float)(max - min));
    }
}
