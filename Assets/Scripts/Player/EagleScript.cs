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
    private int jumpCount = 0, jumpLimit = 2;
    private bool facingForward = true, crouched = false, ragdoll = false;
    private IEnumerator stompCoroutine, trailCoroutine, dampen;
    public LiveRunManager logic;
    public FlipTextGenerator textGen;
    public TrailRenderer trail;
    public PlayerController playerController;
    public PlayerAudio playerAudio;
    [SerializeField] private RagdollController ragdollController;
    [SerializeField] private CollisionTracker collisionTracker;
    [SerializeField] private Rigidbody2D ragdollBoard;


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

    private void AssignComponents()
    {
        logic = GameObject.FindGameObjectWithTag("Logic").GetComponent<LiveRunManager>();
        stompCoroutine = PlayerCoroutines.Stomp(this);
        dampen = PlayerCoroutines.DampenLanding(rigidEagle);
        trailCoroutine = PlayerCoroutines.BoostTrail(trail, DirectionForward);
    }

    void Update()
    {

        if (logic.runState == RunState.Standby)
        {
            StartCheck();
        }
        DirectionCheck();
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
            if (logic.StompCharge >= logic.StompThreshold)
            {
                logic.StompCharge = 0;
                Stomp();
            }
        }
        FinishCheck();
        UpdateAnimatorParameters();
    }

    private void FixedUpdate()
    {
        if (logic.runState != RunState.Active)
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
        stompCoroutine = PlayerCoroutines.Stomp(this);
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
        if (logic.runState == RunState.Finished)
        {
            return;
        }
        jumpCount = 0;
        if (!Collided)
        {
            animator.SetFloat("forceDelta", ForceDelta);
            animator.SetTrigger("Land");
            StopCoroutine(dampen);
            dampen = PlayerCoroutines.DampenLanding(rigidEagle);
            StartCoroutine(dampen);
        }
        collisionTracker.UpdateCollision(collision, true);
        if (collision.otherCollider.name == "Skate Eagle")
        {
            Die();
            return;
        }

        FlipCheck();

    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        /*
         * UNCOMMENT IF LOSING SECOND JUMP
        if (coroutines.countingDownJump)
        {
            StopCoroutine(jumpCoroutine);
            coroutines.countingDownJump = false;
        }*/
        /*
        if (logic.runState == RunState.GameOver)
        {
            Rigidbody.velocity *= 0.1f * Time.deltaTime;
            return;
        }
        */
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        collisionTracker.UpdateCollision(collision, false);
        rotationStart = rigidEagle.rotation;
    }


    public void Die()
    {
        if (logic.runState == RunState.GameOver)
        {
            return;
        }
        collisionTracker.RemoveNonragdollColliders();
        StopCoroutine(trailCoroutine);
        trail.emitting = false;
        textGen.CancelText();
        ragdollController.TurnOnRagdoll(VectorChange);
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
        StartCoroutine(PlayerCoroutines.DelayedFreeze(this, 0.5f));
        logic.runState = RunState.Fallen;
    }

    private void FlipCheck()
    {
        double spins = Math.Round(Math.Abs(rotationStart - rigidEagle.rotation) / 360);
        rotationStart = rigidEagle.rotation;
        if (spins >= 1 && logic.runState != RunState.GameOver)
        {
            textGen.NewFlipText(spins);
            StartCoroutine(PlayerCoroutines.EndFlip(this, flipDelay, spins));
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
        ragdollController.TurnOnRagdoll(VectorChange);
        ragdoll = true;
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
            if (ragdoll)
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
            if (ragdoll)
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
            return ragdoll;
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
}
