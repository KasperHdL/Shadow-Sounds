using UnityEngine;
using System.Collections;
using System.Linq;
using KInput;

public class PlayerMovement : MonoBehaviour
{

    public int health = 4;
    public float hitForce;

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

    public bool useController;
    private Controller controller;

	void Start () {
        controller = GetComponent<ControllerContainer>().controller;

        body = GetComponent<Rigidbody2D>();
	    footsteps = sourceContainer.AddComponent<AudioSource>();
        collisionSnd = sourceContainer.AddComponent<AudioSource>();
        footsteps.clip = footstepsClip;
	    footsteps.volume = stepVolume;
	}

    void OnCollisionEnter2D(Collision2D collision)
    {
        switch (collision.gameObject.tag)
        {
            case "Enemy":
                collisionSnd.clip = enemyCollision;
                collisionSnd.volume = enemyColVol;
                //Debug.Log("EnemyCollision");

                health--;
                if (health <= 0) Die();

                var avgNormal = collision.contacts.Aggregate(Vector2.zero, (a, c) => a + c.normal) / collision.contacts.Length;
                body.AddForce(avgNormal * hitForce);
                collision.rigidbody.AddForce(-avgNormal * hitForce);

                break;
            case "Wall":
                collisionSnd.clip = wallCollision;
                collisionSnd.volume = defColVol;
                //Debug.Log("WallCollision");
                break;
            default:
                collisionSnd.clip = defaultCol;
                collisionSnd.volume = defColVol;
                //Debug.Log("No tag set!");
                break;
        }

        collisionSnd.Play();
    }

    private void Die()
    {
        Destroy(gameObject);

        // TODO restart game
    }
	
	void Update () {
        Vector3 v;
        if(useController){
           v = new Vector3(controller.GetAxis(Axis.StickLeftX), controller.GetAxis(Axis.StickLeftY),0) ;
        }else{
           v = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"),0) ;
        }

        
        v *= moveSpeed / body.mass;

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


        body.AddForce(v - new Vector3(body.velocity.x, body.velocity.y), ForceMode2D.Impulse);



        if(useController){
            viewDirection = new Vector2(controller.GetAxis(Axis.StickRightX), controller.GetAxis(Axis.StickRightY));
        }else{
            Vector2 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            viewDirection = mouse - (Vector2)transform.position;
            viewDirection = viewDirection.normalized;
        }

        transform.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * Mathf.Atan2(viewDirection.y, viewDirection.x));
    }
}
