using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundSystem : MonoBehaviour, ISerializationCallbackReceiver
{
    private static SoundSystem instance;

    public SortedDictionary<int, string> keys = new SortedDictionary<int, string>();
    public SortedDictionary<string, AudioClip> clips = new SortedDictionary<string, AudioClip>();

    [SerializeField]
    [HideInInspector]
    public List<int> sids = new List<int>();
    [SerializeField]
    [HideInInspector]
    public List<string> snames = new List<string>();
    [SerializeField]
    [HideInInspector]
    public List<AudioClip> sclips = new List<AudioClip>();

    private List<AudioSource> sources = new List<AudioSource>();

    public void OnBeforeSerialize()
    {
        sids.Clear();
        snames.Clear();
        sclips.Clear();
        foreach (var key in keys)
        {
            sids.Add(key.Key);
            snames.Add(key.Value);
            sclips.Add(clips[key.Value]);
        }
    }

    public void OnAfterDeserialize()
    {
        for (int i = 0; i < sids.Count; i++)
        {
            keys[sids[i]] = snames[i];
            clips[snames[i]] = sclips[i];
        }
    }

    public void Start()
    {
        if (instance != null) throw new UnityException("Multiple sound systems found!");
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
