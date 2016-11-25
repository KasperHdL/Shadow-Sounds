using UnityEngine;
using System.Collections;

public class GoOutOrInside : MonoBehaviour
{

    public bool Inside = true;


    void OnTriggerEnter2D(Collider2D enter)
    {
        if (enter.gameObject.tag == "Player")
        {
            Debug.LogWarning("Player collided");

            GameObject.FindWithTag("Player").GetComponent<PlayerMovement>().setInside(Inside);
        }
    }
}
