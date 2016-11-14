using UnityEngine;
using System.Collections;
using UnityEngine.PostProcessing;

public class PostProcessingAnimator : MonoBehaviour {

    public PostProcessingProfile profile;
    public ColorGradingModel colorGrading;

    public ColorGradingCurve masterCurve;
    public ColorGradingCurve redCurve;

    public float fadeInTime = 1f;
    private float exposure;

    public float fadeRedTime = 1f;
    public Vector3 channelRed;
    public Vector3 channelGreen;
    public Vector3 channelBlue;


	// Use this for initialization
	void Start () {
        StartCoroutine(fadeIn());
        //PlayerAttacked();

	
	}
	
	// Update is called once per frame
	void Update () {
        colorGrading.enabled = false;

        var settings = colorGrading.settings;
        settings.basic.postExposure = exposure;
        settings.curves.master = masterCurve;
        settings.curves.red = redCurve;

        settings.channelMixer.red = channelRed;
        settings.channelMixer.green = channelGreen;
        settings.channelMixer.blue = channelBlue;

        colorGrading.settings = settings;
        profile.colorGrading = colorGrading;
        colorGrading.enabled = true;
        
	}


    public void PlayerAttacked(){
        StartCoroutine(fadeBgRedInOut(0.75f,0f, .05f, .1f));
        //StartCoroutine(fadeFgRedInOut(-.75f,0f, .02f, .1f));
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


}
