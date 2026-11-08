using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

// The room's rows as things on the room's prop (the game-feel pass): every item is a card on
// the prop (a card on the risk board, a module in the firewall's rack, an envelope in the sorter,
// a badge at the turnstile, a ticket on the bench) with its short name on a dark plate and a knob
// for each decision. Every knob shows its value on its face, and a lamp beside it is amber while
// the decision is open, teal once made, red for the choice that closes or flags. The room's
// one-off decisions are a card of their own in the same grid. Tap a card to bring it close: a
// flat card turns to face the camera, the full name and line at its top and one row per decision
// ("Owner: Omar"); tap a row to turn it. Rows that must be ordered are a rack: drag a module up or
// down. A record that must be signed has a pad beside the grid; what is not on the list yet lies
// about the room, to be found and pinned. Everything writes the same RoomState the drawer's chips
// wrote, so the checks and the report read the same values. Made from the kit's own materials.
public class RoomRig : MonoBehaviour
{
    public class Switch
    {
        public Field field; public string itemKey;
        public Transform root, notch, rowSlab; public Renderer lamp;
        public TextMeshPro face;   // the value, on the knob's face
        public TextMeshPro row;    // "Owner: Omar", on a row of the card brought close
    }

    public class Token
    {
        public Item item; public string key;
        public Transform root, namePlate, slab;
        public readonly List<Switch> switches = new List<Switch>();
        public List<Field> fields = new List<Field>();
        public Vector3 slot, homeLoose; public Quaternion slotRot;
        public bool loose, pinned, selected, pulsing, console, sign, dragging;
        public int order;
        public float w, h;
        public Renderer lamp;            // the row's lamp: teal once every decision on it is made, amber while one is open
        public Transform flyer;          // a loose thing: what flies to the board when it is tapped
        public bool wasInView; public float glintAt = -10f;
    }

    Building world; Room room; RoomState st; Action changed;
    Transform rig, roomRoot; Camera cam;
    readonly List<Token> tokens = new List<Token>();
    readonly List<Switch> rows = new List<Switch>(); // the card brought close
    Token signPad, selected, held;
    Transform focus, focusSlab, unpinRow;
    Vector3 focusCentre; float focusD = 1f, focusFov = 52f;
    Bounds bounds;
    float zFace, gridLeft, gridRight, gridTop, gridBottom, cx, scale = 1f, cellW, cellH, gap, floorY;
    Vector3 searchSpot;
    bool badges; const float BadgeRoom = 0.13f;
    float rackBottom = float.MaxValue;
    static readonly Dictionary<string, Vector3> SearchSpots = new Dictionary<string, Vector3>();
    int cols; bool rack;
    Vector2 downAt, dragWas; bool pointerDown;
    public bool Interactive = true;
    string lastTurned;                  // "item/field": the knob just turned, the one on the board that shows its value
    Renderer focusLamp;
    bool lookAround, panning, flying; float lastLook = -10f; Vector2 lookWas;
    public bool LookAround => lookAround;
    public event Action<Token> Selected; // a card brought close (null: back to the room)
    public Bounds Bounds => bounds;
    public float? ConsoleX { get; private set; }      // where the signature pad stands, in the room's space; null without one
    public float GridTop => gridTop;
    public float GridLeft => gridLeft;
    public float GridRight => gridRight;
    public float ConsoleTop { get; private set; }
    public Rect? ConsoleRect { get; private set; }    // the pad, in the room's x and y
    public Rect? ConsoleView { get; private set; }    // the same, as the room camera sees it (degrees off its axis)
    public Rect GridView { get; private set; }        // the grid of cards, as the room camera sees it
    public readonly List<Rect> Occluders = new List<Rect>(); // the props between the camera and the rig, as the camera sees them
    public readonly List<Rect> WallOccluders = new List<Rect>(); // the same, for something on the back wall (clipped at the wall)
    public IReadOnlyList<Token> Tokens => tokens;
    static readonly Color Slate = new Color(0.16f, 0.2f, 0.32f), Paper = new Color(0.93f, 0.92f, 0.86f), Dim = new Color(0.62f, 0.66f, 0.72f);
    const float Gap = 0.06f, RackGap = 0.02f, Pitch = 0.1f;

    public static RoomRig Build(Building world, int index, Room room, RoomState st, Action changed)
    {
        var roomRoot = world.RoomRoot(index);
        var go = new GameObject("Rig");
        go.transform.SetParent(roomRoot, false);
        var r = go.AddComponent<RoomRig>();
        r.world = world; r.room = room; r.st = st; r.changed = changed; r.rig = go.transform; r.roomRoot = roomRoot; r.cam = world.Camera;
        r.Make(index);
        return r;
    }

    // ---- Building the rig over the hero ----

    void Make(int index)
    {
        var hb = world.HeroBounds(index);
        // The hero's box in the room's own space (the door at the origin, the room toward -Z).
        var min = roomRoot.InverseTransformPoint(hb.min); var max = roomRoot.InverseTransformPoint(hb.max);
        float hx0 = Mathf.Min(min.x, max.x), hx1 = Mathf.Max(min.x, max.x), hy0 = Mathf.Min(min.y, max.y), hy1 = Mathf.Max(min.y, max.y), hz1 = Mathf.Max(min.z, max.z);
        zFace = hz1 + 0.16f;
        cx = (hx0 + hx1) / 2f;
        floorY = hy0; // the floor, or the desk top the prop stands on
        rack = room.ordered;
        FindOccluders(world.HeroOf(index));
        AddHeroToWall(world.HeroOf(index));
        // The camera stands nearer a small prop: the rig shrinks with it, so the grid always fits the feed.
        scale = 1f;
        if (world.RoomCamera(index, out var eye, out var look, out float fov))
        {
            float visible = 2f * Vector3.Distance(eye, look) * Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
            scale = Mathf.Clamp(visible / 2.2f, 0.5f, 1f);
        }
        var perRow = room.fields.Where(f => !f.once).ToList();
        var onceFields = room.fields.Where(f => f.once).ToList();
        var order = st.order.Where(k => room.items.Any(i => i.key == k)).ToList();
        foreach (var a in st.added) if (!order.Contains(a)) order.Add(a);
        // The room's one-off decisions are a card of their own, last in the grid.
        bool consoleCard = onceFields.Count > 0;
        int n = order.Count + (consoleCard ? 1 : 0);
        int knobs = Mathf.Max(perRow.Count, consoleCard ? onceFields.Count : 0);
        badges = !rack && (room.number == 6 || room.number == 16); // 2.1's accounts and 3.1's roles: the staff badges
        float w = rack ? 1.16f : Mathf.Max(0.34f, Pitch * knobs + 0.1f) + (badges ? BadgeRoom : 0f), h = rack ? 0.15f : 0.22f;
        cellW = w * scale; cellH = h * scale; gap = (rack ? RackGap : Gap) * scale;
        cols = rack ? 1 : Mathf.Clamp(Mathf.FloorToInt((1.62f * scale + gap) / (cellW + gap)), 1, 3);
        if (!rack && n <= 4 && cols > 2) cols = 2;
        if (!rack && cols > 2 && room.signable) cols = 2; // the pad beside the grid needs the room
        int rowsN = Mathf.CeilToInt(n / (float)cols);
        float gridW = cols * cellW + (cols - 1) * gap, gridH = rowsN * cellH + (rowsN - 1) * gap;
        float yc = Mathf.Clamp((hy0 + hy1) / 2f + (rack ? 0.1f : 0.05f) * scale, 0.35f + gridH / 2f, 2.0f - gridH / 2f);
        gridTop = yc + gridH / 2f; gridBottom = yc - gridH / 2f;
        gridLeft = cx - gridW / 2f; gridRight = cx + gridW / 2f;
        // The grid's top row inside the feed: slide down while the floor allows.
        for (int guard = 0; guard < 12 && !InFrame(new Vector3(cx, gridTop + 0.04f, zFace), 3f) && gridBottom - 0.05f > 0.15f; guard++)
        { yc -= 0.05f; gridTop = yc + gridH / 2f; gridBottom = yc - gridH / 2f; }
        // The Operations Center's wall of screens: the grid slides sideways, as little as it can
        // and staying in the feed, so that one screen stays clear for the corridor feed.
        var heroT = world.HeroOf(index);
        var screens = heroT != null ? heroT.GetComponentsInChildren<Renderer>().Where(x => x.name == "Screen").Select(x => x.bounds).ToList() : new List<Bounds>();
        if (!rack && screens.Count > 0)
        {
            Rect ScreenRect(Bounds b)
            {
                var lo = rig.InverseTransformPoint(b.min); var hi = rig.InverseTransformPoint(b.max);
                return ViewRect(Mathf.Min(lo.x, hi.x), Mathf.Min(lo.y, hi.y), Mathf.Max(lo.x, hi.x), Mathf.Max(lo.y, hi.y), Mathf.Max(lo.z, hi.z));
            }
            var shown = screens.Where(b => InFrame(rig.InverseTransformPoint(b.center), 3f)).Select(ScreenRect).ToList();
            bool Clear(float c) { var g = ViewRect(c - gridW / 2f - 0.05f, gridBottom - 0.05f, c + gridW / 2f + 0.05f, gridTop + 0.12f * scale, zFace); return shown.Any(v => !v.Overlaps(g)); }
            bool Fits(float c) => InFrame(new Vector3(c - gridW / 2f, gridTop + 0.04f, zFace), 3f) && InFrame(new Vector3(c + gridW / 2f, gridTop + 0.04f, zFace), 3f)
                                  && InFrame(new Vector3(c - gridW / 2f, gridBottom, zFace), 3f) && InFrame(new Vector3(c + gridW / 2f, gridBottom, zFace), 3f);
            bool moved = false;
            for (float d = 0f; d <= 0.8f && !moved; d += 0.05f)
                foreach (float c in d == 0f ? new[] { cx } : new[] { cx - d, cx + d })
                    if (Fits(c) && Clear(c)) { if (DemoDriver.Wanted && d > 0f) Debug.Log($"[Grid] {room.id}: slid {c - cx:+0.00;-0.00} m to keep a screen clear"); cx = c; moved = true; break; }
            if (!moved && DemoDriver.Wanted) Debug.Log($"[Grid] {room.id}: no slide keeps a screen clear");
            gridLeft = cx - gridW / 2f; gridRight = cx + gridW / 2f;
        }
        for (int k = 0; k < order.Count; k++)
        {
            var item = room.items.First(i => i.key == order[k]);
            var t = MakeToken(item, w, h, perRow, false);
            t.order = k;
            t.pinned = item.hidden;
            Seat(t, k);
        }
        if (consoleCard)
        {
            var t = MakeToken(new Item("room", Script.ConsoleName(room), Script.ConsoleLine(room)), w, h, onceFields, true);
            t.order = order.Count;
            Seat(t, order.Count);
        }
        if (rack && tokens.Count > 0)
        {
            // One text size for every module (the smallest any name needed), and a dark panel
            // behind them: the modules are mounted in a rack, not floating.
            float font = tokens.Min(x => x.namePlate.GetComponentInChildren<TextMeshPro>().fontSize);
            foreach (var x in tokens)
            {
                var tm = x.namePlate.GetComponentInChildren<TextMeshPro>(); tm.fontSize = font; Building.FitPlate(x.namePlate);
                x.namePlate.localPosition = new Vector3(-x.w / 2f + 0.09f + x.namePlate.Find("Slab").localScale.x / 2f, 0f, -0.024f);
            }
            Building.Box(rig, "Rack panel", new Vector3(gridRight - gridLeft + 0.08f * scale, gridTop - gridBottom + 0.08f * scale, 0.03f), new Vector3(cx, (gridTop + gridBottom) / 2f, zFace - 0.03f), Building.Trim, new Color(0.08f, 0.09f, 0.12f));
            foreach (float side in new[] { -1f, 1f })
                Building.Box(rig, "Rail", new Vector3(0.03f * scale, gridTop - gridBottom + 0.1f * scale, 0.04f), new Vector3(cx + side * (gridRight - gridLeft + 0.08f * scale) / 2f, (gridTop + gridBottom) / 2f, zFace - 0.015f), Building.Metal, new Color(0.35f, 0.38f, 0.44f));
            if (room.number == 9) MountPatchPanel();
        }
        if (room.signable) PlacePad();
        GridView = ViewRect(gridLeft - 0.05f, Mathf.Min(gridBottom, rackBottom) - 0.05f, gridRight + 0.05f, gridTop + 0.12f * scale, zFace);
        // What is not on the list yet, about the room: a folder on a crate, a printer by the door, a sheet on the wall.
        int loose = 0;
        lookAround = room.items.Any(i => i.hidden && !i.decoy);
        if (lookAround)
        {
            // What nobody listed sits on a crate in a corner of the room, out of the feed as it
            // opens: the player turns the camera to find it.
            if (!SearchSpots.TryGetValue(room.id, out searchSpot)) // found once per room, so it stays put between visits
                SearchSpots[room.id] = searchSpot = SearchSpot(new Vector3(Mathf.Min(1.45f, hx1 + 0.35f), 0f, hz1 + 0.35f)); // clear of the printer
            Building.Box(rig, "Crate", new Vector3(0.42f, 0.36f, 0.36f), new Vector3(searchSpot.x, 0.18f, searchSpot.z), Building.Metal, new Color(0.55f, 0.5f, 0.42f));
            Building.Box(rig, "Crate lid", new Vector3(0.44f, 0.02f, 0.38f), new Vector3(searchSpot.x, 0.37f, searchSpot.z), Building.Metal, new Color(0.5f, 0.45f, 0.37f), false);
        }
        foreach (var hidden in room.items.Where(i => i.hidden && !st.added.Contains(i.key)))
            tokens.Add(MakeLoose(hidden, loose++, hx0, hx1, hz1));
        // What the close camera frames: the grid and the pad.
        bounds = new Bounds(rig.TransformPoint(new Vector3(cx, (gridTop + gridBottom) / 2f, zFace)), Vector3.zero);
        bounds.Encapsulate(rig.TransformPoint(new Vector3(gridLeft, gridBottom, zFace)));
        bounds.Encapsulate(rig.TransformPoint(new Vector3(gridRight, gridTop, zFace)));
        if (ConsoleRect.HasValue) { var r = ConsoleRect.Value; bounds.Encapsulate(rig.TransformPoint(new Vector3(r.xMin, r.yMin, zFace))); bounds.Encapsulate(rig.TransformPoint(new Vector3(r.xMax, r.yMax, zFace))); }
        bounds.Expand(new Vector3(0.3f, 0.3f, 0.2f));
        Refresh();
    }

    void Seat(Token t, int k)
    {
        t.root.localScale = Vector3.one * scale;
        t.root.localPosition = Slot(k); t.root.localRotation = Quaternion.Euler(0f, 180f, 0f);
        t.slot = t.root.localPosition; t.slotRot = t.root.localRotation;
        tokens.Add(t);
    }

    // The signature pad beside the grid: the first spot, in this order, whose corners are in the
    // feed, inside the room's walls and not behind a prop nearer the camera (the bin, the desk, the
    // rack): beside the grid on the screen's right, then its left, at rising heights; then under it.
    void PlacePad()
    {
        float ps = scale, nameSpan = 0.2f * ps, valueSpan = 0.24f * ps, gapX = 0.05f * scale, padH = 0.12f * ps;
        bool FitsAt(float x, float top)
        {
            float bottom = top - padH;
            if (bottom < floorY + 0.05f || top > 2.35f) return false;
            if (x - valueSpan < -1.75f || x + nameSpan > 1.75f) return false;
            foreach (var (px, py) in new[] { (x - valueSpan, top), (x + nameSpan, top), (x - valueSpan, bottom), (x + nameSpan, bottom) })
                if (!InFrame(new Vector3(px, py, zFace), 3f)) return false;
            return !Hidden(ViewRect(x - valueSpan, bottom, x + nameSpan, top, zFace));
        }
        float right = gridLeft - nameSpan - gapX, left = gridRight + valueSpan + gapX;
        float x0 = right, top0 = gridTop; bool found = false;
        foreach (var (x, l) in new[] { 0f, 0.15f, 0.3f, -0.15f, -0.3f }.Select(l => (right, l)).Concat(new[] { 0f, 0.15f, 0.3f, -0.15f }.Select(l => (left, l))))
            if (!found && FitsAt(x, gridTop + l * scale)) { found = true; x0 = x; top0 = gridTop + l * scale; }
        if (!found && FitsAt(cx, gridBottom - 0.08f * scale)) { found = true; x0 = cx; top0 = gridBottom - 0.08f * scale; }
        signPad = new Token { key = "sign", sign = true, w = 0.34f, h = 0.12f };
        signPad.root = new GameObject("Sign pad").transform;
        signPad.root.SetParent(rig, false);
        signPad.root.localPosition = new Vector3(x0, top0 - padH / 2f, zFace);
        signPad.root.localRotation = Quaternion.Euler(0f, 180f, 0f);
        signPad.root.localScale = Vector3.one * ps;
        signPad.slab = Building.Box(signPad.root, "Pad", new Vector3(0.34f, 0.12f, 0.02f), Vector3.zero, Building.Metal, Slate).transform;
        signPad.namePlate = Building.DarkPlate(signPad.root, "Sign", 0.5f, out _, 0.2f);
        signPad.namePlate.localPosition = new Vector3(0.03f, 0f, -0.025f);
        var sw = new Switch { itemKey = "sign" };
        sw.root = new GameObject("Lamp").transform; sw.root.SetParent(signPad.root, false); sw.root.localPosition = new Vector3(-0.12f, 0f, -0.015f);
        sw.lamp = Building.Lamp(sw.root, Vector3.zero, 0.03f, Theme.Amber);
        signPad.switches.Add(sw);
        signPad.slot = signPad.root.localPosition; signPad.slotRot = signPad.root.localRotation;
        tokens.Add(signPad);
        ConsoleX = x0;
        ConsoleRect = Rect.MinMaxRect(x0 - valueSpan, top0 - padH, x0 + nameSpan, top0);
        ConsoleView = ViewRect(x0 - valueSpan, top0 - padH, x0 + nameSpan, top0, zFace);
        ConsoleTop = top0;
        if (DemoDriver.Wanted) Debug.Log($"[Pad] {room.id}: {(found ? "placed" : "fallback")} x {x0:0.00} top {top0:0.00}");
    }

    // Whether a local point lies inside the feed, with a margin in degrees, seen from the
    // room's camera point (the aspect is the screen's).
    bool InFrame(Vector3 local, float marginDeg = 6f)
    {
        if (!world.RoomCamera(Facility.RoomIndex(room.room), out var eye, out var look, out float fov)) return true;
        var fwd = (look - eye).normalized;
        var d = rig.TransformPoint(local) - eye;
        float aspect = (float)Screen.width / Mathf.Max(1, Screen.height);
        float half = fov * 0.5f * Mathf.Deg2Rad;
        float hhalf = Mathf.Atan(Mathf.Tan(half) * aspect);
        var right = Vector3.Cross(Vector3.up, fwd).normalized; var up = Vector3.Cross(fwd, right);
        float z = Vector3.Dot(d, fwd); if (z <= 0.05f) return false;
        float ax = Mathf.Atan2(Vector3.Dot(d, right), z), ay = Mathf.Atan2(Vector3.Dot(d, up), z);
        float m = marginDeg * Mathf.Deg2Rad;
        return Mathf.Abs(ax) < hhalf - m && Mathf.Abs(ay) < half - m;
    }

    public bool PointInFrame(Vector3 local, float marginDeg = 6f) => InFrame(local, marginDeg);

    // Where a local point sits in the room camera's view: degrees right and up of its axis.
    public Vector2 View(Vector3 local)
    {
        if (!world.RoomCamera(Facility.RoomIndex(room.room), out var eye, out var look, out _)) return Vector2.zero;
        var fwd = (look - eye).normalized;
        var right = Vector3.Cross(Vector3.up, fwd).normalized; var up = Vector3.Cross(fwd, right);
        var d = rig.TransformPoint(local) - eye;
        float z = Mathf.Max(0.05f, Vector3.Dot(d, fwd));
        return new Vector2(Mathf.Atan2(Vector3.Dot(d, right), z), Mathf.Atan2(Vector3.Dot(d, up), z)) * Mathf.Rad2Deg;
    }

    public Rect ViewRect(float x0, float y0, float x1, float y1, float z)
    {
        var a = View(new Vector3(x0, y0, z)); var b = View(new Vector3(x1, y1, z));
        var c = View(new Vector3(x0, y1, z)); var d = View(new Vector3(x1, y0, z));
        return Rect.MinMaxRect(Mathf.Min(Mathf.Min(a.x, b.x), Mathf.Min(c.x, d.x)), Mathf.Min(Mathf.Min(a.y, b.y), Mathf.Min(c.y, d.y)),
                               Mathf.Max(Mathf.Max(a.x, b.x), Mathf.Max(c.x, d.x)), Mathf.Max(Mathf.Max(a.y, b.y), Mathf.Max(c.y, d.y)));
    }

    // The props standing between the room camera and the rig's plane (the bin by the camera, the
    // desk and its chair, the rack, a cabinet), each as the rectangle the camera sees it cover.
    void FindOccluders(Transform hero)
    {
        Occluders.Clear(); WallOccluders.Clear();
        if (!world.RoomCamera(Facility.RoomIndex(room.room), out var eye, out var look, out _)) return;
        var fwd = (look - eye).normalized;
        float camZ = rig.InverseTransformPoint(eye).z;
        foreach (var r in roomRoot.GetComponentsInChildren<Renderer>())
        {
            if (r.transform.IsChildOf(rig) || (hero != null && r.transform.IsChildOf(hero))) continue;
            if (r is ParticleSystemRenderer || r.GetComponent<TMPro.TextMeshPro>() != null) continue;
            if (r.sharedMaterial != null && r.sharedMaterial.renderQueue >= 3000) continue; // beams, glass, effects
            if (r.GetComponentsInParent<Transform>(true).Any(t => t.name == "CorridorMonitor")) continue;
            var b = r.bounds;
            var corners = new Vector3[8];
            float zMin = 9f, zMax = -9f, xSize, ySize;
            Vector3 lmin = Vector3.one * 99f, lmax = -Vector3.one * 99f;
            for (int i = 0; i < 8; i++)
            {
                var w = b.center + Vector3.Scale(b.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
                corners[i] = rig.InverseTransformPoint(w);
                lmin = Vector3.Min(lmin, corners[i]); lmax = Vector3.Max(lmax, corners[i]);
            }
            zMin = lmin.z; zMax = lmax.z; xSize = lmax.x - lmin.x; ySize = lmax.y - lmin.y;
            if (zMax < -3.4f || zMin > camZ) continue;  // behind the wall's monitor, or wholly behind the camera (the camera looks toward -z)
            if (xSize > 2.5f || ySize > 2.5f || lmax.z - lmin.z > 2.5f) continue; // walls, floor, ceiling
            if (lmin.y > 2.15f) continue;                              // the lamp, the conduit, the beacon overhead
            // Only the part in front of a plane can hide what stands on it (a desk under the prop does not hide the prop).
            Rect Clip(float plane)
            {
                float x0 = 999f, y0 = 999f, x1 = -999f, y1 = -999f;
                foreach (var c0 in corners) { var c = c0; if (c.z < plane) c.z = plane; var v = View(c); x0 = Mathf.Min(x0, v.x); x1 = Mathf.Max(x1, v.x); y0 = Mathf.Min(y0, v.y); y1 = Mathf.Max(y1, v.y); }
                return Rect.MinMaxRect(x0, y0, x1, y1);
            }
            WallOccluders.Add(Clip(-3.44f));
            if (zMax < zFace + 0.05f) continue;
            var rect = Clip(zFace);
            float x0 = rect.xMin, x1 = rect.xMax, y0 = rect.yMin, y1 = rect.yMax;
            Occluders.Add(rect);
            if (DemoDriver.Wanted) Debug.Log($"[Occluder] {room.id} {r.name}: view x {x0:0}..{x1:0} y {y0:0}..{y1:0}, z {zMin:0.00}..{zMax:0.00}, camera z {camZ:0.00}");
        }
    }

    // The prop itself hides what is on the wall behind it.
    void AddHeroToWall(Transform hero)
    {
        if (hero == null) return;
        var rs = hero.GetComponentsInChildren<Renderer>().Where(r => !(r is ParticleSystemRenderer)).ToArray();
        if (rs.Length == 0) return;
        var b = rs[0].bounds; foreach (var r in rs) b.Encapsulate(r.bounds);
        float x0 = 999f, y0 = 999f, x1 = -999f, y1 = -999f;
        for (int i = 0; i < 8; i++)
        {
            var w = b.center + Vector3.Scale(b.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
            var v = View(rig.InverseTransformPoint(w));
            x0 = Mathf.Min(x0, v.x); x1 = Mathf.Max(x1, v.x); y0 = Mathf.Min(y0, v.y); y1 = Mathf.Max(y1, v.y);
        }
        WallOccluders.Add(Rect.MinMaxRect(x0, y0, x1, y1));
    }

    public bool Hidden(Rect view) => Occluders.Any(o => o.Overlaps(view));
    public bool HiddenOnWall(Rect view) => WallOccluders.Any(o => o.Overlaps(view));

    Vector3 Slot(int k)
    {
        // The camera inside looks down -Z, so the screen's left is the larger local x.
        int col = k % cols, row = k / cols;
        return new Vector3(gridRight - cellW / 2f - col * (cellW + gap), gridTop - cellH / 2f - row * (cellH + gap), zFace);
    }

    Token MakeToken(Item item, float w, float h, List<Field> fields, bool console)
    {
        var t = new Token { item = item, key = item.key, w = w, h = h, fields = fields, console = console };
        t.root = new GameObject("Token " + item.key).transform;
        t.root.SetParent(rig, false);
        bool paper = !console && (room.number == 3 || room.number == 5); // envelopes and report forms
        t.slab = Building.Box(t.root, "Slab", new Vector3(w, h, 0.02f), Vector3.zero, paper ? Building.Enamel : Building.Metal, paper ? Paper : console ? new Color(0.12f, 0.22f, 0.3f) : Slate).transform;
        if (rack)
        {
            // A module in the rack: a grip at the left end, the name along it, the knobs at the right end.
            for (int g = 0; g < 2; g++) Building.Box(t.root, "Grip", new Vector3(0.012f, h * 0.7f, 0.01f), new Vector3(-w / 2f + 0.03f + g * 0.02f, 0f, -0.015f), Building.Trim, null, false);
            t.namePlate = Building.DarkPlate(t.root, Script.ItemName(room, item), 0.46f, out var nameText, 0.3f);
            float room_ = w - 0.14f - fields.Count * 0.11f;
            for (int guard = 0; guard < 6 && t.namePlate.Find("Slab").localScale.x > room_; guard++) { nameText.fontSize *= 0.9f; Building.FitPlate(t.namePlate); }
            t.namePlate.localPosition = new Vector3(-w / 2f + 0.09f + t.namePlate.Find("Slab").localScale.x / 2f, 0f, -0.024f);
            float sx = w / 2f - 0.065f;
            for (int k = fields.Count - 1; k >= 0; k--) { t.switches.Add(Knob(t.root, fields[k], item.key, new Vector3(sx, 0f, -0.02f))); sx -= 0.11f; }
            t.switches.Reverse();
            t.lamp = Building.Lamp(t.root, new Vector3(-w / 2f + 0.07f, 0f, -0.026f), 0.022f, Theme.Amber);
        }
        else
        {
            // A card with a staff badge clipped on its left: the name and the knobs move over.
            bool badge = badges && !console && Badge(t.root, item.key, 0.17f, new Vector3(-w / 2f + BadgeRoom / 2f + 0.01f, -0.005f, -0.016f)) != null;
            float shift = badge ? BadgeRoom / 2f : 0f, room_ = w - 0.04f - (badge ? BadgeRoom : 0f);
            t.namePlate = Building.DarkPlate(t.root, console ? item.name : Script.ItemName(room, item), 0.44f, out var nameText, 0.2f);
            for (int guard = 0; guard < 6 && t.namePlate.Find("Slab").localScale.x > room_; guard++) { nameText.fontSize *= 0.9f; Building.FitPlate(t.namePlate); }
            t.namePlate.localPosition = new Vector3(shift, h / 2f - 0.05f, -0.024f);
            float span = (fields.Count - 1) * Pitch;
            for (int k = 0; k < fields.Count; k++) t.switches.Add(Knob(t.root, fields[k], item.key, new Vector3(shift - span / 2f + k * Pitch, -h / 2f + 0.065f, -0.02f)));
            t.lamp = Building.Lamp(t.root, new Vector3(w / 2f - 0.032f, h / 2f - 0.032f, -0.034f), 0.022f, Theme.Amber);
            var group = console ? null : Script.Group(room, item);
            if (group != null) { var g = Building.DarkPlate(t.root, group, 0.26f, out var gt, 0.1f); gt.color = Dim; g.localPosition = new Vector3(0f, h / 2f + 0.045f, -0.02f); }
        }
        return t;
    }

    // A knob: a dark housing, a disc with the current value on its face, a notch on its rim that
    // turns with the value, and a lamp at its corner.
    Switch Knob(Transform parent, Field f, string itemKey, Vector3 at)
    {
        var sw = new Switch { field = f, itemKey = itemKey };
        sw.root = new GameObject("Knob " + f.key).transform;
        sw.root.SetParent(parent, false);
        sw.root.localPosition = at;
        var housing = Building.Box(sw.root, "Housing", new Vector3(0.088f, 0.088f, 0.012f), Vector3.zero, Building.Metal, new Color(0.1f, 0.11f, 0.14f), false);
        Building.Matte(housing.GetComponent<Renderer>()); // a shiny housing flares under the lamp
        Disc(sw.root, 0.076f, new Vector3(0f, 0f, -0.012f));
        sw.face = Label(sw.root, "", 0.2f, Color.white, true, new Vector2(0.064f, 0.034f), new Vector3(0f, 0f, -0.019f), TextAlignmentOptions.Center, false);
        sw.face.enableAutoSizing = true; sw.face.fontSizeMax = 0.2f; sw.face.fontSizeMin = 0.05f;
        sw.notch = Notch(sw.root, new Vector3(0f, 0f, -0.018f), 0.033f, 0.008f);
        return sw;
    }

    static void Disc(Transform parent, float diameter, Vector3 at, Color? tint = null)
    {
        var disc = new GameObject("Dial");
        disc.transform.SetParent(parent, false);
        disc.transform.localPosition = at;
        disc.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        // The built-in cylinder mesh is two units across (not one, as the primitive is): scale by its own size.
        var mesh = Resources.GetBuiltinResource<Mesh>("Cylinder.fbx");
        var size = mesh.bounds.size;
        disc.transform.localScale = new Vector3(diameter / size.x, 0.008f / size.y, diameter / size.z);
        disc.AddComponent<MeshFilter>().sharedMesh = mesh;
        var dr = disc.AddComponent<MeshRenderer>(); dr.sharedMaterial = Building.Metal; dr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        Building.Tint(dr, tint ?? new Color(0.12f, 0.15f, 0.22f));
        Building.Matte(dr);
    }

    // The notch: a short bright tick at the rim of a disc, turning about its centre.
    static Transform Notch(Transform parent, Vector3 at, float radius, float size)
    {
        var pivot = new GameObject("Notch").transform;
        pivot.SetParent(parent, false);
        pivot.localPosition = at;
        var bar = Building.Box(pivot, "Tick", new Vector3(size, size * 1.6f, 0.004f), new Vector3(0f, radius, 0f), Building.Lens, null, false);
        Building.Glow(bar.GetComponent<Renderer>(), Color.white, 0.9f);
        return pivot;
    }

    // A staff badge (Higgsfield's printed sheet, cropped into Resources/Badges/<key>) on a thin
    // white card with a clip at its top. Null when the person has no badge.
    static Transform Badge(Transform parent, string key, float height, Vector3 at)
    {
        var tex = Resources.Load<Texture2D>("Badges/" + key);
        if (tex == null) return null;
        float w = height * tex.width / tex.height;
        var root = new GameObject("Badge " + key).transform;
        root.SetParent(parent, false);
        root.localPosition = at;
        Building.Matte(Building.Box(root, "Card", new Vector3(w + 0.006f, height + 0.006f, 0.004f), Vector3.zero, Building.Metal, new Color(0.9f, 0.9f, 0.88f), false).GetComponent<Renderer>());
        var mesh = Resources.GetBuiltinResource<Mesh>("Quad.fbx");
        var size = mesh.bounds.size;
        var face = new GameObject("Face");
        face.transform.SetParent(root, false);
        face.transform.localPosition = new Vector3(0f, 0f, -0.0026f);
        face.transform.localScale = new Vector3(w / Mathf.Max(1e-4f, size.x), height / Mathf.Max(1e-4f, size.y), 1f);
        face.AddComponent<MeshFilter>().sharedMesh = mesh;
        var r = face.AddComponent<MeshRenderer>();
        r.sharedMaterial = Building.Enamel; r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        var b = new MaterialPropertyBlock();
        r.GetPropertyBlock(b);
        b.SetTexture("_BaseMap", tex); b.SetVector("_BaseMap_ST", new Vector4(1f, 1f, 0f, 0f)); b.SetColor("_BaseColor", Color.white);
        r.SetPropertyBlock(b);
        Building.Matte(r);
        Building.Box(root, "Clip", new Vector3(w * 0.32f, 0.014f, 0.008f), new Vector3(0f, height / 2f + 0.002f, -0.003f), Building.Trim, null, false);
        return root;
    }

    // A knob on the card brought close: a light face on the dark card and a dark pointer from its
    // centre to its rim, read at a glance. Returns the pointer's pivot, which turns with the value.
    static Transform LightKnob(Transform parent, Vector3 at, float diameter)
    {
        var mesh = Resources.GetBuiltinResource<Mesh>("Cylinder.fbx");
        var size = mesh.bounds.size;
        var face = new GameObject("Knob face");
        face.transform.SetParent(parent, false);
        face.transform.localPosition = at;
        face.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        face.transform.localScale = new Vector3(diameter / size.x, 0.008f / size.y, diameter / size.z);
        face.AddComponent<MeshFilter>().sharedMesh = mesh;
        var r = face.AddComponent<MeshRenderer>(); r.sharedMaterial = Building.Lens; r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        Building.Glow(r, new Color(0.84f, 0.88f, 0.93f), 0.6f);
        var pivot = new GameObject("Notch").transform;
        pivot.SetParent(parent, false);
        pivot.localPosition = at + new Vector3(0f, 0f, -0.007f);
        Building.Matte(Building.Box(pivot, "Pointer", new Vector3(0.011f, diameter * 0.44f, 0.004f), new Vector3(0f, diameter * 0.22f, 0f), Building.Metal, Building.PlateInk, false).GetComponent<Renderer>());
        Building.Matte(Building.Box(pivot, "Cap", new Vector3(0.018f, 0.018f, 0.004f), Vector3.zero, Building.Metal, Building.PlateInk, false).GetComponent<Renderer>());
        return pivot;
    }

    static TextMeshPro Label(Transform parent, string text, float size, Color colour, bool bold, Vector2 box, Vector3 at, TextAlignmentOptions align, bool wrap)
    {
        var go = new GameObject("Text");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = at;
        var t = go.AddComponent<TextMeshPro>();
        t.font = bold ? Theme.Bold : Theme.Body; t.fontSize = size; t.color = colour; t.alignment = align;
        t.rectTransform.sizeDelta = box;
        t.textWrappingMode = wrap ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
        t.overflowMode = TextOverflowModes.Overflow;
        t.text = text;
        return t;
    }

    // Something not on the list yet, standing about the room: a folder on a crate, a printer
    // box, a sheet on the wall. Tap it to pin it to the board.
    Token MakeLoose(Item item, int n, float hx0, float hx1, float hz1)
    {
        var t = new Token { item = item, key = item.key, loose = true, w = 0.3f, h = 0.2f };
        t.root = new GameObject("Loose " + item.key).transform;
        t.root.SetParent(rig, false);
        switch (item.key)
        {
            case "shared":
            {
                // A small network drive on the crate, turned toward the camera, its label on top.
                t.root.localPosition = new Vector3(searchSpot.x, 0.38f, searchSpot.z);
                if (world.RoomCamera(Facility.RoomIndex(room.room), out var eyeW, out _, out _))
                {
                    var toEye = rig.InverseTransformPoint(eyeW) - t.root.localPosition; toEye.y = 0f;
                    t.root.localRotation = Quaternion.LookRotation(-toEye.normalized);
                }
                var drive = Building.Drive(t.root);
                t.flyer = drive;
                t.slab = drive.Find("Body");
                t.namePlate = drive.Find("Label");
                Glow(t.root, new Vector3(0f, 0.6f, 0f), 1.0f);
                break;
            }
            case "printer":
            {
                t.root.localPosition = new Vector3(Mathf.Min(1.45f, hx1 + 0.35f), 0f, hz1 + 0.35f);
                t.root.localRotation = Quaternion.Euler(0f, 180f, 0f);
                t.slab = Building.Box(t.root, "Printer", new Vector3(0.44f, 0.3f, 0.4f), new Vector3(0f, 0.15f, 0f), Building.Lens, null).transform;
                Building.Glow(t.slab.GetComponent<Renderer>(), new Color(0.78f, 0.8f, 0.84f), 0.3f);
                Glow(t.root, new Vector3(0f, 0.8f, -0.3f));
                Building.Box(t.root, "Tray", new Vector3(0.3f, 0.02f, 0.12f), new Vector3(0f, 0.31f, 0.1f), Building.Trim, null, false);
                Building.Box(t.root, "Slot", new Vector3(0.3f, 0.02f, 0.01f), new Vector3(0f, 0.2f, -0.2f), Building.Trim, null, false);
                var led = Building.Box(t.root, "Led", new Vector3(0.015f, 0.015f, 0.01f), new Vector3(0.16f, 0.26f, -0.2f), Building.Lens, null, false);
                Building.Glow(led.GetComponent<Renderer>(), Theme.Teal, 1.2f);
                break;
            }
            default:
            {
                // The canteen menu, a sheet pinned on the back wall beside the board.
                t.root.localPosition = new Vector3(Mathf.Min(1.5f, hx1 + 0.75f), 1.35f, -3.565f);
                t.root.localRotation = Quaternion.Euler(0f, 180f, 0f);
                t.slab = Building.Box(t.root, "Sheet", new Vector3(0.21f, 0.29f, 0.008f), Vector3.zero, Building.Lens, null).transform;
                Building.Glow(t.slab.GetComponent<Renderer>(), Paper, 0.45f);
                Glow(t.root, new Vector3(0f, 0.1f, -0.35f), 1.0f);
                for (int i = 0; i < 5; i++) Building.Box(t.root, "Line", new Vector3(0.14f, 0.006f, 0.004f), new Vector3(0f, 0.08f - i * 0.04f, -0.006f), Building.Trim, null, false);
                var pin = Building.Box(t.root, "Pin", new Vector3(0.016f, 0.016f, 0.01f), new Vector3(0f, 0.13f, -0.008f), Building.Lens, null, false);
                Building.Glow(pin.GetComponent<Renderer>(), Look.Breached, 1f);
                break;
            }
        }
        if (item.key != "shared") t.root.localScale = Vector3.one * Mathf.Max(scale, 0.75f);
        t.homeLoose = t.root.localPosition; t.slot = t.homeLoose; t.slotRot = t.root.localRotation;
        return t;
    }

    // 2.4's firewall rack: the patch panel (Higgsfield) is its bottom unit, under the five rule
    // modules, its cables hanging in front; the rails run down to it.
    void MountPatchPanel()
    {
        var prefab = Resources.Load<GameObject>("Facility/Hero/PatchPanel");
        if (prefab == null) return;
        var panel = Instantiate(prefab, rig, false).transform;
        panel.name = "Patch panel";
        var b = LocalBounds(panel);
        float width = (gridRight - gridLeft) * 0.94f;
        float k = width / Mathf.Max(0.01f, b.size.x);
        float room_ = gridBottom - 0.03f * scale - Mathf.Max(floorY, 0f) - 0.03f; // what is left above the floor
        if (b.size.y * k > room_) k = room_ / Mathf.Max(0.01f, b.size.y);
        panel.localScale *= k;
        b = LocalBounds(panel);
        float top = gridBottom - 0.025f * scale;
        panel.localPosition += new Vector3(cx - b.center.x, top - b.max.y, (zFace - 0.03f) - b.min.z);
        rackBottom = top - b.size.y;
        if (DemoDriver.Wanted) Debug.Log($"[Panel] {room.id}: {b.size.x * 1f:0.00} x {b.size.y:0.00} x {b.size.z:0.00} m under the rack, its top at {top:0.00}");
    }

    // A thing's renderers' box, in the rig's own space.
    Bounds LocalBounds(Transform t)
    {
        var rs = t.GetComponentsInChildren<Renderer>();
        var bb = new Bounds(rig.InverseTransformPoint(t.position), Vector3.zero);
        bool first = true;
        foreach (var r in rs)
            for (int i = 0; i < 8; i++)
            {
                var c = r.bounds.center + Vector3.Scale(r.bounds.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
                var l = rig.InverseTransformPoint(c);
                if (first) { bb = new Bounds(l, Vector3.zero); first = false; } else bb.Encapsulate(l);
            }
        return bb;
    }

    // Where the unlisted thing waits: a clear patch of floor, seen from the camera with nothing in
    // front of it, just outside the feed as it opens and within reach of the camera turned.
    Vector3 SearchSpot(Vector3 keepClear)
    {
        var fallback = new Vector3(1.3f, 0f, -1.6f);
        if (!world.RoomCamera(Facility.RoomIndex(room.room), out var eyeW, out var lookW, out float fov)) return fallback;
        float aspect = (float)Screen.width / Mathf.Max(1, Screen.height);
        float half = fov * 0.5f, hhalf = Mathf.Atan(Mathf.Tan(half * Mathf.Deg2Rad) * aspect) * Mathf.Rad2Deg;
        var fwd = (lookW - eyeW).normalized;
        var obstacles = roomRoot.GetComponentsInChildren<Renderer>()
            .Where(r => !r.transform.IsChildOf(rig) && !(r is ParticleSystemRenderer) && r.GetComponent<TextMeshPro>() == null
                        && (r.sharedMaterial == null || r.sharedMaterial.renderQueue < 3000) && r.enabled && r.gameObject.activeInHierarchy
                        && r.bounds.size.x < 2.5f && r.bounds.size.y < 2.5f && r.bounds.size.z < 2.5f && !r.bounds.Contains(eyeW))
            .Select(r => r.bounds).ToList();
        var hb = world.HeroBounds(Facility.RoomIndex(room.room)); obstacles.Add(hb);
        Vector3 best = fallback; float bestScore = float.MinValue;
        for (float x = -1.45f; x <= 1.46f; x += 0.15f)
            for (float z = -0.6f; z >= -3.25f; z -= 0.15f)
            {
                var p = new Vector3(x, 0.47f, z);
                var pw = rig.TransformPoint(p);
                if (Vector3.Dot(pw - eyeW, fwd) < 1.0f) continue;
                if (Vector2.Distance(new Vector2(x, z), new Vector2(keepClear.x, keepClear.z)) < 0.7f) continue;
                // The turn that centres it, and whether the camera's reach brings it well inside the feed.
                var turn = TurnFor(pw - eyeW, lookW - eyeW);
                float offX = Mathf.Abs(turn.x) - MaxYaw, offY = turn.y - MaxPitch;
                if (offX > hhalf - 14f || offY > half - 10f || turn.y < MinPitch - (half - 10f)) continue;
                var v = View(p);
                var foot = new Bounds(rig.TransformPoint(new Vector3(x, 0.25f, z)), new Vector3(0.55f, 0.5f, 0.55f));
                if (obstacles.Any(o => o.Intersects(foot))) continue;
                var ray = new Ray(eyeW, (pw - eyeW).normalized); float dist = Vector3.Distance(eyeW, pw);
                if (obstacles.Any(o => o.IntersectRay(ray, out float hit) && hit < dist - 0.25f)) continue;
                bool hidden = !InFrame(p, -3f);
                float score = (hidden ? 100f : 0f) - Mathf.Abs(Mathf.Abs(turn.x) - (hhalf + 6f)) - Mathf.Max(0f, turn.y) * 0.5f;
                if (score > bestScore) { bestScore = score; best = new Vector3(x, 0f, z); }
            }
        if (DemoDriver.Wanted) Debug.Log($"[Search] {room.id}: camera at {rig.InverseTransformPoint(eyeW)}, the crate at {best}, {(bestScore >= 50f ? "out of the opening view" : bestScore > float.MinValue ? "in the opening view" : "no clear spot")}, view {View(best + Vector3.up * 0.47f)}");
        return best;
    }

    const float MaxYaw = 36f, MaxPitch = 22f, MinPitch = -8f;

    // The turn of the camera (yaw right, pitch down, in degrees) that brings a direction to the
    // middle of the feed: the camera turns about the world's up, then tilts, so yaw and pitch add.
    static Vector2 TurnFor(Vector3 to, Vector3 baseDir)
    {
        static Vector2 YawPitch(Vector3 d) { d.Normalize(); return new Vector2(Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg, -Mathf.Asin(Mathf.Clamp(d.y, -1f, 1f)) * Mathf.Rad2Deg); }
        var a = YawPitch(baseDir); var b = YawPitch(to);
        return new Vector2(Mathf.DeltaAngle(a.x, b.x), b.y - a.y);
    }

    // How far to turn the camera to bring a loose thing to the middle of the feed, within reach.
    public Vector2 LookToward(string key)
    {
        var t = tokens.FirstOrDefault(x => x.key == key && x.loose);
        if (t == null || !world.RoomCamera(Facility.RoomIndex(room.room), out var eyeW, out var lookW, out _)) return Vector2.zero;
        var src = t.slab != null ? t.slab : t.root;
        var turn = TurnFor(src.position - eyeW, lookW - eyeW);
        return new Vector2(Mathf.Clamp(turn.x, -MaxYaw, MaxYaw), Mathf.Clamp(turn.y, MinPitch, MaxPitch));
    }

    // A small warm lamp over a loose thing, so it shows outside the hero's pool of light.
    static void Glow(Transform parent, Vector3 at, float range = 1.3f)
    {
        var go = new GameObject("Glow");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = at;
        var l = go.AddComponent<Light>();
        l.type = LightType.Point; l.range = range * 0.7f; l.intensity = 0.9f; l.color = new Color(1f, 0.9f, 0.76f); l.shadows = LightShadows.None;
    }

    // ---- The state on the things ----

    public void Refresh()
    {
        foreach (var t in tokens)
        {
            if (t.sign)
            {
                Building.SetPlate(t.namePlate, st.signed ? "Signed" : "Sign", st.signed ? Theme.Teal : Color.white);
                Building.Glow(t.switches[0].lamp, st.signed ? Theme.Teal : Theme.Amber);
                continue;
            }
            foreach (var sw in t.switches) Show(sw);
        }
        foreach (var sw in rows) Show(sw);
        ShowLamps();
    }

    // Each row's lamp on the board, and the card brought close: teal once every decision on the
    // row is made, amber while one is still a question mark. Whether it was right, only the
    // attack shows.
    public void ShowLamps()
    {
        foreach (var t in tokens)
            if (t.lamp != null) { bool ok = AllSet(t); Building.Glow(t.lamp, ok ? Theme.Teal : Theme.Amber, ok ? 1.8f : 1.5f); }
        if (focusLamp != null && selected != null) { bool ok = AllSet(selected); Building.Glow(focusLamp, ok ? Theme.Teal : Theme.Amber, ok ? 1.8f : 1.5f); }
    }

    bool AllSet(Token t) => t.fields.All(f => { int p = st.Pick(t.key, f.key); return p >= 0 && p < f.options.Length && f.options[p] != "?"; });

    void Show(Switch sw)
    {
        var f = sw.field;
        int idx = st.Pick(sw.itemKey, f.key);
        bool unset = idx < 0 || idx >= f.options.Length || f.options[idx] == "?";
        bool na = !unset && f.options[idx] == "n/a";
        if (sw.lamp != null) Building.Glow(sw.lamp, unset ? Theme.Amber : Theme.Teal, unset ? 1.4f : 1.8f);
        int first = f.options[0] == "?" || f.options[0] == "n/a" ? 1 : 0;
        int real = f.options.Length - first;
        float a = unset || na ? -150f : Mathf.Lerp(-120f, 120f, real > 1 ? (idx - first) / (float)(real - 1) : 0.5f);
        if (sw.notch != null) sw.notch.localRotation = Quaternion.Euler(0f, 0f, -a);
        string value = unset ? "?" : Script.Option(room, sw.itemKey, f, idx);
        // On the board a knob shows its value only just after it is turned; the card brought close shows them all.
        if (sw.face != null) { sw.face.text = lastTurned == sw.itemKey + "/" + f.key ? value : ""; sw.face.color = unset ? Theme.Amber : na ? Dim : Color.white; }
        if (sw.row != null)
        {
            string full = unset ? "?" : Script.Long(room, sw.itemKey, f, idx);
            sw.row.text = $"<color=#9EAEBB>{f.label}:</color> {full}";
            sw.row.color = unset ? Theme.Amber : Color.white;
        }
    }

    // A tap on a row turns its decision to the next choice (skipping the question mark); the
    // knob on the board turns with it.
    void Turn(Switch sw)
    {
        var f = sw.field;
        int next = (st.Pick(sw.itemKey, f.key) + 1) % f.options.Length;
        if (f.options[next] == "?" && f.options.Length > 1) next = (next + 1) % f.options.Length;
        st.Set(sw.itemKey, f.key, next);
        Sfx.Tick();
        lastTurned = sw.itemKey + "/" + f.key;
        foreach (var t in tokens) foreach (var k in t.switches) Show(k);
        Show(sw);
        StartCoroutine(Pop(sw.rowSlab != null ? sw.rowSlab : sw.root, 1.04f));
        changed?.Invoke();
    }

    public void Select(Token t)
    {
        if (selected == t) return;
        if (selected != null) { selected.selected = false; if (!selected.pulsing) Building.Halo(selected.namePlate, false); }
        CloseFocus();
        selected = t;
        if (t == null) { Selected?.Invoke(null); return; }
        if (t.sign || t.loose) return;
        t.selected = true;
        Building.Halo(t.namePlate, true);
        OpenFocus(t);
        Sfx.Whoosh();
        Selected?.Invoke(t);
    }

    // The card brought close: flat, facing the camera, in front of the board. Its full name and
    // line at the top, then one row per decision, "Owner: Omar", each with its lamp and knob; a
    // pinned card has a last row to take it off the board again.
    void OpenFocus(Token t)
    {
        const float W = 0.96f, Pad = 0.05f, RH = 0.1f, RG = 0.018f, TextW = W - 0.1f;
        focus = new GameObject("Card close").transform;
        focus.SetParent(rig, false);
        focus.localRotation = Quaternion.Euler(0f, 180f, 0f);
        string titleText = t.console ? t.item.name : t.item.name;
        const float TitleW = TextW - 0.06f; // room at its right for the card's lamp
        var title = Label(focus, titleText, 0.5f, Color.white, true, new Vector2(TitleW, 0.5f), Vector3.zero, TextAlignmentOptions.TopLeft, true);
        float titleH = title.GetPreferredValues(titleText, TitleW, 0f).y;
        TextMeshPro line = null; float lineH = 0f;
        if (!string.IsNullOrEmpty(t.item.detail))
        {
            line = Label(focus, t.item.detail, 0.34f, new Color(0.62f, 0.68f, 0.76f), false, new Vector2(TextW, 0.5f), Vector3.zero, TextAlignmentOptions.TopLeft, true);
            lineH = line.GetPreferredValues(t.item.detail, TextW, 0f).y;
        }
        bool canUnpin = t.pinned && t.item.hidden;
        int n = t.fields.Count + (canUnpin ? 1 : 0);
        float H = Pad + titleH + (line != null ? 0.012f + lineH : 0f) + 0.035f + n * (RH + RG) - RG + Pad;
        float y = H / 2f - Pad;
        title.rectTransform.sizeDelta = new Vector2(TitleW, titleH + 0.01f);
        title.transform.localPosition = new Vector3(-0.03f, y - titleH / 2f, -0.016f);
        focusLamp = Building.Lamp(focus, new Vector3(W / 2f - 0.05f, y - 0.04f, -0.016f), 0.026f, Theme.Amber);
        y -= titleH;
        if (line != null)
        {
            y -= 0.012f;
            line.rectTransform.sizeDelta = new Vector2(TextW, lineH + 0.01f);
            line.transform.localPosition = new Vector3(0f, y - lineH / 2f, -0.016f);
            y -= lineH;
        }
        y -= 0.035f;
        focusSlab = Building.Box(focus, "Card", new Vector3(W, H, 0.02f), new Vector3(0f, 0f, 0.012f), Building.Metal, Building.PlateInk).transform;
        Building.Matte(focusSlab.GetComponent<Renderer>());
        var keyline = Building.Box(focus, "Keyline", new Vector3(0.012f, H - 0.06f, 0.004f), new Vector3(-W / 2f + 0.012f, 0f, -0.002f), Building.Lens, null, false);
        Building.Glow(keyline.GetComponent<Renderer>(), Theme.Teal, 1.1f);
        foreach (var f in t.fields)
        {
            var sw = new Switch { field = f, itemKey = t.key };
            sw.root = new GameObject("Row " + f.key).transform;
            sw.root.SetParent(focus, false);
            sw.root.localPosition = new Vector3(0f, y - RH / 2f, 0f);
            sw.rowSlab = Building.Box(sw.root, "Row", new Vector3(W - 0.06f, RH, 0.01f), new Vector3(0f, 0f, -0.002f), Building.Metal, new Color(0.14f, 0.19f, 0.3f), false).transform;
            Building.Matte(sw.rowSlab.GetComponent<Renderer>());
            sw.row = Label(sw.root, "", 0.42f, Color.white, true, new Vector2(W - 0.22f, RH * 0.8f), new Vector3(-W / 2f + 0.05f + (W - 0.22f) / 2f, 0f, -0.012f), TextAlignmentOptions.MidlineLeft, false);
            sw.row.enableAutoSizing = true; sw.row.fontSizeMax = 0.38f; sw.row.fontSizeMin = 0.22f;
            sw.notch = LightKnob(sw.root, new Vector3(W / 2f - 0.075f, 0f, -0.01f), 0.076f);
            rows.Add(sw);
            Show(sw);
            y -= RH + RG;
        }
        // One text size for every row of the card: the one its longest possible line needs, so the
        // rows read alike and none jumps in size when it is turned.
        if (rows.Count > 0)
        {
            float avail = W - 0.22f, size = 0.38f;
            foreach (var sw in rows)
                for (int i = 0; i < sw.field.options.Length; i++)
                {
                    string full = $"{sw.field.label}: {Script.Long(room, sw.itemKey, sw.field, i)}";
                    sw.row.enableAutoSizing = false; sw.row.fontSize = 0.38f;
                    float wNeed = sw.row.GetPreferredValues(full, 0f, 0f).x;
                    if (wNeed > avail) size = Mathf.Min(size, 0.38f * avail / wNeed);
                }
            size = Mathf.Max(size, 0.24f);
            foreach (var sw in rows) { sw.row.enableAutoSizing = false; sw.row.fontSize = size; sw.row.overflowMode = TextOverflowModes.Ellipsis; Show(sw); }
        }
        if (canUnpin)
        {
            unpinRow = new GameObject("Unpin").transform;
            unpinRow.SetParent(focus, false);
            unpinRow.localPosition = new Vector3(0f, y - RH / 2f, 0f);
            var slab = Building.Box(unpinRow, "Row", new Vector3(W - 0.06f, RH, 0.01f), new Vector3(0f, 0f, -0.002f), Building.Metal, new Color(0.3f, 0.12f, 0.12f), false);
            Building.Matte(slab.GetComponent<Renderer>());
            Label(unpinRow, "Take it off the board", 0.4f, new Color(1f, 0.6f, 0.55f), true, new Vector2(W - 0.2f, RH * 0.8f), new Vector3(0f, 0f, -0.012f), TextAlignmentOptions.Center, false);
        }
        // Where it stands: in front of the card it opens, inside the room, clear of the floor.
        float fx = Mathf.Clamp(t.slot.x, -1.7f + W / 2f, 1.7f - W / 2f);
        float fy = Mathf.Clamp(t.slot.y, Mathf.Max(floorY, 0f) + 0.12f + H / 2f, 2.3f - H / 2f);
        focusCentre = new Vector3(fx, fy, zFace + 0.3f);
        focus.localPosition = focusCentre;
        // The loose things in front of the board would poke through it, and the lamp's cone and its
        // dust would wash over it: they step aside while it is open.
        foreach (var x in tokens) if (x.loose) x.root.gameObject.SetActive(false);
        foreach (Transform c in roomRoot.GetComponentsInChildren<Transform>(true))
            if ((c.name == "Beam" || c.name == "Dust") && !c.IsChildOf(rig)) { c.gameObject.SetActive(false); hiddenForFocus.Add(c.gameObject); }
        FitFocus(W, H);
        ShowLamps();
    }

    // The camera faces the card squarely, as far back as fits the whole card with a margin for
    // the bars at the top and the bottom of the screen; when something in the room stands in the
    // way (the bin, the chair, the desk), the camera comes closer and the card shrinks with it.
    void FitFocus(float W, float H)
    {
        focusFov = 52f;
        float aspect = (float)Screen.width / Mathf.Max(1, Screen.height);
        float tv = Mathf.Tan(focusFov * 0.5f * Mathf.Deg2Rad), th = tv * aspect;
        float d0 = Mathf.Max(H * 0.5f * 1.4f / tv, W * 0.5f * 1.15f / th);
        var blockers = roomRoot.GetComponentsInChildren<Renderer>()
            .Where(r => !r.transform.IsChildOf(rig) && !(r is ParticleSystemRenderer) && r.GetComponent<TextMeshPro>() == null
                        && (r.sharedMaterial == null || r.sharedMaterial.renderQueue < 3000) && r.bounds.size.x < 2.5f && r.bounds.size.y < 2.5f && r.bounds.size.z < 2.5f
                        && r.enabled && r.gameObject.activeInHierarchy)
            .Select(r => r.bounds).ToList();
        float s = 1f;
        for (int guard = 0; guard < 10; guard++)
        {
            float d = d0 * s;
            var eyeL = focusCentre + new Vector3(0f, 0f, d);
            if (eyeL.z <= -0.4f && !Blocked(eyeL, W * s, H * s, blockers)) break;
            s *= 0.85f;
        }
        s = Mathf.Max(s, 0.3f);
        focus.localScale = Vector3.one * s;
        focusD = d0 * s;
        if (DemoDriver.Wanted) Debug.Log($"[Focus] {room.id}: card {W:0.00} x {H:0.00} m at {s:P0}, camera {focusD:0.00} m off");
    }

    bool Blocked(Vector3 eyeL, float w, float h, List<Bounds> blockers)
    {
        var eyeW = rig.TransformPoint(eyeL);
        for (int i = 0; i <= 4; i++)
            for (int j = 0; j <= 4; j++)
            {
                var pL = focusCentre + new Vector3((i / 4f - 0.5f) * w, (j / 4f - 0.5f) * h, 0.01f);
                var pW = rig.TransformPoint(pL);
                var ray = new Ray(eyeW, (pW - eyeW).normalized);
                float dist = Vector3.Distance(eyeW, pW);
                foreach (var b in blockers) if (b.IntersectRay(ray, out float hit) && hit < dist - 0.02f) return true;
                if (blockers.Any(b => b.Contains(eyeW))) return true;
            }
        return false;
    }

    // Where the camera stands to face the card brought close.
    public bool FocusView(out Vector3 eye, out Vector3 look, out float fov)
    {
        eye = look = Vector3.zero; fov = focusFov;
        if (focus == null) return false;
        look = rig.TransformPoint(focusCentre);
        eye = rig.TransformPoint(focusCentre + new Vector3(0f, 0f, focusD));
        return true;
    }

    void CloseFocus()
    {
        if (focus != null) Destroy(focus.gameObject);
        focus = null; focusSlab = null; unpinRow = null; focusLamp = null;
        rows.Clear();
        foreach (var x in tokens) if (x.loose && x.root != null) x.root.gameObject.SetActive(true);
        foreach (var g in hiddenForFocus) if (g != null) g.SetActive(true);
        hiddenForFocus.Clear();
    }
    readonly List<GameObject> hiddenForFocus = new List<GameObject>();

    // The planned mistake's things pulse after a rehearsal: a halo on their plates.
    public void Pulse(IEnumerable<string> keys, bool on)
    {
        foreach (var t in tokens)
        {
            if (t.sign || !keys.Contains(t.key)) continue;
            t.pulsing = on;
            if (t.namePlate != null) Building.Halo(t.namePlate, on || t.selected);
        }
    }

    // Unpinning: a card that was found in the room goes back where it lay.
    void Unpin(Token t)
    {
        st.added.Remove(t.key); st.order.Remove(t.key);
        foreach (var f in room.fields) st.picks.Remove(t.key + "/" + f.key);
        Sfx.Ruin();
        Rebuild();
        changed?.Invoke();
    }

    void Rebuild()
    {
        Select(null);
        var pulsing = tokens.Where(x => x.pulsing).Select(x => x.key).ToList();
        foreach (var t in tokens) Destroy(t.root.gameObject);
        tokens.Clear(); rows.Clear(); signPad = null;
        ConsoleX = null; ConsoleRect = null; ConsoleView = null;
        foreach (Transform c in rig) Destroy(c.gameObject);
        Make(Facility.RoomIndex(room.room));
        Pulse(pulsing, true);
    }

    // The walk's fix, and a reset: the state applied through the rig as a player would leave it.
    public void Apply(Dictionary<string, int> picks, string[] order, bool sign)
    {
        foreach (var kv in picks) st.picks[kv.Key] = kv.Value;
        foreach (var k in picks.Keys.Select(k => k.Split('/')[0]).Distinct())
        {
            var item = room.items.FirstOrDefault(i => i.key == k);
            if (item != null && item.hidden && !st.added.Contains(k)) { st.added.Add(k); st.order.Add(k); }
        }
        if (order != null) { st.order.Clear(); st.order.AddRange(order); }
        if (sign && room.signable) { st.signed = true; st.signedBy = "You, as the reviewer"; st.signedOn = DateTime.Now.ToString("d MMM yyyy"); }
        changed?.Invoke();
        Rebuild();
    }

    // ---- Picking: a tap on a card, a tap on a row of the card brought close, a drag up or down the rack ----

    void Update()
    {
        if (!Interactive || cam == null) return;
        foreach (var t in tokens)
        {
            if (t.dragging || t.root == null || !t.root.gameObject.activeSelf) continue;
            t.root.localPosition = Vector3.Lerp(t.root.localPosition, t.slot, 1f - Mathf.Exp(-10f * Time.unscaledDeltaTime));
            if (t.pulsing && t.namePlate != null) { var halo = t.namePlate.Find("Halo"); if (halo != null) Building.Glow(halo.GetComponent<Renderer>(), Theme.Teal, 0.8f + 0.8f * Mathf.Sin(Time.unscaledTime * 5f)); }
        }
        // Something not on the list yet glints as the camera passes it: when it comes into the
        // feed, or crosses its middle, while the camera is turning.
        var lookNow = new Vector2(world.cam.lookYaw, world.cam.lookPitch);
        if ((lookNow - lookWas).sqrMagnitude > 0.0004f) lastLook = Time.unscaledTime;
        lookWas = lookNow;
        bool turning = Time.unscaledTime - lastLook < 0.4f;
        foreach (var t in tokens)
        {
            if (!t.loose || t.root == null || !t.root.gameObject.activeInHierarchy) continue;
            var at = (t.slab != null ? t.slab : t.root).position;
            var c = cam.WorldToViewportPoint(at);
            bool inView = c.z > 0f && c.x > 0.05f && c.x < 0.95f && c.y > 0.05f && c.y < 0.95f;
            bool middle = inView && Mathf.Abs(c.x - 0.5f) < 0.2f && Mathf.Abs(c.y - 0.5f) < 0.3f;
            if (turning && !flying && ((inView && !t.wasInView) || middle) && Time.unscaledTime - t.glintAt > 1.8f) Glint(t);
            t.wasInView = inView;
        }
        if (GameMenu.Paused || FileSheet.IsOpen || flying) return;
        // The look around, where something is still to be found: the arrow keys turn the camera.
        if (lookAround && focus == null)
        {
            float kx = (Input.GetKey(KeyCode.RightArrow) ? 1f : 0f) - (Input.GetKey(KeyCode.LeftArrow) ? 1f : 0f);
            float ky = (Input.GetKey(KeyCode.DownArrow) ? 1f : 0f) - (Input.GetKey(KeyCode.UpArrow) ? 1f : 0f);
            if (kx != 0f || ky != 0f) TurnCamera(kx * 45f * Time.unscaledDeltaTime, ky * 35f * Time.unscaledDeltaTime);
        }
        var pos = (Vector2)Input.mousePosition;
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
            pointerDown = true; downAt = pos; dragWas = pos;
            if (rack && focus == null) held = PickToken(pos, 90f, true);
            // A drag that starts on the room itself, not on a card, turns the camera.
            panning = lookAround && focus == null && held == null && PickToken(pos, 60f, false) == null;
        }
        if (pointerDown && Input.GetMouseButton(0) && held != null && !held.dragging && (pos - downAt).magnitude > 10f) { held.dragging = true; Select(null); }
        if (held != null && held.dragging) Drag(pos);
        if (pointerDown && panning && Input.GetMouseButton(0) && (pos - downAt).magnitude > 6f)
        {
            var d = pos - dragWas;
            TurnCamera(-d.x * 0.11f, d.y * 0.11f);
        }
        dragWas = pos;
        if (Input.GetMouseButtonUp(0) && pointerDown)
        {
            pointerDown = false;
            if (held != null && held.dragging) { held.dragging = false; held = null; Settle(); return; }
            held = null;
            if ((pos - downAt).magnitude > 14f) return;
            if (focus != null)
            {
                // The card brought close: a row turns, the last row unpins, outside the card closes it.
                foreach (var sw in rows) if (Inside(sw.rowSlab, pos)) { Turn(sw); return; }
                if (unpinRow != null && Inside(unpinRow.Find("Row"), pos)) { var t0 = selected; Unpin(t0); return; }
                if (Inside(focusSlab, pos)) return;
                Select(null);
                return;
            }
            var t = PickToken(pos, 95f, false);
            if (t == null) return;
            if (t.loose) { StartCoroutine(FlyAndPin(t)); return; }
            if (t.sign) { st.signed = true; st.signedBy = "You, as the reviewer"; st.signedOn = DateTime.Now.ToString("d MMM yyyy"); Sfx.Buy(); Refresh(); changed?.Invoke(); return; }
            Select(t);
        }
    }

    // The security camera turned by the player, within its mount's reach.
    void TurnCamera(float yaw, float pitch)
    {
        world.cam.lookYaw = Mathf.Clamp(world.cam.lookYaw + yaw, -MaxYaw, MaxYaw);
        world.cam.lookPitch = Mathf.Clamp(world.cam.lookPitch + pitch, MinPitch, MaxPitch);
    }

    public float LastGlint { get; private set; } = -10f;
    public float GlintAt(string key) { var t = tokens.FirstOrDefault(x => x.key == key && x.loose); return t != null ? t.glintAt : -10f; }

    // A glint: a four-pointed star of light that flares and fades on the thing, with a small sound.
    void Glint(Token t)
    {
        t.glintAt = Time.unscaledTime; LastGlint = Time.unscaledTime;
        Sfx.Glint();
        StartCoroutine(DoGlint(t));
    }

    System.Collections.IEnumerator DoGlint(Token t)
    {
        var src = t.slab != null ? t.slab : t.root;
        var r = src.GetComponent<Renderer>();
        var b = r != null ? r.bounds : new Bounds(src.position, Vector3.one * 0.2f);
        var star = new GameObject("Glint").transform;
        star.SetParent(roomRoot, false);
        star.position = b.center + Vector3.up * b.extents.y * 0.7f;
        foreach (var size in new[] { new Vector3(0.3f, 0.014f, 0.004f), new Vector3(0.014f, 0.3f, 0.004f), new Vector3(0.05f, 0.05f, 0.004f) })
        {
            var ray = Building.Box(star, "Ray", size, Vector3.zero, Building.Lens, null, false);
            Building.Glow(ray.GetComponent<Renderer>(), new Color(1f, 0.97f, 0.88f), 7f);
        }
        const float D = 0.55f;
        for (float e = 0f; e < D && star != null; e += Time.unscaledDeltaTime)
        {
            float k = Mathf.Sin(e / D * Mathf.PI);
            star.localScale = Vector3.one * Mathf.Max(0.001f, k);
            star.rotation = Quaternion.LookRotation(star.position - cam.transform.position) * Quaternion.Euler(0f, 0f, 45f * e / D);
            yield return null;
        }
        if (star != null) Destroy(star.gameObject);
    }

    // Tapping a thing in the room: it flies up to the board and becomes a card there, while the
    // camera turns back to the board.
    System.Collections.IEnumerator FlyAndPin(Token t)
    {
        flying = true;
        var fly = t.flyer != null ? t.flyer : t.root;
        fly.SetParent(roomRoot, true);
        if (t.flyer == null) { t.root = new GameObject("Loose " + t.key + " (flown)").transform; t.root.SetParent(rig, false); } // the rebuild clears this, not the thing in flight
        var fromW = fly.position; var rotW = fly.rotation; var scale0 = fly.localScale;
        float yaw0 = world.cam.lookYaw, pitch0 = world.cam.lookPitch;
        Sfx.Whoosh();
        st.added.Add(t.key); st.order.Add(t.key);
        Rebuild();
        changed?.Invoke();
        world.cam.lookYaw = yaw0; world.cam.lookPitch = pitch0;
        var card = tokens.FirstOrDefault(x => x.key == t.key && !x.loose);
        var toW = card != null ? card.root.position : fromW;
        var toRot = card != null ? card.root.rotation : rotW;
        if (card != null) card.root.gameObject.SetActive(false);
        const float D = 0.9f;
        for (float e = 0f; e < D; e += Time.unscaledDeltaTime)
        {
            float k = Mathf.SmoothStep(0f, 1f, e / D);
            fly.position = Vector3.Lerp(fromW, toW, k) + Vector3.up * Mathf.Sin(k * Mathf.PI) * 0.45f;
            fly.rotation = Quaternion.Slerp(rotW, toRot, k);
            fly.localScale = scale0 * Mathf.Lerp(1f, 0.55f, k);
            world.cam.lookYaw = Mathf.Lerp(yaw0, 0f, k); world.cam.lookPitch = Mathf.Lerp(pitch0, 0f, k);
            yield return null;
        }
        world.cam.lookYaw = 0f; world.cam.lookPitch = 0f;
        if (fly != null) Destroy(fly.gameObject);
        if (card != null && card.root != null) { card.root.gameObject.SetActive(true); StartCoroutine(Pop(card.root, 1.15f)); }
        Sfx.Chime();
        flying = false;
    }

    // The walk's tap on a thing in the room.
    public void Find(string key)
    {
        var t = tokens.FirstOrDefault(x => x.key == key && x.loose);
        if (t != null && !flying) StartCoroutine(FlyAndPin(t));
    }
    public bool Flying => flying;

    // The walk's turn of a row on the card brought close.
    public void TurnRow(int i) { if (i >= 0 && i < rows.Count) Turn(rows[i]); }

    // The rows in the grid's order (the board of lamps and the lamps at the top).
    public List<string> RowKeys() => tokens.Where(x => !x.loose && !x.sign && !x.console).OrderBy(x => x.order).Select(x => x.key).ToList();

    // Whether a screen point falls on a box's face, as the camera sees it.
    bool Inside(Transform box, Vector2 screen)
    {
        if (box == null) return false;
        var r = box.GetComponent<Renderer>();
        if (r == null) return false;
        var b = r.bounds;
        float x0 = 1e9f, x1 = -1e9f, y0 = 1e9f, y1 = -1e9f;
        for (int i = 0; i < 8; i++)
        {
            var c = b.center + Vector3.Scale(b.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
            var s = cam.WorldToScreenPoint(c);
            if (s.z < 0) return false;
            x0 = Mathf.Min(x0, s.x); x1 = Mathf.Max(x1, s.x); y0 = Mathf.Min(y0, s.y); y1 = Mathf.Max(y1, s.y);
        }
        return screen.x >= x0 && screen.x <= x1 && screen.y >= y0 && screen.y <= y1;
    }

    Token PickToken(Vector2 screen, float reach, bool gridOnly)
    {
        Token best = null; float bestD = reach;
        foreach (var t in tokens)
        {
            if (t.root == null || !t.root.gameObject.activeInHierarchy) continue;
            if (gridOnly && (t.loose || t.sign || t.console)) continue;
            if (!t.loose && !t.sign && Inside(t.slab, screen)) return t;
            var centre = t.slab != null ? t.slab.position : t.root.position;
            var s = cam.WorldToScreenPoint(centre);
            if (s.z < 0) continue;
            float d = Vector2.Distance(new Vector2(s.x, s.y), screen);
            if (d < bestD) { bestD = d; best = t; }
        }
        return best;
    }

    // Dragging a module: it follows the pointer up and down the rack's plane and swaps places
    // with the module it passes.
    void Drag(Vector2 screen)
    {
        var plane = new Plane(roomRoot.forward, rig.TransformPoint(new Vector3(cx, gridTop, zFace - 0.05f)));
        var ray = cam.ScreenPointToRay(screen);
        if (!plane.Raycast(ray, out float enter)) return;
        var local = rig.InverseTransformPoint(ray.GetPoint(enter));
        float y = Mathf.Clamp(local.y, gridBottom - 0.05f, gridTop + 0.05f);
        held.root.localPosition = new Vector3(held.slot.x, y, zFace + 0.06f * scale);
        var grid = tokens.Where(t => !t.loose && !t.sign && !t.console).OrderBy(t => t.order).ToList();
        int nearest = held.order; float best = float.MaxValue;
        for (int k = 0; k < grid.Count; k++) { float d = Mathf.Abs(Slot(k).y - y); if (d < best) { best = d; nearest = k; } }
        if (nearest != held.order)
        {
            string key = held.key;
            st.order.Remove(key);
            st.order.Insert(Mathf.Clamp(nearest, 0, st.order.Count), key);
            for (int k = 0; k < grid.Count; k++) { var t = grid[k]; t.order = st.order.IndexOf(t.key); t.slot = Slot(t.order); }
            Sfx.Tick();
        }
    }

    void Settle() { changed?.Invoke(); }

    System.Collections.IEnumerator Pop(Transform t, float by)
    {
        if (t == null) yield break;
        float d = 0.18f, e = 0f;
        var s0 = t.localScale;
        while (e < d && t != null) { e += Time.unscaledDeltaTime; float k = e / d; t.localScale = s0 * (1f + (by - 1f) * Mathf.Sin(k * Mathf.PI)); yield return null; }
        if (t != null) t.localScale = s0;
    }

    // Whether every per-row decision of each listed item is made, in the grid's order (the board of lamps).
    public List<bool> Decided()
    {
        var perRow = room.fields.Where(f => !f.once).ToList();
        var list = new List<bool>();
        foreach (var t in tokens.Where(x => !x.loose && !x.sign && !x.console).OrderBy(x => x.order))
            list.Add(perRow.All(f => { int p = st.Pick(t.key, f.key); return p >= 0 && p < f.options.Length && f.options[p] != "?"; }));
        return list;
    }
}
