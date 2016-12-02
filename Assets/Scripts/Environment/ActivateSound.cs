using UnityEngine;
using System.Collections;

public class ActivateSound : MonoBehaviour, IActivatable {
    public void Activate()
    {
        GetComponent<AudioSource>().Play();
        Debug.Log("activated");
    }

    public void ShutDown()
    {
        GetComponent<AudioSource>().Stop();
    }

    public void Trigger()
    {
        Activate();
    }
}
