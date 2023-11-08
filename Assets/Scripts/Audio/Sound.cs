using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip[] clips;
    public bool loop;
    [Range(0f, 1f)]
    public float volume = 0.5f;
    [Range(.1f, 3f)]
    public float pitch = 1;
    [Range(0f, 1f)]
    public float volumeVariance = 1;
    [Range(0f, 1f)]
    public float pitchVariance = 1;
    public bool randomize = false;
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
    public float AdjustedVolume(float intensity)
    {
#if UNITY_EDITOR
        if (Mathf.Abs(intensity) > 1)
        {
            Debug.LogWarning($"Intensity for adjusted volume in sound {name} is too big: {intensity}");
        }
#endif
        /*Debug.Log($"Adjusting volume for {name}. Volume: {volume} Variance: {volumeVariance} Intensity: {intensity}");
        Debug.Log($"Result: {volume + (volumeVariance * intensity)}");*/
        return volume + (volumeVariance * intensity);
    }

    public float AdjustedPitch(float intensity)
    {
        return pitch + (pitchVariance * intensity);
    }
}
