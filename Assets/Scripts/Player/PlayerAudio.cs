using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;
using System.Linq;
using System;

public enum OneShotFX { Jump, SecondJump, Wheel, Board, Body, HardBody};
public enum LoopFX { Roll, Freewheel, Board, Body, Wind };


public class PlayerAudio : MonoBehaviour
{
    [SerializeField] private SerializedDictionary<OneShotFX, Sound> _oneShotDict = new();
    [SerializeField] private SerializedDictionary<LoopFX, Sound> _loopDict = new();
    private bool _wheelsOnGround = false;
    [SerializeField] private Player _player;
    private float _wheelTimer = -1;
    private const float _wheelTimeLimit = 0.2f, _wheelFadeCoefficient = 0.01f;
    private AudioManager _audioManager;
    private CameraOperator _cameraOperator;

    private void Awake()
    {
        List<Sound> sounds = _oneShotDict.Values.ToList();
        sounds.AddRange(_loopDict.Values.ToList());
        _audioManager = AudioManager.Instance;
        if(_audioManager == null)
        {
            Debug.LogWarning("No audio manager found by player audio. Deleting player audio.");
            DestroyImmediate(this);
            return;
        }
        _audioManager.InitializeModifiers(TrackedBodies(sounds));
    }

    private void Start()
    {
        _player.CollisionManager.OnCollide += Collide;
        _player.CollisionManager.OnUncollide += Uncollide;
    }

    private void OnEnable()
    {

        Player.OnJump += JumpSound;
        Player.OnDismount += Dismount;
        Player.OnSlowToStop += SlowToStop;
        _cameraOperator = Camera.main.GetComponent<CameraOperator>();
        if(_cameraOperator != null)
        {
            _cameraOperator.OnZoomOut += StartWind;
            _cameraOperator.OnFinishZoomIn += StopWind;
        }
    }
    private void OnDisable()
    {
        if(_audioManager == null)
        {
            return;
        }
        _player.CollisionManager.OnCollide -= Collide;
        _player.CollisionManager.OnUncollide -= Uncollide;
        Player.OnJump -= JumpSound;
        Player.OnDismount -= Dismount;
        Player.OnSlowToStop -= SlowToStop;
        if (_cameraOperator != null)
        {
            _cameraOperator.OnZoomOut -= StartWind;
            _cameraOperator.OnFinishZoomIn -= StopWind;
        }
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

    private void JumpSound(IPlayer player)        
    {
        if (player.JumpCount == 0)
        {
            _audioManager.PlayOneShot(_oneShotDict[OneShotFX.Jump]);
            _wheelTimer = 0;
            if (!_audioManager.playingSounds.ContainsValue(_loopDict[LoopFX.Freewheel]))
            {
                _audioManager.StopLoop(_loopDict[LoopFX.Roll]);
                _audioManager.StartLoop(_loopDict[LoopFX.Freewheel], WheelFadeTime(player.Velocity.magnitude));
            }
        }
        else
        {
            _audioManager.PlayOneShot(_oneShotDict[OneShotFX.SecondJump]);
        }
        _wheelsOnGround = false;
    }
    private void SlowToStop(IPlayer player)
    {
        _wheelsOnGround = true;
        float stopDuration = AudioManagerUtility.StopDuration(player.Velocity.x);
        StartCoroutine(SpikeIntensityDenom(stopDuration, 5));
        _audioManager.StartLoop(_loopDict[LoopFX.Board]);

    }

    private void StartWind(ICameraOperator camera)
    {
        if (!_audioManager.playingSounds.ContainsValue(_loopDict[LoopFX.Wind]))
        {
            _audioManager.TimedFadeInZoomFadeOut(_loopDict[LoopFX.Wind], camera, 0.5f, 3f, _cameraOperator.DefaultSize * 2f);
        }
    }

    private void StopWind()
    {
        _audioManager.StopLoop(_loopDict[LoopFX.Wind]);
    }

    private void Dismount()
    {
        _audioManager.PlayOneShot(_oneShotDict[OneShotFX.Jump]);
        _audioManager.ClearLoops();
    }

    private void Collide(ColliderCategory category, float magnitudeDelta)
    {
        switch (category)
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

    private void Uncollide(ColliderCategory category, float magnitudeAtCollisionExit)
    {
        switch (category)
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

    private void WheelCollision()
    {
        if (!_wheelsOnGround && _wheelTimer < 0)
        {
            _audioManager.PlayOneShot(_oneShotDict[OneShotFX.Wheel]);
            _wheelsOnGround = true;
            _audioManager.StopLoop(_loopDict[LoopFX.Freewheel]);
            _audioManager.StartLoop(_loopDict[LoopFX.Roll]);
            _wheelTimer = 0;
        }
    }

    private void BodyCollision(float magnitudeDelta)
    {
        if(magnitudeDelta > 120)
        {
            _audioManager.PlayOneShot(_oneShotDict[OneShotFX.HardBody]);
        }
        else
        {
            _audioManager.PlayOneShot(_oneShotDict[OneShotFX.Body]);
        }
        _audioManager.StartLoop(_loopDict[LoopFX.Body]);
    }

    private void BoardCollision()
    {
        _audioManager.PlayOneShot(_oneShotDict[OneShotFX.Board]);
        _audioManager.StartLoop(_loopDict[LoopFX.Board]);
    }

    private void WheelExit(float magnitudeAtCollisionExit)
    {
        if (_player.CollisionManager.WheelsCollided)
        {
            return;
        }
        if (!_audioManager.playingSounds.ContainsValue(_loopDict[LoopFX.Freewheel]))
        {
            _audioManager.StopLoop(_loopDict[LoopFX.Roll]);
            _audioManager.StartLoop(_loopDict[LoopFX.Freewheel], WheelFadeTime(magnitudeAtCollisionExit));
        }
        _wheelsOnGround = false;
    }

    private void BoardExit()
    {
        _audioManager.StopLoop(_loopDict[LoopFX.Board]);
    }

    private void BodyExit()
    {
        _audioManager.StopLoop(_loopDict[LoopFX.Body]);
    }

    private IEnumerator SpikeIntensityDenom(float duration, float denomMultiplier)
    {
        float accelDuration = duration * 0.05f;
        float holdDuration = duration * 0.45f;
        float decelDuration = duration * 0.5f;
        float timeElapsed = 0;
        float startDenom = _audioManager.intensityDenominator;
        float floor = _audioManager.intensityDenominator /denomMultiplier;
        float shelf = startDenom;
        while (timeElapsed < accelDuration)
        {
            timeElapsed += Time.deltaTime;
            _audioManager.intensityDenominator = Mathf.Lerp(startDenom, floor, AudioManagerUtility.EaseInOut(timeElapsed / accelDuration));
            yield return null;
        }
        timeElapsed = 0;
        yield return new WaitForSeconds(holdDuration);
        while (timeElapsed < decelDuration)
        {
            timeElapsed += Time.deltaTime;
            _audioManager.intensityDenominator = Mathf.Lerp(floor, shelf, AudioManagerUtility.EaseOut(timeElapsed / decelDuration));
            yield return null;
        }
    }

    private void UpdateWheelTimer()
    {
        if(_wheelTimer < 0)
        {
            return;
        }
        _wheelTimer += Time.deltaTime;
        if (_wheelTimer >= _wheelTimeLimit)
        {
            _wheelTimer = -1;
        }
    }

    private float WheelFadeTime(float magnitudeAtCollisionExit)
    {
        return _wheelFadeCoefficient * magnitudeAtCollisionExit;
    }

}
