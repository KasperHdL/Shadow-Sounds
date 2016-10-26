using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour {

    public float moveForce = 500f;
    [HideInInspector]
    public Vector2 viewDirection;
    private Rigidbody2D body;

	void Start () {
        body = GetComponent<Rigidbody2D>();
	
	}
	
	void Update () {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        body.AddForce(new Vector3(h,v,0) * moveForce * Time.deltaTime);


        Vector2 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        viewDirection = mouse - (Vector2)transform.position;

        transform.rotation = Quaternion.Euler(0,0, Mathf.Rad2Deg * Mathf.Atan2(-viewDirection.y, viewDirection.x));
	
	}
}
