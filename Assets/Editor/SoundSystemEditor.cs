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
        var sound = (SoundSystem) target;

        var keyChanges = new Dictionary<int, string>();
        var valueChanges = new Dictionary<int, AudioClip>();
        int? remove = null;

        EditorGUILayout.BeginVertical();
        foreach (var key in sound.keys)
        {
            var clip = sound.clips[key.Value];
            EditorGUILayout.BeginHorizontal();
            var newkey = EditorGUILayout.TextField("", key.Value);
            if(GUILayout.Button("-")) remove = key.Key;
            EditorGUILayout.EndHorizontal();
            var newclip = (AudioClip)EditorGUILayout.ObjectField("", clip, typeof(AudioClip), false);
            EditorGUILayout.Space();

            if (newkey != key.Value) keyChanges.Add(key.Key, newkey);
            if (newclip != clip) valueChanges.Add(key.Key, newclip);
        }
        EditorGUILayout.EndVertical();

        foreach (var change in keyChanges)
        {
            if (sound.clips.ContainsKey(change.Value))
            {
                Debug.LogWarning("That sound name is already used. Please pick another");
            }
            else
            {
                sound.clips[change.Value] = sound.clips[sound.keys[change.Key]];
                sound.clips.Remove(sound.keys[change.Key]);
                sound.keys[change.Key] = change.Value;
            }
        }
        foreach (var change in valueChanges)
        {
            sound.clips[sound.keys[change.Key]] = change.Value;
        }
        if (remove.HasValue)
        {
            sound.clips.Remove(sound.keys[remove.Value]);
            sound.keys.Remove(remove.Value);
        }


        if (GUILayout.Button("Add new"))
        {
            if(sound.clips.ContainsKey("new-sound"))
                Debug.LogWarning("A new sound already exists. Rename 'new-sound' first.");
            else
            {
                var max = sound.keys.Count == 0 ? 1 : sound.keys.Keys.Max() + 1;
                sound.keys.Add(max+1, "new-sound");
                sound.clips.Add("new-sound", null);
            }
        }
    }
}
