using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class FollowPlayer : CharacterMovement
{
    
    public float ChaseSpeed;
    private float normalSpeed;


    private Transform target;
    private PostProcessingAnimator postProcessingAnimator;
    public GameObject sprite;
    [Header("Visibility")]
    public bool visibleOverride = false;
    public float visibleToPlayerRange = 5f;

    [HideInInspector] public bool visible = false;

    private bool withinPlayerVisibleRange = false;

    [Header("Wandering")]
    public bool allowedToWander = true;
    public bool randomWanderAfterPlayerSight = false;
    private bool hasSeenPlayer = false;

    public bool useGlobalPositionForWanderArea = false;
    public Vector2 wanderingAreaGlobalPosition = Vector2.zero;
    private Vector2 wanderingAreaStartPosition;
    public Vector2 wanderingAreaOffset = Vector2.zero;
    public Vector2 wanderingAreaSize = new Vector2(5,5);
    [Header("Random Wandering")]
    public float minWanderDistance = 2f;
    public float maxWanderDistance = 5f;
    
    [Header("Attack properties")]
    public float attackDistance = 2f;
    public float attackCooldown = 2;
    public float attackChargeTime = 0.2f;
    public float attackForce = 100;
    public float hitForce = 800;
    public bool attacking;
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
    private Vector2 lookDirection;
    private bool playedSound = false;
    public float broadcastToOthersDelay = 0.25f;

    [HideInInspector] public bool canSeePlayer = false;
    [HideInInspector] public float broadcastTime = -1f;

    private Collider2D coll;
    private Rigidbody2D body;
    
    public override void Start()
    {
        body = GetComponent<Rigidbody2D>();
        sprite = transform.FindChild("nonvaginabody").gameObject;
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

        normalSpeed = MoveSpeed;
    }

    public override void Update()
    {
        if (target != null && !charging && Vector3.Distance(transform.position, target.position) < attackDistance)
            StartCoroutine(Attack());

        sprite.SetActive(visible || visibleOverride);

        if(visible)
            postProcessingAnimator.RegisterEnemyWithinPlayer(this);
        else
            postProcessingAnimator.RegitsterEnemyOutsidePlayer(this);
        
        DisableMovement = charging;
        if (!isPlayingEnmMov && visible)
        {
            SoundSystem.Play("enemy movement", 1, 0.25f);
            isPlayingEnmMov = true;
        }
        else if (!visible)
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

        visible = charging || withinPlayerVisibleRange;

        if(isSeeingPlayer())
        {
            MoveSpeed = ChaseSpeed;
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
            MoveSpeed = normalSpeed;
        }

        Vector2 moveDirection = Vector2.zero;

        if (isMovingTowardsKnownPlayerPosition)
        {
            if (IsAtPosition(knownPlayerPosition) || (!canSeePlayer && body.velocity.magnitude < .01f))
            {
                isMovingTowardsKnownPlayerPosition = false;
                isMovingTowardsWanderPosition = false;
            }else if(canSeePlayer && body.velocity.magnitude < .01f){
                moveDirection = Vector2.zero;
            }else{
                moveDirection = knownPlayerPosition - (Vector2)transform.position;
            }

        }else if(isMovingTowardsEnemyWithKnownPlayerPosition){

            if (IsAtPosition(knownEnemyPosition) )
            {
                isMovingTowardsEnemyWithKnownPlayerPosition = false;
                isMovingTowardsWanderPosition = false;
            }
            moveDirection = knownEnemyPosition - (Vector2)transform.position;
        }
        else if (allowedToWander)
        {
            if (!isMovingTowardsWanderPosition || IsAtPosition(nextWanderPosition))
            {
                nextWanderPosition = pickWanderPosition();
                isMovingTowardsWanderPosition = true;
            }


            moveDirection = nextWanderPosition - (Vector2)transform.position;
        }
        if (body.velocity.magnitude > 0.01f) lookDirection = body.velocity;
        transform.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * Mathf.Atan2(lookDirection.y, lookDirection.x) - 90);
        Move = Vector2.ClampMagnitude(moveDirection,maxWanderDistance);

        base.Update();
    }


    bool isSeeingPlayer()
    {
        if (target == null) return false;
        Vector2 delta = target.position - transform.position;
        RaycastHit2D hit = Physics2D.Linecast(transform.position, target.position, detectionBlockMask);
        var result = false;
        if (hit.collider != null)
            result = hit.collider.transform == target;

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

            if (randomWanderAfterPlayerSight && hasSeenPlayer)
                location = getRandomWanderPosition();
            else
            {
                location = getWanderPositionWithinArea();
                //clamping location to maxWanderDistance
                location = Vector2.ClampMagnitude((location - (Vector2)transform.position), maxWanderDistance) + (Vector2)transform.position;

            }


            if (checkIfWanderPositionIsPossible(location)){
                Debug.DrawLine(transform.position,location, Color.green, 1f);
                foundLocation = true;
                break;
            }

            Debug.DrawLine(transform.position,location, Color.red, 1f);

            if (numTries >= maxTries)
            {
                //Debug.Log("tried too much");
                break;
            }
        }
        if(!foundLocation )
            return transform.position;

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


    void OnCollisionEnter2D(Collision2D collision) {
        HandleCollision(collision);
    }
    void OnCollisionStay2D(Collision2D collision) {
        HandleCollision(collision);
    }

    void HandleCollision(Collision2D collision) {
        var player = collision.gameObject.GetComponent<PlayerMovement>();

        if(player != null && attacking) {
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
