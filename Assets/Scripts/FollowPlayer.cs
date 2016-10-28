using UnityEngine;
using System.Collections;

public class FollowPlayer : MonoBehaviour {

	public Rigidbody2D target;
    private Rigidbody2D body;
    public float moveForce;

	// Use this for initialization
    void Start(){
        body = GetComponent<Rigidbody2D>();
        if(target == null){
            /* Finds the player and follows it(To be discussed) */
            Debug.LogWarning("Enemy has no target, gonna try to find an object tagged 'Player'");
            target = GameObject.FindWithTag("Player").GetComponent<Rigidbody2D>();
            if(target == null)
                Debug.LogError("No object tagged 'Player'");
        }

    }
	
	// Update is called once per frame
	void Update () {

        Vector3 delta = target.transform.position - transform.position;
        body.AddForce(delta.normalized * moveForce * Time.deltaTime);

	}
}
