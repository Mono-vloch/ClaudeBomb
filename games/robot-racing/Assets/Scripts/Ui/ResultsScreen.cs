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
// next level's unlock, and on a capstone the two if-then plans.
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
        app.Paper(Theme.Page);
        if (app.lesson != null && app.lesson.quiz.Length > 0 && !Answered()) Quiz();
        else Build();
    }

    bool Answered() => app.lesson.quiz.All(q => q.IsText ? x.texts.ContainsKey(q.key) : x.picks.ContainsKey(q.key));

    // ---- the questions ------------------------------------------------------------------

    void Quiz()
    {
        Ui.Clear(root);
        var l = app.lesson;
        var title = Ui.Text(root, $"Debrief · Level {l.id} · Before the checks", 44, Theme.Ink, true);
        Ui.Place(title.rectTransform, 80, 44, 1400, 56);
        var sub = Ui.Text(root, "A few questions first: what you can name, explain or read off the run. The checks count them.", 17, Theme.Text2);
        Ui.Place(sub.rectTransform, 80, 104, 1600, 26);
        var scroll = Ui.Scroll(root, "Scroll", out var content);
        Ui.Place(scroll.GetComponent<RectTransform>(), 80, 144, 1760, 800);
        float y = 0;
        var inputs = new Dictionary<string, TMP_InputField>();
        foreach (var q in l.quiz)
        {
            var card = Ui.Card(content, "Q", Theme.Panel, Theme.Border1);
            bool row = !q.IsText && !q.rulebook && q.options.Length <= 5 && q.options.All(o => o.Length <= 26);
            float h = q.IsText ? 120 : row ? 112 : 60 + 42 * (q.rulebook ? Moves.Count : q.options.Length);
            Ui.Place(card.rectTransform, 0, y, 1760, h);
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
            var pt = Ui.Text(card.transform, prompt, 18, Theme.Ink, true);
            Ui.Place(pt.rectTransform, 24, 14, 1700, 44);
            if (q.IsText)
            {
                var input = Ui.Input(card.transform, "One line.", false, 16);
                Ui.Place(input.GetComponent<RectTransform>(), 24, 60, 1700, 44);
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
                    var b = Ui.Button(card.transform, options[i], () =>
                    {
                        x.picks[key] = q.rulebook ? (index == answer ? 1 : 0) : index;
                        foreach (var ob in buttons) ob.transform.Find("Fill").GetComponent<Image>().color = Theme.Panel;
                        buttons[index].transform.Find("Fill").GetComponent<Image>().color = Theme.Surface2;
                    }, Ui.ButtonStyle.Secondary, 15);
                    if (row) { float bw = (1712 - 12 * (options.Length - 1)) / options.Length; Ui.Place(b.GetComponent<RectTransform>(), 24 + i * (bw + 12), 62, bw, 40); }
                    else Ui.Place(b.GetComponent<RectTransform>(), 24, 60 + i * 42, 900, 36);
                    var label = b.GetComponentInChildren<TextMeshProUGUI>();
                    label.alignment = row ? TextAlignmentOptions.Midline : TextAlignmentOptions.MidlineLeft;
                    Ui.Stretch(label.rectTransform, 16, 0, 16, 0);
                    if (picked) b.transform.Find("Fill").GetComponent<Image>().color = Theme.Surface2;
                    buttons.Add(b);
                }
            }
            y += h + 12;
        }
        content.sizeDelta = new Vector2(0, y);
        var done = Ui.Button(root, "To the checks", () =>
        {
            foreach (var kv in inputs) x.texts[kv.Key] = kv.Value.text;
            foreach (var q in l.quiz) if (!q.IsText && !x.picks.ContainsKey(q.key)) x.picks[q.key] = -1;
            Build();
        }, Ui.ButtonStyle.Accent, 18);
        Ui.Place(done.GetComponent<RectTransform>(), 80, 980, 240, 56);
        Ui.Emblem(root);
    }

    public void SkipQuiz() { if (app.lesson != null) foreach (var q in app.lesson.quiz) { if (q.IsText) x.texts[q.key] = ""; else x.picks[q.key] = -1; } Build(); }

    // ---- the debrief --------------------------------------------------------------------

    void Build()
    {
        Ui.Clear(root);
        var l = app.lesson;
        bool training = app.Training, counts = l != null && !training, capstone = l != null && l.Capstone;
        var title = Ui.Text(root, l != null ? $"Debrief · Level {l.id} · {app.StepName}" : "The race is over", 44, Theme.Ink, true);
        Ui.Place(title.rectTransform, 80, 40, 1300, 56);

        // The curve of every run, and the numbers side by side.
        var chart = new Chart(root, 80, 104, 900, 300, "POINTS PER ATTEMPT  ·  EVERY RUN OF THIS LEVEL");
        chart.Draw(x.runs);
        var table = Ui.Card(root, "Runs", Theme.Panel, Theme.Border1);
        Ui.Place(table.rectTransform, 1000, 104, 840, 300);
        var th = Ui.Eyebrow(table.transform, "THE RUNS  ·  ATTEMPTS TO THE FIRST FINISH, TO SETTLE, AND THE RACE", Theme.TealDeep);
        Ui.Place(th.rectTransform, 16, 10, 800, 16);
        float ry = 32;
        for (int i = 0; i < x.runs.Count && ry < 270; i++)
        {
            var r = x.runs[i];
            var race = i < x.races.Count ? x.races[i] : null;
            string name = string.IsNullOrEmpty(r.label) ? $"Run {i + 1}" : r.label;
            var line = Ui.Text(table.transform, $"<b>{name}</b>  ·  first finish {(r.firstFinish > 0 ? r.firstFinish.ToString() : "never")}  ·  settled {(r.settled > 0 ? r.settled.ToString() : "never")} of {r.s.attempts}  ·  " +
                               (race != null ? $"race {race.Finishes}/{race.laps.Count} finished, {race.CrashesPerLap:0.0} crashes, {race.Touches:0.0} touches a lap, {race.LapTime:0} moves a lap" : ""), 13, Theme.Ink);
            Ui.Place(line.rectTransform, 16, ry, 808, 20);
            line.enableWordWrapping = false; line.overflowMode = TextOverflowModes.Ellipsis;
            var knobs = Ui.Text(table.transform, (l != null && (l.knobs || l.explore || l.points) ? r.s.Knobs + "  ·  " : "") + $"{r.s.SensorsOn} sensors, {r.s.MovesOn} moves, top speed {r.s.topSpeed}, finish {r.s.finish:0} crash {r.s.crash:0} toward {r.s.toward:0.#} speed bonus {r.s.speedBonus:0.#}", 11, Theme.Muted);
            Ui.Place(knobs.rectTransform, 16, ry + 20, 808, 16);
            knobs.enableWordWrapping = false; knobs.overflowMode = TextOverflowModes.Ellipsis;
            ry += 42;
        }
        if (x.real != null)
        {
            var real = Ui.Text(table.transform, $"<b>The real run</b>  ·  {(x.real.Finishes > 0 ? $"finished in {x.real.LapTime:0} moves" : "crashed")}, {x.real.unknown} unknown situations met", 13, x.real.Finishes > 0 ? Theme.TealDeep : Theme.Danger);
            Ui.Place(real.rectTransform, 16, ry, 808, 20);
        }
        if (l != null && l.measure && x.races.Count > 0)
        {
            var ml = Ui.Text(table.transform, "Judged by " + Measures.Names[x.settings.measure].ToLowerInvariant() + ": " + string.Join(", ", x.runs.Select((r, i) => $"{(string.IsNullOrEmpty(r.label) ? "run " + (i + 1) : r.label)} {x.races[i].Measure(x.settings.measure):0.0}")), 13, Theme.TealDeep, true);
            Ui.Place(ml.rectTransform, 16, 268, 808, 24);
        }

        // The checks
        int medal = 0;
        float y = 420;
        if (l != null)
        {
            var checks = l.Check(x);
            int ticked = checks.Count(c => c.result == Practised.Yes), needed = checks.Count(c => c.result != Practised.NotNeeded);
            medal = needed > 0 && ticked == needed ? 3 : ticked * 2 >= needed && needed > 0 ? 2 : x.RaceOk(x.LastRace) ? 1 : 0;
            var card = Ui.Card(root, "Checks", Theme.Panel, Theme.Border1);
            Ui.Place(card.rectTransform, 80, y, 1760, 32 + checks.Count * 28);
            var ch = Ui.Eyebrow(card.transform, "THE CHECKS  ·  WHAT YOU DID", Theme.TealDeep); Ui.Place(ch.rectTransform, 16, 10, 400, 16);
            if (medal > 0)
            {
                var pill = Ui.Box(card.transform, "Medal", medal == 3 ? Gold : medal == 2 ? Silver : Bronze);
                Ui.Place(pill.rectTransform, 1660, 8, 84, 24);
                var mt = Ui.Text(pill.transform, MedalNames[medal], 11, Color.white, true, TextAlignmentOptions.Center); mt.characterSpacing = 3; Ui.Stretch(mt.rectTransform);
            }
            float cy = 32;
            foreach (var (result, label) in checks)
            {
                var word = Ui.Text(card.transform, result == Practised.Yes ? "Done" : result == Practised.Missed ? "Missed" : "n/a", 14, result == Practised.Yes ? Theme.TealDeep : result == Practised.Missed ? Theme.Danger : Theme.Muted, true);
                Ui.Place(word.rectTransform, 16, cy + 1, 60, 22);
                var lab = Ui.Text(card.transform, label, 15, Theme.Ink);
                Ui.Place(lab.rectTransform, 84, cy, 1600, 22);
                cy += 28;
            }
            y += 32 + checks.Count * 28 + 12;

            if (counts)
            {
                app.SetStars(l.number, medal);
                string result = ticked == needed ? "complete" : ticked * 2 >= needed ? "partial" : "incomplete";
                app.StartCoroutine(Platform.ReportProgress($"{l.id}:{result}", medal, string.Join("; ", checks.Where(c => c.result != Practised.NotNeeded).Select(c => $"{c.label}: {(c.result == Practised.Yes ? "done" : "missed")}"))));
            }
            app.luma.Say("recap");
            bool unlock = counts && l.number < Ladder.Count;
            string second = "Ask back at work: " + l.ask;
            string takeaway = l.takeaway + (l.number == 3 ? " Studying with an answer key: supervised. Finding groups with no key: unsupervised. Learning to ride a bike: reinforcement. The robot was never shown a correct lap." : "");
            Luma.Card(root, 80, y, unlock ? 1320 : 1760, 136, takeaway, 17, second);
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
            if (training && app.step == App.Step.SeeItWork)
            {
                if (l.broken != null) Add("Next: Fix the broken bot", () => { app.SetStep(App.Step.FixIt); app.ShowPit(); }, Ui.ButtonStyle.Primary, 300);
                Add("Next: Your run", () => { app.SetStep(App.Step.YourTurn); app.ShowPit(); }, l.broken != null ? Ui.ButtonStyle.Secondary : Ui.ButtonStyle.Primary, 200);
                Add("Watch it again", app.ShowPit, Ui.ButtonStyle.Secondary, 180);
            }
            else if (training)
            {
                Add("Fix the setting, train again", () => { app.SetStep(App.Step.YourTurn); app.ShowPit(); }, Ui.ButtonStyle.Primary, 320);
                Add("Watch the broken bot again", app.ShowPit, Ui.ButtonStyle.Secondary, 280);
            }
            else
            {
                bool last = l.number >= Ladder.Count;
                Add(last ? "Free play" : $"Next: {Ladder.Get(l.number + 1).id} {Ladder.Get(l.number + 1).title.Replace("Capstone: ", "")}", last ? app.StartFree : (System.Action)app.NextLevel, Ui.ButtonStyle.Primary, last ? 180 : 520);
                Add("Train again", app.ShowPit, Ui.ButtonStyle.Secondary, 160);
                if (capstone) Add("If-then plans", ShowPlans, Ui.ButtonStyle.Secondary, 180);
            }
        }
        else Add("Set up again", app.ShowPit, Ui.ButtonStyle.Primary, 200);
        Add("Title screen", app.ShowTitle, Ui.ButtonStyle.Quiet, 160);
        if (Platform.IsWeb) Add("Games menu", Platform.GoToMenu, Ui.ButtonStyle.Quiet, 160);
        Ui.Emblem(root);
    }

    void Unlock(float y, Lesson next, int stars)
    {
        var card = Ui.Card(root, "Unlock", Theme.TealDeep, Theme.TealDeep);
        Ui.Place(card.rectTransform, 1420, y, 420, 136);
        var t = card.transform;
        var tag = Ui.Eyebrow(t, $"UNLOCKED FOR LEVEL {next.id}", Theme.Teal); tag.font = Theme.Bold;
        Ui.Place(tag.rectTransform, 20, 14, 380, 20);
        string what = next.number == Ladder.AttemptsLevel ? "How many attempts to train before the race."
                    : next.number == Ladder.SensorsLevel ? "The sensors: which ones are on."
                    : next.number == Ladder.MovesLevel ? "The moves it may make, and a constraint."
                    : next.number == Ladder.PointsLevel ? "The points table."
                    : next.number == Ladder.ExploreLevel ? "The chance it tries something new, and how it falls."
                    : next.number == Ladder.TracksLevel ? "The training tracks and the test track."
                    : next.number == Ladder.MeasureLevel ? "The measure you judge it by."
                    : next.number == Ladder.KnobsLevel ? "The knobs, and the seed."
                    : next.Capstone ? "Everything, and no help: the capstone." : next.title;
        var body = Ui.Text(t, what, 17, Color.white, true);
        Ui.Place(body.rectTransform, 20, 38, 380, 56);
        var starsLine = Ui.Text(t, $"{stars} of 3 stars banked  ·  {app.TotalStars} in all  ·  {app.Rank}", 13, Theme.Teal);
        Ui.Place(starsLine.rectTransform, 20, 100, 380, 24);
    }

    // The capstone's two if-then plans, about the student's own work.
    void ShowPlans()
    {
        if (plans != null) { Object.Destroy(plans.gameObject); plans = null; return; }
        plans = Ui.Node("Plans", root);
        Ui.Stretch(plans);
        var dim = Ui.Box(plans, "Dim", new Color(Theme.Ink.r, Theme.Ink.g, Theme.Ink.b, 0.45f), false);
        Ui.Stretch(dim.rectTransform);
        var card = Ui.Float(plans, "Card", 360, 200, 1200, 680);
        var t = card.transform;
        var title = Ui.Text(t, "Your two if-then plans", 26, Theme.Ink, true);
        Ui.Place(title.rectTransform, 32, 26, 800, 36);
        var sub = Ui.Text(t, "Two plans about your own work, to look at again at the follow-up session. Write, save.", 15, Theme.Text2);
        Ui.Place(sub.rectTransform, 32, 62, 1000, 24);
        var close = Ui.Button(t, "Close", () => { Object.Destroy(plans.gameObject); plans = null; }, Ui.ButtonStyle.Secondary, 15);
        Ui.Place(close.GetComponent<RectTransform>(), 1200 - 32 - 100, 26, 100, 40);
        string key = $"robot_racing_plans_{app.lesson.id}";
        var saved = PlayerPrefs.GetString(key, "\n").Split('\n');
        var inputs = new TMP_InputField[2];
        for (int i = 0; i < 2; i++)
        {
            var label = Ui.Eyebrow(t, i == 0 ? "PLAN 1  ·  IF ..., THEN I ..." : "PLAN 2  ·  IF ..., THEN I ...", Theme.TealDeep);
            Ui.Place(label.rectTransform, 32, 110 + i * 220, 600, 20);
            inputs[i] = Ui.Input(t, "If we reward a number at work, then I ...", true, 18);
            Ui.Place(inputs[i].GetComponent<RectTransform>(), 32, 136 + i * 220, 1136, 160);
            inputs[i].text = i < saved.Length ? saved[i] : "";
        }
        var save = Ui.Button(t, "Save", () => { PlayerPrefs.SetString(key, inputs[0].text.Replace("\n", " ") + "\n" + inputs[1].text.Replace("\n", " ")); PlayerPrefs.Save(); Sfx.Buy(); }, Ui.ButtonStyle.Primary, 16);
        Ui.Place(save.GetComponent<RectTransform>(), 32, 600, 140, 46);
    }
}
