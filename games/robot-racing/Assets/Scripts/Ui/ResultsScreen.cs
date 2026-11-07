using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Maaloomatiia.Games;

// The debrief is a moment, then a door (the game-feel pass): the car where the race left
// it, the lap board reading the laps, the medal revealed over it with its stars, LUMA's one
// line, Next and a quiet Again. The curve of every run, the checks, the runs' numbers, the
// stars in all and the rank sit behind Details, closed by default. The report to the
// platform and the stars banked are as they were.
public class ResultsScreen
{
    public static ResultsScreen Current;
    public readonly RectTransform root;
    readonly App app;
    readonly Session x;
    RectTransform sheet, plans;
    int medal;
    List<(Practised result, string label)> checks;
    readonly List<(string head, string body)> extras = new List<(string head, string body)>();

    static readonly Color Gold = Theme.Hex("#E0B23A"), Silver = Theme.Hex("#AAB4BE"), Bronze = Theme.Hex("#C48A5A");
    static readonly string[] MedalNames = { "", "BRONZE", "SILVER", "GOLD" };

    public static bool Answered(Session x) => x.lesson == null || x.lesson.quiz.All(q => q.IsText ? x.texts.ContainsKey(q.key) : x.picks.ContainsKey(q.key));

    public ResultsScreen(Transform parent, App app)
    {
        Current = this;
        this.app = app;
        x = app.session;
        root = Ui.Node("Results", parent);
        Ui.Stretch(root);
        app.Paper(Color.clear);
        if (app.lesson != null && app.lesson.quiz.Length > 0 && !Answered(x)) Quiz();
        else Build();
    }

    Track Track => x.LastRace?.track ?? x.Last?.track ?? (app.lesson != null ? Track.Get(app.lesson.training[0]) : Track.Get(x.settings.training[0]));

    // ---- the moment ----

    void Build()
    {
        Ui.Clear(root);
        var l = app.lesson;
        bool counts = l != null;
        var frame = Ui.Node("Frame", root);
        Ui.Place(frame, 0, 0, Ui.W, Ui.H);
        app.world.Build(Track, false);
        var race = x.LastRace;
        if (race != null)
        {
            var c = race.car;
            app.world.Pose(c.from, c.pos, c.heading, 0, 1f, race.readings, x.settings.sensors);
            app.world.LapBoard($"LAP {race.Finishes}/{race.laps.Count}");
        }
        if (Circuit.HasKit) app.world.ShowCarOrbit(frame, 1.75f, 17f); else app.world.ShowOrbit(frame);
        app.atmosphere.MenuBlur(false);

        // The checks, and the medal they earn.
        medal = 0;
        checks = null;
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
            extras.Add(("THE CHECKS", string.Join("\n", checks.Select(c => (c.result == Practised.Yes ? "Done: " : c.result == Practised.Missed ? "Missed: " : "Not needed: ") + c.label))));
            for (int i = 0; i < x.runs.Count; i++)
            {
                var r = x.runs[i];
                string name = string.IsNullOrEmpty(r.label) ? $"Run {i + 1}" : r.label;
                extras.Add((name.ToUpperInvariant(), r.s.Knobs + $"  ·  {r.s.SensorsOn} sensors, {r.s.MovesOn} moves, top speed {r.s.topSpeed}, finish {r.s.finish:0}, crash {r.s.crash:0}, toward {r.s.toward:0.#}, speed bonus {r.s.speedBonus:0.#}"));
            }
        }
        Medal(medal, l != null);
        GameMenu.Icon(root);

        if (l != null)
        {
            string take = "take-" + l.number;
            if (Luma.Lines.ContainsKey(take)) app.StartCoroutine(SayLater(take, 2.2f));
        }

        // The door: Next, a quiet Again, and Details.
        bool last = l != null && l.number >= Ladder.Count;
        var next = Ui.Button(root, l == null ? "Set up again" : last ? "Free play" : "Next", l == null ? app.ShowPit : last ? app.StartFree : (System.Action)app.NextLevel, Ui.ButtonStyle.Accent, 22);
        Ui.Place(next.GetComponent<RectTransform>(), Ui.W - 40 - 240, 996, 240, 64);
        if (l != null)
        {
            var again = Ui.Button(root, "Again", app.ShowPit, Ui.ButtonStyle.GhostOnDark, 18);
            Ui.Place(again.GetComponent<RectTransform>(), Ui.W - 40 - 240 - 14 - 140, 1004, 140, 56);
        }
        var details = Ui.Button(root, "Details", ToggleSheet, Ui.ButtonStyle.GhostOnDark, 18);
        Ui.Place(details.GetComponent<RectTransform>(), 40, 1004, 140, 56);
    }

    IEnumerator SayLater(string id, float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);
        if (root != null) app.luma.Cue(root, id, 40, 920, 820, stay: true);
    }

    // The medal, revealed over the car: the badge comes in with an overshoot, the stars light
    // one by one, confetti and a fanfare for silver and gold.
    void Medal(int medal, bool hasLevel)
    {
        Color colour = medal == 3 ? Gold : medal == 2 ? Silver : medal == 1 ? Bronze : Theme.Faint;
        var badge = Ui.Glass(root, "Medal", (Ui.W - 440) / 2, 90, 440, 170, true, colour);
        var b = badge.transform;
        var name = Ui.Text(b, medal > 0 ? MedalNames[medal] : "", 52, colour, true, TextAlignmentOptions.Center);
        name.characterSpacing = 8;
        Ui.Place(name.rectTransform, 0, 28, 440, 64);
        var stars = new Image[3];
        for (int s = 0; s < 3; s++)
        {
            stars[s] = Ui.Box(b, "Star", s < medal ? colour : new Color(1, 1, 1, 0.16f));
            Ui.Place(stars[s].rectTransform, 220 - 48 + s * 34 - 14, 108, 28, 28);
            stars[s].raycastTarget = false;
        }
        app.StartCoroutine(Reveal(badge, stars, medal, colour));
    }

    IEnumerator Reveal(Image badge, Image[] stars, int medal, Color colour)
    {
        var group = Ui.Group(badge);
        group.alpha = 0f;
        badge.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        badge.rectTransform.anchoredPosition = new Vector2(Ui.W / 2, -(90 + 85));
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

    // ---- Details: the curve, the checks, the runs, the stars, behind one button ----

    void ToggleSheet()
    {
        if (sheet != null) { Object.Destroy(sheet.gameObject); sheet = null; return; }
        var l = app.lesson;
        const float W = 760f, DW = W - 56;
        sheet = Ui.Node("Details", root);
        Ui.Place(sheet, Ui.W - W, 0, W, Ui.H);
        var bg = Ui.Box(sheet, "Glass", Ui.DarkFill, false);
        Ui.Stretch(bg.rectTransform);
        var keyline = Ui.Box(sheet, "Keyline", Theme.Teal, false);
        Ui.Place(keyline.rectTransform, 0, 0, 3, Ui.H);
        var tag = Ui.Eyebrow(sheet, l != null ? $"DETAILS  ·  LEVEL {l.id}" : "DETAILS", Theme.Teal); tag.font = Theme.Bold;
        Ui.Place(tag.rectTransform, 28, 26, 500, 22);
        var close = Ui.Button(sheet, "Close", ToggleSheet, Ui.ButtonStyle.GhostOnDark, 16);
        Ui.Place(close.GetComponent<RectTransform>(), W - 112, 18, 88, 40);
        var scroll = Ui.Scroll(sheet, "Scroll", out var content);
        Ui.Place(scroll.GetComponent<RectTransform>(), 20, 70, W - 40, Ui.H - 90);
        float y = 0;
        var chart = new Chart(content, 0, y, W - 40, 260, "POINTS PER ATTEMPT  ·  EVERY RUN OF THIS LEVEL", dark: true);
        chart.Draw(x.runs);
        y += 272;
        if (l != null && checks != null)
        {
            var card = Ui.DarkCard(content, "Checks");
            float h = 40 + checks.Count * 30 + 10;
            Ui.Place(card.rectTransform, 0, y, W - 40, h);
            var ch = Ui.Eyebrow(card.transform, "THE CHECKS", Theme.Teal); Ui.Place(ch.rectTransform, 16, 10, 400, 22);
            float cy = 40;
            foreach (var (result, label) in checks)
            {
                var lamp = Ui.Box(card.transform, "Lamp", result == Practised.Yes ? Theme.Aqua : result == Practised.Missed ? Theme.Danger : Theme.Border, false);
                Ui.Place(lamp.rectTransform, 18, cy + 6, 12, 12);
                lamp.raycastTarget = false;
                var lab = Ui.Text(card.transform, Labels.Of(label), 16, result == Practised.NotNeeded ? Theme.Faint : Color.white);
                Ui.Place(lab.rectTransform, 42, cy, W - 100, 24);
                lab.enableWordWrapping = false; lab.overflowMode = TextOverflowModes.Ellipsis;
                cy += 30;
            }
            y += h + 12;
            var stars = Ui.Text(content, $"{medal} of 3 stars  ·  {app.TotalStars} in all  ·  {app.Rank}", 16, Theme.Teal, true);
            Ui.Place(stars.rectTransform, 8, y, DW, 24); y += 34;
        }
        float runsH = 40 + x.runs.Count * 56 + (x.real != null ? 28 : 0) + (l != null && l.measure && x.races.Count > 0 ? 30 : 0) + 12;
        var table = Ui.DarkCard(content, "Runs");
        Ui.Place(table.rectTransform, 0, y, W - 40, runsH);
        var th = Ui.Eyebrow(table.transform, "THE RUNS  ·  FIRST FINISH, SETTLED, THE RACE", Theme.Teal);
        Ui.Place(th.rectTransform, 16, 10, 600, 22);
        float ry = 40;
        for (int i = 0; i < x.runs.Count; i++)
        {
            var r = x.runs[i];
            var race = i < x.races.Count ? x.races[i] : null;
            string name = string.IsNullOrEmpty(r.label) ? $"Run {i + 1}" : r.label;
            var line = Ui.Text(table.transform, $"<b>{name}</b>  ·  first finish {(r.firstFinish > 0 ? r.firstFinish.ToString() : "none")}  ·  settled {(r.settled > 0 ? "at " + r.settled : "no")}  ·  {r.s.attempts} attempts" +
                               (race != null ? $"\nrace {race.Finishes}/{race.laps.Count}  ·  {race.CrashesPerLap:0.0} crashes, {race.Touches:0.0} touches, {race.LapTime:0} moves a lap" : ""), 16, Color.white);
            Ui.Place(line.rectTransform, 16, ry, W - 80, 50);
            line.enableWordWrapping = false; line.overflowMode = TextOverflowModes.Ellipsis;
            ry += 56;
        }
        if (x.real != null)
        {
            var real = Ui.Text(table.transform, $"<b>The real run</b>  ·  {(x.real.Finishes > 0 ? $"finished in {x.real.LapTime:0} moves" : "crashed")}  ·  {x.real.unknown} unknown situations", 16, x.real.Finishes > 0 ? Theme.Teal : Theme.Danger);
            Ui.Place(real.rectTransform, 16, ry, W - 80, 24);
            ry += 28;
        }
        if (l != null && l.measure && x.races.Count > 0)
        {
            var ml = Ui.Text(table.transform, "Judged by " + Measures.Names[x.settings.measure].ToLowerInvariant() + ": " + string.Join(", ", x.runs.Select((r, i) => $"{(string.IsNullOrEmpty(r.label) ? "run " + (i + 1) : r.label)} {x.races[i].Measure(x.settings.measure):0.0}")), 16, Theme.Teal, true);
            Ui.Place(ml.rectTransform, 16, ry, W - 80, 24);
        }
        y += runsH + 12;
        if (l != null)
        {
            var take = Ui.Text(content, l.takeaway, 17, Color.white);
            float th2 = take.GetPreferredValues(l.takeaway, DW, 0).y + 4;
            Ui.Place(take.rectTransform, 8, y, DW, th2); y += th2 + 10;
            var ask = Ui.Text(content, l.ask, 16, Theme.Faint);
            float ah = ask.GetPreferredValues(l.ask, DW, 0).y + 4;
            Ui.Place(ask.rectTransform, 8, y, DW, ah); y += ah + 16;
            if (l.number < Ladder.Count)
            {
                var nextL = Ladder.Get(l.number + 1);
                var unlock = Ui.Text(content, $"Next: {nextL.id} {nextL.title.Replace("Capstone: ", "")}", 16, Theme.Teal, true);
                Ui.Place(unlock.rectTransform, 8, y, DW, 24); y += 34;
            }
            if (l.Capstone)
            {
                var pl = Ui.Button(content, "If-then plans", ShowPlans, Ui.ButtonStyle.GhostOnDark, 16);
                Ui.Place(pl.GetComponent<RectTransform>(), 8, y, 200, 46); y += 56;
            }
            var file = Ui.Button(content, "The file", () => app.ShowFile(extras.ToArray()), Ui.ButtonStyle.GhostOnDark, 16);
            Ui.Place(file.GetComponent<RectTransform>(), 8, y, 140, 46); y += 56;
        }
        content.sizeDelta = new Vector2(0, y + 20);
    }

    // ---- the questions the world cannot carry: the old list (the rule book, a line of text) ----

    void Quiz()
    {
        Ui.Clear(root);
        var l = app.lesson;
        var window = Ui.Node("Window", root);
        Ui.Place(window, 40, 60, 420, 960);
        app.world.Build(Track, false);
        if (Circuit.HasKit) app.world.ShowCarOrbit(window); else app.world.ShowOrbit(window);
        var card = Ui.Box(root, "Quiz", Ui.DarkFill);
        Ui.Place(card.rectTransform, 480, 40, 1380, 1000);
        var t = card.transform;
        var title = Ui.Text(t, "Before the checks", 40, Color.white, true);
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
