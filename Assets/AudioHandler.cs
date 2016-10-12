using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioHandler : MonoBehaviour {
    
    public Transform player;
    public List<AudioSource> sources;

	void Update () {
        for(int i = 0;i < sources.Count; i++){
            if(sources[i].transform.position.x < player.position.x -.5f){
                sources[i].panStereo = -1f;
                sources[i].volume = 1f;
            }else if(sources[i].transform.position.x > player.position.x + .5f){
                sources[i].panStereo = 1f;
                sources[i].volume = 1f;
            }else{
                sources[i].volume = 0f;
            }
        }
	
	}
}
