using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// The circuit made of real pieces (the realism pass): the kit that Robot Racing > Build track
// kit makes in Resources/Track, placed here from the track's grid. The asphalt and its painted
// lines are one mesh each, made from the road cells with their UVs in metres so the texture
// runs on without a seam; everything else is a prefab: kerbs at every bend, jersey barriers
// with their hazard band along the straights, tyre walls with their belt where a bend's
// outside is, the start gantry over the grid, the pit wall along the first straight with the
// pit lane, the pit box and the grandstand behind it, the chequered line and its flags,
// floodlights, trees and grass round about, and the robot car.
//
// The kit is in metres and the world's unit is a cell (2 m), so the kit stands at half scale.
// Evening on the circuit: the sun low and warm, the floodlights on, the air hazy blue.
public partial class Circuit
{
    const float M = 0.5f; // one metre of the kit in the world's units
    static GameObject Piece(string name) => Resources.Load<GameObject>("Track/" + name);
    public static bool HasKit => Piece("Road") != null && Piece("Car") != null;

    Transform kit;
    CarRig carRig;
    ReflectionProbe probe;
    public int PitSide { get; private set; } = 1;      // +1: the pit lane is on the car's left on the grid
    readonly HashSet<(Vector2Int, int)> kerbs = new HashSet<(Vector2Int, int)>();

    static Vector3 D(int heading) { var d = Track.Dir(heading); return new Vector3(d.x, 0f, -d.y); }
    static float Yaw(Vector3 v) => Mathf.Atan2(v.x, v.z) * Mathf.Rad2Deg;

    GameObject Put(string name, Vector3 world, float yaw, float scale = 1f)
    {
        var prefab = Piece(name);
        if (prefab == null) return null;
        var go = Instantiate(prefab, kit, false);
        go.transform.localScale = Vector3.one * scale;
        go.transform.SetPositionAndRotation(world, Quaternion.Euler(0f, yaw, 0f));
        return go;
    }

    // ---- light and air ----

    void BuildLightKit()
    {
        var sunGo = new GameObject("Sun");
        sunGo.transform.SetParent(transform, false);
        var sun = sunGo.AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.shadows = LightShadows.Soft;
        sun.shadowStrength = 0.9f;
        sun.shadowBias = 0.04f; sun.shadowNormalBias = 0.3f;
        var camera = cam.GetComponent<Camera>();
        RenderSettings.ambientMode = AmbientMode.Trilight;
        Color top, horizon, bottom, fog;
        if (dusk)
        {
            // The cover: the sun all but gone, the floodlights doing the work.
            sun.transform.rotation = Quaternion.Euler(9f, -62f, 0f); sun.color = Theme.Hex("#FF9F62"); sun.intensity = 1.5f;
            RenderSettings.ambientSkyColor = Theme.Hex("#2A3D66"); RenderSettings.ambientEquatorColor = Theme.Hex("#4B3F5C"); RenderSettings.ambientGroundColor = Theme.Hex("#141B28");
            top = Theme.Hex("#0B1536"); horizon = Theme.Hex("#B4705E"); bottom = Theme.Hex("#141B28"); fog = Theme.Hex("#2E3550");
            RenderSettings.fogDensity = 0.05f;
        }
        else
        {
            // Play: the last of the evening sun across the track, warm, long shadows.
            sun.transform.rotation = Quaternion.Euler(14f, -58f, 0f); sun.color = Theme.Hex("#FFC08A"); sun.intensity = 1.9f;
            RenderSettings.ambientSkyColor = Theme.Hex("#4C6A9C"); RenderSettings.ambientEquatorColor = Theme.Hex("#7C6A78"); RenderSettings.ambientGroundColor = Theme.Hex("#252D3A");
            top = Theme.Hex("#1D3363"); horizon = Theme.Hex("#D9967A"); bottom = Theme.Hex("#252D3A"); fog = Theme.Hex("#56607E");
            RenderSettings.fogDensity = 0.04f;
        }
        // The edge of the world: the grass runs on far past the field (the outer ring in
        // BuildTrackKit) and linear fog in the sky's horizon colour swallows it, starting past
        // the farthest the cameras look at the track (the largest track reaches 9 units from
        // its centre), so the play area keeps its light. Below the horizon the dome is the same
        // haze, so the fogged grass meets the sky with no line. The fog modes are kept in builds
        // by ProjectSettings/GraphicsSettings (fog stripping: custom, linear).
        var haze = Color.Lerp(horizon, fog, 0.35f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = FogStart;
        RenderSettings.fogEndDistance = FogEnd;
        RenderSettings.fogColor = haze;
        camera.backgroundColor = haze;
        Look.SetSky(top, horizon);
        var sky = Instantiate(Piece("Sky"), transform, false);
        sky.transform.localScale = Vector3.one * 300f;
        var dome = sky.GetComponentInChildren<Renderer>();
        var mat = new Material(dome.sharedMaterial);
        mat.SetColor("_Top", top); mat.SetColor("_Horizon", haze); mat.SetColor("_Bottom", haze);
        dome.sharedMaterial = mat;
    }

    // ---- the track ----

    void BuildTrackKit()
    {
        var t = track;
        kit = new GameObject("Kit").transform;
        kit.SetParent(ground, false);
        kit.localScale = Vector3.one * M;
        kerbs.Clear();

        // The grass: one big field, its UVs in units of 4 m, and round it a ring of the same
        // grass out to where the fog has long closed in, so no camera sees where it ends. The
        // ring is its own renderer, so the field's lamps light the field as before.
        Holder("Ground", GroundMesh(FieldSize, 4f));
        Holder("Ground", RingMesh(FieldSize, RingSize, 4f)).name = "Ground ring";
        // The road: every road cell's square, in one mesh, UVs in metres over 3.2 m.
        Holder("Road", RoadMesh(t));

        // Kerbs: on the outside of every bend, and on both sides of a bend's inside corner.
        for (int y = 0; y < t.H; y++)
            for (int x = 0; x < t.W; x++)
            {
                var p = new Vector2Int(x, y);
                if (t.Road(p))
                {
                    if (Corner(t, p, out _, out _))
                        for (int h = 0; h < 4; h++) if (!t.Road(p + Track.Dir(h))) kerbs.Add((p, h));
                    continue;
                }
                // A wall cell with road on two sides at right angles: the inside of a bend.
                bool n = t.Road(p + Track.Dir(0)), e = t.Road(p + Track.Dir(1)), s = t.Road(p + Track.Dir(2)), w = t.Road(p + Track.Dir(3));
                if ((n || s) && (e || w))
                    for (int h = 0; h < 4; h++) { var q = p + Track.Dir(h); if (t.Road(q)) kerbs.Add((q, (h + 2) % 4)); }
            }
        foreach (var (p, h) in kerbs) Put("Kerb", Cell(p) + D(h) * (0.5f - 0.18f * M), Yaw(-D(h)));
        // The painted edge lines, where there is no kerb; the start line and the grid slot.
        Holder("Paint", PaintMesh(t));

        // The pit straight: from the grid along the start heading, the pit lane on its outer side.
        int left = (t.startHeading + 3) % 4, right = (t.startHeading + 1) % 4;
        bool Boundary(Vector2Int q) => q.x <= 0 || q.y <= 0 || q.x >= t.W - 1 || q.y >= t.H - 1;
        int pit = Boundary(t.start + Track.Dir(left)) || !Boundary(t.start + Track.Dir(right)) ? left : right;
        PitSide = pit == left ? 1 : -1;
        var straight = new List<Vector2Int>();
        for (var q = t.start; t.Road(q) && !t.Road(q + Track.Dir(pit)); q += Track.Dir(t.startHeading)) straight.Add(q);
        var pitEdges = new HashSet<Vector2Int>();
        for (int i = 0; i + 1 < straight.Count; i += 2)
        {
            var mid = (Cell(straight[i]) + Cell(straight[i + 1])) / 2f + D(pit) * (0.5f + 0.52f * M);
            Put("PitWall", mid, Yaw(-D(pit)));
            pitEdges.Add(straight[i]); pitEdges.Add(straight[i + 1]);
        }

        // Barriers and tyre walls on every edge between road and wall.
        for (int y = 0; y < t.H; y++)
            for (int x = 0; x < t.W; x++)
            {
                var p = new Vector2Int(x, y);
                if (!t.Road(p)) continue;
                bool corner = Corner(t, p, out int h1, out int h2);
                for (int h = 0; h < 4; h++)
                {
                    if (t.Road(p + Track.Dir(h))) continue;
                    if (p == t.start && h == (t.startHeading + 2) % 4) continue; // the grid's back: the gantry
                    if (h == pit && pitEdges.Contains(p)) continue;
                    bool tyres = corner || (t.Finish(p) && RoadNeighbours(t, p) == 1);
                    var edge = Cell(p) + D(h) * 0.5f;
                    if (tyres) Put("TyreWall", edge + D(h) * 0.36f * M, Yaw(-D(h)));
                    else Put("Barrier", edge + D(h) * 0.32f * M, Yaw(-D(h)));
                }
                // The outer corner of a bend, between its two walls: a stack of tyres.
                if (corner)
                {
                    int a = (h1 + 2) % 4, b = (h2 + 2) % 4; // the walls are opposite the two ways out
                    Put("TyreStack", Cell(p) + (D(a) + D(b)) * (0.5f + 0.34f * M), 0f);
                }
            }

        // The start gantry over the back of the grid, its lights toward the car and the camera in front.
        Put("Gantry", Cell(t.start) - D(t.startHeading) * 0.5f, Yaw(D(t.startHeading)));

        // The pit lane behind the wall, the pit box by the grid, the grandstand behind, the floodlights.
        var along = D(t.startHeading); var outward = D(pit);
        var s0 = Cell(straight[0]); var s1 = Cell(straight[straight.Count - 1]);
        var laneCentre = (s0 + s1) / 2f + outward * (0.5f + 2.6f * M);
        float laneLen = (straight.Count + 3) * 1f;
        var lane = Holder("PitLane", Strip(laneLen, 3.2f * M, 3.2f));
        lane.transform.SetPositionAndRotation(laneCentre + Vector3.up * 0.002f, Quaternion.Euler(0f, Yaw(along), 0f));
        Put("PitBox", s0 + outward * (0.5f + 2.3f * M) + along * 0.3f, Yaw(-outward));
        for (int i = 0; i < 3; i++) Put("Cone", s0 + outward * (0.5f + 1.25f * M) + along * (1.4f + i * 0.45f), i * 23f);
        Put("Grandstand", (s0 + s1) / 2f + along * 0.8f + outward * (0.5f + 7.4f * M), Yaw(-outward));
        Put("Floodlight", s0 - along * 1.6f + outward * (0.5f + 4.6f * M), Yaw(-outward + along * 0.45f));
        Put("Floodlight", s1 + along * 1.4f + outward * (0.5f + 4.6f * M), Yaw(-outward - along * 0.35f));
        // Two more across the circuit, aimed back at it.
        var b0 = Bounds;
        Put("Floodlight", b0.center - outward * (Mathf.Abs(Vector3.Dot(b0.extents, outward)) + 2.2f) + along * (b0.extents.magnitude * 0.3f), Yaw(outward));

        // The finish: the chequered line across the way in, a flag each side.
        foreach (var p in AllCells(t))
        {
            if (!t.Finish(p)) continue;
            for (int h = 0; h < 4; h++)
            {
                var q = p + Track.Dir(h);
                if (!t.Road(q) || t.Distance(q) != 1) continue;
                var edge = Cell(p) + D(h) * 0.5f;
                var across = Vector3.Cross(Vector3.up, D(h));
                Put("FinishLine", edge, Yaw(D(h)));
                Put("FinishFlag", edge + across * 0.98f, Yaw(D(h)) + 90f);
                Put("FinishFlag", edge - across * 0.98f, Yaw(D(h)) - 90f);
            }
        }

        // Grass along the outside of the barriers, trees in the infield and round the circuit.
        var rnd = new System.Random(t.id.GetHashCode());
        float R() => (float)rnd.NextDouble();
        int clumps = 0;
        foreach (var p in AllCells(t))
        {
            if (t.Road(p)) continue;
            int roads = RoadNeighbours(t, p);
            bool pitRow = straight.Contains(p - Track.Dir(pit));
            if (pitRow) continue;
            // Nothing growing where the cameras stand behind the grid.
            var behind = t.start - Track.Dir(t.startHeading);
            if (Mathf.Abs(p.x - behind.x) <= 1 && Mathf.Abs(p.y - behind.y) <= 1) continue;
            int n = roads > 0 ? 3 : 1;
            for (int i = 0; i < n && clumps < 260; i++)
            {
                var at = Cell(p) + new Vector3(R() - 0.5f, 0f, R() - 0.5f) * (roads > 0 ? 0.55f : 0.9f);
                if (roads > 0 && NearRoad(t, at, 0.42f)) continue;
                Put(i % 3 == 0 ? "GrassC" : i % 2 == 0 ? "GrassB" : "GrassA", at, R() * 360f, 0.8f + R() * 0.6f);
                clumps++;
            }
            // Deep in the infield, a tree.
            if (roads == 0 && Surrounded(t, p) && R() < 0.45f)
                Put(R() < 0.5f ? "TreeB" : "TreeA", Cell(p) + new Vector3(R() - 0.5f, 0f, R() - 0.5f) * 0.4f, R() * 360f, 0.55f + R() * 0.2f);
        }
        // A ring of trees round the circuit, not on the paddock's side.
        var ext = b0.extents;
        for (int i = 0; i < 34; i++)
        {
            float a = R() * Mathf.PI * 2f;
            var dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
            if (Vector3.Dot(dir, outward) > 0.45f) continue;
            float rx = ext.x + 1.6f + R() * 7f, rz = ext.z + 1.6f + R() * 7f;
            var at = b0.center + new Vector3(dir.x * rx, 0f, dir.z * rz);
            Put(i % 3 == 0 ? "TreeC" : i % 2 == 0 ? "TreeA" : "TreeB", new Vector3(at.x, 0f, at.z), R() * 360f, 0.85f + R() * 0.4f);
            if (clumps < 300) { Put("GrassA", new Vector3(at.x, 0f, at.z) + new Vector3(R() - 0.5f, 0, R() - 0.5f), R() * 360f, 1.2f); clumps++; }
        }

        // One reflection of the evening for the paint and the metal, rendered once the lamps are on.
        var pgo = new GameObject("Reflections");
        pgo.transform.SetParent(ground, false);
        pgo.transform.position = Cell(t.start) + Vector3.up * 0.6f;
        probe = pgo.AddComponent<ReflectionProbe>();
        probe.mode = ReflectionProbeMode.Realtime;
        probe.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
        probe.timeSlicingMode = ReflectionProbeTimeSlicingMode.NoTimeSlicing;
        probe.resolution = 128;
        probe.size = new Vector3(t.W + 30f, 20f, t.H + 30f);
        probe.intensity = 1f;
        probe.clearFlags = ReflectionProbeClearFlags.Skybox;
        StartCoroutine(RenderProbe());
    }

    IEnumerator RenderProbe()
    {
        yield return null;
        if (probe != null) probe.RenderProbe();
    }

    static IEnumerable<Vector2Int> AllCells(Track t)
    {
        for (int y = 0; y < t.H; y++) for (int x = 0; x < t.W; x++) yield return new Vector2Int(x, y);
    }

    static int RoadNeighbours(Track t, Vector2Int p)
    {
        int n = 0;
        for (int h = 0; h < 4; h++) if (t.Road(p + Track.Dir(h))) n++;
        return n;
    }

    // A road cell where the way turns: exactly two road neighbours, at right angles.
    static bool Corner(Track t, Vector2Int p, out int h1, out int h2)
    {
        h1 = h2 = -1;
        for (int h = 0; h < 4; h++) if (t.Road(p + Track.Dir(h))) { if (h1 < 0) h1 = h; else if (h2 < 0) h2 = h; else return false; }
        return h1 >= 0 && h2 >= 0 && (h2 - h1) % 2 != 0;
    }

    static bool Surrounded(Track t, Vector2Int p)
    {
        for (int dy = -1; dy <= 1; dy++) for (int dx = -1; dx <= 1; dx++) if (t.Road(p + new Vector2Int(dx, dy))) return false;
        return true;
    }

    bool NearRoad(Track t, Vector3 at, float within)
    {
        foreach (var p in AllCells(t))
        {
            if (!t.Road(p)) continue;
            var c = Cell(p);
            float dx = Mathf.Max(0f, Mathf.Abs(at.x - c.x) - 0.5f), dz = Mathf.Max(0f, Mathf.Abs(at.z - c.z) - 0.5f);
            if (dx * dx + dz * dz < within * within) return true;
        }
        return false;
    }

    // A holder from the kit (it carries the material), given a mesh made here in world units.
    GameObject Holder(string name, Mesh mesh)
    {
        var go = Instantiate(Piece(name), ground, false);
        go.transform.localScale = Vector3.one;
        go.GetComponent<MeshFilter>().sharedMesh = mesh;
        return go;
    }

    static Mesh Finish(List<Vector3> v, List<Vector2> uv, List<int> tris, string name)
    {
        var mesh = new Mesh { name = name, indexFormat = v.Count > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16 };
        mesh.SetVertices(v); mesh.SetUVs(0, uv); mesh.SetTriangles(tris, 0);
        var n = new List<Vector3>(); for (int i = 0; i < v.Count; i++) n.Add(Vector3.up);
        mesh.SetNormals(n);
        mesh.RecalculateTangents(); mesh.RecalculateBounds();
        return mesh;
    }

    // A flat quad facing up, its corners given clockwise from above.
    static void Quad(List<Vector3> v, List<Vector2> uv, List<int> tris, Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector2 ua, Vector2 ub, Vector2 uc, Vector2 ud)
    {
        int i = v.Count;
        v.Add(a); v.Add(b); v.Add(c); v.Add(d);
        uv.Add(ua); uv.Add(ub); uv.Add(uc); uv.Add(ud);
        // Wound so the face points up.
        if (Vector3.Cross(b - a, c - a).y >= 0f) tris.AddRange(new[] { i, i + 1, i + 2, i, i + 2, i + 3 });
        else tris.AddRange(new[] { i, i + 2, i + 1, i, i + 3, i + 2 });
    }

    const float FieldSize = 90f, RingSize = 280f; // the field and the ring round it, across, in world units
    const float FogStart = 30f, FogEnd = 75f;     // past every track (9 units from the centre at most) and its cameras

    // The ring of grass round the field: the square RingSize across with the field's square
    // cut out, in tiles of about 10 units so the fog, worked out at the corners, stays smooth.
    Mesh RingMesh(float inner, float outer, float repeatM)
    {
        var v = new List<Vector3>(); var uv = new List<Vector2>(); var tris = new List<int>();
        var c = Bounds.center; float hi = inner / 2f, ho = outer / 2f;
        var cuts = new List<float>();
        for (float a = -ho; a < -hi - 0.01f; a += 10f) cuts.Add(a);
        for (float a = -hi; a <= hi + 0.01f; a += 10f) cuts.Add(a); // the field is 90 across: nine tens
        for (float a = hi + 5f; a < ho - 0.01f; a += 10f) cuts.Add(a);
        cuts.Add(ho);
        Vector2 U(float x, float z) => new Vector2(x / M / repeatM, z / M / repeatM);
        for (int i = 0; i + 1 < cuts.Count; i++)
            for (int j = 0; j + 1 < cuts.Count; j++)
            {
                float a0 = cuts[i], a1 = cuts[i + 1], b0 = cuts[j], b1 = cuts[j + 1];
                if (a0 >= -hi && a1 <= hi && b0 >= -hi && b1 <= hi) continue; // the field's own square
                float x0 = c.x + a0, x1 = c.x + a1, z0 = c.z + b0, z1 = c.z + b1;
                Quad(v, uv, tris, new Vector3(x0, -0.01f, z0), new Vector3(x0, -0.01f, z1), new Vector3(x1, -0.01f, z1), new Vector3(x1, -0.01f, z0), U(x0, z0), U(x0, z1), U(x1, z1), U(x1, z0));
            }
        return Finish(v, uv, tris, "ground ring");
    }

    Mesh GroundMesh(float size, float repeatM)
    {
        var v = new List<Vector3>(); var uv = new List<Vector2>(); var tris = new List<int>();
        const int n = 12; // tiles, so the field takes the lamps' light per tile
        var c = Bounds.center; float half = size / 2f, step = size / n;
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
            {
                float x0 = c.x - half + i * step, z0 = c.z - half + j * step, x1 = x0 + step, z1 = z0 + step;
                Vector2 U(float x, float z) => new Vector2(x / M / repeatM, z / M / repeatM);
                Quad(v, uv, tris, new Vector3(x0, -0.01f, z0), new Vector3(x0, -0.01f, z1), new Vector3(x1, -0.01f, z1), new Vector3(x1, -0.01f, z0), U(x0, z0), U(x0, z1), U(x1, z1), U(x1, z0));
            }
        return Finish(v, uv, tris, "ground");
    }

    Mesh RoadMesh(Track t)
    {
        var v = new List<Vector3>(); var uv = new List<Vector2>(); var tris = new List<int>();
        const float repeatM = 3.2f;
        Vector2 U(Vector3 p) => new Vector2(p.x / M / repeatM, p.z / M / repeatM);
        foreach (var p in AllCells(t))
        {
            if (!t.Road(p)) continue;
            var c = Cell(p) + Vector3.up * 0.002f;
            Vector3 a = c + new Vector3(-0.5f, 0, -0.5f), b = c + new Vector3(-0.5f, 0, 0.5f), cc = c + new Vector3(0.5f, 0, 0.5f), d = c + new Vector3(0.5f, 0, -0.5f);
            Quad(v, uv, tris, a, b, cc, d, U(a), U(b), U(cc), U(d));
        }
        return Finish(v, uv, tris, "road");
    }

    // A strip along +Z, centred, width across X; UVs in metres over repeatM.
    static Mesh Strip(float length, float width, float repeatM)
    {
        var v = new List<Vector3>(); var uv = new List<Vector2>(); var tris = new List<int>();
        float hl = length / 2f, hw = width / 2f;
        Vector2 U(float x, float z) => new Vector2(x / M / repeatM, z / M / repeatM);
        Quad(v, uv, tris, new Vector3(-hw, 0, -hl), new Vector3(-hw, 0, hl), new Vector3(hw, 0, hl), new Vector3(hw, 0, -hl), U(-hw, -hl), U(-hw, hl), U(hw, hl), U(hw, -hl));
        return Finish(v, uv, tris, "strip");
    }

    Mesh PaintMesh(Track t)
    {
        var v = new List<Vector3>(); var uv = new List<Vector2>(); var tris = new List<int>();
        const float inset = 0.16f * M, width = 0.1f * M, y = 0.004f;
        void Line(Vector3 from, Vector3 to, float w)
        {
            var along = (to - from); float len = along.magnitude; along /= len;
            var side = Vector3.Cross(Vector3.up, along) * (w / 2f);
            float l = len / M;
            Quad(v, uv, tris, from - side + Vector3.up * y, from + side + Vector3.up * y, to + side + Vector3.up * y, to - side + Vector3.up * y,
                 new Vector2(0, 0), new Vector2(0, w / M), new Vector2(l, w / M), new Vector2(l, 0));
        }
        foreach (var p in AllCells(t))
        {
            if (!t.Road(p)) continue;
            for (int h = 0; h < 4; h++)
            {
                if (t.Road(p + Track.Dir(h)) || kerbs.Contains((p, h))) continue;
                if (p == t.start && h == (t.startHeading + 2) % 4) continue;
                var n = D(h); var across = Vector3.Cross(Vector3.up, n);
                var mid = Cell(p) + n * (0.5f - inset - width / 2f);
                // Stop short where the next cell's own line or a kerb takes over.
                float a0 = t.Road(p + Track.Dir((h + 1) % 4)) ? 0.5f : 0.5f - inset, a1 = t.Road(p + Track.Dir((h + 3) % 4)) ? 0.5f : 0.5f - inset;
                Line(mid - across * a1, mid + across * a0, width);
            }
        }
        // The start line in front of the grid, and the grid slot's bar just ahead of the car's nose.
        var f = D(t.startHeading); var x = Vector3.Cross(Vector3.up, f);
        var s = Cell(t.start);
        Line(s + f * 0.47f - x * 0.42f, s + f * 0.47f + x * 0.42f, 0.12f * M);
        Line(s + f * 0.24f - x * 0.36f, s + f * 0.24f + x * 0.05f, 0.08f * M);
        return Finish(v, uv, tris, "paint");
    }

    // ---- the car ----

    void BuildCarKit()
    {
        carRoot = new GameObject("Car").transform;
        carRoot.SetParent(transform, false);
        var car = Instantiate(Piece("Car"), carRoot, false);
        car.transform.localScale = Vector3.one * M;
        carRig = car.GetComponent<CarRig>();
        carBody = car.transform.Find("Body");
        carPaint = null;
        int wheel = 0;
        foreach (var x in new[] { -0.15f, 0.15f })
        {
            // Teal trails off the rear wheels at speed: motion the camera can see.
            var trail = new GameObject("trail").AddComponent<TrailRenderer>();
            trail.transform.SetParent(carRoot, false);
            trail.transform.localPosition = new Vector3(x, 0.02f, -0.27f);
            trail.time = 0.4f; trail.widthMultiplier = 0.05f; trail.minVertexDistance = 0.05f;
            trail.sharedMaterial = Look.Glass(new Color(Theme.Teal.r, Theme.Teal.g, Theme.Teal.b, 0.5f));
            trail.widthCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
            trail.shadowCastingMode = ShadowCastingMode.Off; trail.receiveShadows = false;
            trail.emitting = false; trail.autodestruct = false;
            trails[wheel++] = trail;
        }
        // The sensors: front, left, right, as beams from the pod on the nose.
        for (int i = 0; i < 3; i++)
        {
            var ray = new GameObject("ray").transform;
            ray.SetParent(carRoot, false);
            ray.localPosition = new Vector3(0f, 0f, 0.22f);
            ray.localRotation = Quaternion.Euler(0, i == 0 ? 0 : i == 1 ? -90 : 90, 0);
            var beam = MeshKit.Part(ray, "beam", MeshKit.Box, Look.Glass(Look.RayClear), new Vector3(0, 0.2f, 0.5f), new Vector3(0.06f, 0.02f, 1f));
            beam.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
            rays[i] = ray;
            rayRenderers[i] = beam.GetComponent<Renderer>();
        }
    }

    // The pit lane's portrait: in front of the car and to the side away from the pit, low,
    // close enough that the car fills the screen's world (the pit lane, the gantry behind).
    void PortraitKit()
    {
        // The road is one cell wide between its barriers: the camera stays on it, on the car's
        // right, the side of the shell with both arches open (see TrackKit.BuildCar).
        cam.portraitAhead = 1.05f; cam.portraitSide = -0.3f; cam.portraitUp = 0.34f; cam.portraitAnchor = 0.13f; cam.portraitLook = 0f; cam.portraitFov = 42f;
    }

    // The briefing's shot: low behind the car on the grid, looking down the first straight.
    public void ShowGrid(RectTransform frame)
    {
        this.frame = frame; Canvas.ForceUpdateCanvases();
        if (HasKit) { cam.portraitAhead = -1.2f; cam.portraitSide = PitSide * 0.2f; cam.portraitUp = 0.5f; cam.portraitAnchor = 0.12f; cam.portraitLook = 1.3f; cam.portraitFov = 46f; }
        else { cam.portraitAhead = 3.3f; cam.portraitSide = 1.4f; cam.portraitUp = 1.25f; cam.portraitAnchor = 0.3f; cam.portraitLook = 0f; cam.portraitFov = 30f; }
        cam.Portrait(carRoot, frame); cam.Snap();
    }

    // A slow swing in front of the car where it stands (the title, the debrief): an arc, not a
    // full turn, since the road is one cell wide between its barriers.
    public void ShowCarOrbit(RectTransform frame, float distance = 1.45f, float pitch = 7f)
    {
        this.frame = frame; Canvas.ForceUpdateCanvases();
        if (!HasKit || carRoot == null) { cam.orbitArc = 0f; cam.Orbit(frame, Bounds.center); cam.Snap(); return; }
        cam.orbitDistance = distance; cam.orbitPitch = pitch; cam.orbitSpeed = 6f; cam.orbitFov = 42f;
        cam.orbitArc = 17f; cam.orbitBase = Yaw(carRoot.forward) + 180f;
        cam.Orbit(frame, carRoot.position + Vector3.up * 0.15f);
        cam.Snap();
    }

    public Transform Car => carRoot;
}
