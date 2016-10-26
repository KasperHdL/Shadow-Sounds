using UnityEngine;
using System.Collections;

public class EnemyMove : MonoBehaviour
{
    public Transform target;
    public float speed;
    public bool visible;


    void Update()
    {
        //target = GetComponent<pla>();

        float step = speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, target.position, step);
    }

    // Use this for initialization
    void Start () {
	
	}
	
}
