using System;
using System.Collections;
using UnityEngine;

public class EagleScript : MonoBehaviour, IPlayer
{
    public Rigidbody2D rigidEagle;
    public Animator animator;
    [HideInInspector] public float rotationSpeed = 0, jumpForce = 40, downForce = 95, rotationAccel = 1200, minBoost = 0.6f, flipDelay = 0.75f, flipBoost = 70, stompSpeedLimit = -250;
    private float rotationStart = 0, jumpStartTime;
    private Vector2 lastSpeed;
    private int jumpCount = 0, jumpLimit = 2, _stompThreshold = 2;
    private float jumpDuration = 0.25f;
    private float minimumJumpDuration = 0.1f;
    [SerializeField] private int _stompCharge = 0;
    private bool facingForward = true, crouched = false, ragdoll = false;
    private IEnumerator stompCoroutine, trailCoroutine, dampen;
    public LiveRunManager logic;
    public TrailRenderer trail;
    public PlayerController playerController;
    [SerializeField] private CollisionTracker collisionTracker;
    [SerializeField] private Rigidbody2D ragdollBoard, ragdollSpine;
    private static Action _finishStop, _onStomp, _onDismount;
    private static Action<IPlayer, double> _onFlip, _flipRoutine;
    private static Action<IPlayer> _onStartWithStomp, _onJump, _onSlowToStop;
    private static Action<Collision2D, float, ColliderCategory?> _addCollision, _removeCollision;
    private static Action<FinishScreenData> onFinish;


    private void Awake()
    {
        AssignComponents();
        rigidEagle.bodyType = RigidbodyType2D.Kinematic;
        rigidEagle.centerOfMass = new Vector2(0, -2f);
    }

    private void Start()
    {
        lastSpeed = Rigidbody.velocity;
    }

    private void OnEnable()
    {
        onFinish += _ => SlowToStop();
        onFinish += _ => _onSlowToStop?.Invoke(this);
        LiveRunManager.OnFinish += onFinish;
        _flipRoutine += (eagleScript, spins) => StartCoroutine(PlayerCoroutines.EndFlip(eagleScript, spins));
        _onFlip += _flipRoutine;
        collisionTracker.OnAirborne += GoAirborne;
        _addCollision += collisionTracker.AddCollision;
        _removeCollision += collisionTracker.RemoveCollision;
    }
    private void OnDisable()
    {
        _onFlip -= _flipRoutine;
        _addCollision -= collisionTracker.AddCollision;
        _removeCollision += collisionTracker.RemoveCollision;
    }

    private void AssignComponents()
    {
        logic = GameObject.FindGameObjectWithTag("Logic").GetComponent<LiveRunManager>();
        stompCoroutine = PlayerCoroutines.Stomp(this);
        dampen = PlayerCoroutines.DampenLanding(rigidEagle);
        trailCoroutine = PlayerCoroutines.BoostTrail(trail, facingForward);
    }

    void Update()
    {

        if (logic.RunState == RunState.Standby)
        {
            StartCheck();
        }
        if (logic.RunState != RunState.Active)
        {
            return;
        }
        DirectionCheck();
        if (playerController.down)
        {
            if (!crouched)
            {
                animator.SetTrigger("Crouch");
                crouched = true;
            }
        }
        else
        {
            if (crouched)
            {
                animator.SetTrigger("Stand Up");
                crouched = false;
            }
        }
        if (playerController.stomp)
        {
            playerController.stomp = false;
            if (_stompCharge >= _stompThreshold)
            {
                _stompCharge = 0;
                Stomp();
            }
        }
        FinishCheck();
        if (!ragdoll)
        {
            UpdateAnimatorParameters();
        }
    }

    private void FixedUpdate()
    {
        if (logic.RunState != RunState.Active)
        {
            return;
        }
        if (playerController.down && !PlayerCoroutines.Stomping)
        {
            rigidEagle.AddForce(new Vector2(0, -downForce * 20));
        }

        rigidEagle.AddTorque(-rotationAccel * playerController.rotation.x);
        lastSpeed = rigidEagle.velocity;
    }

    public void Stomp()
    {
        _onStomp?.Invoke();
        stompCoroutine = PlayerCoroutines.Stomp(this);
        StartCoroutine(stompCoroutine);
    }

    public void JumpValidation()
    {
        if (jumpCount >= jumpLimit || logic.RunState != RunState.Active)
        {
            return;
        }
        float timeSinceLastJump = Time.time - jumpStartTime;
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

    [HideInInspector] public float jumpMultiplier = 1;

    public void Jump()
    {
        animator.SetTrigger("Jump");
        jumpMultiplier = 1 - (jumpCount * 0.25f);
        _onJump?.Invoke(this);
        jumpStartTime = Time.time;
        if (rigidEagle.velocity.y < 0)
        {
            rigidEagle.velocity = new Vector2(rigidEagle.velocity.x, 0);
        }
        if (jumpCount == 0)
        {
            rigidEagle.angularVelocity *= 0.1f;
            rigidEagle.centerOfMass = new Vector2(0, 0.0f);
        }
        rigidEagle.AddForce(new Vector2(0, jumpForce * 1000 * jumpMultiplier));
    }

    public void JumpRelease()
    {
        float scaledJumpDuration = jumpDuration * jumpMultiplier;
        float scaledMinimumJumpDuration = minimumJumpDuration * jumpMultiplier;
        float timeChange = Time.time - jumpStartTime;
        if (timeChange <= scaledJumpDuration)
        {
            if (timeChange > scaledMinimumJumpDuration)
            {
                rigidEagle.AddForce(new Vector2(0, -jumpForce * 250 * jumpMultiplier));
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
        if (logic.RunState == RunState.Finished)
        {
            return;
        }
        if (!ragdoll)
        {
            StopCoroutine(dampen);
            dampen = PlayerCoroutines.DampenLanding(rigidEagle);
            StartCoroutine(dampen);
        }
        _addCollision?.Invoke(collision, MagnitudeDelta(TrackingBody.PlayerNormal), null);
        if (ragdoll)
        {
            return;
        }
        if (collision.otherCollider.name == "Skate Eagle")
        {
            Die();
            return;
        }
        jumpCount = 0;
        if (!Collided)
        {
            animator.SetFloat("forceDelta", MagnitudeDelta(TrackingBody.PlayerNormal));
            animator.SetTrigger("Land");
        }
        FlipCheck();

    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        _removeCollision?.Invoke(collision, Velocity.magnitude, null);
        rotationStart = rigidEagle.rotation;
    }

    
    public void Die()
    {
        if (logic.RunState == RunState.GameOver)
        {
            return;
        }
        StopCoroutine(trailCoroutine);
        trail.emitting = false;
        ragdoll = true;
        logic.GameOver();
    }

    private void UpdateAnimatorParameters()
    {
        animator.SetFloat("Speed", rigidEagle.velocity.magnitude);
        animator.SetFloat("YSpeed", rigidEagle.velocity.y);
        animator.SetBool("FacingForward", facingForward);
        animator.SetBool("Airborne", !Collided);
        animator.SetBool("Crouched", playerController.down);
        if (!Collided)
        {
            animator.SetBool("AirborneUp", rigidEagle.velocity.y >= 0);
        }
    }


    public void SlowToStop()
    {
        StopCoroutine(trailCoroutine);
        trail.emitting = false;
        animator.SetTrigger("Brake");
        StartCoroutine(PlayerCoroutines.SlowToStop(this));
    }

    public void TriggerFinishStop()
    {
        _finishStop?.Invoke();
    }

    public void TriggerBoost(float boostValue, float boostMultiplier)
    {
        StartCoroutine(PlayerCoroutines.AddBoost(rigidEagle, boostValue, boostMultiplier));
        StartCoroutine(PlayerCoroutines.BoostTrail(trail, facingForward));
    }


    private void StartCheck()
    {
        if (playerController.down)
        {
            logic.StartAttempt();
            animator.SetBool("OnBoard", true);
            rigidEagle.bodyType = RigidbodyType2D.Dynamic;
            rigidEagle.velocity += new Vector2(15, 0);
        }
    }

    private void DirectionCheck()
    {
        bool lastDirection = facingForward;
        facingForward = Rigidbody.velocity.x >= 0;
        if (lastDirection != facingForward && !ragdoll)
        {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
    }

    public void Dismount()
    {
        animator.SetBool("OnBoard", false);
    }

    public void DismountSound()
    {
        _onDismount?.Invoke();
    }

    private void FinishCheck()
    {
        if (transform.position.x >= logic.FinishPoint.x && collisionTracker.BothWheelsCollided)
        {
            logic.Finish();
        }
    }

    public void Fall()
    {
        StartCoroutine(PlayerCoroutines.DelayedFreeze(this, 0.5f));
        logic.RunState = RunState.Fallen;
    }

    private void FlipCheck()
    {
        double spins = Math.Round(Math.Abs(rotationStart - rigidEagle.rotation) / 360);
        rotationStart = rigidEagle.rotation;
        if (spins >= 1 && logic.RunState != RunState.GameOver)
        {
            if (_stompCharge < _stompThreshold)
            {
                _stompCharge = Mathf.Min((int)spins + _stompCharge, _stompThreshold);
            }
            _onFlip?.Invoke(this, spins);
        }
    }

    public void GoAirborne()
    {
        jumpCount = Mathf.Max(1, jumpCount);
        StopCoroutine(dampen);
        rigidEagle.centerOfMass = new Vector2(0, 0f);
    }

    public void Ragdoll()
    {
        ragdoll = true;
        logic.GameOver();
    }
    public float MagnitudeDelta(TrackingBody body)
    {
        return MagnitudeDelta();
    }
    public float MagnitudeDelta()
    {
        Vector2 delta = VectorChange(TrackingBody.PlayerNormal);
        float forceDelta = 0;
        if (lastSpeed.x > 0 && delta.x < 0)
        {
            forceDelta -= delta.x;
        }
        else if (lastSpeed.x < 0 && delta.x > 0)
        {
            forceDelta += delta.x;
        }
        forceDelta += delta.y;
        return forceDelta;
    }
    public Vector2 VectorChange(TrackingBody body)
    {
        return new(Rigidbody.velocity.x - lastSpeed.x, Rigidbody.velocity.y - lastSpeed.y);
    }

    public Transform Transform
    {
        get
        {
            if (ragdoll)
            {
                return ragdollSpine.transform;
            }
            return rigidEagle.transform;
        }
    }

    public Rigidbody2D Rigidbody
    {
        get
        {
            if (ragdoll)
            {
                return ragdollSpine;
            }
            return rigidEagle;
        }
    }

    public Rigidbody2D RagdollBoard { get => ragdollBoard; }
    public Rigidbody2D RagdollBody { get => ragdollSpine; }
    public Animator Animator { get => animator; }
    public ICollisionManager CollisionManager { get => collisionTracker; }
    public bool FacingForward { get => facingForward; }
    public bool Collided { get => collisionTracker.Collided; }
    public bool Stomping { get => PlayerCoroutines.Stomping; set => PlayerCoroutines.Stomping = value; }
    public int JumpCount { get => jumpCount; set => jumpCount = Mathf.Min(2, value); }
    public Vector2 Velocity { get => Rigidbody.velocity; }
    public bool IsRagdoll { get => ragdoll; }
    public int StompCharge { get => _stompCharge; set => _stompCharge = value; }
    public int StompThreshold { get => _stompThreshold; }
    public float RotationAccel { get => rotationAccel; set => rotationAccel = value; }
    public float FlipBoost { get => flipBoost; set => flipBoost = value; }
    public float StompSpeedLimit { get => stompSpeedLimit; set => stompSpeedLimit = value; }
    public float JumpForce { get => jumpForce; set => jumpForce = value; }
    public float FlipDelay { get => flipDelay; set => flipDelay = value; }
    public float JumpMultiplier { get => jumpMultiplier; set => jumpMultiplier = value; }
    public static Action FinishStop { get => _finishStop; set => _finishStop = value; }
    public static Action OnStomp { get => _onStomp; set => _onStomp = value; }
    public static Action OnDismount { get => _onDismount; set => _onDismount = value; }
    public static Action<Collision2D, float, ColliderCategory?> AddCollision { get => _addCollision; set => _addCollision = value; }
    public static Action<Collision2D, float, ColliderCategory?> RemoveCollision { get => _removeCollision; set => _removeCollision = value; }
    public static Action<IPlayer> OnJump { get => _onJump; set => _onJump = value; }
    public static Action<IPlayer> OnSlowToStop { get => _onSlowToStop; set => _onSlowToStop = value; }
    public static Action<IPlayer, double> OnFlip { get => _onFlip; set => _onFlip = value; }
    public static Action<IPlayer, double> FlipRoutine { get => _flipRoutine; set => _flipRoutine = value; }
    public static Action<IPlayer> OnStartWithStomp { get => _onStartWithStomp; set => _onStartWithStomp = value; }
}
