using UnityEngine;
using System.Collections;

public class LightActive : MonoBehaviour, IActivatable {
    public void Activate() {
        GetComponent<Light>().enabled = true;
    }

    public void ShutDown() {
        GetComponent<Light>().enabled = false;
    }

    public void Trigger() {
        GetComponent<Light>().enabled = !GetComponent<Light>().enabled;
    }
}
