using System.Collections;
using UnityEngine;

public static class PlayerCoroutines
{
    public enum EagleCoroutines { Stomp, Dampen, JumpCountDelay, BoostTrail, EndFlip, DelayedFreeze, AddBoost }
    public static bool Stomping = false, CountingDownJump = false, CheckingForSecondJump = false, DelayedSecondJump = false;

    public static IEnumerator Stomp(IPlayer player)
    {
        player.Stomping = true;
        player.JumpCount = 2;
        Rigidbody2D rigidBody = player.Rigidbody;
        rigidBody.angularVelocity = 0;
        float originalRotationAccel = player.RotationAccel;
        player.RotationAccel *= 1.5f;
        float stompTimer = 0f;
        while (stompTimer < 0.075f)
        {
            rigidBody.velocity -= new Vector2(rigidBody.velocity.x * 0.1f, rigidBody.velocity.y * 0.4f);
            stompTimer += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }
        rigidBody.centerOfMass = new Vector2(0, -2f);
        while (rigidBody.velocity.y > player.StompSpeedLimit && !player.Collided)
        {
            rigidBody.velocity -= new Vector2(0, 0.15f * Mathf.Abs(rigidBody.velocity.y));
            rigidBody.velocity = new Vector2(rigidBody.velocity.x, Mathf.Clamp(rigidBody.velocity.y, player.StompSpeedLimit, -64));
            yield return new WaitForFixedUpdate();
        }
        yield return new WaitUntil(() => player.Collided);
        player.Stomping = false;
        yield return new WaitForSeconds(0.2f);
        player.TriggerBoost(player.FlipBoost, 1.8f);
        player.RotationAccel = originalRotationAccel;
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

    public static IEnumerator CheckForSecondJump(IPlayer player)
    {
        CheckingForSecondJump = true;
        float timer = 0;
        while (timer < 0.2f)
        {
            if (player.JumpCount == 2)
            {
                player.Rigidbody.AddForce(new Vector2(0, player.JumpForce * 300 * player.JumpMultiplier));
                break;
            }
            //if second jump, don't dampen first or second jump
            timer += Time.deltaTime;
            yield return null;
        }
        CheckingForSecondJump = false;
    }

    public static IEnumerator DelayedJump(IPlayer player, float delayTimeInSeconds)
    {
        yield return new WaitForSeconds(delayTimeInSeconds);
        player.Jump();
    }

    public static IEnumerator SlowToStop(IPlayer player)
    {
        Rigidbody2D rigidbody = player.Rigidbody;
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
            if (rigidbody.velocity.x < 10f && player.Animator.GetBool("OnBoard"))
            {
                player.Dismount();
            }
            yield return new WaitForFixedUpdate();
        }
        yield return new WaitForSeconds(0.5f);
        player.TriggerFinishStop();
    }


    public static IEnumerator DelayedJumpDampen(IPlayer player, float delayTimerInSeconds)
    {
        yield return new WaitForSeconds(delayTimerInSeconds);
        player.Rigidbody.AddForce(new Vector2(0, -player.JumpForce * 250 * player.JumpMultiplier));

    }

    public static IEnumerator EndFlip(IPlayer player, double spins)
    {
        yield return new WaitForSeconds(player.FlipDelay * 0.1f);
        float boostMultiplier = 1 + ((-1 / (float)spins) + 1);
        player.TriggerBoost(player.FlipBoost, boostMultiplier);
    }


    public static IEnumerator DelayedFreeze(IPlayer player, float timer)
    {
        yield return new WaitForSeconds(timer);
        player.Rigidbody.bodyType = RigidbodyType2D.Kinematic;
        player.Rigidbody.velocity = new Vector2(0, 0);
        player.Rigidbody.freezeRotation = true;
        player.Die();
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
