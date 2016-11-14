using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KInput;

public class SonarTool : MonoBehaviour, SonarSource {

    private PlayerMovement player;
    public Vector2 Direction { get { return player.viewDirection; } }

    public SonarBullet sonarBulletPrefab;

    public float coneAngle = 30f;
    public float Angle { get { return coneAngle; } }
    public float coneIncrement = 1f;

    public float distance  = 20f;
    public float Distance { get { return distance; } }
    public float soundDelayPerMeter = 1f;
    public float Speed { get { return soundDelayPerMeter; } }
    
    public int Rays { get { return (int)(coneAngle * 2 / coneIncrement); } }
    public RaycastHit2D[] soundHits;
    public RaycastHit2D[] blockHits;

    private float lastShotTime = 0f;
    public float shotCooldown = 1f;

    private Controller controller;

	// Use this for initialization
	void Start () {
        player = GetComponent<PlayerMovement>();
        controller = GetComponent<ControllerContainer>().controller;
	}


    void FixedUpdate(){
        if(lastShotTime + shotCooldown < Time.time){
            if(Input.GetButtonDown("Fire1") || controller.GetAxis(Axis.TriggerRight) > 0.75f){
                Shoot();
                lastShotTime = Time.time;
            }
        }
    }
	
	void Shoot()
	{
        SoundSystem.Play("sonar noise", 0.2f, 1, 0, distance * soundDelayPerMeter);

        var bullet = (SonarBullet)Instantiate(sonarBulletPrefab, transform.position, Quaternion.identity);
	    bullet.source = this;
    }
}
