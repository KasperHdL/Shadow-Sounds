using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Rendering;

public class PillarVein : MonoBehaviour {

    public Transform End;
    public float SectionLength = 2f;
    public Material Material;
    public Color VeinColor;
    public float VeinWidth;
    public Color PulseColor;
    public float PulseWidth;
    public float PulseRate;
    public float PulseSpeed;
    public AnimationCurve PulseSpeedCurve;
    public float PulseSize;
    public float Offset;
    public float MoveRate;
    public float MoveAmount;
    public float DeathSpeed;

    private LineRenderer line;
    private Pillar root;
    private int n;
    private Vector3[] positions;
    private Vector3[] targets;

    private List<GameObject> lines;
    private List<float> times;

    private Dictionary<GameObject, LineRenderer> starts;
    private Dictionary<GameObject, LineRenderer> ends;

    private float life;
    private int psecs = 3;

    public void Start() {
        lines = new List<GameObject>();
        times = new List<float>();
        line = gameObject.AddComponent<LineRenderer>();
        if(root == null)
            root = GetComponent<Pillar>();
        if(End == null) {
            Destroy(this);
            return;
        }

        starts = new Dictionary<GameObject, LineRenderer>();
        ends = new Dictionary<GameObject, LineRenderer>();

        n = Mathf.CeilToInt(Vector3.Distance(transform.position, End.position) / SectionLength);
        var dir = (End.position - transform.position).normalized;
        SectionLength = Vector3.Distance(transform.position, End.position) / n;
        line.material = Material;
        line.SetVertexCount(n + 1);
        line.SetColors(VeinColor, VeinColor);
        line.SetWidth(VeinWidth, VeinWidth);
        line.useWorldSpace = true;
        line.shadowCastingMode = ShadowCastingMode.Off;

        positions = new Vector3[n + 1];
        positions[0] = transform.position + Vector3.back;
        positions[n] = End.position + Vector3.back;
        for(int i = 1; i < n; i++) {
            var offset = new Vector3(-dir.y, dir.x) * (1 - 2 * Random.value) * Offset;
            var p = transform.position + dir * SectionLength * i + offset;
            positions[i] = p + Vector3.back;
        }
        Smooth();
        targets = positions.Select(a => a).ToArray();
        line.SetPositions(positions);

        life = Vector3.Distance(transform.position, End.position);

        StartCoroutine(Pulse());
        StartCoroutine(Move());
    }

    public IEnumerator Pulse() {
        while(root != null && !root.IsDead) {
            yield return new WaitForSeconds(1 / PulseRate);

            var go = new GameObject("Pulse");
            var pline = go.AddComponent<LineRenderer>();
            pline.useWorldSpace = true;
            pline.shadowCastingMode = ShadowCastingMode.Off;
            pline.material = Material;
            pline.SetColors(PulseColor, PulseColor);
            pline.SetWidth(PulseWidth, PulseWidth);
            pline.SetVertexCount(psecs);
            for(int i = 0; i < psecs; i++)
                pline.SetPosition(i, transform.position + Vector3.back);
            lines.Add(go);
            times.Add(0);


            var goStart = new GameObject("Pulse Start");
            var plineStart = goStart.AddComponent<LineRenderer>();
            plineStart.useWorldSpace = true;
            plineStart.shadowCastingMode = ShadowCastingMode.Off;
            plineStart.material = Material;
            plineStart.SetColors(VeinColor, PulseColor);
            plineStart.SetWidth(VeinWidth, PulseWidth);
            plineStart.SetVertexCount(2);
            plineStart.SetPosition(0, transform.position + Vector3.back);
            plineStart.SetPosition(1, transform.position + Vector3.back);
            plineStart.transform.parent = pline.transform;
            starts.Add(go, plineStart);

            var goEnd = new GameObject("Pulse End");
            var plineEnd = goEnd.AddComponent<LineRenderer>();
            plineEnd.useWorldSpace = true;
            plineEnd.shadowCastingMode = ShadowCastingMode.Off;
            plineEnd.material = Material;
            plineEnd.SetColors(PulseColor, VeinColor);
            plineEnd.SetWidth(PulseWidth, VeinWidth);
            plineEnd.SetVertexCount(2);
            plineEnd.SetPosition(0, transform.position + Vector3.back);
            plineEnd.SetPosition(1, transform.position + Vector3.back);
            plineEnd.transform.parent = pline.transform;
            ends.Add(go, plineEnd);
        }
    }
    public IEnumerator Move() {
        var dir = (End.position - transform.position).normalized;
        while(root == null || !root.IsDead) {
            yield return new WaitForSeconds(1 / MoveRate);

            for(int i = 1; i < n; i++) {
                var offset = new Vector3(-dir.y, dir.x) * (1 - 2 * Random.value) * MoveAmount;
                targets[i] = transform.position + dir * SectionLength * i + offset;
            }
        }
    }

    public void Update() {
        if(End == null)
            return;

        var d = Vector3.Distance(End.position, transform.position);
        if(root != null && root.IsDead)
            life -= Time.deltaTime * DeathSpeed;

        if(life < 0) {
            if(End.GetComponent<PillarVein>() != null)
                End.GetComponent<PillarVein>().root = root;
            foreach(var l in lines)
                Destroy(l);
            Destroy(line);
            Destroy(this);
            return;
        }

        for(int i = 0; i < n; i++) {
            if(root == null || !root.IsDead)
                positions[i] = Vector3.Lerp(positions[i], targets[i], 0.01f);
            var more = positions[i] - End.position;
            var p = End.position + more.normalized * Mathf.Min(more.magnitude, life);
            positions[i] = Vector3.Project(p - positions[i], positions[i] - p) + positions[i];
        }

        for(int j = 0; j < lines.Count; j++)
        {
            var latest = Vector3.zero;

            for(int k = -2; k < psecs + 2; k++) {
                var aline = k < 0
                    ? starts[lines[j]]
                    : (k >= psecs
                        ? ends[lines[j]]
                        : lines[j].GetComponent<LineRenderer>());

                var t = times[j] + (k < 0 ? k + 1 : (k >= psecs ? k - 1 : k)) * (PulseSize / psecs);
                var v = Mathf.Max(Mathf.Min(Mathf.FloorToInt(t / SectionLength), positions.Length - 2), 0);
                var o = (t % SectionLength) / SectionLength;
                
                aline.SetPosition(k < 0 ? k + 2 : (k >= psecs ? k - psecs : k), Vector3.LerpUnclamped(positions[v], positions[v + 1], o) + Vector3.back);
                latest = Vector3.LerpUnclamped(positions[v], positions[v + 1], o);
            }

            times[j] += Time.deltaTime*PulseSpeed * PulseSpeedCurve.Evaluate(Time.time / PulseRate + Vector3.Distance(latest, transform.position) * 0.5f);
        }
        if(times.Count > 0 && times[0] + PulseSize > d) {
            if(End.GetComponent<PillarVein>()) {
                End.GetComponent<PillarVein>().lines.Add(lines[0]);
                End.GetComponent<PillarVein>().starts.Add(lines[0], starts[lines[0]]);
                End.GetComponent<PillarVein>().ends.Add(lines[0], ends[lines[0]]);
                End.GetComponent<PillarVein>().times.Add(0);
                lines.RemoveAt(0);
                times.RemoveAt(0);
            } else {
                var l = lines[0];
                lines.RemoveAt(0);
                times.RemoveAt(0);
                Destroy(l);
            }
        }

        Smooth();
        line.SetPositions(positions);
    }

    private void Smooth() {
        // Smoothing
        for(int j = 0; j < 3; j++) {
            for(int i = 2; i < positions.Length - 2; i++) {
                var dir = (positions[i] - transform.position).normalized;
                var dist0 = Vector3.Distance(transform.position, positions[i - 2]);
                var dist1 = Vector3.Distance(transform.position, positions[i - 1]);
                var dist2 = Vector3.Distance(transform.position, positions[i]);
                var dist3 = Vector3.Distance(transform.position, positions[i + 1]);
                var dist4 = Vector3.Distance(transform.position, positions[i + 2]);
                positions[i] = transform.position + dir * ((dist0 + dist1 + dist2 + dist3 + dist4) / 5);
            }
        }
    }

    public void OnDrawGizmos() {
        if(End != null) {
            Gizmos.color = VeinColor;
            Gizmos.DrawLine(transform.position, End.position);

            if(End.GetComponent<PillarVein>()) {
                End.GetComponent<PillarVein>().Material = Material;
                End.GetComponent<PillarVein>().SectionLength = SectionLength;
                End.GetComponent<PillarVein>().VeinColor = VeinColor;
                End.GetComponent<PillarVein>().VeinWidth = VeinWidth;
                End.GetComponent<PillarVein>().PulseColor = PulseColor;
                End.GetComponent<PillarVein>().PulseWidth = PulseWidth;
                End.GetComponent<PillarVein>().PulseRate = PulseRate;
                End.GetComponent<PillarVein>().PulseSpeed = PulseSpeed;
                End.GetComponent<PillarVein>().PulseSpeedCurve = PulseSpeedCurve;
                End.GetComponent<PillarVein>().PulseSize = PulseSize;
                End.GetComponent<PillarVein>().MoveAmount = MoveAmount;
                End.GetComponent<PillarVein>().MoveRate = MoveRate;
                End.GetComponent<PillarVein>().DeathSpeed = DeathSpeed;
                End.GetComponent<PillarVein>().Offset = Offset;
            }
        }
    }
}
