using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

[RequireComponent(typeof(SpriteRenderer))]
public class FollowPlayer : CharacterMovement
{

    public Transform target;
    private PostProcessingAnimator postProcessingAnimator;
    private new SpriteRenderer renderer;
    public bool visibleOverride = false;

    private bool showEnemy = false;
    private bool lastFrameRendererEnabled = false;
    public float visibleRange = 1.5f;

    public LayerMask detectionBlockMask;

    public bool allowedToWander = true;
    public float raycastStartRadius = 1f;
    public float minWanderDistance = 2f;
    public float maxWanderDistance = 5f;
    
    public float attackDistance = 2f;
    public float attackCooldown = 2;
    public float attackChargeTime = 0.2f;
    public float attackForce = 100;
    public float hitForce = 800;
    private bool attacking;
    private bool charging;
    private bool isPlayingEnmMov = false;

    public float immidiateDetectionDistance = 6;
    public float detectionTime = 1;
    private float? targetSeenTime = 0;

    private bool isMovingTowardsWanderPosition = false;
    private bool isMovingTowardsKnownPlayerPosition = false;
    private Vector2 knownPlayerPosition;
    private Vector2 nextWanderPosition;
    private bool playedSound = false;
    
    public override void Start()
    {
        renderer = GetComponent<SpriteRenderer>();
        if (target == null)
        {
            Debug.LogWarning("Enemy has no target, gonna try to find an object tagged 'Player'");
            target = GameObject.FindWithTag("Player").GetComponent<Transform>();
            if (target == null)
                Debug.LogError("No object tagged 'Player'");
        }
        postProcessingAnimator = Camera.main.GetComponent<PostProcessingAnimator>();
        base.Start();
    }

    public override void Update()
    {
        if (target != null && !charging && Vector3.Distance(transform.position, target.position) < attackDistance)
            StartCoroutine(Attack());


        renderer.enabled = charging || visibleOverride || showEnemy;
        if(renderer.enabled && !lastFrameRendererEnabled)
            postProcessingAnimator.FlickerInWorld();
        else if(!renderer.enabled && lastFrameRendererEnabled)
            postProcessingAnimator.FlickerOutWorld();
        lastFrameRendererEnabled = renderer.enabled;
        
        
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
            isMovingTowardsWanderPosition = false;


            showEnemy = (target.position - transform.position).magnitude < visibleRange;

        }
        else
        {
            showEnemy = false;
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

        }
        if (allowedToWander && !isMovingTowardsKnownPlayerPosition)
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
        RaycastHit2D hit = Physics2D.Linecast((Vector2)transform.position + delta.normalized * raycastStartRadius, target.position, detectionBlockMask);
        var result = hit.collider.transform == target;

        if (!result) targetSeenTime = null;
        if (result && targetSeenTime == null) targetSeenTime = Time.fixedTime;
        if (delta.magnitude > immidiateDetectionDistance && Time.fixedTime - targetSeenTime < detectionTime) result = false;

        return result;
    }

    Vector2 pickWanderPosition()
    {
        bool foundLocation = false;
        int numTries = 0;
        int maxTries = 16;
        Vector2 location = Vector2.zero;
        while (!foundLocation)
        {
            numTries++;
            float angle = Random.Range(-180f, 180f);
            float dist = Random.Range(minWanderDistance, maxWanderDistance);
            location = (Vector2)transform.position + new Vector2(Mathf.Cos(angle) * dist, Mathf.Sin(angle) * dist);
            Vector2 delta = location - (Vector2)transform.position;

            if(!Physics2D.Linecast((Vector2)transform.position + delta.normalized * raycastStartRadius, location, detectionBlockMask)){
                //found a location
                foundLocation = true;
                Debug.DrawLine(transform.position, location, Color.green, 1f);
            }

            if (numTries >= maxTries) break;
        }
        return location;

    }


    bool IsAtPosition(Vector2 pos)
    {
        return (pos - (Vector2)transform.position).magnitude < 0.5f;
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
    }
}
