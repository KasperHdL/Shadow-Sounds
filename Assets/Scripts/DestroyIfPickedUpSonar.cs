using UnityEngine;
using System.Collections;

public class DestroyIfPickedUpSonar : MonoBehaviour {

	void Start () {
	    var savesystem = GameObject.FindGameObjectWithTag("SaveSystem");
        if (savesystem != null ){

            SaveSystem save = savesystem.GetComponent<SaveSystem>();

            if(save.playerPickedUpSonar)
                Destroy(gameObject);
        }
	}
}
