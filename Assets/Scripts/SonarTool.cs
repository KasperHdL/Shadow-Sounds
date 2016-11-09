using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public RaycastHit2D[] soundHits;
    public RaycastHit2D[] blockHits;

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
        soundHits = new RaycastHit2D[rays];
        blockHits = new RaycastHit2D[rays];
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

	    var colliderHits = new Dictionary<Collider2D, RaycastHit2D>();
        var numRaysHit = 0;

        float startAngle = angle - coneAngleRad;
        for(int i = 0; i < rays; i++){

            float a = startAngle + coneIncrementRad * i;

            Vector3 d = new Vector3(Mathf.Cos(a), Mathf.Sin(-a),0); 

            var hits = Physics2D.RaycastAll(player.transform.position + d, d, distance, SoundMask | BlockMask);
            blockHits[i] = Physics2D.Raycast(player.transform.position + d, d, distance, BlockMask);

            foreach (var hit in hits)
            {
                var c = hit.collider;
                if (c != null && c.tag == "Enemy")
                {
                    numRaysHit++;
                    if (!colliderHits.ContainsKey(c) || hit.distance < colliderHits[c].distance)
                        colliderHits[hit.collider] = hit;
                }
                else
                {
                    break;
                }
            }

            //draw debug
            /*if (soundHits[i].collider != null){
                Debug.DrawLine(player.transform.position, soundHits[i].point, Color.white, shotCooldown);
            } else */if (blockHits[i].collider != null) {
                Debug.DrawLine(player.transform.position, blockHits[i].point, Color.blue, shotCooldown);
            } else { 
                Debug.DrawLine(player.transform.position, player.transform.position + d * distance, Color.red, shotCooldown);
            }
        }

	    SoundSystem.Play("sonar noise", 0.2f, 1, 0, distance*soundDelayPerMeter);

        soundHits = colliderHits.Values.ToArray();
	    foreach (var hit in colliderHits.Values)
        {
            SoundSystem.Play("sonar hit",
                hitPitch * (1 - hit.distance / distance),
                hitVolume * (1 - hit.distance / distance),
                hit.distance * soundDelayPerMeter);
        }

        if(numRaysHit < rays){
            SoundSystem.Play("sonar no hit", noHitVolume, 1f, distance * soundDelayPerMeter);

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
