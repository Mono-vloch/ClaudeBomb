using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Maaloomatiia.Games;

// The debrief: first the level's questions (what the student can explain, name
// or predict), then the curve of every run, the numbers side by side (attempts
// to the first finish and to settle, the race's measures), the checks ticked or
// missed, a medal, LUMA's takeaway and the question to take back to work, the
// next level's unlock, and on a capstone the two if-then plans. The circuit turns
// slowly in a window the whole time; the medal is revealed over it.
public class ResultsScreen
{
    public static ResultsScreen Current;
    public readonly RectTransform root;
    readonly App app;
    readonly Session x;
    RectTransform plans;

    static readonly Color Gold = Theme.Hex("#E0B23A"), Silver = Theme.Hex("#AAB4BE"), Bronze = Theme.Hex("#C48A5A");
    static readonly string[] MedalNames = { "", "BRONZE", "SILVER", "GOLD" };

    public ResultsScreen(Transform parent, App app)
    {
        Current = this;
        this.app = app;
        x = app.session;
        root = Ui.Node("Results", parent);
        Ui.Stretch(root);
        app.Paper(Color.clear);
        if (app.lesson != null && app.lesson.quiz.Length > 0 && !Answered()) Quiz();
        else Build();
    }

    bool Answered() => app.lesson.quiz.All(q => q.IsText ? x.texts.ContainsKey(q.key) : x.picks.ContainsKey(q.key));

    Track Track => x.LastRace?.track ?? x.Last?.track ?? (app.lesson != null ? Track.Get(app.lesson.training[0]) : Track.Get(x.settings.training[0]));

    // The world behind the screen: the track the race ran on, turning slowly, soft.
    void World(RectTransform window)
    {
        app.world.Build(Track, false);
        if (Circuit.HasKit) app.world.ShowCarOrbit(window); else app.world.ShowOrbit(window);
        app.atmosphere.MenuBlur(true);
        var scrim = Ui.Box(root, "Scrim", new Color(Theme.Ink.r, Theme.Ink.g, Theme.Ink.b, 0.3f), false);
        Ui.Stretch(scrim.rectTransform);
        scrim.raycastTarget = false;
    }

    // ---- the questions ------------------------------------------------------------------

    void Quiz()
    {
        Ui.Clear(root);
        var l = app.lesson;
        var window = Ui.Node("Window", root);
        Ui.Place(window, 40, 60, 420, 960);
        World(window);
        var card = Ui.Box(root, "Quiz", Ui.DarkFill);
        Ui.Place(card.rectTransform, 480, 40, 1380, 1000);
        var t = card.transform;
        var title = Ui.Text(t, $"Debrief · Level {l.id} · Before the checks", 40, Color.white, true);
        Ui.Place(title.rectTransform, 32, 28, 1300, 52);
        var scroll = Ui.Scroll(t, "Scroll", out var content);
        Ui.Place(scroll.GetComponent<RectTransform>(), 32, 122, 1316, 790);
        float y = 0;
        var inputs = new Dictionary<string, TMP_InputField>();
        foreach (var q in l.quiz)
        {
            var qc = Ui.DarkCard(content, "Q");
            bool row = !q.IsText && !q.rulebook && q.options.Length <= 5 && q.options.All(o => o.Length <= 26);
            float h = q.IsText ? 120 : row ? 112 : 60 + 42 * (q.rulebook ? Moves.Count : q.options.Length);
            Ui.Place(qc.rectTransform, 0, y, 1316, h);
            string prompt = q.prompt;
            string[] options = q.options;
            int right = q.answer;
            if (q.rulebook)
            {
                var book = x.Last?.book;
                var page = book?.pages.FirstOrDefault(p => book.Best(p.Key, x.Last.s.moves) >= 0 && p.Value.Any(v => v != 0));
                if (page.HasValue && page.Value.Key != null)
                {
                    prompt = $"{q.prompt}\nThe situation: {Driving.Words(Driving.Parse(page.Value.Key), x.Last.s.sensors)}.";
                    options = Moves.Names;
                    right = book.Best(page.Value.Key, x.Last.s.moves);
                }
                else { prompt = q.prompt + "\n(The book is empty: nothing to read.)"; options = Moves.Names; right = -2; }
            }
            var pt = Ui.Text(qc.transform, prompt, 18, Color.white, true);
            Ui.Place(pt.rectTransform, 24, 14, 1268, 44);
            if (q.IsText)
            {
                var input = Ui.DarkInput(qc.transform, "One line.", false, 16);
                Ui.Place(input.GetComponent<RectTransform>(), 24, 60, 1268, 44);
                input.text = x.texts.TryGetValue(q.key, out var t0) ? t0 : "";
                inputs[q.key] = input;
            }
            else
            {
                var buttons = new List<Button>();
                for (int i = 0; i < options.Length; i++)
                {
                    int index = i;
                    string key = q.key;
                    int answer = right;
                    bool picked = x.picks.TryGetValue(key, out int p) && (q.rulebook ? false : p == i);
                    var b = Ui.Button(qc.transform, options[i], () =>
                    {
                        x.picks[key] = q.rulebook ? (index == answer ? 1 : 0) : index;
                        foreach (var ob in buttons) Ui.Chip(ob, false);
                        Ui.Chip(buttons[index], true);
                    }, Ui.ButtonStyle.GhostOnDark, 16);
                    if (row) { float bw = (1268 - 12 * (options.Length - 1)) / options.Length; Ui.Place(b.GetComponent<RectTransform>(), 24 + i * (bw + 12), 62, bw, 40); }
                    else Ui.Place(b.GetComponent<RectTransform>(), 24, 60 + i * 42, 900, 36);
                    var label = b.GetComponentInChildren<TextMeshProUGUI>();
                    label.alignment = row ? TextAlignmentOptions.Midline : TextAlignmentOptions.MidlineLeft;
                    Ui.Stretch(label.rectTransform, 16, 0, 16, 0);
                    Ui.Chip(b, picked);
                    buttons.Add(b);
                }
            }
            y += h + 12;
        }
        content.sizeDelta = new Vector2(0, y);
        var done = Ui.Button(t, "To the checks", () =>
        {
            foreach (var kv in inputs) x.texts[kv.Key] = kv.Value.text;
            foreach (var q in l.quiz) if (!q.IsText && !x.picks.ContainsKey(q.key)) x.picks[q.key] = -1;
            Build();
        }, Ui.ButtonStyle.Accent, 18);
        Ui.Place(done.GetComponent<RectTransform>(), 32, 928, 240, 56);
    }

    public void SkipQuiz() { if (app.lesson != null) foreach (var q in app.lesson.quiz) { if (q.IsText) x.texts[q.key] = ""; else x.picks[q.key] = -1; } Build(); }

    // ---- the debrief --------------------------------------------------------------------

    void Build()
    {
        Ui.Clear(root);
        var l = app.lesson;
        bool counts = l != null, capstone = l != null && l.Capstone;
        var window = Ui.Node("Window", root);
        Ui.Place(window, 1000, 140, 440, 260);
        World(window);
        var title = Ui.Text(root, l != null ? $"Debrief · Level {l.id}" : "The race is over", 44, Color.white, true);
        Ui.Place(title.rectTransform, 80, 36, 1300, 56);
        var extras = new List<(string head, string body)>(); // the full checks and the runs' settings, for the file

        // The curve of every run, left; the circuit and the medal, right.
        var chart = new Chart(root, 80, 100, 900, 300, "POINTS PER ATTEMPT  ·  EVERY RUN OF THIS LEVEL", dark: true);
        chart.Draw(x.runs);

        // The checks, and the medal they earn.
        int medal = 0;
        List<(Practised result, string label)> checks = null;
        if (l != null)
        {
            checks = l.Check(x);
            int ticked = checks.Count(c => c.result == Practised.Yes), needed = checks.Count(c => c.result != Practised.NotNeeded);
            medal = needed > 0 && ticked == needed ? 3 : ticked * 2 >= needed && needed > 0 ? 2 : x.RaceOk(x.LastRace) ? 1 : 0;
            if (counts)
            {
                app.SetStars(l.number, medal);
                string result = ticked == needed ? "complete" : ticked * 2 >= needed ? "partial" : "incomplete";
                app.StartCoroutine(Platform.ReportProgress($"{l.id}:{result}", medal, string.Join("; ", checks.Where(c => c.result != Practised.NotNeeded).Select(c => $"{c.label}: {(c.result == Practised.Yes ? "done" : "missed")}"))));
            }
        }
        Medal(medal, l != null);

        float y = 420;
        float checksH = l != null ? 40 + checks.Count * 30 + 10 : 0;
        if (l != null)
        {
            var card = Ui.DarkCard(root, "Checks");
            Ui.Place(card.rectTransform, 80, y, 900, checksH);
            var ch = Ui.Eyebrow(card.transform, "THE CHECKS", Theme.Teal); Ui.Place(ch.rectTransform, 16, 10, 400, 22);
            float cy = 40;
            foreach (var (result, label) in checks)
            {
                // A lamp and a short label; the full label is in the file.
                var lamp = Ui.Box(card.transform, "Lamp", result == Practised.Yes ? Theme.Aqua : result == Practised.Missed ? Theme.Danger : Theme.Border, false);
                Ui.Place(lamp.rectTransform, 18, cy + 6, 12, 12);
                lamp.raycastTarget = false;
                var lab = Ui.Text(card.transform, Labels.Of(label), 16, result == Practised.NotNeeded ? Theme.Faint : Color.white);
                Ui.Place(lab.rectTransform, 42, cy, 840, 24);
                lab.enableWordWrapping = false; lab.overflowMode = TextOverflowModes.Ellipsis;
                cy += 30;
            }
            extras.Add(("THE CHECKS", string.Join("\n", checks.Select(c => (c.result == Practised.Yes ? "Done: " : c.result == Practised.Missed ? "Missed: " : "Not needed: ") + c.label))));
        }

        // The numbers side by side.
        float runsH = Mathf.Max(checksH, 40 + x.runs.Count * 56 + (x.real != null ? 28 : 0) + (l != null && l.measure && x.races.Count > 0 ? 30 : 0) + 12);
        var table = Ui.DarkCard(root, "Runs");
        Ui.Place(table.rectTransform, 1000, y, 860, runsH);
        var th = Ui.Eyebrow(table.transform, "THE RUNS  ·  FIRST FINISH, SETTLED, THE RACE", Theme.Teal);
        Ui.Place(th.rectTransform, 16, 10, 820, 22);
        float ry = 40;
        for (int i = 0; i < x.runs.Count; i++)
        {
            var r = x.runs[i];
            var race = i < x.races.Count ? x.races[i] : null;
            string name = string.IsNullOrEmpty(r.label) ? $"Run {i + 1}" : r.label;
            var line = Ui.Text(table.transform, $"<b>{name}</b>  ·  first finish {(r.firstFinish > 0 ? r.firstFinish.ToString() : "never")}  ·  settled {(r.settled > 0 ? r.settled.ToString() : "never")} of {r.s.attempts}" +
                               (race != null ? $"\nrace {race.Finishes}/{race.laps.Count}  ·  {race.CrashesPerLap:0.0} crashes, {race.Touches:0.0} touches, {race.LapTime:0} moves a lap" : ""), 16, Color.white);
            Ui.Place(line.rectTransform, 16, ry, 828, 50);
            line.enableWordWrapping = false; line.overflowMode = TextOverflowModes.Ellipsis;
            extras.Add((name.ToUpperInvariant(), r.s.Knobs + $"  ·  {r.s.SensorsOn} sensors, {r.s.MovesOn} moves, top speed {r.s.topSpeed}, finish {r.s.finish:0}, crash {r.s.crash:0}, toward {r.s.toward:0.#}, speed bonus {r.s.speedBonus:0.#}"));
            ry += 56;
        }
        if (x.real != null)
        {
            var real = Ui.Text(table.transform, $"<b>The real run</b>  ·  {(x.real.Finishes > 0 ? $"finished in {x.real.LapTime:0} moves" : "crashed")}  ·  {x.real.unknown} unknown situations", 16, x.real.Finishes > 0 ? Theme.Teal : Theme.Danger);
            Ui.Place(real.rectTransform, 16, ry, 828, 24);
            ry += 28;
        }
        if (l != null && l.measure && x.races.Count > 0)
        {
            var ml = Ui.Text(table.transform, "Judged by " + Measures.Names[x.settings.measure].ToLowerInvariant() + ": " + string.Join(", ", x.runs.Select((r, i) => $"{(string.IsNullOrEmpty(r.label) ? "run " + (i + 1) : r.label)} {x.races[i].Measure(x.settings.measure):0.0}")), 16, Theme.Teal, true);
            Ui.Place(ml.rectTransform, 16, ry, 828, 24);
        }
        y += runsH + 12;

        if (l != null)
        {
            app.luma.Say("recap");
            bool unlock = counts && l.number < Ladder.Count;
            app.luma.Line(root, 80, y, unlock ? 1320 : 1780, l.takeaway, true, true, 18);
            if (unlock) Unlock(y, Ladder.Get(l.number + 1), medal);
            y += 150;
        }

        float bx = 80;
        Button Add(string label, System.Action act, Ui.ButtonStyle style, float w)
        {
            var b = Ui.Button(root, label, act, style, 17);
            Ui.Place(b.GetComponent<RectTransform>(), bx, 990, w, 56);
            bx += w + 14;
            return b;
        }
        if (l != null)
        {
            {
                bool last = l.number >= Ladder.Count;
                Add(last ? "Free play" : $"Next: {Ladder.Get(l.number + 1).id} {Ladder.Get(l.number + 1).title.Replace("Capstone: ", "")}", last ? app.StartFree : (System.Action)app.NextLevel, Ui.ButtonStyle.Primary, last ? 180 : 520);
                Add("Train again", app.ShowPit, Ui.ButtonStyle.GhostOnDark, 160);
                if (capstone) Add("If-then plans", ShowPlans, Ui.ButtonStyle.GhostOnDark, 180);
            }
        }
        else Add("Set up again", app.ShowPit, Ui.ButtonStyle.Primary, 200);
        Add("The file", () => app.ShowFile(extras.ToArray()), Ui.ButtonStyle.GhostOnDark, 130);
        Add("Title screen", app.ShowTitle, Ui.ButtonStyle.GhostOnDark, 160);
        if (Platform.IsWeb) Add("Games menu", Platform.GoToMenu, Ui.ButtonStyle.GhostOnDark, 160);
    }

    // The medal, revealed over the turning circuit: the badge comes in with an overshoot,
    // the stars light one by one, confetti and a fanfare for silver and gold.
    void Medal(int medal, bool hasLevel)
    {
        var cap = Ui.Glass(root, "Window caption", 1000, 100, 860, 40);
        var caption = Ui.Eyebrow(cap.transform, $"{Track.name.ToUpperInvariant()}  ·  THE TEST TRACK  ·  {(x.LastRace != null ? $"{x.LastRace.Finishes} OF {x.LastRace.laps.Count} LAPS FINISHED" : "NO RACE YET")}", Theme.Teal);
        Ui.Place(caption.rectTransform, 20, 10, 820, 22);
        Color colour = medal == 3 ? Gold : medal == 2 ? Silver : medal == 1 ? Bronze : Theme.Ink;
        var badge = Ui.Glass(root, "Medal", 1460, 200, 400, 160, true, colour);
        var b = badge.transform;
        var tag = Ui.Eyebrow(b, hasLevel ? "THE MEDAL" : "FREE PLAY", Theme.Teal);
        Ui.Place(tag.rectTransform, 0, 14, 400, 18); tag.alignment = TextAlignmentOptions.Top;
        var name = Ui.Text(b, medal > 0 ? MedalNames[medal] : "NO MEDAL YET", medal > 0 ? 44 : 28, medal > 0 ? colour : Look.AsphaltLine, true, TextAlignmentOptions.Center);
        name.characterSpacing = 6;
        Ui.Place(name.rectTransform, 0, 34, 400, 60);
        var stars = new Image[3];
        for (int s = 0; s < 3; s++)
        {
            stars[s] = Ui.Box(b, "Star", s < medal ? colour : new Color(1, 1, 1, 0.16f));
            Ui.Place(stars[s].rectTransform, 155 + s * 32, 98, 26, 26);
            stars[s].raycastTarget = false;
        }
        var rank = Ui.Text(b, hasLevel ? $"{medal} of 3 stars  ·  {app.TotalStars} in all  ·  {app.Rank}" : "the stars count on a level's own run", 16, Look.AsphaltLine, false, TextAlignmentOptions.Center);
        Ui.Place(rank.rectTransform, 0, 128, 400, 24);
        app.StartCoroutine(Reveal(badge, stars, medal, colour));
    }

    IEnumerator Reveal(Image badge, Image[] stars, int medal, Color colour)
    {
        var group = Ui.Group(badge);
        group.alpha = 0f;
        badge.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        badge.rectTransform.anchoredPosition = new Vector2(1460 + 200, -(200 + 80));
        foreach (var s in stars) s.transform.localScale = Vector3.zero;
        yield return new WaitForSecondsRealtime(0.55f);
        if (badge == null) yield break;
        if (medal > 0) Sfx.Sting(Sfx.Mood.Fanfare); else Sfx.Tick();
        yield return Ui.Animate(0.45f, k =>
        {
            if (badge == null) return;
            group.alpha = Mathf.Min(1f, k * 3f);
            badge.rectTransform.localScale = Vector3.one * Mathf.LerpUnclamped(0.5f, 1f, Ui.EaseOutBack(k));
        });
        for (int s = 0; s < medal; s++)
        {
            int i = s;
            Sfx.Chime();
            yield return Ui.Animate(0.22f, k => { if (stars[i] != null) stars[i].transform.localScale = Vector3.one * Mathf.LerpUnclamped(0f, 1f, Ui.EaseOutBack(k)); });
            if (medal >= 2) app.world.Celebrate();
            yield return new WaitForSecondsRealtime(0.1f);
        }
        for (int s = medal; s < 3; s++) if (stars[s] != null) stars[s].transform.localScale = Vector3.one;
    }

    void Unlock(float y, Lesson next, int stars)
    {
        var card = Ui.Card(root, "Unlock", Theme.TealDeep, Theme.TealDeep);
        Ui.Place(card.rectTransform, 1420, y, 440, 136);
        var t = card.transform;
        var tag = Ui.Eyebrow(t, $"UNLOCKED FOR LEVEL {next.id}", Theme.Teal); tag.font = Theme.Bold;
        Ui.Place(tag.rectTransform, 20, 14, 400, 20);
        string what = next.number == Ladder.AttemptsLevel ? "The attempts"
                    : next.number == Ladder.SensorsLevel ? "The sensors"
                    : next.number == Ladder.MovesLevel ? "The moves and a constraint"
                    : next.number == Ladder.PointsLevel ? "The points table"
                    : next.number == Ladder.ExploreLevel ? "New moves"
                    : next.number == Ladder.TracksLevel ? "The tracks"
                    : next.number == Ladder.MeasureLevel ? "The measure"
                    : next.number == Ladder.KnobsLevel ? "The knobs and the seed"
                    : next.Capstone ? "Everything open: the capstone" : next.title;
        var body = Ui.Text(t, what, 20, Color.white, true);
        Ui.Place(body.rectTransform, 20, 38, 400, 56);
        var starsLine = Ui.Text(t, $"{stars} of 3 stars banked  ·  {app.TotalStars} in all  ·  {app.Rank}", 16, Theme.Teal);
        Ui.Place(starsLine.rectTransform, 20, 100, 400, 24);
    }

    // The capstone's two if-then plans, about the student's own work.
    void ShowPlans()
    {
        if (plans != null) { Object.Destroy(plans.gameObject); plans = null; return; }
        plans = Ui.Node("Plans", root);
        Ui.Stretch(plans);
        var dim = Ui.Box(plans, "Dim", new Color(Theme.Ink.r, Theme.Ink.g, Theme.Ink.b, 0.45f), false);
        Ui.Stretch(dim.rectTransform);
        var card = Ui.Box(plans, "Card", Ui.DarkFill);
        Ui.Place(card.rectTransform, 360, 200, 1200, 680);
        var t = card.transform;
        var title = Ui.Text(t, "Your two if-then plans", 26, Color.white, true);
        Ui.Place(title.rectTransform, 32, 26, 800, 36);
        var sub = Ui.Text(t, "Two plans about your own work, to look at again at the follow-up session. Write, save.", 16, Theme.Faint);
        Ui.Place(sub.rectTransform, 32, 62, 1000, 24);
        var close = Ui.Button(t, "Close", () => { Object.Destroy(plans.gameObject); plans = null; }, Ui.ButtonStyle.GhostOnDark, 16);
        Ui.Place(close.GetComponent<RectTransform>(), 1200 - 32 - 100, 26, 100, 40);
        string key = $"robot_racing_plans_{app.lesson.id}";
        var saved = PlayerPrefs.GetString(key, "\n").Split('\n');
        var inputs = new TMP_InputField[2];
        for (int i = 0; i < 2; i++)
        {
            var label = Ui.Eyebrow(t, i == 0 ? "PLAN 1  ·  IF ..., THEN I ..." : "PLAN 2  ·  IF ..., THEN I ...", Theme.Teal);
            Ui.Place(label.rectTransform, 32, 110 + i * 220, 600, 22);
            inputs[i] = Ui.DarkInput(t, "If we reward a number at work, then I ...", true, 18);
            Ui.Place(inputs[i].GetComponent<RectTransform>(), 32, 136 + i * 220, 1136, 160);
            inputs[i].text = i < saved.Length ? saved[i] : "";
        }
        var save = Ui.Button(t, "Save", () => { PlayerPrefs.SetString(key, inputs[0].text.Replace("\n", " ") + "\n" + inputs[1].text.Replace("\n", " ")); PlayerPrefs.Save(); Sfx.Buy(); }, Ui.ButtonStyle.Primary, 16);
        Ui.Place(save.GetComponent<RectTransform>(), 32, 600, 140, 46);
    }
}
