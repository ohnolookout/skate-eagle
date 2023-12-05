using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;
    [SerializeField] private AudioSource[] audioSources; //Minimum 3 audiosources
    private Soundtrack soundtrack;
    public Dictionary<AudioSource, Sound> playingSounds = new();
    public Dictionary<AudioSource, IEnumerator> fadingSources = new();
    private Dictionary<Rigidbody2D, SoundModifiers> modifiers = new();
    private LiveRunManager runManager;
    public static float intensityDenominator = 300, maxSoundDistance = 80, zoomLimit = 110, zoomModifier = 1;

    void Awake()
    {
        //Check to see if other instance exists that has already 
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (soundtrack != null)
        {
            PlaySoundtrack(soundtrack);
        }
    }

    private void FixedUpdate()
    {
        if (modifiers.Count < 0 || runManager == null)
        {
            return;
        }
        if ((int)runManager.runState < 2)
        {
            return;
        }
        zoomModifier = AudioUtility.CalculateZoomModifier(runManager, zoomLimit);
        AudioUtility.UpdateModifiers(modifiers, runManager, maxSoundDistance);
        foreach (var source in playingSounds.Keys)
        {
            if (!fadingSources.ContainsKey(source))
            {
                UpdateLoopSource(source);
            }
        }
    }
    public void StopLoops()
    {
        intensityDenominator = 300;
        for (int i = 1; i < audioSources.Length; i++)
        {
            audioSources[i].Stop();
        }
    }
    //Plays soundtrack on audioSources[0]
    public void PlaySoundtrack(Soundtrack soundtrack)
    {
        Sound track = soundtrack.tracks;
        audioSources[0].clip = track.Clip();
        audioSources[0].volume = track.volume;
        track.source = audioSources[0];
        audioSources[0].Play();
    }

    //Plays soundtrack on audioSources[1]
    public void PlayOneShot(Sound sound)
    {
        float modifier = AudioUtility.TotalModifier(sound, modifiers[sound.localizedSource], zoomModifier, runManager.PlayerIsRagdoll);
        if (sound.trackIntensity)
        {
            float intensity = -1 + Mathf.Clamp(runManager.Player.ForceDelta / 100, 0, 2);
            audioSources[1].volume = 0.1f + sound.AdjustedVolume(intensity, modifier, runManager.PlayerIsRagdoll);
        }
        else
        {
            audioSources[1].volume = sound.volume * modifier;
        }
        //One shots don't have pitch variance.
        audioSources[1].panStereo = modifiers[sound.localizedSource].pan;
        audioSources[1].pitch = sound.pitch;
        //Debug.Log("Playing oneshot for sound " + sound.name + " at volume " + audioSources[1].volume);
        audioSources[1].PlayOneShot(sound.Clip());
    }

    //Plays loop on first unused source beginning at audioSources[2]
    public void StartLoop(Sound sound, float fadeTime = 0)
    {
        for (int i = 2; i < audioSources.Length; i++)
        {
            if (!audioSources[i].isPlaying)
            {
                LoadSoundWithModifiers(sound, audioSources[i]);
                audioSources[i].Play();
                playingSounds[audioSources[i]] = sound;
                if (fadeTime > 0)
                {
                    fadingSources[sound.source] = FadeAudioSource(sound.source, 0, fadeTime);
                    StartCoroutine(fadingSources[sound.source]);
                }
                return;
            }
        }
        Debug.LogError("No available audiosource to play sound " + sound.name);
    }

    public void StopLoop(Sound sound)
    {
        if (sound.source != null)
        {
            sound.source.Stop();
        }
        if (fadingSources.ContainsKey(sound.source))
        {
            StopCoroutine(fadingSources[sound.source]);
            fadingSources.Remove(sound.source);
        }
        playingSounds.Remove(sound.source);
    }

    private void UpdateLoopSource(AudioSource loopSource)
    {
        Rigidbody2D trackingBody = playingSounds[loopSource].localizedSource;
        //Apply updated modifiers
        Sound sound = playingSounds[loopSource];
        //Debug.Log($"Updating loop source for " + sound.name);
        float modifier = AudioUtility.TotalModifier(sound, modifiers[sound.localizedSource], zoomModifier, runManager.PlayerIsRagdoll);
        //Debug.Log($"Modifier: {modifier}");
        loopSource.volume = sound.AdjustedVolume(modifiers[sound.localizedSource].intensity, modifier, runManager.PlayerIsRagdoll);
        //Debug.Log($"New volume: {loopSource.volume}");
        loopSource.panStereo = modifiers[sound.localizedSource].pan;
        loopSource.pitch = sound.AdjustedPitch(modifiers[sound.localizedSource].intensity, runManager.PlayerIsRagdoll);
    }
    private IEnumerator FadeAudioSource(AudioSource source, float finishVolume, float fadeDuration)
    {
        float startVolume = source.volume;
        float timeElapsed = 0;
        while (timeElapsed < fadeDuration)
        {
            timeElapsed += Time.deltaTime;
            float t = timeElapsed / fadeDuration;
            source.volume = Mathf.Lerp(startVolume, finishVolume, t) * zoomModifier;
            yield return new WaitForFixedUpdate();
        }
        source.Stop();
        playingSounds.Remove(source);
        fadingSources.Remove(source);
    }


    private void LoadSoundWithModifiers(Sound sound, AudioSource source)
    {
        source.clip = sound.Clip();
        float modifier = AudioUtility.TotalModifier(sound, modifiers[sound.localizedSource], zoomModifier, runManager.PlayerIsRagdoll);
        source.volume = sound.AdjustedVolume(modifiers[sound.localizedSource].intensity, modifier, runManager.PlayerIsRagdoll);
        source.panStereo = modifiers[sound.localizedSource].pan;
        source.pitch = sound.AdjustedPitch(modifiers[sound.localizedSource].intensity, runManager.PlayerIsRagdoll);
        sound.source = source;

    }
    //Called by player audio to send localized objects and runManager (runManager must exist for localization).
    public void BuildModifierDict(Rigidbody2D[] trackedBodies)
    {
        foreach (var body in trackedBodies)
        {
            if (!modifiers.ContainsKey(body))
            {
                modifiers[body] = new(1);
            }
        }
    }
    private static void LoadSoundToSource(Sound sound, AudioSource source, float volume, float pitch = 1, float pan = 0)
    {
        source.clip = sound.Clip();
        source.volume = volume;
        source.pitch = pitch;
        source.panStereo = pan;
        sound.source = source;
    }

    private static void LoadSoundToSource(Sound sound, AudioSource source)
    {
        source.clip = sound.Clip();
        source.volume = sound.volume;
        source.pitch = sound.pitch;
        source.panStereo = 0;
        sound.source = source;
    }
    public static AudioManager Instance
    {
        get
        {
            if (instance != null)
            {
                return instance;
            }
            Debug.Log("No instance found. Creating instance.");
            GameObject managerObject = new GameObject("AudioManager");
            instance = managerObject.AddComponent<AudioManager>();
            return instance;
        }
    }

    public LiveRunManager RunManager
    {
        set
        {
            runManager = value;
        }
    }
}
public class SoundModifiers
{
    public float intensity, distance, pan;
    public SoundModifiers(float intensity, float distance, float pan)
    {
        this.intensity = intensity;
        this.distance = distance;
        this.pan = pan;
    }

    public SoundModifiers(float intensity)
    {
        this.intensity = intensity;
        distance = 1;
        pan = 0;
    }


}