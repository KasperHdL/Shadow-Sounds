using UnityEngine;
using System.Collections;

public class SpawnHandler : MonoBehaviour {

    public GameObject obstacle_prefab;
    public int numSpawns;
    public Vector2 spawnArea;

    private Transform player;
    private AudioHandler audioHandler;


    void Start(){
        audioHandler = GetComponent<AudioHandler>();
        player = audioHandler.player;

        spawnObstacles(numSpawns, spawnArea, player.position.y + 15);
    }

    void spawnObstacles(int num, Vector2 area, float yPos){

        for(int i = 0; i < num; i++){
            int x = (int)(Random.Range(0,area.x) - area.x/2);
            float y = Random.Range(0,area.y) + yPos;
            
            GameObject g = Instantiate(obstacle_prefab, new Vector3(x,y,0), Quaternion.identity) as GameObject;
            audioHandler.sources.Add(g.GetComponent<AudioSource>());
        }
    }
}
