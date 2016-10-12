using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioHandler : MonoBehaviour {
    
    public Transform player;
    public float listenXRange = 2f;
    public List<AudioSource> sources;
    private List<int> playInts = new List<int>();
    public int Idx;
    public bool Pitch;

    //void Start()
    //{
    //    //setting up the play positions
    //    for (int i = 1; i < Idx; i*=2)
    //    {
    //        playInts.Add(Idx-i);
    //    }
    //}

	void Update () {
        for(int i = 0;i < sources.Count; i++){
            
            //The relative difference
            int soundIdx = (int)((Idx/((sources[i].transform.position.y - player.position.y))));

            // play the sound at each play position
            //if (playInts.Contains(soundIdx)) 
            //{
            //    sources[i].Play();
            //    Debug.Log("played at " + soundIdx);
            //}


            Vector2 d = sources[i].transform.position - player.position;

            float m = d.sqrMagnitude;
            if(d.y < 0f)
                m *= 2f;

            sources[i].volume = 1f / m;
            if(Pitch)
                sources[i].pitch = 3f / (sources[i].transform.position.y - player.position.y);
            


            if (d.x < -0.5f && d.x > -listenXRange){
                sources[i].panStereo = -1f;
            }else if(d.x > 0.5f && d.x < listenXRange){
                sources[i].panStereo = 1f;
            }else{
                sources[i].panStereo = 0f;
            }
        }
    }
}
