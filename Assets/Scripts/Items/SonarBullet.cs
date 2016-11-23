using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SonarBullet : MonoBehaviour {

    public SonarSource source;

    public LayerMask SoundMask;
    public LayerMask BlockMask;
    public LayerMask HighlightMask;

    public Material material;
    public Color colorStart = new Color(1, 1, 1, 1);
    public Color colorEnd = new Color(1, 1, 1, 0);
    public Color colorHighlight = new Color(1, 1, 1, 1);
    public AnimationCurve colorCurve;
    public float width = 0.1f;
    public float highlightWidth = 0.2f;

    public float noise = 10;
    public float noiseBlockMultiplier = 0.1f;
    public float noiseHitMultiplier = 1.5f;
    public float noiseHighlightMultiplier = 3.0f;

    public float hitPitch = 0.5f;
    public float hitVolume = 1.0f;
    public float noHitVolume = 1.0f;


    private LineRenderer line;

    public void Start() {
        StartCoroutine(DoUpdate());
    }

    private IEnumerator DoUpdate() {
        while(source == null)
            yield return null;

        var sonarPct = source.SonarPct;

        Vector2 origin = transform.position;
        var angle = Mathf.Atan2(source.Direction.y, source.Direction.x);
        var coneAngleRad = Mathf.Deg2Rad * source.Angle;
        var coneIncrementRad = coneAngleRad / source.Rays;
        var startAngle = angle - coneAngleRad / 2;
        var done = new List<bool>();
        for(int i = 0; i < source.Rays; i++)
            done.Add(false);
        var highlight = new List<Vector2?>();
        for(int i = 0; i < source.Rays; i++)
            highlight.Add(null);

        line = line == null ? gameObject.AddComponent<LineRenderer>() : GetComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.SetColors(colorStart, colorStart);
        line.SetWidth(width * sonarPct, width * sonarPct);
        line.SetVertexCount(source.Rays);
        line.material = material;

        var tt = source.Distance / source.Speed;
        var t = tt;

        var played = new HashSet<Collider2D>();
        var finished = false;

        while(t > 0) {
            var l = ((tt - t) / tt);
            var cl = colorCurve.Evaluate(1-t);
            var d = source.Distance * l;
            var dd = source.Speed * Time.fixedDeltaTime;
            var h = 0;

            for(int i = 0; i < source.Rays; i++) {
                if(done[i])
                    continue;
                h++;

                var a = startAngle + coneIncrementRad * i;
                var dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));

                var soundhit = Physics2D.Raycast(origin + dir * d, dir, dd, SoundMask);
                var blockhit = Physics2D.Raycast(origin + dir * d, dir, dd, BlockMask);
                var highlighthit = Physics2D.Raycast(origin + dir * d, dir, dd, HighlightMask);

                var nois = noise / 1000;

                Vector2 hit = origin + dir * d;
                if(soundhit.collider != null) {
                    nois *= noiseHitMultiplier;

                    if(!played.Contains(soundhit.collider)) {
                        played.Add(soundhit.collider);
                        SoundSystem.Play("sonar hit",
                            hitPitch * t,
                            (float)(sonarPct * hitVolume * (1 - System.Math.Log(d) / System.Math.Log(source.Distance))));
                    }
                }

                highlight[i] = highlighthit.collider != null ? (Vector2?)hit : null;
                if(highlighthit.collider != null)
                    nois *= noiseHighlightMultiplier;

                if(blockhit.collider != null) {
                    hit = blockhit.point;
                    done[i] = true;
                    highlight[i] = null;

                    nois *= noiseBlockMultiplier;
                }

                hit += Random.insideUnitCircle.normalized * nois;
                line.SetPosition(i, hit);
            }

            var n = 0;
            LineRenderer cline = null;
            for(int i = 0; i < source.Rays; i++) {
                if(highlight[i].HasValue) {
                    if(cline == null) {
                        var go = new GameObject("Hightlight");
                        go.transform.parent = transform;
                        cline = go.AddComponent<LineRenderer>();
                        cline.useWorldSpace = true;
                        cline.material = material;
                        cline.SetColors(colorHighlight, colorHighlight);
                        cline.SetWidth(highlightWidth, highlightWidth);
                        Destroy(go, Time.fixedDeltaTime);
                    }
                    n++;
                    cline.SetVertexCount(n);
                    cline.SetPosition(n - 1, (Vector3)highlight[i].Value + Vector3.back);
                }
                if(!highlight[i].HasValue && cline != null) {
                    cline = null;
                    n = 0;
                }
            }

            var color = Color.LerpUnclamped(colorStart, colorEnd, cl);
            line.SetColors(color, color);

            if(h == 0 && !finished) {
                SoundSystem.Play("sonar no hit", 1, noHitVolume);
                SoundSystem.Stop("sonar noise");
                finished = true;
            }

            yield return new WaitForFixedUpdate();
            t -= Time.fixedDeltaTime;
        }

        Destroy(gameObject);
    }
}

public interface SonarSource {
    float Distance { get; }
    float Speed { get; }
    Vector2 Direction { get; }
    float Angle { get; }
    int Rays { get; }
    float SonarPct { get; }
}
