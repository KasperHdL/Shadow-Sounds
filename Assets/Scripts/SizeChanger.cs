using UnityEngine;
using System.Collections;

public class SizeChanger : MonoBehaviour {

    public float amount = 0f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnTriggerEnter2D(Collider2D coll){
        if(coll.gameObject.tag == "Player"){
            coll.gameObject.GetComponent<Player>().changeSize(amount);
        }
    }
}
