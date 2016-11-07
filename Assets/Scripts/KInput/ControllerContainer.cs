using UnityEngine;
using KInput;

public class ControllerContainer : MonoBehaviour{
    public int controllerIndex;
    public Controller controller;
    [Range(0,1)] public float deadzone = 0.1f;

    void Awake(){
        controller = new Xbox360(deadzone, controllerIndex);
    }
}
