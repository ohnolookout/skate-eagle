using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;
using System.Linq;

public enum OneShotFX { Jump, SecondJump, Wheel, Board, Body, HardBody}
public enum LoopFX { Roll, Freewheel, Board, Body, Wind };
public class PlayerAudio : MonoBehaviour
{
    [SerializeField]
    private SerializedDictionary<OneShotFX, Sound> oneShotDict = new();
    [SerializeField]
    private SerializedDictionary<LoopFX, Sound> loopDict = new();
    [SerializeField]
    private AudioSource wheelSource, windBodySource, boardSource, oneShotSource;
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
    private Dictionary<GameObject, float> localObjectModifiers = new();
    [SerializeField]
    private float maxSoundDistance = 115;

    private void Awake()
    {
        runManager = GameObject.FindGameObjectWithTag("Logic").GetComponent<LiveRunManager>();
        currentLoopDict[wheelSource] = LoopFX.Roll;
        currentLoopDict[windBodySource] = LoopFX.Wind;
        currentLoopDict[boardSource] = LoopFX.Board;
        List<Sound> sounds = oneShotDict.Values.ToList();
        sounds.AddRange(loopDict.Values.ToList());
        localObjectModifiers = BuildLocalObjDict(sounds);
    }

    void Start()
    {
        rollFade = AudioManager.FadeAudioSource(wheelSource, 0, airborneFadeTime * (eagleScript.Velocity.magnitude / intensityDenominator));
        StartLoop(wheelSource, LoopFX.Roll);
        wheelSource.volume = 0;
    }

    void FixedUpdate()
    {
        if ((int)runManager.runState < 2)
        {
            return;
        }
        if (runManager.PlayerIsRagdoll)
        {
            UpdateLocalModifiers();
        }
        //rollIntensity ranges from -1 to 1
        rollIntensity = -1 + Mathf.Clamp(eagleScript.Velocity.magnitude / intensityDenominator, 0, 2);
        //Calculate loop sources' volumes such that the current clips' volume sits at the center of possible ranges.        
        UpdateLoopSource(windBodySource, eagleScript.Rigidbody);
        Rigidbody2D boardBody = null;
        if (eagleScript.IsRagdoll)
        {
            boardBody = eagleScript.RagdollBoard;
        }
        UpdateLoopSource(boardSource, boardBody);
        if (!fadingRoll)
        {
            UpdateLoopSource(wheelSource);
        }
        UpdateWheelTimer();
    }

    private void UpdateLoopSource(AudioSource loopSource, Rigidbody2D trackingBody = null)
    {
        if (trackingBody != null)
        {
            rollIntensity = Intensity(trackingBody);
        }
        if (!loopSource.isPlaying)
        {
            return;
        }
        loopSource.volume = SoundVolume(loopDict[currentLoopDict[loopSource]], rollIntensity);
        loopSource.pitch = loopDict[currentLoopDict[loopSource]].AdjustedPitch(rollIntensity, eagleScript.IsRagdoll);
    }

    private void UpdateLocalModifiers()
    {
        foreach (var localObj in localObjectModifiers.Keys.ToList())
        {            
            localObjectModifiers[localObj] = 
                (maxSoundDistance - Mathf.Abs(localObj.transform.position.x - runManager.CameraCenter.x)) / maxSoundDistance;            
        }
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
        Sound sound = oneShotDict[effectName];
        //Adjust source volume to use volume adjusted by intensity only if variance is > 0
        if (sound.volumeVariance == 0 && sound.localizedSource == null)
        {
            oneShotSource.volume = sound.volume;
        }
        else
        {
            float intensity = -1 + Mathf.Clamp(eagleScript.ForceDelta / 100, 0, 2);
            oneShotSource.volume = 0.1f + SoundVolume(sound, intensity);
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
        if (currentLoopDict[wheelSource] != LoopFX.Freewheel)
        {
            StartLoop(wheelSource, LoopFX.Freewheel);
        }
        StopLoop(boardSource);
        wheelsOnGround = false;
    }


    public void Finish()
    {
        fadingRoll = false;
        wheelsOnGround = true;
        StopCoroutine(rollFade);
        float stopDuration = StopDuration(eagleScript.Velocity.x);
        StartCoroutine(SpikeIntensityDenom(stopDuration, 5));
        StartLoop(boardSource, LoopFX.Board);

    }

    public void Dismount()
    {
        StopLoop(wheelSource);
        StopLoop(windBodySource);
        StopLoop(boardSource);
        PlayOneShot(OneShotFX.Jump);
    }

    public void Collide(ColliderCategory colliderName)
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
                BodyCollision();
                break;
        }
    }

    public void Uncollide(ColliderCategory colliderName)
    {
        switch (colliderName)
        {
            case ColliderCategory.LWheel:
                WheelExit();
                break;
            case ColliderCategory.RWheel:
                WheelExit();
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
            PlayOneShot(OneShotFX.Wheel);
            wheelsOnGround = true;
            StartLoop(wheelSource, LoopFX.Roll);
            wheelTimer = 0;
        }
    }

    public void BodyCollision()
    {
        if(eagleScript.ForceDelta > 140)
        {
            PlayOneShot(OneShotFX.HardBody);
        }
        else
        {
            PlayOneShot(OneShotFX.Body);
        }
        StartLoop(windBodySource, LoopFX.Body);
    }

    public void BoardCollision()
    {
        PlayOneShot(OneShotFX.Board);
        StartLoop(boardSource, LoopFX.Board);
    }

    public void WheelExit()
    {
        if (!collisionTracker.WheelsCollided)
        {
            StartLoop(wheelSource, LoopFX.Freewheel);
            wheelsOnGround = false;
        }
    }

    public void BoardExit()
    {
        StopLoop(boardSource);
    }

    public void BodyExit()
    {
        StopLoop(windBodySource);
    }

    private void StartLoop(AudioSource source, LoopFX loopName)
    {
        if (source == wheelSource && loopName != LoopFX.Freewheel)
        {
            fadingRoll = false;
            StopCoroutine(rollFade);
        }
        source.clip = loopDict[loopName].Clip();
        source.volume = SoundVolume(loopDict[loopName], rollIntensity);
        source.pitch = loopDict[loopName].AdjustedPitch(rollIntensity, eagleScript.IsRagdoll);
        source.Play();
        currentLoopDict[source] = loopName;
        if (loopName == LoopFX.Freewheel)
        {
            StopCoroutine(rollFade);
            //Need to add rollfade method in this class that factors in distance
            rollFade = AudioManager.FadeAudioSource(source, 0, FreewheelFadeTime);
            StartCoroutine(rollFade);
            fadingRoll = true;
        }
    }

    private void StopLoop(AudioSource source)
    {
        source.Stop();
    }

    private float SoundVolume(Sound sound, float intensity)
    {
        if(sound.localizedSource == null || !eagleScript.IsRagdoll)
        {
            return sound.AdjustedVolume(intensity);
        }
        //Debug.Log($"Localizing sound {sound.name} with modifier {localObjectModifiers[sound.localizedSource]}");
        return sound.AdjustedVolume(intensity, eagleScript.IsRagdoll) * localObjectModifiers[sound.localizedSource];
        
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

    private Dictionary<GameObject, float> BuildLocalObjDict(List<Sound> sounds)
    {
        Dictionary<GameObject, float> objDict = new();
        foreach(var sound in sounds)
        {
            if (sound.localizedSource != null && !objDict.ContainsKey(sound.localizedSource))
            {
                objDict[sound.localizedSource] = 0;
            }
        }
        return objDict;
    }

    

    public static float EaseIn(float t)
    {
        return t * t;
    }

    public static float EaseOut(float t)
    {
        return Flip(EaseIn(Flip(t)));
    }

    static float Flip(float x)
    {
        return 1 - x;
    }

    public static float EaseInOut(float t)
    {
        return Mathf.Lerp(EaseIn(t), EaseOut(t), t);
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

    public float FreewheelFadeTime
    {
        get
        {
            return airborneFadeTime * ((Intensity(eagleScript.Rigidbody) + 1) * 2);
        }
    }

    public float Intensity(Rigidbody2D trackingBody)
    {
        return -1 + Mathf.Clamp(trackingBody.velocity.magnitude / intensityDenominator, 0, 2);
    }
}
