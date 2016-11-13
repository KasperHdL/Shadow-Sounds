using UnityEngine;
using System.Collections;

public class OpenDoor : MonoBehaviour {

    public Material[] materials;
    public Transform door;
    public bool open = false;
    public float openSpeed;
    private float maxDistance;
    private float currDistance;
	
    void Start()
    {
        maxDistance = door.localScale.y;
        currDistance = 0.0f;
        if(open == false){
            GetComponent<Renderer>().sharedMaterial = materials[0];
        }
        else if(open == true){
            GetComponent<Renderer>().sharedMaterial = materials[1];
        }
    }

    void Update()
    {
        if(open == true && currDistance < maxDistance){
            door.transform.position += door.transform.up*openSpeed*Time.deltaTime;
            currDistance += openSpeed*Time.deltaTime;
        }

        else if(open == false && currDistance > 0.0f){
            door.transform.position -= door.transform.up*openSpeed*Time.deltaTime;
            currDistance -= openSpeed*Time.deltaTime;

            if(currDistance < 0.0f){
                door.transform.position += door.transform.up*currDistance;
                currDistance = 0.0f;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D enter)
    {
        if(enter.gameObject.tag == "Player"){
            if(open == false){
                open = true;
                GetComponent<Renderer>().sharedMaterial = materials[1];
            }
            else if(open == true){
                open = false;
                GetComponent<Renderer>().sharedMaterial = materials[0];
            }
        }
    }
}
