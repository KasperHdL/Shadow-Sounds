using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

    public LevelHandler levelHandler;
    public float forwardSpeed;
    public KeyCode leftKey;
    public KeyCode rightKey;

	void Start () {
	
	}
	
	void Update () {
        Vector3 d = Vector3.up * forwardSpeed * Time.deltaTime;
        if(Input.GetKeyDown(leftKey))
            d.x = -1f;
        else if(Input.GetKeyDown(rightKey))
            d.x = 1f;
	
        transform.position += d;
	}

    void OnCollisionEnter2D(Collision2D coll){
        if(coll.gameObject.tag == "Obstacle"){
            transform.position = levelHandler.GetResetPosition();


        }
    }
}
