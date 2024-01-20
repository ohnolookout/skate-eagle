using System.Collections;
using UnityEngine;
using System;

public class Player : MonoBehaviour, IPlayer
{
    [SerializeField] private Rigidbody2D _body, _ragdollBody, _ragdollBoard;
    private Animator _animator;
    [HideInInspector] private float _jumpForce = 40, _downForce = 95, _rotationAccel = 1200,// _minBoost = 0.6f,
        _flipDelay = 0.75f, _flipBoost = 70, _stompSpeedLimit = -250, _jumpMultiplier = 1;
    private float _rotationStart = 0, _jumpStartTime;
    private Vector2 _rotationInput = new(0, 0);
    private int jumpCount = 0, jumpLimit = 2, _stompThreshold = 2, _downCount;
    private float jumpDuration = 0.25f;
    private float minimumJumpDuration = 0.1f;
    [SerializeField] private int _stompCharge = 0;
    private bool facingForward = true, crouched = false, ragdoll = false, doRotate = false;
    private IEnumerator stompCoroutine, trailCoroutine, dampen;
    public ILevelManager _levelManager;
    public TrailRenderer trail;
    private InputEventController _inputEvents;
    [SerializeField] private CollisionManager _collisionManager;
    public MomentumTracker MomentumTracker { get; set; }
    public static Action FinishStop { get; set; }
    public static Action OnStomp { get; set; }
    public static Action OnDismount { get; set; }
    public static Action<IPlayer, double> OnFlip { get; set; }
    public static Action<IPlayer, double> FlipRoutine { get; set; }
    public static Action<IPlayer> OnStartWithStomp { get; set; }
    public static Action<IPlayer> OnJump { get; set; }
    public static Action<IPlayer> OnSlowToStop { get; set; }
    public Action<Collision2D, ColliderCategory?, TrackingType?> AddCollision { get; set; }
    public Action<Collision2D, ColliderCategory?> RemoveCollision { get; set; }
    public static Action<FinishScreenData> OnFinish { get; set; }
    public static Action OnStartAttempt { get; set; }


    private void Awake()
    {
        AssignComponents();
        _body.bodyType = RigidbodyType2D.Kinematic;
        _body.centerOfMass = new Vector2(0, -2f);
        MomentumTracker = new(_body, _ragdollBoard, _ragdollBody, 12);
    }

    private void Start()
    {
        if (_stompCharge > 0)
        {
            OnStartWithStomp?.Invoke(this);
        }
    }

    private void OnEnable()
    {
        OnFinish += _ => SlowToStop();
        OnFinish += _ => OnSlowToStop?.Invoke(this);
        LevelManager.OnFinish += OnFinish;
        FlipRoutine += (eagleScript, spins) => StartCoroutine(PlayerCoroutines.EndFlip(eagleScript, spins));
        OnFlip += FlipRoutine;
        _collisionManager.OnAirborne += GoAirborne;
        _collisionManager.AddPlayerEvents(this);
        _inputEvents = new(InputType.Player);
        _inputEvents.OnJumpPress += JumpValidation;
        _inputEvents.OnJumpRelease += JumpRelease;
        _inputEvents.OnDownPress += DoStart;
        _inputEvents.OnDownPress += StartCrouch;
        _inputEvents.OnDownRelease += StopCrouch;
        _inputEvents.OnRotate += StartRotate;
        _inputEvents.OnRotateRelease += StopRotate;
        _inputEvents.OnRagdoll += Ragdoll;

    }
    private void OnDisable()
    {
        OnFlip -= FlipRoutine;
        FinishStop = null;
        OnStomp = null;
        OnDismount = null;
        OnFlip = null;
        FlipRoutine = null;
        OnJump = null;
        OnSlowToStop = null;
        AddCollision = null;
        RemoveCollision = null;
        OnFinish = null;
        OnStartAttempt = null;
        _inputEvents.OnJumpPress -= JumpValidation;
        _inputEvents.OnJumpRelease -= JumpRelease;
        _inputEvents.OnDownPress -= StartCrouch;
        _inputEvents.OnDownRelease -= StopCrouch;
        _inputEvents.OnRotate -= StartRotate;
        _inputEvents.OnRotateRelease -= StopRotate;
        _inputEvents.OnRagdoll -= Ragdoll;
    }

    private void AssignComponents()
    {
        _levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<ILevelManager>();
        stompCoroutine = PlayerCoroutines.Stomp(this);
        dampen = PlayerCoroutines.DampenLanding(_body);
        trailCoroutine = PlayerCoroutines.BoostTrail(trail, facingForward);
        _animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (_levelManager.RunState != RunState.Active)
        {
            return;
        }
        DirectionCheck();
        FinishCheck();
        if (!ragdoll)
        {
            UpdateAnimatorParameters();
        }
    }

    private void FixedUpdate()
    {
        MomentumTracker.Update();
        if (_levelManager.RunState != RunState.Active)
        {
            return;
        }
        //Start added to active state
        if (crouched && !PlayerCoroutines.Stomping)
        {
            _body.AddForce(new Vector2(0, -_downForce * 20));
        }
        if (doRotate)
        {
            _body.AddTorque(-_rotationAccel * _rotationInput.x);
        }
    }
    private void StopCrouch()
    {
        _animator.SetBool("Crouched", false);
        if (!PlayerCoroutines.Stomping && _levelManager.RunState == RunState.Active)
        {
            _animator.SetTrigger("Stand Up");
            crouched = false;
        }
    }

    private void StartCrouch()
    {
        //End added to active state
        if (!Collided)
        {
            _downCount++;
            if (_downCount > 1 && StompCharge >= StompThreshold)
            {
                _downCount = 0;
                _stompCharge = 0;
                Stomp();
                return;
            }
            StartCoroutine(DoubleTapWindow());
        }
        //Start added to active state
        _animator.SetBool("Crouched", true);
        if (!PlayerCoroutines.Stomping)
        {
            _animator.SetTrigger("Crouch");
            crouched = true;
        }
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
    //End added to active state
    public void Stomp()
    {
        OnStomp?.Invoke();
        stompCoroutine = PlayerCoroutines.Stomp(this);
        StartCoroutine(stompCoroutine);
    }

    public void JumpValidation()
    {
        if (jumpCount >= jumpLimit || _levelManager.RunState != RunState.Active)
        {
            return;
        }
        float timeSinceLastJump = Time.time - _jumpStartTime;
        if (jumpCount > 0 && timeSinceLastJump < jumpDuration)
        {
            StartCoroutine(PlayerCoroutines.DelayedJump(this, jumpDuration - timeSinceLastJump));
        }
        else
        {
            Jump();
        }
        jumpCount++;
        StopCoroutine(dampen);
    }

    public void Jump()
    {
        _animator.SetTrigger("Jump");
        _jumpMultiplier = 1 - (jumpCount * 0.25f);
        OnJump?.Invoke(this);
        _jumpStartTime = Time.time;
        if (_body.velocity.y < 0)
        {
            _body.velocity = new Vector2(_body.velocity.x, 0);
        }
        if (jumpCount == 0)
        {
            _body.angularVelocity *= 0.1f;
            _body.centerOfMass = new Vector2(0, 0.0f);
        }
        _body.AddForce(new Vector2(0, _jumpForce * 1000 * _jumpMultiplier));
    }

    public void JumpRelease()
    {
        float scaledJumpDuration = jumpDuration * _jumpMultiplier;
        float scaledMinimumJumpDuration = minimumJumpDuration * _jumpMultiplier;
        float timeChange = Time.time - _jumpStartTime;
        if (timeChange <= scaledJumpDuration)
        {
            if (timeChange > scaledMinimumJumpDuration)
            {
                _body.AddForce(new Vector2(0, -_jumpForce * 250 * _jumpMultiplier));
            }
            else
            {
                StartCoroutine(PlayerCoroutines.DelayedJumpDampen(this, scaledMinimumJumpDuration - timeChange));
            }
            if (jumpCount == 1)
            {
                StartCoroutine(PlayerCoroutines.CheckForSecondJump(this));
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_levelManager.RunState == RunState.Finished)
        {
            return;
        }
        if (!ragdoll)
        {
            StopCoroutine(dampen);
            dampen = PlayerCoroutines.DampenLanding(_body);
            StartCoroutine(dampen);
        }
        if (ragdoll)
        {
            return;
        }
        else if(collision.otherCollider.name != "Player")
        {
            AddCollision?.Invoke(collision, null, TrackingType.PlayerNormal);
        }else
        {
            AddCollision?.Invoke(collision, ColliderCategory.Body, TrackingType.PlayerNormal);
            Die();
            return;
        }
        jumpCount = 0;
        if (!Collided)
        {
            _animator.SetFloat("forceDelta", MagnitudeDelta(TrackingType.PlayerNormal));
            _animator.SetTrigger("Land");
        }
        FlipCheck();

    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        RemoveCollision?.Invoke(collision, null);
        _rotationStart = _body.rotation;
    }


    public void Die()
    {
        if (_levelManager.RunState == RunState.GameOver)
        {
            return;
        }
        _inputEvents.DisableInputs();
        StopCoroutine(trailCoroutine);
        trail.emitting = false;
        ragdoll = true;
        _levelManager.GameOver();
    }

    private void UpdateAnimatorParameters()
    {
        _animator.SetFloat("Speed", _body.velocity.magnitude);
        _animator.SetFloat("YSpeed", _body.velocity.y);
        _animator.SetBool("Airborne", !Collided);
        if (!Collided)
        {
            _animator.SetBool("AirborneUp", _body.velocity.y >= 0);
        }
    }


    public void SlowToStop()
    {
        _animator.SetTrigger("Brake");
        _inputEvents.DisableInputs();
        StopCoroutine(trailCoroutine);
        trail.emitting = false;
        StartCoroutine(PlayerCoroutines.SlowToStop(this));
    }

    public void TriggerFinishStop()
    {
        FinishStop?.Invoke();
    }

    public void TriggerBoost(float boostValue, float boostMultiplier)
    {
        StartCoroutine(PlayerCoroutines.AddBoost(_body, boostValue, boostMultiplier));
        StartCoroutine(PlayerCoroutines.BoostTrail(trail, facingForward));
    }

    //Can probably move standby exit input to UI controls and eliminate need for player startattempt event
    public void DoStart()
    {
        OnStartAttempt?.Invoke();
        //Begin add to standbystate onexit
        _animator.SetBool("OnBoard", true);
        _body.bodyType = RigidbodyType2D.Dynamic;
        _body.velocity += new Vector2(15, 0);
        _inputEvents.OnDownPress -= DoStart;
        //end
    }

    private void DirectionCheck()
    {
        bool lastDirection = facingForward;
        facingForward = Rigidbody.velocity.x >= 0;
        if (facingForward != lastDirection && !ragdoll)
        {
            //What is this animator bool used for?
            _animator.SetBool("FacingForward", facingForward);
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
    }

    public void Dismount()
    {
        _animator.SetBool("OnBoard", false);
    }

    public void DismountSound()
    {
        OnDismount?.Invoke();
    }

    private void FinishCheck()
    {
        if (transform.position.x >= _levelManager.FinishPoint.x && _collisionManager.BothWheelsCollided)
        {
            _levelManager.Finish();
        }
    }

    public void Fall()
    {
        StartCoroutine(PlayerCoroutines.DelayedFreeze(this, 0.5f));
        _levelManager.RunState = RunState.Fallen;
    }

    private void FlipCheck()
    {
        double spins = Math.Round(Math.Abs(_rotationStart - _body.rotation) / 360);
        _rotationStart = _body.rotation;
        if (spins >= 1 && _levelManager.RunState != RunState.GameOver)
        {
            if (_stompCharge < _stompThreshold)
            {
                _stompCharge = Mathf.Min((int)spins + _stompCharge, _stompThreshold);
            }
            OnFlip?.Invoke(this, spins);
        }
    }
    private IEnumerator DoubleTapWindow()
    {
        float doubleTapDelay = 0.25f;
        yield return new WaitForSeconds(doubleTapDelay);
        _downCount = Mathf.Clamp(_downCount - 1, 0, 3);

    }
    public void GoAirborne()
    {
        jumpCount = Mathf.Max(1, jumpCount);
        StopCoroutine(dampen);
        _body.centerOfMass = new Vector2(0, 0f);
    }

    public void Ragdoll()
    {
        ragdoll = true;
        _levelManager.GameOver();
    }

    public float MagnitudeDelta(TrackingType body)
    {
        return MomentumTracker.ReboundMagnitude(body);
    }

    public float MagnitudeDelta()
    {
        if (ragdoll)
        {
            return MomentumTracker.ReboundMagnitude(TrackingType.PlayerRagdoll);
        }
        return MomentumTracker.ReboundMagnitude(TrackingType.PlayerNormal);
    }

    public Vector2 VectorChange(TrackingType body)
    {
        return MomentumTracker.VectorChange(body);
    }

    public Transform Transform
    {
        get
        {
            if (ragdoll)
            {
                return _ragdollBody.transform;
            }
            return _body.transform;
        }
    }

    public Rigidbody2D Rigidbody
    {
        get
        {
            if (ragdoll)
            {
                return _ragdollBody;
            }
            return _body;
        }
    }

    public ICollisionManager CollisionManager { get => _collisionManager; }
    public Rigidbody2D RagdollBoard { get => _ragdollBoard; }
    public Rigidbody2D RagdollBody { get => _ragdollBody; }
    public Animator Animator { get => _animator; }
    public Vector2 Velocity { get => Rigidbody.velocity; }
    //public Vector2 VectorChange { get => new(Rigidbody.velocity.x - lastSpeed.x, Rigidbody.velocity.y - lastSpeed.y); }
    public bool FacingForward { get => facingForward; }
    public bool Collided { get => _collisionManager.Collided; }
    public bool Stomping { get => PlayerCoroutines.Stomping; set => PlayerCoroutines.Stomping = value; }
    public bool IsRagdoll { get => ragdoll; }
    public int JumpCount { get => jumpCount; set => jumpCount = Mathf.Min(2, value); }
    public int StompCharge { get => _stompCharge; set => _stompCharge = value; }
    public int StompThreshold { get => _stompThreshold; }
    public float RotationAccel { get => _rotationAccel; set => _rotationAccel = value; }
    public float FlipBoost { get => _flipBoost; set => _flipBoost = value; }
    public float StompSpeedLimit { get => _stompSpeedLimit; set => _stompSpeedLimit = value; }
    public float JumpForce { get => _jumpForce; set => _jumpForce = value; }
    public float FlipDelay { get => _flipDelay; set => _flipDelay = value; }
    public float JumpMultiplier { get => _jumpMultiplier; set => _jumpMultiplier = value; }
    public InputEventController InputEvents { get => _inputEvents; set => _inputEvents = value; }
    public float DownForce { get => _downForce; set => _downForce = value; }
}
