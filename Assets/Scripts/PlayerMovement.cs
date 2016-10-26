using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour {

    public float moveSpeed = 50.0f;
    [HideInInspector]
    public Vector2 viewDirection;
    private Rigidbody2D body;

	void Start () {
        body = GetComponent<Rigidbody2D>();
	
	}
	
	void Update () {
        var v = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")) * moveSpeed / body.mass;

	    body.AddForce(v - new Vector3(body.velocity.x, body.velocity.y), ForceMode2D.Impulse);

        Vector2 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        viewDirection = mouse - (Vector2)transform.position;

        transform.rotation = Quaternion.Euler(0,0, Mathf.Rad2Deg * Mathf.Atan2(viewDirection.y, viewDirection.x));
	
	}
}
