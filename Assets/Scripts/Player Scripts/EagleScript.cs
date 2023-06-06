using System.Collections;
using UnityEngine;
using System;

public class EagleScript : MonoBehaviour
{
    private Rigidbody2D rigidEagle;
    private SpriteRenderer spriteRenderer;
    public float jumpForce = 35, downForce = 80, rotationAccel = 40000, rotationSpeed = 0, stallLimit = 3, 
        stallVelocity = 2.2f, minBoost = 5, flipDelay = 1f, dashLength = 10, flipBoost = 60, stompSpeedLimit = -400;
    private float stallCount = 0, rotationStart = 0, jumpStartTime;
    private int jumpCount = 0, jumpLimit = 2;
    private bool dead = false, fallen = false, airborne = false, collided = true, isActive = false, stomping = false;
    private IEnumerator jumpCoroutine, stompCoroutine, trailCoroutine;
    public IEnumerator dampen;
    public LogicScript logic;
    public Canvas textGenerator;
    private FlipTextGenerator textGen;
    public Sprite defaultSprite, crouchSprite, deadSprite, jumpSprite, diveSprite;
    public GameObject boostTrail;
    private TrailRenderer trail;
    private PlayerController playerController;
    public enum SpriteState { Default, Crouch, Dive, Dead, Jump}
    private SpriteState currentSprite = SpriteState.Default;
    private PlayerCoroutines coroutines;


    private void Awake()
    {
        AssignComponents();
        CurrentSprite = SpriteState.Default;
        rigidEagle.bodyType = RigidbodyType2D.Kinematic;
        rigidEagle.centerOfMass = new Vector2(0, -2f);
    }

    private void Start()
    {
    }


    void Update()
    {
        if (!PlayerIsActive())
        {
            isActive = false;
            return;
        }
        
        if (playerController.down)
        {
            CurrentSprite = SpriteState.Crouch;
        }
        else
        {
            CurrentSprite = SpriteState.Default;
        }
        if (playerController.stomp)
        {
            playerController.stomp = false;
            if (logic.StompCharge == logic.StompThreshold)
            {
                Stomp();
            }
        }
        StallCheck();
        DirectionCheck();
        FinishCheck();
    }
    
    private void FixedUpdate() 
    {
        if (!isActive)
        {
            return;
        }
        if (playerController.down && !stomping)
        {
            rigidEagle.AddForce(new Vector2(0, -downForce * 20));
        }
        
        rigidEagle.AddTorque(-rotationAccel * playerController.rotation.x);
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
        if (jumpCount >= jumpLimit || !isActive)
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

    public float jumpMultiplier = 1;
    public void Jump()
    {
        jumpMultiplier = 1 - (jumpCount * 0.25f);
        jumpStartTime = Time.time;
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
            if(timeChange > scaledMinimumJumpDuration)
            {
                rigidEagle.AddForce(new Vector2(0, -jumpForce * 250 * jumpMultiplier));
            }
            else
            {
                StartCoroutine(coroutines.DelayedJumpDampen(scaledMinimumJumpDuration - timeChange));
            }
            if(jumpCount == 1)
            {
                StartCoroutine(coroutines.CheckForSecondJump());
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (logic.Finished)
        {
            return;
        }
        collided = true;
        StopCoroutine(jumpCoroutine);
        jumpCount = 0;
        if (airborne)
        {
            StopCoroutine(dampen);
            dampen = coroutines.DampenLanding();
            StartCoroutine(dampen);
        }
        airborne = false;
        if(collision.otherCollider.name != "SkateboardCollider")
        {
            Die();
            return;
        }

        FlipCheck();
        
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        collided = true;
        airborne = false;
        StopCoroutine(jumpCoroutine);
        if (dead)
        {
            rigidEagle.velocity *= 1 - (Time.deltaTime);
            return;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        collided = false;
        rotationStart = rigidEagle.rotation;
        jumpCoroutine = coroutines.JumpCountDelay();
        StartCoroutine(jumpCoroutine);
    }


    public void Die()
    {
        dead = true;
        StopCoroutine(trailCoroutine);
        trail.emitting = false;
        textGen.CancelText();
        logic.GameOver();
    }
    



    private void SlowToStop()
    {
        if (Mathf.Abs(rigidEagle.velocity.x) < 1 && Mathf.Abs(rigidEagle.velocity.y) < 1)
        {
            rigidEagle.velocity *= 0;
        }
        else
        {
            rigidEagle.velocity -= rigidEagle.velocity * 4 * Time.deltaTime;
        }
    }

    
    private void StartCheck()
    {
        if (playerController.down)
        {
            logic.StartAttempt();
            spriteRenderer.sprite = crouchSprite;
            rigidEagle.bodyType = RigidbodyType2D.Dynamic;
            rigidEagle.velocity += new Vector2(15, 0);
        }
    }

    private void DirectionCheck()
    {
        if (rigidEagle.velocity.x < 0) spriteRenderer.flipX = true;
        else spriteRenderer.flipX = false;
    }

    private void FinishCheck()
    {
        if (transform.position.x >= logic.FinishPoint.x && collided)
        {
            logic.Finish();
            spriteRenderer.sprite = defaultSprite;
        }
    }

    private void StompCheck()
    {
        if (playerController.stomp && !collided)
        {
            stompCoroutine = coroutines.Stomp();
            playerController.stomp = false;
            StartCoroutine(stompCoroutine);
        }
    }

    private void StallCheck()
    {
        if (Math.Abs(rigidEagle.velocity.x) <= stallVelocity && Math.Abs(rigidEagle.velocity.y) <= stallVelocity)
        {
            stallCount += Time.deltaTime;
            if (stallCount > stallLimit)
            {
                rigidEagle.velocity = new Vector2(rigidEagle.velocity.x + jumpForce, rigidEagle.velocity.y);
            }
        }
        else
        {
            stallCount = 0;
        }
    }

    private bool PlayerIsActive()
    {
        if (!logic.Started)
        {
            StartCheck();
            return false ;
        }

        if (fallen)
        {
            StartCoroutine(coroutines.DelayedFreeze(0.5f));
            return false;
        }

        if (dead)
        {
            return false;
        }


        if (logic.Finished)
        {
            SlowToStop();
            return false;
        }
        if (!isActive)
        {
            isActive = true;
        }
        return true;
    }

    private void FlipCheck()
    {
        double spins = Math.Round(Math.Abs(rotationStart - rigidEagle.rotation) / 360);
        rotationStart = rigidEagle.rotation;
        if (spins >= 1 && !dead)
        {
            textGen.NewFlipText(spins);
            StartCoroutine(coroutines.EndFlip(flipDelay, spins));
        }
    }
    public bool Fallen
    {
        get
        {
            return fallen;
        }
        set
        {
            fallen = value;
        }
    }

    public SpriteState CurrentSprite
    {
        get
        {
            return currentSprite;
        }
        set
        {
            if (stomping && value != SpriteState.Dive)
            {
                return;
            }
            if (value == CurrentSprite)
            {
                return;
            }
            currentSprite = value;
            switch (value)
            {
                case SpriteState.Default:
                    spriteRenderer.sprite = defaultSprite;
                    break;
                case SpriteState.Crouch:
                    spriteRenderer.sprite = crouchSprite;
                    break;
                case SpriteState.Dive:
                    spriteRenderer.sprite = diveSprite;
                    break;
                case SpriteState.Dead:
                    spriteRenderer.sprite = deadSprite;
                    break;
                case SpriteState.Jump:
                    spriteRenderer.sprite = jumpSprite;
                    break;
            }
        }
    }

    public bool DirectionForward
    {
        get
        {
            return !spriteRenderer.flipX;
        }
    }

    public bool Collided
    {
        get
        {
            return collided;
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
            if(value >= 0 && value <= 2)
            {
                jumpCount = value;
            }
        }
    }
    private void AssignComponents()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        textGen = textGenerator.GetComponent<FlipTextGenerator>();
        logic = GameObject.FindGameObjectWithTag("Logic").GetComponent<LogicScript>();
        rigidEagle = GetComponent<Rigidbody2D>();
        coroutines = GetComponent<PlayerCoroutines>();
        jumpCoroutine = coroutines.JumpCountDelay();
        stompCoroutine = coroutines.Stomp();
        dampen = coroutines.DampenLanding();
        trail = boostTrail.GetComponent<TrailRenderer>();
        trailCoroutine = coroutines.BoostTrail();
        playerController = GetComponent<PlayerController>();
    }
}
