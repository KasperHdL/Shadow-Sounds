using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.PostProcessing;

public class PostProcessingAnimator : MonoBehaviour {

    public PostProcessingProfile profile;
    private ColorGradingModel colorGrading;

    private ColorGradingCurve masterCurve;
    private ColorGradingCurve redCurve;

    public float fadeInTime = 1f;
    public float fadeOutTime = 2;
    public float fadeFromExposure = -10f;
    private float exposure;

    private float temperature;
    public float fadedInTemperature = 0;
    public float fadedOutTemperature = -10;
 
    public float fadeRedTime = 1f;
    public Vector3 channelRed;
    public Vector3 channelGreen;
    public Vector3 channelBlue;

    public bool flickering = false;
    public bool flickeringIn = false;
    public bool flickeredIn = false;

    public bool playerAttacked = false;
    public bool fadeToBlack = false;

    public bool forceNormalMode = false;
    private List<FollowPlayer> enemies;
    private IEnumerator flickerEnumerator;

    public float playerAttackedRedValue = -1f;
    public float playerAttackedRedFadeIn = 0.03f;
    public float playerAttackedRedFadeOut = 0.2f;

    public float[] flickerInDelay = {
        0.03f, 0.07f, 0.04f, 0.02f
    };
    public float[] flickerOutDelay = {
        0.02f, 0.04f, 0.07f, 0.03f
    };
    public float[] flickerInValues = {
        -1f, -.1f, -1, -.2f, -1f
    };
    public float[] flickerOutValues = {
        .2f, -.1f, .1f, -1f, 0 
    };

	// Use this for initialization
	void Start () {
        enemies = new List<FollowPlayer>();
        StartCoroutine(fadeIn());
        temperature = fadedOutTemperature;

        colorGrading = profile.colorGrading;

        var linear = new AnimationCurve();
        linear.AddKey(0,0);
        linear.AddKey(1,1);
        masterCurve = new ColorGradingCurve(linear, 0, false, new Vector2(0,1));
        redCurve = new ColorGradingCurve(linear, 0, false, new Vector2(0,1));

        


	}
	// Update is called once per frame
	void FixedUpdate() {
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

        if(playerAttacked){
            playerAttacked = false;
            StartCoroutine(fadeBgRedInOut(playerAttackedRedValue, 0, playerAttackedRedFadeIn, playerAttackedRedFadeOut));
        }

        if(fadeToBlack){
            fadeToBlack = false;
            forceNormalMode = true;
            enemies.Clear();
            FlickerOutWorld();


            StartCoroutine(fadeOut(fadeOutTime));
        }

	}

    public void RegisterEnemyWithinPlayer(FollowPlayer enemy){
        if(forceNormalMode)return;
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
        Camera.main.GetComponent<PostProcessingAnimator>().playerAttacked = true;
    }

    public void FadeToBlack(){
        Camera.main.GetComponent<PostProcessingAnimator>().fadeToBlack = true;
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
            exposure = Mathf.Lerp(fadeFromExposure, 0f, t);
            yield return new WaitForFixedUpdate();
        }
        exposure = 0f;
        
    }

    IEnumerator fadeOut(float length){
        float startTime = Time.time;
        float endTime = Time.time + length;
        
        float t = 0;

        while(Time.time < endTime){
            t = (Time.time - startTime) / length;
            exposure = Mathf.Lerp(0f, fadeFromExposure, t);
            yield return new WaitForFixedUpdate();
        }
        exposure = fadeFromExposure;
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

            yield return new WaitForFixedUpdate();
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

            yield return new WaitForFixedUpdate();
        }

        redCurve.curve.keys[0].value = to;
    }

    IEnumerator FlickerIn(){
        flickering = true;
        flickeringIn = true;

        bool isIn = false;
        for(int i = 0;i < flickerInValues.Length; i++){
            float val = flickerInValues[i];
            channelGreen = new Vector3(val, 1, 0);
            channelBlue  = new Vector3(val, 0, 1);

            isIn = !isIn;
            temperature = (isIn ? fadedOutTemperature : fadedInTemperature);


            if(i == flickerInDelay.Length) break;

            yield return new WaitForSeconds(flickerInDelay[i]);

        }
        temperature = fadedInTemperature;

        flickering = false;
        flickeredIn = true;
    }


    IEnumerator FlickerOut(){
        flickering = true;
        flickeringIn = false;

        bool isIn = true;
        for(int i = 0;i < flickerOutValues.Length; i++){
            float val = flickerOutValues[i];
            channelGreen = new Vector3(val, 1, 0);
            channelBlue  = new Vector3(val, 0, 1);

            isIn = !isIn;
            temperature = (isIn ? fadedOutTemperature : fadedInTemperature);


            if(i == flickerOutDelay.Length) break;

            yield return new WaitForSeconds(flickerOutDelay[i]);

        }
        temperature = fadedOutTemperature;
        
        flickering = false;
        flickeredIn = false;
    }
	
    void OnApplicationQuit(){
        colorGrading.enabled = false;
        var settings = colorGrading.settings;
        settings.basic.postExposure = 0;
        settings.basic.temperature = fadedOutTemperature;
        settings.curves.master = masterCurve;
        settings.curves.red = masterCurve;

        settings.channelMixer.red = new Vector3(1,0,0);
        settings.channelMixer.green = new Vector3(0,1,0);
        settings.channelMixer.blue = new Vector3(0,0,1);

        colorGrading.settings = settings;
        profile.colorGrading = colorGrading;
        colorGrading.enabled = true;


    }
}
