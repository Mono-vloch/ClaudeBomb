using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The pit lane: where the robot is set up before training. The car stands on the
// grid on the left, its sensors drawn as rays; the settings are on the right as
// upgrade cards, locked until their level: the tracks, the sensors, the moves
// and the constraint, the points table, how much it tries new things, the knobs,
// the measure. A level opens on its training runs (Watch the pro, Fix the broken
// bot) before Your run.
public class PitScreen
{
    public readonly RectTransform root;
    readonly App app;
    readonly Settings s;
    readonly RectTransform frame, editor;
    readonly TextMeshProUGUI caption;
    RectTransform coachLine;
    TMP_InputField prediction;

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
        Ui.Place(frame, 60, 300, 680, 580);
        BuildWorld();

        var top = Ui.Box(root, "Top", new Color(1, 1, 1, 0.86f), false);
        Ui.Place(top.rectTransform, -400, -300, 2800, 460);
        top.raycastTarget = false;
        var eyebrow = Ui.Eyebrow(root, l != null ? $"LEVEL {l.id}  ·  THE PIT LANE" : "FREE PLAY  ·  THE PIT LANE", Theme.TealDeep);
        Ui.Place(eyebrow.rectTransform, 60, 24, 600, 20);
        var title = Ui.Text(root, l != null ? l.title : "Set up the robot", 30, Theme.Ink, true);
        Ui.Place(title.rectTransform, 60, 46, 1000, 40);
        title.enableWordWrapping = false;
        if (l != null)
        {
            var back = Ui.Button(root, "Briefing", app.ShowLesson, Ui.ButtonStyle.Quiet, 15);
            Ui.Place(back.GetComponent<RectTransform>(), 1080, 44, 100, 42);
        }
        var file = Ui.Button(root, "The file", () => app.ShowFile(), Ui.ButtonStyle.Quiet, 15);
        Ui.Place(file.GetComponent<RectTransform>(), 1184, 44, 90, 42);
        var train = Ui.Button(root, "TRAIN THE ROBOT", Train, Ui.ButtonStyle.Accent, 20);
        Ui.Place(train.GetComponent<RectTransform>(), 1600, 32, 260, 64);

        if (l != null && (l.ready != null || l.broken != null))
        {
            var labels = new System.Collections.Generic.List<string>();
            var steps = new System.Collections.Generic.List<App.Step>();
            if (l.ready != null) { labels.Add("Watch the pro"); steps.Add(App.Step.SeeItWork); }
            if (l.broken != null) { labels.Add("Fix the broken bot"); steps.Add(App.Step.FixIt); }
            labels.Add("Your run"); steps.Add(App.Step.YourTurn);
            var stepsLabel = Ui.Eyebrow(root, "THE RUNS", Theme.Muted);
            Ui.Place(stepsLabel.rectTransform, 60, 104, 100, 20);
            var seg = new Ui.Segmented(root, labels.ToArray(), steps.IndexOf(app.step), 560, 40, 14);
            Ui.Place(seg.root, 150, 100, 560, 40);
            seg.OnSelect = i => { app.SetStep(steps[i]); app.ShowPit(); };
        }

        // The track
        var card = Ui.Soft(root, "Track");
        Ui.Place(card.rectTransform, 60, 176, 1800, 108);
        var t0 = Track.Get(s.training[0]);
        var tag = Ui.Eyebrow(card.transform, $"THE TRACK  ·  {t0.name.ToUpperInvariant()}" + (s.training.Length > 1 ? $"  ·  TRAINING ON {s.training.Length} TRACKS" : "") + (s.test != s.training[0] ? $"  ·  TEST TRACK: {Track.Get(s.test).name.ToUpperInvariant()}" : ""), Theme.TealDeep);
        Ui.Place(tag.rectTransform, 24, 12, 1200, 22);
        var note = Ui.Text(card.transform, t0.note, 20, Theme.Ink);
        Ui.Place(note.rectTransform, 24, 38, 1200, 36);
        var budget = Ui.Text(card.transform, $"{s.attempts} attempts  ·  {(l != null ? l.laps : 5)} laps on the test track", 14, Theme.Text2);
        Ui.Place(budget.rectTransform, 24, 74, 1200, 24);
        LessonScreen.Map(card.transform, t0, 1300, 10, 480, 88);

        var captionCard = Ui.Box(root, "Caption", new Color(1, 1, 1, 0.86f));
        Ui.Place(captionCard.rectTransform, 60, 884, 680, 80);
        captionCard.raycastTarget = false;
        caption = Ui.Text(captionCard.transform, "", 13, Theme.Text2, false, TextAlignmentOptions.MidlineLeft);
        Ui.Stretch(caption.rectTransform, 14, 0, 14, 0);

        editor = Ui.Node("Editor", root);
        Ui.Place(editor, 780, 300, 1080, 760);
        BuildEditor();
        Caption();
        Coach();
        Ui.Emblem(root);
    }

    // On a training run LUMA says why the ready setup learns, or what the broken one has
    // in it, once, over the car; the words stay in the file.
    void Coach()
    {
        if (coachLine != null) Object.Destroy(coachLine.gameObject);
        coachLine = null;
        var setup = app.CurrentSetup;
        if (setup == null) return;
        coachLine = app.luma.Line(root, 60, 300, 680, (app.step == App.Step.FixIt ? "Fix it: " + setup.name + ". " : "") + setup.why, true, false, 15);
    }

    void BuildWorld()
    {
        app.world.Build(Track.Get(s.training[0]), false);
        app.world.ShowPortrait(frame);
        app.world.SetSensors(s.sensors);
    }

    void Train()
    {
        if (app.lesson != null && app.lesson.prediction && app.step == App.Step.YourTurn && (prediction == null || prediction.text.Trim().Length < 8))
        {
            caption.text = "Write your prediction first.";
            caption.color = Theme.Danger;
            return;
        }
        if (prediction != null) app.session.prediction = prediction.text;
        app.StartTraining();
    }

    void Caption()
    {
        var setup = app.CurrentSetup;
        caption.color = Theme.Text2;
        if (setup != null)
        {
            caption.text = app.step == App.Step.FixIt ? "Fix it: " + setup.name : "Watch the pro: a ready setup";
            return;
        }
        caption.text = $"Senses: {string.Join(", ", Enumerable.Range(0, Sensors.Count).Where(i => s.sensors[i]).Select(i => Sensors.Names[i].ToLowerInvariant()))}  ·  " +
                       $"Moves: {s.MovesOn} of 5, top speed {s.topSpeed}  ·  Finish {s.finish:0}, crash {s.crash:0}, toward {s.toward:0.#}, away {s.away:0.#}, moment {s.moment:0.##}, speed bonus {s.speedBonus:0.#}  ·  " +
                       $"New moves {s.explore:0.0} falling {Settings.Fall(s.exploreFall)} to {s.exploreMin:0.00}  ·  rewrite {s.rewrite:0.00}  ·  {s.attempts} attempts, seed {s.seed}";
    }

    // ---- the settings ------------------------------------------------------------------

    void BuildEditor()
    {
        Ui.Clear(editor);
        var l = app.lesson;
        bool free = l == null;
        var card = Ui.Float(editor, "Card", 0, 0, 1080, 760);
        var t = card.transform;
        bool locked = app.Training; // a ready setup is read, not written

        // Tracks
        var tracks = Section(t, 16, 14, 1048, 108, "THE TRACKS", (free || l.tracks) && !locked, "3.1",
            $"Training on {string.Join(", ", s.training.Select(id => Track.Get(id).name.ToLowerInvariant()))}. Test track: {Track.Get(s.test).name.ToLowerInvariant()}.");
        if (tracks != null)
        {
            var tl = Ui.Eyebrow(tracks, "TRAINING ON", Theme.Muted); Ui.Place(tl.rectTransform, 0, 0, 200, 16);
            var vl = Ui.Eyebrow(tracks, "TEST TRACK", Theme.Muted); Ui.Place(vl.rectTransform, 0, 44, 300, 16);
            for (int i = 0; i < Track.All.Length; i++)
            {
                var id = Track.All[i].id;
                bool on = s.training.Contains(id);
                Toggle(tracks, i * 104, 16, 100, TrackShort[i], on, () =>
                {
                    var list = s.training.ToList();
                    if (on && list.Count > 1) list.Remove(id); else if (!on) list.Add(id);
                    s.training = list.ToArray(); Refresh();
                });
                bool test = s.test == id;
                Toggle(tracks, i * 104, 60, 100, TrackShort[i], test, () => { s.test = id; Refresh(); });
            }
        }

        // Sensors and moves
        var sensors = Section(t, 16, 134, 520, 96, "WHAT IT SENSES", (free || l.sensors) && !locked, "2.1",
            string.Join(", ", Enumerable.Range(0, Sensors.Count).Where(i => s.sensors[i]).Select(i => Sensors.Names[i])));
        if (sensors != null)
            for (int i = 0; i < Sensors.Count; i++)
            {
                int index = i;
                Toggle(sensors, i * 100, 0, 96, Sensors.Names[i], s.sensors[i], () => { s.sensors[index] = !s.sensors[index]; app.world.SetSensors(s.sensors); Refresh(); });
            }
        var moves = Section(t, 548, 134, 516, 96, "WHAT IT MAY DO", (free || l.moves) && !locked, "2.2",
            string.Join(", ", Enumerable.Range(0, Moves.Count).Where(i => s.moves[i]).Select(i => Moves.Names[i])) + $". Top speed {s.topSpeed}.");
        if (moves != null)
        {
            for (int i = 0; i < Moves.Count; i++)
            {
                int index = i;
                Toggle(moves, i * 86, 0, 82, Moves.Names[i], s.moves[i], () => { s.moves[index] = !s.moves[index]; if (s.MovesOn == 0) s.moves[index] = true; Refresh(); });
            }
            var tsl = Ui.Eyebrow(moves, "TOP SPEED", Theme.Muted); Ui.Place(tsl.rectTransform, 0, 40, 100, 16);
            Pick(moves, 100, 36, 160, new[] { "1", "2" }, s.topSpeed - 1, i => { s.topSpeed = i + 1; s.constraintSet = true; Refresh(); });
        }

        // The points table
        var points = Section(t, 16, 244, 1048, 168, "THE POINTS TABLE", (free || l.points) && !locked, "2.3",
            $"Finish {s.finish:0}, crash {s.crash:0}, each step toward the finish {s.toward:0.#}, each step away {s.away:0.#}, each moment {s.moment:0.##}, speed bonus {s.speedBonus:0.#}.");
        if (points != null)
        {
            Row(points, 0, 0, "The finish", new[] { "0", "50", "100", "200" }, new[] { 0f, 50, 100, 200 }, s.finish, v => s.finish = v);
            Row(points, 0, 44, "A crash", new[] { "0", "-20", "-100", "-200" }, new[] { 0f, -20, -100, -200 }, s.crash, v => s.crash = v);
            Row(points, 0, 88, "Each moment", new[] { "0", "-0.2", "-1", "-3" }, new[] { 0f, -0.2f, -1, -3 }, s.moment, v => s.moment = v);
            Row(points, 528, 0, "Step toward", new[] { "0", "1", "2", "5" }, new[] { 0f, 1, 2, 5 }, s.toward, v => s.toward = v);
            Row(points, 528, 44, "Step away", new[] { "0", "-1", "-2", "-5" }, new[] { 0f, -1, -2, -5 }, s.away, v => s.away = v);
            Row(points, 528, 88, "Speed bonus", new[] { "0", "1", "3" }, new[] { 0f, 1, 3 }, s.speedBonus, v => s.speedBonus = v);
        }

        // New moves, and the knobs
        var explore = Section(t, 16, 426, 520, 150, "NEW MOVES", (free || l.explore) && !locked, "2.5",
            $"Starts at {s.explore:0.0}, falls {Settings.Fall(s.exploreFall)}, floor {s.exploreMin:0.00}.");
        if (explore != null)
        {
            Row(explore, 0, 0, "At the start", new[] { "0.1", "0.5", "0.9" }, new[] { 0.1f, 0.5f, 0.9f }, s.explore, v => s.explore = v, 300);
            Row(explore, 0, 44, "How it falls", new[] { "never", "slowly", "fast", "at once" }, new[] { 1f, 0.995f, 0.98f, 0.9f }, s.exploreFall, v => { s.exploreFall = v; s.exploreOffAfterFinish = false; }, 300);
            Row(explore, 0, 88, "The floor", new[] { "0", "0.05", "0.5" }, new[] { 0f, 0.05f, 0.5f }, s.exploreMin, v => s.exploreMin = v, 300);
        }
        var knobs = Section(t, 548, 426, 516, 150, "THE KNOBS", (free || l.knobs || l.attempts) && !locked, "3.3",
            $"Rewrite speed {s.rewrite:0.00}, {s.attempts} attempts, seed {s.seed}.");
        if (knobs != null)
        {
            bool all = free || l.knobs;
            if (all) Row(knobs, 0, 0, "Rewrite speed", new[] { "0.02", "0.1", "0.3", "0.95" }, new[] { 0.02f, 0.1f, 0.3f, 0.95f }, s.rewrite, v => s.rewrite = v, 300);
            else { var rl = Ui.Text(knobs, $"Rewrite speed {s.rewrite:0.00}  ·  opens at 3.3", 13, Theme.Muted); Ui.Place(rl.rectTransform, 0, 8, 400, 20); }
            Row(knobs, 0, 44, "Attempts", new[] { "50", "120", "250", "400", "1000" }, new[] { 50f, 120, 250, 400, 1000 }, s.attempts, v => s.attempts = (int)v, 300);
            if (all)
            {
                var sl = Ui.Text(knobs, "The seed", 13, Theme.Ink, true); Ui.Place(sl.rectTransform, 0, 92, 200, 20);
                Pick(knobs, 200, 88, 240, new[] { $"Keep {s.seed}", "New seed" }, 0, i => { if (i == 1) { s.seed = new System.Random().Next(1, 1000); Refresh(); } });
            }
        }

        // The measure, and the prediction
        var measure = Section(t, 16, 590, 520, 70, "THE MEASURE", (free || l.measure) && !locked, "3.2", Measures.Names[s.measure] + ".");
        if (measure != null) Pick(measure, 0, 0, 500, Measures.Names, s.measure, i => { s.measure = i; Refresh(); });
        if (l != null && l.prediction && app.step == App.Step.YourTurn)
        {
            var pl = Ui.Eyebrow(t, "YOUR PREDICTION  ·  BEFORE YOU TRAIN", Theme.TealDeep);
            Ui.Place(pl.rectTransform, 564, 590, 500, 16);
            prediction = Ui.Input(t, l.predictionPrompt, false, 14);
            Ui.Place(prediction.GetComponent<RectTransform>(), 564, 610, 500, 44);
            prediction.text = app.session.prediction ?? "";
        }
    }

    void Refresh()
    {
        BuildEditor();
        Caption();
    }

    Transform Section(Transform t, float x, float y, float w, float h, string title, bool open, string opensAt, string fixedText)
    {
        var card = Ui.Card(t, title, open ? Theme.Panel : Theme.Surface1, open ? Theme.Border1 : Theme.Border);
        Ui.Place(card.rectTransform, x, y, w, h);
        var l = Ui.Eyebrow(card.transform, title, open ? Theme.TealDeep : Theme.Muted);
        l.font = Theme.Bold;
        Ui.Place(l.rectTransform, 12, 8, w - 100, 16);
        if (open)
        {
            var content = Ui.Node("Content", card.transform);
            Ui.Place(content, 12, 30, w - 24, h - 36);
            return content;
        }
        var tag = Ui.Eyebrow(card.transform, "OPENS AT " + opensAt.ToUpperInvariant(), Theme.Muted);
        Ui.Place(tag.rectTransform, w - 330, 8, 318, 16);
        tag.alignment = TextAlignmentOptions.TopRight;
        var f = Ui.Text(card.transform, fixedText, 13, Theme.TealDeep, true);
        Ui.Place(f.rectTransform, 12, 30, w - 24, h - 36);
        return null;
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
