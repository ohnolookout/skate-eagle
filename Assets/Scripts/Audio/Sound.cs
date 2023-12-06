using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip[] clips;
    [Range(0f, 1f)]
    public float volume = 0.5f;
    [Range(.1f, 3f)]
    public float pitch = 1;
    [Range(0f, 1f)]
    public float volumeVariance = 1;
    [Range(-3f, 3f)]
    public float zoomVariance = 1;
    [Range(0f, 1f)]
    public float pitchVariance = 1;
    [Range(0f, 2f)]
    public float ragdollVolModifier = 1f;
    [Range(0f, 2f)]
    public float ragdollPitchModifier = 1f;
    public bool randomize = false;
    public Rigidbody2D localizedSource;
    public bool loop, trackZoom = false, trackIntensity = false, trackDistance = false, trackPan = false;
    [HideInInspector] public AudioSource source;

    public AudioClip Clip(int? index = null)
    {
#if UNITY_EDITOR
        if(clips.Length == 0)
        {
            Debug.LogWarning($"No clips available in sound {name}");
            return null;
        }
#endif
        if(!randomize)
        {
            if (index != null && index < clips.Length)
            {
                return clips[(int)index];
            }
            return clips[0];
        }
        return clips[Random.Range(0, clips.Length)];
    }

    //Takes intensity between -1 and 1, applies it to variance and adds it to volume.
    public float AdjustedVolume(float intensity, float modifier, bool ragdoll = false)
    {
        if (ragdoll)
        {
            modifier *= ragdollVolModifier;
        }
        if (!trackIntensity)
        {
            intensity = 1;
        }
        return (volume + (volumeVariance * intensity)) * modifier;
    }

    public float AdjustedPitch(float intensity, bool ragdoll = false)
    {
        if (!trackIntensity)
        {
            intensity = 1;
        }
        if (ragdoll)
        {
            return (pitch + (pitchVariance * intensity)) * ragdollPitchModifier;
        }
        return pitch + (pitchVariance * intensity);
    }
}
