using UnityEngine;
using System.Collections;
using System.Linq;
using KInput;
using UnityEngine.SceneManagement; 

public class PlayerMovement : MonoBehaviour
{

    public int health = 4;
    public float hitForce;

    public float moveSpeed = 50.0f;
    public float stepVolume = 0.1f;
    public bool fallOnWalls;
    
    public float enemyColVol = 0.5f;
    public float defColVol = 0.5f;

    [HideInInspector]
    public Vector2 viewDirection;
    private Rigidbody2D body;
    private bool fallen;

    public bool useController;
    [Range(0,1)] public float deadzone = 0.1f;
    private Controller controller;

    void Start()
    {
        controller = GetComponent<ControllerContainer>().controller;

        body = GetComponent<Rigidbody2D>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        switch (collision.gameObject.tag)
        {
            case "Enemy":
                SoundSystem.Play("enemy collision", enemyColVol);
                //Debug.Log("EnemyCollision");

                var avgNormal = collision.contacts.Aggregate(Vector2.zero, (a, c) => a + c.normal) / collision.contacts.Length;
                body.AddForce(avgNormal * hitForce);
                collision.rigidbody.AddForce(-avgNormal * hitForce);

                break;
            case "Wall":
                if (fallOnWalls)
                    SoundSystem.Play("wall collision", defColVol);

                //Debug.Log("WallCollision");
                break;
            default:
                if (fallOnWalls)
                    SoundSystem.Play("default collision", defColVol);

                //Debug.Log("No collision tag set!");
                break;
        }

    }

    public void Hit(int damage)
    {
        health -= damage;
        if (health <= 0) Die();
    }
    private void Die()
    {
        Destroy(gameObject);

        Scene scene = SceneManager.GetActiveScene(); 
        SceneManager.LoadScene(scene.name);
    }

    void Update()
    {
        Vector3 v;
        if (useController)
        {
            v = new Vector3(controller.GetAxis(Axis.StickLeftX), controller.GetAxis(Axis.StickLeftY), 0);

            if(v.magnitude < deadzone){
                v = Vector2.zero;
            }else{
                v = v.normalized * ((v.magnitude - deadzone) / (1 - deadzone));
            }

        }
        else
        {
            v = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0);
        }


        v *= moveSpeed / body.mass;
        
        if (body.velocity.magnitude >= 0.1)
        {
            SoundSystem.Play("footsteps", stepVolume, 1, 0, true);
        }
        else
        {
            SoundSystem.Stop("footsteps");
        }

        body.AddForce(v - new Vector3(body.velocity.x, body.velocity.y), ForceMode2D.Impulse);



        if (useController)
        {
            Vector2 d = new Vector2(controller.GetAxis(Axis.StickRightX), controller.GetAxis(Axis.StickRightY));
            if(d.magnitude < deadzone){
                d = Vector2.zero;
            }else{
                d = d.normalized * ((d.magnitude - deadzone) / (1 - deadzone));
            }

            if(d != Vector2.zero){
                viewDirection = d;
            }
        }
        else
        {
            Vector2 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            viewDirection = mouse - (Vector2)transform.position;
            viewDirection = viewDirection.normalized;
        }

        transform.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * Mathf.Atan2(viewDirection.y, viewDirection.x));
    }
}
