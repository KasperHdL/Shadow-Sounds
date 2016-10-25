using UnityEngine;
using System.Collections;

public class TrackingCamera : MonoBehaviour {

	public Rigidbody2D target;

	public float smoothFactor = 0.8f;

	public float offsetZ  = -10;
	public float viewOffsetMultiplier = 2f;

    void Start(){
        if(target == null){
            Debug.LogWarning("Camera has no target, gonna try to find an object tagged 'Player'");
            target = GameObject.FindWithTag("Player").GetComponent<Rigidbody2D>();
            if(target == null)
                Debug.LogError("No object tagged 'Player'");
        }

    }
	
	void FixedUpdate () {
        
        Vector3 delta = target.transform.position - transform.position;
        delta.z = 0;
        Vector3 desiredPosition = (Vector2)transform.position + (Vector2)delta.normalized * delta.sqrMagnitude + target.velocity * viewOffsetMultiplier;

        desiredPosition.z = offsetZ;

        delta = desiredPosition - transform.position;
        transform.position += (Vector3)delta * smoothFactor * Time.deltaTime;


	}
}
