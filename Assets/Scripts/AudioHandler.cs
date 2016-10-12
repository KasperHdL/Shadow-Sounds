using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioHandler : MonoBehaviour {
    
    public Transform player;
    public float listenXRange = 2f;
    public List<AudioSource> sources;

	void Update () {
        for(int i = 0;i < sources.Count; i++){
            Vector2 d = sources[i].transform.position - player.position;

            float m = d.sqrMagnitude;
            if(d.y < 0f)
                m *= 2f;

            sources[i].volume = 1f / m;

            if(d.x < -0.5f && d.x > -listenXRange){
                sources[i].panStereo = -1f;
            }else if(d.x > 0.5f && d.x < listenXRange){
                sources[i].panStereo = 1f;
            }else{
                sources[i].panStereo = 0f;
            }
        }
    }
}
