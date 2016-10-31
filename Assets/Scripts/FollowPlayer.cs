using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshRenderer))]
public class FollowPlayer : MonoBehaviour
{

    public Transform target;
    private Rigidbody2D body;
    private MeshRenderer renderer;
    public float moveForce;
    public float visibleDistance = 1.5f;

    public bool allowedToWander = true;
    public float raycastStartRadius = 1f;
    public float minWanderDistance = 2f;
    public float maxWanderDistance = 5f;

    private bool isMovingTowardsWanderPosition = false;
    private bool isMovingTowardsKnownPlayerPosition = false;
    private Vector2 knownPlayerPosition;
    private Vector2 nextWanderPosition;

	// Use this for initialization
    void Start(){
        body = GetComponent<Rigidbody2D>();
        renderer = GetComponent<MeshRenderer>();
        if(target == null){
            Debug.LogWarning("Enemy has no target, gonna try to find an object tagged 'Player'");
            target = GameObject.FindWithTag("Player").GetComponent<Transform>();
            if(target == null)
                Debug.LogError("No object tagged 'Player'");
        }

    }

    void Update()
    {
        if (target != null)
        {
            if (Vector3.Distance(transform.position, target.position) < visibleDistance)
            {
                renderer.enabled = true;
            }
            else
            {
                renderer.enabled = false;
            }
        }
        else
        {
            renderer.enabled = false;
        }
    }
	
	void FixedUpdate () {
        if(isSeeingPlayer()){
            Debug.DrawLine(transform.position, target.position, Color.red, 1f);
            knownPlayerPosition = target.position;
            isMovingTowardsKnownPlayerPosition = true;
            isMovingTowardsWanderPosition = false;

        }

        Vector2 moveDirection = Vector2.zero;

        if(isMovingTowardsKnownPlayerPosition){
            if(isAtPosition(knownPlayerPosition)){
                isMovingTowardsKnownPlayerPosition = false;
                isMovingTowardsWanderPosition = false;
            }
            moveDirection = knownPlayerPosition - (Vector2)transform.position;

        }
        if(allowedToWander && !isMovingTowardsKnownPlayerPosition){
            if(!isMovingTowardsWanderPosition || isAtPosition(nextWanderPosition)){
                nextWanderPosition = pickWanderPosition();
                isMovingTowardsWanderPosition = true;
            }
                

            moveDirection = nextWanderPosition - (Vector2) transform.position;
        }

        body.AddForce(moveDirection.normalized * moveForce * Time.deltaTime);

	}


    bool isSeeingPlayer(){
        Vector2 delta = target.position - transform.position;
        RaycastHit2D hit = Physics2D.Linecast((Vector2)transform.position + delta.normalized * raycastStartRadius, target.position);
        return hit.collider.transform == target;
    }

    Vector2 pickWanderPosition(){
        bool foundLocation = false;
        int numTries = 0;
        int maxTries = 16;
        Vector2 location = Vector2.zero;
        while(!foundLocation){
            numTries++;
            float angle = Random.Range(-180f,180f);
            float dist = Random.Range(minWanderDistance,maxWanderDistance);
            location = (Vector2)transform.position + new Vector2(Mathf.Cos(angle) * dist, Mathf.Sin(angle) * dist);
            Vector2 delta = location - (Vector2)transform.position;

            if(!Physics2D.Linecast((Vector2)transform.position + delta.normalized * raycastStartRadius, location)){
                //found a location
                foundLocation = true;
                Debug.DrawLine(transform.position, location, Color.green, 1f);
            }

            if(numTries >= maxTries) break;
        }
        return location;

    }


    bool isAtPosition(Vector2 pos){
        return (pos - (Vector2) transform.position).magnitude < 0.5f;

    }


}
