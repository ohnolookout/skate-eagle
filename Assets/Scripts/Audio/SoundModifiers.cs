using UnityEngine;

public class SoundModifiers
{
    public float intensity, distance, pan;
    private float _lastIntensity, _lastDistance, _lastPan, _targetIntensity, _targetDistance, _targetPan;
    public SoundModifiers(float intensity, float distance, float pan)
    {
        this.intensity = intensity;
        this.distance = distance;
        this.pan = pan;
        _lastIntensity = intensity;
        _lastDistance = distance;
        _lastPan = pan;
        _targetIntensity = intensity;
        _targetDistance = distance;
        _targetPan = pan;
    }

    public SoundModifiers(float intensity)
    {
        this.intensity = intensity;
        _lastIntensity = intensity;
        _targetIntensity = intensity;
        distance = 1;
        pan = 0;
    }

    public void LerpModifiers(float t)
    {
        intensity = Mathf.Lerp(_lastIntensity, _targetIntensity,  t);
        distance = Mathf.Lerp(_lastDistance, _targetDistance, t);
        pan = Mathf.Lerp(_lastPan, _targetPan, t); 
    }

    public void LerpIntensity(float t)
    {
        intensity = Mathf.Lerp(_lastIntensity, _targetIntensity, t);
    }

    public void SetNewTargets(float newIntensity, float newDistance, float newPan)
    {
        _targetIntensity = newIntensity + (newIntensity - _lastIntensity);
        _targetDistance = newDistance + (newDistance - _lastDistance);
        _targetPan = newPan + (newPan - _lastPan);
        intensity = newIntensity;
        distance = newDistance;
        pan = newPan;
        _lastIntensity = intensity;
        _lastDistance = distance;
        _lastPan = pan;
    }

    public void SetNewTargetIntensity(float newIntensity)
    {
        _targetIntensity = newIntensity + (newIntensity - _lastIntensity);
        intensity = newIntensity;
        _lastIntensity = intensity;
    }


}