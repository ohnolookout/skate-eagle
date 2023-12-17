using System.Collections;
using UnityEngine;
using System;

public class EagleScript : MonoBehaviour
{
    public Rigidbody2D rigidEagle;
    public Animator animator;
    [HideInInspector] public float rotationSpeed = 0, jumpForce = 40, downForce = 95, rotationAccel = 1200, minBoost = 0.6f, flipDelay = 0.75f, flipBoost = 70, stompSpeedLimit = -250;
    private float rotationStart = 0, jumpStartTime;
    private Vector2 lastSpeed;
    private int jumpCount = 0, jumpLimit = 2, stompThreshold = 2;
    [SerializeField] private int stompCharge = 0;
    private bool facingForward = true, crouched = false;
    private IEnumerator trailCoroutine, dampen;
    public LiveRunManager logic;
    public FlipTextGenerator textGen;
    public TrailRenderer trail;
    public PlayerController playerController;
    public PlayerAudio playerAudio;
    public Action<EagleScript, int> EndFlip; //Pass in context and flipcount
    public Action<EagleScript> StompEvent;
    [SerializeField] private RagdollController ragdollController;
    [SerializeField] private CollisionTracker collisionTracker;
    [SerializeField] private Rigidbody2D ragdollBoard;


    private void Awake()
    {
        AssignComponents();
        rigidEagle.bodyType = RigidbodyType2D.Kinematic;
        rigidEagle.centerOfMass = new Vector2(0, -2f);
        logic.EnterAttempt += _ => StartAttempt();
        EndFlip += (_, flipCount) => DelayedFlipBoost(flipCount);
        StompEvent += _ => Stomp();
        logic.EnterGameOver += _ => StopTrail();
        logic.EnterFinish += _ => SlowToStop();
        logic.EnterFinish += _ => StopTrail();
    }

    private void Start()
    {
        lastSpeed = Rigidbody.velocity;
    }

    private void AssignComponents()
    {
        logic = GameObject.FindGameObjectWithTag("Logic").GetComponent<LiveRunManager>();
        dampen = PlayerCoroutines.DampenLanding(rigidEagle);
        trailCoroutine = PlayerCoroutines.BoostTrail(trail, DirectionForward);
    }

    void Update()
    {

        if (LiveRunManager.runState == RunState.Standby)
        {
            StartCheck();
        }
        if (LiveRunManager.runState != RunState.Active)
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
            if (StompCharge >= StompThreshold)
            {
                StompEvent?.Invoke(this);
            }
        }
        FinishCheck();
        if (!IsRagdoll)
        {
            UpdateAnimatorParameters();
        }
    }

    private void FixedUpdate()
    {
        if (LiveRunManager.runState != RunState.Active)
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
        StompCharge = 0;
        StartCoroutine(PlayerCoroutines.Stomp(this));
    }

    private float jumpDuration = 0.25f;
    private float minimumJumpDuration = 0.1f;
    public void JumpValidation()
    {
        if (jumpCount >= jumpLimit || LiveRunManager.runState != RunState.Active)
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
        playerAudio.Jump(jumpCount);
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
        if (IsRagdoll)
        {
            collisionTracker.UpdateCollision(collision, true);
            return;
        }
        if (collision.otherCollider.name == "Skate Eagle")
        {
            Die();
            collisionTracker.UpdateCollision(collision, true);
            return;
        }
        jumpCount = 0;
        if (!Collided)
        {
            animator.SetFloat("forceDelta", ForceDelta);
            Debug.Log("Landed!");
            animator.SetTrigger("Land");
            StopCoroutine(dampen);
            dampen = PlayerCoroutines.DampenLanding(rigidEagle);
            StartCoroutine(dampen);
        }
        collisionTracker.UpdateCollision(collision, true);
        FlipCheck();

    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        collisionTracker.UpdateCollision(collision, false);
        rotationStart = rigidEagle.rotation;
    }


    public void Die()
    {
        if (LiveRunManager.runState == RunState.GameOver)
        {
            return;
        }
        logic.GameOver();
    }

    private void StopTrail()
    {        
        StopCoroutine(trailCoroutine);
        trail.emitting = false;
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
        playerAudio.Finish();
        animator.SetTrigger("Brake");
        StartCoroutine(PlayerCoroutines.SlowToStop(this));
    }

    public void TriggerBoost(float boostValue, float boostMultiplier)
    {
        StartCoroutine(PlayerCoroutines.AddBoost(rigidEagle, boostValue, boostMultiplier));
        StartCoroutine(PlayerCoroutines.BoostTrail(trail, DirectionForward));
    }


    private void StartCheck()
    {
        if (playerController.down)
        {
            logic.StartAttempt();
        }
    }

    private void StartAttempt()
    {
        animator.SetBool("OnBoard", true);
        rigidEagle.bodyType = RigidbodyType2D.Dynamic;
        rigidEagle.velocity += new Vector2(15, 0);
    }

    private void DirectionCheck()
    {
        bool lastDirection = facingForward;
        facingForward = Rigidbody.velocity.x >= 0;
        if (lastDirection != facingForward && !IsRagdoll)
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
        playerAudio.Dismount();
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
        LiveRunManager.runState = RunState.Fallen;
    }

    private void FlipCheck()
    {
        int flipCount = (int)Math.Round(Math.Abs(rotationStart - rigidEagle.rotation) / 360);
        rotationStart = rigidEagle.rotation;
        if (flipCount >= 1 && LiveRunManager.runState != RunState.GameOver)
        {
            AddStompCharge(flipCount);
            EndFlip?.Invoke(this, flipCount);
        }
    }

    private void AddStompCharge(int flipCount)
    {
        if (StompCharge < StompThreshold)
        {
            StompCharge = Mathf.Clamp(flipCount + StompCharge, 0, StompThreshold);
        }
    }

    private void DelayedFlipBoost(int flipCount)
    {
        StartCoroutine(PlayerCoroutines.FlipBoost(this, flipDelay, flipCount));
    }

    public void GoAirborne()
    {
        jumpCount = Mathf.Max(1, jumpCount);
        StopCoroutine(dampen);
        rigidEagle.centerOfMass = new Vector2(0, 0f);
    }

    public void Ragdoll()
    {
        ragdollController.TurnOnRagdoll(VectorChange);
        logic.GameOver();
    }

    public bool DirectionForward
    {
        get
        {
            return facingForward;
        }
    }

    public bool Collided
    {
        get
        {
            return collisionTracker.Collided;
        }
    }


    public bool Stomping
    {
        get
        {
            return PlayerCoroutines.Stomping;
        }
        set
        {
            PlayerCoroutines.Stomping = value;
        }
    }

    public int JumpCount
    {
        get
        {
            return jumpCount;
        }
        set
        {
            if (value >= 0 && value <= 2)
            {
                jumpCount = value;
            }
        }
    }

    public Vector2 Velocity
    {
        get
        {
            return Rigidbody.velocity;
        }
    }

    public Vector2 VectorChange
    {
        get
        {
            return new(Rigidbody.velocity.x - lastSpeed.x, Rigidbody.velocity.y - lastSpeed.y);
        }
    }

    public float ForceDelta
    {
        get
        {
            Vector2 delta = VectorChange;
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
    }

    public Transform Transform
    {
        get
        {
            if (IsRagdoll)
            {
                return ragdollController.spine.transform;
            }
            return rigidEagle.transform;
        }
    }

    public Rigidbody2D Rigidbody
    {
        get
        {
            if (IsRagdoll)
            {
                return ragdollController.spine;
            }
            return rigidEagle;
        }
    }

    public bool IsRagdoll
    {
        get
        {
            return ragdollController.IsRagdoll;
        }
    }

    public PlayerAudio Audio
    {
        get
        {
            return playerAudio;
        }
    }
    
    public Rigidbody2D RagdollBoard
    {
        get
        {
            if (!IsRagdoll)
            {
                return null;
            }
            return ragdollBoard;
        }
    }

    public int StompCharge
    {
        get
        {
            return stompCharge;
        }
        set
        {
            stompCharge = value;
        }
    }

    public int StompThreshold
    {
        get
        {
            return stompThreshold;
        }
    }
}
