using UnityEngine;
using System.Collections;

public class OpenDoor : MonoBehaviour {

    public Transform door;
    public bool open = false;
    public float openSpeed;
    private float maxDistance;
    private float currDistance;
	
    void Start()
    {
        maxDistance = door.localScale.y;
        currDistance = 0.0f;
    }

    void Update()
    {
        if(open == true && currDistance < maxDistance){
            Vector3 delta = door.transform.position;
            delta.z = 0;
            delta.y += openSpeed*Time.deltaTime;
            door.transform.position = delta;
            currDistance += openSpeed*Time.deltaTime;
        }
    }

    void OnTriggerEnter2D(Collider2D enter)
    {
        if(enter.gameObject.tag == "Player"){
            if(open == false){
                open = true;
            }
        }
    }
}
