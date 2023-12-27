using System.Collections;
using UnityEngine;

public static class PlayerCoroutines
{
    public enum EagleCoroutines { Stomp, Dampen, JumpCountDelay, BoostTrail, EndFlip, DelayedFreeze, AddBoost }
    public static bool Stomping = false, CountingDownJump = false, CheckingForSecondJump = false, DelayedSecondJump = false;

    public static IEnumerator Stomp(EagleScript eagleScript)
    {
        eagleScript.Stomping = true;
        eagleScript.JumpCount = 2;
        Rigidbody2D rigidBody = eagleScript.rigidEagle;
        rigidBody.angularVelocity = 0;
        float originalRotationAccel = eagleScript.rotationAccel;
        eagleScript.rotationAccel *= 1.5f;
        float stompTimer = 0f;
        while (stompTimer < 0.075f)
        {
            rigidBody.velocity -= new Vector2(rigidBody.velocity.x * 0.1f, rigidBody.velocity.y * 0.4f);
            stompTimer += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }
        rigidBody.centerOfMass = new Vector2(0, -2f);
        while (rigidBody.velocity.y > eagleScript.stompSpeedLimit && !eagleScript.Collided)
        {
            rigidBody.velocity -= new Vector2(0, 0.15f * Mathf.Abs(rigidBody.velocity.y));
            rigidBody.velocity = new Vector2(rigidBody.velocity.x, Mathf.Clamp(rigidBody.velocity.y, eagleScript.stompSpeedLimit, -64));
            yield return new WaitForFixedUpdate();
        }
        yield return new WaitUntil(() => eagleScript.Collided);
        eagleScript.Stomping = false;
        yield return new WaitForSeconds(0.2f);
        eagleScript.TriggerBoost(eagleScript.flipBoost, 1.8f);
        eagleScript.rotationAccel = originalRotationAccel;
    }


    public static IEnumerator DampenLanding(Rigidbody2D rigidBody)
    {
        rigidBody.angularVelocity = Mathf.Clamp(rigidBody.angularVelocity, -300, 300);
        float timer = 0;
        int underThresholdCount = 0;
        float threshold = 180;
        while (timer < 0.2f && underThresholdCount < 2)
        {
            if (Mathf.Abs(rigidBody.angularVelocity) > 60)
            {
                rigidBody.angularVelocity *= 0.3f;
            }
            if (rigidBody.angularVelocity < threshold)
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
        rigidBody.centerOfMass = new Vector2(0, -2f);
    }


    public static IEnumerator BoostTrail(TrailRenderer trail, bool directionIsForward)
    {
        Vector3 originalPosition = trail.transform.localPosition;
        if (!directionIsForward)
        {
            trail.transform.localPosition = new Vector3(-trail.transform.localPosition.x, trail.transform.localPosition.y);
        }
        float maxTime = 0.08f;
        float currentTime = 0;
        trail.time = currentTime;
        trail.emitting = true;
        while (currentTime < maxTime)
        {
            currentTime += Time.deltaTime;
            trail.time = currentTime;
            yield return null;
        }
        yield return new WaitForSeconds(0.1f);
        while (currentTime > 0)
        {
            currentTime -= Time.deltaTime / 2;
            trail.time = currentTime;
            yield return null;
        }
        trail.emitting = false;
        trail.transform.localPosition = originalPosition;
    }

    public static IEnumerator CheckForSecondJump(EagleScript eagleScript)
    {
        CheckingForSecondJump = true;
        float timer = 0;
        while (timer < 0.2f)
        {
            if (eagleScript.JumpCount == 2)
            {
                eagleScript.rigidEagle.AddForce(new Vector2(0, eagleScript.jumpForce * 300 * eagleScript.jumpMultiplier));
                break;
            }
            //if second jump, don't dampen first or second jump
            timer += Time.deltaTime;
            yield return null;
        }
        CheckingForSecondJump = false;
    }

    public static IEnumerator DelayedJump(EagleScript eagleScript, float delayTimeInSeconds)
    {
        yield return new WaitForSeconds(delayTimeInSeconds);
        eagleScript.Jump();
    }

    public static IEnumerator SlowToStop(EagleScript eagleScript)
    {
        Rigidbody2D rigidbody = eagleScript.rigidEagle;
        int frameCount = 0;
        while (rigidbody.velocity.x > 0.1f)
        {
            frameCount++;
            if (Mathf.Abs(rigidbody.velocity.x) < 1 && Mathf.Abs(rigidbody.velocity.y) < 1)
            {
                rigidbody.velocity = new Vector2(0, 0);
            }
            else
            {
                rigidbody.velocity -= rigidbody.velocity * 0.08f;
            }
            if (rigidbody.velocity.x < 10f && eagleScript.animator.GetBool("OnBoard"))
            {
                eagleScript.Dismount();
            }
            yield return new WaitForFixedUpdate();
        }
        yield return new WaitForSeconds(0.5f);
        eagleScript.TriggerFinishStop();
    }


    public static IEnumerator DelayedJumpDampen(EagleScript eagleScript, float delayTimerInSeconds)
    {
        yield return new WaitForSeconds(delayTimerInSeconds);
        eagleScript.rigidEagle.AddForce(new Vector2(0, -eagleScript.jumpForce * 250 * eagleScript.jumpMultiplier));

    }

    public static IEnumerator EndFlip(EagleScript eagleScript, double spins)
    {
        yield return new WaitForSeconds(eagleScript.flipDelay * 0.1f);
        float boostMultiplier = 1 + ((-1 / (float)spins) + 1);
        eagleScript.TriggerBoost(eagleScript.flipBoost, boostMultiplier);
    }


    public static IEnumerator DelayedFreeze(EagleScript eagleScript, float timer)
    {
        yield return new WaitForSeconds(timer);
        eagleScript.rigidEagle.bodyType = RigidbodyType2D.Kinematic;
        eagleScript.rigidEagle.velocity = new Vector2(0, 0);
        eagleScript.rigidEagle.freezeRotation = true;
        eagleScript.Die();
    }

    public static IEnumerator AddBoost(Rigidbody2D rigidBody, float boostValue, float boostMultiplier)
    {
        float boostCount = 0;
        while (boostCount < 4)
        {
            boostCount++;
            rigidBody.AddForce(new Vector2(boostValue * boostMultiplier, 0));
            yield return new WaitForFixedUpdate();
        }
    }

}
