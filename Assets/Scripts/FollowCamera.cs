using UnityEngine;
using System.Collections;

public class FollowCamera : MonoBehaviour {

    public Transform player;
    public Vector3 offset;

    void Start(){
        offset = transform.position - player.position;
    }

	void Update () {
        transform.position = player.position + offset;
	
	}
}
