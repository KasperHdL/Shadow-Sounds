using UnityEngine;
using System.Collections;

public class Button : MonoBehaviour {

    public enum ButtonMode{
        ToggleDoor,
        OpenDoor,
        CloseDoor
    };

    public Door door;
    public ButtonMode buttonMode;

    void FixedUpdate(){
        //State Inormation

    }

    void OnTriggerEnter2D(Collider2D coll){
        if(coll.gameObject.tag == "Player"){
            switch(buttonMode){
                case ButtonMode.ToggleDoor:
                    door.Toggle();
                    break;
                case ButtonMode.OpenDoor:
                    door.Open();
                    break;
                case ButtonMode.CloseDoor:
                    door.Close();
                    break;
            }
        }
    }
}
