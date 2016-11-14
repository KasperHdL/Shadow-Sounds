using UnityEngine;
using System.Collections;

public class RemoveOnStart : MonoBehaviour {
    private void Awake() {
        Destroy(gameObject);
    }
}
