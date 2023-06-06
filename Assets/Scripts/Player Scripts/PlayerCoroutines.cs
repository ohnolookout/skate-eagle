using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCoroutines : MonoBehaviour
{
    private EagleScript eagleScript;
    private LogicScript logic;
    private Rigidbody2D rigidEagle;
    private GameObject eagle;
    private GameObject boostTrail;
    private TrailRenderer trail;
    public bool dampening = false, checkingForSecondJump = false, delayedDampenJump = false;
    public enum EagleCoroutines { Stomp, Dampen, JumpCountDelay, BoostTrail, EndFlip, DelayedFreeze, AddBoost }
    void Start()
    {
        eagle = GameObject.FindGameObjectWithTag("Player");
        rigidEagle = eagle.GetComponent<Rigidbody2D>();
        eagleScript = eagle.GetComponent<EagleScript>();
        logic = GameObject.FindGameObjectWithTag("Logic").GetComponent<LogicScript>();
        boostTrail = eagleScript.boostTrail;
        trail = boostTrail.GetComponent<TrailRenderer>();
    }

    public IEnumerator Stomp()
    {
        if (logic.StompCharge < logic.StompThreshold)
        {
            yield break;
        }
        eagleScript.Stomping = true;
        eagleScript.CurrentSprite = EagleScript.SpriteState.Dive;
        eagleScript.JumpCount = 2;
        logic.StompCharge = 0;
        rigidEagle.angularVelocity = 0;
        float originalRotationAccel = eagleScript.rotationAccel;
        eagleScript.rotationAccel *= 1.5f;
        float stompTimer = 0f;
        while (stompTimer < 0.075f)
        {
            rigidEagle.velocity -= new Vector2(rigidEagle.velocity.x * 0.1f, rigidEagle.velocity.y * 0.4f);
            stompTimer += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }
        rigidEagle.centerOfMass = new Vector2(0, -2f);
        while (rigidEagle.velocity.y > eagleScript.stompSpeedLimit && !eagleScript.Collided)
        {
            rigidEagle.velocity -= new Vector2(0, 0.15f * Mathf.Abs(rigidEagle.velocity.y));
            rigidEagle.velocity = new Vector2(rigidEagle.velocity.x, Mathf.Clamp(rigidEagle.velocity.y, eagleScript.stompSpeedLimit, -64));
            yield return new WaitForFixedUpdate();
        }
        yield return new WaitUntil(() => eagleScript.Collided);
        eagleScript.Stomping = false;
        eagleScript.CurrentSprite = EagleScript.SpriteState.Default;
        yield return new WaitForSeconds(0.2f);
        StartCoroutine(AddBoost(eagleScript.flipBoost, 1.8f));
        eagleScript.rotationAccel = originalRotationAccel;
    }


    public IEnumerator DampenLanding()
    {
        rigidEagle.angularVelocity = Mathf.Clamp(rigidEagle.angularVelocity, -300, 300);
        float timer = 0;
        int underThresholdCount = 0;
        float threshold = 180;
        dampening = true;
        while (timer < 0.2f && underThresholdCount < 2)
        {
            if (Mathf.Abs(rigidEagle.angularVelocity) > 60)
            {
                rigidEagle.angularVelocity *= 0.3f;
            }
            if (rigidEagle.angularVelocity < threshold)
            {
                underThresholdCount++;
            }
            else
            {
                underThresholdCount = 0;
            }
            timer += Time.deltaTime;
            yield return new WaitForFixedUpdate();

        }
        rigidEagle.centerOfMass = new Vector2(0, -2f);
        dampening = false;
    }


    public IEnumerator JumpCountDelay()
    {
        yield return new WaitForSeconds(0.2f);
        rigidEagle.centerOfMass = new Vector2(0, 0f);
        eagleScript.Airborne = true;
        StopCoroutine(eagleScript.dampen);
        if (eagleScript.JumpCount < 1)
        {
            eagleScript.JumpCount = 1;
        }
    }

    public IEnumerator BoostTrail()
    {
        Vector3 originalPosition = boostTrail.transform.localPosition;
        if (!eagleScript.DirectionForward)
        {
            boostTrail.transform.localPosition = new Vector3(-boostTrail.transform.localPosition.x, boostTrail.transform.localPosition.y);
        }
        float maxTime = 0.08f;
        float currentTime = 0;
        trail.time = currentTime;
        trail.emitting = true;
        while (currentTime < maxTime)
        {
            if (logic.Dead)
            {
                trail.emitting = false;
            }
            currentTime += Time.deltaTime;
            trail.time = currentTime;
            yield return null;
        }
        yield return new WaitForSeconds(0.1f);
        while (currentTime > 0)
        {
            if (logic.Dead)
            {
                trail.emitting = false;
            }
            currentTime -= Time.deltaTime / 2;
            trail.time = currentTime;
            yield return null;
        }
        trail.emitting = false;
        boostTrail.transform.localPosition = originalPosition;
    }

    public IEnumerator CheckForSecondJump()
    {
        checkingForSecondJump = true;
        float timer = 0;
        while (timer < 0.2f)
        {
            if (eagleScript.JumpCount == 2 && !delayedDampenJump)
            {
                rigidEagle.AddForce(new Vector2(0, eagleScript.jumpForce * 300 * eagleScript.jumpMultiplier));
                break;
            }
            //if second jump, don't dampen first or second jump
            timer += Time.deltaTime;
            yield return null;
        }
        checkingForSecondJump = false;
    }

    public IEnumerator DelayedJump(float delayTimeInSeconds)
    {
        yield return new WaitForSeconds(delayTimeInSeconds);
        eagleScript.Jump();
    }


    public IEnumerator DelayedJumpDampen(float delayTimerInSeconds)
    {
        yield return new WaitForSeconds(delayTimerInSeconds);
        rigidEagle.AddForce(new Vector2(0, -eagleScript.jumpForce * 250 * eagleScript.jumpMultiplier));

    }

    public IEnumerator EndFlip(float flipDelay, double spins)
    {
        yield return new WaitForSeconds(flipDelay * 0.1f);
        if (logic.Dead)
        {
            yield break;
        }
        if (logic.StompCharge < logic.StompThreshold && !logic.Finished)
        {
            logic.StompCharge += (int)spins;
        }
        float boostMultiplier = 1 + ((-1 / (float)spins) + 1);
        StartCoroutine(AddBoost(eagleScript.flipBoost, boostMultiplier));
    }

    public IEnumerator DelayedFreeze(float timer)
    {
        yield return new WaitForSeconds(timer);
        rigidEagle.bodyType = RigidbodyType2D.Kinematic;
        rigidEagle.velocity = new Vector2(0, 0);
        rigidEagle.freezeRotation = true;
        eagleScript.Die();
    }

    public IEnumerator AddBoost(float boostValue, float boostMultiplier)
    {
        StartCoroutine(BoostTrail());
        float boostCount = 0;
        while (boostCount < 4)
        {
            boostCount++;
            rigidEagle.AddForce(new Vector2(boostValue * boostMultiplier, 0));
            yield return new WaitForFixedUpdate();
        }
    }

    public bool stomping = false;

}
