using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

// The room's rows as things on the room's prop (the game-feel pass): every item is a token
// (a card on the risk board, a lever module in the firewall rack, an envelope in the sorter, a
// badge at the turnstile, a ticket on the bench) with its short name on a dark plate; every
// decision is a switch on its token (a lever for two choices, a dial for more) with a lamp that
// is amber while undecided and teal once set (red for the choice that closes or flags). Tap a
// token to bring it close: its full name and its line show, and each switch shows its name and
// its value; tap a switch to turn it. Rows that must be ordered are a rack: drag a module up or
// down. The room's one-off decisions sit on a small console beside the grid, the record's
// signature pad under it, and what is not on the list yet lies about the room to be found and
// pinned. Everything writes the same RoomState the drawer's chips wrote, so the checks and the
// report read the same values. Made from the kit's own materials; no model was added.
public class RoomRig : MonoBehaviour
{
    public class Switch
    {
        public Field field; public string itemKey;
        public Transform root, bar, namePlate, valuePlate; public Renderer lamp;
        public bool dial;
    }

    public class Token
    {
        public Item item; public string key;
        public Transform root, namePlate, detailPlate, unpinPlate; public Transform slab;
        public readonly List<Switch> switches = new List<Switch>();
        public Vector3 slot, homeLoose; public Quaternion slotRot;
        public bool loose, pinned, selected, pulsing, console, sign, dragging;
        public int order;
        public float w, h;
    }

    Building world; Room room; RoomState st; Action changed;
    Transform rig, roomRoot; Camera cam;
    readonly List<Token> tokens = new List<Token>();
    readonly List<Switch> once = new List<Switch>();
    Token signPad, selected, held;
    Transform consoleRoot;
    Transform detail;
    Bounds bounds;
    float zFace, gridLeft, gridRight, gridTop, gridBottom, cx, scale = 1f, cellW, cellH, gap, floorY;
    int cols; bool rack;
    Vector2 downAt; float downTime; bool pointerDown;
    public bool Interactive = true;
    public event Action<Token> Selected; // a token brought close (null: back to the room)
    public Bounds Bounds => bounds;
    public float? ConsoleX { get; private set; } // where the console or the pad stands, in the room's space; null when there is none
    public float GridTop => gridTop;
    public float GridLeft => gridLeft;
    public float GridRight => gridRight;
    public bool ConsoleAbove { get; private set; }
    public float ConsoleTop { get; private set; }     // the console's (or the pad's) top edge, in the room's space
    public Rect? ConsoleRect { get; private set; }    // the console, its plates and the pad, in the room's x and y
    public Rect? ConsoleView { get; private set; }    // the same, as the room camera sees it (degrees off its axis)
    public Rect GridView { get; private set; }        // the grid of tokens, as the room camera sees it
    public readonly List<Rect> Occluders = new List<Rect>(); // the props between the camera and the rig, as the camera sees them
    public readonly List<Rect> WallOccluders = new List<Rect>(); // the same, for something on the back wall (clipped at the wall)
    public IReadOnlyList<Token> Tokens => tokens;
    static readonly Color Slate = new Color(0.16f, 0.2f, 0.32f), Paper = new Color(0.93f, 0.92f, 0.86f), Dim = new Color(0.62f, 0.66f, 0.72f);
    const float Gap = 0.06f, RackGap = 0.02f;

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
        maxValue = 0.5f;
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
        var items = room.items.Where(i => !i.hidden).ToList();
        var order = st.order.Where(k => room.items.Any(i => i.key == k)).ToList();
        foreach (var a in st.added) if (!order.Contains(a)) order.Add(a);
        int n = order.Count;
        float w = rack ? 1.16f : Mathf.Max(0.34f, 0.1f * perRow.Count + 0.1f), h = rack ? 0.15f : 0.22f;
        cellW = w * scale; cellH = h * scale; gap = (rack ? RackGap : Gap) * scale;
        cols = rack ? 1 : Mathf.Clamp(Mathf.FloorToInt((1.62f * scale + gap) / (cellW + gap)), 1, 3);
        if (!rack && n <= 4 && cols > 2) cols = 2;
        // A console or a signature pad beside the grid needs the room: two columns at most.
        if (!rack && cols > 2 && (room.fields.Any(f => f.once) || room.signable)) cols = 2;
        int rows = Mathf.CeilToInt(n / (float)cols);
        float gridW = cols * cellW + (cols - 1) * gap, gridH = rows * cellH + (rows - 1) * gap;
        float yc = Mathf.Clamp((hy0 + hy1) / 2f + (rack ? 0.1f : 0.05f) * scale, 0.35f + gridH / 2f, 2.0f - gridH / 2f);
        gridTop = yc + gridH / 2f; gridBottom = yc - gridH / 2f;
        gridLeft = cx - gridW / 2f; gridRight = cx + gridW / 2f;
        // The grid's top row inside the feed: slide down while the floor allows.
        for (int guard = 0; guard < 12 && !InFrame(new Vector3(cx, gridTop + 0.04f, zFace), 3f) && gridBottom - 0.05f > 0.15f; guard++)
        { yc -= 0.05f; gridTop = yc + gridH / 2f; gridBottom = yc - gridH / 2f; }
        for (int k = 0; k < n; k++)
        {
            var item = room.items.First(i => i.key == order[k]);
            var t = MakeToken(item, w, h, perRow);
            t.order = k;
            t.pinned = item.hidden;
            t.root.localScale = Vector3.one * scale;
            t.root.localPosition = Slot(k); t.root.localRotation = Quaternion.Euler(0f, 180f, 0f);
            t.slot = t.root.localPosition; t.slotRot = t.root.localRotation;
            tokens.Add(t);
        }
        if (rack && tokens.Count > 0)
        {
            // One text size for every module (the smallest any name needed), and a dark panel
            // behind them: the modules are mounted in a rack, not floating.
            float font = tokens.Where(x => !x.loose && !x.sign).Min(x => x.namePlate.GetComponentInChildren<TextMeshPro>().fontSize);
            foreach (var x in tokens.Where(x => !x.loose && !x.sign))
            {
                var tm = x.namePlate.GetComponentInChildren<TextMeshPro>(); tm.fontSize = font; Building.FitPlate(x.namePlate);
                x.namePlate.localPosition = new Vector3(-x.w / 2f + 0.09f + x.namePlate.Find("Slab").localScale.x / 2f, 0f, -0.024f);
            }
            // Behind the modules (the camera looks toward -z): the panel at -z, the rails flush.
            var backing = Building.Box(rig, "Rack panel", new Vector3(gridRight - gridLeft + 0.08f * scale, gridTop - gridBottom + 0.08f * scale, 0.03f), new Vector3(cx, (gridTop + gridBottom) / 2f, zFace - 0.03f), Building.Trim, new Color(0.08f, 0.09f, 0.12f));
            foreach (float side in new[] { -1f, 1f })
                Building.Box(rig, "Rail", new Vector3(0.03f * scale, gridTop - gridBottom + 0.1f * scale, 0.04f), new Vector3(cx + side * (gridRight - gridLeft + 0.08f * scale) / 2f, (gridTop + gridBottom) / 2f, zFace - 0.015f), Building.Metal, new Color(0.35f, 0.38f, 0.44f));
        }
        // The room's one-off decisions on a console beside the grid, the signature pad under it.
        // Where: the first spot, in this order, whose four corners are in the feed, inside the
        // room's walls and not behind a prop nearer the camera (the bin, the desk, the rack):
        // beside the grid on the screen's right, then its left, each at rising heights; under the
        // grid; beside it again with the values shortened; over the grid (in the lamp's pool) last.
        var onceFields = room.fields.Where(f => f.once).ToList();
        float consoleX = gridLeft - 0.3f * scale, consoleH = (onceFields.Count * 0.17f + 0.06f) * scale;
        float cy = gridTop - 0.08f * scale;
        bool under = false, above = false;
        float cs = scale; // the console's own scale: it shrinks when nothing fits at full size
        if (onceFields.Count > 0 || room.signable)
        {
            bool padOnly = onceFields.Count == 0;
            bool found = false; float topY = gridTop;
            foreach (float k in new[] { 1f, 0.8f, 0.65f })
            {
                cs = scale * k;
                consoleH = (onceFields.Count * 0.17f + 0.06f) * cs;
                // A console's name plate runs to the screen's left of its switches, its value to the
                // right (the room's larger x is the screen's left).
                float nameSpan = (padOnly ? 0.2f : 0.4f) * cs, valueSpan = (padOnly ? 0.24f : 0.62f) * cs, gapX = 0.05f * scale;
                float bodyH = padOnly ? 0f : consoleH;
                float totalH = bodyH + (room.signable ? (padOnly ? 0.12f : 0.24f) * cs : 0f); // the console, the gap, the pad
                bool FitsAt(float x, float top)
                {
                    float bottom = top - totalH;
                    if (bottom < floorY + 0.05f || top > 2.35f) return false;
                    if (x - valueSpan < -1.75f || x + nameSpan > 1.75f) return false;
                    foreach (var (px, py) in new[] { (x - valueSpan, top), (x + nameSpan, top), (x - valueSpan, bottom), (x + nameSpan, bottom) })
                        if (!InFrame(new Vector3(px, py, zFace), 3f)) return false;
                    return !Hidden(ViewRect(x - valueSpan, bottom, x + nameSpan, top, zFace));
                }
                float Right() => gridLeft - nameSpan - gapX;
                float Left() => gridRight + valueSpan + gapX;
                var lifts = new[] { 0f, 0.15f, 0.3f, 0.45f, 0.6f, -0.15f };
                void Try(float x, float top, bool u = false, bool a = false)
                {
                    if (found || !FitsAt(x, top)) return;
                    found = true; consoleX = x; topY = top; under = u; above = a;
                }
                foreach (var l in lifts) Try(Right(), gridTop + l * scale);
                foreach (var l in lifts) Try(Left(), gridTop + l * scale);
                Try(cx + (valueSpan - nameSpan) / 2f, gridBottom - 0.1f * scale, u: true);
                if (!found)
                {
                    float fullValue = valueSpan;
                    valueSpan = fullValue * 0.5f;
                    foreach (var l in lifts) Try(Right(), gridTop + l * scale);
                    foreach (var l in lifts) Try(Left(), gridTop + l * scale);
                    if (found) maxValue = 0.26f;
                    valueSpan = fullValue;
                }
                Try(cx + (valueSpan - nameSpan) / 2f, gridTop + 0.1f * scale + totalH, u: true, a: true);
                if (found) break;
            }
            if (!found) { cs = scale; consoleH = (onceFields.Count * 0.17f + 0.06f) * cs; consoleX = gridLeft - (padOnly ? 0.2f : 0.4f) * cs - 0.05f * scale; topY = gridTop; }
            // The console's centre sits under its top; a lone pad's centre likewise.
            cy = padOnly ? topY : topY - 0.115f * cs;
            ConsoleX = under ? (float?)null : consoleX;
            ConsoleAbove = above;
            ConsoleTop = topY;
            if (DemoDriver.Wanted) Debug.Log($"[Console] {room.id}: {(found ? "placed" : "fallback")} at {cs / scale:P0} x {consoleX:0.00} top {topY:0.00} under {under} above {above}");
        }
        if (onceFields.Count > 0)
        {
            var console = new GameObject("Console").transform;
            consoleRoot = console;
            console.SetParent(rig, false);
            console.localPosition = new Vector3(consoleX, cy - (onceFields.Count - 1) * 0.085f * cs, zFace);
            console.localRotation = Quaternion.Euler(0f, 180f, 0f);
            console.localScale = Vector3.one * cs;
            Building.Box(console, "Panel", new Vector3(0.42f, onceFields.Count * 0.17f + 0.06f, 0.025f), Vector3.zero, Building.Metal, Slate);
            for (int i = 0; i < onceFields.Count; i++)
            {
                var sw = MakeSwitch(console, onceFields[i], "room", new Vector3(0.1f, (onceFields.Count - 1) * 0.085f - i * 0.17f, -0.02f), true);
                sw.namePlate.gameObject.SetActive(true);
                var fit = sw.namePlate.GetComponent<Building.PlateFit>(); fit.minW = 0.1f; Building.FitPlate(sw.namePlate);
                sw.namePlate.localPosition = new Vector3(-0.06f - sw.namePlate.Find("Slab").localScale.x / 2f, 0f, -0.03f);
                once.Add(sw);
            }
            cy = console.localPosition.y - consoleH / 2f - 0.12f * cs;
        }

        if (room.signable)
        {
            signPad = new Token { key = "sign", sign = true, w = 0.34f, h = 0.12f };
            signPad.root = new GameObject("Sign pad").transform;
            signPad.root.SetParent(rig, false);
            signPad.root.localPosition = new Vector3(consoleX, Mathf.Max(floorY + 0.06f * cs + 0.02f, cy - 0.06f * cs), zFace);
            signPad.root.localRotation = Quaternion.Euler(0f, 180f, 0f);
            signPad.root.localScale = Vector3.one * cs;
            signPad.slab = Building.Box(signPad.root, "Pad", new Vector3(0.34f, 0.12f, 0.02f), Vector3.zero, Building.Metal, Slate).transform;
            signPad.namePlate = Building.DarkPlate(signPad.root, "Sign", 0.5f, out _, 0.2f);
            signPad.namePlate.localPosition = new Vector3(0.03f, 0f, -0.025f);
            var sw = new Switch { itemKey = "sign" };
            sw.root = new GameObject("Lamp").transform; sw.root.SetParent(signPad.root, false); sw.root.localPosition = new Vector3(-0.12f, 0f, -0.015f);
            sw.lamp = Building.Lamp(sw.root, Vector3.zero, 0.03f, Theme.Amber);
            signPad.switches.Add(sw);
            signPad.slot = signPad.root.localPosition; signPad.slotRot = signPad.root.localRotation;
            tokens.Add(signPad);
        }
        if (onceFields.Count > 0 || room.signable)
        {
            float top = consoleRoot != null ? consoleRoot.localPosition.y + consoleH / 2f : signPad.root.localPosition.y + 0.06f * cs;
            float bottom = signPad != null ? signPad.root.localPosition.y - 0.06f * cs : consoleRoot.localPosition.y - consoleH / 2f;
            float vs = (consoleRoot == null ? 0.24f : 0.62f) * cs, ns = (consoleRoot == null ? 0.2f : 0.4f) * cs;
            ConsoleRect = Rect.MinMaxRect(consoleX - vs, bottom, consoleX + ns, top);
            ConsoleView = ViewRect(consoleX - vs, bottom, consoleX + ns, top, zFace);
            ConsoleTop = top;
        }
        GridView = ViewRect(gridLeft - 0.05f, gridBottom - 0.05f, gridRight + 0.05f, gridTop + 0.12f * scale, zFace);
        // What is not on the list yet, about the room: a folder on a crate, a printer by the door, a sheet on the wall.
        int loose = 0;
        foreach (var hidden in room.items.Where(i => i.hidden && !st.added.Contains(i.key)))
        {
            var t = MakeLoose(hidden, loose++, hx0, hx1, hz1);
            tokens.Add(t);
        }
        // What the close camera frames: the grid, the console and the pad.
        var corners = new List<Vector3> { rig.TransformPoint(new Vector3(gridLeft, gridBottom, zFace)), rig.TransformPoint(new Vector3(gridRight, gridTop, zFace)) };
        if (onceFields.Count > 0 || room.signable) corners.Add(rig.TransformPoint(new Vector3(consoleX + 0.25f * scale, Mathf.Max(0.3f, cy - 0.1f * scale), zFace)));
        if (onceFields.Count > 0 || room.signable) corners.Add(rig.TransformPoint(new Vector3(consoleX - 0.25f * scale, gridTop, zFace)));
        bounds = new Bounds(corners[0], Vector3.zero);
        foreach (var c in corners) bounds.Encapsulate(c);
        bounds.Expand(new Vector3(0.3f, 0.3f, 0.2f));
        Refresh();
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

    // The horizontal angle, in degrees, of a local point off the room camera's axis.
    float Angle(Vector3 local)
    {
        if (!world.RoomCamera(Facility.RoomIndex(room.room), out var eye, out var look, out _)) return 0f;
        var fwd = (look - eye).normalized; fwd.y = 0f; fwd.Normalize();
        var d = rig.TransformPoint(local) - eye; d.y = 0f;
        return Vector3.SignedAngle(fwd, d, Vector3.up);
    }

    // Whether the bin by the camera (0.75 m tall, at z -1.2) stands in the line of sight to a local point.
    bool Occluded(Vector3 local, float binX)
    {
        if (!world.RoomCamera(Facility.RoomIndex(room.room), out var eye, out _, out _)) return false;
        var e = rig.InverseTransformPoint(eye);
        var p = local;
        float dz = p.z - e.z; if (Mathf.Abs(dz) < 0.01f) return false;
        float k = (-1.2f - e.z) / dz; if (k < 0f || k > 1f) return false;
        var at = Vector3.Lerp(e, p, k);
        return Mathf.Abs(at.x - binX) < 0.42f && at.y < 0.95f;
    }

    Vector3 Slot(int k)
    {
        // The camera inside looks down -Z, so the screen's left is the larger local x.
        int col = k % cols, row = k / cols;
        return new Vector3(gridRight - cellW / 2f - col * (cellW + gap), gridTop - cellH / 2f - row * (cellH + gap), zFace);
    }

    Token MakeToken(Item item, float w, float h, List<Field> perRow)
    {
        var t = new Token { item = item, key = item.key, w = w, h = h };
        t.root = new GameObject("Token " + item.key).transform;
        t.root.SetParent(rig, false);
        bool paper = room.number == 3 || room.number == 5; // envelopes and report forms
        t.slab = Building.Box(t.root, "Slab", new Vector3(w, h, 0.02f), Vector3.zero, paper ? Building.Enamel : Building.Metal, paper ? Paper : Slate).transform;
        if (rack)
        {
            // A module in the rack: a grip at the left end, the name along it, the switches at the right end.
            for (int g = 0; g < 2; g++) Building.Box(t.root, "Grip", new Vector3(0.012f, h * 0.7f, 0.01f), new Vector3(-w / 2f + 0.03f + g * 0.02f, 0f, -0.015f), Building.Trim, null, false);
            t.namePlate = Building.DarkPlate(t.root, Script.ItemName(room, item), 0.46f, out var nameText, 0.3f);
            float room_ = w - 0.14f - perRow.Count * 0.13f;
            for (int guard = 0; guard < 6 && t.namePlate.Find("Slab").localScale.x > room_; guard++) { nameText.fontSize *= 0.9f; Building.FitPlate(t.namePlate); }
            t.namePlate.localPosition = new Vector3(-w / 2f + 0.09f + t.namePlate.Find("Slab").localScale.x / 2f, 0f, -0.024f);
            float sx = w / 2f - 0.07f;
            for (int k = perRow.Count - 1; k >= 0; k--) { t.switches.Add(MakeSwitch(t.root, perRow[k], item.key, new Vector3(sx, 0f, -0.02f), false, k % 2 == 1)); sx -= 0.13f; }
            t.switches.Reverse();
        }
        else
        {
            t.namePlate = Building.DarkPlate(t.root, Script.ItemName(room, item), 0.44f, out _, 0.2f);
            t.namePlate.localPosition = new Vector3(0f, h / 2f - 0.05f, -0.024f);
            float span = (perRow.Count - 1) * 0.1f;
            for (int k = 0; k < perRow.Count; k++) t.switches.Add(MakeSwitch(t.root, perRow[k], item.key, new Vector3(-span / 2f + k * 0.1f, -h / 2f + 0.07f, -0.02f), false, k % 2 == 1));
            var group = Script.Group(room, item);
            if (group != null) { var g = Building.DarkPlate(t.root, group, 0.26f, out var gt, 0.1f); gt.color = Dim; g.localPosition = new Vector3(0f, h / 2f + 0.045f, -0.02f); }
        }
        var paperText = paper ? Theme.Ink : Color.white;
        if (paper) foreach (var tm in t.namePlate.GetComponentsInChildren<TextMeshPro>()) tm.color = Color.white;
        return t;
    }

    // A switch: a housing with a lamp, and a lever for two choices or a dial for more; its name
    // over it and its value under it, shown while the card is close (always, for a console).
    Switch MakeSwitch(Transform parent, Field f, string itemKey, Vector3 at, bool console, bool stagger = false)
    {
        var sw = new Switch { field = f, itemKey = itemKey };
        sw.root = new GameObject("Switch " + f.key).transform;
        sw.root.SetParent(parent, false);
        sw.root.localPosition = at;
        int real = f.options.Count(o => o != "?" && o != "n/a");
        sw.dial = real > 2;
        Building.Box(sw.root, "Housing", new Vector3(0.08f, 0.08f, 0.012f), Vector3.zero, Building.Trim, null, false);
        if (sw.dial)
        {
            var disc = new GameObject("Dial");
            disc.transform.SetParent(sw.root, false);
            disc.transform.localPosition = new Vector3(0f, -0.004f, -0.012f);
            disc.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            disc.transform.localScale = new Vector3(0.058f, 0.004f, 0.058f);
            disc.AddComponent<MeshFilter>().sharedMesh = Resources.GetBuiltinResource<Mesh>("Cylinder.fbx");
            var dr = disc.AddComponent<MeshRenderer>(); dr.sharedMaterial = Building.Metal; dr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            Building.Tint(dr, new Color(0.22f, 0.25f, 0.32f));
            Building.Matte(dr);
            sw.bar = Building.Box(sw.root, "Pointer", new Vector3(0.008f, 0.03f, 0.006f), Vector3.zero, Building.Lens, null, false).transform;
            var pivot = new GameObject("Pivot").transform; pivot.SetParent(sw.root, false); pivot.localPosition = new Vector3(0f, -0.004f, -0.02f);
            sw.bar.SetParent(pivot, false); sw.bar.localPosition = new Vector3(0f, 0.013f, 0f);
            sw.bar = pivot;
            Building.Glow(pivot.GetComponentInChildren<Renderer>(), Color.white, 0.8f);
        }
        else
        {
            var pivot = new GameObject("Lever").transform; pivot.SetParent(sw.root, false); pivot.localPosition = new Vector3(0f, -0.006f, -0.02f);
            Building.Box(pivot, "Bar", new Vector3(0.014f, 0.056f, 0.012f), new Vector3(0f, 0.018f, 0f), Building.Metal, new Color(0.8f, 0.83f, 0.88f), false);
            var knob = Building.Box(pivot, "Knob", new Vector3(0.022f, 0.022f, 0.022f), new Vector3(0f, 0.046f, 0f), Building.Lens, null, false);
            Building.Glow(knob.GetComponent<Renderer>(), new Color(0.85f, 0.88f, 0.92f), 0.5f);
            sw.bar = pivot;
        }
        sw.lamp = Building.Lamp(sw.root, new Vector3(0.028f, 0.028f, -0.008f), 0.012f, Theme.Amber);
        sw.namePlate = Building.DarkPlate(sw.root, Script.FieldName(f), console || rack ? 0.3f : 0.24f, out var nt, 0.08f);
        nt.color = Dim;
        sw.namePlate.localPosition = new Vector3(0f, stagger ? 0.11f : 0.065f, -0.022f);
        sw.namePlate.gameObject.SetActive(false);
        sw.valuePlate = Building.DarkPlate(sw.root, "?", console || rack ? 0.34f : 0.27f, out _, 0.1f);
        sw.valuePlate.localPosition = new Vector3(0f, stagger ? -0.115f : -0.07f, -0.024f);
        sw.valuePlate.gameObject.SetActive(console);
        if (console) PlaceValue(sw);
        return sw;
    }

    // A console's value plate hangs to the right of its switch, however long it reads.
    float maxValue = 0.5f; // how wide a console's value plate may run, from where the console stands
    void PlaceValue(Switch sw)
    {
        var tmp = sw.valuePlate.GetComponentInChildren<TextMeshPro>();
        tmp.fontSize = 0.34f; Building.FitPlate(sw.valuePlate);
        for (int guard = 0; guard < 8 && sw.valuePlate.Find("Slab").localScale.x > maxValue; guard++) { tmp.fontSize *= 0.88f; Building.FitPlate(sw.valuePlate); }
        sw.valuePlate.localPosition = new Vector3(0.06f + sw.valuePlate.Find("Slab").localScale.x / 2f, 0f, -0.024f);
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
                // A folder of papers on a crate at the board's foot, stamped.
                t.root.localPosition = new Vector3(Mathf.Max(-1.45f, hx0 - 0.2f), 0f, hz1 + 0.45f);
                t.root.localRotation = Quaternion.Euler(0f, 180f, 0f);
                Building.Box(t.root, "Crate", new Vector3(0.44f, 0.4f, 0.4f), new Vector3(0f, 0.2f, 0f), Building.Metal, new Color(0.55f, 0.5f, 0.42f));
                Glow(t.root, new Vector3(0f, 0.9f, -0.3f));
                t.slab = Building.Box(t.root, "Folder", new Vector3(0.3f, 0.025f, 0.22f), new Vector3(0f, 0.415f, 0f), Building.Lens, null).transform;
                Building.Glow(t.slab.GetComponent<Renderer>(), Paper, 0.45f);
                var stamp = Building.DarkPlate(t.root, "SHARED DRIVE", 0.24f, out var stt, 0.1f);
                stt.color = Look.Breached;
                stamp.localPosition = new Vector3(0.02f, 0.432f, 0.02f);
                stamp.localRotation = Quaternion.Euler(-90f, 0f, 0f);
                t.namePlate = stamp;
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
        t.root.localScale = Vector3.one * Mathf.Max(scale, 0.75f);
        t.homeLoose = t.root.localPosition; t.slot = t.homeLoose; t.slotRot = t.root.localRotation;
        return t;
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
        foreach (var sw in once) Show(sw);
    }

    void Show(Switch sw)
    {
        var f = sw.field;
        int idx = st.Pick(sw.itemKey, f.key);
        bool unset = idx < 0 || idx >= f.options.Length || f.options[idx] == "?";
        bool na = !unset && f.options[idx] == "n/a";
        var colour = unset ? Theme.Amber : na ? Dim : Script.Danger(f, idx) ? Look.Breached : Theme.Teal;
        Building.Glow(sw.lamp, colour, unset ? 1.4f : 1.8f);
        int first = f.options[0] == "?" || f.options[0] == "n/a" ? 1 : 0;
        int real = f.options.Length - first;
        if (sw.dial)
        {
            float a = unset || na ? 0f : Mathf.Lerp(-130f, 130f, real > 1 ? (idx - first) / (float)(real - 1) : 0.5f);
            sw.bar.localRotation = Quaternion.Euler(0f, 0f, -a);
        }
        else
        {
            float a = unset || na ? 0f : (idx - first) == 0 ? 32f : -32f;
            sw.bar.localRotation = Quaternion.Euler(0f, 0f, a);
        }
        string value = unset ? "?" : Script.Option(room, sw.itemKey, f, idx);
        Building.SetPlate(sw.valuePlate, value, unset ? Theme.Amber : na ? Dim : Color.white);
        if (sw.itemKey == "room") PlaceValue(sw);
    }

    // A tap on a switch turns it to its next choice (skipping the question mark).
    void Turn(Switch sw)
    {
        var f = sw.field;
        int next = (st.Pick(sw.itemKey, f.key) + 1) % f.options.Length;
        if (f.options[next] == "?" && f.options.Length > 1) next = (next + 1) % f.options.Length;
        st.Set(sw.itemKey, f.key, next);
        Sfx.Tick();
        Show(sw);
        StartCoroutine(Pop(sw.root, 1.25f));
        changed?.Invoke();
    }

    public void Select(Token t)
    {
        if (selected == t) return;
        if (selected != null) Close(selected);
        selected = t;
        if (t == null) { Selected?.Invoke(null); return; }
        if (t.sign || t.loose) return;
        t.selected = true;
        foreach (var sw in t.switches) { sw.namePlate.gameObject.SetActive(true); sw.valuePlate.gameObject.SetActive(true); }
        if (!rack) t.namePlate.gameObject.SetActive(false);
        Building.Halo(t.namePlate, true);
        // The card's full name and its line, on a plate below the grid.
        if (detail != null) Destroy(detail.gameObject);
        string text = t.item.name + (string.IsNullOrEmpty(t.item.detail) ? "" : "\n<size=80%><color=#9EAEBB>" + t.item.detail + "</color></size>");
        detail = Building.DarkPlate(rig, text, 0.36f, out var dt, 0.5f, rack ? 1.4f : 0.95f);
        dt.alignment = TextAlignmentOptions.Center;
        detail.localRotation = Quaternion.Euler(0f, 180f, 0f);
        detail.localScale = Vector3.one * scale;
        // Under the card it describes, in front of the others while the card is close.
        float dh = detail.Find("Slab").localScale.y * scale;
        float grown = rack ? 1f : 1.45f;
        float dy = t.slot.y - cellH * grown / 2f - 0.14f * scale - dh / 2f;
        // A rack's rule reads above the rack, so it covers no other module.
        if (rack) dy = gridTop + 0.06f * scale + dh / 2f;
        detail.localPosition = new Vector3(t.slot.x, rack ? dy : Mathf.Max(floorY + 0.06f + dh / 2f, dy), zFace + 0.16f * scale);
        if (t.pinned && t.item.hidden)
        {
            t.unpinPlate = Building.DarkPlate(t.root, "Unpin ×", 0.3f, out var ut, 0.1f);
            ut.color = Look.Breached;
            t.unpinPlate.localPosition = new Vector3(t.w / 2f - 0.1f, t.h / 2f + 0.06f, -0.024f);
        }
        Sfx.Whoosh();
        Selected?.Invoke(t);
    }

    // What the camera frames when a token is close: the card grown, its plates, its line under it.
    public Bounds FocusBounds(Token t)
    {
        float grown = rack ? 1f : 1.45f;
        float dh = detail != null ? detail.Find("Slab").localScale.y * scale : 0.15f;
        var top = Mathf.Max(t.slot.y + cellH * grown / 2f + 0.15f * scale, detail != null ? detail.localPosition.y + dh / 2f + 0.04f : 0f);
        var bottom = Mathf.Min(t.slot.y - cellH * grown / 2f - 0.06f * scale, detail != null ? detail.localPosition.y - dh / 2f - 0.04f : 99f);
        float wdt = Mathf.Max(cellW * grown + 0.2f * scale, detail != null ? detail.Find("Slab").localScale.x * scale + 0.1f : 0f);
        var c = rig.TransformPoint(new Vector3(t.slot.x, (top + bottom) / 2f, zFace + 0.12f * scale));
        return new Bounds(c, new Vector3(wdt, top - bottom, 0.2f));
    }

    void Close(Token t)
    {
        t.selected = false;
        if (t.namePlate != null) t.namePlate.gameObject.SetActive(true);
        foreach (var sw in t.switches) { if (sw.namePlate != null) sw.namePlate.gameObject.SetActive(false); if (sw.valuePlate != null) sw.valuePlate.gameObject.SetActive(false); }
        if (!t.pulsing) Building.Halo(t.namePlate, false);
        if (detail != null) { Destroy(detail.gameObject); detail = null; }
        if (t.unpinPlate != null) { Destroy(t.unpinPlate.gameObject); t.unpinPlate = null; }
    }

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

    // Pinning: what was found in the room goes onto the board, as a card with its switches;
    // unpinning sends it back where it lay.
    void Pin(Token t)
    {
        st.added.Add(t.key); st.order.Add(t.key);
        Sfx.Chime();
        changed?.Invoke();
        Rebuild();
        var card = tokens.FirstOrDefault(x => x.key == t.key);
        if (card != null) { card.root.localPosition = t.homeLoose; card.root.localRotation = t.slotRot; }
    }

    void Unpin(Token t)
    {
        st.added.Remove(t.key); st.order.Remove(t.key);
        foreach (var f in room.fields) st.picks.Remove(t.key + "/" + f.key);
        Sfx.Ruin();
        changed?.Invoke();
        Rebuild();
    }

    void Rebuild()
    {
        Select(null);
        var pulsing = tokens.Where(x => x.pulsing).Select(x => x.key).ToList();
        foreach (var t in tokens) Destroy(t.root.gameObject);
        tokens.Clear(); once.Clear(); signPad = null; consoleRoot = null;
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

    // ---- Picking: a tap on a switch, a tap on a token, a drag up or down the rack ----

    void Update()
    {
        if (!Interactive || cam == null) return;
        foreach (var t in tokens)
        {
            if (t.dragging) continue;
            var want = t.slot + (t.selected ? new Vector3(0f, 0f, 0.12f * scale) : Vector3.zero); // toward the camera (+z), in front of its neighbours
            t.root.localPosition = Vector3.Lerp(t.root.localPosition, want, 1f - Mathf.Exp(-10f * Time.unscaledDeltaTime));
            if (t.pulsing && t.namePlate != null) { var halo = t.namePlate.Find("Halo"); if (halo != null) Building.Glow(halo.GetComponent<Renderer>(), Theme.Teal, 0.8f + 0.8f * Mathf.Sin(Time.unscaledTime * 5f)); }
            // Something not on the list yet glints now and then: a thing in the room to pick up.
            if (t.loose && t.slab != null && !t.pulsing)
            {
                float k = Mathf.Max(0f, Mathf.Sin(Time.unscaledTime * 2.2f + t.homeLoose.x * 3f));
                Building.Glow(t.slab.GetComponent<Renderer>(), t.key == "printer" ? new Color(0.78f, 0.8f, 0.84f) : Paper, 0.5f + 0.9f * k);
                var lamp = t.root.Find("Glow"); if (lamp != null) lamp.GetComponent<Light>().intensity = 0.6f + 1.6f * k;
            }
            // The open card grows a little, so its switches' names and values have room.
            if (!t.loose && !t.sign && !rack)
            {
                float want2 = scale * (t.selected ? 1.45f : 1f);
                t.root.localScale = Vector3.one * Mathf.Lerp(t.root.localScale.x, want2, 1f - Mathf.Exp(-10f * Time.unscaledDeltaTime));
            }
        }
        if (GameMenu.Paused || FileSheet.IsOpen) return;
        var pos = (Vector2)Input.mousePosition;
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
            pointerDown = true; downAt = pos; downTime = Time.unscaledTime;
            if (rack) held = PickToken(pos, 90f, true);
        }
        if (pointerDown && Input.GetMouseButton(0) && held != null && !held.dragging && (pos - downAt).magnitude > 10f) { held.dragging = true; Select(null); }
        if (held != null && held.dragging) Drag(pos);
        if (Input.GetMouseButtonUp(0) && pointerDown)
        {
            pointerDown = false;
            if (held != null && held.dragging) { held.dragging = false; held = null; Settle(); return; }
            held = null;
            if ((pos - downAt).magnitude > 14f) return;
            var sw = PickSwitch(pos, 36f);
            if (sw != null) { if (sw.itemKey == "sign") { st.signed = true; st.signedBy = "You, as the reviewer"; st.signedOn = DateTime.Now.ToString("d MMM yyyy"); Sfx.Buy(); Refresh(); changed?.Invoke(); } else Turn(sw); return; }
            var t = PickToken(pos, 95f, false);
            if (t != null && t.unpinPlate != null && t.selected)
            {
                var up = cam.WorldToScreenPoint(t.unpinPlate.position);
                if (up.z > 0 && Vector2.Distance(new Vector2(up.x, up.y), pos) < 40f) { Unpin(t); return; }
            }
            if (t == null) { Select(null); return; }
            if (t.loose) { Pin(t); return; }
            if (t.sign) { st.signed = true; st.signedBy = "You, as the reviewer"; st.signedOn = DateTime.Now.ToString("d MMM yyyy"); Sfx.Buy(); Refresh(); changed?.Invoke(); return; }
            Select(t.selected ? null : t);
        }
    }

    Switch PickSwitch(Vector2 screen, float reach)
    {
        Switch best = null; float bestD = reach;
        IEnumerable<Switch> all = once;
        if (selected != null) all = all.Concat(selected.switches);
        foreach (var sw in all)
        {
            var s = cam.WorldToScreenPoint(sw.root.position);
            if (s.z < 0) continue;
            float d = Vector2.Distance(new Vector2(s.x, s.y), screen);
            if (sw.valuePlate != null && sw.valuePlate.gameObject.activeSelf)
            {
                var v = cam.WorldToScreenPoint(sw.valuePlate.position);
                if (v.z > 0) d = Mathf.Min(d, Vector2.Distance(new Vector2(v.x, v.y), screen));
            }
            if (d < bestD) { bestD = d; best = sw; }
        }
        return best;
    }

    Token PickToken(Vector2 screen, float reach, bool gridOnly)
    {
        Token best = null; float bestD = reach;
        foreach (var t in tokens)
        {
            if (gridOnly && (t.loose || t.sign)) continue;
            var centre = t.slab != null ? t.slab.position : t.root.position;
            var s = cam.WorldToScreenPoint(centre);
            if (s.z < 0) continue;
            // A wide module counts along its whole length.
            float d = Vector2.Distance(new Vector2(s.x, s.y), screen);
            if (rack && !t.loose && !t.sign)
            {
                var left = cam.WorldToScreenPoint(t.root.TransformPoint(new Vector3(-t.w / 2f, 0f, 0f)));
                var right = cam.WorldToScreenPoint(t.root.TransformPoint(new Vector3(t.w / 2f, 0f, 0f))); // TransformPoint carries the root's scale
                var a = new Vector2(left.x, left.y); var b = new Vector2(right.x, right.y);
                var ab = b - a; float k = Mathf.Clamp01(Vector2.Dot(screen - a, ab) / Mathf.Max(1f, ab.sqrMagnitude));
                d = Vector2.Distance(a + ab * k, screen);
            }
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
        // Which slot is nearest to where it is now?
        var grid = tokens.Where(t => !t.loose && !t.sign).OrderBy(t => t.order).ToList();
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
        float d = 0.18f, e = 0f;
        var s0 = Vector3.one;
        while (e < d && t != null) { e += Time.unscaledDeltaTime; float k = e / d; t.localScale = s0 * (1f + (by - 1f) * Mathf.Sin(k * Mathf.PI)); yield return null; }
        if (t != null) t.localScale = s0;
    }

    // Whether every per-row decision of each listed item is made, in the rack's order (the board of lamps).
    public List<bool> Decided()
    {
        var perRow = room.fields.Where(f => !f.once).ToList();
        var list = new List<bool>();
        foreach (var t in tokens.Where(x => !x.loose && !x.sign).OrderBy(x => x.order))
            list.Add(perRow.All(f => { int p = st.Pick(t.key, f.key); return p >= 0 && p < f.options.Length && f.options[p] != "?"; }));
        return list;
    }
}
