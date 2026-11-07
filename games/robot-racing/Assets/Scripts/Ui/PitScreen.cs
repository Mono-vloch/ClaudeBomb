using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The pit lane: where the robot is set up before training. The car stands on the
// grid, big, filling the screen's world, its sensors drawn as rays, and its seven settings hang off
// it as tags: what it senses at the nose, what it may do at the wheels, the points
// on the roof, the knobs at the back, the tracks on the ground; a setting the level
// has not opened wears OPENS AT instead. In a dark drawer on the right (a third of the
// screen; it folds away), the level and the track, then only what this level opens, as controls (the points table on 2.3, the sensors on 2.1, everything on the
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
        Ui.Place(frame, 0, 0, Ui.W - DrawerW, Ui.H);
        BuildWorld();

        // The drawer: the level, the track, the way out (train), then what this level opens.
        drawer = Ui.Node("Drawer", root);
        Ui.Place(drawer, Ui.W - DrawerW, 0, DrawerW, Ui.H);
        drawerBg = Ui.Box(drawer, "Glass", Ui.DarkFill, false);
        Ui.Stretch(drawerBg.rectTransform);
        drawerKey = Ui.Box(drawer, "Keyline", Theme.Teal, false);
        Ui.Place(drawerKey.rectTransform, 0, 0, 3, Ui.H);
        var eyebrow = Ui.Eyebrow(drawer, l != null ? $"LEVEL {l.id}  ·  THE PIT LANE" : "FREE PLAY  ·  THE PIT LANE", Theme.Teal);
        eyebrow.font = Theme.Bold;
        Ui.Place(eyebrow.rectTransform, 28, 26, 460, 22);
        var fold = Ui.Button(drawer, "Hide", () => Fold(false), Ui.ButtonStyle.GhostOnDark, 16);
        Ui.Place(fold.GetComponent<RectTransform>(), DrawerW - 112, 18, 88, 40);
        var title = Ui.Text(drawer, l != null ? l.title : "Set up the robot", 30, Color.white, true);
        Ui.Place(title.rectTransform, 28, 60, DW, 40);
        title.enableWordWrapping = false; title.overflowMode = TextOverflowModes.Ellipsis;

        // The track: its name, its line, its map.
        var t0 = Track.Get(s.training[0]);
        var tag = Ui.Eyebrow(drawer, $"THE TRACK  ·  {t0.name.ToUpperInvariant()}" + (s.training.Length > 1 ? $"  ·  +{s.training.Length - 1}" : "") + (s.test != s.training[0] ? $"  ·  TEST: {Track.Get(s.test).name.ToUpperInvariant()}" : ""), Theme.Teal);
        Ui.Place(tag.rectTransform, 28, 116, DW - 140, 22);
        tag.enableWordWrapping = false; tag.overflowMode = TextOverflowModes.Ellipsis;
        var tnote = Ui.Text(drawer, t0.note, 18, Color.white);
        Ui.Place(tnote.rectTransform, 28, 142, DW - 150, 50);
        note = Ui.Text(drawer, "", 16, Theme.Faint);
        Ui.Place(note.rectTransform, 28, 196, DW - 150, 24);
        LessonScreen.Map(drawer, t0, 28 + DW - 130, 116, 130, 100);

        float bx = 28;
        if (l != null)
        {
            var back = Ui.Button(drawer, "Briefing", app.ShowLesson, Ui.ButtonStyle.GhostOnDark, 16);
            Ui.Place(back.GetComponent<RectTransform>(), bx, 236, 120, 52); bx += 128;
        }
        var file = Ui.Button(drawer, "The file", () => app.ShowFile(), Ui.ButtonStyle.GhostOnDark, 16);
        Ui.Place(file.GetComponent<RectTransform>(), bx, 236, 116, 52); bx += 124;
        var settings = Ui.Button(drawer, "Settings", () => SettingsSheet.Toggle(app, root, null), Ui.ButtonStyle.GhostOnDark, 16);
        Ui.Place(settings.GetComponent<RectTransform>(), bx, 236, 120, 52); bx += 128;
        var train = Ui.Button(drawer, "TRAIN IT", Train, Ui.ButtonStyle.Accent, 18);
        Ui.Place(train.GetComponent<RectTransform>(), bx, 236, 28 + DW - bx, 52);
        var rule = Ui.Box(drawer, "Rule", Ui.Hairline, false);
        Ui.Place(rule.rectTransform, 28, 306, DW, 1);

        tags = Ui.Node("Tags", root);
        Ui.Stretch(tags);
        var scroll = Ui.Scroll(drawer, "Scroll", out editor);
        Ui.Place(scroll.GetComponent<RectTransform>(), 20, 320, DrawerW - 40, Ui.H - 340);

        // The folded drawer: a slim tab and a way back.
        tab = Ui.Node("Tab", root);
        Ui.Place(tab, Ui.W - TabW, 0, TabW, Ui.H);
        var tabBg = Ui.Box(tab, "Glass", Ui.DarkFill, false);
        Ui.Stretch(tabBg.rectTransform);
        var unfold = Ui.Button(tab, "Open", () => Fold(true), Ui.ButtonStyle.GhostOnDark, 16);
        Ui.Place(unfold.GetComponent<RectTransform>(), 6, 18, TabW - 12, 44);
        tab.gameObject.SetActive(false);

        BuildEditor();
        BuildTags();
        Coach();
        Refresh(false);
        var follower = root.gameObject.AddComponent<Follower>();
        follower.Bind(this);
    }

    const float DrawerW = 640f, TabW = 72f, DW = DrawerW - 56; // a third of the screen at most: the world keeps the rest
    readonly RectTransform drawer, tab;
    Image drawerBg, drawerKey;
    bool open = true;
    float WorldW => Ui.W - (open ? DrawerW : TabW);

    void Fold(bool show)
    {
        open = show;
        drawer.gameObject.SetActive(show);
        tab.gameObject.SetActive(!show);
        Ui.Place(frame, 0, 0, WorldW, Ui.H);
        app.world.ShowPortrait(frame);
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
        coachLine = app.luma.Line(root, 40, 960, 900, (broken ? "Fix it: " + start.name + ". " : "") + start.why, true, false, 18);
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
    const float TagW = 290f;
    readonly List<Hook> hooks = new List<Hook>();

    bool Open(bool flag) => app.lesson == null || flag;

    // Seven tags in two columns beside the car, each with a line down to the part of the
    // car it is: the near parts on the left, the far ones on the right.
    void BuildTags()
    {
        Ui.Clear(tags);
        hooks.Clear();
        var l = app.lesson;
        // The parts of the car each tag hangs from: the kit's car (its right side faces the camera), or the old one.
        bool k = Circuit.HasKit; const float sd = 1f;
        Vector3 P(Vector3 kitPoint, Vector3 oldPoint) => k ? kitPoint : oldPoint;
        Tag(P(new Vector3(0f, 0.25f, 0.28f), new Vector3(0f, 0.3f, 0.8f)), 0, 0, "SENSES", string.Join(", ", Enumerable.Range(0, Sensors.Count).Where(i => s.sensors[i]).Select(i => Sensors.Names[i])), Open(l.sensors), "2.1");
        Tag(P(new Vector3(sd * 0.16f, 0.07f, 0.19f), new Vector3(-0.5f, 0.12f, 0.45f)), 0, 1, "MOVES", $"{s.MovesOn} of 5  ·  top speed {s.topSpeed}", Open(l.moves), "2.2");
        Tag(P(new Vector3(sd * 0.5f, 0.01f, 0.05f), new Vector3(-0.9f, 0.02f, 0f)), 0, 2, "TRACKS", Track.Get(s.training[0]).name + (s.test != s.training[0] ? "  ·  test " + Track.Get(s.test).name.ToLowerInvariant() : ""), Open(l.tracks), "3.1");
        Tag(P(new Vector3(sd * 0.5f, 0.01f, -0.45f), new Vector3(-0.9f, 0.02f, -0.8f)), 0, 3, "MEASURE", Measures.Names[s.measure], Open(l.measure), "3.2");
        Tag(P(new Vector3(0f, 0.37f, -0.24f), new Vector3(0f, 0.85f, 0f)), 1, 0, "POINTS", $"finish {s.finish:0}  ·  crash {s.crash:0}  ·  toward {s.toward:0.#}", Open(l.points), "2.3");
        Tag(P(new Vector3(0f, 0.2f, -0.38f), new Vector3(0f, 0.4f, -0.8f)), 1, 1, "KNOBS", $"rewrite {s.rewrite:0.00}  ·  {s.attempts} attempts", Open(l.knobs || l.attempts), l.attempts && !l.knobs ? "3.3 (attempts open)" : "3.3");
        Tag(P(new Vector3(sd * 0.16f, 0.07f, -0.27f), new Vector3(-0.5f, 0.12f, -0.45f)), 1, 2, "NEW MOVES", $"{s.explore:0.0}, falls {Settings.Fall(s.exploreFall)}", Open(l.explore), "2.5");
        PlaceTags();
    }

    void Tag(Vector3 local, int column, int row, string name, string value, bool open, string opensAt)
    {
        var line = Ui.Box(tags, "Lead", open ? new Color(Theme.Teal.r, Theme.Teal.g, Theme.Teal.b, 0.7f) : new Color(1f, 1f, 1f, 0.3f), false);
        line.raycastTarget = false;
        var card = Ui.Glass(tags, name, 0, 0, TagW, 64, true, open ? Theme.Teal : new Color(1f, 1f, 1f, 0.25f));
        var n = Ui.Eyebrow(card.transform, open ? name : name + "  ·  OPENS AT " + opensAt.ToUpperInvariant(), open ? Theme.Teal : Theme.Faint);
        Ui.Place(n.rectTransform, 16, 8, TagW - 24, 22);
        n.enableWordWrapping = false; n.overflowMode = TextOverflowModes.Ellipsis;
        var v = Ui.Text(card.transform, value, 17, open ? Color.white : Theme.Faint, true);
        Ui.Place(v.rectTransform, 16, 32, TagW - 24, 26);
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
            float tx = h.column == 0 ? 40 : WorldW - 40 - TagW, ty = 40 + h.row * 80;
            Ui.Place(h.rect, tx, ty, TagW, 64);
            bool on = app.world.CarPartOnScreen(h.local, out var sp);
            h.dot.gameObject.SetActive(on); h.lead.gameObject.SetActive(on);
            if (!on) continue;
            var p = app.StagePoint(sp);
            Ui.Place(h.dot.rectTransform, p.x - 6, p.y - 6, 12, 12);
            // The line, from the tag's inner edge to the dot.
            float x0 = h.column == 0 ? tx + TagW : tx, y0 = ty + 32;
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
        bool any = tracks || sensors || moves || points || explore || knobs || measure || predict;
        // Nothing to set: the drawer ends under its buttons and the world shows below it.
        float drawerH = any ? Ui.H : 310f;
        Ui.Place(drawerBg.rectTransform, 0, 0, DrawerW, drawerH);
        Ui.Place(drawerKey.rectTransform, 0, 0, 3, drawerH);
        if (!any) return; // the car, its tags, and Train

        var t = editor;
        const float W = DrawerW - 40;
        float y = 4;

        if (tracks)
        {
            var tr = Section(t, 0, y, W, 184, "THE TRACKS");
            var tl = Ui.Eyebrow(tr, "TRAINING ON", Theme.Faint); Ui.Place(tl.rectTransform, 0, 0, 300, 22);
            var vl = Ui.Eyebrow(tr, "TEST TRACK", Theme.Faint); Ui.Place(vl.rectTransform, 0, 92, 300, 22);
            float tw = (W - 24 - 4 * 6) / 5f;
            for (int i = 0; i < Track.All.Length; i++)
            {
                var id = Track.All[i].id;
                bool on = s.training.Contains(id);
                float x = (i % 5) * (tw + 6), row = (i / 5) * 34;
                Toggle(tr, x, 24 + row, tw, TrackShort[i], on, () =>
                {
                    var list = s.training.ToList();
                    if (on && list.Count > 1) list.Remove(id); else if (!on) list.Add(id);
                    s.training = list.ToArray(); Refresh();
                });
                bool test = s.test == id;
                Toggle(tr, x, 116 + row, tw, TrackShort[i], test, () => { s.test = id; Refresh(); });
            }
            y += 184 + 60;
        }
        if (sensors)
        {
            var se = Section(t, 0, y, W, 34, "WHAT IT SENSES");
            float sw = (W - 24 - 4 * 6) / 5f;
            for (int i = 0; i < Sensors.Count; i++)
            {
                int index = i;
                Toggle(se, i * (sw + 6), 0, sw, Sensors.Names[i], s.sensors[i], () => { s.sensors[index] = !s.sensors[index]; app.world.SetSensors(s.sensors); Refresh(); });
            }
            y += 34 + 60;
        }
        if (moves)
        {
            var mv = Section(t, 0, y, W, 88, "WHAT IT MAY DO");
            float mw = (W - 24 - 4 * 6) / 5f;
            for (int i = 0; i < Moves.Count; i++)
            {
                int index = i;
                Toggle(mv, i * (mw + 6), 0, mw, Moves.Names[i], s.moves[i], () => { s.moves[index] = !s.moves[index]; if (s.MovesOn == 0) s.moves[index] = true; Refresh(); });
            }
            var tsl = Ui.Eyebrow(mv, "TOP SPEED", Theme.Faint); Ui.Place(tsl.rectTransform, 0, 52, 160, 22);
            Pick(mv, 160, 46, 220, new[] { "1", "2" }, s.topSpeed - 1, i => { s.topSpeed = i + 1; s.constraintSet = true; Refresh(); });
            y += 88 + 60;
        }
        if (points)
        {
            var pt = Section(t, 0, y, W, 6 * 46 - 6, "THE POINTS TABLE");
            Row(pt, 0, 0, "The finish", new[] { "0", "50", "100", "200" }, new[] { 0f, 50, 100, 200 }, s.finish, v => s.finish = v);
            Row(pt, 0, 46, "A crash", new[] { "0", "-20", "-100", "-200" }, new[] { 0f, -20, -100, -200 }, s.crash, v => s.crash = v);
            Row(pt, 0, 92, "Each moment", new[] { "0", "-0.2", "-1", "-3" }, new[] { 0f, -0.2f, -1, -3 }, s.moment, v => s.moment = v);
            Row(pt, 0, 138, "Step toward", new[] { "0", "1", "2", "5" }, new[] { 0f, 1, 2, 5 }, s.toward, v => s.toward = v);
            Row(pt, 0, 184, "Step away", new[] { "0", "-1", "-2", "-5" }, new[] { 0f, -1, -2, -5 }, s.away, v => s.away = v);
            Row(pt, 0, 230, "Speed bonus", new[] { "0", "1", "3" }, new[] { 0f, 1, 3 }, s.speedBonus, v => s.speedBonus = v);
            y += 6 * 46 - 6 + 60;
        }
        if (explore)
        {
            var ex = Section(t, 0, y, W, 3 * 46 - 6, "NEW MOVES");
            Row(ex, 0, 0, "At the start", new[] { "0.1", "0.5", "0.9" }, new[] { 0.1f, 0.5f, 0.9f }, s.explore, v => s.explore = v);
            Row(ex, 0, 46, "How it falls", new[] { "never", "slowly", "fast", "at once" }, new[] { 1f, 0.995f, 0.98f, 0.9f }, s.exploreFall, v => { s.exploreFall = v; s.exploreOffAfterFinish = false; });
            Row(ex, 0, 92, "The floor", new[] { "0", "0.05", "0.5" }, new[] { 0f, 0.05f, 0.5f }, s.exploreMin, v => s.exploreMin = v);
            y += 3 * 46 - 6 + 60;
        }
        if (knobs)
        {
            bool all = free || l.knobs;
            int rows = all ? 3 : 1;
            var kn = Section(t, 0, y, W, rows * 46 - 6, "THE KNOBS");
            float ky = 0;
            if (all) { Row(kn, 0, ky, "Rewrite speed", new[] { "0.02", "0.1", "0.3", "0.95" }, new[] { 0.02f, 0.1f, 0.3f, 0.95f }, s.rewrite, v => s.rewrite = v); ky += 46; }
            Row(kn, 0, ky, "Attempts", new[] { "50", "120", "250", "400", "1000" }, new[] { 50f, 120, 250, 400, 1000 }, s.attempts, v => s.attempts = (int)v); ky += 46;
            if (all)
            {
                var sl = Ui.Text(kn, "The seed", 16, Color.white, true); Ui.Place(sl.rectTransform, 0, ky + 8, 150, 24);
                Pick(kn, 150, ky, W - 24 - 150, new[] { $"Keep {s.seed}", "New seed" }, 0, i => { if (i == 1) { s.seed = new System.Random().Next(1, 1000); Refresh(); } });
            }
            y += rows * 46 - 6 + 60;
        }
        if (measure)
        {
            var me = Section(t, 0, y, W, 40, "THE MEASURE");
            Pick(me, 0, 0, W - 24, Measures.Names, s.measure, i => { s.measure = i; Refresh(); });
            y += 40 + 60;
        }
        if (predict)
        {
            var pl = Ui.Eyebrow(t, "YOUR PREDICTION  ·  BEFORE YOU TRAIN", Theme.Teal);
            Ui.Place(pl.rectTransform, 8, y, W, 22);
            prediction = Ui.DarkInput(t, l.predictionPrompt, true, 16);
            Ui.Place(prediction.GetComponent<RectTransform>(), 0, y + 28, W, 84);
            prediction.text = app.session.prediction ?? "";
            y += 124;
        }
        editor.sizeDelta = new Vector2(0, y + 10);
    }

    // A setting moved: the car wears it and pops, the tags and the track line follow.
    void Refresh() => Refresh(true);
    void Refresh(bool punch)
    {
        if (punch) { app.world.Punch(); BuildEditor(); BuildTags(); }
        if (note != null) { note.text = $"{s.attempts} attempts  ·  {(app.lesson != null ? app.lesson.laps : 5)} laps on the test track"; note.color = Theme.Faint; }
    }

    Transform Section(Transform t, float x, float y, float w, float h, string title)
    {
        var card = Ui.DarkCard(t, title);
        Ui.Place(card.rectTransform, x, y, w, h + 40 + 8);
        var l = Ui.Eyebrow(card.transform, title, Theme.Teal);
        l.font = Theme.Bold;
        Ui.Place(l.rectTransform, 12, 10, w - 24, 22);
        var content = Ui.Node("Content", card.transform);
        Ui.Place(content, 12, 40, w - 24, h - 4);
        return content;
    }

    void Toggle(Transform t, float x, float y, float w, string label, bool on, System.Action click)
    {
        var b = Ui.Button(t, label, () => { Sfx.Tick(); click(); }, Ui.ButtonStyle.GhostOnDark, 16);
        Ui.Place(b.GetComponent<RectTransform>(), x, y, w, 30);
        Ui.Chip(b, on);
        var text = b.GetComponentInChildren<TextMeshProUGUI>();
        text.enableAutoSizing = true; text.fontSizeMin = 16; text.fontSizeMax = 16;
        Ui.Stretch(text.rectTransform, 4, 0, 4, 0);
    }

    void Pick(Transform t, float x, float y, float w, string[] options, int selected, System.Action<int> onSelect)
    {
        var seg = new Ui.Segmented(t, options, Mathf.Clamp(selected, 0, options.Length - 1), w, 38, 16, dark: true);
        Ui.Place(seg.root, x, y, w, 38);
        seg.OnSelect = onSelect;
    }

    void Row(Transform t, float x, float y, string label, string[] options, float[] values, float current, System.Action<float> set)
    {
        var l = Ui.Text(t, label, 16, Color.white, true);
        Ui.Place(l.rectTransform, x, y + 8, 150, 24);
        int selected = 0;
        for (int i = 0; i < values.Length; i++) if (Mathf.Abs(values[i] - current) < Mathf.Abs(values[selected] - current)) selected = i;
        Pick(t, x + 150, y, DrawerW - 40 - 24 - 150, options, selected, i => { set(values[i]); Refresh(); });
    }
}
