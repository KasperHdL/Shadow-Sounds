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
            if (Inside)
            {
                SoundSystem.Play("background",0,0.8f);
                SoundSystem.Stop("Outside");
            }
            else
            {

                SoundSystem.Stop("background");
                SoundSystem.Play("Outside",1,0.10f);
            }

        }
    }
}
