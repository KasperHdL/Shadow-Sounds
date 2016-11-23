using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PillarAnimator : MonoBehaviour {

    public AnimationCurve RotationCurve;
    public float RotationCurveSpeed;
    public float RotationCurveFactor;
    public float RotationCurveRandomize;
    public AnimationCurve Rotation2Curve;
    public float Rotation2CurveSpeed;
    public float Rotation2CurveFactor;
    public AnimationCurve ScaleCurve;
    public float ScaleCurveSpeed;
    public float ScaleCurveFactor;
    public float ScaleCurveRandomize;
    public AnimationCurve PositionCurve;
    public float PositionCurveSpeed;
    public float PositionCurveFactor;
    public float PositionCurveRandomize;
    public AnimationCurve PositionPulseCurve;
    public float PositionPulseCurveSpeed;
    public float PositionPulseCurveFactor;

    private List<Transform> children;
    private List<Vector3> positions;
    private List<Vector3> scales;

    private List<float> scaleRandom;
    private List<float> rotationRandom;
    private List<float> positionRandom;
    private List<Vector3> target;

    public void Start() {
        children = new List<Transform>();
        positions = new List<Vector3>();
        scales = new List<Vector3>();
        scaleRandom = new List<float>();
        rotationRandom = new List<float>();
        positionRandom = new List<float>();
        target = new List<Vector3>();
        for(int i = 0; i < transform.childCount; i++) {
            var child = transform.GetChild(i);
            if(child.GetComponent<Light>() != null)
                continue;

            children.Add(child);
            positions.Add(child.localPosition);
            scales.Add(child.localScale);
            scaleRandom.Add(Random.value * ScaleCurveRandomize);
            rotationRandom.Add(Random.value * RotationCurveRandomize);
            positionRandom.Add(Random.value * PositionCurveRandomize);
            target.Add(Random.insideUnitCircle.normalized);
        }
    }

    public void Update() {
        for(int i = 0; i < children.Count; i++) {
            var angle = Rotation2Curve.Evaluate(Time.time * Rotation2CurveSpeed) * Mathf.PI*2 + Mathf.Atan2(positions[i].y, positions[i].x);
            children[i].localPosition = Vector3.LerpUnclamped(positions[i], positions[i] + target[i] * PositionCurveFactor,
                    PositionCurve.Evaluate(Time.time * PositionCurveSpeed + positionRandom[i]))
                    + positions[i].normalized * PositionPulseCurve.Evaluate(Time.time * PositionPulseCurveSpeed - positions[i].magnitude) * PositionPulseCurveFactor
                    + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * Rotation2CurveFactor;
            children[i].localEulerAngles += Vector3.forward * RotationCurve.Evaluate(Time.time * RotationCurveSpeed + rotationRandom[i]) * RotationCurveFactor * Time.deltaTime;
            children[i].localScale = scales[i] + Vector3.one * ScaleCurve.Evaluate(Time.time * ScaleCurveFactor + scaleRandom[i]) * ScaleCurveFactor;
        }
    }
}
