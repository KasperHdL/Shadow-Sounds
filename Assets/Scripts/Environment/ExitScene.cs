using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement; 

public class ExitScene : MonoBehaviour {

    void OnTriggerEnter2D(Collider2D enter){
        if(enter.gameObject.tag == "Player"){
            Debug.LogWarning("Player collided");
            Scene scene = SceneManager.GetActiveScene(); 
            SceneManager.LoadScene(scene.name);
        }
    }
}
