using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SoundModifierManager
{

    private Dictionary<Rigidbody2D, SoundModifiers> _modifiers = new();
    private float _zoomModifier = 1, _lastZoomModifier = 1, _targetZoomModifier = 1;
    private float _distanceBuffer = 15;
    public Dictionary<Rigidbody2D, SoundModifiers> Modifiers { get => _modifiers; }
    public float ZoomModifier { get => _zoomModifier; }

    public SoundModifierManager(Rigidbody2D[] trackedBodies)
    {
        _modifiers = new();
        foreach (var body in trackedBodies)
        {
            if (!_modifiers.ContainsKey(body))
            {
                _modifiers[body] = new(0f);
            }
        }
    }
    public void UpdateLocalizedModifiers(IPlayer player, ICameraOperator camera, float maxSoundDistance, float intensityDenominator, int everyXFrames)
    {
        float frameCount = Time.frameCount % everyXFrames;
        foreach (var body in _modifiers.Keys.ToList())
        {
            if (frameCount == 0)
            {
                //Recalculate modifiers based on current positions every X frames
                RecalculateModifier(body, player, camera, maxSoundDistance, intensityDenominator);
                continue;
            }
            //Lerp modifiers based on values predicted by last positions
            LerpModifier(body, player.IsRagdoll, frameCount / everyXFrames);
        }
    }

    private void RecalculateModifier(Rigidbody2D body, IPlayer player, ICameraOperator camera, float maxSoundDistance, float intensityDenominator)
    {
        //If body is null, track intensity of playerBody, pulled from runManager so it updates to lowSpine if ragdoll
        if (body == null || !player.IsRagdoll)
        {
            float intensity = Intensity(player.NormalBody, intensityDenominator);
            _modifiers[body].SetNewTargetIntensity(intensity);
            //Distance and pan are not relevant for playerbody, so continue to next body.
        }
        else
        {
            float intensity = Intensity(body, intensityDenominator);
            float soundDistance = Mathf.Max(Mathf.Abs(body.position.x - player.RagdollBody.position.x) - _distanceBuffer, 0);
            float distance = (maxSoundDistance - soundDistance) / maxSoundDistance;
            float pan = Pan(player.RagdollBody, body, camera);
            _modifiers[body].SetNewTargets(intensity, distance, pan);
        }
    }

    private void LerpModifier(Rigidbody2D body, bool isRagdoll, float t)
    {
        if (body == null || !isRagdoll)
        {
            _modifiers[body].LerpIntensity(t);
        }
        else
        {
            _modifiers[body].LerpModifiers(t);
        }
    }

    public void UpdateZoomModifier(ICameraOperator camera, float zoomLimit, int everyXFrames)
    {
        float frameCount = Time.frameCount % everyXFrames;
        if (frameCount == 0)
        {
            _zoomModifier = AudioManagerUtility.InterpolateValue(camera.DefaultSize, zoomLimit, camera.Camera.orthographicSize, 1, 0.1f);
            _targetZoomModifier = _zoomModifier + (_zoomModifier - _lastZoomModifier);
            _lastZoomModifier = _zoomModifier;
        }
        else
        {
            float t = frameCount / everyXFrames;
            _zoomModifier = Mathf.Lerp(_lastZoomModifier, _targetZoomModifier, t);
        }
    }

    public void ResetZoomModifier(int newZoomModifier = 1)
    {
        _zoomModifier = newZoomModifier;
        _lastZoomModifier = newZoomModifier;
        _targetZoomModifier = newZoomModifier;
    }

    public float GetTotalModifier(Sound sound, bool playerIsRagdoll)
    {
        float totalMod = 1;
        if (sound.trackZoom)
        {
            totalMod *= _zoomModifier;
        }
        if (!playerIsRagdoll)
        {
            return totalMod;
        }
        if (sound.trackDistance)
        {
            totalMod *= _modifiers[sound.localizedSource].distance;
        }
        return totalMod;
    }

    public float GetPan(Sound sound)
    {
        return _modifiers[sound.localizedSource].pan;
    }

    public float GetIntensity(Sound sound)
    {
        return _modifiers[sound.localizedSource].intensity;
    }

    private static float Intensity(Rigidbody2D trackingBody, float denominator)
    {
        return -1 + Mathf.Clamp(trackingBody.velocity.magnitude / denominator, 0, 2);
    }
    private static float Pan(Rigidbody2D playerBody, Rigidbody2D panBody, ICameraOperator camera)
    {
        float halfCamWidth = camera.LeadingCorner.x - camera.Center.x;
        float distanceFromCenter = panBody.position.x - playerBody.position.x;
        return distanceFromCenter / (halfCamWidth * 1.5f);
    }

}
