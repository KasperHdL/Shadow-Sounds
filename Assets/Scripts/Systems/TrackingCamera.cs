using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityStandardAssets.ImageEffects;

[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(VignetteAndChromaticAberration))]
public class TrackingCamera : MonoBehaviour {

    public PlayerMovement target;

    public float smoothFactor = 0.8f;

    public float offsetZ  = -10;
    public float viewOffsetMultiplier = 2f;

    public float velocityFactor = 1;

    public float chaseSize = 5;
    public float size = 6;
    public float sizeLerp = 0.2f;

    public float normalVignette = 0.2f;
    public float normalChomatic = 0.5f;
    public float chaseVignette = 0.5f;
    public float chaseChomatic = 5;

    // How long the object should shake for.
    private static float shakeDuration;

    //used for shaking
    private static Vector3 originalPos;
    private bool shakePosSet;

    // Amplitude of the shake. A larger value shakes the camera harder.
    private static float shakeAmount;



    private Camera cam;
    private VignetteAndChromaticAberration effects;
    private PostProcessingAnimator ppAnimator;

    public float titleTime = 8.0f;
    private bool zoomingOut;

    void Start() {
        cam = GetComponent<Camera>();
        effects = GetComponent<VignetteAndChromaticAberration>();
        ppAnimator = GetComponent<PostProcessingAnimator>();


        if (GameObject.FindGameObjectWithTag("SaveSystem").GetComponent<SaveSystem>().PillarsDestroyed.Count > 0)
        {
            GameObject.FindWithTag("Title").gameObject.GetComponent<SpriteRenderer>().enabled = false;

            ppAnimator.FadeIn();

            GameObject.FindWithTag("Player").GetComponent<PlayerMovement>().enabled = true;
        }
        else
            StartCoroutine(TitleScreen());


        if (target == null) {
            //Debug.LogWarning("Camera has no target, gonna try to find an object tagged 'Player'");

            target = GameObject.FindWithTag("Player").GetComponent<PlayerMovement>();

            transform.position = new Vector3(target.transform.position.x, target.transform.position.y, transform.position.z);

            if(target == null)
                Debug.LogError("No object tagged 'Player'");
        }
    }

    IEnumerator TitleScreen()
    {
        GameObject.FindWithTag("Title").gameObject.GetComponent<SpriteRenderer>().enabled = true;

        ppAnimator.FadeIn();
        yield return new WaitForSeconds(titleTime);

        ppAnimator.fadeToBlack = true;
        yield return new WaitForSeconds(1.5f);
        
        GameObject.FindWithTag("Title").gameObject.GetComponent<SpriteRenderer>().enabled = false;
        
        ppAnimator.FadeIn();

        GameObject.FindWithTag("Player").GetComponent<PlayerMovement>().enabled = true;
    }

    void FixedUpdate() {
        if(target == null)
            return;

        Vector3 delta = target.transform.position - transform.position;
        delta.z = 0;
        Vector3 desiredPosition =
              (Vector2)transform.position
            + (Vector2)delta.normalized * delta.sqrMagnitude
            + target.viewDirection * viewOffsetMultiplier
            + target.GetComponent<Rigidbody2D>().velocity * Time.fixedDeltaTime * velocityFactor;

        desiredPosition.z = offsetZ;

        delta = desiredPosition - transform.position;

        transform.position += delta * smoothFactor * Time.fixedDeltaTime;

    }

    public static void ShakeIt(float duration, float amount = 0.05f)
    {
        shakeDuration = duration;
        shakeAmount = amount;
    }

    void Update()
    {
        if (zoomingOut)
            return;

        var chase = GameObject.FindGameObjectsWithTag("Enemy").Any(e => e.GetComponent<FollowPlayer>().visible);
        if(ppAnimator.forceNormalMode)
            chase = false;

        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, chase ? chaseSize : size, sizeLerp);
        effects.chromaticAberration = chase ? chaseChomatic : normalChomatic;
        effects.intensity = chase ? chaseVignette : normalVignette;


        if (shakeDuration > 0)
        {

            if (!shakePosSet)
            {
                shakePosSet = true;
                originalPos = cam.transform.localPosition;
            }

            float decreaseFactor = 1.0f;

            cam.transform.position = originalPos + Random.insideUnitSphere * shakeAmount;

            shakeDuration -= Time.deltaTime * decreaseFactor;
        }
        else
        {
            shakePosSet = false;
            shakeDuration = 0f;
        }

    }

    public IEnumerator EndAnimation()
    {

        GameObject.FindWithTag("Player").GetComponent<PlayerMovement>().enabled = false;


        SoundSystem.Play("Outro");
        SoundSystem.Stop("background");

        //slow fade to black
        ppAnimator.fadeOutTime = 150;
        ppAnimator.fadeToBlack = true;

        SoundSystem.Stop("footsteps");

        //A slow logarithmic zoom out could be nice.

        var zoomOutFactor = 0.004f;
        zoomingOut = true;

        while (Camera.main.orthographicSize <= 50)
        {
            Camera.main.orthographicSize +=zoomOutFactor;

            zoomOutFactor *= 1.001f;

            yield return new WaitForFixedUpdate();
        }

    }
}
