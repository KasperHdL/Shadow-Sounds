﻿using UnityEngine;
using System.Collections;

public class SonarTool : MonoBehaviour {

    private PlayerMovement player;

    public float coneAngle = 30f;
    public float coneIncrement = 1f;

    private float coneAngleRad;
    private float coneIncrementRad;

    public float distance  = 20f;
    public float soundDelayPerMeter = 1f;


    private AudioSource source;
    public AudioClip hitSound;
    public AudioClip noHitSound;
    public float noHitVolume = 0.5f;
    public float hitPitch = 3f;
    public float hitVolume = 5f;

    private float lastShotTime = 0f;
    public float shotCooldown = 1f;


	// Use this for initialization
	void Start () {
        player = GetComponent<PlayerMovement>();
        source = GetComponent<AudioSource>();
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
	
        coneAngleRad = Mathf.Deg2Rad * coneAngle;
        coneIncrementRad = Mathf.Deg2Rad * coneIncrement;

        float nearestDistance = distance;

        RaycastHit2D hit;
        for(float a = angle - coneAngleRad; a < angle + coneAngleRad; a += coneIncrementRad){
            Vector3 d = new Vector3(Mathf.Cos(a), Mathf.Sin(-a),0); 
            hit = Physics2D.Raycast(player.transform.position + d, d, distance);
            if(hit.collider != null){
                Debug.DrawLine(player.transform.position, hit.point, Color.white, shotCooldown);

                if(hit.distance < nearestDistance)
                    nearestDistance = hit.distance;

            }else{
                Debug.DrawLine(player.transform.position, player.transform.position + d * distance, Color.red, shotCooldown);
            }
        }

        if (!source.isPlaying)
            SendMessage("SonarShoot", nearestDistance);

        if (nearestDistance == distance){
            source.clip = noHitSound;
            source.volume = noHitVolume;
            source.PlayDelayed(distance * soundDelayPerMeter);
        }else{
            source.clip = hitSound;
            source.volume = hitVolume*(1 - nearestDistance/distance);
            source.pitch = hitPitch*(1 - nearestDistance / distance);
            source.PlayDelayed(nearestDistance * soundDelayPerMeter);
        }
    }
}
