using UnityEngine;
using System.Collections;

public class SwarmingHandler : MonoBehaviour {

    public FollowPlayer[] enemies;
    public LayerMask detectionBlockMask;

	// Use this for initialization
	void Start () {
        enemies = FindObjectsOfType<FollowPlayer>();
	
	}
	
	// Update is called once per frame
	void Update () {
        if(enemies.Length < 2) return;

        for(int i = 0; i < enemies.Length; i++){
            if(enemies[i].canSeePlayer){

                for(int j = 0; j < enemies.Length; j++){
                    if(enemies[j].canSeePlayer) continue;

                    Vector2 delta = enemies[i].transform.position - enemies[j].transform.position;

                    bool hit = Physics2D.Linecast((Vector2)enemies[i].transform.position - delta.normalized * enemies[i].transform.localScale.x, (Vector2)enemies[j].transform.position + delta.normalized * enemies[i].transform.localScale.x, detectionBlockMask);

                    if(!hit){
                        //can see eachother
                        
                        enemies[j].GoToLocation(enemies[i].transform.position);


                    }

                }
            }
        
        }
    }
}
