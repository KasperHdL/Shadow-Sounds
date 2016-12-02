using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class SonarBullet : MonoBehaviour {

    public SonarSource source;

    public LayerMask SoundMask;
    public LayerMask BlockMask;
    public LayerMask HighlightMask;
    public LayerMask TransparentMask;

    public Material material;
    public Color colorStart = new Color(1, 1, 1, 1);
    public Color colorEnd = new Color(1, 1, 1, 0);
    public Color colorHighlight = new Color(1, 1, 1, 1);
    public AnimationCurve colorCurve;
    public float width = 0.1f;
    public float highlightWidth = 0.2f;
    public float transparentSlowRate = 0.01f;
    public float transparentMinSpeed = 0.1f;

    public float noise = 10;
    public float noiseBlockMultiplier = 0.1f;
    public float noiseHitMultiplier = 1.5f;
    public float noiseHighlightMultiplier = 3.0f;
    public float noiseTransMultiplier = 3.0f;

    public float hitPitch = 0.5f;
    public float hitVolume = 1.0f;
    public float noHitVolume = 0.2f;


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
        var highlight = new List<Vector2?>();
        var position = new List<Vector2>();
        var direction = new List<Vector2>();
        var slow = new List<int>();
        for(int i = 0; i < source.Rays; i++) {
            done.Add(false);
            highlight.Add(null);
            position.Add(origin);
            slow.Add(0);

            var a = startAngle + coneIncrementRad * i;
            direction.Add(new Vector2(Mathf.Cos(a), Mathf.Sin(a)));
        }

        line = line == null ? gameObject.AddComponent<LineRenderer>() : GetComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.SetColors(colorStart, colorStart);
        line.SetWidth(width * sonarPct, width * sonarPct);
        line.SetVertexCount(source.Rays);
        line.shadowCastingMode = ShadowCastingMode.Off;
        line.material = material;

        var tt = source.Distance / source.Speed;
        var t = tt;

        var played = new HashSet<Collider2D>();
        var playedNoHitSound = false;

        while(t > 0 ) {
            var h = 0; // Number of active points

            for(int i = 0; i < source.Rays; i++) {
                if(done[i])
                    continue;
                h++;

                var dd = (source.Speed - transparentSlowRate * slow[i]) * Time.fixedDeltaTime;

                var hits = Physics2D.RaycastAll(position[i], direction[i], dd,
                    SoundMask | BlockMask | HighlightMask | TransparentMask);

                var nois = noise / 1000;

                Vector2 p = position[i] + direction[i] * dd;
                highlight[i] = null;

                foreach(var hit in hits) {
                    // Check if hit sound maker
                    if((SoundMask & (1 << hit.collider.gameObject.layer)) != 0) {
                        nois *= noiseHitMultiplier;

                        if(!played.Contains(hit.collider)) {
                            played.Add(hit.collider);
                            SoundSystem.Play("sonar hit",
                                hitPitch * t,
                                (float)(sonarPct * hitVolume * (1 - System.Math.Log(Vector3.Distance(p, origin)) / System.Math.Log(source.Distance))));
                        }
                    }

                    // Check if hit a blocking collider
                    if((BlockMask & (1 << hit.collider.gameObject.layer)) != 0) {
                        //direction[i] = Vector2.Reflect(direction[i], hit.normal);
                        p = hit.point + direction[i] * 0.1f;
                        //slow[i] += 5;
                        done[i] = true;
                        highlight[i] = null;

                        nois *= noiseBlockMultiplier;
                        break;
                    }

                    // Check if hit a transparent
                    if((TransparentMask & (1 << hit.collider.gameObject.layer)) != 0) {
                        //direction[i] = Vector3.Lerp(-hit.normal, direction[i], 0.8f).normalized;
                        //direction[i] = -hit.normal;
                        p = hit.point + direction[i] * 0.1f;
                        nois *= noiseTransMultiplier;
                        slow[i]++;
                        if(slow[i] * transparentSlowRate > source.Speed - transparentMinSpeed)
                            slow[i]--;
                    }

                    // Check if hit highlight
                    if((HighlightMask & (1 << hit.collider.gameObject.layer)) != 0) {
                        highlight[i] = p;
                        if(hit.collider != null)
                            nois *= noiseHighlightMultiplier;
                    } 
                }

                position[i] = p;
                line.SetPosition(i, p + Random.insideUnitCircle.normalized * nois);
            }

            // Smoothing
            for(int j = 0; j < 6; j++) {
                for(int i = 2; i < source.Rays - 2; i++) {
                    var dir = (position[i] - origin).normalized;
                    var dist0 = Vector3.Distance(origin, position[i - 2]);
                    var dist1 = Vector3.Distance(origin, position[i - 1]);
                    var dist2 = Vector3.Distance(origin, position[i]);
                    var dist3 = Vector3.Distance(origin, position[i + 1]);
                    var dist4 = Vector3.Distance(origin, position[i + 2]);
                    position[i] = origin + dir * ((dist0 + dist1 + dist2 + dist3 + dist4) / 5);
                }
            }

            // Hightlight lines
            var n = 0;
            LineRenderer cline = null;
            for(int i = 0; i < source.Rays; i++) {
                if(highlight[i].HasValue) {
                    if(cline == null) {
                        var go = new GameObject("Hightlight");
                        go.transform.parent = transform;
                        cline = go.AddComponent<LineRenderer>();
                        cline.useWorldSpace = true;
                        cline.shadowCastingMode = ShadowCastingMode.Off;
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

            // Color
            var color = Color.LerpUnclamped(colorStart, colorEnd, colorCurve.Evaluate(1 - t));
            line.SetColors(color, color);

            // Finish early
            if(h == 0 && !playedNoHitSound)
            {
                SoundSystem.Play("sonar no hit", 1, noHitVolume);
                SoundSystem.Stop("sonar noise");
                playedNoHitSound = true;
            }

            // Sleep
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            t -= Time.fixedDeltaTime * 2;
        }
        if(!playedNoHitSound)
            SoundSystem.Play("sonar no hit", 1, noHitVolume);

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
