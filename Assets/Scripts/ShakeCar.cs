using UnityEngine;
using System.Collections;

public class ShakeCar : MonoBehaviour {

    public float shakeCarAmount = 0.05f;
    public AnimationCurve shakeCarCurve;
    [HideInInspector]
    public Vector3 carOrigin;

    void Start() {
        carOrigin = transform.position;
    }
    void Update() {
        var shake = Vector3.right * Random.Range(-1f, 1f) * shakeCarAmount * shakeCarCurve.Evaluate(Time.time);
        transform.position = carOrigin + shake;
    }
}
