using UnityEngine;

public class Door : MonoBehaviour {

    public enum State{
        Closed,
        Closing,
        Open,
        Opening,
    };

    public State state;

    public float openDuration;
    private float aniTime;

    public float minScale;
    private float maxScale;

    private Vector3 startPosition;
    private float moveAmount;

    public Vector3 openDirection;

    void OnReset(){
        if(openDirection == Vector3.zero)
            openDirection = transform.up;
    }

	// Use this for initialization
	void Start () {
        maxScale = transform.localScale.y;
        moveAmount = maxScale / 2;
        
        if(openDirection == Vector3.zero)
            openDirection = transform.up;

        startPosition = transform.position + openDirection * moveAmount;

        if(state == State.Open || state == State.Opening)
            aniTime = 0;
        else
            aniTime = openDuration;

        SetDoor();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        if(state == State.Closed || state == State.Open)
            return;

        SetDoor();

        switch(state){
        case State.Opening:
                aniTime -= Time.fixedDeltaTime;
                if(aniTime <= 0){
                    aniTime = 0;
                    state = State.Open;
                }
            break;
            case State.Closing:
                aniTime += Time.fixedDeltaTime;
                if(aniTime >= openDuration){
                    aniTime = openDuration;
                    state = State.Closed;
                }
            break;
        }
	}
    
    private void SetDoor(){
        float t = aniTime / openDuration;
        transform.position = startPosition - openDirection * moveAmount * t;

        Vector3 s = transform.localScale;
        s.y = (maxScale - minScale) * t;
        transform.localScale = s;
    }

    public void Toggle(){
        if(state == State.Closed || state == State.Closing)
            state = State.Opening;
        else
            state = State.Closing;
    }

    public void Open(){
        if(state != State.Open)
            state = State.Opening;
    }

    public void Close(){
        if(state != State.Closed)
            state = State.Closing;
    }



}
