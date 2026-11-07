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
// - the dials on the shelf, tapped to turn: the fuel can (the attempts), the die, the fall
//   and the floor (the chance of a new move), the rewrite dial and the seed (the knobs), the
//   stopwatch (the measure); a red cap on the fuel can (stop at the first finish) or on the die
//   (never again after the first finish) comes off with a tap;
// - the parts on the car: the five sensors and the five moves, fitted or removed with a tap,
//   and the limiter at the tail (the top speed);
// - the track cards on the board by the pit lane: off, training, or the test track.
//
// Everything is made at run time from the kit's own pieces and materials. No physics: picking
// by projection to the screen, dragging along the pointer's ray on the ground. Race control's
// points (1.1, once the flag is on the finish) stand dim and cannot be touched.
public partial class Circuit
{
    public class Marker
    {
        public string kind;
        public float[] options;      // for a reward the first is zero: on the shelf
        public string[] labels;      // the plate's text per option; null: the number
        public int index;
        public Vector3 home, spot;   // shelf and track (rewards); the stand (dials, cards)
        public float spotYaw;
        public Vector3 local;        // a part on the car: where it sits on it
        public bool placeable, attached, card, dim;
        public Marker parent;        // a cap sits on its parent
        public Transform root;
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
    GameObject shelfLamp;
    public bool MarkersActive { get; private set; }
    public IReadOnlyList<Marker> Markers => markers;

    public static readonly string[] Rewards = { "finish", "crash", "toward", "away", "moment", "bonus" };

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

    // kinds: which markers this level has; fixedKinds: race control's, dim and untouchable.
    // nameParts: the parts on the car carry their names; false (the capstones, where every
    // part is open) shows a name only for two seconds after a tap.
    public void BuildMarkers(Settings s, IEnumerable<string> kindsIn, Action changed, IEnumerable<string> fixedKinds = null, bool nameParts = true)
    {
        ClearMarkers();
        if (!HasKit) return;
        markersChanged = changed;
        var kinds = kindsIn.ToList();
        var t = track;
        var along = D(t.startHeading);
        var outward = D(PitSide == 1 ? (t.startHeading + 3) % 4 : (t.startHeading + 1) % 4);
        // The shelf: up the pit lane from the grid, clear of the gantry, in two rows when long.
        var shelf0 = Cell(t.start) + outward * (0.5f + 1.7f * M) + along * 0.9f;
        if (kit != null)
            foreach (Transform child in kit)
            {
                // The kit's own cones and pit box give way to the shelf and the markers; the floodlight
                // mast beside the shelf would stand between the camera and the plates.
                if (child.name.StartsWith("Cone") || child.name.StartsWith("PitBox")) child.gameObject.SetActive(false);
                if (child.name.StartsWith("Floodlight") && (child.position - shelf0).magnitude < 3f) child.gameObject.SetActive(false);
            }
        int n = 0, cards = 0;
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
                m.home = m.spot = Cell(t.start) + outward * (0.5f + 1.1f * M + (cards / 5) * 0.62f) + along * (1.5f + (cards % 5) * 0.64f);
                m.spotYaw = Yaw(-outward);
                cards++;
            }
            else if (kind.StartsWith("sense") || kind.StartsWith("move") || kind == "limit")
            {
                m.attached = true;
                int i = kind == "limit" ? 0 : kind[kind.Length - 1] - '0';
                if (kind.StartsWith("sense")) { m.options = new[] { 1f, 0f }; m.index = s.sensors[i] ? 0 : 1; m.set = v => { s.sensors[i] = v > 0; SetSensors(s.sensors); }; m.local = SensorLocal(i); m.labels = new[] { Sensors.Names[i], Sensors.Names[i] }; }
                else if (kind.StartsWith("move")) { m.options = new[] { 1f, 0f }; m.index = s.moves[i] ? 0 : 1; m.set = v => { s.moves[i] = v > 0; if (s.MovesOn == 0) s.moves[i] = true; }; m.local = MoveLocal(i); m.labels = new[] { Moves.Names[i], Moves.Names[i] }; }
                else { m.options = Options["limit"]; m.labels = Labels["limit"]; m.index = s.topSpeed == 1 ? 1 : 0; m.set = v => { s.topSpeed = (int)v; s.constraintSet = true; }; m.local = new Vector3(0f, 0.3f, -0.4f); }
                m.home = m.spot = carRoot != null ? carRoot.position : Cell(t.start);
                m.quiet = !nameParts && kind != "limit";
            }
            else if (kind == "stopcap" || kind == "nevercap")
            {
                m.parent = kind == "stopcap" ? fuel : die;
                if (m.parent == null) continue;
                m.options = Options[kind];
                m.index = kind == "stopcap" ? (s.stopAtFirstFinish ? 0 : 1) : (s.exploreOffAfterFinish ? 0 : 1);
                m.set = kind == "stopcap" ? (Action<float>)(v => s.stopAtFirstFinish = v > 0) : v => s.exploreOffAfterFinish = v > 0;
                m.home = m.spot = m.parent.home + Vector3.up * 0.42f;
            }
            else
            {
                if (!Options.ContainsKey(kind)) continue;
                m.options = Options[kind];
                Labels.TryGetValue(kind, out m.labels);
                m.placeable = Rewards.Contains(kind);
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
                int row = n / 7;
                m.home = shelf0 + along * (0.55f * (n % 7)) + outward * (row * 0.7f);
                if (m.placeable) (m.spot, m.spotYaw) = Spot(kind); else { m.spot = m.home; m.spotYaw = Yaw(along) + 90f; }
                if (kind == "fuel") fuel = m;
                if (kind == "die") die = m;
                n++;
            }
            m.root = MakeMarker(m);
            m.plate = Plate(m.root, m.kind, out m.baseLift);
            markers.Add(m);
            Settle(m, true);
            if (m.dim) Dim(m.root);
            Debug.Log($"[Markers] {kind}: home {m.home} spot {m.spot} index {m.index}{(m.dim ? " dim" : "")}");
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
    }

    // The seed's plate must read the seed as it is (the marker reads it back after a tap).
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
        foreach (var m in markers) if (m.root != null) Destroy(m.root.gameObject);
        markers.Clear();
        held = null;
        if (shelfLamp != null) { Destroy(shelfLamp); shelfLamp = null; }
        if (beckon != null) { StopCoroutine(beckon); beckon = null; }
        if (beckonLight != null) { Destroy(beckonLight.gameObject); beckonLight = null; }
        beckoning = null;
        MarkersActive = false;
    }

    public void FreezeMarkers(bool frozen) => MarkersActive = !frozen && markers.Count > 0;

    // Race control's points (1.1): shown once the flag is on the finish.
    public void ShowDimMarkers(bool on) { foreach (var m in markers) if (m.dim && m.root != null) m.root.gameObject.SetActive(on); }

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

    // The sensors' and the moves' places on the car (the car is at half scale: its length is 0.78).
    static Vector3 SensorLocal(int i) => i switch
    {
        Sensors.Front => new Vector3(0f, 0.17f, 0.36f), Sensors.Left => new Vector3(-0.2f, 0.14f, 0.22f), Sensors.Right => new Vector3(0.2f, 0.14f, 0.22f),
        Sensors.Speed => new Vector3(0f, 0.3f, -0.02f), _ => new Vector3(0f, 0.46f, -0.2f),
    };
    static Vector3 MoveLocal(int i) => i switch
    {
        Moves.SpeedUp => new Vector3(0f, 0.3f, -0.1f), Moves.Hold => new Vector3(0f, 0.3f, -0.24f), Moves.SlowDown => new Vector3(0f, 0.3f, -0.38f),
        Moves.Left => new Vector3(-0.17f, 0.3f, -0.24f), _ => new Vector3(0.17f, 0.3f, -0.24f),
    };

    // ---- the pieces ----

    Transform MakeMarker(Marker m)
    {
        var root = new GameObject("Marker " + m.kind).transform;
        root.SetParent(m.attached && carRoot != null ? carRoot : transform, false);
        var steel = KitMaterial("Floodlight", "steel");
        var paint = KitMaterial("Paint", "paint");
        switch (m.kind)
        {
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
            case "fuel":
                Box(root, new Vector3(0.3f, 0.36f, 0.16f), Vector3.up * 0.18f, steel, new Color(0.78f, 0.16f, 0.1f));
                Box(root, new Vector3(0.12f, 0.04f, 0.06f), new Vector3(0f, 0.38f, 0f), steel, Color.black);
                Box(root, new Vector3(0.04f, 0.05f, 0.04f), new Vector3(0.1f, 0.38f, 0f), steel, new Color(0.9f, 0.85f, 0.2f));
                break;
            case "die": Box(root, new Vector3(0.2f, 0.2f, 0.2f), Vector3.up * 0.1f, paint, Color.white); Pips(root); break;
            case "seed": Box(root, new Vector3(0.18f, 0.18f, 0.18f), Vector3.up * 0.09f, paint, Theme.Teal); break;
            case "fall": Post(root, 0.5f); Chevron(root, new Vector3(0f, 0.55f, 0f), Theme.Teal).transform.localRotation = Quaternion.Euler(-90f, 0f, 180f); break;
            case "floor": Post(root, 0.4f); Box(root, new Vector3(0.3f, 0.02f, 0.14f), Vector3.up * 0.42f, steel, Theme.Teal); break;
            case "rewrite": Post(root, 0.5f); Disc(root, 0.14f, Vector3.up * 0.62f, steel, new Color(0.25f, 0.28f, 0.32f)); Box(root, new Vector3(0.016f, 0.1f, 0.004f), new Vector3(0f, 0.66f, -0.016f), steel, Theme.Teal); break;
            case "measure": Post(root, 0.6f); Disc(root, 0.17f, Vector3.up * 0.76f, paint, Color.white); Box(root, new Vector3(0.016f, 0.12f, 0.004f), new Vector3(0f, 0.8f, -0.018f), steel, Theme.Danger); Box(root, new Vector3(0.05f, 0.05f, 0.03f), new Vector3(0f, 0.96f, 0f), steel, null); break;
            case "stopcap": case "nevercap": Disc(root, 0.1f, Vector3.zero, paint, Theme.Danger).transform.localRotation = Quaternion.Euler(90f, 0f, 0f); break;
            case "limit": Box(root, new Vector3(0.12f, 0.08f, 0.015f), Vector3.zero, steel, new Color(0.15f, 0.15f, 0.17f)); break;
            default:
                if (m.card)
                {
                    // The card turns to the camera, so its map reads from wherever the pit lane is seen.
                    root.gameObject.AddComponent<Billboard>().cam = cam.transform;
                    Box(root, new Vector3(0.64f, 0.4f, 0.02f), Vector3.up * 0.56f, KitMaterial("Barrier", "barrier"), new Color(0.1f, 0.12f, 0.2f));
                    Post(root, 0.36f);
                    var tr = Track.Get(m.kind.Substring(6));
                    float cell = Mathf.Min(0.54f / tr.W, 0.32f / tr.H);
                    float ox = -cell * tr.W / 2f, oy = 0.56f + cell * tr.H / 2f;
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

    void Pips(Transform root)
    {
        var steel = KitMaterial("Floodlight", "steel");
        foreach (var (x, y) in new[] { (-0.05f, 0.15f), (0.05f, 0.05f), (0f, 0.1f), (-0.05f, 0.05f), (0.05f, 0.15f) })
            Disc(root, 0.018f, new Vector3(x, y, -0.101f), steel, Color.black);
    }

    // The number or the word on a marker: a plate of text above it, turned to the camera.
    TextMeshPro Plate(Transform root, string kind, out float lift)
    {
        lift = kind == "finish" ? 1.95f : kind == "crash" ? 0.6f : kind == "toward" || kind == "away" ? 0.42f : kind == "fuel" ? 0.62f : kind == "die" || kind == "seed" ? 0.42f
             : kind == "measure" ? 1.15f : kind == "limit" ? 0.12f : kind.StartsWith("track:") ? 0.86f : kind.StartsWith("sense") || kind.StartsWith("move") ? 0.12f : kind.EndsWith("cap") ? -1f : 1.05f;
        var go = new GameObject("Plate");
        go.transform.SetParent(root, false);
        go.transform.localPosition = Vector3.up * Mathf.Max(0f, lift);
        var t = go.AddComponent<TextMeshPro>();
        bool reward = Rewards.Contains(kind);
        bool part = kind.StartsWith("sense") || kind.StartsWith("move") || kind == "limit";
        t.font = Theme.Bold; t.fontSize = kind.StartsWith("track:") ? 1.9f : part ? 1.2f : reward ? 3.4f : 2.4f; t.color = Color.white; t.alignment = TextAlignmentOptions.Center;
        t.rectTransform.sizeDelta = new Vector2(1.4f, 0.4f);
        t.outlineWidth = 0.25f; t.outlineColor = new Color32(11, 21, 54, 255);
        t.text = "";
        if (lift < 0) go.SetActive(false);
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

    // Race control's: everything darkened, the plate grey.
    void Dim(Transform root)
    {
        foreach (var r in root.GetComponentsInChildren<Renderer>())
        {
            if (r.GetComponent<TextMeshPro>() != null) continue;
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

    void Settle(Marker m, bool jump)
    {
        if (m.attached)
        {
            m.root.localPosition = m.local; m.root.localRotation = Quaternion.identity;
            // A part that is off is a ghost: there, faint, to be tapped back on.
            foreach (var r in m.root.GetComponentsInChildren<Renderer>()) if (r.GetComponent<TextMeshPro>() == null && r.name != "Box") Tint(r, m.Value > 0 ? (m.kind == "limit" ? new Color(0.15f, 0.15f, 0.17f) : Theme.Teal) : new Color(0.35f, 0.38f, 0.42f, 1f));
            if (m.plate != null) { m.plate.text = m.Text; m.plate.color = m.Value > 0 ? Color.white : new Color(0.62f, 0.66f, 0.72f); if (m.quiet) m.plate.gameObject.SetActive(Time.unscaledTime < m.namedUntil); }
            m.set(m.Value);
            return;
        }
        if (m.parent != null)
        {
            m.root.position = m.parent.root.position + Vector3.up * (m.kind == "stopcap" ? 0.42f : 0.22f);
            m.root.rotation = Quaternion.identity;
            m.root.gameObject.SetActive(m.Value > 0);
            m.set(m.Value);
            return;
        }
        if (m.card)
        {
            m.root.position = m.spot;
            var plateColour = m.index == 2 ? Theme.Amber : m.index == 1 ? Theme.Teal : new Color(0.6f, 0.64f, 0.7f);
            foreach (var r in m.root.GetComponentsInChildren<Renderer>()) if (r.name == "Box" && Mathf.Abs(r.transform.localPosition.y - 0.56f) < 0.001f && r.transform.localPosition.z == 0f) Tint(r, m.index == 2 ? new Color(0.45f, 0.3f, 0.05f) : m.index == 1 ? new Color(0.05f, 0.3f, 0.28f) : new Color(0.1f, 0.12f, 0.2f));
            m.plate.text = m.Text; m.plate.color = plateColour;
            m.set(m.Value);
            return;
        }
        var at = m.Placed ? m.spot : m.home;
        float yaw = m.kind == "finish" ? Yaw(D(track.startHeading)) : m.Placed ? m.spotYaw : Yaw(D(track.startHeading)) + 90f;
        if (jump) { m.root.position = at; m.root.rotation = Quaternion.Euler(0, yaw, 0); }
        else StartCoroutine(Glide(m, at, yaw));
        m.plate.text = m.Text;
        m.set(m.Value);
        foreach (var cap in markers) if (cap.parent == m) Settle(cap, true);
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

    // The walk and a test act on a marker by name: a reward placed with a value, a dial or a
    // part set to an option's index.
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
        beckonLight.transform.SetParent(m.root, false); beckonLight.transform.localPosition = Vector3.up * 0.5f;
        beckonLight.type = LightType.Point; beckonLight.color = Theme.Teal; beckonLight.range = 1.6f; beckonLight.shadows = LightShadows.None;
        while (m.root != null)
        {
            float k = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 4f);
            if (!m.attached) m.root.localScale = Vector3.one * (1f + 0.08f * k);
            beckonLight.intensity = 1.5f + 2.5f * k;
            yield return null;
        }
    }

    // A tap: a reward on the shelf is placed, a placed one turns its number; a dial turns; a
    // part comes off or goes on; a cap comes off; a card changes its role.
    void Tap(Marker m)
    {
        if (m.dim) return;
        if (m.quiet) m.namedUntil = Time.unscaledTime + 2f;
        if (m.placeable) m.index = m.Placed ? 1 + (m.index % (m.options.Length - 1)) : 1;
        else if (m.kind == "seed") { m.set(1f); m.plate.text = m.labels[0]; Pop(m); Sfx.Tick(); markersChanged?.Invoke(); return; }
        else m.index = (m.index + 1) % m.options.Length;
        if (m.card)
        {
            // One test track at a time: the card that was the test becomes training.
            if (m.index == 2) foreach (var o in markers) if (o.card && o != m && o.index == 2) { o.index = 1; Settle(o, true); }
        }
        Settle(m, false);
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
            if (m.plate == null) continue;
            if (m.quiet && m.plate.gameObject.activeSelf && Time.unscaledTime >= m.namedUntil) m.plate.gameObject.SetActive(false);
            if (m.plate.gameObject.activeSelf) m.plate.transform.rotation = Quaternion.LookRotation(m.plate.transform.position - cam.transform.position);
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

    // Plates never sit on each other or on a marker: a plate whose box on the screen overlaps
    // a lower one's is lifted until it is clear.
    void Unstack()
    {
        var shown = markers.Where(m => m.plate != null && m.plate.gameObject.activeSelf && m.root != null && m.root.gameObject.activeInHierarchy && !string.IsNullOrEmpty(m.plate.text)).ToList();
        if (shown.Count < 2) return;
        var rects = new List<Rect>();
        // The lowest on the screen settles first; each higher one lifts until it is clear of those below.
        foreach (var m in shown.OrderBy(x => cam.ToScreen(x.root.position).y))
        {
            float lift = m.baseLift;
            // The plate's size on the screen, from its height in the world (the text is about a
            // tenth of its font size tall, and roughly half as wide per character).
            float worldH = m.plate.fontSize * 0.11f;
            for (int guard = 0; guard < 10; guard++)
            {
                m.plate.transform.localPosition = Vector3.up * lift;
                var s = cam.ToScreen(m.plate.transform.position);
                var top = cam.ToScreen(m.plate.transform.position + Vector3.up * worldH);
                float h = Mathf.Max(6f, Mathf.Abs(top.y - s.y)) * 1.2f;
                float w = Mathf.Max(1, m.plate.text.Length) * h * 0.55f;
                var r = new Rect(s.x - w / 2, s.y - h / 2, w, h);
                if (!rects.Any(o => o.Overlaps(r))) { rects.Add(r); break; }
                lift += worldH * 0.9f;
            }
        }
    }

    Marker Pick(Vector2 screen)
    {
        Marker best = null; float bestD = 70f;
        foreach (var m in markers)
        {
            if (m.dim || m.root == null || !m.root.gameObject.activeInHierarchy) continue;
            var s = cam.ToScreen(m.root.position + Vector3.up * (m.attached ? 0f : m.parent != null ? 0f : 0.35f));
            if (s.z < 0) continue;
            float d = Vector2.Distance(new Vector2(s.x, s.y), screen);
            float reach = m.attached || m.parent != null ? 40f : 70f;
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
