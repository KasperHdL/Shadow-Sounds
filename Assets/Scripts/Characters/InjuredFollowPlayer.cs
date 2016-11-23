using UnityEngine;
using System.Collections;

public class InjuredFollowPlayer : FollowPlayer
{

    [Header("Flickering")]
    public AnimationCurve FlickerCurve;
    public float FlickerCurveTime;
    public float FlickerRate;

    public override void Start()
    {
        base.Start();
        StartCoroutine(Flicker());
    }

    private IEnumerator Flicker()
    {
        while(true) { 
            visibleOverride = Random.value < FlickerCurve.Evaluate(Time.time / FlickerCurveTime);
            yield return new WaitForSeconds(1/FlickerRate);
        }
    }
}
