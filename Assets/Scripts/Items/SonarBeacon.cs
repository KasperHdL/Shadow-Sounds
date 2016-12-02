using UnityEngine;
using System.Collections;

public class SonarBeacon : MonoBehaviour, SonarSource {

    public SonarBullet sonarBulletPrefab;

    public float rate;

    public float distance;
    public float speed;
    public Vector2 direction;
    public float angle;
    public int rays;
    public float sonarPct = 1.0f;

    public float Distance { get { return distance; } }
    public float Speed { get { return speed; } }
    public Vector2 Direction { get { return direction; } }
    public float Angle { get { return angle; } }
    public int Rays { get { return rays; } }
    public float SonarPct { get { return sonarPct; } }
    private GameObject pl;
    public float silenceDistance = 5.0f;


    void Start()
    {
        pl = GameObject.FindGameObjectWithTag("Player");
        StartCoroutine(DoUpdate());
    }
    
    private IEnumerator DoUpdate() {
        while (true)
        {
            yield return new WaitForSeconds(1/rate);

            var plyrDist = Vector3.Distance(transform.position, pl.transform.position);

            var bullet = (SonarBullet)Instantiate(sonarBulletPrefab, transform.position, Quaternion.identity);
			bullet.source = this;

            if (plyrDist > silenceDistance)
            {
                bullet.hitVolume = 0.0f;
                bullet.noise = 0;
                bullet.noHitVolume = 0;
            }
            else
            {

                bullet.hitVolume = 0.5f/plyrDist;
                bullet.noise = 1 / plyrDist;
                
            }
            
        }
    }
}
