using UnityEngine;
using System.Collections;
using KInput;

public class SonarTool : MonoBehaviour {

    private PlayerMovement player;

    public LayerMask SoundMask;
    public LayerMask BlockMask;

    public float coneAngle = 30f;
    public float coneIncrement = 1f;

    private float coneAngleRad;
    private float coneIncrementRad;

    public float distance  = 20f;
    public float soundDelayPerMeter = 1f;

    [HideInInspector]
    public int rays;
    public GameObject sourceContainer;
    public AudioSource[] sources;
    public RaycastHit2D[] soundHits;
    public RaycastHit2D[] blockHits;

    public AudioClip hitSound;
    public AudioClip noHitSound;
    public AudioClip sonarNoise;
    private AudioSource _noise;

    public float noHitVolume = 0.5f;
    public float hitPitch    = 3f;
    public float hitVolume   = 5f;

    private float lastShotTime = 0f;
    public float shotCooldown = 1f;

    private Controller controller;

	// Use this for initialization
	void Start () {
        player = GetComponent<PlayerMovement>();
        controller = GetComponent<ControllerContainer>().controller;
        coneAngleRad = Mathf.Deg2Rad * coneAngle;
        coneIncrementRad = Mathf.Deg2Rad * coneIncrement;

	    rays = (int) (coneAngle*2/coneIncrement);
        sources = new AudioSource[rays];
        soundHits = new RaycastHit2D[rays];
        blockHits = new RaycastHit2D[rays];

        for (int i = 0;i<sources.Length;i++)
            sources[i] = sourceContainer.AddComponent<AudioSource>();

	}


    void FixedUpdate(){
        if(lastShotTime + shotCooldown < Time.time){
            if(Input.GetButtonDown("Fire1") || controller.GetAxis(Axis.TriggerRight) > 0.75f){
                Shoot();
                lastShotTime = Time.time;
            }
        }
    }
	
	void Shoot() {
        Vector3 viewDir = player.viewDirection.normalized;
        float angle = Mathf.Atan2(-viewDir.y, viewDir.x);

	    _noise = sourceContainer.AddComponent<AudioSource>();
	    _noise.volume = 0.15f;
        _noise.clip = sonarNoise;
        _noise.Play();

	    float startAngle = angle - coneAngleRad;
        for(int i = 0; i < rays; i++){

            float a = startAngle + coneIncrementRad * i;

            Vector3 d = new Vector3(Mathf.Cos(a), Mathf.Sin(-a),0); 

            soundHits[i] = Physics2D.Raycast(player.transform.position + d, d, distance, SoundMask | BlockMask);
            blockHits[i] = Physics2D.Raycast(player.transform.position + d, d, distance, BlockMask);

            //draw debug
            if (soundHits[i].collider != null){
                Debug.DrawLine(player.transform.position, soundHits[i].point, Color.white, shotCooldown);
            } else if (blockHits[i].collider != null) {
                Debug.DrawLine(player.transform.position, blockHits[i].point, Color.blue, shotCooldown);
            } else { 
                Debug.DrawLine(player.transform.position, player.transform.position + d * distance, Color.red, shotCooldown);
            }
        }

        
        
        int[] indexOfNearestCollider = new int[rays];
        int numCollidersHit = 0;
        int numRaysHit = 0;

        for(int i = 0; i < soundHits.Length; i++){
            if(soundHits[i].collider == null || soundHits[i].collider.gameObject.tag != "Enemy") continue;

            //check if last hit had the same collider
            if(i > 0 && soundHits[i-1].collider == soundHits[i].collider){
                //check if this hit is nearer than previously hit
                if(soundHits[i-1].distance > soundHits[i].distance){
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
            sources[indexOfNearestCollider[i]].volume = hitVolume * (1 - soundHits[indexOfNearestCollider[i]].distance / distance);
            sources[indexOfNearestCollider[i]].pitch = hitPitch * (1 - soundHits[indexOfNearestCollider[i]].distance / distance);
            sources[indexOfNearestCollider[i]].PlayDelayed(soundHits[indexOfNearestCollider[i]].distance * soundDelayPerMeter);
        }

        if(numRaysHit < sources.Length){
            sources[numCollidersHit].clip = noHitSound;
            sources[numCollidersHit].volume = noHitVolume;
            sources[numCollidersHit].pitch = 1f;
            sources[numCollidersHit].PlayDelayed(distance * soundDelayPerMeter);
            

            SendMessage("SonarShoot", distance);
        }else{
            float maxDist = 0f;
            for(int i = 0;i < soundHits.Length; i++){
                if(soundHits[i].collider != null && soundHits[i].distance > maxDist)
                    maxDist = soundHits[i].distance;
            }
            SendMessage("SonarShoot", maxDist);
        }
        
    }
}
