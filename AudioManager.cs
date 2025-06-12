using UnityEngine.Audio;
using System;
using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    [Range(0, 1)] public float volume = 1;
    [Range(-3, 3)] public float pitch = 1;
    public bool loop = false;
    public bool playOnAwake = false;
    [HideInInspector] public AudioSource source;
}

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;

    public static AudioManager instance;

    void Awake()
    {
        // ——— Singleton + Persist ———
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        // ——— Set up each Sound’s AudioSource ———
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.playOnAwake = s.playOnAwake;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
            if (s.playOnAwake)
                s.source.Play();
        }
    }

    /// <summary>
    /// Play a sound by name.
    /// </summary>
    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning($"[AudioManager] Sound '{name}' not found!");
            return;
        }
        s.source.Play();
    }

    /// <summary>
    /// Stop a sound by name.
    /// </summary>
    public void Stop(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s != null) s.source.Stop();
    }

    /// <summary>
    /// Set the volume of **all** sounds (e.g. your SFX slider).
    /// </summary>
    public void SetGlobalSFXVolume(float value)
    {
        foreach (var s in sounds)
        {
            // if you want to separate music vs SFX by name convention,
            // you can skip ones named "Music" or similar.
            s.source.volume = value;
            s.volume = value;  // keep your serialized value in sync
        }
    }
}
