using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;
    [SerializeField] public Sound[] sounds;
    private Sound soundtrack;
    private Dictionary<string, Sound> soundDict;

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
        soundDict = new();
    }

    void Start()
    {
        BuildAudioSources();
        BuildSoundtrack();
    }

    public void Refresh()
    {

    }

    public static AudioManager Instance
    {
        get
        {
            if (instance != null)
            {
                return instance;
            }
            Debug.Log("No instance found. Creating instance.");
            GameObject managerObject = new GameObject("GameManager");
            instance = managerObject.AddComponent<AudioManager>();
            return instance;
        }
    }

    public void BuildAudioSources()
    {
        foreach (var s in sounds)
        {
            AssignSourceToSound(s, gameObject.AddComponent<AudioSource>());
            soundDict[s.name] = s;
        }
    }

    public void BuildSoundtrack()
    {
        soundtrack = GameObject.FindGameObjectWithTag("Soundtrack").GetComponent<Soundtrack>().tracks;
        if (soundtrack == null)
        {
            Debug.LogWarning("No soundtrack found!");
            return;
        }
        AssignSourceToSound(soundtrack, gameObject.AddComponent<AudioSource>());
        soundtrack.source.Play();
    }

    public void Play(string name)
    {
        if (!soundDict.ContainsKey(name))
        {
            Debug.LogWarning($"Sound {name} not found!");
            return;
        }
        if (soundDict[name].randomize)
        {
            soundDict[name].source.clip = soundDict[name].Clip();
        }
        soundDict[name].source.Play();

    }

    public static IEnumerator FadeAudioSource(AudioSource source, float finishVolume, float fadeDuration)
    {
        float startVolume = source.volume;
        float timeElapsed = 0;
        while(timeElapsed < fadeDuration)
        {
            timeElapsed += Time.deltaTime;
            float t = timeElapsed / fadeDuration;
            source.volume = Mathf.Lerp(startVolume, finishVolume, t);
            yield return new WaitForFixedUpdate();
        }
        if(finishVolume == 0)
        {
            source.Stop();
        }
    }

    public static void AssignSourceToSound(Sound sound, AudioSource source)
    {
        sound.source = source;
        sound.source.clip = sound.Clip();
        sound.source.volume = sound.volume;
        sound.source.loop = sound.loop;
        sound.source.pitch = sound.pitch;
    }

    public static void LoadSoundToSource(Sound sound, int? clipIndex = null)
    {
#if UNITY_EDITOR
        if(sound.source == null)
        {
            Debug.LogWarning($"Sound {sound.name} has no source!");
            return;
        }
#endif
        LoadSoundToSource(sound, sound.source, clipIndex);
    }

    public static void LoadSoundToSource(Sound sound, AudioSource source, int? clipIndex = null)
    {
        if (clipIndex != null)
        {
            source.clip = sound.Clip((int)clipIndex);
        }
        else
        {
            source.clip = sound.Clip();
        }
        source.volume = sound.volume;
        source.loop = sound.loop;
        source.pitch = sound.pitch;
    }
}
