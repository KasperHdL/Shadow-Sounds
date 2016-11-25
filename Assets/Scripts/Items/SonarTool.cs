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
    public float sonarSpeed = 1f;
    public float Speed { get { return sonarSpeed; } }
    
    public int Rays { get { return (int)(coneAngle * 2 / coneIncrement); } }
    public float SonarPct { get { return sonarChargeLeft / initialSonarCharge; } }
    public RaycastHit2D[] soundHits;
    public RaycastHit2D[] blockHits;

    private float lastShotTime = 0f;
    public float shotCooldown = 1f;

    public float sonarChargeLeft = 6000f;
    private float initialSonarCharge;
    private float lastUpdateTime;

    private Controller controller;
    

    // Use this for initialization
	void Start () {
        player = GetComponent<PlayerMovement>();
        controller = GetComponent<ControllerContainer>().controller;
        lastUpdateTime = Time.time;
        initialSonarCharge = sonarChargeLeft;
    }


    void FixedUpdate(){
        if(player.isDead) return;


        sonarChargeLeft -= (Time.time - lastUpdateTime);
        lastUpdateTime = Time.time;

        if (lastShotTime + shotCooldown < Time.time){
            if(Input.GetButton("Fire1") || controller.GetAxis(Axis.TriggerRight) > 0.75f || controller.GetButton(KInput.Button.BumperRight))
            {
                Shoot();
                lastShotTime = Time.time;
            }
        }
    }
	
	void Shoot()
	{
        SoundSystem.Play("sonar noise", 1.0f, 0.5f, 0, distance/sonarSpeed);
        
        var bullet = (SonarBullet)Instantiate(sonarBulletPrefab, transform.position, Quaternion.identity);
	    bullet.source = this;
    }
}
