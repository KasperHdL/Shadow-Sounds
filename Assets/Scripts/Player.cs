using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

    public Vector3 startSize;
    public LevelHandler levelHandler;
    public float forwardSpeed;
    public float sideSpeed;
    public KeyCode leftKey;
    public KeyCode rightKey;

    public float boundary = 13f;

	void Start () {
        startSize = transform.localScale;
	
	}
	
	void Update () {
        Vector3 d = Vector3.up * forwardSpeed;


        if(Input.GetKey(leftKey))
            d.x = -sideSpeed;
        else if(Input.GetKey(rightKey))
            d.x = sideSpeed;

        if(Mathf.Abs(transform.position.x + (d.x * Time.deltaTime)) >= boundary)
            d.x = 0f;

        transform.position += d * Time.deltaTime;
	}

    void OnCollisionEnter2D(Collision2D coll){
        if(coll.gameObject.tag == "Obstacle"){
            transform.position = levelHandler.GetResetPosition();
            resetSize();
        }
    }

    public void changeSize(float a){
        transform.localScale += new Vector3(a,a,0);
    }

    public void resetSize(){
        transform.localScale = startSize;

    }

    void OnTriggerEnter2D(Collider2D other) {

        if(other.tag == "PositiveObject"){
            Destroy(other.gameObject);
        }

        if(other.tag == "NegativeObject"){
            Destroy(other.gameObject);
        }
    }
}
