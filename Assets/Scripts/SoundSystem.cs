using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundSystem : MonoBehaviour
{
    private static SoundSystem instance;

    public SortedDictionary<int, string> keys = new SortedDictionary<int, string>();
    public SortedDictionary<string, AudioClip> clips = new SortedDictionary<string, AudioClip>();

    private List<AudioSource> sources = new List<AudioSource>();

    public void Start()
    {
        if(instance != null) throw new UnityException("Multiple sound systems found!");
        instance = this;
    }

    public static void Play(string sound, float pitch, float volume)
    {
        if (instance == null) throw new UnityException("No sound system found!");
        instance.PlaySound(sound, pitch, volume);
    }

    private void PlaySound(string sound, float pitch, float volume)
    {
        //if (source == null) source = gameObject.AddComponent<AudioSource>();
        //source.PlayOneShot(clips[sound]);
    }
}
