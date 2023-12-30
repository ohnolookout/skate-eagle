using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;
using System.Linq;
using System;

public enum OneShotFX { Jump, SecondJump, Wheel, Board, Body, HardBody};
public enum LoopFX { Roll, Freewheel, Board, Body };


public class PlayerAudio : MonoBehaviour
{
    [SerializeField]
    private SerializedDictionary<OneShotFX, Sound> oneShotDict = new();
    [SerializeField]
    private SerializedDictionary<LoopFX, Sound> loopDict = new();
    private bool wheelsOnGround = false;
    [SerializeField]
    private EagleScript eagleScript;
    [SerializeField]
    private CollisionTracker collisionTracker;
    private float wheelTimer = -1, wheelTimeLimit = 0.2f, wheelFadeCoefficient = 0.01f;
    private AudioManager audioManager;
    private Action<FinishScreenData> onFinish; 

    private void Awake()
    {
        List<Sound> sounds = oneShotDict.Values.ToList();
        sounds.AddRange(loopDict.Values.ToList());
        audioManager = AudioManager.Instance;
        audioManager.BuildModifierDict(TrackedBodies(sounds));
    }

    private void OnEnable()
    {
        collisionTracker.OnCollide += Collide;
        collisionTracker.OnUncollide += Uncollide;
        eagleScript.OnJump += Jump;
        eagleScript.OnDismount += Dismount;
        eagleScript.OnSlowToStop += SlowToStop;
    }
    private void OnDisable()
    {
        collisionTracker.OnCollide -= Collide;
        collisionTracker.OnUncollide -= Uncollide;
        eagleScript.OnJump -= Jump;
        eagleScript.OnDismount -= Dismount;
        eagleScript.OnSlowToStop -= SlowToStop;
    }
    void FixedUpdate()
    {
        UpdateWheelTimer();
    }

    private Rigidbody2D[] TrackedBodies(List<Sound> sounds)
    {
        List<Rigidbody2D> bodies = new();
        foreach(var sound in sounds)
        {
            if (!bodies.Contains(sound.localizedSource))
            {
                bodies.Add(sound.localizedSource);
            }
        }
        return bodies.ToArray();
    }

    public void Jump(EagleScript player)        
    {
        if (player.JumpCount == 0)
        {
            audioManager.PlayOneShot(oneShotDict[OneShotFX.Jump]);
            wheelTimer = 0;
            if (!AudioManager.playingSounds.ContainsValue(loopDict[LoopFX.Freewheel]))
            {
                audioManager.StopLoop(loopDict[LoopFX.Roll]);
                audioManager.StartLoop(loopDict[LoopFX.Freewheel], WheelFadeTime(player.Velocity.magnitude));
            }
        }
        else
        {
            audioManager.PlayOneShot(oneShotDict[OneShotFX.SecondJump]);
        }
        wheelsOnGround = false;
    }
    public void SlowToStop(EagleScript player)
    {
        wheelsOnGround = true;
        float stopDuration = AudioUtility.StopDuration(player.Velocity.x);
        StartCoroutine(SpikeIntensityDenom(stopDuration, 5));
        audioManager.StartLoop(loopDict[LoopFX.Board]);

    }

    public void Dismount()
    {
        audioManager.PlayOneShot(oneShotDict[OneShotFX.Jump]);
        audioManager.ClearLoops();
    }

    public void Collide(ColliderCategory colliderName, float magnitudeDelta)
    {
        switch (colliderName)
        {
            case ColliderCategory.LWheel:
                WheelCollision();
                break;
            case ColliderCategory.RWheel:
                WheelCollision();
                break;
            case ColliderCategory.Board:
                BoardCollision();
                break;
            case ColliderCategory.Body:
                BodyCollision(magnitudeDelta);
                break;
        }
    }

    public void Uncollide(ColliderCategory colliderName, float magnitudeAtCollisionExit)
    {
        switch (colliderName)
        {
            case ColliderCategory.LWheel:
                WheelExit(magnitudeAtCollisionExit);
                break;
            case ColliderCategory.RWheel:
                WheelExit(magnitudeAtCollisionExit);
                break;
            case ColliderCategory.Board:
                BoardExit();
                break;
            case ColliderCategory.Body:
                BodyExit();
                break;
        }
    }

    public void WheelCollision()
    {
        if (!wheelsOnGround && wheelTimer < 0)
        {
            audioManager.PlayOneShot(oneShotDict[OneShotFX.Wheel]);
            wheelsOnGround = true;
            audioManager.StopLoop(loopDict[LoopFX.Freewheel]);
            audioManager.StartLoop(loopDict[LoopFX.Roll]);
            wheelTimer = 0;
        }
    }

    public void BodyCollision(float magnitudeDelta)
    {
        if(magnitudeDelta > 140)
        {
            audioManager.PlayOneShot(oneShotDict[OneShotFX.HardBody]);
        }
        else
        {
            audioManager.PlayOneShot(oneShotDict[OneShotFX.Body]);
        }
        audioManager.StartLoop(loopDict[LoopFX.Body]);
    }

    public void BoardCollision()
    {
        audioManager.PlayOneShot(oneShotDict[OneShotFX.Board]);
        audioManager.StartLoop(loopDict[LoopFX.Board]);
    }

    public void WheelExit(float magnitudeAtCollisionExit)
    {
        if (collisionTracker.WheelsCollided)
        {
            return;
        }
        if (!AudioManager.playingSounds.ContainsValue(loopDict[LoopFX.Freewheel]))
        {
            audioManager.StopLoop(loopDict[LoopFX.Roll]);
            audioManager.StartLoop(loopDict[LoopFX.Freewheel], WheelFadeTime(magnitudeAtCollisionExit));
        }
        wheelsOnGround = false;
    }

    public void BoardExit()
    {
        audioManager.StopLoop(loopDict[LoopFX.Board]);
    }

    public void BodyExit()
    {
        audioManager.StopLoop(loopDict[LoopFX.Body]);
    }

    private IEnumerator SpikeIntensityDenom(float duration, float denomMultiplier)
    {
        float accelDuration = duration * 0.05f;
        float holdDuration = duration * 0.45f;
        float decelDuration = duration * 0.5f;
        float timeElapsed = 0;
        float startDenom = AudioManager.intensityDenominator;
        float floor = AudioManager.intensityDenominator /denomMultiplier;
        float shelf = startDenom;
        while (timeElapsed < accelDuration)
        {
            timeElapsed += Time.deltaTime;
            AudioManager.intensityDenominator = Mathf.Lerp(startDenom, floor, AudioUtility.EaseInOut(timeElapsed / accelDuration));
            yield return null;
        }
        timeElapsed = 0;
        yield return new WaitForSeconds(holdDuration);
        while (timeElapsed < decelDuration)
        {
            timeElapsed += Time.deltaTime;
            AudioManager.intensityDenominator = Mathf.Lerp(floor, shelf, AudioUtility.EaseOut(timeElapsed / decelDuration));
            yield return null;
        }
    }

    public void UpdateWheelTimer()
    {
        if(wheelTimer < 0)
        {
            return;
        }
        wheelTimer += Time.deltaTime;
        if (wheelTimer >= wheelTimeLimit)
        {
            wheelTimer = -1;
        }
    }

    private float WheelFadeTime(float magnitudeAtCollisionExit)
    {
        return wheelFadeCoefficient * magnitudeAtCollisionExit;
    }

}
