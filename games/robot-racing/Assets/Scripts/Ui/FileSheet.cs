using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The file: everything the course says about the level, on one sheet, for whoever
// wants it. The screens show the track and one spoken line; the words live here:
// the idea, the track, the training runs' reasons, the takeaway, the question for
// work, how the pit lane works, and whatever the screen that opened it adds (the
// debrief's full checks and the runs' settings). Click anywhere off the sheet to
// close it.
public static class FileSheet
{
    static RectTransform open;
    public static bool IsOpen => open != null;

    public const string PitLane =
        "What it senses: the sensors, front, left, right, speed, bend ahead. A sensor that is off is a situation the robot cannot tell apart from another. " +
        "What it may do: speed up, hold, slow down, turn left, turn right, and a top speed it must respect. " +
        "The points table: the finish, a crash, each step toward the finish, each step away, each moment, a speed bonus. The robot learns from these and from nothing else.\n\n" +
        "New moves: the chance it tries something new, how that chance falls over the attempts, and the floor it never goes below. " +
        "The knobs: how fast it rewrites its rule book, how many attempts it gets, and the seed (the same luck twice). " +
        "The tracks: the ones it trains on, and a test track it never trains on. The measure: the number you judge it by.\n\n" +
        "A setting that is locked opens at a later level; until then every robot gets the same, so only this level's idea is in play. " +
        "The attempt budget is the robot's time: it learns from the points, over the attempts, on the tracks, with these settings and nothing else.";

    public static void Toggle(App app, Transform screen, params (string head, string body)[] extra)
    {
        if (open != null) { Close(); return; }
        if (screen == null) return;
        var l = app.lesson;
        var sections = new List<(string head, string body)>();
        if (l != null)
        {
            sections.Add(($"LEVEL {l.id}  ·  {l.title.ToUpperInvariant()}", l.kicker));
            sections.Add(("THE IDEA", l.learn));
            var track = Track.Get(l.training[0]);
            string tracks = track.name + ": " + track.note;
            if (l.training.Length > 1) tracks += $"\n\nTraining on {string.Join(", ", l.training.Select(id => Track.Get(id).name.ToLowerInvariant()))}.";
            if (l.test != l.training[0]) tracks += $"\n\nThe test track, never trained on: {Track.Get(l.test).name.ToLowerInvariant()}. {Track.Get(l.test).note}";
            sections.Add(("THE TRACK", tracks));
            // The rewards as placed on the track: the table the player built.
            if (l.points || l.flag)
            {
                var s = app.settings;
                sections.Add(("THE REWARDS, AS PLACED", $"The finish {s.finish:0}  ·  a crash {s.crash:0}  ·  each step toward the finish {s.toward:0.#}  ·  each step away {s.away:0.#}  ·  each moment {s.moment:0.#}  ·  speed bonus {s.speedBonus:0.#}"));
            }
            if (l.ready != null) sections.Add(("WATCH THE PRO", l.ready.why));
            if (l.broken != null) sections.Add(("FIX THE BROKEN BOT  ·  " + l.broken.name.ToUpperInvariant(), l.broken.why));
            sections.Add(("LUMA", LumaLines(l)));
        }
        sections.AddRange(extra);
        if (l != null)
        {
            sections.Add(("THE TAKEAWAY", l.takeaway));
            if (l.number == 3) sections.Add(("THE THREE KINDS OF LEARNING", "Studying with an answer key: supervised. Finding groups with no key: unsupervised. Learning to ride a bike: reinforcement. The robot was never shown a correct lap."));
            sections.Add(("ASK BACK AT WORK", l.ask));
        }
        sections.Add(("HOW THE PIT LANE WORKS", PitLane));

        open = Ui.Node("File", screen);
        Ui.Stretch(open);
        var dim = Ui.Box(open, "Dim", new Color(Theme.Ink.r, Theme.Ink.g, Theme.Ink.b, 0.55f), false);
        Ui.Stretch(dim.rectTransform);
        dim.gameObject.AddComponent<Button>().onClick.AddListener(Close);
        var card = Ui.Float(open, "Sheet", 360, 80, 1200, 920);
        var t = card.transform;
        var tag = Ui.Eyebrow(t, "THE FILE", Theme.TealDeep);
        tag.font = Theme.Bold;
        Ui.Place(tag.rectTransform, 40, 28, 400, 20);
        var title = Ui.Text(t, l != null ? l.title : "Free play", 30, Theme.Ink, true);
        Ui.Place(title.rectTransform, 40, 50, 1000, 40);
        title.enableWordWrapping = false; title.overflowMode = TextOverflowModes.Ellipsis;
        var close = Ui.Button(t, "Close", Close, Ui.ButtonStyle.Secondary, 15);
        Ui.Place(close.GetComponent<RectTransform>(), 1200 - 40 - 100, 36, 100, 40);
        var scroll = Ui.Scroll(t, "Scroll", out var content);
        Ui.Place(scroll.GetComponent<RectTransform>(), 40, 108, 1120, 780);
        float y = 0;
        foreach (var (head, body) in sections)
        {
            if (string.IsNullOrWhiteSpace(body)) continue;
            var h = Ui.Eyebrow(content, head, Theme.TealDeep);
            h.font = Theme.Bold;
            Ui.Place(h.rectTransform, 0, y, 1080, 20);
            var text = Ui.Text(content, body, 17, Theme.Ink);
            float th = text.GetPreferredValues(body, 1060, 0).y + 4;
            Ui.Place(text.rectTransform, 0, y + 26, 1060, th);
            y += 26 + th + 28;
        }
        content.sizeDelta = new Vector2(0, y + 10);
        Sfx.Tick();
    }

    // LUMA speaks only when something happens in the world (the first crash, the first
    // finish, the medal); the lines she no longer says on the screens are kept here, word
    // for word: the title's, the pit lane's, the training's, the race's and the questions'.
    static string LumaLines(Lesson l)
    {
        var lines = Luma.Lines;
        var said = new List<string>();
        void Add(string id) { if (lines.ContainsKey(id)) { var s = Luma.Spoken(id); if (!said.Contains(s)) said.Add(s); } }
        if (l.number == 1) Add("title");
        Add(l.number == 6 ? "cue-dark" : "pit-" + l.number);
        Add("why-" + l.number);
        Add("fix-" + l.number);
        if (l.number == 1) Add("cue-zero");
        Add("cue-settled");
        Add("guess-never");
        Add("cue-race");
        if (l.realRun) Add("cue-real");
        Add("cue-right");
        Add("cue-wrong");
        return string.Join("\n", said);
    }

    public static void Close()
    {
        if (open != null) Object.Destroy(open.gameObject);
        open = null;
    }
}
