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
    public float PulseSize;
    public float Offset;
    public float MoveRate;
    public float MoveAmount;

    private LineRenderer line;
    private Pillar root;
    private int n;
    private Vector3[] positions;
    private Vector3[] targets;

    private List<GameObject> lines;
    private List<float> times;

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
        positions[0] = transform.position;
        positions[n] = End.position;
        for(int i = 1; i < n; i++) {
            var offset = new Vector3(-dir.y, dir.x) * (1 - 2 * Random.value) * Offset;
            var p = transform.position + dir * SectionLength * i + offset;
            positions[i] = p;
        }
        Smooth();
        targets = positions.Select(a => a).ToArray();
        line.SetPositions(positions);

        StartCoroutine(Pulse());
        StartCoroutine(Move());
    }

    public IEnumerator Pulse() {
        while(root != null) {
            yield return new WaitForSeconds(1 / PulseRate);

            var go = new GameObject("Pulse");
            var pline = go.AddComponent<LineRenderer>();
            pline.useWorldSpace = true;
            pline.shadowCastingMode = ShadowCastingMode.Off;
            pline.material = Material;
            pline.SetColors(PulseColor, PulseColor);
            pline.SetWidth(PulseWidth, PulseWidth);
            pline.SetVertexCount(psecs);
            lines.Add(go);
            times.Add(0);
        }
    }
    public IEnumerator Move() {
        var dir = (End.position - transform.position).normalized;
        while(true) {
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

        for(int i = 1; i < n; i++) {
            positions[i] = Vector3.Lerp(positions[i], targets[i], 0.01f);

            for(int j = 0; j < lines.Count; j++) {
                //var s = "";
                for(int k = 0; k < psecs; k++) {
                    var t = times[j] + k * (PulseSize / psecs);
                    var v = Mathf.FloorToInt(t / SectionLength);
                    var o = (t % SectionLength)/SectionLength;
                    //s += "(" + v + " " + o + ") ";
                    lines[j].GetComponent<LineRenderer>().SetPosition(k, Vector3.LerpUnclamped(positions[v], positions[v + 1], o) + Vector3.back);
                }

                //Debug.Log(s);

                times[j] += Time.deltaTime * PulseSpeed;
            }
            if(times.Count > 0 && times[0] + PulseSize > d) {
                if(End.GetComponent<PillarVein>()) {
                    End.GetComponent<PillarVein>().lines.Add(lines[0]);
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
                End.GetComponent<PillarVein>().PulseSize = PulseSize;
                End.GetComponent<PillarVein>().MoveAmount = MoveAmount;
                End.GetComponent<PillarVein>().MoveRate = MoveRate;
                End.GetComponent<PillarVein>().Offset = Offset;
            }
        }
    }
}
