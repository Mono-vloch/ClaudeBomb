using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The pit lane: where the robot is set up before training. The car stands on the
// grid on the left, big, its sensors drawn as rays, and its seven settings hang off
// it as tags: what it senses at the nose, what it may do at the wheels, the points
// on the roof, the knobs at the back, the tracks on the ground; a setting the level
// has not opened wears OPENS AT instead. On the right, only what this level opens,
// as controls (the points table on 2.3, the sensors on 2.1, everything on the
// capstone), and the prediction when the level asks for one. The robot runs by
// itself; this is the last word before it does.
public class PitScreen
{
    public readonly RectTransform root;
    readonly App app;
    readonly Settings s;
    readonly RectTransform frame, editor, tags;
    RectTransform coachLine;
    TMP_InputField prediction;
    TextMeshProUGUI note;

    static readonly string[] TrackShort = { "One bend", "Two bends", "Both ways", "Chicane", "Plaza", "Hairpin", "Grand", "Left only", "Test bend", "Real" };

    public PitScreen(Transform parent, App app)
    {
        this.app = app;
        s = app.settings;
        var l = app.lesson;
        root = Ui.Node("Pit", parent);
        Ui.Stretch(root);
        app.Paper(Color.clear);

        frame = Ui.Node("Frame", root);
        Ui.Place(frame, 60, 290, 700, 700);
        BuildWorld();

        var top = Ui.Box(root, "Top", new Color(1, 1, 1, 0.86f), false);
        Ui.Place(top.rectTransform, -400, -300, 2800, 436);
        top.raycastTarget = false;
        var eyebrow = Ui.Eyebrow(root, l != null ? $"LEVEL {l.id}  ·  THE PIT LANE" : "FREE PLAY  ·  THE PIT LANE", Theme.TealDeep);
        Ui.Place(eyebrow.rectTransform, 60, 24, 600, 20);
        var title = Ui.Text(root, l != null ? l.title : "Set up the robot", 34, Theme.Ink, true);
        Ui.Place(title.rectTransform, 60, 48, 1000, 50);
        title.enableWordWrapping = false;
        float bx = 1200;
        if (l != null)
        {
            var back = Ui.Button(root, "Briefing", app.ShowLesson, Ui.ButtonStyle.Quiet, 15);
            Ui.Place(back.GetComponent<RectTransform>(), bx, 44, 100, 42); bx += 104;
        }
        var file = Ui.Button(root, "The file", () => app.ShowFile(), Ui.ButtonStyle.Quiet, 15);
        Ui.Place(file.GetComponent<RectTransform>(), bx, 44, 90, 42); bx += 94;
        var settings = Ui.Button(root, "Settings", () => SettingsSheet.Toggle(app, root, null), Ui.ButtonStyle.Quiet, 15);
        Ui.Place(settings.GetComponent<RectTransform>(), bx, 44, 100, 42);
        var train = Ui.Button(root, "TRAIN THE ROBOT", Train, Ui.ButtonStyle.Accent, 20);
        Ui.Place(train.GetComponent<RectTransform>(), 1600, 32, 260, 64);

        // The track
        var card = Ui.Soft(root, "Track");
        Ui.Place(card.rectTransform, 60, 152, 1800, 108);
        var t0 = Track.Get(s.training[0]);
        var tag = Ui.Eyebrow(card.transform, $"THE TRACK  ·  {t0.name.ToUpperInvariant()}" + (s.training.Length > 1 ? $"  ·  TRAINING ON {s.training.Length} TRACKS" : "") + (s.test != s.training[0] ? $"  ·  TEST TRACK: {Track.Get(s.test).name.ToUpperInvariant()}" : ""), Theme.TealDeep);
        Ui.Place(tag.rectTransform, 24, 12, 1200, 22);
        var tnote = Ui.Text(card.transform, t0.note, 20, Theme.Ink);
        Ui.Place(tnote.rectTransform, 24, 38, 1200, 36);
        note = Ui.Text(card.transform, $"{s.attempts} attempts  ·  {(l != null ? l.laps : 5)} laps on the test track", 14, Theme.Text2);
        Ui.Place(note.rectTransform, 24, 74, 1200, 24);
        LessonScreen.Map(card.transform, t0, 1300, 10, 480, 88);

        tags = Ui.Node("Tags", root);
        Ui.Stretch(tags);
        editor = Ui.Node("Editor", root);
        Ui.Place(editor, 780, 290, 1080, 770);
        BuildEditor();
        BuildTags();
        Coach();
        var follower = root.gameObject.AddComponent<Follower>();
        follower.Bind(this);
        Ui.Emblem(root);
    }

    void BuildWorld()
    {
        app.world.Build(Track.Get(s.training[0]), false);
        app.world.ShowPortrait(frame);
        app.world.SetSensors(s.sensors);
    }

    // The level opened with a setup already in the pit lane: LUMA says why it works, or
    // what is wrong with it, once, over the car; the words stay in the file.
    void Coach()
    {
        if (coachLine != null) Object.Destroy(coachLine.gameObject);
        coachLine = null;
        var start = app.StartingSetup;
        if (start == null) return;
        bool broken = app.lesson.broken != null;
        coachLine = app.luma.Line(root, 60, 996, 700, (broken ? "Fix it: " + start.name + ". " : "") + start.why, true, false, 14);
    }

    void Train()
    {
        if (app.lesson != null && app.lesson.prediction && (prediction == null || prediction.text.Trim().Length < 8))
        {
            note.text = "Write your prediction first.";
            note.color = Theme.Danger;
            return;
        }
        if (prediction != null) app.session.prediction = prediction.text;
        app.StartTraining();
    }

    // ---- the tags on the car ------------------------------------------------------------

    class Hook { public RectTransform rect, lead; public Image dot; public Vector3 local; public int column, row; }
    readonly List<Hook> hooks = new List<Hook>();

    bool Open(bool flag) => app.lesson == null || flag;

    // Seven tags in two columns beside the car, each with a line down to the part of the
    // car it is: the near parts on the left, the far ones on the right.
    void BuildTags()
    {
        Ui.Clear(tags);
        hooks.Clear();
        var l = app.lesson;
        Tag(new Vector3(0f, 0.3f, 0.8f), 0, 0, "SENSES", string.Join(", ", Enumerable.Range(0, Sensors.Count).Where(i => s.sensors[i]).Select(i => Sensors.Names[i])), Open(l.sensors), "2.1");
        Tag(new Vector3(-0.5f, 0.12f, 0.45f), 0, 1, "MOVES", $"{s.MovesOn} of 5  ·  top speed {s.topSpeed}", Open(l.moves), "2.2");
        Tag(new Vector3(-0.9f, 0.02f, 0f), 0, 2, "TRACKS", Track.Get(s.training[0]).name + (s.test != s.training[0] ? "  ·  test " + Track.Get(s.test).name.ToLowerInvariant() : ""), Open(l.tracks), "3.1");
        Tag(new Vector3(-0.9f, 0.02f, -0.8f), 0, 3, "MEASURE", Measures.Names[s.measure], Open(l.measure), "3.2");
        Tag(new Vector3(0f, 0.85f, 0f), 1, 0, "POINTS", $"finish {s.finish:0}  ·  crash {s.crash:0}  ·  toward {s.toward:0.#}", Open(l.points), "2.3");
        Tag(new Vector3(0f, 0.4f, -0.8f), 1, 1, "KNOBS", $"rewrite {s.rewrite:0.00}  ·  {s.attempts} attempts", Open(l.knobs || l.attempts), l.attempts && !l.knobs ? "3.3 (attempts open)" : "3.3");
        Tag(new Vector3(-0.5f, 0.12f, -0.45f), 1, 2, "NEW MOVES", $"{s.explore:0.0}, falls {Settings.Fall(s.exploreFall)}", Open(l.explore), "2.5");
        PlaceTags();
    }

    void Tag(Vector3 local, int column, int row, string name, string value, bool open, string opensAt)
    {
        var line = Ui.Box(tags, "Lead", open ? new Color(Theme.Teal.r, Theme.Teal.g, Theme.Teal.b, 0.7f) : new Color(1f, 1f, 1f, 0.3f), false);
        line.raycastTarget = false;
        var card = Ui.Glass(tags, name, 0, 0, 230, 56, true, open ? Theme.Teal : new Color(1f, 1f, 1f, 0.25f));
        var n = Ui.Eyebrow(card.transform, open ? name : name + "  ·  OPENS AT " + opensAt.ToUpperInvariant(), open ? Theme.Teal : Theme.Faint);
        Ui.Place(n.rectTransform, 16, 8, 210, 16);
        n.enableWordWrapping = false; n.overflowMode = TextOverflowModes.Ellipsis;
        var v = Ui.Text(card.transform, value, 15, open ? Color.white : Theme.Faint, true);
        Ui.Place(v.rectTransform, 16, 26, 210, 24);
        v.enableWordWrapping = false; v.overflowMode = TextOverflowModes.Ellipsis;
        var dot = Ui.Box(tags, "Dot", open ? Theme.Teal : new Color(1f, 1f, 1f, 0.5f));
        Ui.Place(dot.rectTransform, 0, 0, 12, 12);
        dot.raycastTarget = false;
        hooks.Add(new Hook { rect = card.rectTransform, lead = line.rectTransform, dot = dot, local = local, column = column, row = row });
    }

    // The tags stay in their columns; the dots and the lines follow the car after the camera has moved.
    void PlaceTags()
    {
        foreach (var h in hooks)
        {
            float tx = h.column == 0 ? 60 : 530, ty = 300 + h.row * 74;
            Ui.Place(h.rect, tx, ty, 230, 56);
            bool on = app.world.CarPartOnScreen(h.local, out var sp);
            h.dot.gameObject.SetActive(on); h.lead.gameObject.SetActive(on);
            if (!on) continue;
            var p = app.StagePoint(sp);
            Ui.Place(h.dot.rectTransform, p.x - 6, p.y - 6, 12, 12);
            // The line, from the tag's inner edge to the dot.
            float x0 = h.column == 0 ? tx + 230 : tx, y0 = ty + 28;
            float dx = p.x - x0, dy = p.y - y0;
            h.lead.anchorMin = h.lead.anchorMax = new Vector2(0, 1);
            h.lead.pivot = new Vector2(0, 0.5f);
            h.lead.anchoredPosition = new Vector2(x0, -y0);
            h.lead.sizeDelta = new Vector2(Mathf.Sqrt(dx * dx + dy * dy), 2);
            h.lead.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(-dy, dx) * Mathf.Rad2Deg);
        }
    }

    class Follower : MonoBehaviour
    {
        PitScreen screen;
        public void Bind(PitScreen s) { screen = s; s.app.view.Framed += s.PlaceTags; }
        void OnDestroy() { if (screen != null) screen.app.view.Framed -= screen.PlaceTags; }
    }

    // ---- the settings this level opens ---------------------------------------------------

    void BuildEditor()
    {
        Ui.Clear(editor);
        var l = app.lesson;
        bool free = l == null;
        bool tracks = free || l.tracks, sensors = free || l.sensors, moves = free || l.moves, points = free || l.points;
        bool explore = free || l.explore, knobs = free || l.knobs || l.attempts, measure = free || l.measure;
        bool predict = l != null && l.prediction;
        if (!(tracks || sensors || moves || points || explore || knobs || measure || predict)) return; // nothing to set: the car, its tags, and Train

        var card = Ui.Float(editor, "Card", 0, 0, 1080, 770);
        var t = card.transform;
        float y = 14;

        if (tracks)
        {
            var tr = Section(t, 16, y, 1048, 108, "THE TRACKS");
            var tl = Ui.Eyebrow(tr, "TRAINING ON", Theme.Muted); Ui.Place(tl.rectTransform, 0, 0, 200, 16);
            var vl = Ui.Eyebrow(tr, "TEST TRACK", Theme.Muted); Ui.Place(vl.rectTransform, 0, 44, 300, 16);
            for (int i = 0; i < Track.All.Length; i++)
            {
                var id = Track.All[i].id;
                bool on = s.training.Contains(id);
                Toggle(tr, i * 104, 16, 100, TrackShort[i], on, () =>
                {
                    var list = s.training.ToList();
                    if (on && list.Count > 1) list.Remove(id); else if (!on) list.Add(id);
                    s.training = list.ToArray(); Refresh();
                });
                bool test = s.test == id;
                Toggle(tr, i * 104, 60, 100, TrackShort[i], test, () => { s.test = id; Refresh(); });
            }
            y += 120;
        }
        if (sensors || moves)
        {
            if (sensors)
            {
                var se = Section(t, 16, y, moves ? 520 : 1048, 96, "WHAT IT SENSES");
                for (int i = 0; i < Sensors.Count; i++)
                {
                    int index = i;
                    Toggle(se, i * 100, 0, 96, Sensors.Names[i], s.sensors[i], () => { s.sensors[index] = !s.sensors[index]; app.world.SetSensors(s.sensors); Refresh(); });
                }
            }
            if (moves)
            {
                var mv = Section(t, sensors ? 548 : 16, y, sensors ? 516 : 1048, 96, "WHAT IT MAY DO");
                for (int i = 0; i < Moves.Count; i++)
                {
                    int index = i;
                    Toggle(mv, i * 86, 0, 82, Moves.Names[i], s.moves[i], () => { s.moves[index] = !s.moves[index]; if (s.MovesOn == 0) s.moves[index] = true; Refresh(); });
                }
                var tsl = Ui.Eyebrow(mv, "TOP SPEED", Theme.Muted); Ui.Place(tsl.rectTransform, 0, 40, 100, 16);
                Pick(mv, 100, 36, 160, new[] { "1", "2" }, s.topSpeed - 1, i => { s.topSpeed = i + 1; s.constraintSet = true; Refresh(); });
            }
            y += 108;
        }
        if (points)
        {
            var pt = Section(t, 16, y, 1048, 168, "THE POINTS TABLE");
            Row(pt, 0, 0, "The finish", new[] { "0", "50", "100", "200" }, new[] { 0f, 50, 100, 200 }, s.finish, v => s.finish = v);
            Row(pt, 0, 44, "A crash", new[] { "0", "-20", "-100", "-200" }, new[] { 0f, -20, -100, -200 }, s.crash, v => s.crash = v);
            Row(pt, 0, 88, "Each moment", new[] { "0", "-0.2", "-1", "-3" }, new[] { 0f, -0.2f, -1, -3 }, s.moment, v => s.moment = v);
            Row(pt, 528, 0, "Step toward", new[] { "0", "1", "2", "5" }, new[] { 0f, 1, 2, 5 }, s.toward, v => s.toward = v);
            Row(pt, 528, 44, "Step away", new[] { "0", "-1", "-2", "-5" }, new[] { 0f, -1, -2, -5 }, s.away, v => s.away = v);
            Row(pt, 528, 88, "Speed bonus", new[] { "0", "1", "3" }, new[] { 0f, 1, 3 }, s.speedBonus, v => s.speedBonus = v);
            y += 180;
        }
        if (explore || knobs)
        {
            if (explore)
            {
                var ex = Section(t, 16, y, knobs ? 520 : 1048, 150, "NEW MOVES");
                Row(ex, 0, 0, "At the start", new[] { "0.1", "0.5", "0.9" }, new[] { 0.1f, 0.5f, 0.9f }, s.explore, v => s.explore = v, 300);
                Row(ex, 0, 44, "How it falls", new[] { "never", "slowly", "fast", "at once" }, new[] { 1f, 0.995f, 0.98f, 0.9f }, s.exploreFall, v => { s.exploreFall = v; s.exploreOffAfterFinish = false; }, 300);
                Row(ex, 0, 88, "The floor", new[] { "0", "0.05", "0.5" }, new[] { 0f, 0.05f, 0.5f }, s.exploreMin, v => s.exploreMin = v, 300);
            }
            if (knobs)
            {
                var kn = Section(t, explore ? 548 : 16, y, explore ? 516 : 1048, 150, "THE KNOBS");
                bool all = free || l.knobs;
                if (all) Row(kn, 0, 0, "Rewrite speed", new[] { "0.02", "0.1", "0.3", "0.95" }, new[] { 0.02f, 0.1f, 0.3f, 0.95f }, s.rewrite, v => s.rewrite = v, 300);
                Row(kn, 0, 44, "Attempts", new[] { "50", "120", "250", "400", "1000" }, new[] { 50f, 120, 250, 400, 1000 }, s.attempts, v => s.attempts = (int)v, 300);
                if (all)
                {
                    var sl = Ui.Text(kn, "The seed", 13, Theme.Ink, true); Ui.Place(sl.rectTransform, 0, 92, 200, 20);
                    Pick(kn, 200, 88, 240, new[] { $"Keep {s.seed}", "New seed" }, 0, i => { if (i == 1) { s.seed = new System.Random().Next(1, 1000); Refresh(); } });
                }
            }
            y += 162;
        }
        if (measure)
        {
            var me = Section(t, 16, y, 520, 70, "THE MEASURE");
            Pick(me, 0, 0, 500, Measures.Names, s.measure, i => { s.measure = i; Refresh(); });
            if (!predict) y += 82;
        }
        if (predict)
        {
            float px = measure ? 564 : 16, pw = measure ? 500 : 1048;
            var pl = Ui.Eyebrow(t, "YOUR PREDICTION  ·  BEFORE YOU TRAIN", Theme.TealDeep);
            Ui.Place(pl.rectTransform, px + 12, y, pw, 16);
            prediction = Ui.Input(t, l.predictionPrompt, false, 14);
            Ui.Place(prediction.GetComponent<RectTransform>(), px + 12, y + 20, pw - 24, 44);
            prediction.text = app.session.prediction ?? "";
            y += 82;
        }
        card.rectTransform.sizeDelta = new Vector2(1080, Mathf.Max(120, y + 6));
    }

    // A setting moved: the car wears it and pops, the tags and the track line follow.
    void Refresh()
    {
        app.world.Punch();
        BuildEditor();
        BuildTags();
        if (note != null) { note.text = $"{s.attempts} attempts  ·  {(app.lesson != null ? app.lesson.laps : 5)} laps on the test track"; note.color = Theme.Text2; }
    }

    Transform Section(Transform t, float x, float y, float w, float h, string title)
    {
        var card = Ui.Card(t, title, Theme.Panel, Theme.Border1);
        Ui.Place(card.rectTransform, x, y, w, h);
        var l = Ui.Eyebrow(card.transform, title, Theme.TealDeep);
        l.font = Theme.Bold;
        Ui.Place(l.rectTransform, 12, 8, w - 100, 16);
        var content = Ui.Node("Content", card.transform);
        Ui.Place(content, 12, 30, w - 24, h - 36);
        return content;
    }

    void Toggle(Transform t, float x, float y, float w, string label, bool on, System.Action click)
    {
        var b = Ui.Button(t, label, () => { Sfx.Tick(); click(); }, Ui.ButtonStyle.Secondary, 12);
        Ui.Place(b.GetComponent<RectTransform>(), x, y, w, 30);
        b.GetComponent<Image>().color = on ? Theme.BorderTeal : Theme.Border;
        b.transform.Find("Fill").GetComponent<Image>().color = on ? Theme.Surface2 : Theme.Panel;
        b.GetComponentInChildren<TextMeshProUGUI>().color = on ? Theme.TealDeep : Theme.Muted;
    }

    void Pick(Transform t, float x, float y, float w, string[] options, int selected, System.Action<int> onSelect)
    {
        var seg = new Ui.Segmented(t, options, Mathf.Clamp(selected, 0, options.Length - 1), w, 30, 12);
        Ui.Place(seg.root, x, y, w, 30);
        seg.OnSelect = onSelect;
    }

    void Row(Transform t, float x, float y, string label, string[] options, float[] values, float current, System.Action<float> set, float w = 380)
    {
        var l = Ui.Text(t, label, 13, Theme.Ink, true);
        Ui.Place(l.rectTransform, x, y + 6, 130, 20);
        int selected = 0;
        for (int i = 0; i < values.Length; i++) if (Mathf.Abs(values[i] - current) < Mathf.Abs(values[selected] - current)) selected = i;
        Pick(t, x + 130, y, w, options, selected, i => { set(values[i]); Refresh(); });
    }
}
