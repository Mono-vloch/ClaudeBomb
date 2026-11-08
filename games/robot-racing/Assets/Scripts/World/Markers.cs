using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

// Every setting the student makes, as a thing in the world (the game-feel pass):
//
// - the rewards, placed on the track: the chequered flag on the finish, cones against the wall
//   (a crash), teal chevrons along the way (each step toward), a red one pointing back (each
//   step away), a clock post (each moment), an orange sign (the speed bonus). On the shelf a
//   reward is worth zero; dragged onto its place it takes its first value; a tap turns it;
// - the knobs on a small desk at the pit wall, tapped to turn: the tank (the attempts), new
//   moves (the chance of one), its fall and its floor, the rewrite, the seed, the measure. Each
//   has its name on a plate on the desk and its value on a plate on its face; the knob the level
//   is about is lit, the rest stand dim. A red cap on the tank (stop at the first finish) or on
//   new moves (never again after the first finish) comes off with a tap;
// - the parts on the car: five sensor lamps with their rays on the track and five move chevrons
//   on the rear deck, each with its name on a plate beside it, fitted or removed with a tap, and
//   the limiter at the tail (the top speed);
// - the track cards on the pit lane, with the circuit drawn on each: off, training, or the test.
//
// Every label is the same dark plate (Circuit.DarkPlate). Everything is made at run time from
// the kit's own pieces and materials. No physics: picking by projection to the screen, dragging
// along the pointer's ray on the ground. Race control's points (1.1, once the flag is on the
// finish) stand dim and cannot be touched.
public partial class Circuit
{
    public class Marker
    {
        public string kind;
        public float[] options;      // for a reward the first is zero: on the shelf
        public string[] labels;      // the plate's text per option; null: the number
        public int index;
        public Vector3 home, spot;   // shelf and track (rewards); the desk (knobs); the stand (cards)
        public float spotYaw;
        public Quaternion pose = Quaternion.identity; // a knob or a cap: how it stands on the console's panel
        public int column;           // a knob's place along its row of the console
        public Vector3 local;        // a part on the car: where it sits on it
        public Vector3 plateLocal;   // a part's plate, beside it, in the car's space
        public bool placeable, attached, card, knob, dim, lit;
        public Marker parent;        // a cap sits on its parent
        public Transform root, plateRoot, namePlate;
        public TextMeshPro plate;
        public Action<float> set;
        public float baseLift;       // the plate's height over the marker
        public bool quiet;           // a part whose name shows only for a moment after a tap
        public float namedUntil;
        public bool Placed => !placeable || index > 0;
        public float Value => options[index];
        public string Text => labels != null ? labels[Mathf.Clamp(index, 0, labels.Length - 1)] : Placed && options.Length > 1 ? Number(Value) : "";
    }

    readonly List<Marker> markers = new List<Marker>();
    Action markersChanged;
    Marker held;
    Vector3 heldOffset;
    Vector2 downAt;
    float downTime;
    bool pointerDown;
    GameObject shelfLamp, desk;
    public bool MarkersActive { get; private set; }
    public IReadOnlyList<Marker> Markers => markers;

    public static readonly string[] Rewards = { "finish", "crash", "toward", "away", "moment", "bonus" };
    public static readonly string[] Knobs = { "fuel", "die", "fall", "floor", "rewrite", "seed", "measure" };

    static readonly Dictionary<string, float[]> Options = new Dictionary<string, float[]>
    {
        ["finish"] = new[] { 0f, 100f, 200f, 50f },
        ["crash"] = new[] { 0f, -100f, -200f, -20f },
        ["toward"] = new[] { 0f, 2f, 5f, 1f },
        ["away"] = new[] { 0f, -2f, -5f, -1f },
        ["moment"] = new[] { 0f, -0.2f, -1f, -3f },
        ["bonus"] = new[] { 0f, 1f, 3f },
        ["fuel"] = new[] { 120f, 250f, 400f, 1000f, 50f },
        ["die"] = new[] { 0.9f, 0.1f, 0.5f },
        ["fall"] = new[] { 0.985f, 1f, 0.995f, 0.98f, 0.9f },
        ["floor"] = new[] { 0.05f, 0f, 0.5f },
        ["rewrite"] = new[] { 0.3f, 0.02f, 0.1f, 0.95f },
        ["seed"] = new[] { 0f, 1f },
        ["measure"] = new[] { 0f, 1f, 2f, 3f },
        ["limit"] = new[] { 2f, 1f },
        ["stopcap"] = new[] { 1f, 0f },
        ["nevercap"] = new[] { 1f, 0f },
    };
    static readonly Dictionary<string, string[]> Labels = new Dictionary<string, string[]>
    {
        ["fuel"] = new[] { "120", "250", "400", "1000", "50" },
        ["die"] = new[] { "90%", "10%", "50%" },
        ["fall"] = new[] { "falls", "never", "slowly", "fast", "at once" },
        ["floor"] = new[] { "5%", "0%", "50%" },
        ["rewrite"] = new[] { "0.3", "0.02", "0.1", "0.95" },
        ["measure"] = new[] { "Finishes", "Crashes", "Touches", "Lap time" },
        ["limit"] = new[] { "2", "1" },
    };
    // The knobs' names, on the console, and the caps'.
    static readonly Dictionary<string, string> KnobNames = new Dictionary<string, string>
    {
        ["fuel"] = "Tank", ["die"] = "New moves", ["fall"] = "Fall", ["floor"] = "Floor", ["rewrite"] = "Rewrite", ["seed"] = "Seed", ["measure"] = "Measure",
        ["stopcap"] = "Stop at first finish", ["nevercap"] = "Lock after first finish",
    };
    const float PanelTilt = 32f;   // the console's panel, tipped toward the pit lane's camera
    const float KnobStep = 0.26f;  // one knob to the next along the panel

    // kinds: which markers this level has; fixedKinds: race control's, dim and untouchable;
    // nameParts: the parts on the car carry their names (false on the capstones: a name shows
    // for two seconds after a tap); litKinds: the knobs the level is about (null: all of them).
    public void BuildMarkers(Settings s, IEnumerable<string> kindsIn, Action changed, IEnumerable<string> fixedKinds = null, bool nameParts = true, IEnumerable<string> litKinds = null)
    {
        ClearMarkers();
        if (!HasKit) return;
        markersChanged = changed;
        var kinds = kindsIn.ToList();
        var lit = litKinds?.ToList();
        var t = track;
        var along = D(t.startHeading);
        var outward = D(PitSide == 1 ? (t.startHeading + 3) % 4 : (t.startHeading + 1) % 4);
        int knobCount = kinds.Count(k => Knobs.Contains(k));
        bool parts = kinds.Any(k => k.StartsWith("sense") || k.StartsWith("move"));
        // The console at the pit wall behind the grid, close to the pit lane's camera and clear of
        // the gantry, long enough for its knobs: its panel tips toward the camera so the knobs'
        // faces and names read. The shelf for the rewards stands further up the pit lane.
        int perRow = knobCount > 4 ? Mathf.CeilToInt(knobCount / 2f) : knobCount, rows = knobCount > perRow ? 2 : 1;
        float deskLen = Mathf.Max(0.6f, 0.16f + KnobStep * perRow), deskDepth = rows == 2 ? 1.0f : 0.7f;
        var deskAt = Cell(t.start) + outward * (0.5f + 1.45f * M) - along * 0.9f;
        var consoleRot = Quaternion.LookRotation(-along, Vector3.up);
        var panelRot = consoleRot * Quaternion.Euler(PanelTilt, 0f, 0f);
        var panelAt = deskAt + Vector3.up * 0.31f + consoleRot * new Vector3(0f, 0f, 0.02f);
        var shelf0 = Cell(t.start) + outward * (0.5f + 1.7f * M) + along * 0.9f;
        if (kit != null)
            foreach (Transform child in kit)
            {
                // The kit's own cones and pit box give way to the shelf, the console and the markers; the
                // floodlight mast beside them would stand between the camera and the plates, and the
                // gantry's posts would stand in front of the parts' names.
                if (child.name.StartsWith("Cone") || child.name.StartsWith("PitBox")) child.gameObject.SetActive(false);
                if (child.name.StartsWith("Floodlight") && (child.position - shelf0).magnitude < 3f) child.gameObject.SetActive(false);
                if (parts && child.name.StartsWith("Gantry")) child.gameObject.SetActive(false);
            }
        int n = 0, cards = 0, knobs = 0;
        Marker fuel = null, die = null;
        foreach (var kind in kinds.Concat(fixedKinds ?? Enumerable.Empty<string>()))
        {
            bool fixedOne = !kinds.Contains(kind);
            var m = new Marker { kind = kind, dim = fixedOne };
            if (kind.StartsWith("track:"))
            {
                string id = kind.Substring(6);
                string shortName = ShortName(id);
                m.card = true; m.options = new[] { 0f, 1f, 2f }; m.labels = new[] { "", shortName, shortName };
                m.index = s.test == id ? 2 : s.training.Contains(id) ? 1 : 0;
                m.set = v => SetTrack(s, id, (int)v);
                m.home = m.spot = Cell(t.start) + outward * (0.5f + 1.1f * M + (cards / 5) * 0.95f) + along * (1.5f + (cards % 5) * 0.7f + (cards / 5) * 0.35f); // the back row staggered, seen between the front cards
                m.spotYaw = Yaw(-outward);
                cards++;
            }
            else if (kind.StartsWith("sense") || kind.StartsWith("move") || kind == "limit")
            {
                m.attached = true;
                int i = kind == "limit" ? 0 : kind[kind.Length - 1] - '0';
                if (kind.StartsWith("sense")) { m.options = new[] { 1f, 0f }; m.index = s.sensors[i] ? 0 : 1; m.set = v => { s.sensors[i] = v > 0; SetSensors(s.sensors); }; m.local = SensorLocal(i); m.plateLocal = SensorPlate(i); m.labels = new[] { Sensors.Names[i], Sensors.Names[i] }; }
                else if (kind.StartsWith("move")) { m.options = new[] { 1f, 0f }; m.index = s.moves[i] ? 0 : 1; m.set = v => { s.moves[i] = v > 0; if (s.MovesOn == 0) s.moves[i] = true; }; m.local = MoveLocal(i); m.plateLocal = MovePlate(i); m.labels = new[] { Moves.Names[i], Moves.Names[i] }; }
                else { m.options = Options["limit"]; m.labels = Labels["limit"]; m.index = s.topSpeed == 1 ? 1 : 0; m.set = v => { s.topSpeed = (int)v; s.constraintSet = true; }; m.local = new Vector3(0f, 0.3f, -0.4f); m.plateLocal = new Vector3(0f, 0.3f, -0.62f); }
                m.home = m.spot = carRoot != null ? carRoot.TransformPoint(m.plateLocal) : Cell(t.start);
                m.quiet = !nameParts && kind != "limit";
            }
            else if (kind == "stopcap" || kind == "nevercap")
            {
                m.parent = kind == "stopcap" ? fuel : die;
                if (m.parent == null) continue;
                m.options = Options[kind];
                m.index = kind == "stopcap" ? (s.stopAtFirstFinish ? 0 : 1) : (s.exploreOffAfterFinish ? 0 : 1);
                m.set = kind == "stopcap" ? (Action<float>)(v => s.stopAtFirstFinish = v > 0) : v => s.exploreOffAfterFinish = v > 0;
                m.lit = lit == null || lit.Contains(kind);
                m.pose = m.parent.pose;
                // Up the panel from its knob; beside it when the console has two rows.
                m.home = m.spot = m.parent.home + m.parent.pose * (rows == 1 ? new Vector3(0f, 0.012f, -0.15f) : new Vector3(0.15f, 0.012f, 0f));
            }
            else
            {
                if (!Options.ContainsKey(kind)) continue;
                m.options = Options[kind];
                Labels.TryGetValue(kind, out m.labels);
                m.placeable = Rewards.Contains(kind);
                m.knob = Knobs.Contains(kind);
                // Race control's points stand at the ready robot's values and never write the settings.
                var src = fixedOne ? Lesson.Fixed : s;
                float current = kind switch
                {
                    "finish" => src.finish, "crash" => src.crash, "toward" => src.toward, "away" => src.away, "moment" => src.moment, "bonus" => src.speedBonus,
                    "fuel" => src.attempts, "die" => src.explore, "fall" => src.exploreFall, "floor" => src.exploreMin, "rewrite" => src.rewrite, "measure" => src.measure, _ => 0f,
                };
                m.index = 0;
                for (int i = 0; i < m.options.Length; i++) if (Mathf.Abs(m.options[i] - current) < 0.001f) { m.index = i; break; }
                if (m.placeable && m.index == 0 && Mathf.Abs(current) > 0.001f) m.index = 1;
                m.set = kind switch
                {
                    "finish" => v => s.finish = v, "crash" => v => s.crash = v, "toward" => v => s.toward = v, "away" => v => s.away = v,
                    "moment" => v => s.moment = v, "bonus" => v => s.speedBonus = v, "fuel" => v => s.attempts = (int)v,
                    "die" => v => s.explore = v, "fall" => v => { s.exploreFall = v; }, "floor" => v => s.exploreMin = v,
                    "rewrite" => v => s.rewrite = v, "measure" => v => s.measure = (int)v, _ => v => { },
                };
                if (kind == "seed") { m.labels = new[] { s.seed.ToString(), s.seed.ToString() }; m.set = v => { if (v > 0) { s.seed = new System.Random().Next(1, 1000); m.labels = new[] { s.seed.ToString(), s.seed.ToString() }; } }; }
                if (fixedOne) m.set = v => { };
                if (m.knob)
                {
                    m.lit = lit == null || lit.Contains(kind);
                    m.pose = panelRot;
                    int row = knobs / perRow, inRow = row == 0 ? Mathf.Min(perRow, knobCount) : knobCount - perRow;
                    float z = rows == 1 ? 0f : row == 0 ? 0.14f : -0.3f; // the front row nearer the camera
                    m.column = knobs % perRow;
                    m.home = m.spot = panelAt + panelRot * new Vector3(((inRow - 1) / 2f - m.column) * KnobStep, 0.01f, z);
                    m.spotYaw = Yaw(along) + 90f;
                    if (kind == "fuel") fuel = m;
                    if (kind == "die") die = m;
                    knobs++;
                }
                else
                {
                    int row = n / 7;
                    m.home = shelf0 + along * (0.55f * (n % 7)) + outward * (row * 0.7f);
                    if (m.placeable) (m.spot, m.spotYaw) = Spot(kind); else { m.spot = m.home; m.spotYaw = Yaw(along) + 90f; }
                    n++;
                }
            }
            m.root = MakeMarker(m);
            MakePlate(m);
            markers.Add(m);
            Settle(m, true);
            if (m.dim) Dim(m.root);
            Debug.Log($"[Markers] {kind}: home {m.home} spot {m.spot} index {m.index}{(m.dim ? " dim" : "")}{(m.knob ? (m.lit ? " lit" : " unlit") : "")}");
        }
        MarkersActive = markers.Count > 0;
        if (n > 0)
        {
            var lamp = new GameObject("Shelf lamp").AddComponent<Light>();
            lamp.transform.SetParent(transform, false);
            lamp.transform.position = shelf0 + along * (0.55f * Mathf.Min(n - 1, 6) / 2f - 0.5f) + outward * 0.7f + Vector3.up * 1.4f;
            lamp.type = LightType.Point; lamp.color = new Color(1f, 0.93f, 0.8f); lamp.intensity = 3f; lamp.range = 3.6f; lamp.shadows = LightShadows.None;
            shelfLamp = lamp.gameObject;
        }
        if (knobCount > 0)
        {
            // The console: a dark base with a steel panel tipped toward the camera, lit from above.
            var console = new GameObject("Desk").transform;
            console.SetParent(transform, false);
            console.SetPositionAndRotation(deskAt, consoleRot);
            var baseBox = Box(console, new Vector3(deskLen, 0.18f, deskDepth * 0.5f), new Vector3(0f, 0.09f, -0.12f), KitMaterial("Barrier", "barrier"), new Color(0.1f, 0.12f, 0.2f)); // set back under the panel, never through it
            baseBox.name = "Desk base";
            var panel = new GameObject("Panel").transform;
            panel.SetParent(console, false);
            panel.localPosition = new Vector3(0f, 0.27f, 0.02f);
            panel.localRotation = Quaternion.Euler(PanelTilt, 0f, 0f);
            var top = Box(panel, new Vector3(deskLen + 0.04f, 0.02f, deskDepth), Vector3.zero, KitMaterial("Floodlight", "steel"), new Color(0.17f, 0.2f, 0.27f));
            top.name = "Desk top";
            var lamp = new GameObject("Desk lamp").AddComponent<Light>();
            lamp.transform.SetParent(console, false); lamp.transform.localPosition = new Vector3(0f, 1.2f, 0.5f);
            lamp.type = LightType.Point; lamp.color = new Color(1f, 0.93f, 0.8f); lamp.intensity = 3.2f; lamp.range = 3.4f; lamp.shadows = LightShadows.None;
            desk = console.gameObject;
        }
    }

    // A track's name as the card carries it: two words at most.
    static string ShortName(string id) => id switch
    {
        "bend" => "One bend", "twobends" => "Two bends", "bothways" => "Both ways", "chicane" => "Chicane", "plaza" => "Plaza",
        "hairpin" => "Hairpin", "grand" => "Grand circuit", "leftonly" => "Left only", "rightbend" => "Test bend", "real" => "Real track", _ => Track.Get(id).name,
    };

    static void SetTrack(Settings s, string id, int state)
    {
        var list = s.training.ToList();
        list.Remove(id);
        if (state == 1) list.Add(id);
        if (state == 2) { if (s.test != id) s.test = id; }
        else if (s.test == id) s.test = list.Count > 0 ? list[0] : id;
        if (list.Count == 0) list.Add(id == s.test ? Track.All[0].id : id);
        s.training = list.ToArray();
    }

    public void ClearMarkers()
    {
        foreach (var m in markers) { if (m.root != null) Destroy(m.root.gameObject); if (m.namePlate != null) Destroy(m.namePlate.gameObject); }
        markers.Clear();
        held = null;
        if (shelfLamp != null) { Destroy(shelfLamp); shelfLamp = null; }
        if (desk != null) { Destroy(desk); desk = null; }
        if (beckon != null) { StopCoroutine(beckon); beckon = null; }
        if (beckonLight != null) { Destroy(beckonLight.gameObject); beckonLight = null; }
        beckoning = null;
        MarkersActive = false;
    }

    public void FreezeMarkers(bool frozen) => MarkersActive = !frozen && markers.Count > 0;

    // Race control's points (1.1): shown once the flag is on the finish.
    public void ShowDimMarkers(bool on) { foreach (var m in markers) if (m.dim && m.root != null) { m.root.gameObject.SetActive(on); if (m.plateRoot != null) m.plateRoot.gameObject.SetActive(on && m.Text != ""); } }

    // Where each reward stands once placed: all of them a few cells up the first straight,
    // where the pit lane's camera is, and the flag at the finish.
    (Vector3, float) Spot(string kind)
    {
        var t = track;
        var along = D(t.startHeading);
        var across = Vector3.Cross(Vector3.up, along);
        var outward = D(PitSide == 1 ? (t.startHeading + 3) % 4 : (t.startHeading + 1) % 4);
        var straight = new List<Vector2Int>();
        for (var q = t.start; t.Road(q); q += Track.Dir(t.startHeading)) straight.Add(q);
        Vector2Int At(int i) => straight[Mathf.Min(i, straight.Count - 1)];
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
            case "crash": return (Cell(At(4)) - outward * 0.34f, Yaw(outward));
            case "toward": return (Cell(At(3)) + along * 0.1f, Yaw(along));
            case "away": return (Cell(At(2)) - across * 0.3f * PitSide, Yaw(-along));
            case "moment": return (Cell(At(1)) + outward * 0.47f, Yaw(-outward));
            default: return (Cell(At(2)) - outward * 0.47f, Yaw(outward));
        }
    }

    // The sensors' and the moves' places on the car (the car is at half scale: its length is
    // 0.78), and where each one's name plate stands beside it.
    static Vector3 SensorLocal(int i) => i switch
    {
        Sensors.Front => new Vector3(0f, 0.17f, 0.36f), Sensors.Left => new Vector3(-0.2f, 0.14f, 0.22f), Sensors.Right => new Vector3(0.2f, 0.14f, 0.22f),
        Sensors.Speed => new Vector3(0f, 0.3f, -0.02f), _ => new Vector3(0f, 0.46f, -0.2f),
    };
    static Vector3 SensorPlate(int i) => i switch
    {
        Sensors.Front => new Vector3(0.36f, 0.22f, 0.5f), Sensors.Left => new Vector3(-0.5f, 0.4f, 0.3f), Sensors.Right => new Vector3(0.5f, 0.36f, 0.1f),
        Sensors.Speed => new Vector3(-0.42f, 0.6f, -0.15f), _ => new Vector3(0.42f, 0.62f, -0.35f),
    };
    static Vector3 MoveLocal(int i) => i switch
    {
        Moves.SpeedUp => new Vector3(0f, 0.3f, -0.1f), Moves.Hold => new Vector3(0f, 0.3f, -0.24f), Moves.SlowDown => new Vector3(0f, 0.3f, -0.38f),
        Moves.Left => new Vector3(-0.17f, 0.3f, -0.24f), _ => new Vector3(0.17f, 0.3f, -0.24f),
    };
    static Vector3 MovePlate(int i) => i switch
    {
        Moves.SpeedUp => new Vector3(0f, 0.46f, 0.3f), Moves.Hold => new Vector3(0f, 0.6f, -0.2f), Moves.SlowDown => new Vector3(0f, 0.4f, -0.66f),
        Moves.Left => new Vector3(-0.5f, 0.5f, -0.1f), _ => new Vector3(0.5f, 0.5f, -0.4f),
    };

    // ---- the pieces ----

    Transform MakeMarker(Marker m)
    {
        var root = new GameObject("Marker " + m.kind).transform;
        root.SetParent(m.attached && carRoot != null ? carRoot : transform, false);
        var steel = KitMaterial("Floodlight", "steel");
        var paint = KitMaterial("Paint", "paint");
        if (m.knob)
        {
            // A round knob standing on the console's panel, its pointer at the rim; its value sits on
            // its face (MakePlate) and its name on the panel below it.
            var body = Cylinder(root, 0.08f, 0.04f, Vector3.up * 0.02f, steel, new Color(0.2f, 0.22f, 0.26f));
            body.name = "Body";
            var pointer = Box(root, new Vector3(0.012f, 0.01f, 0.025f), new Vector3(0f, 0.045f, 0.07f), paint, Color.white);
            pointer.name = "Pointer";
            return root;
        }
        switch (m.kind)
        {
            case "stopcap": case "nevercap":
            {
                // A red cap on the panel above its knob: raised while it is on, flat and grey when off.
                var cap = Cylinder(root, 0.04f, 0.025f, Vector3.up * 0.0125f, paint, Theme.Danger);
                cap.name = "Cap";
                break;
            }
            case "finish": { var flag = Instantiate(Piece("FinishFlag"), root, false); flag.transform.localScale = Vector3.one * (M * 1.3f); flag.name = "MarkerFlag"; break; }
            case "crash":
                for (int i = 0; i < 3; i++)
                {
                    var cone = Instantiate(Piece("Cone"), root, false);
                    cone.transform.localScale = Vector3.one * (M * 1.7f);
                    cone.transform.localPosition = new Vector3((i - 1) * 0.22f, 0f, (i % 2) * 0.08f);
                }
                break;
            case "toward": for (int i = 0; i < 3; i++) Chevron(root, new Vector3(0f, 0.008f, i * 0.34f), Theme.Teal); break;
            case "away": Chevron(root, new Vector3(0f, 0.008f, 0f), Theme.Danger); break;
            case "moment":
            {
                Post(root, 0.7f);
                Disc(root, 0.16f, Vector3.up * 0.8f, paint, Color.white);
                var hand = Box(root, new Vector3(0.016f, 0.1f, 0.004f), new Vector3(0f, 0.84f, -0.016f), steel, Color.black);
                hand.transform.localEulerAngles = new Vector3(0, 0, -30f);
                break;
            }
            case "bonus": Post(root, 0.66f); Box(root, new Vector3(0.36f, 0.28f, 0.02f), Vector3.up * 0.82f, KitMaterial("Cone", "cone"), new Color(1f, 0.62f, 0.12f)); break;
            case "limit": Box(root, new Vector3(0.12f, 0.08f, 0.015f), Vector3.zero, steel, new Color(0.15f, 0.15f, 0.17f)); break;
            default:
                if (m.card)
                {
                    // The card turns to the camera, so its map reads from wherever the pit lane is seen.
                    root.gameObject.AddComponent<Billboard>().cam = cam.transform;
                    Box(root, new Vector3(0.64f, 0.46f, 0.02f), Vector3.up * 0.56f, KitMaterial("Barrier", "barrier"), new Color(0.1f, 0.12f, 0.2f));
                    Post(root, 0.33f);
                    var tr = Track.Get(m.kind.Substring(6));
                    float cell = Mathf.Min(0.54f / tr.W, 0.27f / tr.H); // the map under the name strip at the card's top
                    float ox = -cell * tr.W / 2f, oy = 0.50f + cell * tr.H / 2f;
                    for (int y = 0; y < tr.H; y++)
                        for (int x = 0; x < tr.W; x++)
                        {
                            var c = new Vector2Int(x, y);
                            if (!tr.Road(c)) continue;
                            var tint = tr.Finish(c) ? Theme.Amber : c == tr.start ? Theme.Teal : new Color(0.9f, 0.92f, 0.95f);
                            Box(root, new Vector3(cell * 0.85f, cell * 0.85f, 0.01f), new Vector3(ox + (x + 0.5f) * cell, oy - (y + 0.5f) * cell, -0.015f), KitMaterial("Paint", "paint"), tint);
                        }
                }
                else if (m.kind.StartsWith("sense"))
                {
                    var lamp = Disc(root, 0.07f, Vector3.zero, paint, Theme.Teal); lamp.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                    Box(root, new Vector3(0.1f, 0.025f, 0.1f), new Vector3(0f, -0.012f, 0f), steel, new Color(0.2f, 0.22f, 0.26f));
                }
                else if (m.kind.StartsWith("move"))
                {
                    int i = m.kind[m.kind.Length - 1] - '0';
                    Box(root, new Vector3(0.12f, 0.012f, 0.12f), new Vector3(0f, -0.008f, 0f), steel, new Color(0.2f, 0.22f, 0.26f));
                    if (i == Moves.Hold) Box(root, new Vector3(0.09f, 0.008f, 0.035f), Vector3.zero, paint, Theme.Teal);
                    else { var ch = Chevron(root, Vector3.zero, Theme.Teal); ch.transform.localScale = Vector3.one * 0.42f; ch.transform.localRotation = Quaternion.Euler(0f, i == Moves.SlowDown ? 180f : i == Moves.Left ? -90f : i == Moves.Right ? 90f : 0f, 0f); }
                }
                break;
        }
        return root;
    }

    // The marker's plate: the same dark plate as every label in the world. A knob's value sits
    // on its face and its name on the panel below it; a cap's name on the panel above it; a
    // card's name in a strip at its top. The rest stand over their marker, turned to the camera.
    void MakePlate(Marker m)
    {
        string kind = m.kind;
        bool cap = kind.EndsWith("cap");
        bool part = kind.StartsWith("sense") || kind.StartsWith("move") || kind == "limit";
        if (!cap)
        {
            float size = m.card ? 1.1f : part ? 1.15f : m.knob ? (m.labels != null && m.labels.Max(l => l.Length) > 4 ? 0.6f : 0.8f) : Rewards.Contains(kind) ? 2.4f : 2f;
            m.baseLift = kind == "finish" ? 1.95f : kind == "crash" ? 0.6f : kind == "toward" || kind == "away" ? 0.42f : part ? 0f : 1.05f;
            m.plateRoot = DarkPlate(m.root, "", size, out m.plate, m.knob ? 0.1f : part ? 0.2f : 0.26f);
            m.plateRoot.name = "Plate";
            if (m.knob) { m.plateRoot.localPosition = Vector3.up * 0.058f; m.plateRoot.localRotation = OnPanel; m.plateRoot.GetComponent<PlateFit>().pad = 0.04f; }
            else if (m.card) m.plateRoot.localPosition = new Vector3(0f, 0.73f, -0.012f);
            else m.plateRoot.localPosition = part ? m.plateLocal - m.local : Vector3.up * m.baseLift;
            m.plateRoot.gameObject.SetActive(false);
        }
        if (m.knob || cap)
        {
            // The name, flat on the panel, the halo lit when the level is about this control. A
            // control the level is not about keeps its name for a moment after a tap only.
            m.namePlate = DarkPlate(m.root, KnobNames[kind], 0.62f, out var nameText, 0.14f);
            m.namePlate.name = "Name " + kind;
            m.namePlate.localPosition = new Vector3(0f, 0.03f, cap ? -0.09f : m.column % 2 == 0 ? 0.15f : 0.27f); // the names staggered, so neighbours never overlap
            m.namePlate.localRotation = OnPanel;
            nameText.color = m.lit ? Color.white : new Color(0.62f, 0.66f, 0.72f);
            var halo = m.namePlate.Find("Halo"); if (halo != null) halo.gameObject.SetActive(m.lit);
            m.namePlate.gameObject.SetActive(m.lit);
        }
    }

    // A plate lying on the console's panel: readable from above, its top up the slope.
    static readonly Quaternion OnPanel = Quaternion.LookRotation(Vector3.down, Vector3.back);

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
        // Both faces: the back too, so a disc reads from either side.
        int baseCount = v.Count;
        for (int i = 0; i < baseCount; i++) { v.Add(v[i]); uv.Add(uv[i]); }
        for (int i = 1; i <= n; i++) tris.AddRange(new[] { baseCount, baseCount + i, baseCount + i + 1 });
        var mesh = new Mesh { name = "disc" };
        mesh.SetVertices(v); mesh.SetUVs(0, uv); mesh.SetTriangles(tris, 0);
        var normals = new List<Vector3>(); for (int i = 0; i < baseCount; i++) normals.Add(Vector3.back); for (int i = 0; i < baseCount; i++) normals.Add(Vector3.forward);
        mesh.SetNormals(normals);
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

    // A short cylinder standing on its base: the knobs.
    GameObject Cylinder(Transform parent, float radius, float height, Vector3 at, Material mat, Color? tint)
    {
        var v = new List<Vector3>(); var nrm = new List<Vector3>(); var uv = new List<Vector2>(); var tris = new List<int>();
        const int n = 18;
        float h = height / 2f;
        for (int i = 0; i <= n; i++)
        {
            float a = i / (float)n * Mathf.PI * 2f;
            var dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
            v.Add(dir * radius - Vector3.up * h); v.Add(dir * radius + Vector3.up * h);
            nrm.Add(dir); nrm.Add(dir);
            uv.Add(new Vector2(i / (float)n, 0)); uv.Add(new Vector2(i / (float)n, 1));
            if (i > 0) { int b = (i - 1) * 2; tris.AddRange(new[] { b, b + 1, b + 2, b + 1, b + 3, b + 2 }); }
        }
        foreach (float sign in new[] { 1f, -1f })
        {
            int c = v.Count;
            v.Add(Vector3.up * h * sign); nrm.Add(Vector3.up * sign); uv.Add(new Vector2(0.5f, 0.5f));
            for (int i = 0; i <= n; i++)
            {
                float a = i / (float)n * Mathf.PI * 2f;
                v.Add(new Vector3(Mathf.Cos(a) * radius, h * sign, Mathf.Sin(a) * radius)); nrm.Add(Vector3.up * sign); uv.Add(new Vector2(0.5f + Mathf.Cos(a) * 0.5f, 0.5f + Mathf.Sin(a) * 0.5f));
                if (i > 0) { if (sign > 0) tris.AddRange(new[] { c, c + i + 1, c + i }); else tris.AddRange(new[] { c, c + i, c + i + 1 }); }
            }
        }
        var mesh = new Mesh { name = "cylinder" };
        mesh.SetVertices(v); mesh.SetNormals(nrm); mesh.SetUVs(0, uv); mesh.SetTriangles(tris, 0);
        mesh.RecalculateTangents(); mesh.RecalculateBounds();
        var go = new GameObject("Cylinder");
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

    static Mesh unitBox;
    static Mesh BoxMesh(Vector3 s)
    {
        if (s == Vector3.one && unitBox != null) return unitBox;
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
        if (s == Vector3.one) unitBox = mesh;
        return mesh;
    }

    static readonly Dictionary<string, Material> kitMaterials = new Dictionary<string, Material>();
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

    // Race control's: everything darkened, the plate's text grey.
    void Dim(Transform root)
    {
        foreach (var r in root.GetComponentsInChildren<Renderer>())
        {
            if (r.GetComponent<TextMeshPro>() != null || r.name == "Slab" || r.name == "Halo") continue;
            var block = new MaterialPropertyBlock();
            r.GetPropertyBlock(block);
            var c = block.GetColor("_BaseColor"); if (c == Color.clear || c.a == 0f) c = Color.white;
            block.SetColor("_BaseColor", c * 0.45f);
            r.SetPropertyBlock(block);
        }
        var plate = root.GetComponentInChildren<TextMeshPro>(true);
        if (plate != null) plate.color = new Color(0.7f, 0.74f, 0.8f, 0.85f);
    }

    // ---- state ----

    static string Number(float v) => Mathf.Abs(v) >= 10f ? v.ToString("+0;-0") : v.ToString("+0.#;-0.#");

    void ShowPlate(Marker m, bool on)
    {
        if (m.plateRoot == null) return;
        m.plateRoot.gameObject.SetActive(on && !string.IsNullOrEmpty(m.plate.text));
        if (on) FitPlate(m.plateRoot);
    }

    void Settle(Marker m, bool jump)
    {
        if (m.attached)
        {
            m.root.localPosition = m.local; m.root.localRotation = Quaternion.identity;
            // A part that is off is a ghost: there, faint, to be tapped back on.
            foreach (var r in m.root.GetComponentsInChildren<Renderer>()) if (r.GetComponent<TextMeshPro>() == null && r.name != "Box" && r.name != "Slab" && r.name != "Halo") Tint(r, m.Value > 0 ? (m.kind == "limit" ? new Color(0.15f, 0.15f, 0.17f) : Theme.Teal) : new Color(0.35f, 0.38f, 0.42f, 1f));
            if (m.plate != null) { m.plate.text = m.Text; m.plate.color = m.Value > 0 ? Color.white : new Color(0.62f, 0.66f, 0.72f); ShowPlate(m, !m.quiet || Time.unscaledTime < m.namedUntil); }
            m.set(m.Value);
            return;
        }
        if (m.parent != null)
        {
            // The cap on the panel above its knob: raised and red while on, flat and grey when off.
            m.root.position = m.home; m.root.rotation = m.pose;
            var cap = m.root.Find("Cap");
            if (cap != null) { cap.localScale = new Vector3(1f, m.Value > 0 ? 1f : 0.3f, 1f); Tint(cap.GetComponent<Renderer>(), m.Value > 0 ? Theme.Danger : new Color(0.35f, 0.38f, 0.42f)); }
            m.set(m.Value);
            return;
        }
        if (m.card)
        {
            m.root.position = m.spot;
            var plateColour = m.index == 2 ? Theme.Amber : m.index == 1 ? Theme.Teal : new Color(0.6f, 0.64f, 0.7f);
            foreach (var r in m.root.GetComponentsInChildren<Renderer>()) if (r.name == "Box" && Mathf.Abs(r.transform.localPosition.y - 0.56f) < 0.001f && r.transform.localPosition.z == 0f) Tint(r, m.index == 2 ? new Color(0.45f, 0.3f, 0.05f) : m.index == 1 ? new Color(0.05f, 0.3f, 0.28f) : new Color(0.1f, 0.12f, 0.2f));
            m.plate.text = m.Text; m.plate.color = plateColour; ShowPlate(m, true);
            m.set(m.Value);
            return;
        }
        if (m.knob)
        {
            m.root.position = m.home; m.root.rotation = m.pose;
            // The pointer turns with the value; a lit knob is teal, a dim one grey.
            var pointer = m.root.Find("Pointer"); var body = m.root.Find("Body");
            float turn = m.options.Length > 1 ? -135f + 270f * (m.index / (float)(m.options.Length - 1)) : 0f;
            if (m.kind == "seed") turn = 0f;
            if (pointer != null) { pointer.localRotation = Quaternion.Euler(0f, turn, 0f); pointer.localPosition = Quaternion.Euler(0f, turn, 0f) * new Vector3(0f, 0.045f, 0.07f); }
            if (body != null) Tint(body.GetComponent<Renderer>(), m.lit ? new Color(0.09f, 0.42f, 0.38f) : new Color(0.2f, 0.22f, 0.26f));
            if (pointer != null) Tint(pointer.GetComponent<Renderer>(), m.lit ? Theme.Teal : new Color(0.7f, 0.74f, 0.8f));
            m.plate.text = m.Text; m.plate.color = m.lit ? Color.white : new Color(0.62f, 0.66f, 0.72f);
            var halo = m.plateRoot.Find("Halo"); if (halo != null) halo.gameObject.SetActive(m.lit);
            ShowPlate(m, true);
            m.set(m.Value);
            foreach (var cap in markers) if (cap.parent == m) Settle(cap, true);
            return;
        }
        var at = m.Placed ? m.spot : m.home;
        float yaw = m.kind == "finish" ? Yaw(D(track.startHeading)) : m.Placed ? m.spotYaw : Yaw(D(track.startHeading)) + 90f;
        if (jump) { m.root.position = at; m.root.rotation = Quaternion.Euler(0, yaw, 0); }
        else StartCoroutine(Glide(m, at, yaw));
        m.plate.text = m.Text; ShowPlate(m, true);
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

    // The walk and a test act on a marker by name: a reward placed with a value, a knob or a
    // part set to an option's value.
    public void PlaceMarker(string kind, float? value = null)
    {
        var m = markers.FirstOrDefault(x => x.kind == kind && !x.dim);
        if (m == null) return;
        m.index = m.placeable ? 1 : m.index;
        if (value.HasValue) for (int i = 0; i < m.options.Length; i++) if (Mathf.Abs(m.options[i] - value.Value) < 0.001f) m.index = i;
        Settle(m, false);
        Pop(m);
        markersChanged?.Invoke();
    }

    public void TapMarker(string kind)
    {
        var m = markers.FirstOrDefault(x => x.kind == kind && !x.dim);
        if (m != null) Tap(m);
    }

    public Marker MarkerOf(string kind) => markers.FirstOrDefault(x => x.kind == kind && !x.dim);

    void Pop(Marker m) { if (m.root != null && !m.attached) StartCoroutine(DoPunch(m.root)); else if (m.root != null && carBody != null) StartCoroutine(DoPunch(carBody)); }

    // A marker that wants attention: it pulses under a teal lamp.
    public void Beckon(string kind, bool on)
    {
        var m = MarkerOf(kind);
        if (beckon != null) { StopCoroutine(beckon); beckon = null; }
        if (beckoning != null && beckoning.root != null) beckoning.root.localScale = Vector3.one;
        if (beckonLight != null) Destroy(beckonLight.gameObject);
        beckoning = null;
        if (on && m != null) { beckoning = m; beckon = StartCoroutine(DoBeckon(m)); }
    }
    Coroutine beckon; Marker beckoning; Light beckonLight;
    System.Collections.IEnumerator DoBeckon(Marker m)
    {
        beckonLight = new GameObject("Beckon").AddComponent<Light>();
        beckonLight.transform.SetParent(m.root, false); beckonLight.transform.localPosition = Vector3.up * (m.knob ? 0.25f : 0.5f);
        beckonLight.type = LightType.Point; beckonLight.color = Theme.Teal; beckonLight.range = m.knob ? 0.9f : 1.6f; beckonLight.shadows = LightShadows.None;
        while (m.root != null)
        {
            float k = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 4f);
            if (!m.attached) m.root.localScale = Vector3.one * (1f + 0.08f * k);
            beckonLight.intensity = 1.5f + 2.5f * k;
            yield return null;
        }
    }

    // A tap: a reward on the shelf is placed, a placed one turns its number; a knob turns; a
    // part comes off or goes on; a cap comes off; a card changes its role.
    void Tap(Marker m)
    {
        if (m.dim) return;
        if (m.quiet || ((m.knob || m.parent != null) && !m.lit)) { m.namedUntil = Time.unscaledTime + 2.5f; if (m.namePlate != null) m.namePlate.gameObject.SetActive(true); }
        if (m.placeable) m.index = m.Placed ? 1 + (m.index % (m.options.Length - 1)) : 1;
        else if (m.kind == "seed") { m.set(1f); m.plate.text = m.labels[0]; ShowPlate(m, true); Pop(m); Sfx.Tick(); markersChanged?.Invoke(); return; }
        else m.index = (m.index + 1) % m.options.Length;
        if (m.card)
        {
            // One test track at a time: the card that was the test becomes training.
            if (m.index == 2) foreach (var o in markers) if (o.card && o != m && o.index == 2) { o.index = 1; Settle(o, true); }
        }
        Settle(m, !m.placeable);
        Sfx.Tick();
        Pop(m);
        markersChanged?.Invoke();
    }

    // Every frame while a pit lane is up: the plates face the camera and keep out of each
    // other's way; the pointer picks, drags, drops and taps.
    void UpdateMarkers()
    {
        if (cam == null || markers.Count == 0) return;
        foreach (var m in markers)
        {
            // A knob's or a cap's name shows for a moment after a tap when the level is not about it.
            if ((m.knob || m.parent != null) && !m.lit && m.namePlate != null && m.namePlate.gameObject.activeSelf && Time.unscaledTime >= m.namedUntil) m.namePlate.gameObject.SetActive(false);
            if (m.plateRoot == null || m.knob || m.card) continue; // the console's and the cards' plates lie where they are
            if (m.quiet && m.plateRoot.gameObject.activeSelf && Time.unscaledTime >= m.namedUntil) m.plateRoot.gameObject.SetActive(false);
            if (m.plateRoot.gameObject.activeSelf) m.plateRoot.rotation = Quaternion.LookRotation(m.plateRoot.position - cam.transform.position);
        }
        Unstack();
        if (!MarkersActive) return;
        bool overUi = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        Vector2 p = Input.mousePosition;
        if (Input.GetMouseButtonDown(0) && !overUi)
        {
            held = Pick(p);
            pointerDown = held != null;
            downAt = p; downTime = Time.unscaledTime;
            if (held != null && held.placeable) { GroundHit(p, out var g); heldOffset = held.root.position - g; heldOffset.y = 0; }
        }
        if (pointerDown && held != null && held.placeable && Input.GetMouseButton(0))
        {
            if ((p - downAt).magnitude > 12f && GroundHit(p, out var g))
            {
                var at = g + heldOffset; at.y = 0.12f;
                held.root.position = at;
                held.plate.text = Number(held.options[Mathf.Max(1, held.index)]);
                ShowPlate(held, true);
            }
        }
        if (pointerDown && Input.GetMouseButtonUp(0))
        {
            pointerDown = false;
            if (held == null) return;
            bool tap = (p - downAt).magnitude <= 12f && Time.unscaledTime - downTime < 0.6f;
            if (tap || !held.placeable) Tap(held);
            else
            {
                var flat = held.root.position; flat.y = 0;
                bool near = (flat - held.spot).magnitude < 0.9f;
                bool wasPlaced = held.Placed;
                held.index = near ? Mathf.Max(1, held.index) : 0;
                Settle(held, false);
                if (near && !wasPlaced) { Sfx.Chime(); Pop(held); } else if (!near && wasPlaced) Sfx.Tick();
                markersChanged?.Invoke();
            }
            held = null;
        }
    }

    // Plates never sit on each other: the lowest on the screen settles first; each higher one
    // lifts until it is clear of those below. A part's plate starts beside its part.
    void Unstack()
    {
        var shown = markers.Where(m => m.plateRoot != null && !m.knob && !m.card && m.plateRoot.gameObject.activeSelf && m.root != null && m.root.gameObject.activeInHierarchy).ToList();
        if (shown.Count < 2) return;
        var rects = new List<Rect>();
        foreach (var m in shown.OrderBy(x => cam.ToScreen(x.root.position).y))
        {
            var start = m.attached ? m.plateLocal - m.local : Vector3.up * m.baseLift;
            float lift = 0f;
            var slab = m.plateRoot.Find("Slab");
            float worldH = slab != null ? slab.localScale.y : 0.15f, worldW = slab != null ? slab.localScale.x : 0.4f;
            for (int guard = 0; guard < 10; guard++)
            {
                m.plateRoot.localPosition = start + Vector3.up * lift;
                var s = cam.ToScreen(m.plateRoot.position);
                var top = cam.ToScreen(m.plateRoot.position + Vector3.up * worldH);
                var side = cam.ToScreen(m.plateRoot.position + m.plateRoot.right * worldW);
                float h = Mathf.Max(6f, Mathf.Abs(top.y - s.y)) * 1.1f;
                float w = Mathf.Max(6f, Mathf.Abs(side.x - s.x)) * 1.05f;
                var r = new Rect(s.x - w / 2, s.y - h / 2, w, h);
                if (!rects.Any(o => o.Overlaps(r))) { rects.Add(r); break; }
                lift += worldH * 1.1f;
            }
        }
    }

    Marker Pick(Vector2 screen)
    {
        Marker best = null; float bestD = 70f;
        foreach (var m in markers)
        {
            if (m.dim || m.root == null || !m.root.gameObject.activeInHierarchy) continue;
            var s = cam.ToScreen(m.root.position + Vector3.up * (m.attached || m.parent != null || m.knob ? 0f : 0.35f));
            if (s.z < 0) continue;
            float d = Vector2.Distance(new Vector2(s.x, s.y), screen);
            float reach = m.attached || m.parent != null ? 40f : m.knob ? 50f : 70f;
            // A part's plate counts as the part: it is the thing the player sees to tap.
            if (m.attached && m.plateRoot != null && m.plateRoot.gameObject.activeSelf)
            {
                var ps = cam.ToScreen(m.plateRoot.position);
                if (ps.z > 0) d = Mathf.Min(d, Vector2.Distance(new Vector2(ps.x, ps.y), screen));
            }
            if (d < reach && d < bestD) { bestD = d; best = m; }
        }
        return best;
    }

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
