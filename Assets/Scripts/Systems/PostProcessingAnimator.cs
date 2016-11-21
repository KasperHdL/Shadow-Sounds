﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.PostProcessing;

public class PostProcessingAnimator : MonoBehaviour {

    public PostProcessingProfile profile;
    public ColorGradingModel colorGrading;

    public ColorGradingCurve masterCurve;
    public ColorGradingCurve redCurve;

    public float fadeInTime = 1f;
    private float exposure;

    public float temperature;

    public float fadeRedTime = 1f;
    public Vector3 channelRed;
    public Vector3 channelGreen;
    public Vector3 channelBlue;

    public bool flickering = false;
    public bool flickeringIn = false;
    public bool flickeredIn = false;


    public List<FollowPlayer> enemies;
    private IEnumerator flickerEnumerator;

    [Header("Debug")]
    public bool flickerIn = false;
    public bool flickerOut = false;

	// Use this for initialization
	void Start () {
        enemies = new List<FollowPlayer>();
        StartCoroutine(fadeIn());
        //PlayerAttacked();
	
	}
	
	// Update is called once per frame
	void Update () {

        if(flickerIn){
            FlickerInWorld();
            flickerIn = false;
        }

        if(flickerOut){
            FlickerOutWorld();
            flickerOut = false;
        }
 
        colorGrading.enabled = false;

        var settings = colorGrading.settings;
        settings.basic.postExposure = exposure;
        settings.basic.temperature = temperature;
        settings.curves.master = masterCurve;
        settings.curves.red = redCurve;

        settings.channelMixer.red = channelRed;
        settings.channelMixer.green = channelGreen;
        settings.channelMixer.blue = channelBlue;

        colorGrading.settings = settings;
        profile.colorGrading = colorGrading;
        colorGrading.enabled = true;
        
	}

    public void RegisterEnemyWithinPlayer(FollowPlayer enemy){
        if(enemies.IndexOf(enemy) == -1){
            enemies.Add(enemy);
            FlickerInWorld();
        }
    }
    public void RegitsterEnemyOutsidePlayer(FollowPlayer enemy){
        enemies.Remove(enemy);

        if(enemies.Count == 0)
            FlickerOutWorld();

    }


    public void PlayerAttacked(){

        StartCoroutine(fadeBgRedInOut(1,0f, .03f, .2f));
    }

    public void FlickerInWorld(){
        if(!flickeredIn){
            if(flickering && flickeringIn)
                return;

            if(flickerEnumerator != null)
                StopCoroutine(flickerEnumerator);

            flickerEnumerator = FlickerIn();
            StartCoroutine(flickerEnumerator);
        }
    }

    public void FlickerOutWorld(){
        if(flickeredIn){
            if(flickering && !flickeringIn)
                return;

            if(flickerEnumerator != null)
                StopCoroutine(flickerEnumerator);

            flickerEnumerator = FlickerOut();
            StartCoroutine(flickerEnumerator);
        }
    }
    


    IEnumerator fadeIn(){
        float startTime = Time.time;
        float endTime = Time.time + fadeInTime;
        
        float t = 0;

        while(Time.time < endTime){
            t = (Time.time - startTime) / fadeInTime;
            exposure = Mathf.Lerp(-10f, 0f, t);
            yield return null;
        }
        exposure = 0f;
        

    }

    IEnumerator fadeBgRedInOut(float to, float end, float lengthIn, float lengthOut){
        StartCoroutine(fadeBgRed(to, lengthIn));
        yield return new WaitForSeconds(lengthIn);
        StartCoroutine(fadeBgRed(end, lengthOut));
    }



    IEnumerator fadeFgRedInOut(float to, float end, float lengthIn, float lengthOut){
        StartCoroutine(fadeFgRed(channelGreen.x, to, lengthIn));
        yield return new WaitForSeconds(lengthIn);
        StartCoroutine(fadeFgRed(to, end, lengthOut));
    }

    IEnumerator fadeFgRed(float from, float to, float length){
        float startTime = Time.time;
        float endTime = Time.time + fadeRedTime;
        
        float t = 0;

        while(Time.time < endTime){
            t = (Time.time - startTime) / fadeRedTime;
            float v = Mathf.Lerp(from, to, t);

            channelRed = new Vector3(1, 0, 0);
            channelGreen = new Vector3(v, 1, 0);
            channelBlue = new Vector3(v, 0, 1);

            yield return null;
        }
        
        channelRed = new Vector3(1, 0, 0);
        channelGreen = new Vector3(to, 1, 0);
        channelBlue = new Vector3(to, 0, 1);


    }


    IEnumerator fadeBgRed(float to, float length){

        float startTime = Time.time;
        float endTime = Time.time + fadeRedTime;
        
        float t = 0;

        float from = colorGrading.settings.curves.red.curve.keys[0].value;

        while(Time.time < endTime){
            t = (Time.time - startTime) / fadeRedTime;
            float v = Mathf.Lerp(from, to, t);
            redCurve.curve.RemoveKey(0);
            redCurve.curve.AddKey(0f,v);

            yield return null;
        }

        redCurve.curve.keys[0].value = to;
    }

    IEnumerator FlickerIn(){
        flickering = true;
        flickeringIn = true;

        float val = -1f;
        channelGreen = new Vector3(val, 1, 0);
        channelBlue = new Vector3(val, 0, 1);

        yield return new WaitForSeconds(0.03f);

        val = -0.1f;
        channelGreen = new Vector3(val, 1, 0);
        channelBlue = new Vector3(val, 0, 1);

        yield return new WaitForSeconds(0.07f);

        val = -1f;
        channelGreen = new Vector3(val, 1, 0);
        channelBlue = new Vector3(val, 0, 1);

        yield return new WaitForSeconds(0.04f);

        val = -0.2f;
        channelGreen = new Vector3(val, 1, 0);
        channelBlue = new Vector3(val, 0, 1);

        yield return new WaitForSeconds(0.02f);

        val = -1f;
        channelGreen = new Vector3(val, 1, 0);
        channelBlue = new Vector3(val, 0, 1);

        flickering = false;
        flickeredIn = true;
    }


    IEnumerator FlickerOut(){
        flickering = true;
        flickeringIn = false;
        float val = 0.2f;
        channelGreen = new Vector3(val, 1, 0);
        channelBlue = new Vector3(val, 0, 1);

        yield return new WaitForSeconds(0.02f);

        val = -1f;
        channelGreen = new Vector3(val, 1, 0);
        channelBlue = new Vector3(val, 0, 1);

        yield return new WaitForSeconds(0.04f);

        val = 0.1f;
        channelGreen = new Vector3(val, 1, 0);
        channelBlue = new Vector3(val, 0, 1);

        yield return new WaitForSeconds(0.07f);

        val = -1f;
        channelGreen = new Vector3(val, 1, 0);
        channelBlue = new Vector3(val, 0, 1);

        yield return new WaitForSeconds(0.03f);

        val = 0; 
        channelGreen = new Vector3(val, 1, 0);
        channelBlue = new Vector3(val, 0, 1);
        flickering = false;
        flickeredIn = false;
    }


}
