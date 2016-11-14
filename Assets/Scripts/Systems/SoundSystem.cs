using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class SoundSystem : MonoBehaviour, ISerializationCallbackReceiver
{
    [SerializeField]
    [System.Serializable]
    public class ClipList
    {
        [SerializeField]
        public List<AudioClip> data = new List<AudioClip>();
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

    private Dictionary<string, List<AudioSource>> sources = new Dictionary<string, List<AudioSource>>();

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

    public static void Play(string sound, float pitch = 1, float volume = 1, float delay = 0, float? duration = null, bool loop = false)
    {
        if (instance == null) throw new UnityException("No sound system found!");
        if (loop && IsPlaying(sound))
            return; 

        //to prevent the above to trigger
        //instance.sources.Add(sound,new List<AudioSource>());

        instance.StartCoroutine(instance.PlaySound(sound, pitch, volume, delay, duration, loop));
    }
    public static void Stop(string sound)
    {
        if (instance == null) throw new UnityException("No sound system found!");
        instance.DoStop(sound);
    }
    public static bool IsPlaying(string sound)
    {
        if (instance == null) throw new UnityException("No sound system found!");
        return instance.sources.ContainsKey(sound) && instance.sources[sound].Count > 0;
    }

    private IEnumerator PlaySound(string sound, float pitch, float volume, float delay, float? duration, bool loop)
    {
        if (!clips.ContainsKey(sound) || clips[sound].data.Count == 0)
        {
            Debug.LogError("Sound not found: " + sound);
            yield break;
        }

        yield return new WaitForSeconds(delay);

        var source = gameObject.AddComponent<AudioSource>();
        source.pitch = pitch;
        source.volume = volume;
        source.clip = clips[sound].data[Random.Range(0, clips[sound].data.Count)];
        source.loop = loop;
        source.Play();

        if (!sources.ContainsKey(sound)) sources.Add(sound, new List<AudioSource>());
        sources[sound].Add(source);

        if (loop) yield break;

        if (!duration.HasValue)
        {
            yield return new WaitWhile(() => source.isPlaying);
            Destroy(source);
            sources[sound].Remove(source);
        }
        else
        {
            yield return new WaitForSeconds(duration.Value);
            StartCoroutine(FadeOut(sound, source));
            sources[sound].Remove(source);
        }
    }

    private void DoStop(string sound)
    {
        if (!sources.ContainsKey(sound)) return;
        foreach (var source in sources[sound])
        {
            StartCoroutine(FadeOut(sound, source));
        }
        sources[sound].Clear();
    }

    private IEnumerator FadeOut(string sound, AudioSource source)
    {
        var faceOutTime = 0.5f;
        var t = faceOutTime;
        var v0 = source.volume;
        while (t > 0)
        {
            source.volume = Mathf.Lerp(0, v0, t / faceOutTime);

            yield return new WaitForFixedUpdate();
            t -= Time.fixedDeltaTime;
        }
        Destroy(source);
        if(sources[sound].Contains(source))
            sources[sound].Remove(source);
    }
}
