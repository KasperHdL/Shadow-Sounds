using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterMovement : MonoBehaviour
{
    [HideInInspector]
    public Vector2 Move;

    public float MoveSpeed = 10;
    public float Acceleration = 0.5f;
    public float Deceleration = 0.5f;
    public AnimationCurve StartCurve;
    public AnimationCurve StopCurve;

    protected Rigidbody2D body;
    private Vector2 speed;

    public virtual void Start()
    {
        body = GetComponent<Rigidbody2D>();
        speed = Vector3.zero;
    }

    public virtual void Update()
    {
        var input = Move.normalized;
        speed = Vector2.MoveTowards(speed, input, Time.deltaTime / (input.sqrMagnitude > 0 ? Acceleration : Deceleration));
        var actualSpeed = MoveSpeed * (input.sqrMagnitude > 0 ? StartCurve.Evaluate(speed.magnitude) : StopCurve.Evaluate(speed.magnitude));
        
        body.AddForce(speed.normalized * actualSpeed - body.velocity, ForceMode2D.Impulse);
    }
}
