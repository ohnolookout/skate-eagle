using System.Collections;
using UnityEngine;
using System;

public class Player : MonoBehaviour, IPlayer
{
    [SerializeField] private Rigidbody2D _body, _ragdollBody, _ragdollBoard;
    private Animator _animator;
    private Vector2 _rotationInput = new(0, 0);
    private int _downCount;
    [SerializeField] private int _initialStompCharge = 0;
    private bool facingForward = true, crouched = false, ragdoll = false, doRotate = false;
    private IEnumerator stompCoroutine, trailCoroutine, dampen;
    private PlayerParameters _params;
    public ILevelManager _levelManager;
    public TrailRenderer trail;
    public InputEventController _inputEvents;
    [SerializeField] private CollisionManager _collisionManager;
    public MomentumTracker MomentumTracker { get; set; }
    public Action FinishStop { get; set; }
    public Action OnStomp { get; set; }
    public Action OnDismount { get; set; }
    public Action<IPlayer, double> OnFlip { get; set; }
    public Action<IPlayer, double> FlipRoutine { get; set; }
    public Action<IPlayer> OnStartWithStomp { get; set; }
    public Action<IPlayer> OnJump { get; set; }
    public Action<IPlayer> OnSlowToStop { get; set; }
    public Action<Collision2D, MomentumTracker, ColliderCategory, TrackingType> AddCollision { get; set; }
    public Action<Collision2D, ColliderCategory> RemoveCollision { get; set; }
    public Action<FinishScreenData> OnFinish { get; set; }
    public Action OnStartAttempt { get; set; }


    private void Awake()
    {
        AssignComponents();
        _body.bodyType = RigidbodyType2D.Kinematic;
        _body.centerOfMass = new Vector2(0, -2f);
        MomentumTracker = new(_body, _ragdollBoard, _ragdollBody, 12);
    }

    private void Start()
    {
        if (_initialStompCharge > 0)
        {
            OnStartWithStomp?.Invoke(this);
        }
    }

    private void OnEnable()
    {
        OnFinish += _ => SlowToStop();
        OnFinish += _ => OnSlowToStop?.Invoke(this);
        LevelManager.OnFinish += OnFinish;
        FlipRoutine += (eagleScript, spins) => StartCoroutine(PlayerCoroutines.EndFlip(this, spins));
        OnFlip += FlipRoutine;
        _collisionManager.OnAirborne += GoAirborne;
        _collisionManager.AddPlayer(this);
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
        Debug.Log("Disabling player...");
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
        _params = new(_initialStompCharge);
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
            _body.AddForce(new Vector2(0, -_params.DownForce * 20));
        }
        if (doRotate)
        {
            _body.AddTorque(-_params.RotationAccel * _rotationInput.x);
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
                StompCharge = 0;
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
        if (JumpCount >= _params.JumpLimit || _levelManager.RunState != RunState.Active)
        {
            return;
        }
        float timeSinceLastJump = Time.time - _params.JumpStartTime;
        if (JumpCount > 0 && timeSinceLastJump < _params.FullJumpDuration)
        {
            StartCoroutine(PlayerCoroutines.DelayedJump(this, _params.FullJumpDuration - timeSinceLastJump));
        }
        else
        {
            Jump();
        }
        JumpCount++;
        StopCoroutine(dampen);
    }

    public void Jump()
    {
        _animator.SetTrigger("Jump");
        JumpMultiplier = 1 - (_params.JumpCount * 0.25f);
        OnJump?.Invoke(this);
        _params.JumpStartTime = Time.time;
        if (_body.velocity.y < 0)
        {
            _body.velocity = new Vector2(_body.velocity.x, 0);
        }
        if (_params.JumpCount == 0)
        {
            _body.angularVelocity *= 0.1f;
            _body.centerOfMass = new Vector2(0, 0.0f);
        }
        _body.AddForce(new Vector2(0, JumpForce * 1000 * JumpMultiplier));
    }

    public void JumpRelease()
    {
        float scaledJumpDuration = _params.FullJumpDuration * JumpMultiplier;
        float scaledMinimumJumpDuration = _params.MinJumpDuration * JumpMultiplier;
        float timeChange = Time.time - _params.JumpStartTime;
        if (timeChange <= scaledJumpDuration)
        {
            if (timeChange > scaledMinimumJumpDuration)
            {
                _body.AddForce(new Vector2(0, -_params.JumpForce * 250 * JumpMultiplier));
            }
            else
            {
                StartCoroutine(PlayerCoroutines.DelayedJumpDampen(this, scaledMinimumJumpDuration - timeChange));
            }
            if (JumpCount == 1)
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
            AddCollision?.Invoke(collision, MomentumTracker, ParseColliderName(collision.otherCollider.name), TrackingType.PlayerNormal);
        }else
        {
            AddCollision?.Invoke(collision, MomentumTracker, ColliderCategory.Body, TrackingType.PlayerNormal);
            Die();
            return;
        }
        JumpCount = 0;
        if (!Collided)
        {
            _animator.SetFloat("forceDelta", MagnitudeDelta(TrackingType.PlayerNormal));
            _animator.SetTrigger("Land");
        }
        FlipCheck();

    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        RemoveCollision?.Invoke(collision, ParseColliderName(collision.otherCollider.name));
        _params.RotationStart = _body.rotation;
    }

    private static ColliderCategory ParseColliderName(string colliderName)
    {
        switch (colliderName)
        {
            case "LWheelCollider":
                return ColliderCategory.LWheel;
            case "RWheelCollider":
                return ColliderCategory.RWheel;
            case "BoardCollider":
                return ColliderCategory.Board;
            default:
                return ColliderCategory.Body;
        }
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

    //Begin add to ActiveState Update
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
    //End

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
        double spins = Math.Round(Math.Abs(_params.RotationStart - _body.rotation) / 360);
        _params.RotationStart = _body.rotation;
        if (spins >= 1 && _levelManager.RunState != RunState.GameOver)
        {
            if (StompCharge < StompThreshold)
            {
                StompCharge = Mathf.Min((int)spins + StompCharge, StompThreshold);
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
        JumpCount = Mathf.Max(1, JumpCount);
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
    public bool FacingForward { get => facingForward; set => facingForward = value; }
    public bool Collided { get => _collisionManager.Collided; }
    public bool Stomping { get => PlayerCoroutines.Stomping; set => PlayerCoroutines.Stomping = value; }
    public bool IsRagdoll { get => ragdoll; set => ragdoll = value; }
    public int JumpCount { get => _params.JumpCount; set => _params.JumpCount = Mathf.Min(2, value); }
    public int StompCharge { get => _params.StompCharge; set => _params.StompCharge = value; }
    public int StompThreshold { get => _params.StompThreshold; }
    public float RotationAccel { get => _params.RotationAccel; set => _params.RotationAccel = value; }
    public int FlipBoost { get => _params.FlipBoost; set => _params.FlipBoost = value; }
    public float StompSpeedLimit { get => _params.StompSpeedLimit; }
    public float JumpForce { get => _params.JumpForce;}
    public float FlipDelay { get => _params.FlipDelay;}
    public float JumpMultiplier { get => _params.JumpMultiplier; set => _params.JumpMultiplier = value; }
    public InputEventController InputEvents { get => _inputEvents; set => _inputEvents = value; }
    public float DownForce { get => _params.DownForce; }
    public Action OnDie { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public PlayerParameters Params => throw new NotImplementedException();

    public bool DoLanding { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public Rigidbody2D NormalBody => throw new NotImplementedException();

    public JumpManager JumpManager => throw new NotImplementedException();
}
