using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Profiling;
using System;

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;
    [SerializeField] private AudioSource[] audioSources; //Minimum 3 audiosources, works best with 6.
    private Soundtrack soundtrack;
    public Dictionary<AudioSource, Sound> playingSounds = new();
    public Dictionary<AudioSource, IEnumerator> fadingSources = new();
    //private ILevelManager _levelManager;
    public float intensityDenominator = 300, maxSoundDistance = 160, zoomLimit = 110;
    private bool _updateZoomModifier = false, _updateLocalModifiers = false;
    private Action<ICameraOperator> updateZoom;
    private Action stopUpdateZoom;
    private SoundModifierManager _modifierManager;
    private int _soundModFrameRate = 6, _zoomModFrameRate = 10;
    private IPlayer _player;
    private ICameraOperator _camera;

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
        _player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        _camera = Camera.main.GetComponent<ICameraOperator>();
    }

    private void Start()
    {
        if (soundtrack != null)
        {
            PlaySoundtrack(soundtrack);
        }
        if (_camera != null)
        {
            updateZoom += _ => _updateZoomModifier = true;
            stopUpdateZoom += () =>
            {
                _modifierManager.ResetZoomModifier();
                _updateZoomModifier = false;
            };
            _camera.OnZoomOut += updateZoom;
            _camera.OnFinishZoomIn += stopUpdateZoom;
        }
    }

    public void AssignLevelEvents()
    {
        LevelManager.OnRestart += ClearLoops;
        LevelManager.OnGameOver += _ => ClearLoops();
        LevelManager.OnLanding += LoadComponents;
        LevelManager.OnAttempt += () => StartUpdatingModifiers(true);
        LevelManager.OnFinish += _ => SetModifierFramerate(1);
        LevelManager.OnResultsScreen += () => StartUpdatingModifiers(false);
    }

    public void LoadComponents(ILevelManager levelManager)
    {
        _player = levelManager.GetPlayer;
        _camera = levelManager.CameraOperator;
    }

    public void StartUpdatingModifiers(bool doUpdate)
    {
        _updateLocalModifiers = doUpdate;
    }

    private void OnDisable()
    {
        if(_camera == null)
        {
            return;
        }
        _camera.OnZoomOut -= updateZoom;
        _camera.OnFinishZoomIn -= stopUpdateZoom;
    }

    private void Update()
    {
        if (!_updateLocalModifiers || _player == null || _camera == null)
        {
            return;
        }        
        if (_updateZoomModifier)
        {
            _modifierManager.UpdateZoomModifier(_camera, zoomLimit, _zoomModFrameRate);
        }
        _modifierManager.UpdateLocalizedModifiers(_player, _camera, maxSoundDistance, intensityDenominator, _soundModFrameRate);
        ApplyModifiersToLoops();
        
    }

    private void ApplyModifiersToLoops()
    {
        foreach (var source in playingSounds.Keys)
        {
            if (!fadingSources.ContainsKey(source))
            {
                AudioManagerUtility.UpdateLoopSource(source, playingSounds[source], _modifierManager, _player.IsRagdoll);
            }
        }
    }

    private void SetModifierFramerate(int newFrameRate)
    {
        _soundModFrameRate = newFrameRate;
    }

    public void ClearLoops()
    {
        Debug.Log("Clearing loops");
        if (audioSources == null) return;
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
        _soundModFrameRate = 6;
    }
    //Plays soundtrack on audioSources[0]
    private void PlaySoundtrack(Soundtrack soundtrack)
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
        float modifier = _modifierManager.GetTotalModifier(sound, _player.IsRagdoll);
        if (sound.trackIntensity)
        {
            float intensity = -1 + Mathf.Clamp(_player.MagnitudeDelta() / 100, 0, 2);
            audioSources[1].volume = 0.1f + sound.AdjustedVolume(intensity, modifier, _player.IsRagdoll);
        }
        else
        {
            audioSources[1].volume = sound.volume * modifier;
        }
        //One shots don't have pitch variance.
        audioSources[1].panStereo = _modifierManager.GetPan(sound);
        audioSources[1].pitch = sound.pitch;
        audioSources[1].PlayOneShot(sound.Clip());
    }

    //Plays loop on first unused source beginning at audioSources[2]
    public void StartLoop(Sound sound, float fadeTime = 0)
    {
        AudioSource source = AudioManagerUtility.FirstAvailableSource(audioSources);
        AudioManagerUtility.LoadSoundWithModifiers(sound, source, _modifierManager, _player.IsRagdoll);
        source.time = UnityEngine.Random.Range(0, source.clip.length);
        source.Play();
        AddPlayingSound(source, sound);
        if (fadeTime > 0)
        {
            fadingSources[source] = AudioManagerUtility.FadeAudioSource(this, sound.source, 0, fadeTime, sound.trackZoom);
            StartCoroutine(fadingSources[source]);
        }
    }

    public void TimedFadeInZoomFadeOut(Sound sound, ICameraOperator camera, float initialDelay, float fadeInTime, float cameraSizeThreshold)
    {
        AudioSource source = AudioManagerUtility.FirstAvailableSource(audioSources);
        AudioManagerUtility.LoadSoundWithModifiers(sound, source, _modifierManager, _player.IsRagdoll);
        IEnumerator thisFade = AudioManagerUtility.TimedInZoomOut(this, source, camera, sound.volume, initialDelay, fadeInTime, cameraSizeThreshold);
        AddPlayingSound(source, sound, thisFade);
        StartCoroutine(thisFade);
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

    public void InitializeModifiers(Rigidbody2D[] trackedBodies)
    {
        _modifierManager = new(trackedBodies);
    }

    
    public static AudioManager Instance
    {
        get
        {
            if (instance != null)
            {
                return instance;
            }
            Debug.Log("No AudioManager instance found.");
            return null;
        }
    }

    public float ZoomModifier { get => _modifierManager.ZoomModifier; }
}
