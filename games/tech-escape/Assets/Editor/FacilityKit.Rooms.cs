using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

// The twelve rooms. Every room is the same kind of place (a 3.6 × 3.4 m office in the permit
// office, 2.8 m high, in lockdown) and a different room: its own prop, the one thing that says
// what the room is, against the back wall under Claude Bomb's hanging lamp; its name
// stencilled beside it; the board of decision lamps; an amber lockdown beacon turning in the
// corner; and furniture that changes with the room's kind (an office keeps files, a lab keeps
// racks). The camera point is measured from the prop, so each prop fills about the same share
// of the frame as the mail sorter (Docs/claude-bomb-recipe.md: one hero, close).
// Origin: the door (the corridor face of its wall); the room lies towards -Z,
// x from -1.8 to 1.8, z from -3.6 to -0.2.
public static partial class FacilityKit
{
    enum Mount { Floor, Wall, Desk, Screens }

    // In the order of Facility.Rooms.
    static readonly (string room, string hero, string words, bool tech, Mount mount)[] Rooms =
    {
        ("Risk Room", "RiskBoard", "RISK\nROOM", false, Mount.Floor),
        ("Suspicious Inbox", "MailSorter", "POST\nROOM", false, Mount.Floor),
        ("Password Vault", "Safe", "VAULT", false, Mount.Floor),
        ("Malware Containment", "Containment", "LAB\nB2", true, Mount.Floor),
        ("Identity Chamber", "Turnstile", "ID\nCHECK", false, Mount.Floor),
        ("Patch Laboratory", "PatchBench", "PATCH\nLAB", true, Mount.Floor),
        ("Network Maze", "NetworkCabinet", "COMMS", true, Mount.Wall),
        ("Firewall Control", "SwitchConsole", "CONTROL", true, Mount.Floor),
        ("Encryption Chamber", "Lockbox", "KEY\nSTORE", false, Mount.Desk),
        ("Cloud Chamber", "CloudLamp", "CLOUD", true, Mount.Floor),
        ("Operations Center", "MonitorWall", "OPS", true, Mount.Screens),
        ("Backup Vault", "TapeLibrary", "BACKUP", true, Mount.Floor),
    };

    const float X0 = -1.8f, X1 = 1.8f, ZFront = -Wall, ZBack = -3.6f;

    static void BuildRooms(Mats m, Dictionary<string, GameObject> heroes)
    {
        for (int i = 0; i < Rooms.Length; i++)
        {
            var spec = Rooms[i];
            if (spec.mount != Mount.Screens && !heroes.ContainsKey(spec.hero)) { Debug.LogWarning($"[FacilityKit] {spec.room}: no hero model yet ({spec.hero}), room skipped"); continue; }
            BuildRoom(m, heroes, i, spec);
        }
    }

    static void BuildRoom(Mats m, Dictionary<string, GameObject> heroes, int index, (string room, string hero, string words, bool tech, Mount mount) spec)
    {
        string id = spec.room.Replace(" ", "");
        var root = new GameObject("Room_" + id);
        var t = root.transform;
        float w = X1 - X0, d = ZFront - ZBack, zc = (ZFront + ZBack) / 2f;
        bool inbox = spec.room == "Suspicious Inbox";

        // ---- The shell ----
        Part(t, "Floor", spec.tech ? m.labFloor : m.roomFloor, new Vector3(0f, -0.045f, zc), new Vector3(w, 0.1f, d), shadows: false, repeat: spec.tech ? 2.1f : 1.6f);
        var ceiling = new GameObject("Ceiling").transform; ceiling.SetParent(t, false);
        Part(ceiling, "Ceiling", m.ceiling, new Vector3(0f, H + 0.05f, zc), new Vector3(w + 2f * Wall, 0.1f, d + Wall), shadows: false);
        var wallMat = spec.tech ? m.labWall : m.roomWall;
        Part(t, "BackWall", wallMat, new Vector3(0f, H / 2f, ZBack - Wall / 2f), new Vector3(w + 2f * Wall, H, Wall));
        Part(t, "SideWallL", wallMat, new Vector3(X0 - Wall / 2f, H / 2f, zc), new Vector3(Wall, H, d));
        Part(t, "SideWallR", wallMat, new Vector3(X1 + Wall / 2f, H / 2f, zc), new Vector3(Wall, H, d));
        float segW = (w - DoorW) / 2f;
        foreach (int dir in new[] { -1, 1 })
            Part(t, "FrontWall", wallMat, new Vector3(dir * (DoorW / 2f + segW / 2f), H / 2f, ZFront + 0.005f), new Vector3(segW, H, 0.01f), shadows: false);
        Part(t, "FrontLintel", wallMat, new Vector3(0f, DoorH + (H - DoorH) / 2f, ZFront + 0.005f), new Vector3(DoorW, H - DoorH, 0.01f), shadows: false);
        Part(t, "SkirtingBack", m.trim, new Vector3(0f, 0.06f, ZBack + 0.01f), new Vector3(w, 0.12f, 0.02f), shadows: false);
        Part(t, "SkirtingL", m.trim, new Vector3(X0 + 0.01f, 0.06f, zc), new Vector3(0.02f, 0.12f, d), shadows: false);
        Part(t, "SkirtingR", m.trim, new Vector3(X1 - 0.01f, 0.06f, zc), new Vector3(0.02f, 0.12f, d), shadows: false);
        Part(t, "CorniceBack", m.trim, new Vector3(0f, H - 0.04f, ZBack + 0.012f), new Vector3(w, 0.08f, 0.024f), shadows: false);
        // A painted band at desk height on the lab rooms' walls: a lab is not an office.
        if (spec.tech)
        {
            Part(t, "BandBack", m.trim, new Vector3(0f, 1.0f, ZBack + 0.008f), new Vector3(w, 0.06f, 0.016f), shadows: false);
            Part(t, "BandL", m.trim, new Vector3(X0 + 0.008f, 1.0f, zc), new Vector3(0.016f, 0.06f, d), shadows: false);
        }
        Conduit(t, m.metal, new Vector3(X0, 2.02f, ZBack + 0.05f), new Vector3(X1, 2.02f, ZBack + 0.05f), 0.05f);
        Conduit(t, m.metal, new Vector3(X0, 2.12f, ZBack + 0.05f), new Vector3(X1, 2.12f, ZBack + 0.05f), 0.035f);
        Conduit(t, m.metal, new Vector3(X0 + 0.08f, 0.12f, ZBack + 0.06f), new Vector3(X0 + 0.08f, 2.02f, ZBack + 0.06f), 0.05f);

        // ---- The light: the ceiling's tube, the room's lamps; the key set over the hero below ----
        var fitting = Prop("mounted_fluorescent_lights", ceiling, new Vector3(0f, H, zc), 90f, onFloor: false);
        foreach (var r in fitting.GetComponentsInChildren<Renderer>()) r.shadowCastingMode = ShadowCastingMode.Off;
        var lights = new GameObject("Lights").transform; lights.SetParent(t, false);
        AddLight(lights, "Fill", LightType.Point, new Vector3(0f, H - 0.4f, zc), new Color(0.78f, 0.86f, 1f), 0.7f, 5.5f);
        AddLight(lights, "Warm", LightType.Point, new Vector3(-0.9f, 1.9f, -2.2f), new Color(1f, 0.78f, 0.55f), 0.5f, 3.2f);

        // ---- The hero, against the back wall ----
        float hx = -0.42f;
        GameObject hero;
        if (spec.mount == Mount.Screens) hero = MonitorWall(t, m, heroes["Monitor"], new Vector3(hx, 0f, ZBack));
        else if (spec.mount == Mount.Desk)
        {
            var table = Prop("metal_office_desk", t, new Vector3(hx, 0f, ZBack + 0.4f), 0f);
            var tb = Bounds(table);
            table.transform.position += new Vector3(0f, 0f, ZBack + 0.02f - tb.min.z);
            hero = Place(heroes[spec.hero], t, new Vector3(hx + 0.05f, Bounds(table).max.y, ZBack + 0.38f), -8f);
        }
        else
        {
            hero = Place(heroes[spec.hero], t, new Vector3(hx, spec.mount == Mount.Wall ? 0.55f : 0f, ZBack), 0f);
            var b0 = Bounds(hero);
            hero.transform.position += new Vector3(0f, 0f, ZBack + 0.03f - b0.min.z);
        }
        hero.name = "Hero";
        var hb = Bounds(hero);
        // The cloud lamp is a lamp: a cool light inside its glass.
        if (spec.hero == "CloudLamp")
            AddLight(lights, "CloudGlow", LightType.Point, new Vector3(hb.center.x, hb.max.y - 0.18f, hb.center.z + 0.05f), new Color(0.62f, 0.8f, 1f), 1.6f, 2.4f);

        // Claude Bomb's hanging lamp over the hero: its warm pool, its beam, the dust in it.
        var lampAt = new Vector3(hb.center.x, 0f, Mathf.Min(hb.max.z + 0.45f, -1.6f));
        var lampProp = Prop("hanging_industrial_lamp", t, lampAt, 0f, onFloor: false);
        var lb = Bounds(lampProp);
        lampProp.transform.position += Vector3.up * (2.18f - lb.min.y);
        foreach (var r in lampProp.GetComponentsInChildren<Renderer>()) r.shadowCastingMode = ShadowCastingMode.Off;
        var spot = AddLight(lights, "Key", LightType.Spot, new Vector3(lampAt.x, 2.14f, lampAt.z), new Color(1f, 0.86f, 0.7f), 8f, 4.5f);
        spot.spotAngle = 74f; spot.innerSpotAngle = 30f;
        spot.transform.LookAt(t.TransformPoint(new Vector3(hb.center.x, hb.center.y * 0.9f, hb.center.z)));
        Beam(t, new Vector3(lampAt.x, 2.16f, lampAt.z), 0.16f, 0.95f, 2.16f);

        // ---- Where the camera stands, measured from the hero; then what the back wall shows inside that view ----
        var view = root.AddComponent<RoomView>();
        var (camPos, target, fov) = CameraPoint(t, hb);
        view.fov = fov;
        // The part of the back wall the feed shows: the frame is the left two thirds of the screen,
        // centred on the target, so a third of the screen's width either side of it.
        float wallDist = Mathf.Abs(camPos.z - ZBack);
        float halfVis = wallDist * Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad) * (16f / 9f) * (2f / 3f) * 0.9f;
        float visMax = target.x + halfVis, visMin = target.x - halfVis;
        bool twoLines = spec.words.Contains("\n");
        // The board of decision lamps first (the game answers the player on it), then the stencil above it.
        float? bx = null;
        if (hb.max.x + 0.4f <= visMax - 0.36f) bx = hb.max.x + 0.4f;
        else if (hb.min.x - 0.4f >= visMin + 0.36f) bx = hb.min.x - 0.4f;
        var board = new GameObject("Board").transform;
        board.SetParent(t, false);
        // No room beside the prop in this view: the board goes on the right-hand wall, out of the feed (the drawer shows its lamps).
        if (bx.HasValue) board.localPosition = new Vector3(bx.Value, 1.0f, ZBack + 0.012f);
        else { board.localPosition = new Vector3(X1 - 0.012f, 1.45f, -2.95f); board.localRotation = Quaternion.Euler(0f, -90f, 0f); }
        Part(board, "Plate", m.metal, new Vector3(0f, 0f, 0.012f), new Vector3(0.66f, 0.13f, 0.024f), shadows: false);
        if (bx.HasValue) Stencil(t, spec.words, new Vector3(bx.Value, twoLines ? 1.42f : 1.46f, ZBack + 0.006f), spec.words.Length > 7 ? 1.25f : 1.4f, new Vector2(0.8f, 0.45f));
        Beacon(t, m, new Vector3(X1 - 0.3f, 2.42f, ZBack + 0.02f));
        Prop("power_box_01", t, new Vector3(X0 + 0.32f, 1.25f, ZBack + 0.04f), 0f, onFloor: false, scale: 0.8f);
        Prop("wall_clock", t, new Vector3(hb.center.x, 2.38f, ZBack + 0.03f), 0f, onFloor: false, scale: 0.85f);

        // ---- The furniture: the desk and its screen on the right wall, the rest by kind ----
        if (spec.mount != Mount.Desk)
        {
            var desk = Prop("metal_office_desk", t, new Vector3(X1 - 0.42f, 0f, -2.0f), -90f);
            float deskTop = Bounds(desk).max.y;
            var monitor = Place(heroes["Monitor"], t, new Vector3(X1 - 0.28f, deskTop, -2.0f), -90f);
            ScreenOn(t, m, monitor, index, "Screen");
            Place(heroes["Chair"], t, new Vector3(X1 - 0.82f, 0f, -1.75f), 70f + index * 7f);
            Prop("office_notepads", t, new Vector3(X1 - 0.5f, deskTop, -1.48f), -100f, onFloor: true, floorY: deskTop);
            if (index % 2 == 0) Prop("stationery_supplies", t, new Vector3(X1 - 0.25f, deskTop, -2.45f), -80f, onFloor: true, floorY: deskTop);
            else Prop("clipboard", t, new Vector3(X1 - 0.58f, deskTop, -2.3f), -70f, onFloor: true, floorY: deskTop);
            AddLight(lights, "ScreenGlow", LightType.Point, new Vector3(X1 - 0.7f, 1.15f, -2.0f), new Color(0.45f, 0.75f, 1f), 0.6f, 1.8f);
        }
        if (inbox || spec.tech) Place(heroes["ServerRack"], t, new Vector3(X1 - 0.48f, 0f, ZBack + 0.52f), -20f).name = "Rack";
        else Place(heroes["FilingCabinet"], t, new Vector3(X1 - 0.38f, 0f, ZBack + 0.3f), -10f);
        if (inbox)
        {
            var shelves = Prop("steel_frame_shelves_01", t, new Vector3(X0 + 0.32f, 0f, -1.55f), 90f);
            var sb = Bounds(shelves);
            Prop("cardboard_box_01", t, new Vector3(X0 + 0.32f, 0f, -1.3f), 12f, onFloor: true, floorY: sb.min.y + sb.size.y * 0.52f, scale: 0.55f);
            Prop("cardboard_box_01", t, new Vector3(X0 + 0.3f, 0f, -1.85f), -8f, onFloor: true, floorY: sb.min.y + sb.size.y * 0.52f, scale: 0.5f);
            Prop("cardboard_box_01", t, new Vector3(X0 + 0.33f, 0f, -1.6f), 30f, onFloor: true, floorY: sb.min.y + 0.05f, scale: 0.6f);
            Prop("cardboard_box_01", t, new Vector3(0.38f, 0f, ZBack + 0.3f), 18f, scale: 0.6f);
            Prop("cardboard_box_01", t, new Vector3(0.4f, 0f, ZBack + 0.32f), -12f, onFloor: true, floorY: 0.3f, scale: 0.45f);
            Part(t, "Poster", m.poster, new Vector3(X0 + 0.012f, 1.45f, -2.65f), new Vector3(0.012f, 1f, 1f), shadows: false, repeat: 1f).transform.localScale = new Vector3(1f, 0.84f, 0.6f);
        }
        else
        {
            Place(heroes["NoticeBoard"], t, new Vector3(X0 + 0.03f, 1.12f, -2.2f), 90f);
            if (spec.tech)
            {
                var shelves = Prop("steel_frame_shelves_01", t, new Vector3(X0 + 0.32f, 0f, -1.1f), 90f);
                var sb = Bounds(shelves);
                Prop("cardboard_box_01", t, new Vector3(X0 + 0.3f, 0f, -1.1f), 10f + index * 9f, onFloor: true, floorY: sb.min.y + sb.size.y * 0.52f, scale: 0.5f);
                Prop("metal_toolbox", t, new Vector3(X0 + 0.32f, 0f, -0.95f), 80f, onFloor: true, floorY: sb.min.y + 0.05f);
            }
            else
            {
                Place(heroes["FilingCabinet"], t, new Vector3(X0 + 0.3f, 0f, -1.05f), 90f);
                Prop("cardboard_box_01", t, new Vector3(0.45f, 0f, ZBack + 0.3f), 8f + index * 11f, scale: 0.55f);
            }
        }
        Prop("metal_trash_can", t, new Vector3(index % 2 == 0 ? 0.55f : -1.0f, 0f, -1.2f), index * 23f);
        Prop("fire_alarm", t, new Vector3(X0 + 0.03f, 1.45f, -0.6f), 90f, onFloor: false);
        Prop("korean_fire_extinguisher_01", t, new Vector3(X0 + 0.15f, 0f, -0.55f), 90f);
        Prop("security_camera_01", t, new Vector3(X1 - 0.18f, H - 0.12f, ZFront - 0.2f), -140f, onFloor: false);

        PrefabUtility.SaveAsPrefabAsset(root, $"{Out}/Room_{id}.prefab");
        Object.DestroyImmediate(root);
    }

    // Shoulder height, a little to the right of the hero's axis, at the farthest distance where
    // the hero's box, projected through the camera, covers about 30 % of the frame (Claude
    // Bomb's device covers 40 %) without spilling much past its edges; tall, narrow props are
    // allowed a slight crop top and bottom. Measured with a camera, not estimated.
    static (Vector3 pos, Vector3 target, float fov) CameraPoint(Transform t, Bounds hb)
    {
        var target = new Vector3(hb.center.x + 0.1f, hb.min.y + hb.size.y * 0.5f, hb.center.z);
        var probeCam = new GameObject("FitCamera").AddComponent<Camera>();
        probeCam.aspect = 16f / 9f;
        float fov = 48f;
        probeCam.fieldOfView = fov;
        var corners = new Vector3[8];
        for (int i = 0; i < 8; i++)
            corners[i] = hb.center + Vector3.Scale(hb.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
        Vector3 Place(float dist)
        {
            float pitch = Mathf.Asin(Mathf.Clamp((1.5f - target.y) / dist, -0.2f, 0.75f));
            var dir = Quaternion.Euler(0f, 22f, 0f) * Vector3.forward;
            var p = target + dir * (dist * Mathf.Cos(pitch)) + Vector3.up * (dist * Mathf.Sin(pitch));
            p.x = Mathf.Clamp(p.x, X0 + 0.3f, X1 - 0.6f);
            p.z = Mathf.Clamp(p.z, ZBack + 0.8f, ZFront - 0.25f);
            return p;
        }
        (float w, float h, float wRaw, float hRaw) Measure(Vector3 p)
        {
            probeCam.transform.position = p;
            probeCam.transform.LookAt(target);
            float x0 = 9, x1 = -9, y0 = 9, y1 = -9;
            foreach (var c in corners)
            {
                var v = probeCam.WorldToViewportPoint(c);
                x0 = Mathf.Min(x0, v.x); x1 = Mathf.Max(x1, v.x); y0 = Mathf.Min(y0, v.y); y1 = Mathf.Max(y1, v.y);
            }
            // The feed is the left two thirds of the screen: wider than two thirds is spilling.
            return (Mathf.Min(x1 - x0, 2f / 3f), Mathf.Min(y1 - y0, 1f), x1 - x0, y1 - y0);
        }
        bool narrow = hb.size.x < hb.size.y * 0.62f;
        float best = 1.0f;
        for (float dist = 3.0f; dist >= 1.0f; dist -= 0.05f)
        {
            var p = Place(dist);
            var (w, h, wRaw, hRaw) = Measure(p);
            bool fits = hRaw <= (narrow ? 1.08f : 0.92f) && wRaw <= 0.64f;
            best = dist;
            if (w * h >= 0.30f || !fits) { if (!fits) best = dist + 0.05f; break; }
        }
        var pos = Place(best);
        var m = Measure(pos);
        Object.DestroyImmediate(probeCam.gameObject);
        var cam = new GameObject("CamPoint").transform; cam.SetParent(t, false);
        cam.localPosition = pos;
        var look = new GameObject("LookAt").transform; look.SetParent(t, false);
        look.localPosition = target;
        cam.LookAt(look);
        Debug.Log($"[FacilityKit] {t.name}: hero {hb.size.x:F2} x {hb.size.y:F2} x {hb.size.z:F2}, camera {Vector3.Distance(pos, target):F2} m, box {m.w:P0} x {m.h:P0} = {m.w * m.h:P0}");
        return (pos, target, fov);
    }

    // The Operations Center's hero: a wall of six screens on a steel rail, each showing its board.
    static GameObject MonitorWall(Transform t, Mats m, GameObject monitor, Vector3 at)
    {
        var wall = new GameObject("MonitorWall").transform;
        wall.SetParent(t, false);
        Part(wall, "Rail", m.trim, new Vector3(at.x, 1.28f, ZBack + 0.03f), new Vector3(1.9f, 0.9f, 0.04f));
        int k = 0;
        for (int row = 0; row < 2; row++)
            for (int col = 0; col < 3; col++)
            {
                var mon = Place(monitor, wall, new Vector3(at.x + (col - 1) * 0.6f, 0f, ZBack + 0.16f), 0f);
                var b = Bounds(mon);
                mon.transform.position += Vector3.up * ((row == 0 ? 0.88f : 1.38f) - b.min.y);
                ScreenOn(wall, m, mon, 20 + k++, "Screen");
            }
        // A console desk in front of the wall.
        var desk = Prop("metal_office_desk", t, new Vector3(at.x, 0f, ZBack + 0.75f), 180f);
        return wall.gameObject;
    }

    // A monitor's screen, lit: the inbox for the post room, a board of charts elsewhere.
    static void ScreenOn(Transform parent, Mats m, GameObject monitor, int variant, string name)
    {
        var mb = Bounds(monitor);
        var fwd = monitor.transform.forward;
        var screen = Part(parent, name, variant == 1 ? m.screen : m.boards[variant % m.boards.Length], Vector3.zero, new Vector3(0.012f, 1f, 1f), shadows: false, repeat: 1f);
        // The panel sits just proud of the monitor's face, facing where the monitor faces.
        screen.transform.rotation = Quaternion.LookRotation(Vector3.Cross(fwd, Vector3.up), Vector3.up);
        float depth = Mathf.Abs(Vector3.Dot(mb.size, new Vector3(Mathf.Abs(fwd.x), 0f, Mathf.Abs(fwd.z))));
        float width = Mathf.Abs(Vector3.Dot(mb.size, new Vector3(Mathf.Abs(fwd.z), 0f, Mathf.Abs(fwd.x))));
        screen.transform.position = mb.center + fwd * (depth / 2f - 0.012f) + Vector3.up * mb.size.y * 0.13f;
        screen.transform.localScale = new Vector3(1f, mb.size.y * 0.52f, width * 0.93f);
    }

    // The lockdown beacon: a dark base, an amber dome, two beams turning (RoomLife turns "BeaconHead").
    static void Beacon(Transform t, Mats m, Vector3 at)
    {
        var root = new GameObject("Beacon").transform;
        root.SetParent(t, false);
        root.localPosition = at;
        Part(root, "Bracket", m.trim, new Vector3(0f, -0.06f, 0.06f), new Vector3(0.14f, 0.03f, 0.14f), shadows: false);
        Disc(root, "Base", m.trim, new Vector3(0f, -0.02f, 0.08f), 0.12f, 0.05f).transform.localRotation = Quaternion.identity;
        var dome = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Object.DestroyImmediate(dome.GetComponent<Collider>());
        dome.name = "BeaconDome";
        dome.transform.SetParent(root, false);
        dome.transform.localPosition = new Vector3(0f, 0.05f, 0.08f);
        dome.transform.localScale = Vector3.one * 0.12f;
        var r = dome.GetComponent<MeshRenderer>(); r.sharedMaterial = m.beacon; r.shadowCastingMode = ShadowCastingMode.Off;
        var head = new GameObject("BeaconHead").transform;
        head.SetParent(root, false);
        head.localPosition = new Vector3(0f, 0.05f, 0.08f);
        foreach (float yaw in new[] { 0f, 180f })
        {
            var l = AddLight(head, "BeaconLight", LightType.Spot, Vector3.zero, new Color(1f, 0.42f, 0.08f), 5f, 6f);
            l.transform.localRotation = Quaternion.Euler(12f, yaw, 0f);
            l.spotAngle = 44f; l.innerSpotAngle = 10f;
        }
    }

    // The boards on the other screens: charts and rows, three of them, made once.
    static Material[] BoardMaterials()
    {
        var mats = new List<Material>();
        for (int v = 0; v < 3; v++)
        {
            string path = $"{Generated}/screen_board_{v}.png";
            if (!File.Exists(path))
            {
                const int W = 512, Hh = 320;
                var px = new Color32[W * Hh];
                Color32 bg = new Color32(12, 20, 38, 255), panel = new Color32(24, 38, 66, 255), teal = new Color32(36, 224, 192, 255), amber = new Color32(224, 155, 26, 255), red = new Color32(214, 69, 61, 255), dim = new Color32(90, 112, 150, 255);
                for (int i = 0; i < px.Length; i++) px[i] = bg;
                void Rect(int x, int y, int w, int h, Color32 c) { for (int yy = y; yy < y + h; yy++) for (int xx = x; xx < x + w; xx++) if (xx >= 0 && xx < W && yy >= 0 && yy < Hh) px[(Hh - 1 - yy) * W + xx] = c; }
                var rng = new System.Random(7 + v);
                Rect(0, 0, W, 28, panel); Rect(12, 10, 80, 8, teal);
                // A line chart, a bar chart and a list, placed differently on each board.
                int cx = v == 1 ? 260 : 12, lx = v == 1 ? 12 : 260;
                Rect(cx, 40, 240, 130, panel);
                int prev = 120;
                for (int x = 0; x < 220; x += 4) { int y = Mathf.Clamp(prev + rng.Next(-9, 10), 60, 160); Rect(cx + 10 + x, y, 4, 3, v == 2 ? amber : teal); prev = y; }
                Rect(cx, 180, 240, 128, panel);
                for (int k = 0; k < 9; k++) { int hgt = 20 + rng.Next(0, 80); Rect(cx + 14 + k * 25, 300 - hgt, 16, hgt, k == 6 ? red : k % 3 == 0 ? amber : teal); }
                Rect(lx, 40, 240, 268, panel);
                for (int k = 0; k < 9; k++) { Rect(lx + 12, 54 + k * 28, 10, 10, k == 3 + v ? red : k % 4 == 0 ? amber : teal); Rect(lx + 30, 56 + k * 28, 80 + rng.Next(0, 110), 6, dim); }
                WritePng(path, W, Hh, px);
            }
            var tex = Texture(path, 512, true, wrap: TextureWrapMode.Clamp);
            mats.Add(Lit($"monitor_board_{v}", tex, FlatNormal(), null, tex, Color.white, Color.white * 1.5f, null, 0.92f));
        }
        return mats.ToArray();
    }
}
