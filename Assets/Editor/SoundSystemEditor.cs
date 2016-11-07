using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

[CustomEditor(typeof(SoundSystem))]
public class SoundSystemEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var sound = (SoundSystem)target;

        int? keyid = null;
        string newKey = null;
        AudioClip newClip = null;
        int? remove = null;
        int? loc = null;

        EditorGUILayout.BeginVertical();
        foreach (var key in sound.keys)
        {
            var clips = sound.clips[key.Value].data;
            EditorGUILayout.BeginHorizontal();
            var newkey = EditorGUILayout.TextField("", key.Value);
            if (GUILayout.Button("-")) remove = key.Key;
            EditorGUILayout.EndHorizontal();
            foreach (var clip in clips)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField("", clip, typeof(AudioClip), false);
                if (GUILayout.Button("-"))
                {
                    remove = key.Key;
                    loc = clips.IndexOf(clip);
                }
                EditorGUILayout.EndHorizontal();
            }
            var newclip = (AudioClip)EditorGUILayout.ObjectField("", null, typeof(AudioClip), false);
            EditorGUILayout.Space();

            if (newkey != key.Value)
            {
                keyid = key.Key;
                newKey = newkey;
            }
            if (newclip != null)
            {
                keyid = key.Key;
                newClip = newclip;
            }
        }
        EditorGUILayout.EndVertical();

        if (newKey != null)
        {
            if (sound.clips.ContainsKey(newKey))
            {
                Debug.LogWarning("That sound name is already used. Please pick another");
            }
            else
            {
                sound.clips[newKey] = sound.clips[sound.keys[keyid.Value]];
                sound.clips.Remove(sound.keys[keyid.Value]);
                sound.keys[keyid.Value] = newKey;
            }
        }
        if (newClip != null)
        {
            sound.clips[sound.keys[keyid.Value]].data.Add(newClip);
        }
        if (remove.HasValue)
        {
            if (loc.HasValue)
            {
                sound.clips[sound.keys[remove.Value]].data.RemoveAt(loc.Value);
            }
            else
            {
                sound.clips.Remove(sound.keys[remove.Value]);
                sound.keys.Remove(remove.Value);
            }
        }


        if (GUILayout.Button("Add new"))
        {
            if (sound.clips.ContainsKey("new-sound"))
                Debug.LogWarning("A new sound already exists. Rename 'new-sound' first.");
            else
            {
                var max = sound.keys.Count == 0 ? 1 : sound.keys.Keys.Max() + 1;
                sound.keys.Add(max + 1, "new-sound");
                sound.clips.Add("new-sound", new SoundSystem.ClipList());
            }
        }
    }
}
