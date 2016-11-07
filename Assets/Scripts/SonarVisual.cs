using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(SonarTool))]
public class SonarVisual : MonoBehaviour
{
    public Material material;
    public Color colorStart = new Color(1, 1, 1, 1);
    public Color colorEnd = new Color(1, 1, 1, 0);
    public float width = 0.1f;
    public bool overrideTime = true;
    public float time = 0.6f;
    public bool overrideDistance = true;
    public float dist = 2.0f;

    private PlayerMovement player;
    private SonarTool sonar;
    private LineRenderer line;

    private void Start()
    {
        sonar = GetComponent<SonarTool>();
        player = GetComponent<PlayerMovement>();
    }

    private void SonarShoot(float distance)
    {
        StopAllCoroutines();
        StartCoroutine(Shoot(distance));
    }

    private IEnumerator Shoot(float distance)
    {
        var origin = transform.position;
        var viewDir = player.viewDirection.normalized;
        var angle = Mathf.Atan2(-viewDir.y, viewDir.x);
        var coneAngleRad = Mathf.Deg2Rad * sonar.coneAngle;
        var coneIncrementRad = Mathf.Deg2Rad * sonar.coneIncrement;
        var startAngle = angle - coneAngleRad;
        
        line = line == null ? gameObject.AddComponent<LineRenderer>() : GetComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.SetColors(colorStart, colorStart);
        line.SetWidth(width, width);
        line.SetVertexCount(sonar.rays);
        line.material = material;

        if (overrideDistance) distance = dist;
        var tt = overrideTime ? time : sonar.soundDelayPerMeter * distance;
        var t = tt;

        while (t > 0)
        {
            var l = ((tt - t)/tt);
            var d = distance * l;

            for (int i = 0; i < sonar.rays; i++)
            {
                var hit = sonar.blockHits[i];

                var a = startAngle + coneIncrementRad * i;
                var md = hit.collider == null ? d : Vector3.Distance(origin, sonar.blockHits[i].point);
                md = Mathf.Min(md, d);
                line.SetPosition(i, origin + new Vector3(Mathf.Cos(a) * md, -Mathf.Sin(a) * md, 0));
            }

            var color = Color.Lerp(colorStart, colorEnd, l);
            line.SetColors(color, color);

            yield return new WaitForFixedUpdate();
            t -= Time.fixedDeltaTime;
        }

        Destroy(line);
        line = null;
    }
}
