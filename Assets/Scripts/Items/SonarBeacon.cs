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

    public float Distance { get { return distance; } }
    public float Speed { get { return speed; } }
    public Vector2 Direction { get { return direction; } }
    public float Angle { get { return angle; } }
    public int Rays { get { return rays; } }


    void Start()
    {
        StartCoroutine(DoUpdate());
    }
    
    private IEnumerator DoUpdate() {
        while (true)
        {
            yield return new WaitForSeconds(1/rate);
            Debug.Log("SHOOT");
            var bullet = (SonarBullet)Instantiate(sonarBulletPrefab, transform.position, Quaternion.identity);
			bullet.source = this;
        }
    }
}
