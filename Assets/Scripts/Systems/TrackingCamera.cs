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

    public float preTitleTime = 5.0f;
    public float titleTime = 8.0f;
    public Transform introCar;
    public AnimationCurve exitCarCurve;
    public AnimationCurve shakeCarCurve;
    public Vector3 endPosition;
    private SpriteRenderer title;
    private bool zoomingOut;

    void Start() {
        cam = GetComponent<Camera>();
        effects = GetComponent<VignetteAndChromaticAberration>();
        ppAnimator = GetComponent<PostProcessingAnimator>();
        title = GameObject.FindWithTag("Title").gameObject.GetComponent<SpriteRenderer>();

        if(target == null) {
            //Debug.LogWarning("Camera has no target, gonna try to find an object tagged 'Player'");

            target = GameObject.FindWithTag("Player").GetComponent<PlayerMovement>();

            transform.position = new Vector3(target.transform.position.x, target.transform.position.y, transform.position.z);

            if(target == null)
                Debug.LogError("No object tagged 'Player'");
        }

        if (GameObject.FindGameObjectWithTag("SaveSystem").GetComponent<SaveSystem>().PillarsDestroyed.Count > 0)
        {
            title.enabled = false;

            ppAnimator.FadeIn();

            target.enabled = true;
        }
        else
            StartCoroutine(TitleScreen());
    }

    IEnumerator TitleScreen() {
        ppAnimator.FadeIn();

        foreach (var l in target.GetComponentsInChildren<Light>())
            l.enabled = false;
        target.enabled = false;

        var shake = ShakeCar();
        StartCoroutine(shake);
        yield return new WaitForSeconds(preTitleTime);

        title.transform.position = transform.position;
        title.enabled = true;

        yield return new WaitForSeconds(titleTime);

        ppAnimator.fadeToBlack = true;
        yield return new WaitForSeconds(1.5f);

        title.enabled = false;
        StopCoroutine(shake);

        ppAnimator.FadeIn();
        introCar.position = endPosition + Vector3.down;

        var t = 2.0f;
        while (t > 0)
        {
            target.transform.position = Vector3.Lerp(endPosition, endPosition + Vector3.left * 2,
                exitCarCurve.Evaluate(1 - t/2));
            yield return null;
            t -= Time.deltaTime;
        }
        
        target.enabled = true;
        foreach(var l in target.GetComponentsInChildren<Light>())
            l.enabled = true;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(endPosition, 0.1f);
    }

    IEnumerator ShakeCar()
    {
        var origin = introCar.position;
        while (true)
        {
            yield return null;

            var shake = Vector3.right * Random.Range(-1f,1f) * 0.1f * shakeCarCurve.Evaluate(Time.time);

            introCar.position = origin + shake;
            target.transform.position = introCar.position;
            transform.position = introCar.position - shake;
        }
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
        GameObject.FindWithTag("Player").GetComponent<PlayerMovement>().health = 50000;


        SoundSystem.Play("Outro");
        SoundSystem.Stop("background");

        //slow fade to black
        ppAnimator.fadeOutTime = 150;
        ppAnimator.fadeToBlack = true;

        SoundSystem.Stop("footsteps");

        //A slow logarithmic zoom out could be nice.

        var zoomOutFactor = 0.004f;
        zoomingOut = true;

        while (Camera.main.orthographicSize <= 100)
        {
            Camera.main.orthographicSize +=zoomOutFactor;
            Camera.main.transform.position -= new Vector3(0, zoomOutFactor, 0);

            zoomOutFactor *= 1.001f;

            yield return new WaitForFixedUpdate();
        }

    }
}
