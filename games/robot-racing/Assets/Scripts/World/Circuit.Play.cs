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
        // Low and close; from higher up when the track cards are out, so the gantry does not hide them.
        bool cards = markers.Any(m => m.card), parts = markers.Any(m => m.attached) && !markers.Any(m => m.placeable);
        cam.pitch = cards ? 38f : parts ? 12f : 22f; // lower still for the parts, so the gantry does not hide their names
        cam.yaw = Yaw(D(track.startHeading)) + 24f;
        cam.fov = 32f;
        var pts = new System.Collections.Generic.List<Vector3>();
        if (carRoot != null) pts.Add(carRoot.position);
        float near = 7.5f;
        foreach (var m in markers)
        {
            if (m.parent != null) continue;
            if (m.attached) { if (carRoot != null) { pts.Add(carRoot.position + carRoot.forward * 0.6f); pts.Add(carRoot.position - carRoot.forward * 0.6f); } continue; }
            pts.Add(m.home); if (m.kind != "finish" || (m.spot - Cell(track.start)).magnitude < near) pts.Add(m.spot);
        }
        pts.AddRange(also);
        if (pts.Count < 2)
        {
            // Nothing placed yet: the grid and its first straight.
            var along = D(track.startHeading);
            pts.Add(Cell(track.start) - along * 0.5f); pts.Add(Cell(track.start) + along * 3.5f);
        }
        var b = new Bounds(pts[0], Vector3.zero);
        foreach (var p in pts) b.Encapsulate(p);
        b.Expand(new Vector3(0.6f, 0f, 0.6f));
        b.center = new Vector3(b.center.x, 0.9f, b.center.z); b.size = new Vector3(b.size.x, 2.0f, b.size.z); // headroom for the plates
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

    // A flash of light at a point: the sign that was tapped.
    public void FlashAt(Vector3 at, bool right)
    {
        if (effects != null) { if (right) effects.Confetti(at, 20); else effects.Sparks(at, 0.4f); }
        StartCoroutine(FlashLight(at, right ? Theme.Teal : Theme.Danger));
    }

    // Rings of light on the ground under things that can be tapped: the quiz's signs.
    readonly System.Collections.Generic.List<GameObject> rings = new System.Collections.Generic.List<GameObject>();
    Coroutine ringPulse;
    public void RingSigns(System.Collections.Generic.IEnumerable<Transform> signs)
    {
        foreach (var r in rings) if (r != null) Destroy(r);
        rings.Clear();
        if (ringPulse != null) { StopCoroutine(ringPulse); ringPulse = null; }
        if (signs == null) return;
        foreach (var sign in signs)
        {
            if (sign == null) continue;
            var ring = Disc(transform, 0.5f, Vector3.zero, KitMaterial("Paint", "paint"), Theme.Teal);
            ring.name = "Ring";
            ring.transform.position = sign.position + Vector3.up * 0.012f;
            ring.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            ring.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            var lamp = new GameObject("Ring lamp").AddComponent<Light>();
            lamp.transform.SetParent(ring.transform, false); lamp.transform.localPosition = new Vector3(0f, 0f, -0.6f);
            lamp.type = LightType.Point; lamp.color = Theme.Teal; lamp.range = 1.4f; lamp.intensity = 2f; lamp.shadows = LightShadows.None;
            rings.Add(ring);
        }
        if (rings.Count > 0) ringPulse = StartCoroutine(PulseRings());
    }

    IEnumerator PulseRings()
    {
        while (true)
        {
            float k = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 3f);
            foreach (var r in rings)
            {
                if (r == null) continue;
                r.transform.localScale = Vector3.one * (0.9f + 0.15f * k);
                var l = r.GetComponentInChildren<Light>(); if (l != null) l.intensity = 1f + 2.5f * k;
            }
            yield return null;
        }
    }

    // A board on two posts, by the pit lane, with a few lines of text: a question too long
    // for the objective line, the situation from the rule book.
    public Transform Board(string text, Vector3 at)
    {
        var root = new GameObject("Board").transform;
        root.SetParent(transform, false);
        root.position = at;
        var toCam = cam.transform.position - at; toCam.y = 0f;
        root.rotation = Quaternion.LookRotation(-toCam.normalized);
        var steel = KitMaterial("Floodlight", "steel");
        Box(root, new Vector3(0.04f, 1.2f, 0.04f), new Vector3(-1.1f, 0.6f, 0f), steel, null);
        Box(root, new Vector3(0.04f, 1.2f, 0.04f), new Vector3(1.1f, 0.6f, 0f), steel, null);
        Box(root, new Vector3(2.5f, 0.9f, 0.03f), new Vector3(0f, 1.5f, 0f), KitMaterial("Barrier", "barrier"), new Color(0.06f, 0.09f, 0.18f));
        var go = new GameObject("Text");
        go.transform.SetParent(root, false);
        go.transform.localPosition = new Vector3(0f, 1.5f, -0.02f);
        var t = go.AddComponent<TextMeshPro>();
        t.font = Theme.Bold; t.fontSize = 1.7f; t.color = Color.white; t.alignment = TextAlignmentOptions.Center;
        t.rectTransform.sizeDelta = new Vector2(2.36f, 0.86f);
        t.enableWordWrapping = true;
        t.text = text;
        var lamp = new GameObject("Lamp").AddComponent<Light>();
        lamp.transform.SetParent(root, false); lamp.transform.localPosition = new Vector3(0f, 2.0f, -0.9f);
        lamp.type = LightType.Point; lamp.color = new Color(1f, 0.93f, 0.8f); lamp.intensity = 3f; lamp.range = 3.4f; lamp.shadows = LightShadows.None;
        return root;
    }

    // ---- the curve, in the world ----

    // The points per attempt as a bar chart on a board by the grid, for the levels whose
    // lesson is the curve (the user's rule: a number that is the lesson stays in the world).
    Transform curveBoard;
    readonly System.Collections.Generic.List<Transform> curveBars = new System.Collections.Generic.List<Transform>();
    TextMeshPro curveTitle;
    public void CurveBoard(bool on)
    {
        if (!on) { if (curveBoard != null) Destroy(curveBoard.gameObject); curveBoard = null; curveBars.Clear(); return; }
        if (curveBoard != null || !HasKit) return;
        var t = track;
        var along = D(t.startHeading);
        int pit = PitSide == 1 ? (t.startHeading + 3) % 4 : (t.startHeading + 1) % 4;
        var outward = D(pit);
        curveBoard = new GameObject("Curve board").transform;
        curveBoard.SetParent(ground, false);
        curveBoard.position = Cell(t.start) + along * 2.4f + outward * (0.5f + 1.4f * M);
        curveBoard.rotation = Quaternion.Euler(0f, Yaw(outward), 0f);
        curveBoard.gameObject.AddComponent<Billboard>().cam = cam.transform; // it turns to the camera
        var steel = KitMaterial("Floodlight", "steel");
        Box(curveBoard, new Vector3(0.04f, 1.0f, 0.04f), new Vector3(-1.0f, 0.5f, 0.03f), steel, null);
        Box(curveBoard, new Vector3(0.04f, 1.0f, 0.04f), new Vector3(1.0f, 0.5f, 0.03f), steel, null);
        Box(curveBoard, new Vector3(2.2f, 1.1f, 0.03f), new Vector3(0f, 1.25f, 0f), KitMaterial("Barrier", "barrier"), new Color(0.06f, 0.09f, 0.18f));
        var go = new GameObject("Title");
        go.transform.SetParent(curveBoard, false);
        go.transform.localPosition = new Vector3(0f, 1.68f, -0.02f);
        curveTitle = go.AddComponent<TextMeshPro>();
        curveTitle.font = Theme.Bold; curveTitle.fontSize = 1.3f; curveTitle.color = Theme.Teal; curveTitle.alignment = TextAlignmentOptions.Center;
        curveTitle.rectTransform.sizeDelta = new Vector2(2.1f, 0.24f);
        curveTitle.text = "POINTS PER ATTEMPT";
        var lamp = new GameObject("Lamp").AddComponent<Light>();
        lamp.transform.SetParent(curveBoard, false); lamp.transform.localPosition = new Vector3(0f, 1.9f, -1.2f);
        lamp.type = LightType.Point; lamp.color = new Color(1f, 0.93f, 0.8f); lamp.intensity = 3f; lamp.range = 3.6f; lamp.shadows = LightShadows.None;
    }

    // The bars so far: one per bucket of attempts, the height the mean points of the bucket.
    public void CurveDraw(System.Collections.Generic.IReadOnlyList<float> points, float top)
    {
        if (curveBoard == null) return;
        foreach (var b in curveBars) if (b != null) Destroy(b.gameObject);
        curveBars.Clear();
        if (points.Count == 0) return;
        const int bars = 30;
        int per = Mathf.Max(1, Mathf.CeilToInt(points.Count / (float)bars));
        int n = Mathf.CeilToInt(points.Count / (float)per);
        float w = 2.0f / bars, x0 = -1.0f + w / 2f;
        float lo = Mathf.Min(0f, points.Min()), hi = Mathf.Max(top, points.Max());
        for (int i = 0; i < n; i++)
        {
            float mean = 0; int c = 0;
            for (int j = i * per; j < Mathf.Min(points.Count, (i + 1) * per); j++) { mean += points[j]; c++; }
            mean /= Mathf.Max(1, c);
            float h = Mathf.Max(0.012f, (mean - lo) / Mathf.Max(1f, hi - lo) * 0.84f);
            var bar = Box(curveBoard, new Vector3(w * 0.8f, h, 0.02f), new Vector3(x0 + i * w, 0.74f + h / 2f, -0.025f), KitMaterial("Paint", "paint"), mean >= top * 0.8f ? Theme.Teal : new Color(0.45f, 0.6f, 0.72f));
            curveBars.Add(bar.transform);
        }
    }

    // ---- the points, from the car ----

    // A number that rises from the car's roof, in the world, on a thin teal thread, so it
    // reads as points coming off the car and not as a label on the glass.
    public void PointsFloat(float reward, Color colour)
    {
        if (carRoot == null || floatsOut > 5) return;
        StartCoroutine(DoPointsFloat(reward, colour));
    }
    int floatsOut;
    IEnumerator DoPointsFloat(float reward, Color colour)
    {
        floatsOut++;
        var from = carRoot.position + Vector3.up * 0.42f;
        var label = WorldLabel(reward.ToString(Mathf.Abs(reward) >= 10 ? "+0;-0" : "+0.#;-0.#"), from, Mathf.Abs(reward) >= 10 ? 4.2f : 3.2f, colour);
        var thread = Box(transform, new Vector3(0.012f, 1f, 0.012f), Vector3.zero, KitMaterial("Paint", "paint"), colour);
        thread.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        float t = 0, life = 0.85f, rise = 0.7f;
        while (t < life && label != null)
        {
            t += Time.unscaledDeltaTime;
            float k = t / life, e = 1f - (1f - k) * (1f - k);
            var top = from + Vector3.up * (rise * e);
            label.transform.position = top + Vector3.up * 0.08f;
            float a = 1f - Mathf.Clamp01((k - 0.55f) / 0.45f);
            label.color = new Color(colour.r, colour.g, colour.b, a);
            label.transform.localScale = Vector3.one * (0.6f + 0.4f * Mathf.Min(1f, k * 4f));
            thread.transform.position = (from + top) / 2f;
            thread.transform.localScale = new Vector3(1f, Mathf.Max(0.01f, (top - from).magnitude), 1f);
            Tint(thread.GetComponent<MeshRenderer>(), new Color(colour.r, colour.g, colour.b, 1f) * (0.3f + 0.7f * a));
            yield return null;
        }
        if (label != null) Destroy(label.gameObject);
        if (thread != null) Destroy(thread);
        floatsOut--;
    }

    // ---- the podium ----

    // The debrief's podium beside the car where the race ended: three steps, and the medal
    // rising on the step it earned (gold on 1, silver on 2, bronze on 3), its three stars
    // along the ribbon lit as earned. No medal: an empty podium.
    Transform podium;
    public Transform Podium(int medal, Color colour)
    {
        if (podium != null) Destroy(podium.gameObject);
        if (carRoot == null) return null;
        podium = new GameObject("Podium").transform;
        podium.SetParent(transform, false);
        var side = -carRoot.right;
        podium.position = carRoot.position + side * 1.35f + carRoot.forward * 0.15f;
        // Its front toward where the debrief's camera stands: behind the car, on the podium's side.
        var toCam = (-carRoot.forward * 1.0f + side * 0.9f).normalized;
        podium.rotation = Quaternion.LookRotation(-toCam);
        var barrier = KitMaterial("Barrier", "barrier");
        var steel = KitMaterial("Floodlight", "steel");
        var tops = new float[3];
        float[] heights = { 0.42f, 0.3f, 0.2f };
        float[] xs = { 0f, -0.44f, 0.44f };
        var faceDark = new Color(0.1f, 0.14f, 0.26f);
        for (int i = 0; i < 3; i++)
        {
            Box(podium, new Vector3(0.42f, heights[i], 0.42f), new Vector3(xs[i], heights[i] / 2f, 0f), barrier, faceDark);
            Box(podium, new Vector3(0.42f, 0.02f, 0.42f), new Vector3(xs[i], heights[i] + 0.01f, 0f), steel, new Color(0.7f, 0.74f, 0.8f));
            var num = new GameObject("Step").AddComponent<TextMeshPro>();
            num.transform.SetParent(podium, false);
            num.transform.localPosition = new Vector3(xs[i], heights[i] * 0.55f, -0.215f);
            num.font = Theme.Bold; num.fontSize = 2.2f; num.color = new Color(0.8f, 0.84f, 0.9f); num.alignment = TextAlignmentOptions.Center;
            num.rectTransform.sizeDelta = new Vector2(0.4f, 0.3f);
            num.text = (i + 1).ToString();
            tops[i] = heights[i];
        }
        if (medal > 0)
        {
            int step = 3 - medal; // gold on the first step
            var at = new Vector3(xs[step], tops[step], 0f);
            var m = new GameObject("Medal").transform;
            m.SetParent(podium, false);
            m.localPosition = at;
            // The ribbon post, the disc, the rim, and the stars on the ribbon.
            Box(m, new Vector3(0.1f, 0.5f, 0.012f), new Vector3(0f, 0.25f, 0f), KitMaterial("Paint", "paint"), new Color(0.16f, 0.2f, 0.42f));
            var rim = Disc(m, 0.19f, new Vector3(0f, 0.5f, -0.012f), steel, colour * 0.75f);
            var disc = Disc(m, 0.16f, new Vector3(0f, 0.5f, -0.02f), steel, colour);
            for (int i = 0; i < 3; i++)
            {
                float a = (i - 1) * 0.6f;
                var star = Star(m, 0.05f, new Vector3(Mathf.Sin(a) * 0.25f, 0.5f + Mathf.Cos(a) * 0.25f, -0.025f), i < medal ? Theme.Amber : new Color(0.3f, 0.33f, 0.4f));
                star.transform.localScale = Vector3.zero;
                star.name = "Star" + i;
            }
            var lamp = new GameObject("Medal lamp").AddComponent<Light>();
            lamp.transform.SetParent(m, false); lamp.transform.localPosition = new Vector3(0f, 0.55f, -0.5f);
            lamp.type = LightType.Point; lamp.color = colour; lamp.intensity = 2.5f; lamp.range = 1.8f; lamp.shadows = LightShadows.None;
            m.localScale = new Vector3(1f, 0f, 1f); // it rises
        }
        return podium;
    }

    // The medal rises out of its step, then the stars pop one by one.
    public IEnumerator RaisePodium(int medal)
    {
        if (podium == null) yield break;
        var m = podium.Find("Medal");
        if (m == null) yield break;
        float t = 0;
        while (t < 0.6f && m != null) { t += Time.unscaledDeltaTime; float k = Mathf.Clamp01(t / 0.6f); k = 1f - (1f - k) * (1f - k); m.localScale = new Vector3(1f, k, 1f); yield return null; }
        for (int i = 0; i < 3 && m != null; i++)
        {
            var star = m.Find("Star" + i);
            if (star == null) continue;
            if (i < medal) Sfx.Chime();
            float u = 0;
            while (u < 0.22f && star != null) { u += Time.unscaledDeltaTime; star.localScale = Vector3.one * Mathf.LerpUnclamped(0f, 1f, Ui.EaseOutBack(Mathf.Clamp01(u / 0.22f))); yield return null; }
            if (star != null) star.localScale = Vector3.one;
            if (i < medal && medal >= 2) Celebrate();
            yield return new WaitForSecondsRealtime(0.1f);
        }
    }

    // The debrief's view: the car and the podium beside it, from the podium's front quarter,
    // swaying a little.
    public void ShowPodiumView(RectTransform frame)
    {
        this.frame = frame; Canvas.ForceUpdateCanvases();
        if (carRoot == null) return;
        if (rig != null) rig.Release();
        var centre = carRoot.position + (podium != null ? (podium.position - carRoot.position) * 0.5f : Vector3.zero) + Vector3.up * 0.5f;
        cam.orbitDistance = 3.4f; cam.orbitPitch = 12f; cam.orbitSpeed = 3f; cam.orbitFov = 40f;
        var from = (-carRoot.forward * 1.0f - carRoot.right * 0.9f).normalized;
        cam.orbitArc = 8f; cam.orbitBase = Yaw(from) + 180f;
        cam.Orbit(frame, centre);
        cam.Snap();
    }

    GameObject Star(Transform parent, float r, Vector3 at, Color tint)
    {
        var v = new System.Collections.Generic.List<Vector3>(); var uv = new System.Collections.Generic.List<Vector2>(); var tris = new System.Collections.Generic.List<int>();
        v.Add(Vector3.zero); uv.Add(new Vector2(0.5f, 0.5f));
        for (int i = 0; i < 10; i++)
        {
            float a = Mathf.PI / 2f + i * Mathf.PI / 5f;
            float rr = i % 2 == 0 ? r : r * 0.45f;
            v.Add(new Vector3(Mathf.Cos(a) * rr, Mathf.Sin(a) * rr, 0f)); uv.Add(new Vector2(0.5f, 0.5f));
        }
        for (int i = 1; i <= 10; i++) { int j = i % 10 + 1; tris.AddRange(new[] { 0, j, i }); }
        var mesh = new Mesh { name = "star" };
        mesh.SetVertices(v); mesh.SetUVs(0, uv); mesh.SetTriangles(tris, 0);
        mesh.SetNormals(System.Linq.Enumerable.Repeat(Vector3.back, v.Count).ToList());
        mesh.RecalculateBounds();
        var go = new GameObject("Star");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = at;
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        var mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterial = KitMaterial("Paint", "paint");
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        Tint(mr, tint);
        return go;
    }

    // A signpost: one post by the grid with the answers stacked as plates, the nearest the
    // camera. Each plate glows teal behind its edge while it can be tapped. Returns the
    // plates, top first, in the options' order.
    public System.Collections.Generic.List<Transform> Signpost(string[] options, Vector3 at)
    {
        var plates = new System.Collections.Generic.List<Transform>();
        var root = new GameObject("Signpost").transform;
        root.SetParent(transform, false);
        root.position = at;
        var toCam = cam.transform.position - at; toCam.y = 0f;
        root.rotation = Quaternion.LookRotation(-toCam.normalized);
        var steel = KitMaterial("Floodlight", "steel");
        int n = options.Length;
        bool wide = options.Any(o => o.Length > 22);
        float w = wide ? 2.4f : 1.9f, h = wide ? 0.52f : 0.42f, gap = 0.08f;
        float top = 0.55f + n * (h + gap);
        Box(root, new Vector3(0.05f, top, 0.05f), new Vector3(0f, top / 2f, 0.03f), steel, null);
        for (int i = 0; i < n; i++)
        {
            float y = top - (i + 0.5f) * (h + gap);
            var plate = new GameObject("Plate " + i).transform;
            plate.SetParent(root, false);
            plate.localPosition = new Vector3(0f, y, 0f);
            var halo = Box(plate, new Vector3(w + 0.08f, h + 0.08f, 0.02f), new Vector3(0f, 0f, 0.015f), KitMaterial("Paint", "paint"), Theme.Teal);
            halo.name = "Halo";
            halo.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            Box(plate, new Vector3(w, h, 0.03f), Vector3.zero, KitMaterial("Barrier", "barrier"), new Color(0.06f, 0.09f, 0.18f));
            var go = new GameObject("Text");
            go.transform.SetParent(plate, false);
            go.transform.localPosition = new Vector3(0f, 0f, -0.02f);
            var t = go.AddComponent<TextMeshPro>();
            t.font = Theme.Bold; t.fontSize = wide ? 1.35f : 1.7f; t.color = Color.white; t.alignment = TextAlignmentOptions.Center;
            t.rectTransform.sizeDelta = new Vector2(w - 0.1f, h);
            t.enableWordWrapping = true;
            t.text = options[i];
            plates.Add(plate);
        }
        var lamp = new GameObject("Lamp").AddComponent<Light>();
        lamp.transform.SetParent(root, false); lamp.transform.localPosition = new Vector3(0f, top + 0.2f, -1.0f);
        lamp.type = LightType.Point; lamp.color = new Color(1f, 0.93f, 0.8f); lamp.intensity = 3f; lamp.range = 3.6f; lamp.shadows = LightShadows.None;
        return plates;
    }

    // The plates' halos pulse while a question is up; null stops them.
    readonly System.Collections.Generic.List<Renderer> halos = new System.Collections.Generic.List<Renderer>();
    Coroutine haloPulse;
    public void GlowPlates(System.Collections.Generic.IEnumerable<Transform> plates)
    {
        halos.Clear();
        if (haloPulse != null) { StopCoroutine(haloPulse); haloPulse = null; }
        if (plates == null) return;
        foreach (var p in plates) { if (p == null) continue; var h = p.Find("Halo"); if (h != null) halos.Add(h.GetComponent<Renderer>()); }
        if (halos.Count > 0) haloPulse = StartCoroutine(PulseHalos());
    }

    IEnumerator PulseHalos()
    {
        var dim = new Color(0.05f, 0.35f, 0.3f);
        while (true)
        {
            float k = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 3f);
            foreach (var r in halos) if (r != null) Tint(r, Color.Lerp(dim, Theme.Teal, k));
            yield return null;
        }
    }

    // The quiz's view of a signpost: the post, the plates and the car at its grid.
    public void ShowSignpost(RectTransform frame, Vector3 post, float top, Vector3? board = null)
    {
        this.frame = frame; Canvas.ForceUpdateCanvases();
        if (rig != null) rig.Release();
        var along = D(track.startHeading);
        cam.pitch = 12f; cam.yaw = Yaw(along) + 30f; cam.fov = 34f;
        var b = new Bounds(post + Vector3.up * (top / 2f), new Vector3(2.6f, top + 0.3f, 1.0f));
        if (carRoot != null) b.Encapsulate(carRoot.position + Vector3.up * 0.2f);
        if (board.HasValue) { b.Encapsulate(board.Value); b.Encapsulate(board.Value + Vector3.up * 0.6f); }
        cam.Overview(frame, b);
        cam.Snap();
    }

    public class Billboard : MonoBehaviour
    {
        public Transform cam;
        void LateUpdate() { if (cam != null) transform.rotation = Quaternion.LookRotation(transform.position - cam.position); }
    }
}
