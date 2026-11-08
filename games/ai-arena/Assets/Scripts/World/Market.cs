using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

// The market in three dimensions: a paved square in a park, a stall under every
// item, each robot's house on the edge, the robots themselves, and the square a
// robot can actually see. It mirrors the Arena (pure state) the way BoardView
// drew it flat: Build() for an arena, Refresh* as the round goes on, Animate()
// for a turn's moves. Two ways of looking: the whole market from above, or what
// one robot sees (RobotsEye), from behind it, where every stall beyond its sight
// is a grey box, because for the robot it does not exist. On the cover the same
// square is shown at dusk, with the robots wandering.
//
// The realism pass (Docs/realism-report.md): when the market kit is in Resources/Market
// (AI Arena > Build market kit), the square is real: cobbles and sandstone, a ring of houses,
// a fountain, lamps, the Higgsfield stalls heaped with Kenney's and Poly Haven's produce, the
// rigged agent; a cell is 2.4 m and the main view is the street, behind the chosen agent.
// Without the kit everything below falls back to the boxes it was made of before.
public partial class Market : MonoBehaviour
{
    public enum View { Market, RobotsEye, Street }

    public MarketCamera cam;                 // on the scene's camera; set by App
    public CameraRig rig;                    // the walk behind an agent, the countdown, the shake; set by App
    Effects effects;
    Transform beatRing;
    Renderer beatRenderer;
    public RectTransform frame;              // where on the stage the market sits; set by the screen
    public Arena arena;
    public View view = View.Market;
    public AgentState Highlighted { get; private set; }
    public bool thinking;                    // the robots are asking Claude: the antennas pulse
    public event Action<AgentState> Picked;  // a robot was clicked

    const int Size = Arena.Size;
    Transform ground, stallsRoot, botsRoot;
    bool dusk;
    readonly Dictionary<Vector2Int, Stall> stalls = new Dictionary<Vector2Int, Stall>();
    readonly Dictionary<AgentState, RobotModel> bots = new Dictionary<AgentState, RobotModel>();
    Transform vision;
    Renderer visionFill;
    readonly Renderer[] visionEdges = new Renderer[4];
    Coroutine wandering;
    Light[] lamps;
    readonly System.Random dice = new System.Random(3); // for the look only; the arena's own is the game's

    class Stall { public Vector2Int cell; public ItemKind kind; public Transform root, known, unknown, counter; }

    public const float KitPitch = 2.4f;      // one cell, in metres, in the kit's square (MarketKit.Pitch)
    static GameObject kitSquare;
    static bool kitChecked;
    public static GameObject Kit(string name) => Resources.Load<GameObject>("Market/" + name);
    public static bool Real { get { if (!kitChecked) { kitChecked = true; kitSquare = Kit("Square"); } return kitSquare != null; } }
    public static float Pitch => Real ? KitPitch : 1f;

    public static Vector3 Cell(Vector2Int p) => new Vector3(p.x - (Size - 1) / 2f, 0, (Size - 1) / 2f - p.y) * Pitch;
    public static Bounds Square => Real ? new Bounds(new Vector3(0, 1.0f, 0), new Vector3(Size * KitPitch + 1f, 2.2f, Size * KitPitch + 1f))
                                        : new Bounds(new Vector3(0, 0.5f, 0), new Vector3(Size + 0.5f, 1.2f, Size + 0.5f));

    // Shadows and lights for the whole game; the quality levels in the project
    // settings were made for the flat version.
    public static void Setup() { } // shadows and anti-aliasing come from the pipeline asset (AI Arena > Set up URP) and the camera's Atmosphere

    // ---- building ----------------------------------------------------------

    public void Build(Arena arena, bool dusk)
    {
        Clear();
        gameObject.SetActive(true);
        Sfx.Ambient(true);
        this.arena = arena;
        this.dusk = dusk;
        if (effects == null) { effects = new GameObject("Effects").AddComponent<Effects>(); effects.transform.SetParent(transform.parent, false); }
        BuildGround();
        BuildLight();
        stallsRoot = new GameObject("Stalls").transform; stallsRoot.SetParent(transform, false);
        botsRoot = new GameObject("Robots").transform; botsRoot.SetParent(transform, false);
        foreach (var a in arena.agents) { BuildHouse(a); bots[a] = new RobotModel(botsRoot, a, dusk); }
        BuildVision();
        RefreshItems();
        RefreshBots(true);
    }

    // The market of the round, in daylight, fitted into the frame.
    public void ShowMarket(RectTransform frame)
    {
        this.frame = frame;
        view = View.Market;
        Canvas.ForceUpdateCanvases(); // the frame's place on the screen is known before the first picture
        cam.Overview(frame, Square);
        cam.Snap();
        ApplyFog();
    }

    // The main view of a round in the real square: at street level behind the chosen agent.
    public void ShowStreet(RectTransform frame)
    {
        this.frame = frame;
        view = View.Street;
        UpdateVision();
        Canvas.ForceUpdateCanvases();
        var a = Highlighted ?? (arena != null && arena.agents.Count > 0 ? arena.agents[0] : null);
        if (a != null && bots.TryGetValue(a, out var r)) cam.Follow(r.root, frame); else cam.Overview(frame, Square);
        cam.Snap();
        ApplyFog();
    }

    // The agent's body (its skinned meshes), for the realism pass's coverage measure.
    public Transform BodyOf(AgentState a) => a != null && bots.TryGetValue(a, out var r) ? r.body : null;

    // Is a point inside a stall? The street camera keeps out of them.
    public bool InsideStall(Vector3 p)
    {
        if (!Real) return false;
        foreach (var s in stalls.Values)
        {
            if (s.root == null) continue;
            var d = p - s.root.position;
            if (Mathf.Abs(d.x) < 1.2f && Mathf.Abs(d.z) < 0.9f && p.y < 2.4f) return true;
        }
        return false;
    }

    // The lists screen: one robot up close, for its dials to show on it.
    public void ShowPortrait(RectTransform frame, AgentState a)
    {
        this.frame = frame;
        view = View.Market;
        Canvas.ForceUpdateCanvases();
        Highlight(a);
        if (bots.TryGetValue(a, out var r)) { cam.Portrait(r.root, frame); cam.Snap(); }
    }

    // The robot built again, after its colour changed.
    public void RefreshRobot(AgentState a)
    {
        if (!bots.TryGetValue(a, out var old)) return;
        old.Destroy();
        var r = bots[a] = new RobotModel(botsRoot, a, dusk);
        r.Position = Cell(a.pos);
        r.root.rotation = RobotModel.Facing(a.facing);
        r.Wear(a.config);
        if (Highlighted == a && cam.mode == MarketCamera.Mode.Portrait) cam.Portrait(r.root, frame);
        else if (Highlighted == a && cam.mode == MarketCamera.Mode.Follow) cam.Follow(r.root, frame);
        UpdateVision();
        ApplyFog();
    }

    // The robot's dials, on the robot.
    public void Dress(AgentState a)
    {
        if (bots.TryGetValue(a, out var r)) r.Wear(a.config);
        UpdateVision();
    }

    // A point on a robot's part, on the screen in pixels: 0 the antenna, 1 the eyes, 2 its
    // back. False when it is behind the camera.
    public bool PartOnScreen(AgentState a, int part, out Vector3 screen)
    {
        screen = Vector3.zero;
        if (!bots.TryGetValue(a, out var r) || !r.root.gameObject.activeSelf) return false;
        screen = cam.ToScreen(part == 0 ? r.AntennaPoint : part == 1 ? r.EyePoint : r.BackPoint);
        return screen.z > 0;
    }

    // A dial moved: the robot pops, so the change is seen on it.
    public void Punch(AgentState a) { if (bots.TryGetValue(a, out var r)) StartCoroutine(DoPunch(r.body)); }
    IEnumerator DoPunch(Transform t)
    {
        float k = 0f;
        while (k < 1f)
        {
            if (t == null) yield break;
            k += Time.unscaledDeltaTime * 4f;
            t.localScale = Vector3.one * (1f + 0.14f * Mathf.Sin(Mathf.Clamp01(k) * Mathf.PI));
            yield return null;
        }
        if (t != null) t.localScale = Vector3.one;
    }

    // The cover: four robots at dusk, wandering, the camera turning slowly. With hero (the
    // title, in the real square) the first of them stands close, among the lit stalls, the way
    // Claude Bomb's title shows its device, the others strolling behind it.
    AgentState coverHero;
    public void ShowCover(RectTransform frame, bool hero = false, AgentConfig heroConfig = null)
    {
        var rng = new System.Random(5);
        var configs = new List<AgentConfig>();
        for (int i = 0; i < 4; i++) configs.Add(new AgentConfig { name = Theme.TeamNames[i], colour = i });
        if (heroConfig != null) { configs[0] = heroConfig.Clone(); for (int i = 1; i < 4; i++) if (configs[i].colour == heroConfig.colour) configs[i].colour = (heroConfig.colour + i * 3) % Theme.Team.Length; }
        var cover = new Arena(Brief.Make(0, rng), configs, 30_000, 21);
        coverHero = null;
        if (Real && hero)
        {
            coverHero = cover.agents[0];
            coverHero.pos = new Vector2Int(4, 2); coverHero.facing = 2;
            for (int x = 3; x <= 5; x++)
                for (int y = 2; y <= 4; y++)
                    if (x == 4 || y > 2) cover.RemoveItem(new Vector2Int(x, y)); // the camera's way in; a stall either side of it
        }
        Build(cover, true);
        this.frame = frame;
        Canvas.ForceUpdateCanvases();
        if (coverHero != null && bots.TryGetValue(coverHero, out var r))
        {
            view = View.Street;
            Highlight(coverHero);
            foreach (var s in stalls.Values)
                if (Distance(s.cell, coverHero.pos) <= 2)
                {
                    var lantern = s.known.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == "Lantern");
                    if (lantern != null) { lantern.gameObject.SetActive(true); var l = lantern.GetComponentInChildren<Light>(true); if (l != null) l.enabled = true; }
                }
            cam.Portrait(r.root, frame);
        }
        else cam.Orbit(frame, Square.center);
        cam.Snap();
        wandering = StartCoroutine(Wander());
    }

    // The square turning slowly in a frame, as built: the debrief's window.
    public void ShowOrbitOf(RectTransform frame)
    {
        this.frame = frame;
        view = View.Market;
        Canvas.ForceUpdateCanvases();
        cam.Orbit(frame, Square.center);
        cam.Snap();
        ApplyFog();
    }

    public AgentState CoverHero => coverHero;

    public void Clear()
    {
        StopAllCoroutines();
        ClearGarage();
        batteries.Clear();
        podium = null; familySpot = null; familyFor = null;
        wandering = null;
        thinking = false;
        Highlighted = null;
        foreach (var r in bots.Values) r.Destroy();
        bots.Clear();
        stalls.Clear();
        for (int i = transform.childCount - 1; i >= 0; i--) Destroy(transform.GetChild(i).gameObject);
    }

    public void Hide()
    {
        Clear();
        Sfx.Ambient(false);
        if (rig != null) rig.Release();
        gameObject.SetActive(false);
        if (cam != null) { cam.Sleep(); var c = cam.GetComponent<Camera>(); c.clearFlags = CameraClearFlags.SolidColor; }
    }

    void BuildGround()
    {
        ground = new GameObject("Ground").transform;
        ground.SetParent(transform, false);
        if (Real)
        {
            var square = Instantiate(kitSquare, ground, false);
            square.name = "Square";
            lamps = square.GetComponentsInChildren<Light>(true);
            foreach (var l in lamps) l.enabled = dusk;
            return;
        }
        // The park: grass to the horizon, the square a step above it with a kerb.
        NoShadow(MeshKit.Part(ground, "grass", MeshKit.Box, Look.Mat(Look.Grass), new Vector3(0, -0.3f, 0), new Vector3(120, 0.2f, 120)));
        NoShadow(MeshKit.Part(ground, "verge", MeshKit.Box, Look.Mat(Look.GrassDark), new Vector3(0, -0.29f, 0), new Vector3(Size + 5f, 0.2f, Size + 5f)));
        MeshKit.Part(ground, "kerb", MeshKit.Box, Look.Mat(Look.Kerb), new Vector3(0, -0.16f, 0), new Vector3(Size + 2.2f, 0.12f, Size + 2.2f));
        MeshKit.Part(ground, "plaza", MeshKit.Box, Look.Mat(Look.Plaza), new Vector3(0, -0.05f, 0), new Vector3(Size + 1.4f, 0.1f, Size + 1.4f));
        for (int x = 0; x < Size; x++)
            for (int y = 0; y < Size; y++)
                NoShadow(MeshKit.Part(ground, "tile", MeshKit.Box, Look.Mat((x + y) % 2 == 0 ? Look.TileA : Look.TileB), Cell(new Vector2Int(x, y)) + Vector3.up * 0.03f, new Vector3(0.9f, 0.06f, 0.9f)));

        // Trees and bushes around, fewer on the south side where the camera stands.
        var r = new System.Random(11);
        for (int i = 0; i < 30; i++)
        {
            float angle = (float)(r.NextDouble() * 360), radius = 7.6f + (float)r.NextDouble() * 3.2f;
            if (angle > 125 && angle < 235 && radius < 9.6f) continue;
            var pos = new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad) * radius, -0.2f, Mathf.Cos(angle * Mathf.Deg2Rad) * radius);
            float s = 0.8f + (float)r.NextDouble() * 0.5f;
            var tree = new GameObject("tree").transform;
            tree.SetParent(ground, false);
            tree.localPosition = pos;
            tree.localScale = Vector3.one * s;
            MeshKit.Part(tree, "trunk", MeshKit.Cylinder, Look.Mat(Look.Trunk), new Vector3(0, 0.35f, 0), new Vector3(0.18f, 0.7f, 0.18f));
            MeshKit.Part(tree, "crown", MeshKit.Sphere, Look.Mat(i % 2 == 0 ? Look.Leaf : Look.LeafDark), new Vector3(0, 1.2f, 0), new Vector3(1.3f, 1.2f, 1.3f));
            MeshKit.Part(tree, "crown", MeshKit.Sphere, Look.Mat(i % 2 == 0 ? Look.LeafDark : Look.Leaf), new Vector3(0.35f, 1.5f, 0.2f), new Vector3(0.8f, 0.75f, 0.8f));
        }
        for (int i = 0; i < 18; i++)
        {
            float angle = (float)(r.NextDouble() * 360), radius = 6.6f + (float)r.NextDouble() * 0.9f;
            if (angle > 140 && angle < 220) continue;
            var pos = new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad) * radius, -0.2f, Mathf.Cos(angle * Mathf.Deg2Rad) * radius);
            MeshKit.Part(ground, "bush", MeshKit.Sphere, Look.Mat(Look.Bush), pos, new Vector3(0.7f, 0.5f, 0.7f));
        }
    }

    // The light story, from Docs/art-bible.md. Day: a warm late-morning key from the camera's
    // right, full soft shadows, a gradient ambient that keeps the shadow sides cool, haze that
    // deepens with distance. Dusk (the title, the story, the cover): the sun low and amber,
    // the brand's ink sky, every stall's lamp lit.
    void BuildLight()
    {
        var sunGo = new GameObject("Sun");
        sunGo.transform.SetParent(transform, false);
        var sun = sunGo.AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.shadows = LightShadows.Soft;
        sun.shadowStrength = dusk ? 0.75f : 0.85f;
        if (Real) { KitLight(sun); return; }

        var dome = MeshKit.Part(transform, "sky", MeshKit.Sphere, null, Vector3.zero, Vector3.one * 300f);
        NoShadow(dome);
        var domeRenderer = dome.GetComponent<Renderer>();
        domeRenderer.receiveShadows = false;

        var camera = cam.GetComponent<Camera>();
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        if (dusk)
        {
            sun.transform.rotation = Quaternion.Euler(16, 55, 0); sun.color = Look.SunDusk; sun.intensity = 0.9f;
            RenderSettings.ambientSkyColor = Theme.Hex("#2A3F6B"); RenderSettings.ambientEquatorColor = Theme.Hex("#6B4A5E"); RenderSettings.ambientGroundColor = Theme.Hex("#1A2433");
            RenderSettings.fogColor = Theme.Hex("#2B3E52");
            domeRenderer.sharedMaterial = Look.Sky(Look.DuskTop, Look.DuskHorizon, Look.DuskBottom);
            camera.backgroundColor = Look.DuskHorizon;
            Look.SetSky(Look.DuskTop, Look.DuskHorizon);
        }
        else
        {
            sun.transform.rotation = Quaternion.Euler(42, -55, 0); sun.color = Look.SunDay; sun.intensity = 1.15f;
            RenderSettings.ambientSkyColor = Theme.Hex("#8FB6D8"); RenderSettings.ambientEquatorColor = Theme.Hex("#C9C2B4"); RenderSettings.ambientGroundColor = Theme.Hex("#4A5A6B");
            RenderSettings.fogColor = Look.Haze;
            domeRenderer.sharedMaterial = Look.Sky(Look.SkyTop, Look.SkyHorizon, Look.SkyBottom);
            camera.backgroundColor = Look.SkyHorizon;
            Look.SetSky(Look.SkyTop, Look.SkyHorizon);
        }
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 30f;
        RenderSettings.fogEndDistance = 120f;
    }

    // The real square's light, Claude Bomb's recipe outdoors: one warm key with soft shadows,
    // a cool sky ambient, a procedural sky, exponential-squared haze that thickens for the
    // street and thins for the overview (Update), the lamps and lanterns lit at dusk.
    void KitLight(Light sun)
    {
        var camera = cam.GetComponent<Camera>();
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.sun = sun;
        if (dusk)
        {
            // Evening: a cool moon key, warm lamps and lanterns, the houses sinking into ink haze
            // (Claude Bomb's night colour, a little warmer).
            sun.transform.rotation = Quaternion.Euler(38, 150, 0); sun.color = new Color(0.62f, 0.7f, 1f); sun.intensity = 0.45f;
            RenderSettings.ambientSkyColor = new Color(0.16f, 0.2f, 0.36f); RenderSettings.ambientEquatorColor = new Color(0.2f, 0.17f, 0.24f); RenderSettings.ambientGroundColor = new Color(0.07f, 0.08f, 0.12f);
            RenderSettings.fogColor = new Color(0.06f, 0.08f, 0.16f);
            RenderSettings.skybox = null;
        }
        else
        {
            sun.transform.rotation = Quaternion.Euler(42, -55, 0); sun.color = new Color(1f, 0.89f, 0.75f); sun.intensity = 1.6f;
            RenderSettings.ambientSkyColor = new Color(0.62f, 0.72f, 0.86f); RenderSettings.ambientEquatorColor = new Color(0.66f, 0.62f, 0.56f); RenderSettings.ambientGroundColor = new Color(0.3f, 0.33f, 0.38f);
            RenderSettings.fogColor = new Color(0.8f, 0.84f, 0.88f);
            RenderSettings.skybox = Resources.Load<Material>("Market/SkyDay");
        }
        camera.clearFlags = RenderSettings.skybox != null ? CameraClearFlags.Skybox : CameraClearFlags.SolidColor;
        camera.backgroundColor = RenderSettings.fogColor;
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = FogFor(cam.mode);
        DynamicGI.UpdateEnvironment();
    }

    float FogFor(MarketCamera.Mode m) => dusk ? (m == MarketCamera.Mode.Overview ? 0.012f : 0.034f)
                                              : (m == MarketCamera.Mode.Overview || m == MarketCamera.Mode.Orbit ? 0.0055f : 0.013f);

    // A robot's house: just off the square, behind its home cell, facing in.
    void BuildHouse(AgentState a)
    {
        var c = Theme.Team[a.config.colour];
        var h = a.home;
        if (Real)
        {
            // The real square's houses ring it already: the agent's home is a small mat in its
            // colour, under its feet only; the square round it keeps its own stone.
            var stone = Instantiate(Kit("HomeStone"), ground, false);
            stone.name = "Home " + a.Name;
            stone.transform.localPosition = Cell(h);
            stone.transform.localRotation = Quaternion.LookRotation(Inward(h));
            stone.transform.localScale = new Vector3(0.42f, 1f, 0.3f); // 0.8 m by 0.57 m
            foreach (var r in stone.GetComponentsInChildren<Renderer>()) r.material.SetColor("_BaseColor", Color.Lerp(Color.white, c, 0.6f));
            return;
        }
        var outward = h.y == 0 ? new Vector2Int(0, -1) : h.x == Size - 1 ? new Vector2Int(1, 0) : h.y == Size - 1 ? new Vector2Int(0, 1) : new Vector2Int(-1, 0);
        var root = new GameObject("House " + a.Name).transform;
        root.SetParent(ground, false);
        var pos = Cell(h) + (Cell(h + outward) - Cell(h)) * 1.45f;
        pos.y = -0.1f;
        root.localPosition = pos;
        root.localRotation = Quaternion.LookRotation(new Vector3(-outward.x, 0, outward.y));
        MeshKit.Part(root, "walls", MeshKit.Box, Look.Mat(Look.Lighter(c, 0.5f)), new Vector3(0, 0.25f, 0), new Vector3(0.78f, 0.5f, 0.66f));
        MeshKit.Part(root, "roof", MeshKit.Pyramid, Look.Mat(Look.Darker(c, 0.3f)), new Vector3(0, 0.5f, 0), new Vector3(0.94f, 0.44f, 0.82f));
        MeshKit.Part(root, "door", MeshKit.Box, Look.Mat(Look.Dark), new Vector3(0, 0.15f, 0.34f), new Vector3(0.18f, 0.3f, 0.03f));
        MeshKit.Part(root, "window", MeshKit.Box, Look.Mat(dusk ? Look.Lamp : Look.Eye, dusk ? Look.Lamp * 0.7f : Color.black), new Vector3(0.22f, 0.32f, 0.34f), new Vector3(0.14f, 0.12f, 0.03f));
        MeshKit.Part(root, "chimney", MeshKit.Box, Look.Mat(Look.Darker(c, 0.45f)), new Vector3(-0.24f, 0.66f, -0.14f), new Vector3(0.1f, 0.22f, 0.1f));
        // The home cell, tinted.
        NoShadow(MeshKit.Part(ground, "home", MeshKit.Box, Look.Glass(new Color(c.r, c.g, c.b, 0.3f)), Cell(h) + Vector3.up * 0.065f, new Vector3(0.9f, 0.01f, 0.9f)));
    }

    void BuildVision()
    {
        vision = new GameObject("Vision").transform;
        vision.SetParent(transform, false);
        visionFill = NoShadow(MeshKit.Part(vision, "fill", MeshKit.Box, Look.Glass(Color.clear), new Vector3(0, 0.075f, 0), new Vector3(1, 0.01f, 1))).GetComponent<Renderer>();
        for (int i = 0; i < 4; i++)
            visionEdges[i] = NoShadow(MeshKit.Part(vision, "edge", MeshKit.Box, Look.Glass(Color.clear), Vector3.zero, Vector3.one)).GetComponent<Renderer>();
        vision.gameObject.SetActive(false);
    }

    static GameObject NoShadow(GameObject go)
    {
        go.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        return go;
    }

    // A stall: a cloth-covered table with the goods on top and a striped banner
    // behind it, so from above the class sees what it sells. Under it, hidden,
    // the grey box it turns into when a robot cannot see it.
    Stall MakeStall(Vector2Int cell, ItemKind kind)
    {
        var s = new Stall { cell = cell, kind = kind };
        s.root = new GameObject("Stall " + kind.id).transform;
        s.root.SetParent(stallsRoot, false);
        s.root.localPosition = Cell(cell) + Vector3.up * 0.06f;
        s.known = new GameObject("Known").transform; s.known.SetParent(s.root, false);
        s.unknown = new GameObject("Unknown").transform; s.unknown.SetParent(s.root, false);
        if (Real) { KitStall(s); stalls[cell] = s; return s; }

        var k = s.known;
        MeshKit.Part(k, "cloth", MeshKit.Box, Look.Mat(Look.Cloth), new Vector3(0, 0.18f, 0.02f), new Vector3(0.84f, 0.36f, 0.56f));
        MeshKit.Part(k, "table", MeshKit.Box, Look.Mat(Look.Wood), new Vector3(0, 0.4f, 0.02f), new Vector3(0.92f, 0.08f, 0.64f));
        MeshKit.Part(k, "post", MeshKit.Cylinder, Look.Mat(Look.WoodDark), new Vector3(-0.43f, 0.62f, 0.32f), new Vector3(0.06f, 1.24f, 0.06f));
        MeshKit.Part(k, "post", MeshKit.Cylinder, Look.Mat(Look.WoodDark), new Vector3(0.43f, 0.62f, 0.32f), new Vector3(0.06f, 1.24f, 0.06f));
        MeshKit.Part(k, "bar", MeshKit.Cylinder, Look.Mat(Look.WoodDark), new Vector3(0, 1.24f, 0.32f), new Vector3(0, 0, 90), new Vector3(0.04f, 0.92f, 0.04f));
        for (int i = 0; i < 5; i++)
            MeshKit.Part(k, "banner", MeshKit.Box, Look.Mat(i % 2 == 0 ? Look.Awning : Look.AwningWhite, Color.black, 0.25f), new Vector3(-0.352f + 0.176f * i, 0.97f, 0.33f), new Vector3(12, 0, 0), new Vector3(0.176f, 0.5f, 0.04f));
        var spots = new[] { new Vector2(-0.24f, 0.0f), new Vector2(0.22f, -0.05f), new Vector2(-0.02f, 0.2f), new Vector2(0.0f, -0.18f) };
        int pieces = ItemModels.Pieces(kind.id);
        var r = dice;
        for (int i = 0; i < pieces && i < spots.Length; i++)
        {
            var piece = ItemModels.Make(kind.id, k).transform;
            piece.localPosition = new Vector3(spots[i].x, 0.44f, spots[i].y);
            piece.localRotation = Quaternion.Euler(0, r.Next(360), 0);
            piece.localScale = Vector3.one * (1.2f + (float)r.NextDouble() * 0.2f);
        }
        if (dusk)
        {
            var lamp = MeshKit.Part(k, "lamp", MeshKit.Sphere, Look.Mat(Look.Lamp, Look.Lamp * 1.6f), new Vector3(0, 1.08f, 0.1f), 0.11f);
            MeshKit.Part(k, "cord", MeshKit.Cylinder, Look.Mat(Look.Dark), new Vector3(0, 1.17f, 0.1f), new Vector3(0.015f, 0.1f, 0.015f));
            if (r.Next(3) == 0)
            {
                var light = lamp.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = Look.Lamp;
                light.range = 3.2f;
                light.intensity = 2.2f;
                light.shadows = LightShadows.None;
            }
        }

        var u = s.unknown;
        MeshKit.Part(u, "crate", MeshKit.Box, Look.Mat(Look.Unknown), new Vector3(0, 0.26f, 0), new Vector3(0.66f, 0.52f, 0.62f));
        MeshKit.Part(u, "lid", MeshKit.Box, Look.Mat(Look.UnknownDark), new Vector3(0, 0.54f, 0), new Vector3(0.72f, 0.06f, 0.68f));
        u.gameObject.SetActive(false);

        stalls[cell] = s;
        return s;
    }

    // A real stall: one of the two Higgsfield stalls, its goods on the counter, facing the
    // middle of the square; at dusk its lantern glows (a third of them light the cobbles).
    // Under it, hidden, the covered crates it becomes when the agent cannot see it.
    void KitStall(Stall s)
    {
        s.root.localPosition = Cell(s.cell);
        int hash = (s.cell.x * 7 + s.cell.y * 13) % 5;
        var stall = Instantiate(Kit(hash == 1 || hash == 3 ? "StallB" : "StallA"), s.known, false);
        float yaw = s.cell.y < Size / 2 ? 180f : 0f;
        stall.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
        s.counter = stall.transform.Find("Counter");
        var goods = Kit("Goods/" + s.kind.id);
        if (goods != null && s.counter != null) Instantiate(goods, s.counter, false);
        var lantern = stall.transform.Find("Lantern");
        if (lantern != null && dusk)
        {
            lantern.gameObject.SetActive(true);
            var l = lantern.GetComponentInChildren<Light>(true);
            if (l != null) l.enabled = dice.Next(3) == 0;
        }
        var crates = Instantiate(Kit("Unknown"), s.unknown, false);
        crates.transform.localRotation = Quaternion.Euler(0f, yaw + 8f, 0f);
        s.unknown.gameObject.SetActive(false);
    }

    // ---- keeping up with the arena -------------------------------------------

    // Stalls appear where items have been put down and are packed up where they
    // were bought.
    public void RefreshItems()
    {
        for (int x = 0; x < Size; x++)
            for (int y = 0; y < Size; y++)
            {
                var cell = new Vector2Int(x, y);
                var kind = arena.items[x, y];
                bool has = stalls.TryGetValue(cell, out var s);
                if (kind != null && !has) StartCoroutine(Pop(MakeStall(cell, kind).root));
                else if (kind == null && has) { stalls.Remove(cell); StartCoroutine(PackUp(s.root)); }
            }
        ApplyFog();
    }

    public void RefreshBots(bool snap)
    {
        foreach (var a in arena.agents)
        {
            var r = bots[a];
            if (snap)
            {
                r.Position = Cell(a.pos);
                r.root.rotation = RobotModel.Facing(a.facing);
                r.Rest();
            }
            r.SetDim(a.alive && a.outOfTokens);
        }
        UpdateVision();
        ApplyFog();
    }

    public void Highlight(AgentState a)
    {
        Highlighted = a;
        UpdateVision();
        ApplyFog();
        if (a == null || !bots.TryGetValue(a, out var r)) return;
        if (view == View.RobotsEye) rig.Chase(r.root, frame);
        else if (view == View.Street && !rig.Live) cam.Follow(r.root, frame);
    }

    // Street: behind the chosen agent at its height (the main view of the real square).
    // Agent's eye: over its shoulder, what it cannot see is not there. Market: from above.
    public void SetView(View v)
    {
        view = v;
        UpdateVision();
        RobotModel r = null;
        if (Highlighted != null) bots.TryGetValue(Highlighted, out r);
        if (v == View.RobotsEye && r != null) rig.Chase(r.root, frame);
        else if (v == View.Street && r != null) { rig.Release(); cam.Follow(r.root, frame); }
        else { view = View.Market; rig.Release(); cam.Overview(frame, Square); }
        ApplyFog();
    }

    // The countdown: the camera flies in from high and wide, down behind the first agent,
    // then back to the round's view as they set out.
    public void ShowCountdown(RectTransform frame, float seconds)
    {
        this.frame = frame;
        var first = arena.agents.Count > 0 && bots.TryGetValue(arena.agents[0], out var r) ? r.root : null;
        if (first != null) rig.Countdown(first, frame, seconds);
    }
    public void BackToOverview()
    {
        if (view == View.Market) { rig.Release(); cam.Overview(frame, Square); }
        else if (view == View.Street && Highlighted != null && bots.TryGetValue(Highlighted, out var r)) { rig.Release(); cam.Follow(r.root, frame); }
    }

    // All home: a slow turn around the square in slow motion, confetti over every house.
    public void Celebrate(System.Action done)
    {
        if (effects != null)
            foreach (var a in arena.agents) if (a.alive && bots.TryGetValue(a, out var r)) effects.Confetti(r.Position + Vector3.up * 1.2f, 24);
        var who = arena.agents.FirstOrDefault(a => a.alive);
        var was = cam.mode; // the orbit of a debrief comes back as an orbit, the market as the overview
        if (who != null && bots.TryGetValue(who, out var hero))
            rig.FinishMoment(hero.root, 2.2f, () => { if (was == MarketCamera.Mode.Orbit) { rig.Release(); cam.Orbit(frame, Square.center); } else BackToOverview(); done?.Invoke(); });
        else done?.Invoke();
    }

    // The loop, visible: a ring in the agent's colour flashes under it as it looks, the
    // antenna is the thinking (Think), the walk is the move (Animate).
    public void Beat(AgentState a)
    {
        if (a == null || !bots.TryGetValue(a, out var r)) return;
        if (beatRing == null)
        {
            var go = NoShadow(MeshKit.Part(transform, "beat", MeshKit.Cylinder, Look.Glass(Color.clear), Vector3.zero, new Vector3(1f, 0.01f, 1f))); // an effect, not the world
            beatRing = go.transform; beatRenderer = go.GetComponent<Renderer>();
        }
        StartCoroutine(Ring(r, Theme.Team[a.config.colour]));
    }

    IEnumerator Ring(RobotModel r, Color c)
    {
        float t = 0, d = 0.5f;
        beatRing.gameObject.SetActive(true);
        while (t < d && r.root != null)
        {
            t += Time.deltaTime;
            float k = t / d;
            beatRing.position = r.Position + Vector3.up * 0.09f;
            beatRing.localScale = new Vector3((0.5f + 1.3f * k) * Pitch, 0.01f, (0.5f + 1.3f * k) * Pitch);
            beatRenderer.sharedMaterial = Look.Glass(new Color(c.r, c.g, c.b, 0.55f * (1 - k)));
            yield return null;
        }
        beatRing.gameObject.SetActive(false);
    }

    // The square the chosen robot can see, on the ground around it.
    void UpdateVision()
    {
        var a = Highlighted;
        if (a == null || !a.alive || vision == null || (Real && view == View.Street) || g != null || walkingHome) { if (vision != null) vision.gameObject.SetActive(false); return; }
        var c = Theme.Team[a.config.colour];
        float w = (2 * a.config.vision + 1) * Pitch - 0.06f;
        vision.gameObject.SetActive(true);
        vision.position = Cell(a.pos);
        visionFill.transform.localScale = new Vector3(w, 0.01f, w);
        visionFill.sharedMaterial = Look.Glass(new Color(c.r, c.g, c.b, 0.2f));
        var edge = Look.Glass(new Color(c.r, c.g, c.b, 0.85f));
        for (int i = 0; i < 4; i++)
        {
            bool alongX = i < 2;
            float side = i % 2 == 0 ? -1 : 1;
            var t = visionEdges[i].transform;
            t.localPosition = alongX ? new Vector3(0, 0.085f, side * w / 2) : new Vector3(side * w / 2, 0.085f, 0);
            t.localScale = alongX ? new Vector3(w, 0.012f, 0.05f * Pitch) : new Vector3(0.05f * Pitch, 0.012f, w);
            visionEdges[i].sharedMaterial = edge;
        }
    }

    static int Distance(Vector2Int a, Vector2Int b) => Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));

    // Through the robot's eyes: what it cannot see is not there.
    void ApplyFog()
    {
        var h = Highlighted;
        bool fog = view == View.RobotsEye && h != null && h.alive;
        foreach (var s in stalls.Values)
        {
            bool known = !fog || Distance(s.cell, h.pos) <= h.config.vision;
            s.known.gameObject.SetActive(known);
            s.unknown.gameObject.SetActive(!known);
        }
        foreach (var kv in bots)
            kv.Value.SetGhost(fog && kv.Key != h && kv.Key.alive && Distance(kv.Key.pos, h.pos) > h.config.vision);
    }

    // ---- a turn's moves --------------------------------------------------------

    // Walks each robot to its new cell, then what it bought hops into its basket,
    // or it keels over and is sent home.
    public IEnumerator Animate(List<Simulation.Move> moves, float duration)
    {
        if (Real) { yield return AnimateKit(moves, duration); yield break; }
        var from = moves.ToDictionary(m => m.agent, m => Cell(m.from));
        var to = moves.ToDictionary(m => m.agent, m => Cell(m.to));
        var fromRot = moves.ToDictionary(m => m.agent, m => bots[m.agent].root.rotation);
        var toRot = moves.ToDictionary(m => m.agent, m => m.to != m.from ? Quaternion.LookRotation(Cell(m.to) - Cell(m.from)) : RobotModel.Facing(m.agent.facing));
        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(0, 1, Mathf.Clamp01(t / duration));
            foreach (var m in moves)
            {
                var r = bots[m.agent];
                r.Position = Vector3.Lerp(from[m.agent], to[m.agent], k);
                r.root.rotation = Quaternion.Slerp(fromRot[m.agent], toRot[m.agent], Mathf.Clamp01(k * 2.5f));
                if (m.to != m.from) { r.Bob(k); if (effects != null && Time.frameCount % 6 == 0) effects.Dust(r.Position + Vector3.up * 0.05f, 1); }
            }
            yield return null;
        }
        foreach (var m in moves) { bots[m.agent].Position = to[m.agent]; bots[m.agent].Rest(); }
        foreach (var m in moves)
        {
            if (m.ate == null) continue;
            if (m.agent.alive) { Sfx.Chime(); StartCoroutine(Buy(m)); if (effects != null) effects.Confetti(bots[m.agent].Position + Vector3.up * 0.9f, 12); }
            else { Sfx.Crash(); StartCoroutine(Out(m.agent)); }
        }
        RefreshItems();
        RefreshBots(false);
    }


    // ---- a turn's moves in the real square ------------------------------------------

    // The simulation waits for this before the next turn: a real walk takes real time.
    public bool Animating { get; private set; }
    public static float StepSeconds(float speed) => Real ? 5.6f / Mathf.Max(0.5f, speed) : 0.45f / Mathf.Max(0.5f, speed);

    // Every agent walks its step at once: the walk clip, then, for a buy, it stops at the
    // stall, reaches for the goods (the pick-up clip), the piece flies into its basket and the
    // stall packs up; a buy that ruins dinner ends in the sent-home clip.
    IEnumerator AnimateKit(List<Simulation.Move> moves, float duration)
    {
        Animating = true;
        int running = 0;
        foreach (var m in moves) { running++; StartCoroutine(Step(m, duration, () => running--)); }
        float waited = 0f;
        while (running > 0 && waited < 20f) { waited += Time.deltaTime; yield return null; }
        RefreshItems();
        RefreshBots(false);
        Animating = false;
    }

    IEnumerator Step(Simulation.Move m, float duration, System.Action done)
    {
        var r = bots[m.agent];
        var from = Cell(m.from); var to = Cell(m.to);
        var rot0 = r.root.rotation;
        var rot1 = m.to != m.from ? Quaternion.LookRotation(to - from) : RobotModel.Facing(m.agent.facing);
        bool buys = m.ate != null;
        float reach = buys ? 0.36f : 1f; // a buyer stops in front of the stall
        if (m.to != m.from)
        {
            r.Walk(Pitch / duration / 1.15f);
            float t = 0, d = duration * reach;
            while (t < d)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / d);
                float e = buys ? Ui.EaseOut(k) : k;
                r.Position = Vector3.Lerp(from, to, reach * e);
                r.root.rotation = Quaternion.Slerp(rot0, rot1, Mathf.Clamp01(k * 4f));
                if (effects != null && Time.frameCount % 9 == 0) effects.Dust(r.Position + Vector3.up * 0.05f, 1);
                yield return null;
            }
        }
        else r.root.rotation = rot1;
        if (buys)
        {
            r.PickUp();
            yield return new WaitForSeconds(0.55f);
            stalls.TryGetValue(m.to, out var stall);
            var start = stall != null && stall.counter != null ? stall.counter.position + Vector3.up * 0.12f : to + Vector3.up * 0.9f;
            var piece = Piece(m.ate.id, start);
            float t = 0, d = 0.5f;
            while (t < d && piece != null)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / d);
                var p = Vector3.Lerp(start, r.BasketPoint, k);
                p.y += Mathf.Sin(k * Mathf.PI) * 0.45f;
                piece.position = p;
                piece.Rotate(0f, 400f * Time.deltaTime, 0f, Space.World);
                yield return null;
            }
            if (piece != null) Destroy(piece.gameObject);
            if (stall != null && arena.ItemAt(m.to) == null) { stalls.Remove(m.to); StartCoroutine(PackUp(stall.root)); }
            if (m.agent.alive)
            {
                Sfx.Chime();
                r.AddGoods(m.ate.id, ItemModels.Colour(m.ate.id));
                if (effects != null) effects.Confetti(r.Position + Vector3.up * 1.3f, 12);
                yield return new WaitForSeconds(0.35f);
            }
            // Then onto the stall's cell, where the stall was.
            r.Walk(1f);
            float t2 = 0, d2 = duration * (1f - reach) * 0.8f;
            var at = r.Position;
            while (t2 < d2)
            {
                t2 += Time.deltaTime;
                r.Position = Vector3.Lerp(at, to, Mathf.SmoothStep(0, 1, t2 / d2));
                yield return null;
            }
            r.Position = to;
            if (!m.agent.alive) { Sfx.Crash(); yield return Out(m.agent); done(); yield break; }
        }
        r.Position = to;
        r.Rest();
        done();
    }

    // One piece of an item, for the throw into the basket (a sphere of its colour without the kit's pieces).
    Transform Piece(string id, Vector3 at)
    {
        var prefab = Kit("Pieces/" + id);
        Transform t = prefab != null ? Instantiate(prefab, transform, false).transform
                                     : MeshKit.Part(transform, "bought", MeshKit.Sphere, Look.Mat(ItemModels.Colour(id)), Vector3.zero, 0.12f).transform;
        t.position = at;
        return t;
    }

    // The LOOK of the loop on the chosen agent: it looks around while the stalls are read.
    public void LookAround(AgentState a) { if (a != null && bots.TryGetValue(a, out var r)) r.LookAround(); }

    IEnumerator Buy(Simulation.Move m)
    {
        var colour = ItemModels.Colour(m.ate.id);
        var ball = MeshKit.Part(transform, "bought", MeshKit.Sphere, Look.Mat(colour), Cell(m.to) + Vector3.up * 0.55f, 0.22f).transform;
        var r = bots[m.agent];
        var start = ball.position;
        float t = 0, d = 0.45f;
        while (t < d)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / d);
            var p = Vector3.Lerp(start, r.BasketPoint, k);
            p.y += Mathf.Sin(k * Mathf.PI) * 0.8f;
            ball.position = p;
            yield return null;
        }
        Destroy(ball.gameObject);
        r.AddGoods(colour);
    }

    IEnumerator Out(AgentState a)
    {
        var r = bots[a];
        r.SetOut();
        if (effects != null) effects.Sparks(r.Position + Vector3.up * 0.6f, 1.2f);
        rig.Shake(1f);
        Atmosphere.Current?.Pulse(Theme.Danger, 0.35f, 0.5f);
        Atmosphere.Current?.HitStop(0.07f);
        if (Real)
        {
            // The real agent holds its head: dinner is ruined. Then it is called home.
            r.SentHome();
            yield return new WaitForSeconds(2.1f);
            float s = 0;
            while (s < 0.5f) { s += Time.deltaTime; r.root.localScale = Vector3.one * Mathf.Max(0, 1 - s / 0.5f); yield return null; }
            r.root.gameObject.SetActive(false);
            yield break;
        }
        var rot = r.root.rotation;
        var pos = r.Position;
        float t = 0;
        while (t < 0.5f)
        {
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(0, 1, t / 0.5f);
            r.root.rotation = rot * Quaternion.Euler(0, 0, 82 * k);
            r.Position = pos + Vector3.up * Mathf.Sin(k * Mathf.PI) * 0.35f;
            yield return null;
        }
        r.Position = pos;
        yield return new WaitForSeconds(1.3f);
        t = 0;
        while (t < 0.5f) { t += Time.deltaTime; r.root.localScale = Vector3.one * RobotModel.Scale * Mathf.Max(0, 1 - t / 0.5f); yield return null; }
        r.root.gameObject.SetActive(false);
    }

    IEnumerator Pop(Transform t)
    {
        t.localScale = Vector3.zero;
        float k = 0;
        while (k < 1) { k += Time.deltaTime / 0.3f; if (t == null) yield break; t.localScale = Vector3.one * Ui.EaseOutBack(Mathf.Clamp01(k)); yield return null; }
    }

    IEnumerator PackUp(Transform t)
    {
        float k = 1;
        while (k > 0) { k -= Time.deltaTime / 0.25f; if (t == null) yield break; t.localScale = Vector3.one * Mathf.Max(0, k); yield return null; }
        if (t != null) Destroy(t.gameObject);
    }

    // The cover: the robots stroll about the empty cells.
    IEnumerator Wander()
    {
        var r = dice;
        while (true)
        {
            yield return new WaitForSeconds(0.7f + (float)r.NextDouble() * 0.9f);
            var a = arena.agents[r.Next(arena.agents.Count)];
            if (a == coverHero) continue; // the title's agent stays for its picture
            var options = new List<Vector2Int>();
            foreach (var step in new[] { new Vector2Int(0, -1), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(-1, 0) })
            {
                var p = a.pos + step;
                if (Arena.InBounds(p) && arena.ItemAt(p) == null && arena.AgentAt(p) == null && !arena.agents.Any(o => o != a && o.home == p)) options.Add(p);
            }
            if (options.Count == 0) continue;
            var to = options[r.Next(options.Count)];
            var bot = bots[a];
            var from = Cell(a.pos);
            var rot0 = bot.root.rotation;
            var rot1 = Quaternion.LookRotation(Cell(to) - from);
            a.pos = to;
            float t = 0, d = Real ? 2.2f : 0.8f;
            bot.Walk(Real ? 2.4f / d / 1.15f : 1f);
            while (t < d)
            {
                t += Time.deltaTime;
                float k = Real ? t / d : Mathf.SmoothStep(0, 1, t / d);
                bot.Position = Vector3.Lerp(from, Cell(to), k);
                bot.root.rotation = Quaternion.Slerp(rot0, rot1, Mathf.Clamp01(k * 3));
                bot.Bob(k);
                yield return null;
            }
            bot.Position = Cell(to);
            bot.Rest();
        }
    }

    // ---- every frame ----------------------------------------------------------

    void Update()
    {
        float pulse = thinking ? (Mathf.Sin(Time.time * 7f) + 1) / 2 : 0;
        foreach (var kv in bots) kv.Value.Think(kv.Key.Active ? pulse : 0);
        // The haze: thick enough at street level for the houses to recede, thin from above.
        if (Real && cam != null) RenderSettings.fogDensity = Mathf.Lerp(RenderSettings.fogDensity, FogFor(cam.mode), 1f - Mathf.Exp(-3f * Time.unscaledDeltaTime));

        if (g != null) UpdateGarage();
        // A click on a robot chooses it, unless a card or a button took the click.
        if (g == null && Input.GetMouseButtonDown(0) && Picked != null && !(EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()))
        {
            RobotModel best = null;
            float bestDist = 48f;
            Vector2 mouse = Input.mousePosition;
            foreach (var kv in bots)
            {
                if (!kv.Key.alive || !kv.Value.root.gameObject.activeSelf) continue;
                var s = cam.ToScreen(kv.Value.root.position + Vector3.up * (Real ? 0.7f : 0.55f));
                if (s.z < 0) continue;
                float d = Vector2.Distance(mouse, new Vector2(s.x, s.y));
                if (d < bestDist) { bestDist = d; best = kv.Value; }
            }
            if (best != null) Picked(best.agent);
        }
    }

    // Where a robot is on the screen, in pixels, for the labels; false when it is gone.
    public bool OnScreen(AgentState a, out Vector3 feet, out Vector3 head)
    {
        feet = head = Vector3.zero;
        if (!bots.TryGetValue(a, out var r) || !r.root.gameObject.activeSelf) return false;
        feet = cam.ToScreen(r.Feet);
        head = cam.ToScreen(r.Head);
        return feet.z > 0 && head.z > 0;
    }
}

// Faces the camera: the question mark over a stall the robot cannot see.
public class Billboard : MonoBehaviour
{
    public Transform target;
    void LateUpdate() { if (target != null) transform.rotation = Quaternion.LookRotation(transform.position - target.position); }
}
