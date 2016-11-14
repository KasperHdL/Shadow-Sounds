using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SonarBullet : MonoBehaviour {

    public SonarSource source;

    public Material material;
    public Color colorStart = new Color(1, 1, 1, 1);
    public Color colorEnd = new Color(1, 1, 1, 0);
    public float width = 0.1f;

    public float hitPitch = 1.0f;
    public float hitVolume = 1.0f;

    private LineRenderer line;

    public void Start() {
        StartCoroutine(DoUpdate());
    }

    private IEnumerator DoUpdate() {
        while(source == null)
            yield return null;

        Vector2 origin = transform.position;
        var angle = Mathf.Atan2(source.Direction.y, source.Direction.x);
        var coneAngleRad = Mathf.Deg2Rad * source.Angle;
        var coneIncrementRad = coneAngleRad / source.Rays;
        var startAngle = angle - coneAngleRad / 2;
        var done = new List<bool>();
        for(int i = 0; i < source.Rays; i++)done.Add(false);

        line = line == null ? gameObject.AddComponent<LineRenderer>() : GetComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.SetColors(colorStart, colorStart);
        line.SetWidth(width, width);
        line.SetVertexCount(source.Rays);
        line.material = material;

        var tt = source.Distance / source.Speed;
        var t = tt;

        var played = new HashSet<Collider2D>();

        while(t > 0) {
            var l = ((tt - t) / tt);
            var d = source.Distance * l;
            var dd = source.Speed * Time.fixedDeltaTime;

            for(int i = 0; i < source.Rays; i++)
            {
                if (done[i]) continue;

                var a = startAngle + coneIncrementRad * i;
                var dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));

                var soundhit = Physics2D.Raycast(origin + dir * d, dir, dd, source.SoundMask);
                var blockhit = Physics2D.Raycast(origin + dir * d, dir, dd, source.BlockMask);

                Vector2 hit = origin + dir*d;
                if (soundhit.collider != null && !played.Contains(soundhit.collider))
                {
                    played.Add(soundhit.collider);
                    SoundSystem.Play("sonar hit",
                        hitPitch * t,
                        (float)(hitVolume * (1 - System.Math.Log(d) / System.Math.Log(source.Distance))));
                }
                if (blockhit.collider != null)
                {
                    hit = blockhit.point;
                    done[i] = true;
                }

                line.SetPosition(i, hit);
            }

            var color = Color.Lerp(colorStart, colorEnd, l);
            line.SetColors(color, color);

            yield return new WaitForFixedUpdate();
            t -= Time.fixedDeltaTime;
        }

        Destroy(gameObject);
    }
}

public interface SonarSource {
    LayerMask SoundMask { get; }
    LayerMask BlockMask { get; }
    float Distance { get; }
    float Speed { get; }
    Vector2 Direction { get; }
    float Angle { get; }
    int Rays { get; }
}
