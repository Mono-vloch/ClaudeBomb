using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

// What the world shows that the HUD used to say (the game-feel pass): the lap board by the
// grid, the start lights under the gantry, the placing view of the pit lane, and the taps on
// the world the debrief's questions are answered with.
public partial class Circuit
{
    Transform lapBoard;
    TextMeshPro lapText;
    Coroutine lapFlash;

    // The pit lane's view: from behind the grid, low, fitted to what the player works with
    // (the car, the shelf, every marker's place), not to the whole track, so the rewards
    // read big. Extra points (the quiz's signs) widen it.
    public void ShowPlacing(RectTransform frame, params Vector3[] also)
    {
        this.frame = frame; Canvas.ForceUpdateCanvases();
        if (rig != null) rig.Release();
        cam.pitch = 30f;
        cam.yaw = Yaw(D(track.startHeading)) + 22f;
        cam.fov = 36f;
        var pts = new System.Collections.Generic.List<Vector3>();
        if (carRoot != null) pts.Add(carRoot.position);
        float near = 7.5f;
        foreach (var m in markers) { pts.Add(m.home); if (m.kind != "finish" || (m.spot - Cell(track.start)).magnitude < near) pts.Add(m.spot); }
        pts.AddRange(also);
        if (pts.Count < 2)
        {
            // Nothing placed yet: the grid and its first straight.
            var along = D(track.startHeading);
            pts.Add(Cell(track.start) - along * 0.5f); pts.Add(Cell(track.start) + along * 3.5f);
        }
        var b = new Bounds(pts[0], Vector3.zero);
        foreach (var p in pts) b.Encapsulate(p);
        b.Expand(new Vector3(1.4f, 0f, 1.4f));
        b.center = new Vector3(b.center.x, 0.9f, b.center.z); b.size = new Vector3(b.size.x, 2.2f, b.size.z);
        cam.Overview(frame, b);
    }

    // ---- the lap board ----

    // A board on two posts beside the grid, on the pit side, that reads the lap: the race's
    // state in the world, not in a panel.
    public void LapBoard(string text)
    {
        if (!HasKit) return;
        if (lapBoard == null)
        {
            lapBoard = new GameObject("Lap board").transform;
            lapBoard.SetParent(ground, false);
            var t = track;
            var along = D(t.startHeading);
            int pit = PitSide == 1 ? (t.startHeading + 3) % 4 : (t.startHeading + 1) % 4;
            var outward = D(pit);
            lapBoard.position = Cell(t.start) + along * 1.1f + outward * (0.5f + 0.62f * M);
            lapBoard.rotation = Quaternion.Euler(0f, Yaw(outward), 0f); // its face (-Z) toward the track
            var steel = KitMaterial("Floodlight", "steel");
            Box(lapBoard, new Vector3(0.03f, 0.9f, 0.03f), new Vector3(-0.3f, 0.45f, 0f), steel, null);
            Box(lapBoard, new Vector3(0.03f, 0.9f, 0.03f), new Vector3(0.3f, 0.45f, 0f), steel, null);
            Box(lapBoard, new Vector3(0.74f, 0.34f, 0.03f), new Vector3(0f, 0.74f, 0f), KitMaterial("Barrier", "barrier"), new Color(0.06f, 0.09f, 0.18f));
            var go = new GameObject("Lap");
            go.transform.SetParent(lapBoard, false);
            go.transform.localPosition = new Vector3(0f, 0.74f, -0.02f);
            lapText = go.AddComponent<TextMeshPro>();
            lapText.font = Theme.Bold; lapText.fontSize = 2.2f; lapText.color = Theme.Teal; lapText.alignment = TextAlignmentOptions.Center;
            lapText.rectTransform.sizeDelta = new Vector2(0.7f, 0.3f);
        }
        lapText.text = text;
    }

    public void LapBoardFlash(Color colour)
    {
        if (lapText == null) return;
        if (lapFlash != null) StopCoroutine(lapFlash);
        lapFlash = StartCoroutine(DoLapFlash(colour));
    }

    IEnumerator DoLapFlash(Color colour)
    {
        for (int i = 0; i < 3; i++)
        {
            if (lapText == null) yield break;
            lapText.color = colour; yield return new WaitForSecondsRealtime(0.12f);
            if (lapText == null) yield break;
            lapText.color = Theme.Teal; yield return new WaitForSecondsRealtime(0.1f);
        }
    }

    // ---- the start lights ----

    // Three red lights under the gantry go out one by one; a teal flash starts the race.
    public IEnumerator StartLights(float perLight)
    {
        var t = track;
        var at = Cell(t.start) - D(t.startHeading) * 0.45f + Vector3.up * 1.35f;
        var across = Vector3.Cross(Vector3.up, D(t.startHeading));
        var lights = new Light[3];
        for (int i = 0; i < 3; i++)
        {
            var go = new GameObject("Start light");
            go.transform.SetParent(transform, false);
            go.transform.position = at + across * ((i - 1) * 0.28f);
            var l = go.AddComponent<Light>();
            l.type = LightType.Point; l.color = Theme.Danger; l.intensity = 4f; l.range = 1.4f; l.shadows = LightShadows.None;
            lights[i] = l;
        }
        for (int i = 0; i < 3; i++)
        {
            Sfx.Sting(Sfx.Mood.Beep);
            yield return new WaitForSecondsRealtime(perLight);
            if (lights[i] != null) lights[i].enabled = false;
        }
        foreach (var l in lights) if (l != null) { l.enabled = true; l.color = Theme.Teal; l.intensity = 6f; }
        yield return new WaitForSecondsRealtime(0.5f);
        foreach (var l in lights) if (l != null) Destroy(l.gameObject);
    }

    // ---- taps on the world (the debrief's questions) ----

    public Vector3 ScreenOf(Vector3 world) => cam.ToScreen(world);

    // Which part of the world a point on the screen is: "wheels", "senses", "car", "track",
    // or null. The small things are tested first.
    public string PartAt(Vector2 screen)
    {
        if (carRoot == null) return null;
        if (carRig != null && carRig.spin != null)
            foreach (var w in carRig.spin) { var s = cam.ToScreen(w.position); if (s.z > 0 && Vector2.Distance(screen, s) < 38f) return "wheels"; }
        for (int i = 0; i < 3; i++)
        {
            if (rays[i] == null) continue;
            var beam = rays[i].GetChild(0);
            var s = cam.ToScreen(beam.position); if (s.z > 0 && Vector2.Distance(screen, s) < 46f) return "senses";
            var tip = cam.ToScreen(beam.position + rays[i].forward * beam.localScale.z * 0.45f); if (tip.z > 0 && Vector2.Distance(screen, tip) < 40f) return "senses";
        }
        var rs = carRoot.GetComponentsInChildren<Renderer>().Where(r => r.enabled && !(r is TrailRenderer) && r.name != "beam").ToArray();
        if (rs.Length > 0)
        {
            var b = rs[0].bounds; foreach (var r in rs) b.Encapsulate(r.bounds);
            float x0 = 1e9f, x1 = -1e9f, y0 = 1e9f, y1 = -1e9f;
            for (int i = 0; i < 8; i++)
            {
                var c = b.center + Vector3.Scale(b.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
                var v = cam.ToScreen(c);
                x0 = Mathf.Min(x0, v.x); x1 = Mathf.Max(x1, v.x); y0 = Mathf.Min(y0, v.y); y1 = Mathf.Max(y1, v.y);
            }
            if (screen.x >= x0 - 10 && screen.x <= x1 + 10 && screen.y >= y0 - 10 && screen.y <= y1 + 10) return "car";
        }
        if (GroundHit(screen, out var g))
        {
            var cell = new Vector2Int(Mathf.RoundToInt(g.x + (track.W - 1) / 2f), Mathf.RoundToInt((track.H - 1) / 2f - g.z));
            if (track.InBounds(cell) && track.Road(cell)) return "track";
        }
        return null;
    }

    // The sensors drawn long and bright for a question about them, then back.
    public void ShowSensors(bool big)
    {
        for (int i = 0; i < 3; i++)
        {
            if (rays[i] == null) continue;
            var beam = rays[i].GetChild(0);
            var s = beam.localScale; s.x = big ? 0.09f : (HasKit ? 0.035f : 0.07f); beam.localScale = s;
        }
    }

    // The five things a question about the parts can mean, each with a soft teal pulse while
    // the question is up: the car, the track ahead, a sensor's ray, a wheel, the points.
    public Vector3 candidatePoints;
    readonly System.Collections.Generic.List<Light> candidateLights = new System.Collections.Generic.List<Light>();
    Coroutine candidatePulse;
    public void LightCandidates(bool on)
    {
        foreach (var l in candidateLights) if (l != null) Destroy(l.gameObject);
        candidateLights.Clear();
        if (candidatePulse != null) { StopCoroutine(candidatePulse); candidatePulse = null; }
        if (!on || carRoot == null) return;
        var spots = new System.Collections.Generic.List<Vector3> { carRoot.position + Vector3.up * 0.35f, Cell(track.start) + D(track.startHeading) * 2.6f + Vector3.up * 0.3f, candidatePoints };
        if (rays[0] != null) spots.Add(rays[0].GetChild(0).position + rays[0].forward * 0.35f + Vector3.up * 0.1f);
        if (carRig != null && carRig.spin != null && carRig.spin.Length > 0) spots.Add(carRig.spin[0].position + Vector3.up * 0.05f);
        foreach (var at in spots)
        {
            var go = new GameObject("Candidate");
            go.transform.SetParent(transform, false);
            go.transform.position = at;
            var l = go.AddComponent<Light>();
            l.type = LightType.Point; l.color = Theme.Teal; l.range = 1.1f; l.intensity = 2f; l.shadows = LightShadows.None;
            candidateLights.Add(l);
        }
        candidatePulse = StartCoroutine(PulseCandidates());
    }

    IEnumerator PulseCandidates()
    {
        while (true)
        {
            float k = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 3f);
            foreach (var l in candidateLights) if (l != null) l.intensity = 1.2f + 2.2f * k;
            yield return null;
        }
    }

    // A teal flash on a part of the world: the right answer, or where the right one was.
    public void FlashPart(string part, bool right)
    {
        var colour = right ? Theme.Teal : Theme.Danger;
        Vector3 at = carRoot != null ? carRoot.position : Vector3.zero;
        if (part == "track") at = Cell(track.start) + D(track.startHeading) * 1.5f;
        else if (part == "senses" && rays[0] != null) at = rays[0].GetChild(0).position + rays[0].forward * 0.3f;
        if (effects != null) { if (right) effects.Confetti(at + Vector3.up * 0.3f, 24); else effects.Sparks(at + Vector3.up * 0.2f, 0.4f); }
        StartCoroutine(FlashLight(at, colour));
    }

    IEnumerator FlashLight(Vector3 at, Color colour)
    {
        var go = new GameObject("Flash");
        go.transform.SetParent(transform, false);
        go.transform.position = at + Vector3.up * 0.5f;
        var l = go.AddComponent<Light>();
        l.type = LightType.Point; l.color = colour; l.range = 2.5f; l.shadows = LightShadows.None;
        float t = 0;
        while (t < 0.6f) { t += Time.unscaledDeltaTime; l.intensity = 6f * (1f - t / 0.6f); yield return null; }
        Destroy(go);
    }

    // A label standing in the world: the quiz's three plates, a floating "+2".
    public TextMeshPro WorldLabel(string text, Vector3 at, float size, Color colour, Transform parent = null)
    {
        var go = new GameObject("Label " + text);
        go.transform.SetParent(parent != null ? parent : transform, false);
        go.transform.position = at;
        var t = go.AddComponent<TextMeshPro>();
        t.font = Theme.Bold; t.fontSize = size; t.color = colour; t.alignment = TextAlignmentOptions.Center;
        t.rectTransform.sizeDelta = new Vector2(2.2f, 0.6f);
        t.outlineWidth = 0.22f; t.outlineColor = new Color32(11, 21, 54, 255);
        t.text = text;
        go.AddComponent<Billboard>().cam = cam.transform;
        return t;
    }

    // A sign on a post, standing in the world, with a line of text: the quiz's answers. It
    // faces the camera, lit by its own small lamp.
    public Transform Sign(string text, Vector3 at)
    {
        var root = new GameObject("Sign").transform;
        root.SetParent(transform, false);
        root.position = at;
        var toCam = cam.transform.position - at; toCam.y = 0f;
        root.rotation = Quaternion.LookRotation(-toCam.normalized);
        var steel = KitMaterial("Floodlight", "steel");
        Box(root, new Vector3(0.04f, 0.7f, 0.04f), new Vector3(0f, 0.35f, 0f), steel, null);
        Box(root, new Vector3(1.5f, 0.62f, 0.03f), new Vector3(0f, 1.0f, 0f), KitMaterial("Barrier", "barrier"), new Color(0.06f, 0.09f, 0.18f));
        var go = new GameObject("Text");
        go.transform.SetParent(root, false);
        go.transform.localPosition = new Vector3(0f, 1.0f, -0.02f);
        var t = go.AddComponent<TextMeshPro>();
        t.font = Theme.Bold; t.fontSize = 2.0f; t.color = Color.white; t.alignment = TextAlignmentOptions.Center;
        t.rectTransform.sizeDelta = new Vector2(1.42f, 0.6f);
        t.enableWordWrapping = true;
        t.text = text;
        var lamp = new GameObject("Lamp").AddComponent<Light>();
        lamp.transform.SetParent(root, false); lamp.transform.localPosition = new Vector3(0f, 1.3f, -0.6f);
        lamp.type = LightType.Point; lamp.color = new Color(1f, 0.93f, 0.8f); lamp.intensity = 2.2f; lamp.range = 2.4f; lamp.shadows = LightShadows.None;
        return root;
    }

    public class Billboard : MonoBehaviour
    {
        public Transform cam;
        void LateUpdate() { if (cam != null) transform.rotation = Quaternion.LookRotation(transform.position - cam.position); }
    }
}
