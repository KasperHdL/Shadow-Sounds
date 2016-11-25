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
    public bool Inside = true;

    public float enemyColVol = 0.5f;
    public float defColVol = 0.5f;
    private bool isPlayingSteps = false;

    public float deathFade = 3f;

    [HideInInspector] public Vector2 viewDirection;
    private bool fallen;

    [Header("Controller Settings")] public bool useController;
    [Range(0, 1)] public float deadzone = 0.1f;
    private Controller controller;
    public PostProcessingAnimator ppAnimator;

    [Header("Ambient Light Chase Settings")] public Light AmbientLight;
    public float ChaseLightIntMultiplier = 5;
    public float ChaseLightRanMultiplier = 5;
    private float OriginalALightIntensity;
    private float OriginalALightRange;

    private SonarTool sonar;

    public GameObject flashlight_prefab;
    public GameObject flashlight;

    [HideInInspector] public bool isDead = false;

    public void Awake()
    {
        var savesystem = GameObject.FindGameObjectWithTag("SaveSystem");
        if (savesystem != null && savesystem.GetComponent<SaveSystem>().PlayerPosition.HasValue)
            transform.position = savesystem.GetComponent<SaveSystem>().PlayerPosition.Value;
    //    doNotNormalize = true;
    }

    public override void Start()
    {
        base.Start();
        sonar = GetComponent<SonarTool>();
        controller = GetComponent<ControllerContainer>().controller;
        if (ppAnimator == null)
            ppAnimator = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<PostProcessingAnimator>();

        if (AmbientLight)
        {
            OriginalALightIntensity = AmbientLight.intensity;
            OriginalALightRange = AmbientLight.range;
        }
    }

    void OnTriggerStay2D(Collider2D coll){
        if(coll.gameObject.tag == "Interactable"){
            if(
                Input.GetButtonDown("Use") || 
                controller.GetButtonDown(KInput.Button.BumperLeft) ||
                controller.GetButtonDown(KInput.Button.X) ||
                controller.GetButtonDown(KInput.Button.A) ||
                controller.GetButtonDown(KInput.Button.StickRightClick) ||
                controller.GetButtonDown(KInput.Button.StickLeftClick) ||
                controller.GetAxis(Axis.TriggerLeft) > 0.75f
                )
            coll.gameObject.GetComponent<Interactable>().Interact();;

        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {

        //Debug.Log("player hit a " + collision.gameObject.tag + ", named " + collision.gameObject.name);

        switch (collision.gameObject.tag)
        {
            case "Wall":
                if (fallOnWalls)
                    SoundSystem.Play("wall collision", 1, defColVol);

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
        if (isDead) return;
        DisableMovement = true;
        StartCoroutine(Reactivate());
        ppAnimator.PlayerAttacked();
        SoundSystem.Play("enemy collision", 1, enemyColVol);
        TrackingCamera.ShakeIt(0.5f, 0.5f);
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
        if (isDead) return;

// Drop Flashlight(does not look good currently)
//        GameObject fl = Instantiate(flashlight_prefab, flashlight.transform.position, flashlight.transform.rotation) as GameObject;
//       flashlight.GetComponent<Light>().enabled = false;

        body.drag = 5;
        body.angularDrag = 1f;

        isDead = true;
        SoundSystem.Play("death", 1, 1, 0, deathFade - 0.75f);

        ppAnimator.FadeToBlack();
        StartCoroutine(ReloadLevel(deathFade));
    }

    private IEnumerator ReloadLevel(float delay)
    {
        yield return new WaitForSeconds(delay);

        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }

    public override void Update()
    {

        if (isDead) return;

        Vector3 v = Vector3.zero;
        if (useController)
        {
            v = new Vector3(controller.GetAxis(Axis.StickLeftX), controller.GetAxis(Axis.StickLeftY), 0);

            if (v.magnitude < deadzone)
            {
                v = Vector3.zero;
            }
            else
            {
                v = v.normalized*((v.magnitude - deadzone)/(1 - deadzone));
            }
        }

        if (v == Vector3.zero)
            v = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0).normalized;

        Move = v;

        if (!isPlayingSteps)
        {
            if (body.velocity.magnitude >= 0.1)
            {
                if (Inside)
                {
                    SoundSystem.Play("footsteps", 1, stepVolume, 0, null, true);
                }
                else
                {
                    SoundSystem.Play("snowsteps", 1, stepVolume * 0.8f, 0, null, true);
                }

                isPlayingSteps = true;
            }
        }
        else if (body.velocity.magnitude < 0.1)
        {
            SoundSystem.Stop("footsteps");
            SoundSystem.Stop("snowsteps");
            isPlayingSteps = false;
        }

        if (useController)
        {
            Vector2 d = new Vector2(controller.GetAxis(Axis.StickRightX), controller.GetAxis(Axis.StickRightY));
            if (d.magnitude < deadzone)
            {
                d = Vector2.zero;
            }
            else
            {
                d = d.normalized*((d.magnitude - deadzone)/(1 - deadzone));
            }

            if (d != Vector2.zero)
            {
                viewDirection = d;
            }
        }
        else
        {
            Vector2 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            viewDirection = mouse - (Vector2) transform.position;
            viewDirection = viewDirection.normalized;
        }

        transform.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg*Mathf.Atan2(viewDirection.y, viewDirection.x));


        if (AmbientLight != null)
        {
            var chase = GameObject.FindGameObjectsWithTag("Enemy").Any(e => e.GetComponent<FollowPlayer>().visible);
            AmbientLight.intensity = OriginalALightIntensity*(chase ? ChaseLightIntMultiplier : 1.0f);
            AmbientLight.range = OriginalALightRange*(chase ? ChaseLightRanMultiplier : 1.0f);
        }

        base.Update();
    }

    public void setInside(bool isInside)
    {
        if ((isInside & !Inside) || (!isInside && Inside))
        {
            SoundSystem.Stop("footsteps");
            SoundSystem.Stop("snowsteps");
            isPlayingSteps = false;
        }

        Inside = isInside;
    }
}
