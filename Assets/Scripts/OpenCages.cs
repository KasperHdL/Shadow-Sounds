using UnityEngine;
using System.Collections;

public class OpenCages : MonoBehaviour {

    public GameObject[] cages;

    void OnTriggerEnter2D(Collider2D enter)
    {
        if(enter.gameObject.tag == "Player"){
            foreach ( GameObject elem in cages){
                Destroy(elem);
            }
        }
    }
}
