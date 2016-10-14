using UnityEngine;
using System.Collections;

public class SpawnHandler : MonoBehaviour {

    public GameObject obstacle_prefab;
    public int amount;

    private Transform player;
    private AudioHandler audioHandler;


    void Start(){
        audioHandler = GetComponent<AudioHandler>();
        player = audioHandler.player;

        spawnObstacles(amount, new Vector2(100,100),player.position.y + 15);
        spawnObstacles(amount, new Vector2(100,100),player.position.y + 115);
        spawnObstacles(amount, new Vector2(100,100),player.position.y + 215);
        spawnObstacles(amount, new Vector2(100,100),player.position.y + 315);
        spawnObstacles(amount, new Vector2(100,100),player.position.y + 415);
        spawnObstacles(amount, new Vector2(100,100),player.position.y + 515);
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
