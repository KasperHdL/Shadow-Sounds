using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
public class Checkpoint : MonoBehaviour {

    public SaveSystem savesystem;
    public Vector2 SpawnOffset;

    public void Start() {

        if(savesystem == null) {
            var go = GameObject.FindGameObjectWithTag("SaveSystem") ??
                     new GameObject("SaveSystem", typeof(SaveSystem)) {tag = "SaveSystem"};
            savesystem = go.GetComponent<SaveSystem>();
        }

    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
            savesystem.PlayerPosition = (Vector2)transform.position + SpawnOffset;
    }

    public void OnDrawGizmos()
    {
        var box = GetComponent<BoxCollider2D>();
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube((Vector2)transform.position + box.offset, box.size);
        Gizmos.DrawWireSphere((Vector2)transform.position + SpawnOffset, 0.2f);
    }
}
