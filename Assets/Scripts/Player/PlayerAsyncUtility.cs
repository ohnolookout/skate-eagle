using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Threading;

public static class PlayerAsyncUtility
{
    private static float _maxTrailDuration = 0.08f;
    public static async void BoostTrail(CancellationToken token, TrailRenderer trail, bool facingForward)
    {
        Vector3 originalPosition = trail.transform.localPosition;
        if (!facingForward)
        {
            trail.transform.localPosition = new Vector3(-trail.transform.localPosition.x, trail.transform.localPosition.y);
        }

        float currentTime = 0;
        trail.time = currentTime;
        trail.emitting = true;
        while (currentTime < _maxTrailDuration)
        {
            if (token.IsCancellationRequested)
            {
                return;
            }
            currentTime += Time.deltaTime;
            trail.time = currentTime;
            await Task.Yield();
        }
        await Task.Delay(100);
        currentTime *= 2;
        while (currentTime > 0)
        {
            if (token.IsCancellationRequested)
            {
                return;
            }
            currentTime -= Time.deltaTime;
            trail.time = currentTime;
            await Task.Yield();
        }
        trail.emitting = false;
        trail.transform.localPosition = originalPosition;
    }
    public static async void AddBoost(CancellationToken token, Rigidbody2D body, float boostValue, float boostMultiplier)
    {
        float boostCount = 0;
        while (boostCount < 4)
        {
            if (token.IsCancellationRequested)
            {
                return;
            }
            boostCount++;
            body.AddForce(new Vector2(boostValue * boostMultiplier, 0));
            await Task.Yield();
        }
    }

    public static async void DelayedFunc(Action callback, float delayInSeconds)
    {
        await Task.Delay((int)delayInSeconds * 1000);
        callback();
    }

    public static async void DelayedFreeze(IPlayer player, float timer)
    {
        await Task.Delay((int)(timer * 1000));
        player.NormalBody.bodyType = RigidbodyType2D.Kinematic;
        player.NormalBody.velocity = new Vector2(0, 0);
        player.NormalBody.freezeRotation = true;
        player.Die();
    }

}
