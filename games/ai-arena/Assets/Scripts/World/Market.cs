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
public class Market : MonoBehaviour
{
    public enum View { Market, RobotsEye }

    public MarketCamera cam;                 // on the scene's camera; set by App
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
    readonly System.Random dice = new System.Random(3); // for the look only; the arena's own is the game's

    class Stall { public Vector2Int cell; public ItemKind kind; public Transform root, known, unknown; }

    public static Vector3 Cell(Vector2Int p) => new Vector3(p.x - (Size - 1) / 2f, 0, (Size - 1) / 2f - p.y);
    public static Bounds Square => new Bounds(new Vector3(0, 0.5f, 0), new Vector3(Size + 0.5f, 1.2f, Size + 0.5f));

    // Shadows and lights for the whole game; the quality levels in the project
    // settings were made for the flat version.
    public static void Setup()
    {
        QualitySettings.shadows = ShadowQuality.All;
        QualitySettings.shadowResolution = ShadowResolution.High;
        QualitySettings.shadowProjection = ShadowProjection.StableFit;
        QualitySettings.shadowDistance = 60f;
        QualitySettings.shadowCascades = 2;
        QualitySettings.pixelLightCount = 3;
        QualitySettings.antiAliasing = 4;
    }

    // ---- building ----------------------------------------------------------

    public void Build(Arena arena, bool dusk)
    {
        Clear();
        gameObject.SetActive(true);
        this.arena = arena;
        this.dusk = dusk;
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

    // The cover: four robots at dusk, wandering, the camera turning slowly.
    public void ShowCover(RectTransform frame)
    {
        var rng = new System.Random(5);
        var configs = new List<AgentConfig>();
        for (int i = 0; i < 4; i++) configs.Add(new AgentConfig { name = Theme.TeamNames[i], colour = i });
        Build(new Arena(Brief.Make(0, rng), configs, 30_000, 21), true);
        this.frame = frame;
        Canvas.ForceUpdateCanvases();
        cam.Orbit(frame, Square.center);
        cam.Snap();
        wandering = StartCoroutine(Wander());
    }

    public void Clear()
    {
        StopAllCoroutines();
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
        gameObject.SetActive(false);
        if (cam != null) cam.Sleep();
    }

    void BuildGround()
    {
        ground = new GameObject("Ground").transform;
        ground.SetParent(transform, false);
        // The park: grass to the horizon, the square a step above it with a kerb.
        NoShadow(MeshKit.Part(ground, "grass", MeshKit.Box, Look.Mat(Look.Grass), new Vector3(0, -0.3f, 0), new Vector3(120, 0.2f, 120)));
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

    void BuildLight()
    {
        var sunGo = new GameObject("Sun");
        sunGo.transform.SetParent(transform, false);
        var sun = sunGo.AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.shadows = LightShadows.Soft;
        sun.shadowStrength = 0.55f;
        sun.shadowBias = 0.05f;
        sun.shadowNormalBias = 0.4f;

        var dome = MeshKit.Part(transform, "sky", MeshKit.Sphere, null, Vector3.zero, Vector3.one * 300f);
        NoShadow(dome);
        var domeRenderer = dome.GetComponent<Renderer>();
        domeRenderer.receiveShadows = false;

        var camera = cam.GetComponent<Camera>();
        if (dusk)
        {
            sun.transform.rotation = Quaternion.Euler(16, 55, 0);
            sun.color = Look.SunDusk;
            sun.intensity = 0.8f;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = Theme.Hex("#2A3F6B");
            RenderSettings.ambientEquatorColor = Theme.Hex("#6B4A5E");
            RenderSettings.ambientGroundColor = Theme.Hex("#1A2433");
            RenderSettings.fogColor = Theme.Hex("#2B3E52");
            domeRenderer.sharedMaterial = Look.Sky(Look.DuskTop, Look.DuskHorizon, Look.DuskBottom);
            camera.backgroundColor = Look.DuskHorizon;
        }
        else
        {
            sun.transform.rotation = Quaternion.Euler(52, -32, 0);
            sun.color = Look.SunDay;
            sun.intensity = 0.7f;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = Theme.Hex("#6B8CA6");
            RenderSettings.ambientEquatorColor = Theme.Hex("#8E877A");
            RenderSettings.ambientGroundColor = Theme.Hex("#4F5F4D");
            RenderSettings.fogColor = Look.Haze;
            domeRenderer.sharedMaterial = Look.Sky(Look.SkyTop, Look.SkyHorizon, Look.SkyBottom);
            camera.backgroundColor = Look.SkyHorizon;
        }
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 40f;
        RenderSettings.fogEndDistance = 140f;
    }

    // A robot's house: just off the square, behind its home cell, facing in.
    void BuildHouse(AgentState a)
    {
        var c = Theme.Team[a.config.colour];
        var h = a.home;
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

        var k = s.known;
        MeshKit.Part(k, "cloth", MeshKit.Box, Look.Mat(Look.Cloth), new Vector3(0, 0.18f, 0.02f), new Vector3(0.84f, 0.36f, 0.56f));
        MeshKit.Part(k, "table", MeshKit.Box, Look.Mat(Look.Wood), new Vector3(0, 0.4f, 0.02f), new Vector3(0.92f, 0.08f, 0.64f));
        MeshKit.Part(k, "post", MeshKit.Cylinder, Look.Mat(Look.WoodDark), new Vector3(-0.43f, 0.62f, 0.32f), new Vector3(0.06f, 1.24f, 0.06f));
        MeshKit.Part(k, "post", MeshKit.Cylinder, Look.Mat(Look.WoodDark), new Vector3(0.43f, 0.62f, 0.32f), new Vector3(0.06f, 1.24f, 0.06f));
        MeshKit.Part(k, "bar", MeshKit.Cylinder, Look.Mat(Look.WoodDark), new Vector3(0, 1.24f, 0.32f), new Vector3(0, 0, 90), new Vector3(0.04f, 0.92f, 0.04f));
        for (int i = 0; i < 5; i++)
            MeshKit.Part(k, "banner", MeshKit.Box, Look.Mat(i % 2 == 0 ? Look.Awning : Look.AwningWhite), new Vector3(-0.352f + 0.176f * i, 0.97f, 0.33f), new Vector3(12, 0, 0), new Vector3(0.176f, 0.5f, 0.04f));
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
            var lamp = MeshKit.Part(k, "lamp", MeshKit.Sphere, Look.Mat(Look.Lamp, Look.Lamp), new Vector3(0, 1.08f, 0.1f), 0.11f);
            MeshKit.Part(k, "cord", MeshKit.Cylinder, Look.Mat(Look.Dark), new Vector3(0, 1.17f, 0.1f), new Vector3(0.015f, 0.1f, 0.015f));
            if (r.Next(3) == 0)
            {
                var light = lamp.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = Look.Lamp;
                light.range = 2.8f;
                light.intensity = 1.6f;
            }
        }

        var u = s.unknown;
        MeshKit.Part(u, "crate", MeshKit.Box, Look.Mat(Look.Unknown), new Vector3(0, 0.26f, 0), new Vector3(0.66f, 0.52f, 0.62f));
        MeshKit.Part(u, "lid", MeshKit.Box, Look.Mat(Look.UnknownDark), new Vector3(0, 0.54f, 0), new Vector3(0.72f, 0.06f, 0.68f));
        var mark = new GameObject("?");
        mark.transform.SetParent(u, false);
        mark.transform.localPosition = new Vector3(0, 0.98f, 0);
        var text = mark.AddComponent<TextMeshPro>();
        text.font = Theme.Bold;
        text.text = "?";
        text.fontSize = 6;
        text.color = Look.GreyDark;
        text.alignment = TextAlignmentOptions.Center;
        text.rectTransform.sizeDelta = new Vector2(1, 1);
        mark.AddComponent<Billboard>().target = cam.transform;
        u.gameObject.SetActive(false);

        stalls[cell] = s;
        return s;
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
        if (view == View.RobotsEye && a != null && bots.TryGetValue(a, out var r)) cam.Follow(r.root, frame);
    }

    public void SetView(View v)
    {
        view = v;
        if (v == View.RobotsEye && Highlighted != null && bots.TryGetValue(Highlighted, out var r)) cam.Follow(r.root, frame);
        else { view = View.Market; cam.Overview(frame, Square); }
        ApplyFog();
    }

    // The square the chosen robot can see, on the ground around it.
    void UpdateVision()
    {
        var a = Highlighted;
        if (a == null || !a.alive || vision == null) { if (vision != null) vision.gameObject.SetActive(false); return; }
        var c = Theme.Team[a.config.colour];
        float w = 2 * a.config.vision + 1 - 0.06f;
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
            t.localScale = alongX ? new Vector3(w, 0.012f, 0.05f) : new Vector3(0.05f, 0.012f, w);
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
                if (m.to != m.from) r.Bob(k);
            }
            yield return null;
        }
        foreach (var m in moves) { bots[m.agent].Position = to[m.agent]; bots[m.agent].Rest(); }
        foreach (var m in moves)
        {
            if (m.ate == null) continue;
            if (m.agent.alive) { Sfx.Buy(); StartCoroutine(Buy(m)); }
            else { Sfx.Ruin(); StartCoroutine(Out(m.agent)); }
        }
        RefreshItems();
        RefreshBots(false);
    }

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
        StartCoroutine(Puff(r.Position + Vector3.up * 0.6f, Theme.Danger));
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

    // A burst of little balls, for a dinner ruined.
    IEnumerator Puff(Vector3 at, Color colour)
    {
        var bits = new List<(Transform t, Vector3 v)>();
        var r = dice;
        for (int i = 0; i < 12; i++)
        {
            var bit = MeshKit.Part(transform, "puff", MeshKit.Sphere, Look.Mat(i % 3 == 0 ? colour : Look.Grey), at, 0.12f).transform;
            var v = new Vector3((float)r.NextDouble() - 0.5f, (float)r.NextDouble() * 0.8f + 0.4f, (float)r.NextDouble() - 0.5f) * 3.2f;
            bits.Add((bit, v));
        }
        float t = 0, d = 0.7f;
        while (t < d)
        {
            t += Time.deltaTime;
            foreach (var (b, v) in bits)
            {
                b.position += (v + Vector3.down * 5f * t) * Time.deltaTime;
                b.localScale = Vector3.one * 0.12f * (1 - t / d);
            }
            yield return null;
        }
        foreach (var (b, _) in bits) Destroy(b.gameObject);
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
            float t = 0, d = 0.8f;
            while (t < d)
            {
                t += Time.deltaTime;
                float k = Mathf.SmoothStep(0, 1, t / d);
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

        // A click on a robot chooses it, unless a card or a button took the click.
        if (Input.GetMouseButtonDown(0) && Picked != null && !(EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()))
        {
            RobotModel best = null;
            float bestDist = 48f;
            Vector2 mouse = Input.mousePosition;
            foreach (var kv in bots)
            {
                if (!kv.Key.alive || !kv.Value.root.gameObject.activeSelf) continue;
                var s = cam.ToScreen(kv.Value.root.position + Vector3.up * 0.55f);
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
