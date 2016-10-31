using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour {

    public float moveSpeed = 50.0f;
    public AudioClip footstepsClip;
    public GameObject sourceContainer;
    public float stepVolume = 0.1f;
    private AudioSource footsteps;

    public AudioClip enemyCollision, wallCollision, defaultCol;
    public float enemyColVol = 0.5f;
    public float defColVol = 0.5f;

    private AudioSource collisionSnd;

    [HideInInspector]
    public Vector2 viewDirection;
    private Rigidbody2D body;
    private bool fallen;

	void Start () {
        body = GetComponent<Rigidbody2D>();
	    footsteps = sourceContainer.AddComponent<AudioSource>();
        collisionSnd = sourceContainer.AddComponent<AudioSource>();
        footsteps.clip = footstepsClip;
	    footsteps.volume = stepVolume;
	}

    void OnCollisionEnter2D(Collision2D other)
    {
        switch (other.gameObject.tag)
        {
            case "Enemy":
                collisionSnd.clip = enemyCollision;
                collisionSnd.volume = enemyColVol;
              //  Debug.Log("EnemyCollision");
                break;
            case "Wall":
                collisionSnd.clip = wallCollision;
                collisionSnd.volume = defColVol;
                //Debug.Log("WallCollision");
                break;
            default:
                collisionSnd.clip = defaultCol;
                collisionSnd.volume = defColVol;
                //Debug.Log("No collision tag set!");
                break;
        }

        collisionSnd.Play();
    }
	
	void Update () {
        var v = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")) * moveSpeed / body.mass;


        if (body.velocity.magnitude >= 0.1)
        {
            if (!footsteps.isPlaying)
            {
                footsteps.Play();
            }
        }
        else
        {
            footsteps.Stop();
        }

	    if (!collisionSnd.isPlaying)
	    {

	        body.AddForce(v - new Vector3(body.velocity.x, body.velocity.y), ForceMode2D.Impulse);

	    }


        Vector2 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        viewDirection = mouse - (Vector2)transform.position;

        transform.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * Mathf.Atan2(viewDirection.y, viewDirection.x));
    }
}
