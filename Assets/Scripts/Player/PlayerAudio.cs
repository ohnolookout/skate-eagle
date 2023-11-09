using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;

public enum OneShotFX { Jump, SecondJump, Wheel, Board, Body, HardBody}
public enum LoopFX { Roll, Freewheel, Board, Body, Wind };
public class PlayerAudio : MonoBehaviour
{
    [SerializeField]
    private SerializedDictionary<OneShotFX, Sound> oneShotDict = new();
    [SerializeField]
    private SerializedDictionary<LoopFX, Sound> loopDict = new();
    [SerializeField]
    private AudioSource wheelBodyLoopSource, windBoardLoopSource, oneShotSource;
    private bool wheelsOnGround = false, fadingRoll = false;
    [SerializeField]
    private EagleScript eagleScript;
    private IEnumerator rollFade;
    private float rollIntensity = 0.4f, airborneFadeTime = 2f, intensityDenominator = 300f;
    [SerializeField]
    private LiveRunManager runManager;
    [SerializeField]
    private CollisionTracker collisionTracker;
    private Dictionary<AudioSource, LoopFX> currentLoopDict = new();
    private float wheelTimer = -1;
    private float wheelTimeLimit = 0.2f;

    private void Awake()
    {
        runManager = GameObject.FindGameObjectWithTag("Logic").GetComponent<LiveRunManager>();
        currentLoopDict[wheelBodyLoopSource] = LoopFX.Roll;
        currentLoopDict[windBoardLoopSource] = LoopFX.Wind;
    }

    void Start()
    {
        rollFade = AudioManager.FadeAudioSource(wheelBodyLoopSource, 0, airborneFadeTime * (eagleScript.Velocity.magnitude / intensityDenominator));
        StartLoop(wheelBodyLoopSource, LoopFX.Roll);
        wheelBodyLoopSource.volume = 0;
    }

    void FixedUpdate()
    {
        if ((int)runManager.runState < 2)
        {
            return;
        }
        //rollIntensity ranges from -1 to 1
        rollIntensity = -1 + Mathf.Clamp(eagleScript.Velocity.magnitude / intensityDenominator, 0, 2);
        //Calculate loop sources' volumes such that the current clips' volume sits at the center of possible ranges.        
        if (!fadingRoll)
        {
            wheelBodyLoopSource.volume = loopDict[currentLoopDict[wheelBodyLoopSource]].AdjustedVolume(rollIntensity);
            wheelBodyLoopSource.pitch = loopDict[currentLoopDict[wheelBodyLoopSource]].AdjustedPitch(rollIntensity);
        }
        windBoardLoopSource.volume = loopDict[currentLoopDict[windBoardLoopSource]].AdjustedVolume(rollIntensity);
        windBoardLoopSource.pitch = loopDict[currentLoopDict[windBoardLoopSource]].AdjustedPitch(rollIntensity);
        UpdateWheelTimer();
    }
    public void PlayOneShot(OneShotFX effectName)
    {
#if UNITY_EDITOR
        if (!oneShotDict.ContainsKey(effectName))
        {
            Debug.LogWarning($"Effect {effectName} not found!");
            return;
        }
#endif
        OneShotFromClip(oneShotDict[effectName]);
    }

    public void OneShotFromClip(Sound sound)
    {
        //Adjust source volume to use volume adjusted by intensity only if variance is > 0
        if (sound.volumeVariance == 0)
        {
            oneShotSource.volume = sound.volume;
        }
        else
        {
            float intensity = -1 + Mathf.Clamp(eagleScript.ForceDelta / 100, 0, 2);
            oneShotSource.volume = 0.1f + sound.AdjustedVolume(intensity);
        }
        //One shots don't have pitch variance.
        oneShotSource.pitch = sound.pitch;
        oneShotSource.PlayOneShot(sound.Clip());
    }

    public void Jump(float jumpCount)
    {
        if (jumpCount == 0)
        {
            PlayOneShot(OneShotFX.Jump);
            wheelTimer = 0;
        }
        else
        {
            PlayOneShot(OneShotFX.SecondJump);
        }
        if (currentLoopDict[wheelBodyLoopSource] != LoopFX.Freewheel)
        {
            StartLoop(wheelBodyLoopSource, LoopFX.Freewheel);
        }
        StopLoop(windBoardLoopSource);
        wheelsOnGround = false;
    }


    public void Finish()
    {
        fadingRoll = false;
        wheelsOnGround = true;
        StopCoroutine(rollFade);
        float stopDuration = StopDuration(eagleScript.Velocity.x);
        StartCoroutine(SpikeIntensityDenom(stopDuration, 5));
        StartLoop(windBoardLoopSource, LoopFX.Board);

    }

    public void Dismount()
    {
        StopLoop(wheelBodyLoopSource);
        StopLoop(windBoardLoopSource);
        PlayOneShot(OneShotFX.Jump);
    }

    public void Collide(PlayerCollider colliderName)
    {
        switch (colliderName)
        {
            case PlayerCollider.LWheel:
                WheelCollision();
                break;
            case PlayerCollider.RWheel:
                WheelCollision();
                break;
            case PlayerCollider.Board:
                BoardCollision();
                break;
            case PlayerCollider.Body:
                BodyCollision();
                break;
        }
    }

    public void Uncollide(PlayerCollider colliderName)
    {
        switch (colliderName)
        {
            case PlayerCollider.LWheel:
                WheelExit();
                break;
            case PlayerCollider.RWheel:
                WheelExit();
                break;
            case PlayerCollider.Board:
                BoardExit();
                break;
            case PlayerCollider.Body:
                BodyExit();
                break;
        }
    }

    public void WheelCollision()
    {
        if (!wheelsOnGround && wheelTimer < 0)
        {
            PlayOneShot(OneShotFX.Wheel);
            wheelsOnGround = true;
            StartLoop(wheelBodyLoopSource, LoopFX.Roll);
            wheelTimer = 0;
        }
    }

    public void BodyCollision()
    {
        if(eagleScript.ForceDelta > 120)
        {
            PlayOneShot(OneShotFX.HardBody);
        }
        else
        {
            PlayOneShot(OneShotFX.Body);
        }
        StartLoop(wheelBodyLoopSource, LoopFX.Body);
    }

    public void BoardCollision()
    {
        PlayOneShot(OneShotFX.Board);
        StartLoop(windBoardLoopSource, LoopFX.Board);
    }

    public void WheelExit()
    {
        if (!collisionTracker.WheelsCollided)
        {
            StartLoop(wheelBodyLoopSource, LoopFX.Freewheel);
            wheelsOnGround = false;
        }
    }

    public void BoardExit()
    {
        StopLoop(windBoardLoopSource);
    }

    public void BodyExit()
    {
        if (collisionTracker.WheelsCollided)
        {
            StartLoop(wheelBodyLoopSource, LoopFX.Roll);
        }
        else
        {
            StartLoop(wheelBodyLoopSource, LoopFX.Freewheel);
        }
    }

    private void StartLoop(AudioSource source, LoopFX loopName)
    {
        if (source == wheelBodyLoopSource && loopName != LoopFX.Freewheel)
        {
            fadingRoll = false;
            StopCoroutine(rollFade);
        }
        source.clip = loopDict[loopName].Clip();
        source.volume = loopDict[loopName].AdjustedVolume(rollIntensity);
        source.pitch = loopDict[loopName].AdjustedPitch(rollIntensity);
        source.Play();
        currentLoopDict[source] = loopName;
        if (loopName == LoopFX.Freewheel)
        {
            StopCoroutine(rollFade);
            rollFade = AudioManager.FadeAudioSource(source, 0, airborneFadeTime * ((rollIntensity + 1) * 2));
            StartCoroutine(rollFade);
            fadingRoll = true;
        }
    }

    private void StopLoop(AudioSource source)
    {
        source.Stop();
    }

    private IEnumerator SpikeIntensityDenom(float duration, float denomMultiplier)
    {
        float startDuration = duration * 0f;
        float accelDuration = duration * 0.05f;
        float holdDuration = duration * 0.45f;
        float decelDuration = duration * 0.5f;
        float timeElapsed = 0;
        float startDenom = intensityDenominator;
        float floor = intensityDenominator/denomMultiplier;
        float shelf = startDenom;
        yield return new WaitForSeconds(startDuration);
        while (timeElapsed < accelDuration)
        {
            timeElapsed += Time.deltaTime;
            intensityDenominator = Mathf.Lerp(startDenom, floor, EaseInOut(timeElapsed / accelDuration));
            yield return null;
        }
        timeElapsed = 0;
        yield return new WaitForSeconds(holdDuration);
        while (timeElapsed < decelDuration)
        {
            timeElapsed += Time.deltaTime;
            intensityDenominator = Mathf.Lerp(floor, shelf, EaseOut(timeElapsed / decelDuration));
            yield return null;
        }
    }

    public static float EaseIn(float t)
    {
        return t * t;
    }

    public static float EaseOut(float t)
    {
        return Flip(EaseIn(Flip(t)));
    }

    public static float Spike(float t)
    {
        if (t <= .5f)
            return EaseIn(t / .5f);

        return EaseIn(Flip(t) / .5f);
    }

    static float Flip(float x)
    {
        return 1 - x;
    }

    public static float EaseInOut(float t)
    {
        return Mathf.Lerp(EaseIn(t), EaseOut(t), t);
    }

    public static float GentleSpike(float t, float spikePosition)
    {
        if (t <= spikePosition) { 
            return EaseInOut(t / spikePosition);
        }
        return EaseInOut(Flip(t) / spikePosition);
    }

    public float StopDuration(float xVelocity)
    {
        return (Mathf.Log(1 / xVelocity) / -0.083f)/50;
        //Denominator should be equal to ln(1 - deceleration coeffeciient).
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
}
