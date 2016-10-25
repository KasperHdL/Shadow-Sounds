using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour {

    public float moveForce = 500f;
    private Rigidbody2D body;

	void Start () {
        body = GetComponent<Rigidbody2D>();
	
	}
	
	void Update () {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");


        body.AddForce(new Vector3(h,v,0) * moveForce * Time.deltaTime);

	
	}
}
