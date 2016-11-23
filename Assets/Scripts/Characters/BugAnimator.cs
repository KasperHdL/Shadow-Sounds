using UnityEngine;
using System.Collections;
using System.Linq;

public class BugAnimator : MonoBehaviour {
    private Rigidbody2D rigidbody;
    private FollowPlayer enemy;

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

    public float velocityFactor = 1;


    public void Start()
    {
        rigidbody = transform.parent.GetComponent<Rigidbody2D>();
        enemy = transform.parent.GetComponent<FollowPlayer>();

        leftJawOffset = Random.value*RandomJawOffset;
        rightJawOffset = Random.value * RandomJawOffset;
        leftLegOffsets = LeftLegs.Select(_ => Random.value * RandomLegOffset).ToArray();
        rightLegOffsets = RightLegs.Select(_ => Random.value * RandomLegOffset).ToArray();
    }

    public void Update() {
        LeftJaw.localEulerAngles = new Vector3(0, 0, AnimationJawFactor * JawCurve.Evaluate(Time.time * AnimationJawSpeed + leftJawOffset));
        RightJaw.localEulerAngles = new Vector3(0, 0, -AnimationJawFactor * JawCurve.Evaluate(Time.time * AnimationJawSpeed + rightJawOffset));

        float speedMultiplier = (enemy.attacking ? 2 : (rigidbody.velocity.magnitude == 0 ? 0 : 1)) * velocityFactor;

        float t = Time.time * AnimationLegSpeed * speedMultiplier;

        for (int i = 0; i < LeftLegs.Length; i++)
            LeftLegs[i].localEulerAngles = new Vector3(0, 0, AnimationLegFactor*LegCurve.Evaluate(t + leftLegOffsets[i]));
        for (int i = 0; i < RightLegs.Length; i++)
            RightLegs[i].localEulerAngles = new Vector3(0, 0, -AnimationLegFactor*LegCurve.Evaluate(t) + rightLegOffsets[i]);
    }
}
