using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class AudioManagerUtility
{

    public static AudioSource FirstAvailableSource(AudioSource[] audioSources)
    {
        for (int i = 2; i < audioSources.Length; i++)
        {
            if (!audioSources[i].isPlaying)
            {
                return audioSources[i];
            }
        }
        Debug.LogWarning("No available audiosource. Overwriting final audiosource.");
        return audioSources[^1];
    }
    public static void LoadSoundWithModifiers(Sound sound, AudioSource source, SoundModifierManager modifierManager, bool isRagdoll)
    {
        source.clip = sound.Clip();
        float modifier = modifierManager.GetTotalModifier(sound, isRagdoll);
        float intensity = modifierManager.GetIntensity(sound);
        source.volume = sound.AdjustedVolume(intensity, modifier, isRagdoll);
        source.panStereo = modifierManager.GetPan(sound);
        source.pitch = sound.AdjustedPitch(intensity, isRagdoll);
        sound.source = source;
    }
    public static void UpdateLoopSource(AudioSource loopSource, Sound sound, SoundModifierManager modifierManager, bool isRagdoll)
    {
        //Apply updated modifiers
        //Key "null" is sometimes not found in modifiers when called by Update on startup.
        float modifier = modifierManager.GetTotalModifier(sound, isRagdoll);
        float intensity = modifierManager.GetIntensity(sound);
        loopSource.volume = sound.AdjustedVolume(intensity, modifier, isRagdoll);
        loopSource.panStereo = modifierManager.GetPan(sound);
        loopSource.pitch = sound.AdjustedPitch(intensity, isRagdoll);
    }
    public static IEnumerator FadeAudioSource(AudioManager audioManager, AudioSource source, float finishVolume, float fadeDuration, bool trackZoom)
    {
        float startVolume = source.volume;
        float timeElapsed = 0;
        while (timeElapsed < fadeDuration)
        {
            timeElapsed += Time.deltaTime;
            float t = timeElapsed / fadeDuration;
            if (trackZoom)
            {
                source.volume = Mathf.Lerp(startVolume, finishVolume, t) * audioManager.ZoomModifier;                
            }
            else
            {
                source.volume = Mathf.Lerp(startVolume, finishVolume, t);
            }
            yield return null;
        }
        source.Stop();
        AudioManager.Instance.RemovePlayingSound(source, true);
    }

    public static IEnumerator TimedInZoomOut(AudioManager audioManager, AudioSource source, ICameraOperator camera, float maxVolume, float initialDelay, float fadeInTime, float cameraSizeThreshold)
    {
        source.volume = 0;
        source.Play();
        while (camera.Camera.orthographicSize < cameraSizeThreshold)
        {
            yield return null;
        }
        float timeElapsed = 0;

        while (timeElapsed < initialDelay)
        {
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        if (!camera.CameraZoomOut)
        {
            audioManager.RemovePlayingSound(source, true);
            audioManager.StopLoop(source);
            yield break;
        }
        float maxCamSize = camera.Camera.orthographicSize;
        timeElapsed = 0;
        while (timeElapsed < fadeInTime && camera.Camera.orthographicSize >= maxCamSize * 0.7f)
        {
            source.volume = Mathf.Lerp(0, maxVolume, timeElapsed / fadeInTime);
            maxCamSize = Mathf.Max(maxCamSize, camera.Camera.orthographicSize);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        while (camera.Camera.orthographicSize >= maxCamSize)
        {
            yield return null;
        }
        float fadeOutTime = Mathf.Max(timeElapsed, 1.5f);
        timeElapsed = 0;
        float peakVolume = source.volume;
        while (timeElapsed < fadeOutTime)
        {
            source.volume = Mathf.Lerp(peakVolume, 0, timeElapsed / fadeOutTime);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        audioManager.StopLoop(source);
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

    public static float StopDuration(float xVelocity)
    {
        return (Mathf.Log(1 / xVelocity) / -0.083f) / 50;
        //Denominator should be equal to ln(1 - deceleration coeffeciient).
    }

    public static float InterpolateValue(float startVal, float endVal, float currentVal, float returnFloor, float returnCeiling)
    {
        // Ensure currentVal is within the specified range
        float clampedCurrentVal = Mathf.Clamp(currentVal, startVal, endVal);

        // Calculate the normalized position of currentVal between startVal and endVal
        float normalizedPosition = (clampedCurrentVal - startVal) / (endVal - startVal);

        // Interpolate the value between returnFloor and returnCeiling based on the normalized position
        float result = returnFloor + normalizedPosition * (returnCeiling - returnFloor);

        return result;
    }
}
