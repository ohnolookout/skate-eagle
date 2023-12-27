using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;
    [SerializeField] private AudioSource[] audioSources; //Minimum 3 audiosources
    private Soundtrack soundtrack;
    public static Dictionary<AudioSource, Sound> playingSounds = new();
    public static Dictionary<AudioSource, IEnumerator> fadingSources = new();
    private Dictionary<Rigidbody2D, SoundModifiers> modifiers = new();
    private LiveRunManager runManager;
    public static float intensityDenominator = 300, maxSoundDistance = 160, zoomLimit = 110, zoomModifier = 1;

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

    private void Update()
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
    public void ClearLoops()
    {
        for (int i = 2; i < audioSources.Length; i++)
        {
            audioSources[i].Stop();
        }
        foreach(var coroutine in fadingSources.Values)
        {
            StopCoroutine(coroutine);
        }
        intensityDenominator = 300;
        playingSounds = new();
        fadingSources = new();
        modifiers = new();
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

    //Plays one shots on audioSources[1]
    public void PlayOneShot(Sound sound)
    {
        float modifier = AudioUtility.TotalModifier(sound, modifiers[sound.localizedSource], zoomModifier, runManager.Player.IsRagdoll);
        if (sound.trackIntensity)
        {
            float intensity = -1 + Mathf.Clamp(runManager.Player.ForceDelta() / 100, 0, 2);
            audioSources[1].volume = 0.1f + sound.AdjustedVolume(intensity, modifier, runManager.Player.IsRagdoll);
        }
        else
        {
            audioSources[1].volume = sound.volume * modifier;
        }
        //One shots don't have pitch variance.
        audioSources[1].panStereo = modifiers[sound.localizedSource].pan;
        audioSources[1].pitch = sound.pitch;
        audioSources[1].PlayOneShot(sound.Clip());
    }

    //Plays loop on first unused source beginning at audioSources[2]
    public void StartLoop(Sound sound, float fadeTime = 0)
    {
        AudioSource source = FirstAvailableSource();
        LoadSoundWithModifiers(sound, source);
        source.time = Random.Range(0, source.clip.length);
        source.Play();
        AddPlayingSound(source, sound);
        if (fadeTime > 0)
        {
            fadingSources[source] = AudioUtility.FadeAudioSource(sound.source, 0, fadeTime, sound.trackZoom);
            StartCoroutine(fadingSources[source]);
        }
    }

    public void TimedFadeInZoomFadeOut(Sound sound, float initialDelay, float fadeInTime, float cameraSizeThreshold)
    {
        AudioSource source = FirstAvailableSource();
        LoadSoundWithModifiers(sound, source);
        IEnumerator thisFade = TimedInZoomOut(source, sound.volume, initialDelay, fadeInTime, cameraSizeThreshold);
        AddPlayingSound(source, sound, thisFade);
        StartCoroutine(thisFade);
    }

    private IEnumerator TimedInZoomOut(AudioSource source, float maxVolume, float initialDelay, float fadeInTime, float cameraSizeThreshold)
    {
        source.volume = 0;
        source.Play();
        Camera camera = Camera.main;
        while (camera.orthographicSize < cameraSizeThreshold)
        {
            yield return null;
        }
        float timeElapsed = 0;
        
        while (timeElapsed < initialDelay)
        {
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        if (!camera.GetComponent<CameraScript>().cameraZoomOut)
        {
            RemovePlayingSound(source, true);
            StopLoop(source);
            yield break;
        }
        float maxCamSize = camera.orthographicSize;
        timeElapsed = 0;
        while (timeElapsed < fadeInTime && camera.orthographicSize >= maxCamSize * 0.7f)
        {
            source.volume = Mathf.Lerp(0, maxVolume, timeElapsed / fadeInTime);
            maxCamSize = Mathf.Max(maxCamSize, camera.orthographicSize);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        while(camera.orthographicSize >= maxCamSize)
        {
            yield return null;
        }
        float fadeOutTime = Mathf.Max(timeElapsed, 1.5f);
        timeElapsed = 0;
        float peakVolume = source.volume;
        while(timeElapsed < fadeOutTime)
        {
            source.volume = Mathf.Lerp(peakVolume, 0, timeElapsed / fadeOutTime);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        StopLoop(source);

    }

    public void AddPlayingSound(AudioSource source, Sound sound, IEnumerator fadeRoutine = null)
    {
        playingSounds[source] = sound;
        if (fadeRoutine != null)
        {
            fadingSources[source] = fadeRoutine;
        }
    }

    public void RemovePlayingSound(AudioSource source, bool removeFade)
    {
        playingSounds.Remove(source);
        if (removeFade)
        {
            fadingSources.Remove(source);
        }
    }

    public void StopLoop(AudioSource source)
    {
        if (fadingSources.ContainsKey(source))
        {
            StopCoroutine(fadingSources[source]);
            fadingSources.Remove(source);
        }
        RemovePlayingSound(source, false);
        source.Stop();
    }

    public void StopLoop(Sound sound)
    {
        if (playingSounds.ContainsKey(sound.source) && playingSounds[sound.source] == sound) 
        {
            StopLoop(sound.source);
        }
    }

    private void UpdateLoopSource(AudioSource loopSource)
    {
        //Apply updated modifiers
        Sound sound = playingSounds[loopSource];
        //Key "null" is sometimes not found in modifiers when called by Update on startup.
        float modifier = AudioUtility.TotalModifier(sound, modifiers[sound.localizedSource], zoomModifier, runManager.Player.IsRagdoll);
        loopSource.volume = sound.AdjustedVolume(modifiers[sound.localizedSource].intensity, modifier, runManager.Player.IsRagdoll);
        loopSource.panStereo = modifiers[sound.localizedSource].pan;
        loopSource.pitch = sound.AdjustedPitch(modifiers[sound.localizedSource].intensity, runManager.Player.IsRagdoll);
    }
    


    private void LoadSoundWithModifiers(Sound sound, AudioSource source)
    {
        source.clip = sound.Clip();
        float modifier = AudioUtility.TotalModifier(sound, modifiers[sound.localizedSource], zoomModifier, runManager.Player.IsRagdoll);
        source.volume = sound.AdjustedVolume(modifiers[sound.localizedSource].intensity, modifier, runManager.Player.IsRagdoll);
        source.panStereo = modifiers[sound.localizedSource].pan;
        source.pitch = sound.AdjustedPitch(modifiers[sound.localizedSource].intensity, runManager.Player.IsRagdoll);
        sound.source = source;

    }
    //Called by player audio to send localized objects and runManager (runManager must exist for localization).
    public void BuildModifierDict(Rigidbody2D[] trackedBodies)
    {
        modifiers = new();
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

    private AudioSource FirstAvailableSource()
    {
        for (int i = 2; i < audioSources.Length; i++)
        {
            if (!audioSources[i].isPlaying)
            {
                return audioSources[i];
            }
        }
        Debug.LogError("No available audiosource. Overwriting final audiosource.");
        return audioSources[^1];
    }
    public static AudioManager Instance
    {
        get
        {
            if (instance != null)
            {
                return instance;
            }
            Debug.Log("No AudioManager instance found. Creating instance.");
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