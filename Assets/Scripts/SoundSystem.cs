using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundSystem : MonoBehaviour
{
    private static SoundSystem instance;

    public Dictionary<string, AudioClip> clips;

    private AudioSource source;
    
    public void Start()
    {
        if(instance != null) throw new UnityException("Multiple sound systems found!");
        instance = this;
    }

    public static void Play(string sound, float pitch, float volume)
    {
        if (instance == null)
            new GameObject("Sound System", typeof(SoundSystem));
        instance.PlaySound(sound, pitch, volume);
    }

    private void PlaySound(string sound, float pitch, float volume)
    {
        if (source == null) source = gameObject.AddComponent<AudioSource>();
        source.PlayOneShot(clips[sound]);
    }
}
