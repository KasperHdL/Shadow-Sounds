using UnityEngine;
using System.Collections;

public class LevelExit : MonoBehaviour {

    private LevelHandler levelHandler;
    private bool loadCallSent = false;

    public void SetLevelHandler(LevelHandler levelHandler){
        this.levelHandler = levelHandler;
    }

    void OnTriggerEnter2D(Collider2D coll){
        if(levelHandler == null || loadCallSent)return;

        if(coll.gameObject.tag == "Player"){
            levelHandler.LevelCompleted();
            loadCallSent = true;
        }
    }
}
