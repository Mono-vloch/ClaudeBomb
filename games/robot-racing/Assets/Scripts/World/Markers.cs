using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

// The rewards, as things placed on the track (the game-feel pass): the chequered flag on the
// finish, cones against the wall where a crash costs, teal chevrons along the way for each
// step toward the finish and a red one pointing back for each step away, a clock post at
// the pit exit for each moment spent, and an orange sign on the straight for the speed
// bonus. Each shows its number on a plate. A marker on the pit wall's shelf is worth zero;
// dragged onto its place on the track it takes its first value, and a tap turns the number.
// They are made at run time from the kit's own pieces and materials. No physics: picking is
// done by projecting the markers to the screen, dragging by the pointer's ray on the ground.
public partial class Circuit
{
    public class Marker
    {
        public string kind;          // finish, crash, toward, away, moment, bonus
        public float[] options;      // the first is zero: on the shelf
        public int index;            // into options
        public Vector3 home, spot;   // on the shelf; on the track
        public float spotYaw;
        public Transform root;
        public TextMeshPro plate;
        public Action<float> set;
        public bool Placed => index > 0;
        public float Value => options[index];
    }

    readonly List<Marker> markers = new List<Marker>();
    Action markersChanged;
    Marker held, lastPlaced;
    Vector3 heldOffset;
    Vector2 downAt;
    float downTime;
    bool pointerDown;
    public bool MarkersActive { get; private set; }
    public IReadOnlyList<Marker> Markers => markers;

    static readonly Dictionary<string, float[]> Options = new Dictionary<string, float[]>
    {
        ["finish"] = new[] { 0f, 100f, 200f, 50f },
        ["crash"] = new[] { 0f, -100f, -200f, -20f },
        ["toward"] = new[] { 0f, 2f, 5f, 1f },
        ["away"] = new[] { 0f, -2f, -5f, -1f },
        ["moment"] = new[] { 0f, -0.2f, -1f, -3f },
        ["bonus"] = new[] { 0f, 1f, 3f },
    };

    // kinds: which markers this level has. Each is read from and written to the settings.
    public void BuildMarkers(Settings s, IEnumerable<string> kindsIn, Action changed)
    {
        ClearMarkers();
        if (!HasKit) return;
        var kinds = kindsIn.ToList();
        markersChanged = changed;
        var t = track;
        var along = D(t.startHeading); var outward = D(PitSide == 1 ? (t.startHeading + 3) % 4 : (t.startHeading + 1) % 4);
        // The shelf: up the pit lane from the grid, clear of the gantry, past the pit wall.
        var shelf0 = Cell(t.start) + outward * (0.5f + 1.7f * M) + along * 0.9f;
        // The kit's own cones by the pit wall would pass for the crash marker: off while it is here.
        if (kinds.Contains("crash") && kit != null)
            foreach (Transform child in kit) if (child.name.StartsWith("Cone")) child.gameObject.SetActive(false);
        int n = 0;
        foreach (var kind in kinds)
        {
            if (!Options.ContainsKey(kind)) continue;
            var m = new Marker { kind = kind, options = Options[kind] };
            float current = kind switch { "finish" => s.finish, "crash" => s.crash, "toward" => s.toward, "away" => s.away, "moment" => s.moment, _ => s.speedBonus };
            m.index = 0;
            for (int i = 1; i < m.options.Length; i++) if (Mathf.Abs(m.options[i] - current) < 0.01f) m.index = i;
            if (m.index == 0 && Mathf.Abs(current) > 0.01f) m.index = 1; // a value off the dial: the nearest named one
            m.set = kind switch
            {
                "finish" => v => s.finish = v, "crash" => v => s.crash = v, "toward" => v => s.toward = v,
                "away" => v => s.away = v, "moment" => v => s.moment = v, _ => v => s.speedBonus = v,
            };
            m.home = shelf0 + along * (0.55f * n);
            (m.spot, m.spotYaw) = Spot(kind);
            m.root = MakeMarker(m);
            m.plate = Plate(m.root, kind);
            markers.Add(m);
            Settle(m, true);
            Debug.Log($"[Markers] {kind}: home {m.home} spot {m.spot} index {m.index}");
            n++;
        }
        MarkersActive = markers.Count > 0;
        if (markers.Count > 0)
        {
            // A lamp over the shelf: the markers are the thing to see here.
            var lamp = new GameObject("Shelf lamp").AddComponent<Light>();
            lamp.transform.SetParent(transform, false);
            lamp.transform.position = shelf0 + along * (0.55f * (n - 1) / 2f - 0.5f) + outward * 0.7f + Vector3.up * 1.4f; // beside the shelf, not over it
            lamp.type = LightType.Point; lamp.color = new Color(1f, 0.93f, 0.8f); lamp.intensity = 3f; lamp.range = 3.2f; lamp.shadows = LightShadows.None;
            shelfLamp = lamp.gameObject;
        }
    }
    GameObject shelfLamp;

    public void ClearMarkers()
    {
        foreach (var m in markers) if (m.root != null) Destroy(m.root.gameObject);
        markers.Clear();
        if (shelfLamp != null) { Destroy(shelfLamp); shelfLamp = null; }
        held = null; lastPlaced = null;
        MarkersActive = false;
    }

    public void FreezeMarkers(bool frozen) => MarkersActive = !frozen && markers.Count > 0;

    // Where each kind stands once placed.
    (Vector3, float) Spot(string kind)
    {
        var t = track;
        var along = D(t.startHeading);
        var across = Vector3.Cross(Vector3.up, along);
        int pit = PitSide == 1 ? (t.startHeading + 3) % 4 : (t.startHeading + 1) % 4;
        var outward = D(pit);
        // The first straight, and its first bend.
        var straight = new List<Vector2Int>();
        for (var q = t.start; t.Road(q); q += Track.Dir(t.startHeading)) straight.Add(q);
        switch (kind)
        {
            case "finish":
            {
                foreach (var p in AllCells(t))
                {
                    if (!t.Finish(p)) continue;
                    for (int h = 0; h < 4; h++)
                    {
                        var q = p + Track.Dir(h);
                        if (!t.Road(q) || t.Distance(q) != 1) continue;
                        var side = Vector3.Cross(Vector3.up, D(h));
                        return (Cell(p) + D(h) * 0.5f + side * 0.62f, Yaw(D(h)) + 90f);
                    }
                    return (Cell(p), 0f);
                }
                return (Cell(t.start) + along * 2f, 0f);
            }
            case "crash":
            {
                var c = straight[Mathf.Min(3, straight.Count - 1)];
                return (Cell(c) - outward * 0.34f, Yaw(outward));
            }
            case "toward":
                return (Cell(straight[Mathf.Min(2, straight.Count - 1)]) + along * 0.1f, Yaw(along));
            case "away":
                return (Cell(t.start) + along * 0.7f - across * 0.3f, Yaw(-along));
            case "moment":
                return (Cell(straight[Mathf.Min(2, straight.Count - 1)]) + outward * 0.47f, Yaw(-outward));
            default:
                return (Cell(straight[Mathf.Min(3, straight.Count - 1)]) - outward * 0.47f, Yaw(outward));
        }
    }

    // ---- the pieces ----

    Transform MakeMarker(Marker m)
    {
        var root = new GameObject("Marker " + m.kind).transform;
        root.SetParent(transform, false);
        switch (m.kind)
        {
            case "finish":
            {
                var flag = Instantiate(Piece("FinishFlag"), root, false);
                flag.transform.localScale = Vector3.one * (M * 1.3f);
                flag.name = "MarkerFlag";
                break;
            }
            case "crash":
                for (int i = 0; i < 3; i++)
                {
                    var cone = Instantiate(Piece("Cone"), root, false);
                    cone.transform.localScale = Vector3.one * (M * 1.7f);
                    cone.transform.localPosition = new Vector3((i - 1) * 0.22f, 0f, (i % 2) * 0.08f);
                }
                break;
            case "toward":
                for (int i = 0; i < 3; i++) Chevron(root, new Vector3(0f, 0.008f, i * 0.34f), Theme.Teal);
                break;
            case "away":
                Chevron(root, new Vector3(0f, 0.008f, 0f), Theme.Danger);
                break;
            case "moment":
            {
                Post(root, 0.7f);
                var face = Disc(root, 0.16f, Vector3.up * 0.8f, KitMaterial("Paint", "paint"), Color.white);
                var hand = Box(root, new Vector3(0.016f, 0.1f, 0.004f), new Vector3(0f, 0.84f, -0.016f), KitMaterial("Floodlight", "steel"), Color.black);
                hand.transform.localEulerAngles = new Vector3(0, 0, -30f);
                break;
            }
            default:
            {
                Post(root, 0.66f);
                Box(root, new Vector3(0.36f, 0.28f, 0.02f), Vector3.up * 0.82f, KitMaterial("Cone", "cone"), new Color(1f, 0.62f, 0.12f));
                break;
            }
        }
        return root;
    }

    // The number on a marker: a plate of text above it, turned to the camera.
    TextMeshPro Plate(Transform root, string kind)
    {
        var go = new GameObject("Plate");
        go.transform.SetParent(root, false);
        go.transform.localPosition = Vector3.up * (kind == "finish" ? 1.95f : kind == "crash" ? 0.6f : kind == "toward" || kind == "away" ? 0.42f : 1.15f);
        var t = go.AddComponent<TextMeshPro>();
        t.font = Theme.Bold; t.fontSize = 3.4f; t.color = Color.white; t.alignment = TextAlignmentOptions.Center;
        t.rectTransform.sizeDelta = new Vector2(1.2f, 0.4f);
        t.outlineWidth = 0.25f; t.outlineColor = new Color32(11, 21, 54, 255);
        t.text = "";
        return t;
    }

    void Post(Transform root, float height) => Box(root, new Vector3(0.03f, height, 0.03f), Vector3.up * height / 2f, KitMaterial("Floodlight", "steel"), null);

    GameObject Chevron(Transform parent, Vector3 at, Color tint)
    {
        var mesh = chevron != null ? chevron : (chevron = ChevronMesh());
        var go = new GameObject("Chevron");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = at;
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        var r = go.AddComponent<MeshRenderer>();
        r.sharedMaterial = KitMaterial("Paint", "paint");
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        Tint(r, tint);
        return go;
    }

    static Mesh chevron;
    static Mesh ChevronMesh()
    {
        // A ">" lying flat, pointing +Z: two bars meeting at the tip.
        var v = new List<Vector3>(); var uv = new List<Vector2>(); var tris = new List<int>();
        void Bar(Vector3 a, Vector3 b, float w)
        {
            var side = Vector3.Cross(Vector3.up, (b - a).normalized) * w / 2f;
            int i = v.Count;
            v.Add(a - side); v.Add(b - side); v.Add(b + side); v.Add(a + side);
            uv.Add(new Vector2(0, 0)); uv.Add(new Vector2(0, 1)); uv.Add(new Vector2(1, 1)); uv.Add(new Vector2(1, 0));
            tris.AddRange(new[] { i, i + 1, i + 2, i, i + 2, i + 3 });
        }
        Bar(new Vector3(-0.26f, 0, -0.16f), new Vector3(0f, 0, 0.12f), 0.1f);
        Bar(new Vector3(0.26f, 0, -0.16f), new Vector3(0f, 0, 0.12f), 0.1f);
        var mesh = new Mesh { name = "chevron" };
        mesh.SetVertices(v); mesh.SetUVs(0, uv); mesh.SetTriangles(tris, 0);
        mesh.SetNormals(Enumerable.Repeat(Vector3.up, v.Count).ToList());
        mesh.RecalculateTangents(); mesh.RecalculateBounds();
        return mesh;
    }

    GameObject Disc(Transform parent, float radius, Vector3 at, Material mat, Color? tint)
    {
        var v = new List<Vector3>(); var uv = new List<Vector2>(); var tris = new List<int>();
        const int n = 20;
        v.Add(Vector3.zero); uv.Add(new Vector2(0.5f, 0.5f));
        for (int i = 0; i <= n; i++)
        {
            float a = i / (float)n * Mathf.PI * 2f;
            v.Add(new Vector3(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius, 0f));
            uv.Add(new Vector2(0.5f + Mathf.Cos(a) * 0.5f, 0.5f + Mathf.Sin(a) * 0.5f));
            if (i > 0) tris.AddRange(new[] { 0, i + 1, i });
        }
        var mesh = new Mesh { name = "disc" };
        mesh.SetVertices(v); mesh.SetUVs(0, uv); mesh.SetTriangles(tris, 0);
        mesh.SetNormals(Enumerable.Repeat(Vector3.back, v.Count).ToList());
        mesh.RecalculateTangents(); mesh.RecalculateBounds();
        var go = new GameObject("Disc");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = at;
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        var r = go.AddComponent<MeshRenderer>();
        r.sharedMaterial = mat;
        if (tint.HasValue) Tint(r, tint.Value);
        return go;
    }

    GameObject Box(Transform parent, Vector3 size, Vector3 at, Material mat, Color? tint)
    {
        var go = new GameObject("Box");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = at;
        go.AddComponent<MeshFilter>().sharedMesh = BoxMesh(size);
        var r = go.AddComponent<MeshRenderer>();
        r.sharedMaterial = mat;
        if (tint.HasValue) Tint(r, tint.Value);
        return go;
    }

    static Mesh BoxMesh(Vector3 s)
    {
        var v = new List<Vector3>(); var n = new List<Vector3>(); var uv = new List<Vector2>(); var tris = new List<int>();
        void Face(Vector3 normal, Vector3 right, Vector3 up, float w, float h)
        {
            int i = v.Count;
            var c = normal * 0.5f;
            v.Add(Vector3.Scale(c - right * 0.5f - up * 0.5f, s)); v.Add(Vector3.Scale(c - right * 0.5f + up * 0.5f, s));
            v.Add(Vector3.Scale(c + right * 0.5f + up * 0.5f, s)); v.Add(Vector3.Scale(c + right * 0.5f - up * 0.5f, s));
            for (int k = 0; k < 4; k++) n.Add(normal);
            uv.Add(new Vector2(0, 0)); uv.Add(new Vector2(0, h)); uv.Add(new Vector2(w, h)); uv.Add(new Vector2(w, 0));
            tris.AddRange(new[] { i, i + 1, i + 2, i, i + 2, i + 3 });
        }
        Face(Vector3.forward, Vector3.left, Vector3.up, s.x, s.y); Face(Vector3.back, Vector3.right, Vector3.up, s.x, s.y);
        Face(Vector3.right, Vector3.forward, Vector3.up, s.z, s.y); Face(Vector3.left, Vector3.back, Vector3.up, s.z, s.y);
        Face(Vector3.up, Vector3.right, Vector3.forward, s.x, s.z); Face(Vector3.down, Vector3.right, Vector3.back, s.x, s.z);
        var mesh = new Mesh { name = "box" };
        mesh.SetVertices(v); mesh.SetNormals(n); mesh.SetUVs(0, uv); mesh.SetTriangles(tris, 0);
        mesh.RecalculateTangents(); mesh.RecalculateBounds();
        return mesh;
    }

    static readonly Dictionary<string, Material> kitMaterials = new Dictionary<string, Material>();
    // A material from one of the kit's pieces (URP Lit, with its base and normal maps): the
    // paint of the lines, the steel of the posts, the cone's orange.
    static Material KitMaterial(string piece, string contains)
    {
        string key = piece + ":" + contains;
        if (kitMaterials.TryGetValue(key, out var m) && m != null) return m;
        var prefab = Piece(piece);
        if (prefab != null)
            foreach (var r in prefab.GetComponentsInChildren<Renderer>(true))
                foreach (var mat in r.sharedMaterials)
                    if (mat != null && mat.name.ToLowerInvariant().Contains(contains)) { kitMaterials[key] = mat; return mat; }
        var any = prefab != null ? prefab.GetComponentInChildren<Renderer>(true) : null;
        m = any != null ? any.sharedMaterial : Piece("Barrier").GetComponentInChildren<Renderer>(true).sharedMaterial;
        kitMaterials[key] = m;
        return m;
    }

    static void Tint(Renderer r, Color c)
    {
        var block = new MaterialPropertyBlock();
        r.GetPropertyBlock(block);
        block.SetColor("_BaseColor", c);
        r.SetPropertyBlock(block);
    }

    // ---- placing, by hand or by the walk ----

    static string Number(float v) => Mathf.Abs(v) >= 10f ? v.ToString("+0;-0") : v.ToString("+0.#;-0.#");

    void Settle(Marker m, bool jump)
    {
        var at = m.Placed ? m.spot : m.home;
        // The flag's cloth faces the camera behind the grid, on the shelf and at the finish alike.
        float yaw = m.kind == "finish" ? Yaw(D(track.startHeading)) : m.Placed ? m.spotYaw : Yaw(D(track.startHeading)) + 90f;
        if (jump) { m.root.position = at; m.root.rotation = Quaternion.Euler(0, yaw, 0); }
        else StartCoroutine(Glide(m, at, yaw));
        m.plate.text = m.Placed ? Number(m.Value) : "";
        m.set(m.Value);
    }

    System.Collections.IEnumerator Glide(Marker m, Vector3 to, float yaw)
    {
        var from = m.root.position; var r0 = m.root.rotation; var r1 = Quaternion.Euler(0, yaw, 0);
        float t = 0;
        while (t < 0.25f && m.root != null)
        {
            t += Time.unscaledDeltaTime; float k = Ui.EaseOut(Mathf.Clamp01(t / 0.25f));
            m.root.position = Vector3.Lerp(from, to, k) + Vector3.up * (Mathf.Sin(k * Mathf.PI) * 0.15f);
            m.root.rotation = Quaternion.Slerp(r0, r1, k);
            yield return null;
        }
        if (m.root != null) { m.root.position = to; m.root.rotation = r1; }
    }

    // The walk and a test place a marker by name: its first value, or a given one.
    public void PlaceMarker(string kind, float? value = null)
    {
        var m = markers.FirstOrDefault(x => x.kind == kind);
        if (m == null) return;
        m.index = 1;
        if (value.HasValue) for (int i = 1; i < m.options.Length; i++) if (Mathf.Abs(m.options[i] - value.Value) < 0.01f) m.index = i;
        Settle(m, false);
        Pop(m);
        markersChanged?.Invoke();
    }

    public Marker MarkerOf(string kind) => markers.FirstOrDefault(x => x.kind == kind);

    void Pop(Marker m) { if (m.root != null) StartCoroutine(DoPunch(m.root)); }

    // A marker that wants attention (the flag after a run with no finish): it pulses.
    public void Beckon(string kind, bool on)
    {
        var m = MarkerOf(kind);
        if (m == null) return;
        if (beckon != null) { StopCoroutine(beckon); beckon = null; if (m.root != null) m.root.localScale = Vector3.one; }
        if (on) beckon = StartCoroutine(DoBeckon(m));
    }
    Coroutine beckon;
    System.Collections.IEnumerator DoBeckon(Marker m)
    {
        var light = new GameObject("Beckon").AddComponent<Light>();
        light.transform.SetParent(m.root, false); light.transform.localPosition = Vector3.up * 0.5f;
        light.type = LightType.Point; light.color = Theme.Teal; light.range = 1.6f; light.shadows = LightShadows.None;
        while (m.root != null && !m.Placed)
        {
            float k = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 4f);
            m.root.localScale = Vector3.one * (1f + 0.08f * k);
            light.intensity = 1.5f + 2.5f * k;
            yield return null;
        }
        if (light != null) Destroy(light.gameObject);
        if (m.root != null) m.root.localScale = Vector3.one;
    }

    // Every frame while a pit lane is up: the pointer picks, drags, drops and taps.
    void UpdateMarkers()
    {
        if (!MarkersActive || cam == null) return;
        foreach (var m in markers) if (m.plate != null) m.plate.transform.rotation = Quaternion.LookRotation(m.plate.transform.position - cam.transform.position);
        bool overUi = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        Vector2 p = Input.mousePosition;
        if (Input.GetMouseButtonDown(0) && !overUi)
        {
            held = Pick(p);
            pointerDown = held != null;
            downAt = p; downTime = Time.unscaledTime;
            if (held != null)
            {
                GroundHit(p, out var g);
                heldOffset = held.root.position - g;
                heldOffset.y = 0;
            }
        }
        if (pointerDown && held != null && Input.GetMouseButton(0))
        {
            if ((p - downAt).magnitude > 12f && GroundHit(p, out var g))
            {
                var at = g + heldOffset; at.y = 0.12f;
                held.root.position = at;
                held.plate.text = Number(held.options[Mathf.Max(1, held.index)]);
            }
        }
        if (pointerDown && Input.GetMouseButtonUp(0))
        {
            pointerDown = false;
            if (held == null) return;
            bool tap = (p - downAt).magnitude <= 12f && Time.unscaledTime - downTime < 0.6f;
            if (tap)
            {
                // A tap on a placed marker turns its number; on a shelf marker, places it.
                held.index = held.Placed ? 1 + (held.index % (held.options.Length - 1)) : 1;
                Settle(held, false);
                Sfx.Tick();
                Pop(held);
            }
            else
            {
                var flat = held.root.position; flat.y = 0;
                bool near = (flat - held.spot).magnitude < 0.9f;
                bool wasPlaced = held.Placed;
                held.index = near ? Mathf.Max(1, held.index) : 0;
                Settle(held, false);
                if (near && !wasPlaced) { Sfx.Chime(); Pop(held); }
                else if (!near && wasPlaced) Sfx.Tick();
            }
            lastPlaced = held;
            held = null;
            markersChanged?.Invoke();
        }
    }

    Marker Pick(Vector2 screen)
    {
        Marker best = null; float bestD = 70f;
        foreach (var m in markers)
        {
            var s = cam.ToScreen(m.root.position + Vector3.up * 0.35f);
            if (s.z < 0) continue;
            float d = Vector2.Distance(new Vector2(s.x, s.y), screen);
            if (d < bestD) { bestD = d; best = m; }
        }
        return best;
    }

    // The pointer's ray down to the ground plane.
    public bool GroundHit(Vector2 screen, out Vector3 hit)
    {
        var ray = cam.GetComponent<Camera>().ScreenPointToRay(screen);
        hit = Vector3.zero;
        if (Mathf.Abs(ray.direction.y) < 1e-4f) return false;
        float t = -ray.origin.y / ray.direction.y;
        if (t < 0) return false;
        hit = ray.origin + ray.direction * t;
        return true;
    }

    public const float Metre = M;

    void Update() => UpdateMarkers();
}
