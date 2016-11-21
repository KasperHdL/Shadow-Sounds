using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

[RequireComponent(typeof(SpriteRenderer))]
public class FollowPlayer : CharacterMovement
{

    private Transform target;
    private PostProcessingAnimator postProcessingAnimator;
    private new SpriteRenderer renderer;
    [Header("Visibility")]
    public bool visibleOverride = false;
    public float visibleToPlayerRange = 5f;

    private bool withinPlayerVisibleRange = false;
    private bool lastFrameRendererEnabled = false;


    [Header("Wandering")]
    public bool allowedToWander = true;
    public bool randomWanderAfterPlayerSight = false;
    private bool hasSeenPlayer = false;

    public bool useGlobalPositionForWanderArea = false;
    public Vector2 wanderingAreaGlobalPosition = Vector2.zero;
    private Vector2 wanderingAreaStartPosition;
    public Vector2 wanderingAreaOffset = Vector2.zero;
    public Vector2 wanderingAreaSize = new Vector2(5,5);
    private float allowedDistanceToWanderPosition = 0.5f;
    [Header("Random Wandering")]
    public float minWanderDistance = 2f;
    public float maxWanderDistance = 5f;
    
    [Header("Attack properties")]
    public float attackDistance = 2f;
    public float attackCooldown = 2;
    public float attackChargeTime = 0.2f;
    public float attackForce = 100;
    public float hitForce = 800;
    private bool attacking;
    private bool charging;
    private bool isPlayingEnmMov = false;

    [Header("Detection")]
    public LayerMask detectionBlockMask;
    public float immidiateDetectionDistance = 6;
    public float detectionTime = 1;
    private float? targetSeenTime = 0;

    private bool isMovingTowardsWanderPosition = false;
    private bool isMovingTowardsKnownPlayerPosition = false;
    private bool isMovingTowardsEnemyWithKnownPlayerPosition = false;
    [HideInInspector] public Vector2 knownPlayerPosition;
    private Vector2 knownEnemyPosition;
    private Vector2 nextWanderPosition;
    private bool playedSound = false;
    public float broadcastToOthersDelay = 0.25f;

    [HideInInspector] public bool canSeePlayer = false;
    [HideInInspector] public float broadcastTime = -1f;

    private Collider2D coll;
    
    public override void Start()
    {
        renderer = GetComponent<SpriteRenderer>();
        coll = GetComponent<Collider2D>();

        target = GameObject.FindWithTag("Player").GetComponent<Transform>();
        if (target == null)
            Debug.LogError("No object tagged 'Player'");

        postProcessingAnimator = Camera.main.GetComponent<PostProcessingAnimator>();

        if(useGlobalPositionForWanderArea)
            wanderingAreaStartPosition = wanderingAreaGlobalPosition;
        else
            wanderingAreaStartPosition = wanderingAreaOffset + (Vector2)transform.position;
        base.Start();
    }

    public override void Update()
    {
        if (target != null && !charging && Vector3.Distance(transform.position, target.position) < attackDistance)
            StartCoroutine(Attack());


        renderer.enabled = charging || visibleOverride || withinPlayerVisibleRange;

        if(renderer.enabled)
            postProcessingAnimator.RegisterEnemyWithinPlayer(this);
        else if(!renderer.enabled)
            postProcessingAnimator.RegitsterEnemyOutsidePlayer(this);

        
        
        DisableMovement = charging;
        if (!isPlayingEnmMov)
        {
            if (renderer.enabled)
            {
                SoundSystem.Play("enemy movement", 1, 0.5f);
                isPlayingEnmMov = true;
            }
        }
        else if (!renderer.enabled)
        {
            SoundSystem.Stop("enemy movement");
            isPlayingEnmMov = false;
        }
    }

    IEnumerator Attack()
    {
        charging = true;
        var dir = target.position - transform.position;
        body.isKinematic = true;
        yield return new WaitForSeconds(attackChargeTime);
        attacking = true;
        body.isKinematic = false;
        body.AddForce(dir * attackForce);
        yield return new WaitForSeconds(attackCooldown);
        attacking = charging = false;
    }
    
    void FixedUpdate () {
        if(isSeeingPlayer())
        {
            //play random ghost sound
            if (!playedSound && !SoundSystem.IsPlaying("ghost sound"))
            {
                SoundSystem.Play("ghost sound",1,0.5f);
                playedSound = true;
            }

            Debug.DrawLine(transform.position, target.position, Color.red, 1f);
            knownPlayerPosition = target.position;
            isMovingTowardsKnownPlayerPosition = true;
            isMovingTowardsEnemyWithKnownPlayerPosition = false;
            isMovingTowardsWanderPosition = false;


            withinPlayerVisibleRange = (target.position - transform.position).magnitude < visibleToPlayerRange;

        }
        else
        {
            broadcastTime = -1f;
            withinPlayerVisibleRange = false;
            playedSound = false;
        }

        Vector2 moveDirection = Vector2.zero;

        if (isMovingTowardsKnownPlayerPosition)
        {
            if (IsAtPosition(knownPlayerPosition))
            {
                isMovingTowardsKnownPlayerPosition = false;
                isMovingTowardsWanderPosition = false;
            }
            moveDirection = knownPlayerPosition - (Vector2)transform.position;

        }else if(isMovingTowardsEnemyWithKnownPlayerPosition){
            if (IsAtPosition(knownEnemyPosition))
            {
                isMovingTowardsEnemyWithKnownPlayerPosition = false;
                isMovingTowardsWanderPosition = false;
            }
            moveDirection = knownEnemyPosition - (Vector2)transform.position;
        }

        if (allowedToWander && !isMovingTowardsKnownPlayerPosition && !isMovingTowardsEnemyWithKnownPlayerPosition)
        {
            if (!isMovingTowardsWanderPosition || IsAtPosition(nextWanderPosition))
            {
                nextWanderPosition = pickWanderPosition();
                isMovingTowardsWanderPosition = true;
            }


            moveDirection = nextWanderPosition - (Vector2)transform.position;
        }
        transform.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * Mathf.Atan2(moveDirection.y, moveDirection.x) - 90);
        Move = moveDirection;

        base.Update();
    }


    bool isSeeingPlayer()
    {
        if (target == null) return false;
        Vector2 delta = target.position - transform.position;
        RaycastHit2D hit = Physics2D.Linecast((Vector2)transform.position, target.position, detectionBlockMask);
        var result = hit.collider.transform == target;

        if (!result) targetSeenTime = null;
        if (result && targetSeenTime == null) targetSeenTime = Time.fixedTime;
        if (delta.magnitude > immidiateDetectionDistance && Time.fixedTime - targetSeenTime < detectionTime) result = false;

        if(!canSeePlayer)
            broadcastTime = Time.time + broadcastToOthersDelay;

        canSeePlayer = result;
        return result;
    }

    public void GoToLocation(Vector2 position){
            Debug.DrawLine(transform.position, position, Color.red, 1f);

            knownEnemyPosition = position;
            isMovingTowardsEnemyWithKnownPlayerPosition = true;
            isMovingTowardsWanderPosition = false;

    }

    Vector2 pickWanderPosition(){
        bool foundLocation = false;
        int numTries = 0;
        const int maxTries = 16;
        Vector2 location = Vector2.zero;

        while(!foundLocation){
            numTries++;

            if(randomWanderAfterPlayerSight && hasSeenPlayer)
                location = getRandomWanderPosition();
            else
                location = getWanderPositionWithinArea();

            
            if(checkIfWanderPositionIsPossible(location)){
                Debug.DrawLine(transform.position,location, Color.green, 1f);
                foundLocation = true;
                break;
            }

            Debug.DrawLine(transform.position,location, Color.red, 1f);

            if (numTries >= maxTries) break;
        }
        if(!foundLocation)
            return Vector2.zero;

        return location;
    }

    Vector2 getWanderPositionWithinArea(){
        return new Vector2(Random.Range(0,wanderingAreaSize.x)-wanderingAreaSize.x/2,Random.Range(0,wanderingAreaSize.y)-wanderingAreaSize.y/2) + wanderingAreaStartPosition;
    }

    Vector2 getRandomWanderPosition(){
        float angle = Random.Range(-180f, 180f);
        float dist = Random.Range(minWanderDistance, maxWanderDistance);
        return (Vector2)transform.position + new Vector2(Mathf.Cos(angle) * dist, Mathf.Sin(angle) * dist);
    }

    bool checkIfWanderPositionIsPossible(Vector2 position){
        return !Physics2D.Linecast((Vector2)transform.position, position, detectionBlockMask);
    }

    bool IsAtPosition(Vector2 pos){
        return (pos - (Vector2)transform.position).magnitude < coll.bounds.size.x / 2;
    }


    void OnCollisionEnter2D(Collision2D collision)
    {
        var player = collision.gameObject.GetComponent<PlayerMovement>();

        if (player != null && attacking) { 
            player.Hit(1);
            attacking = false;

            var avgNormal = collision.contacts.Aggregate(Vector2.zero, (a, c) => a + c.normal) / collision.contacts.Length;
            body.AddForce(avgNormal.normalized * hitForce);
            collision.rigidbody.AddForce(-avgNormal.normalized * hitForce);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, immidiateDetectionDistance);

        if(!allowedToWander)return;
        if(Application.isPlaying){
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(wanderingAreaStartPosition, wanderingAreaSize);
        }else{
            Vector2 center = (useGlobalPositionForWanderArea ? wanderingAreaGlobalPosition :  (Vector2)transform.position + wanderingAreaOffset);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(center, wanderingAreaSize);

        }
    }
}
