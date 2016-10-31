using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FollowPlayer : MonoBehaviour {

	public Transform target;
    private Rigidbody2D body;
    public float moveForce;

    public bool allowedToWander = true;
    public float raycastStartRadius = 1f;
    public float minWanderDistance = 2f;
    public float maxWanderDistance = 5f;


    private List<AudioClip> ghostSounds = new List<AudioClip>();
    public AudioClip ghost1, ghost2, ghost3, ghost4, ghost5, ghost6, ghost7;
    private AudioSource audioSource;
    public GameObject sourceContainer;

    private bool isMovingTowardsWanderPosition = false;
    private bool isMovingTowardsKnownPlayerPosition = false;
    private Vector2 knownPlayerPosition;
    private Vector2 nextWanderPosition;
    private bool playedSound = false;

	// Use this for initialization
    void Start(){
        body = GetComponent<Rigidbody2D>();
        if(target == null){
            Debug.LogWarning("Enemy has no target, gonna try to find an object tagged 'Player'");
            target = GameObject.FindWithTag("Player").GetComponent<Transform>();
            if(target == null)
                Debug.LogError("No object tagged 'Player'");
        }

        audioSource = sourceContainer.AddComponent<AudioSource>();
        audioSource.volume = 0.2f;
        ghostSounds.Add(ghost1);
        ghostSounds.Add(ghost2);
        ghostSounds.Add(ghost3);
        ghostSounds.Add(ghost4);
        ghostSounds.Add(ghost5);
        ghostSounds.Add(ghost6);
        ghostSounds.Add(ghost7);

    }
	
	void FixedUpdate () {
        if(isSeeingPlayer())
        {
            //play random ghost sound
            if (!playedSound && !audioSource.isPlaying)
            {
                audioSource.clip = ghostSounds[Random.Range(0, ghostSounds.Count-1)];
                audioSource.Play();
                playedSound = true;
            }

            Debug.DrawLine(transform.position, target.position, Color.red, 1f);
            knownPlayerPosition = target.position;
            isMovingTowardsKnownPlayerPosition = true;
            isMovingTowardsWanderPosition = false;

        }
        else
        {
            playedSound = false;
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
