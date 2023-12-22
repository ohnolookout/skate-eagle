using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerStateMachine : MonoBehaviour
{
    private PlayerBaseState _currentState;
    private PlayerStateFactory _states;
    [SerializeField] private LiveRunManager _runManager;
    [SerializeField] private Rigidbody2D _rigidEagle, _ragdollBoard;
    [SerializeField] private Animator _animator;
    [SerializeField] private RagdollController _ragdollController;
    [SerializeField] private CollisionTracker _collisionTracker;
    private FlipTextGenerator _textGen;
    private TrailRenderer _trail;
    private PlayerController _playerController;
    private PlayerEventController _playerControls;
    private PlayerAudio _playerAudio;
    private float _rotationSpeed = 0, _jumpForce = 40, _downForce = 95, _rotationAccel = 1200, _minBoost = 0.6f, 
        _flipDelay = 0.75f, _flipBoost = 70, _stompSpeedLimit = -250, _rotationStart = 0, _lastJumpTime = 0, _jumpDuration = 0.25f, _minimumJumpDuration = 0.1f;
    private int _jumpCount = 0, _jumpLimit = 2, _stompThreshold = 2;
    [SerializeField] private int _stompCharge = 0;
    private Vector2 _lastSpeed;
    private bool _facingForward = true, _crouched = false, _stomping = false, _isRotating = false;
    private IEnumerator _trailCoroutine, _dampen;
    public event Action<PlayerStateMachine, int> EndFlip; //Pass in context and flipcount
    public event Action<PlayerStateMachine> OnLanding, OnJump;

    void Awake()
    {
        _runManager = GameObject.FindGameObjectWithTag("Logic").GetComponent<LiveRunManager>();
        _playerControls = new();
        _playerControls.EnterUI();
        _states = new(this);
        _currentState = States.Inactive();
        _currentState.EnterState();
        EndFlip += (_, flipCount) => AddStompCharge(flipCount);
        OnLanding += _ => StartDampen();
        OnLanding += _ => ParseLandingAnimation();
        OnLanding += _ => FlipCheck();

    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void HandleFlip(int flipCount)
    {
        EndFlip?.Invoke(this, flipCount);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CurrentState.CollisionEnter(collision);
        
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        CurrentState.CollisionExit(collision);
        
    }

    public void TriggerBoost(float boostValue, float boostMultiplier, float delay = 0)
    {
        StartCoroutine(PlayerCoroutines.AddBoost(_rigidEagle, boostValue, boostMultiplier));
        StartCoroutine(PlayerCoroutines.BoostTrail(_trail, _facingForward));
    }

    public void StartDampen()
    {
        StopCoroutine(_dampen);
        _dampen = PlayerCoroutines.DampenLanding(_rigidEagle);
        StartCoroutine(_dampen);
    }

    public void EndDampen()
    {
        if (_dampen != null)
        {
            StopCoroutine(_dampen);
        }
    }

    public void ParseLandingAnimation()
    {

    }
    public void AddStompCharge(int flipCount)
    {
        if (_stompCharge < _stompThreshold)
        {
            _stompCharge = Mathf.Clamp(flipCount + _stompCharge, 0, _stompThreshold);
        }
    }

    public Vector2 VectorChange()
    {
        return new(Rigidbody.velocity.x - _lastSpeed.x, Rigidbody.velocity.y - _lastSpeed.y);
    }

    public float ForceDelta()
    {
        Vector2 delta = VectorChange();
        float forceDelta = 0;
        if (_lastSpeed.x > 0 && delta.x < 0)
        {
            forceDelta -= delta.x;
        }
        else if (_lastSpeed.x < 0 && delta.x > 0)
        {
            forceDelta += delta.x;
        }
        forceDelta += delta.y;
        return forceDelta;
    }

    public void DoLanding()
    {
        OnLanding?.Invoke(this);
    }

    public void DoJump()
    {
        OnJump?.Invoke(this);
    }

    private void FlipCheck()
    {
        int flipCount = (int)Math.Round(Math.Abs(_rotationStart - _rigidEagle.rotation) / 360);
        _rotationStart = _rigidEagle.rotation;
        if (flipCount >= 1)
        {
            HandleFlip(flipCount);
        }
    }

    private void StopTrail()
    {
        StopCoroutine(_trailCoroutine);
        _trail.emitting = false;
    }

    public void ValidateJump()
    {
        if (_jumpCount >= _jumpLimit)
        {
            return;
        }
        float timeSinceLastJump = Time.time - _lastJumpTime;
        if (_jumpCount > 0 && timeSinceLastJump < _jumpDuration)
        {
            StartCoroutine(PlayerCoroutines.DelayedJump(this, _jumpDuration - timeSinceLastJump));
        }
        else
        {
            StopCoroutine(_dampen);
            OnJump?.Invoke(this);
        }
        _jumpCount++;
    }

    public Rigidbody2D Rigidbody
    {
        get
        {
            if (IsRagdoll)
            {
                return _ragdollController.spine;
            }
            return _rigidEagle;
        }
    }

    public PlayerBaseState CurrentState { get => _currentState; set => _currentState = value; }
    public PlayerStateFactory States { get => _states;}
    public LiveRunManager RunManager { get => _runManager;}
    public Rigidbody2D RigidEagle { get => _rigidEagle;}
    public Rigidbody2D RagdollBoard { get => _ragdollBoard; }
    public Animator Animator { get => _animator;}
    public RagdollController RagdollController { get => _ragdollController; }
    public CollisionTracker CollisionTracker { get => _collisionTracker; }
    public FlipTextGenerator TextGen { get => _textGen; }
    public TrailRenderer Trail { get => _trail;}
    public PlayerController PlayerController { get => _playerController;}
    public PlayerEventController PlayerControls { get => _playerControls;}
    public PlayerAudio PlayerAudio { get => _playerAudio;}
    public float RotationSpeed { get => _rotationSpeed; set => _rotationSpeed = value; }
    public float JumpForce { get => _jumpForce;}
    public float DownForce { get => _downForce;}
    public float RotationAccel { get => _rotationAccel; set => _rotationAccel = value; }
    public float MinBoost { get => _minBoost;}
    public float FlipDelay { get => _flipDelay;}
    public float FlipBoost { get => _flipBoost;}
    public float StompSpeedLimit { get => _stompSpeedLimit;}
    public float RotationStart { get => _rotationStart; set => _rotationStart = value; }
    public int JumpCount { get => _jumpCount; set => _jumpCount = value; }
    public int JumpLimit { get => _jumpLimit;}
    public int StompThreshold { get => _stompThreshold; set => _stompThreshold = value; }
    public int StompCharge { get => _stompCharge; set => _stompCharge = value; }
    public Vector2 LastSpeed { get => _lastSpeed; set => _lastSpeed = value; }
    public bool FacingForward { get => _facingForward; set => _facingForward = value; }
    public bool Crouched { get => _crouched; set => _crouched = value; }
    public bool IsRagdoll { get => _ragdollController.IsRagdoll; }
    public bool Collided { get => _collisionTracker.Collided; }
    public bool Stomping { get => _stomping; set => _stomping = value; }
    public IEnumerator TrailCoroutine { get => _trailCoroutine; set => _trailCoroutine = value; }
    public IEnumerator Dampen { get => _dampen; set => _dampen = value; }
    public bool IsRotating { get => _isRotating; set => _isRotating = value; }
    public float LastJumpTime { get => _lastJumpTime; set => _lastJumpTime = value; }
    public float JumpDuration { get => _jumpDuration; set => _jumpDuration = value; }
    public float MinimumJumpDuration { get => _minimumJumpDuration; set => _minimumJumpDuration = value; }
}
