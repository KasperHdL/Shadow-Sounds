using UnityEngine;
using System.Collections;

public class Button : Interactable{

    public enum ButtonMode{
        ToggleDoor,
        OpenDoor,
        CloseDoor
    };

    public Door door;
    public ButtonMode buttonMode;

    public Color openColor;
    public Color closedColor;
    public Color errorColor;

    public float blinkingColorReduce = 0.5f;
    public float blinkingDelay = .25f;
    public float errorBlinkLength = .5f;
    private bool isBlinking = false;
    private float nextBlinkTime = 0f;
    private float stopBlinkAt = -1;
    private bool blinkIsOn = false;

    private Material material;

    public MeshRenderer buttonRenderer;
    public Light buttonLight;

    private Door.State doorState;
    private bool first = true;

    public bool debug = false;
    void Start(){

        material = new Material(buttonRenderer.material);
        buttonRenderer.material = material;

        door.AddButton(this);
    }

    void FixedUpdate(){
        if(debug){
            first = true;
            DoorStateChanged(door.state);
        }
        if(!isBlinking) return;
        if(stopBlinkAt > 0 && stopBlinkAt < Time.time){
            isBlinking = false;
            stopBlinkAt = -1;
            setColor(errorColor);
            return;
        }
        
        if(nextBlinkTime < Time.time){
            blinkIsOn = !blinkIsOn;

            Color c = openColor;
            switch(doorState){
                case Door.State.Closing: 
                    c = closedColor; 
                    break;

                case Door.State.Error: 
                    c = errorColor; 
                    break;
            }

            if(!blinkIsOn)
                c -= new Color(blinkingColorReduce, blinkingColorReduce, blinkingColorReduce, 0);

            setColor(c);

            nextBlinkTime = Time.time + blinkingDelay;
        }
    }

    public void DoorStateChanged(Door.State state){
        if(state != doorState || first || state == Door.State.Error){
            switch(state){
                case Door.State.Open:
                    setColor(openColor);
                    isBlinking = false;

                break;
                case Door.State.Closed:
                    setColor(closedColor);
                    isBlinking = false;
                break;

                case Door.State.Opening:
                case Door.State.Closing:
                case Door.State.Error:
                    isBlinking = true;  
                    nextBlinkTime = Time.time + blinkingDelay;
                    blinkIsOn = false;

                break;
            }
            
            if(state == Door.State.Error){
                setColor(errorColor);
                stopBlinkAt = Time.time + errorBlinkLength;
            }

            doorState = state;
            first = false;
        }
    }

    private void setColor(Color color){
        material.color = color;
        material.SetColor("_EmissionColor", color);
        buttonLight.color = color;
     
    }

    public override void Interact(){
	SoundSystem.Play("buttonClick");
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
