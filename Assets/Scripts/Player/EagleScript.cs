using System.Collections;
using UnityEngine;
using System;

public class EagleScript : MonoBehaviour
{
    public Rigidbody2D rigidEagle;
    public Animator animator;
    [HideInInspector] public float rotationSpeed = 0;
    [HideInInspector] public float jumpForce = 40, downForce = 95, rotationAccel = 1200, minBoost = 0.6f, flipDelay = 0.75f, flipBoost = 70, stompSpeedLimit = -250;
    private float rotationStart = 0, jumpStartTime;
    private Vector2 lastSpeed;
    private int jumpCount = 0, jumpLimit = 2;
    private bool airborne = false, stomping = false, facingForward = true, crouched = false;
    private IEnumerator jumpCoroutine, stompCoroutine, trailCoroutine;
    public IEnumerator dampen;
    public LiveRunManager logic;
    public FlipTextGenerator textGen;
    public TrailRenderer trail;
    public PlayerController playerController;
    public PlayerCoroutines coroutines;
    public PlayerAudio playerAudio;
    [SerializeField] private CollisionTracker collisionTracker;


    private void Awake()
    {
        AssignComponents();
        rigidEagle.bodyType = RigidbodyType2D.Kinematic;
        rigidEagle.centerOfMass = new Vector2(0, -2f);
    }

    private void Start()
    {
        lastSpeed = rigidEagle.velocity;
    }

    private void AssignComponents()
    {
        logic = GameObject.FindGameObjectWithTag("Logic").GetComponent<LiveRunManager>();
        jumpCoroutine = coroutines.JumpCountDelay();
        stompCoroutine = coroutines.Stomp();
        dampen = coroutines.DampenLanding();
        trailCoroutine = coroutines.BoostTrail();
    }

    void Update()
    {

        if (logic.runState == RunState.Standby)
        {
            StartCheck();
        }
        if (logic.runState != RunState.Active)
        {
            return;
        }
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
            if (logic.StompCharge == logic.StompThreshold)
            {
                Stomp();
            }
        }
        DirectionCheck();
        FinishCheck();
        UpdateAnimatorParameters();
    }

    private void FixedUpdate()
    {
        if (logic.runState != RunState.Active)
        {
            return;
        }
        if (playerController.down && !stomping)
        {
            rigidEagle.AddForce(new Vector2(0, -downForce * 20));
        }

        rigidEagle.AddTorque(-rotationAccel * playerController.rotation.x);
        lastSpeed = rigidEagle.velocity;
    }

    public void Stomp()
    {
        stompCoroutine = coroutines.Stomp();
        StartCoroutine(stompCoroutine);
    }

    private float jumpDuration = 0.25f;
    private float minimumJumpDuration = 0.1f;
    public void JumpValidation()
    {
        if (jumpCount >= jumpLimit || logic.runState != RunState.Active)
        {
            return;
        }
        float timeSinceLastJump = Time.time - jumpStartTime;
        if (jumpCount > 0 && timeSinceLastJump < jumpDuration)
        {
            StartCoroutine(coroutines.DelayedJump(jumpDuration - timeSinceLastJump));
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
        collisionTracker.RemoveAllCollisions();
        if (rigidEagle.velocity.y < 0)
        {
            rigidEagle.velocity = new Vector2(rigidEagle.velocity.x, 0);
        }
        if (!airborne)
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
                StartCoroutine(coroutines.DelayedJumpDampen(scaledMinimumJumpDuration - timeChange));
            }
            if (jumpCount == 1)
            {
                StartCoroutine(coroutines.CheckForSecondJump());
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (logic.runState == RunState.Finished)
        {
            return;
        }
        collisionTracker.UpdateCollision(collision, true);
        if (coroutines.countingDownJump)
        {
            StopCoroutine(jumpCoroutine);
            coroutines.countingDownJump = false;
        }
        jumpCount = 0;
        if (airborne)
        {
            animator.SetFloat("forceDelta", ForceDelta);
            animator.SetTrigger("Land");
            StopCoroutine(dampen);
            dampen = coroutines.DampenLanding();
            StartCoroutine(dampen);
        }
        airborne = false;
        if (collision.otherCollider.name == "Skate Eagle")
        {
            Die();
            return;
        }

        FlipCheck();

    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        airborne = false;
        if (coroutines.countingDownJump)
        {
            StopCoroutine(jumpCoroutine);
            coroutines.countingDownJump = false;
        }
        if (logic.runState == RunState.GameOver)
        {
            rigidEagle.velocity *= 1 - (Time.deltaTime);
            return;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        collisionTracker.UpdateCollision(collision, false);
        rotationStart = rigidEagle.rotation;
        if (!coroutines.countingDownJump)
        {
            jumpCoroutine = coroutines.JumpCountDelay();
            StartCoroutine(jumpCoroutine);
        }
    }


    public void Die()
    {
        if (logic.runState == RunState.GameOver)
        {
            return;
        }
        StopCoroutine(trailCoroutine);
        trail.emitting = false;
        textGen.CancelText();
        logic.GameOver();
    }

    private void UpdateAnimatorParameters()
    {
        animator.SetFloat("Speed", rigidEagle.velocity.magnitude);
        animator.SetFloat("YSpeed", rigidEagle.velocity.y);
        animator.SetBool("FacingForward", facingForward);
        animator.SetBool("Airborne", airborne);
        animator.SetBool("Crouched", playerController.down);
        if (airborne)
        {
            animator.SetBool("AirborneUp", rigidEagle.velocity.y >= 0);
        }
    }


    public void SlowToStop()
    {
        playerAudio.Finish();
        animator.SetTrigger("Brake");
        StartCoroutine(coroutines.SlowToStop());
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
        facingForward = rigidEagle.velocity.x >= 0;
        if (lastDirection != facingForward)
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
        if (transform.position.x >= logic.FinishPoint.x && Collided)
        {
            logic.Finish();
        }
    }

    public void Fall()
    {
        StartCoroutine(coroutines.DelayedFreeze(0.5f));
        logic.runState = RunState.Fallen;
    }

    private void FlipCheck()
    {
        double spins = Math.Round(Math.Abs(rotationStart - rigidEagle.rotation) / 360);
        rotationStart = rigidEagle.rotation;
        if (spins >= 1 && logic.runState != RunState.GameOver)
        {
            textGen.NewFlipText(spins);
            StartCoroutine(coroutines.EndFlip(flipDelay, spins));
        }
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

    public bool Airborne
    {
        get
        {
            return airborne;
        }
        set
        {
            airborne = value;
        }
    }

    public bool Stomping
    {
        get
        {
            return stomping;
        }
        set
        {
            stomping = value;
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
            return rigidEagle.velocity;
        }
    }

    public Vector2 LastFrameDelta
    {
        get
        {
            return new Vector2(rigidEagle.velocity.x - lastSpeed.x, rigidEagle.velocity.y - lastSpeed.y);
        }
    }

    public float ForceDelta
    {
        get
        {
            Vector2 delta = LastFrameDelta;
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
    
}
