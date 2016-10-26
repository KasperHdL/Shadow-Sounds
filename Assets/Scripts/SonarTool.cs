using UnityEngine;
using System.Collections;

public class SonarTool : MonoBehaviour {

    private PlayerMovement player;

    public float coneAngle = 30f;
    public float coneIncrement = 1f;

    private float coneAngleRad;
    private float coneIncrementRad;

    public float distance  = 20f;
    public float soundDelayPerMeter = 1f;


    public GameObject sourceContainer;
    public AudioSource[] sources;
    public RaycastHit2D[] hits;

    public AudioClip hitSound;
    public AudioClip noHitSound;

    public float noHitVolume = 0.5f;
    public float hitPitch    = 3f;
    public float hitVolume   = 5f;

    private float lastShotTime = 0f;
    public float shotCooldown = 1f;


	// Use this for initialization
	void Start () {
        player = GetComponent<PlayerMovement>();

        coneAngleRad = Mathf.Deg2Rad * coneAngle;
        coneIncrementRad = Mathf.Deg2Rad * coneIncrement;

        sources = new AudioSource[(int)((coneAngle * 2) / coneIncrement)];
        hits    = new RaycastHit2D[sources.Length];

        for(int i = 0;i<sources.Length;i++)
            sources[i] = sourceContainer.AddComponent<AudioSource>();

	}


    void FixedUpdate(){
        if(lastShotTime + shotCooldown < Time.time && Input.GetButtonDown("Fire1")){
            Shoot();
            lastShotTime = Time.time;
        }
    }
	
	// Update is called once per frame
	void Shoot() {
        Vector3 viewDir = player.viewDirection.normalized;
        float angle = Mathf.Atan2(-viewDir.y, viewDir.x);

        float startAngle = angle - coneAngleRad;
        for(int i = 0; i < hits.Length; i++){

            float a = startAngle + coneIncrementRad * i;

            Vector3 d = new Vector3(Mathf.Cos(a), Mathf.Sin(-a),0); 

            hits[i] = Physics2D.Raycast(player.transform.position + d, d, distance);

            //draw debug
            if(hits[i].collider != null){
                Debug.DrawLine(player.transform.position, hits[i].point, Color.white, shotCooldown);
            }else{
                Debug.DrawLine(player.transform.position, player.transform.position + d * distance, Color.red, shotCooldown);
            }
        }
        
        int[] indexOfNearestCollider = new int[sources.Length];
        int numCollidersHit = 0;
        int numRaysHit = 0;

        for(int i = 0; i < hits.Length; i++){
            if(hits[i].collider == null) continue;

            //check if last hit had the same collider
            if(i > 0 && hits[i-1].collider == hits[i].collider){
                //check if this hit is nearer than previously hit
                if(hits[i-1].distance > hits[i].distance){
                    indexOfNearestCollider[numCollidersHit] = i;
                }
            }else{
                indexOfNearestCollider[numCollidersHit] = i;
                numCollidersHit++;
            }
            numRaysHit++;
        
        }

        for(int i = 0; i < numCollidersHit; i++){
            sources[indexOfNearestCollider[i]].clip = hitSound;
            sources[indexOfNearestCollider[i]].volume = hitVolume * (1 - hits[indexOfNearestCollider[i]].distance / distance);
            sources[indexOfNearestCollider[i]].pitch = hitPitch * (1 - hits[indexOfNearestCollider[i]].distance / distance);
            sources[indexOfNearestCollider[i]].PlayDelayed(hits[indexOfNearestCollider[i]].distance * soundDelayPerMeter);
        }

        if(numRaysHit < sources.Length){
            sources[numCollidersHit].clip = noHitSound;
            sources[numCollidersHit].volume = noHitVolume;
            sources[numCollidersHit].pitch = 1f;
            sources[numCollidersHit].PlayDelayed(distance * soundDelayPerMeter);

            SendMessage("SonarShoot", distance);
        }else{
            float maxDist = 0f;
            for(int i = 0;i < hits.Length; i++){
                if(hits[i].collider != null && hits[i].distance > maxDist)
                    maxDist = hits[i].distance;
            }
            SendMessage("SonarShoot", maxDist);
        }
        
    }
}
