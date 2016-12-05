using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

[RequireComponent(typeof(PillarAnimator))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(CircleCollider2D))]
public class Pillar : Interactable {

    public int PillarId;

    public float ShakeTime = 1f;
    public float ShakeFactor = 1f;
    public AnimationCurve ShakeCurve;

    public float ExplosionTime = 1f;
    public float ExplosionRate = 1f;
    public AnimationCurve ExplosionCurve;
    public float EndTransparency = 0.4f;

    public float FadeTime = 1f;

    public float KeepRate = 0.5f;
    public float RemoveRate = 0.5f;
    public float KeepTransparency = 0.4f;
    public float KeepSpread = 0.5f;

    public List<GameObject> OnDestroyTurnOn;
    public List<GameObject> OnDestroyTurnOff;
    public List<GameObject> OnDestroyTurnTrigger;

    private bool IsDead;
    public bool killVein;
    public float enemyStunLength = 3f;

    public bool isFinalPillar;

    public void Start() {
        var savesystem = GameObject.FindGameObjectWithTag("SaveSystem");
        if(savesystem != null) {
            SaveSystem save = savesystem.GetComponent<SaveSystem>();
            if(save.PillarsDestroyed.Contains(PillarId)) {

                ShakeTime = 2.0f;
                //ExplosionTime = 0;
                StartCoroutine(PillarId == save.PillarsDestroyed.Last() ? Explode(0.1f) : Explode(0.0f));

                IsDead = true;

            }
        }
    }

    public override void Interact() {
        if(!IsDead) {
            IsDead = true;
            StartCoroutine(Explode(1.0f));
            var enemies = FindObjectsOfType<FollowPlayer>();


            // Register in save system
            var savesystem = GameObject.FindGameObjectWithTag("SaveSystem");
            if (savesystem != null)
            {
                SaveSystem save = savesystem.GetComponent<SaveSystem>();
                save.PillarsDestroyed.Add(PillarId);
            }


            for (int i = 0; i < enemies.Length; i++)
                enemies[i].Stun(enemyStunLength);

        }

        
    }


    public IEnumerator Explode(float volume) {
        //yield return new WaitForSeconds(2);
        var animator = GetComponent<PillarAnimator>();
        SoundSystem.Play("pre-explode",1,volume,0,ShakeTime);


        // Save state
        var li = GetComponentInChildren<Light>().intensity;
        var p0 = animator.PositionCurveSpeed;
        var r0 = animator.RotationCurveSpeed;
        var r10 = animator.RotationCurveFactor;
        var r20 = animator.Rotation2CurveSpeed;
        var s0 = animator.ScaleCurveSpeed;
        var pp0 = animator.PositionPulseCurveSpeed;
        var vein = gameObject.GetComponent<PillarVein>();

        float vSpd = 0;
        float vSz = 0;
        float vWdt = 0;

        if(vein != null){
            vSpd = vein.PulseSpeed;
            vSz = vein.PulseSize;
            vWdt = vein.PulseWidth;
        }
        // Warm up
        for(float t = ShakeTime; t > 0; t -= Time.fixedDeltaTime)
        {
            if(vein != null){
                vein.PulseSpeed = 0.5f * vSpd * ShakeCurve.Evaluate((t / ShakeTime)) * ShakeFactor;
                vein.PulseSize =  0.5f * vSz * ShakeCurve.Evaluate((t / ShakeTime)) * ShakeFactor;
                vein.PulseWidth = 1.5f * vWdt * ShakeCurve.Evaluate((t / ShakeTime)) * ShakeFactor;
            }

            animator.PositionCurveSpeed = p0 * ShakeCurve.Evaluate(1 - (t / ShakeTime)) * ShakeFactor;
            animator.RotationCurveSpeed = r0 * ShakeCurve.Evaluate(1 - (t / ShakeTime)) * ShakeFactor;
            animator.RotationCurveFactor = r10 * ShakeCurve.Evaluate(1 - (t / ShakeTime)) * ShakeFactor;
            animator.Rotation2CurveSpeed = r20 * ShakeCurve.Evaluate(1 - (t / ShakeTime)) * ShakeFactor;
            animator.ScaleCurveSpeed = s0 * ShakeCurve.Evaluate(1 - (t / ShakeTime)) * ShakeFactor;
            animator.PositionPulseCurveSpeed = pp0 * ShakeCurve.Evaluate(1 - (t / ShakeTime)) * ShakeFactor;
            animator.HeartBearRate = animator.PositionPulseCurveSpeed;
            GetComponentInChildren<Light>().intensity = li * ShakeCurve.Evaluate(1 - (t / ShakeTime)) * ShakeFactor;

            
            if (t/ShakeTime < 0.30f && !killVein && vein != null)
            {
                killVein = true;
                SoundSystem.Play("VeinSnap",1.0f,volume);
            }

            yield return new WaitForFixedUpdate();
        }

        // Floor blood
        var keep = new List<Transform>();
        for(int i = 0; i < transform.childCount; i++)
            keep.Add(transform.GetChild(i));
        keep = keep.OrderBy(_ => Guid.NewGuid()).Take((int)(keep.Count * KeepRate)).ToList();
        var floor = new GameObject("Floor Blood").transform;
        floor.position = transform.position;
        foreach(var trans in keep) {
            if(trans.GetComponent<SpriteRenderer>() == null)
                continue;
            var t2 = (GameObject)Instantiate(trans.gameObject, floor, true);
            var c = Color.Lerp(animator.ColorA, animator.ColorB, 0.6f);
            c.a = KeepTransparency;
            t2.GetComponent<SpriteRenderer>().color = c;
            t2.transform.localPosition *= 1 + (KeepSpread * Random.value);
        }


        


        SoundSystem.Play("Pillar Death",1.0f,volume);


        if (isFinalPillar)
        {
            GameObject.FindWithTag("MainCamera").GetComponent<TrackingCamera>().EndAnimation();
        }

        // Explosion
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<CircleCollider2D>().enabled = false;

        animator.PositionCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));
        animator.RotationCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));
        animator.Rotation2Curve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));
        animator.ScaleCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));
        animator.PositionPulseCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));
        animator.ColorPulseCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));
        animator.ColorMixCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));
        animator.PositionCurveSpeed = 0;
        animator.RotationCurveSpeed = 0;
        animator.RotationCurveFactor = 0;
        animator.Rotation2CurveSpeed = 0;
        animator.ScaleCurveSpeed = 0;
        animator.PositionPulseCurveSpeed = 0;
        GetComponentInChildren<Light>().intensity = li;

        for (float t = ExplosionTime; t > 0; t -= Time.fixedDeltaTime) {
            animator.PositionCurveFactor += ExplosionRate * Time.fixedDeltaTime * ExplosionCurve.Evaluate(t / ExplosionTime);
            animator.ColorA.a = ExplosionCurve.Evaluate((t / ExplosionTime) * (1 - EndTransparency) + EndTransparency);
            animator.ColorB.a = ExplosionCurve.Evaluate((t / ExplosionTime) * (1 - EndTransparency) + EndTransparency);
            

            yield return new WaitForFixedUpdate();
        }

        foreach(var audioSource in gameObject.GetComponents<AudioSource>()) {
            audioSource.Stop();
        }

        animator.enabled = false;


        // Remove some clots
        var remove = new List<Transform>();
        for(int i = 0; i < transform.childCount; i++)
            remove.Add(transform.GetChild(i));
        remove = remove.OrderBy(_ => Guid.NewGuid()).Take((int)(remove.Count * RemoveRate)).ToList();

        for(float t = FadeTime; t > 0; t -= Time.fixedDeltaTime) {
            foreach(var trans in remove) {
                if(trans.GetComponent<SpriteRenderer>() == null)
                    continue;
                var c = trans.GetComponent<SpriteRenderer>().color;
                c.a = EndTransparency * (t / FadeTime);
                trans.GetComponent<SpriteRenderer>().color = c;
            }
            yield return new WaitForFixedUpdate();
        }

        foreach (var go in OnDestroyTurnOn) {
            if(go == null)
                continue;
            foreach(var act in go.GetComponents<IActivatable>()) {
                act.Activate();
            }
        }

        foreach(var go in OnDestroyTurnOff) {
            if(go == null)
                continue;
            foreach(var act in go.GetComponents<IActivatable>()) {
                act.ShutDown();
            }
        }

        foreach(var go in OnDestroyTurnTrigger) {
            if(go == null)
                continue;
            foreach(var act in go.GetComponents<IActivatable>()) {
                act.Trigger();
            }
        }

    }
}
