using UnityEngine;
using System.Collections;
using System.Linq;

public class BugAnimator : MonoBehaviour {

    public Transform LeftJaw;
    public Transform RightJaw;
    public Transform[] LeftLegs;
    public Transform[] RightLegs;

    public AnimationCurve JawCurve;
    public float AnimationJawSpeed;
    public float AnimationJawFactor;
    public float RandomJawOffset;
    public AnimationCurve LegCurve;
    public float AnimationLegSpeed;
    public float AnimationLegFactor;
    public float RandomLegOffset;

    private float leftJawOffset;
    private float rightJawOffset;
    private float[] leftLegOffsets;
    private float[] rightLegOffsets;

    public void Start()
    {
        leftJawOffset = Random.value*RandomJawOffset;
        rightJawOffset = Random.value * RandomJawOffset;
        leftLegOffsets = LeftLegs.Select(_ => Random.value * RandomLegOffset).ToArray();
        rightLegOffsets = RightLegs.Select(_ => Random.value * RandomLegOffset).ToArray();
    }

    public void Update() {
        LeftJaw.localEulerAngles = new Vector3(0, 0, AnimationJawFactor * JawCurve.Evaluate(Time.time * AnimationJawSpeed + leftJawOffset));
        RightJaw.localEulerAngles = new Vector3(0, 0, -AnimationJawFactor * JawCurve.Evaluate(Time.time * AnimationJawSpeed + rightJawOffset));

        for (int i = 0; i < LeftLegs.Length; i++)
            LeftLegs[i].localEulerAngles = new Vector3(0, 0, AnimationLegFactor*LegCurve.Evaluate(Time.time* AnimationLegSpeed + leftLegOffsets[i]));
        for (int i = 0; i < RightLegs.Length; i++)
            RightLegs[i].localEulerAngles = new Vector3(0, 0, -AnimationLegFactor*LegCurve.Evaluate(Time.time* AnimationLegSpeed + rightLegOffsets[i]));
    }
}
