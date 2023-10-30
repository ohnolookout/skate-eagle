using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;

public enum SkateFX { Jump, Fall, Push, Land, Slide  }
public class PlayerAudio : MonoBehaviour
{
    [SerializeField]
    private Sound roll, wind;
    [SerializeField]
    private SerializedDictionary<SkateFX, Sound> fxDict = new();
    private AudioManager audioManager;
    [SerializeField] 
    private AudioSource rollSource, windSource, effectSource;
    public bool rolling = false;
    [SerializeField] 
    private Rigidbody2D eagleBody;
    private IEnumerator rollFade;

    private void Awake()
    {
        AudioManager.AssignSourceToSound(roll, rollSource);
        AudioManager.AssignSourceToSound(wind, windSource);
    }

    void Start()
    {
        audioManager = AudioManager.Instance;
    }

    void Update()
    {
        if (rolling)
        {
            float rollIntensity = eagleBody.velocity.magnitude / 600;
            rollSource.volume = 0.05f + Mathf.Clamp(rollIntensity, 0, 0.5f);
            rollSource.pitch = 0.6f + Mathf.Clamp(rollIntensity, 0, 0.5f);
            windSource.volume = 0.05f + Mathf.Clamp(rollIntensity, 0, 0.5f);
            windSource.pitch = 0.6f + Mathf.Clamp(rollIntensity, 0, 0.5f);
        }
    }
    public void PlayEffect(SkateFX name)
    {
        if (!fxDict.ContainsKey(name))
        {
            Debug.LogWarning($"Effect {name} not found!");
            return;
        }
        AudioManager.LoadSoundToSource(fxDict[name], effectSource);
        effectSource.Play();
    }

    public void Jump()
    {
        PlayEffect(SkateFX.Jump);
        SwitchRoll(1);
        rollFade = AudioManager.FadeAudioSource(rollSource, 0, 1f);
        StartCoroutine(rollFade);
    }

    public void Land()
    {
        PlayEffect(SkateFX.Land);
        StopCoroutine(rollFade);
        SwitchRoll(0);
        rollFade = AudioManager.FadeAudioSource(roll.source, roll.volume, 0.25f);
        StartCoroutine(rollFade);
    }

    public void Airborne()
    {
        SwitchRoll(1);
        rollFade = AudioManager.FadeAudioSource(roll.source, 0, 1);
    }

    public void Finish()
    {
        PlayEffect(SkateFX.Slide);
        rollFade = AudioManager.FadeAudioSource(roll.source, 0, 0.25f);
        StartCoroutine(rollFade);

    }

    public void Fall()
    {
        PlayEffect(SkateFX.Fall);
    }

    public void SwitchRoll(int index)
    {
        if(roll.clips.Length < 0)
        {
            Debug.LogWarning("No roll clips found!");
            return;
        }
        if(roll.clips.Length == 1)
        {
            roll.source.clip = roll.clips[0];
            Debug.LogWarning("Only one roll clip available.");
            return;
        }
        rollSource.clip = roll.clips[index];
    }
}
