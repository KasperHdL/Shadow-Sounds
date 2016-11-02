using UnityEngine;
using KInput;

public class ControllerContainer : MonoBehaviour{
    public int controllerIndex;
    public Controller controller;

    void Awake(){
        controller = new Xbox360(controllerIndex);
    }
}
