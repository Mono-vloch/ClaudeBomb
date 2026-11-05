using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The track in three dimensions: asphalt where the road is, kerbs where it meets
// a wall, barriers on the walls, the start line, the finish arch, oil where the
// real track has it, grass and trees around, a stand along the far side. And the
// car: the robot, with its sensors drawn as rays that reach as far as they read.
// Build() for a track, Pose() every frame from the simulation's car.
public class Circuit : MonoBehaviour
{
    public SceneCamera cam;      // on the scene's camera; set by App
    public CameraRig rig;        // the chase, the countdown, the finish; set by App
    public RectTransform frame;  // where on the stage the track sits; set by the screen
    public Track track;
    public Transform CarRoot => carRoot;

    Transform ground, carRoot, carBody;
    Effects effects;
    readonly TrailRenderer[] trails = new TrailRenderer[2];
    readonly List<LineRenderer> ghosts = new List<LineRenderer>();
    const int GhostsKept = 14;
    Vector3 lastCarPos;
    float dustDistance;
    readonly Transform[] rays = new Transform[3];
    readonly Renderer[] rayRenderers = new Renderer[3];
    Material carPaint;
    bool dusk;
    Coroutine demo;

    // Shadows, cascades and anti-aliasing come from the render pipeline asset
    // (Assets/Settings, made by Robot Racing > Set up URP) and the camera's Atmosphere.
    public static void Setup() { }

    public Vector3 Cell(Vector2Int p) => new Vector3(p.x - (track.W - 1) / 2f, 0, (track.H - 1) / 2f - p.y);
    public Bounds Bounds => new Bounds(new Vector3(0, 0.4f, 0), new Vector3(track.W + 0.6f, 1f, track.H + 0.6f));
    public static Quaternion Facing(int heading) => Quaternion.Euler(0, heading * 90f, 0);

    public void Build(Track t, bool dusk)
    {
        Clear();
        gameObject.SetActive(true);
        Sfx.Ambient(true);
        track = t;
        this.dusk = dusk;
        ground = new GameObject("Ground").transform;
        ground.SetParent(transform, false);
        BuildLight();
        BuildTrack();
        BuildCar();
        if (effects == null) { effects = new GameObject("Effects").AddComponent<Effects>(); effects.transform.SetParent(transform.parent, false); }
        ghosts.Clear();
        Pose(t.start, t.start, t.startHeading, 0, 0f, null, null);
    }

    public void Clear()
    {
        StopAllCoroutines();
        demo = null;
        for (int i = transform.childCount - 1; i >= 0; i--) Destroy(transform.GetChild(i).gameObject);
    }

    public void Hide()
    {
        Clear();
        Sfx.Ambient(false);
        Sfx.Engine(0f);
        if (rig != null) rig.Release();
        gameObject.SetActive(false);
        if (cam != null) cam.Sleep();
    }

    public void ShowOverview(RectTransform frame) { this.frame = frame; Canvas.ForceUpdateCanvases(); cam.Overview(frame, Bounds); cam.Snap(); }
    public void ShowFollow(RectTransform frame) { this.frame = frame; rig.Chase(carRoot, frame); }
    public void ShowOverviewAgain() { rig.Release(); cam.Overview(frame, Bounds); }
    // The race starts: the camera flies in from high and wide, down behind the car.
    public void ShowCountdown(RectTransform frame, float seconds) { this.frame = frame; rig.Countdown(carRoot, frame, seconds); }
    // The race is won: a slow turn around the car, in slow motion, then back to the overview.
    public void FinishMoment(float seconds, System.Action done) => rig.FinishMoment(carRoot, seconds, () => { ShowOverviewAgain(); done?.Invoke(); });
    public void ShowPortrait(RectTransform frame) { this.frame = frame; Canvas.ForceUpdateCanvases(); cam.Portrait(carRoot, frame); cam.Snap(); }
    public void ShowOrbit(RectTransform frame) { this.frame = frame; Canvas.ForceUpdateCanvases(); cam.Orbit(frame, Bounds.center); cam.Snap(); }

    // The light story, from Docs/art-bible.md. Day: a warm late-afternoon key from the
    // camera's right, full soft shadows, a gradient ambient that keeps the shadow sides
    // cool, haze that deepens with distance. Dusk (the title, the cover): the sun low
    // and amber, the brand's ink sky, the stand's lamps and the arch lit.
    void BuildLight()
    {
        var sunGo = new GameObject("Sun");
        sunGo.transform.SetParent(transform, false);
        var sun = sunGo.AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.shadows = LightShadows.Soft;
        sun.shadowStrength = dusk ? 0.75f : 0.85f;
        var dome = MeshKit.Part(transform, "sky", MeshKit.Sphere, null, Vector3.zero, Vector3.one * 300f);
        var dr = dome.GetComponent<Renderer>();
        dr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        dr.receiveShadows = false;
        var camera = cam.GetComponent<Camera>();
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        if (dusk)
        {
            sun.transform.rotation = Quaternion.Euler(16, 55, 0); sun.color = Look.SunDusk; sun.intensity = 0.9f;
            RenderSettings.ambientSkyColor = Theme.Hex("#2A3F6B"); RenderSettings.ambientEquatorColor = Theme.Hex("#6B4A5E"); RenderSettings.ambientGroundColor = Theme.Hex("#1A2433");
            RenderSettings.fogColor = Theme.Hex("#2B3E52");
            dr.sharedMaterial = Look.Sky(Look.DuskTop, Look.DuskHorizon, Look.DuskBottom);
            camera.backgroundColor = Look.DuskHorizon;
            Look.SetSky(Look.DuskTop, Look.DuskHorizon);
        }
        else
        {
            sun.transform.rotation = Quaternion.Euler(42, -55, 0); sun.color = Look.SunDay; sun.intensity = 1.15f;
            RenderSettings.ambientSkyColor = Theme.Hex("#8FB6D8"); RenderSettings.ambientEquatorColor = Theme.Hex("#C9C2B4"); RenderSettings.ambientGroundColor = Theme.Hex("#4A5A6B");
            RenderSettings.fogColor = Look.Haze;
            dr.sharedMaterial = Look.Sky(Look.SkyTop, Look.SkyHorizon, Look.SkyBottom);
            camera.backgroundColor = Look.SkyHorizon;
            Look.SetSky(Look.SkyTop, Look.SkyHorizon);
        }
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 30f;
        RenderSettings.fogEndDistance = 120f;
    }

    // A warm lamp on a post: lit at dusk, a prop by day.
    void Lamp(Transform parent, Vector3 at, float height)
    {
        MeshKit.Part(parent, "post", MeshKit.Cylinder, Look.Mat(Look.Cabin), at + Vector3.up * height / 2, new Vector3(0.05f, height, 0.05f));
        MeshKit.Part(parent, "lamp", MeshKit.Sphere, dusk ? Look.Mat(Look.Lamp, Look.Lamp * 1.6f) : Look.Mat(Look.Shell, Color.black, 0.5f), at + Vector3.up * (height + 0.08f), 0.16f);
        if (!dusk) return;
        var light = new GameObject("light");
        light.transform.SetParent(parent, false);
        light.transform.localPosition = at + Vector3.up * (height - 0.1f);
        var point = light.AddComponent<Light>();
        point.type = LightType.Point; point.color = Look.Lamp; point.intensity = 2.2f; point.range = 7f; point.shadows = LightShadows.None;
    }

    static GameObject NoShadow(GameObject go) { go.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; return go; }

    void BuildTrack()
    {
        var t = track;
        NoShadow(MeshKit.Part(ground, "grass", MeshKit.Box, Look.Mat(Look.Grass), new Vector3(0, -0.1f, 0), new Vector3(120, 0.2f, 120)));
        NoShadow(MeshKit.Part(ground, "verge", MeshKit.Box, Look.Mat(Look.GrassDark), new Vector3(0, -0.09f, 0), new Vector3(t.W + 1.6f, 0.2f, t.H + 1.6f)));
        int kerb = 0;
        for (int y = 0; y < t.H; y++)
            for (int x = 0; x < t.W; x++)
            {
                var p = new Vector2Int(x, y);
                var c = Cell(p);
                if (t.Road(p))
                {
                    var tile = NoShadow(MeshKit.Part(ground, "road", MeshKit.Box, Look.Mat(t.Oil(p) ? Look.Oil : Look.Asphalt), c + Vector3.up * 0.02f, new Vector3(1f, 0.04f, 1f)));
                    if (t.Oil(p)) { var sheen = NoShadow(MeshKit.Part(ground, "sheen", MeshKit.Box, Look.Glass(new Color(0.4f, 0.6f, 0.9f, 0.25f)), c + Vector3.up * 0.045f, new Vector3(0.8f, 0.005f, 0.8f))); }
                    // Kerbs where the road meets a wall.
                    for (int h = 0; h < 4; h++)
                    {
                        if (t.Road(p + Track.Dir(h))) continue;
                        var dir = Track.Dir(h);
                        var at = c + new Vector3(dir.x * 0.47f, 0.05f, -dir.y * 0.47f);
                        var size = h % 2 == 0 ? new Vector3(0.9f, 0.06f, 0.08f) : new Vector3(0.08f, 0.06f, 0.9f);
                        NoShadow(MeshKit.Part(ground, "kerb", MeshKit.Box, Look.Mat(kerb++ % 2 == 0 ? Look.KerbRed : Look.KerbWhite, Color.black, 0.15f), at, size));
                    }
                    if (t.At(p) == 'S')
                        NoShadow(MeshKit.Part(ground, "start", MeshKit.Box, Look.Mat(Look.AsphaltLine), c + Vector3.up * 0.045f, t.startHeading % 2 == 0 ? new Vector3(0.9f, 0.005f, 0.12f) : new Vector3(0.12f, 0.005f, 0.9f)));
                    if (t.Finish(p))
                    {
                        for (int i = 0; i < 4; i++) for (int j = 0; j < 4; j++)
                            NoShadow(MeshKit.Part(ground, "flag", MeshKit.Box, Look.Mat((i + j) % 2 == 0 ? Look.KerbWhite : Look.Cabin), c + new Vector3(-0.375f + i * 0.25f, 0.045f, -0.375f + j * 0.25f), new Vector3(0.25f, 0.005f, 0.25f)));
                        // The arch
                        MeshKit.Part(ground, "post", MeshKit.Cylinder, Look.Mat(Look.Shell), c + new Vector3(-0.55f, 0.6f, 0), new Vector3(0.08f, 1.2f, 0.08f));
                        MeshKit.Part(ground, "post", MeshKit.Cylinder, Look.Mat(Look.Shell), c + new Vector3(0.55f, 0.6f, 0), new Vector3(0.08f, 1.2f, 0.08f));
                        for (int i = 0; i < 6; i++)
                            MeshKit.Part(ground, "banner", MeshKit.Box, i % 2 == 0 ? Look.Mat(Look.Cabin) : Look.Mat(Look.KerbWhite, dusk ? Look.KerbWhite * 0.9f : Color.black, 0.3f), c + new Vector3(-0.5f + 0.1f + i * 0.2f, 1.2f, 0), new Vector3(0.2f, 0.22f, 0.06f));
                    }
                }
                else
                {
                    // A barrier wherever a wall touches the road; grass elsewhere.
                    bool touches = false;
                    for (int h = 0; h < 4; h++) if (t.Road(p + Track.Dir(h))) touches = true;
                    if (touches) MeshKit.Part(ground, "barrier", MeshKit.Box, Look.Mat((x + y) % 2 == 0 ? Look.Barrier : Look.BarrierDark), c + Vector3.up * 0.2f, new Vector3(0.92f, 0.4f, 0.92f));
                }
            }

        // Trees around, and a stand along the north side.
        var r = new System.Random(3);
        float rx = t.W / 2f + 2.5f, rz = t.H / 2f + 2.5f;
        for (int i = 0; i < 22; i++)
        {
            float a = (float)(r.NextDouble() * 360);
            var pos = new Vector3(Mathf.Sin(a * Mathf.Deg2Rad) * (rx + (float)r.NextDouble() * 3), 0, Mathf.Cos(a * Mathf.Deg2Rad) * (rz + (float)r.NextDouble() * 3));
            if (a > 130 && a < 230 && pos.magnitude < rz + 1.5f) continue;
            float s = 0.7f + (float)r.NextDouble() * 0.5f;
            var tree = new GameObject("tree").transform; tree.SetParent(ground, false); tree.localPosition = pos; tree.localScale = Vector3.one * s;
            MeshKit.Part(tree, "trunk", MeshKit.Cylinder, Look.Mat(Look.Trunk), new Vector3(0, 0.35f, 0), new Vector3(0.18f, 0.7f, 0.18f));
            MeshKit.Part(tree, "crown", MeshKit.Sphere, Look.Mat(i % 2 == 0 ? Look.Leaf : Look.LeafDark), new Vector3(0, 1.2f, 0), new Vector3(1.3f, 1.2f, 1.3f));
        }
        var stand = new GameObject("stand").transform; stand.SetParent(ground, false); stand.localPosition = new Vector3(0, 0, rz + 0.5f);
        for (int i = -1; i <= 1; i++) Lamp(stand, new Vector3(i * (t.W / 2f + 0.5f), 0, -0.9f), 1.8f);
        for (int row = 0; row < 3; row++)
        {
            MeshKit.Part(stand, "tier", MeshKit.Box, Look.Mat(row % 2 == 0 ? Look.Stand : Look.StandDark), new Vector3(0, 0.2f + row * 0.4f, row * 0.5f), new Vector3(t.W + 1f, 0.4f, 0.5f));
            for (int i = 0; i < t.W; i++)
                if (r.Next(3) > 0) MeshKit.Part(stand, "fan", MeshKit.Sphere, Look.Mat(Theme.Team[r.Next(Theme.Team.Length)]), new Vector3(-t.W / 2f + 0.5f + i, 0.55f + row * 0.4f, row * 0.5f), 0.22f);
        }
    }

    void BuildCar()
    {
        carRoot = new GameObject("Car").transform;
        carRoot.SetParent(transform, false);
        carBody = new GameObject("Body").transform;
        carBody.SetParent(carRoot, false);
        carPaint = Look.Own(Look.Car, 0.6f);
        var body = MeshKit.Part(carBody, "shell", MeshKit.Box, carPaint, new Vector3(0, 0.22f, 0), new Vector3(0.5f, 0.2f, 0.82f));
        MeshKit.Part(carBody, "nose", MeshKit.Box, Look.Mat(Look.CarDark), new Vector3(0, 0.2f, 0.42f), new Vector3(0.4f, 0.12f, 0.1f));
        MeshKit.Part(carBody, "cabin", MeshKit.Box, Look.Mat(Look.Cabin, Color.black, 0.5f), new Vector3(0, 0.37f, -0.06f), new Vector3(0.38f, 0.16f, 0.36f));
        MeshKit.Part(carBody, "stripe", MeshKit.Box, Look.Mat(Look.Shell), new Vector3(0, 0.325f, 0.15f), new Vector3(0.1f, 0.01f, 0.5f));
        MeshKit.Part(carBody, "eye", MeshKit.Sphere, Look.Mat(Theme.Hex("#9BE8FF"), Theme.Hex("#9BE8FF") * 0.5f, 0.8f), new Vector3(-0.14f, 0.27f, 0.42f), 0.07f);
        MeshKit.Part(carBody, "eye", MeshKit.Sphere, Look.Mat(Theme.Hex("#9BE8FF"), Theme.Hex("#9BE8FF") * 0.5f, 0.8f), new Vector3(0.14f, 0.27f, 0.42f), 0.07f);
        int wheel = 0;
        foreach (var (x, z) in new[] { (-0.28f, 0.28f), (0.28f, 0.28f), (-0.28f, -0.28f), (0.28f, -0.28f) })
        {
            var w = MeshKit.Part(carBody, "wheel", MeshKit.Cylinder, Look.Mat(Look.Wheel), new Vector3(x, 0.12f, z), new Vector3(0, 0, 90), new Vector3(0.24f, 0.1f, 0.24f));
            if (z < 0)
            {
                // Teal trails off the rear wheels at speed: motion the camera can see.
                var trail = new GameObject("trail").AddComponent<TrailRenderer>();
                trail.transform.SetParent(carRoot, false);
                trail.transform.localPosition = new Vector3(x, 0.03f, z);
                trail.time = 0.4f; trail.widthMultiplier = 0.06f; trail.minVertexDistance = 0.05f;
                trail.sharedMaterial = Look.Glass(new Color(Theme.Teal.r, Theme.Teal.g, Theme.Teal.b, 0.55f));
                trail.widthCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
                trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; trail.receiveShadows = false;
                trail.emitting = false; trail.autodestruct = false;
                trails[wheel++] = trail;
            }
        }
        MeshKit.Part(carBody, "mast", MeshKit.Cylinder, Look.Mat(Look.Cabin), new Vector3(0, 0.52f, -0.2f), new Vector3(0.02f, 0.12f, 0.02f));
        MeshKit.Part(carBody, "antenna", MeshKit.Sphere, Look.Mat(Look.Lamp, Look.Lamp * 0.9f, 0.5f), new Vector3(0, 0.6f, -0.2f), 0.07f);
        // The sensors: front, left, right, as rays from the car.
        for (int i = 0; i < 3; i++)
        {
            var ray = new GameObject("ray").transform;
            ray.SetParent(carRoot, false);
            ray.localRotation = Quaternion.Euler(0, i == 0 ? 0 : i == 1 ? -90 : 90, 0);
            var beam = NoShadow(MeshKit.Part(ray, "beam", MeshKit.Box, Look.Glass(Look.RayClear), new Vector3(0, 0.2f, 0.5f), new Vector3(0.06f, 0.02f, 1f)));
            rays[i] = ray;
            rayRenderers[i] = beam.GetComponent<Renderer>();
        }
        carRoot.localScale = Vector3.one * 0.9f;
    }

    // The car where the simulation says, k of the way from its last cell. The
    // rays reach as far as the sensors read; a sensor that is off is dim.
    public void Pose(Vector2Int from, Vector2Int to, int heading, int speed, float k, int[] readings, bool[] on)
    {
        if (carRoot == null) return;
        carRoot.position = Vector3.Lerp(Cell(from), Cell(to), k);
        carRoot.rotation = Quaternion.Slerp(carRoot.rotation, Facing(heading), k >= 1 ? 1 : 0.5f);
        carBody.localRotation = Quaternion.Euler(speed * 2f, 0, 0);
        rig.Speed(speed / 3f);
        Sfx.Engine(speed / 3f);
        bool jumped = (carRoot.position - lastCarPos).sqrMagnitude > 2.25f;
        float moved = jumped ? 0f : (carRoot.position - lastCarPos).magnitude;
        lastCarPos = carRoot.position;
        foreach (var trail in trails) { if (trail == null) continue; if (jumped) trail.Clear(); trail.emitting = speed >= 2 && !jumped; }
        if (speed >= 1 && effects != null)
        {
            dustDistance += moved;
            if (dustDistance > 0.25f) { dustDistance = 0f; effects.Dust(carRoot.position + Vector3.up * 0.05f - carRoot.forward * 0.3f, speed >= 2 ? 3 : 1); }
        }
        for (int i = 0; i < 3; i++)
        {
            bool isOn = on != null && on[i];
            int reading = readings != null ? readings[i] : Sensors.Far;
            float length = isOn ? reading + 0.35f : 0.6f;
            var beam = rays[i].GetChild(0);
            beam.localPosition = new Vector3(0, 0.2f, 0.45f + length / 2);
            beam.localScale = new Vector3(isOn ? 0.07f : 0.04f, 0.02f, length);
            rayRenderers[i].sharedMaterial = Look.Glass(!isOn ? Look.RayOff : reading >= Sensors.Far ? Look.RayClear : reading == 0 ? Look.RayWall : Look.RayNear);
        }
    }

    public void SetSensors(bool[] on) => Pose(track.start, track.start, track.startHeading, 0, 1f, Driving.Sense(track, new Car { pos = track.start, heading = track.startHeading }), on);

    // A crash: a burst of bits where the car is, and the car shakes.
    // A crash: sparks where the car is, the camera takes a hit, the vignette flashes red and,
    // watched at 1x, the world holds still for a moment. Quiet at the faster speeds.
    public void Crash(bool big = true)
    {
        effects.Sparks(carRoot.position + Vector3.up * 0.25f + carRoot.forward * 0.35f, big ? 1f : 0.5f);
        rig.Shake(big ? 1f : 0.4f);
        Sfx.Crash(big ? 1f : 0.45f);
        if (!big) return;
        Atmosphere.Current?.Pulse(Theme.Danger, 0.35f, 0.45f);
        Atmosphere.Current?.HitStop(0.07f);
    }
    public void Finished(bool big = true)
    {
        effects.Confetti(carRoot.position + Vector3.up * 0.4f, big ? 42 : 16);
        if (big) { Atmosphere.Current?.Pulse(Theme.Teal, 0.2f, 0.4f); Sfx.Chime(); }
    }

    // The ghost of an attempt: a faint line along the cells it drove, teal to a finish, red to
    // a crash, the oldest fading out. The learning, visible on the track.
    public void AddGhost(IReadOnlyList<Vector2Int> cells, bool finished)
    {
        if (cells == null || cells.Count < 2) return;
        var go = new GameObject("ghost");
        go.transform.SetParent(ground, false);
        var line = go.AddComponent<LineRenderer>();
        line.useWorldSpace = false; line.positionCount = cells.Count;
        for (int i = 0; i < cells.Count; i++) line.SetPosition(i, Cell(cells[i]) + Vector3.up * (0.075f + 0.002f * (ghosts.Count % 5)));
        line.widthMultiplier = 0.07f; line.numCornerVertices = 2; line.numCapVertices = 2;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; line.receiveShadows = false;
        line.sharedMaterial = Look.Glass(finished ? Theme.Teal : Theme.Danger);
        ghosts.Add(line);
        while (ghosts.Count > GhostsKept) { var old = ghosts[0]; ghosts.RemoveAt(0); if (old != null) Destroy(old.gameObject); }
        for (int i = 0; i < ghosts.Count; i++)
        {
            var g = ghosts[i]; if (g == null) continue;
            bool ok = g.sharedMaterial == Look.Glass(Theme.Teal);
            float a = Mathf.Lerp(0.06f, 0.38f, (i + 1f) / ghosts.Count);
            var c = ok ? Theme.Teal : Theme.Danger;
            g.sharedMaterial = Look.Glass(new Color(c.r, c.g, c.b, a));
        }
    }
    // Confetti over the circuit: the medal moment on the debrief.
    public void Celebrate()
    {
        if (effects == null) return;
        var c = Bounds.center;
        effects.Confetti(carRoot != null ? carRoot.position + Vector3.up * 0.4f : c, 30);
        effects.Confetti(c + new Vector3(-1.5f, 0.6f, 1f), 20);
        effects.Confetti(c + new Vector3(1.5f, 0.6f, -1f), 20);
    }
    public int Ghosts => ghosts.Count;
    public int EffectsAlive => effects != null ? effects.Alive : 0;

    // The cover: the car drives the track on its own, the short way, round and round.
    public void Demo(bool on)
    {
        if (demo != null) { StopCoroutine(demo); demo = null; }
        if (on) demo = StartCoroutine(DriveDemo());
    }

    IEnumerator DriveDemo()
    {
        while (true)
        {
            var p = track.start; int heading = track.startHeading;
            carRoot.position = Cell(p); carRoot.rotation = Facing(heading);
            while (!track.Finish(p))
            {
                Vector2Int next = p; int nextHeading = heading;
                for (int h = 0; h < 4; h++)
                {
                    var n = p + Track.Dir(h);
                    if (track.Road(n) && track.Distance(n) == track.Distance(p) - 1) { next = n; nextHeading = h; break; }
                }
                if (next == p) break;
                var from = Cell(p); var to = Cell(next);
                var r0 = carRoot.rotation; var r1 = Facing(nextHeading);
                float t = 0, d = 0.22f;
                while (t < d) { t += Time.deltaTime; float k = Mathf.Clamp01(t / d); carRoot.position = Vector3.Lerp(from, to, k); carRoot.rotation = Quaternion.Slerp(r0, r1, Mathf.Min(1, k * 3)); yield return null; }
                p = next; heading = nextHeading;
                Pose(p, p, heading, 2, 1f, Driving.Sense(track, new Car { pos = p, heading = heading, speed = 2 }), new[] { true, true, true, true, false });
            }
            yield return new WaitForSeconds(1.2f);
        }
    }
}
