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
    public float volume;
    [Range(.1f, 3f)]
    public float pitch;
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
}
