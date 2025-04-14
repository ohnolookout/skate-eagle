using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Profiling;
using System;

public class AudioManager : MonoBehaviour
{
    #region Declarations
    private static AudioManager _instance;
    [SerializeField] private AudioSource[] _audioSources; //Minimum 3 audiosources, works best with 6.
    private Soundtrack _soundtrack;
    public Dictionary<AudioSource, Sound> playingSounds = new();
    public Dictionary<AudioSource, IEnumerator> fadingSources = new();
    public float intensityDenominator = 300, maxSoundDistance = 160, zoomLimit = 110;
    private bool _updateZoomModifier = false, _updateLocalModifiers = false;
    //private Action<ICameraOperator> _updateZoom;
    private Action _stopUpdateZoom;
    private SoundModifierManager _modifierManager;
    private int _soundModFrameRate = 6, _zoomModFrameRate = 10;
    private IPlayer _player;
    private Camera _camera;
    private IEnumerator _delayedSoundRoutine;

    public float ZoomModifier { get => _modifierManager.ZoomModifier; }

    public static AudioManager Instance
    {
        get
        {
            if (_instance != null)
            {
                return _instance;
            }
            Debug.Log("No AudioManager instance found.");
            return null;
        }
    }

    #endregion

    #region Monobehaviours
    void Awake()
    {
        //Check to see if other instance exists that has already 
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        GameManager.Instance.OnLevelLoaded += OnLevelLoaded;
        GameManager.Instance.OnMenuLoaded += OnMenuLoaded;
        LevelManager.OnRestart += ClearLoops;
        LevelManager.OnGameOver += ClearLoops;
        LevelManager.OnAttempt += () => StartUpdatingModifiers(true);
        LevelManager.OnFinish += _ => SetModifierFramerate(1);
        LevelManager.OnResultsScreen += () => StartUpdatingModifiers(false);
        LevelManager.OnLevelExit += () => StartUpdatingModifiers(false);
    }

    private void Start()
    {
    }
    private void Update()
    {
        if (!_updateLocalModifiers || _player == null || _camera == null)
        {
            return;
        }
        if (_updateZoomModifier)
        {
            _modifierManager.UpdateZoomModifier(zoomLimit, _zoomModFrameRate);
        }
        _modifierManager.UpdateLocalizedModifiers(_player.IsRagdoll, maxSoundDistance, intensityDenominator, _soundModFrameRate);
        ApplyModifiersToLoops();
    }

    private void OnDisable()
    {
        if (_camera == null)
        {
            return;
        }
        //NEED TO HANDLE WITH NEW ZOOM METHODS
        /*
        _camera.OnZoomOut -= _updateZoom;
        _camera.OnFinishZoomIn -= _stopUpdateZoom;
        */
    }

    #endregion

    #region Initialization
    private void OnMenuLoaded(bool goToLevelMenu)
    {
        ClearLoops();
    }

    private void OnLevelLoaded(Level level)
    {
        ClearLoops();

        if (_soundtrack != null)
        {
            PlaySoundtrack(_soundtrack);
        }

        //SubscribeToCameraEvents();
    }

    private void SubscribeToCameraEvents()
    {
        if (_camera != null)
        {
            //_updateZoom += _ => _updateZoomModifier = true;
            _stopUpdateZoom += () =>
            {
                _modifierManager.ResetZoomModifier();
                _updateZoomModifier = false;
            };
            
        //NEED TO HANDLE WITH NEW ZOOM METHODS
        /*
            _camera.OnZoomOut += _updateZoom;
            _camera.OnFinishZoomIn += _stopUpdateZoom;
        */
        }
    }

    #endregion

    #region Modifier Management

    public void InitializeModifiers(IPlayer player, Camera camera, List<Sound> sounds)
    {
        _player = player;
        _camera = camera;
        _modifierManager = new(_player, GetBodiesFromSounds(sounds), camera);
        SubscribeToCameraEvents();
    }

    private Rigidbody2D[] GetBodiesFromSounds(List<Sound> sounds)
    {
        List<Rigidbody2D> bodies = new();
        foreach (var sound in sounds)
        {
            if (!bodies.Contains(sound.localizedSource))
            {
                bodies.Add(sound.localizedSource);
            }
        }
        return bodies.ToArray();
    }

    public void StartUpdatingModifiers(bool doUpdate)
    {
        _updateLocalModifiers = doUpdate;
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
    private TrackingType GetTrackingType(Sound sound)
    {
        if (!_player.IsRagdoll)
        {
            return TrackingType.PlayerNormal;
        }
        else if (sound.localizedSource == null)
        {
            return TrackingType.PlayerRagdoll;
        }
        else
        {
            return TrackingType.Board;
        }
    }

    #endregion

    #region Loop Management

    //Plays loop on first unused source beginning at audioSources[2]
    public void StartLoop(Sound sound, float fadeTime = 0)
    {
        AudioSource source = AudioManagerUtility.FirstAvailableSource(_audioSources);
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

    public void ClearLoops()
    {
        if (_audioSources == null) return;
        for (int i = 2; i < _audioSources.Length; i++)
        {
            _audioSources[i].Stop();
        }
        foreach(var coroutine in fadingSources.Values)
        {
            StopCoroutine(coroutine);
        }

        if (_delayedSoundRoutine != null)
        {
            StopCoroutine(_delayedSoundRoutine);
        }

        intensityDenominator = 300;
        playingSounds = new();
        fadingSources = new();
        _soundModFrameRate = 6;
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
    /*
    public void TimedFadeInZoomFadeOut(Sound sound, ICameraOperator camera, float initialDelay, float fadeInTime, float cameraSizeThreshold)
    {
        AudioSource source = AudioManagerUtility.FirstAvailableSource(_audioSources);
        AudioManagerUtility.LoadSoundWithModifiers(sound, source, _modifierManager, _player.IsRagdoll);
        IEnumerator thisFade = AudioManagerUtility.TimedInZoomOut(this, source, camera, sound.volume, initialDelay, fadeInTime, cameraSizeThreshold);
        AddPlayingSound(source, sound, thisFade);
        StartCoroutine(thisFade);
    }
    */
    //Plays soundtrack on audioSources[0]
    private void PlaySoundtrack(Soundtrack soundtrack)
    {
        Sound track = soundtrack.tracks;
        _audioSources[0].clip = track.Clip();
        _audioSources[0].volume = track.volume;
        track.source = _audioSources[0];
        _audioSources[0].Play();
    }

    #endregion

    #region One Shot Management

    //Plays one shots on audioSources[1]
    public void PlayOneShot(Sound sound, float inputModifier = 1)
    {
        float modifier = _modifierManager.GetTotalModifier(sound, _player.IsRagdoll);
        if (sound.trackIntensity)
        {
            TrackingType trackingType = GetTrackingType(sound);
            float intensity = -1 + Mathf.Clamp(_player.MomentumTracker.ReboundMagnitude(trackingType) / 100, 0, 2);
            _audioSources[1].volume = 0.1f + sound.AdjustedVolume(intensity, modifier, _player.IsRagdoll);
        }
        else
        {
            _audioSources[1].volume = sound.volume * modifier;
        }
        _audioSources[1].volume *= inputModifier;
        //One shots don't have pitch variance.
        _audioSources[1].panStereo = _modifierManager.GetPan(sound);
        _audioSources[1].pitch = sound.pitch;
        _audioSources[1].PlayOneShot(sound.Clip());
    }

    public void PlayOneShotOnDelay(Sound sound, float delay, float modifier)
    {
        _delayedSoundRoutine = PlayOneShotOnDelayRoutine(sound, delay, modifier);
        StartCoroutine(_delayedSoundRoutine);
    }

    private IEnumerator PlayOneShotOnDelayRoutine(Sound sound, float delay, float modifier)
    {
        yield return new WaitForSeconds(delay);
        PlayOneShot(sound, modifier);
    }
    #endregion
}
