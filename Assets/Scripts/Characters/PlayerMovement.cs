using System;
using UnityEngine;
using System.Collections;
using System.Linq;
using KInput;
using UnityEngine.SceneManagement; 

public class PlayerMovement : CharacterMovement
{

    public int health = 4;
    
    public float stepVolume = 0.1f;
    public bool fallOnWalls;
    
    public float enemyColVol = 0.5f;
    public float defColVol = 0.5f;
    private bool isPlayingSteps = false;


    [HideInInspector]
    public Vector2 viewDirection;
    private bool fallen;

    [Header("Controller Settings")]
    public bool useController;
    [Range(0,1)] public float deadzone = 0.1f;
    private Controller controller;
    public PostProcessingAnimator ppAnimator;

    private SonarTool sonar;

    public override void Start()
    {
        base.Start();
        sonar = GetComponent<SonarTool>();
        controller = GetComponent<ControllerContainer>().controller;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {

        Debug.Log("player hit a " + collision.gameObject.tag + ", named " + collision.gameObject.name);

        switch (collision.gameObject.tag)
        {
            case "Wall":
                if (fallOnWalls)
                    SoundSystem.Play("wall collision",1, defColVol);

                //Debug.Log("WallCollision");
                break;
            case "PickUp":
                if (collision.gameObject.name == "SonarChargePU")
                    sonar.sonarChargeLeft += 200f;
                Destroy(collision.gameObject);
                
                break;
            default:
                if (fallOnWalls)
                    SoundSystem.Play("default collision", defColVol);

                //Debug.Log("No collision tag set!");
                break;
        }
    }

    public void Hit(int damage)
    {
        DisableMovement = true;
        StartCoroutine(Reactivate());
        ppAnimator.PlayerAttacked();
        SoundSystem.Play("enemy collision", 1,enemyColVol);
              
        health -= damage;
        if (health <= 0) Die();
    }

    private IEnumerator Reactivate()
    {
        yield return new WaitForSeconds(0.1f);
        DisableMovement = false;
    }

    private void Die()
    {
        Destroy(gameObject);

        Scene scene = SceneManager.GetActiveScene(); 
        SceneManager.LoadScene(scene.name);
    }

    public override void Update() {

        Vector3 v = Vector3.zero;
        if (useController)
        {
            v = new Vector3(controller.GetAxis(Axis.StickLeftX), controller.GetAxis(Axis.StickLeftY), 0);

            if(v.magnitude < deadzone){
                v = Vector3.zero;
            }else{
                v = v.normalized * ((v.magnitude - deadzone) / (1 - deadzone));
            }
        }
        if(v == Vector3.zero) 
            v = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0);

        Move = v;

        if (!isPlayingSteps)
        {
            if (body.velocity.magnitude >= 0.1)
            {
                SoundSystem.Play("footsteps", stepVolume, 1, 0, null, true);
                isPlayingSteps = true;
            }
        }
        else if (body.velocity.magnitude < 0.1)
        {
            SoundSystem.Stop("footsteps");
            isPlayingSteps = false;
        }

            if (useController)
        {
            Vector2 d = new Vector2(controller.GetAxis(Axis.StickRightX), controller.GetAxis(Axis.StickRightY));
            if(d.magnitude < deadzone){
                d = Vector2.zero;
            }else{
                d = d.normalized * ((d.magnitude - deadzone) / (1 - deadzone));
            }

            if(d != Vector2.zero){
                viewDirection = d;
            }
        }
        else
        {
            Vector2 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            viewDirection = mouse - (Vector2)transform.position;
            viewDirection = viewDirection.normalized;
        }

        transform.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * Mathf.Atan2(viewDirection.y, viewDirection.x));

        base.Update();
    }
    
}
