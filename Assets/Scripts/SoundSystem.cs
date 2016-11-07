using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundSystem : MonoBehaviour, ISerializationCallbackReceiver
{
    [SerializeField]
    [System.Serializable]
    public class ClipList
    {
        [SerializeField] public List<AudioClip> data = new List<AudioClip>();
    }

    private static SoundSystem instance;

    public SortedDictionary<int, string> keys = new SortedDictionary<int, string>();
    public SortedDictionary<string, ClipList> clips = new SortedDictionary<string, ClipList>();

    [SerializeField]
    [HideInInspector]
    public List<int> sids = new List<int>();
    [SerializeField]
    [HideInInspector]
    public List<string> snames = new List<string>();
    [SerializeField]
    [HideInInspector]
    public List<ClipList> sclips = new List<ClipList>();

    private AudioSource source;

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

    public static void Play(string sound, float pitch, float volume, float delay)
    {
        if (instance == null) throw new UnityException("No sound system found!");
        instance.StartCoroutine(instance.PlaySound(sound, pitch, volume, delay));
    }

    private IEnumerator PlaySound(string sound, float pitch, float volume, float delay)
    {
        if (!clips.ContainsKey(sound) || clips[sound].data.Count == 0)
        {
            Debug.LogError("Sound not found: "+sound);
            yield break;
        }
        if (source == null) source = gameObject.AddComponent<AudioSource>();

        yield return new WaitForSeconds(delay);

        var clip = clips[sound].data[Random.Range(0, clips[sound].data.Count)];
        source.PlayOneShot(clip);
    }
}
