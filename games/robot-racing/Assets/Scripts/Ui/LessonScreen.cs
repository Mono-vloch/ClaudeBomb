using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The briefing before a level: a warm-up question from an earlier level (no
// score; a wrong answer shows the right one), LUMA's opening line, the idea in
// plain words, the track, and the five parts of the robot with the setting this
// level opens lit up.
public class LessonScreen
{
    public readonly RectTransform root;

    public LessonScreen(Transform parent, App app)
    {
        var l = app.lesson;
        root = Ui.Node("Lesson", parent);
        Ui.Stretch(root);
        app.Paper(Theme.Page);

        var eyebrow = Ui.Eyebrow(root, $"BRIEFING  ·  TIER {l.tier}: {Ladder.TierNames[l.tier - 1].ToUpperInvariant()}", Theme.Text2);
        Ui.Place(eyebrow.rectTransform, 160, 72, 900, 22);

        // The ladder, top right: the tier's levels. Any other step jumps there.
        var ladder = Ui.Eyebrow(root, "THE LADDER  ·  CLICK A STEP TO JUMP THERE", Theme.Muted);
        Ui.Place(ladder.rectTransform, 1160, 40, 600, 20);
        int i = 0;
        foreach (var step in Ladder.All)
        {
            bool current = step.number == l.number, done = step.number <= app.LevelsDone;
            var card = Ui.Card(root, "Step", current ? Theme.TealDeep : done ? Theme.Surface2 : Theme.Panel, current ? Theme.TealDeep : done ? Theme.BorderTeal : Theme.Border);
            Ui.Place(card.rectTransform, 1160 + i * 54, 62, 48, 44);
            var n = Ui.Text(card.transform, step.id, 13, current ? Color.white : done ? Theme.TealDeep : Theme.Muted, true, TextAlignmentOptions.Center);
            Ui.Stretch(n.rectTransform);
            if (!current)
            {
                var button = card.gameObject.AddComponent<Button>();
                button.targetGraphic = card;
                int number = step.number;
                button.onClick.AddListener(() => { Sfx.Tick(); app.StartLevel(number); });
            }
            i++;
        }

        var level = Ui.Eyebrow(root, $"LEVEL {l.id} OF 3.4", Theme.TealDeep);
        level.font = Theme.Bold;
        Ui.Place(level.rectTransform, 160, 130, 600, 22);
        var title = Ui.Text(root, l.title, 54, Theme.Ink, true);
        Ui.Place(title.rectTransform, 156, 156, 1000, 80);
        var kicker = Ui.Text(root, l.kicker, 28, Theme.Aqua, true);
        Ui.Place(kicker.rectTransform, 160, 246, 960, 84);

        Luma.Card(root, 160, 340, 960, 110, app.luma.Say("intro-" + l.number), 18);

        var learn = Ui.Text(root, l.learn, 19, Theme.Text2);
        Ui.Place(learn.rectTransform, 160, 464, 960, 200);

        // The track, as the pit lane will show it.
        var track = Track.Get(l.training[0]);
        var msg = Ui.Soft(root, "Track");
        Ui.Place(msg.rectTransform, 160, 680, 960, 200);
        var from = Ui.Eyebrow(msg.transform, $"THE TRACK  ·  {track.name.ToUpperInvariant()}" + (l.training.Length > 1 ? $"  ·  AND {l.training.Length - 1} MORE TO TRAIN ON" : "") + (l.test != l.training[0] ? $"  ·  TEST TRACK: {Track.Get(l.test).name.ToUpperInvariant()}" : ""), Theme.TealDeep);
        Ui.Place(from.rectTransform, 24, 16, 920, 22);
        var note = Ui.Text(msg.transform, track.note, 20, Theme.Ink);
        Ui.Place(note.rectTransform, 24, 42, 920, 60);
        Map(msg.transform, track, 24, 104, 920, 84);

        // The five parts, right: what each is, and whether this level opens it.
        var parts = Ui.Float(root, "Parts", 1160, 130, 600, 730);
        var pt = Ui.Text(parts.transform, "Your robot, part by part", 22, Theme.Ink, true);
        Ui.Place(pt.rectTransform, 26, 22, 540, 30);
        var ps = Ui.Text(parts.transform, "Each level opens one more setting. The ones with a dot are this level's.", 15, Theme.Text2);
        Ui.Place(ps.rectTransform, 26, 54, 540, 40);
        float y = 102;
        Part(parts.transform, ref y, "The robot", "The one making the decisions. It starts knowing nothing.", "Nobody drives it", true, l.number == 1);
        Part(parts.transform, ref y, "What it senses", "Its sensors: front, left, right, speed, bend ahead.", l.sensors ? "You choose which are on" : "Fixed: front, left, right, speed · opens at 2.1", l.sensors, l.number == Ladder.SensorsLevel);
        Part(parts.transform, ref y, "What it may do", "Speed up, hold, slow down, turn; and a top speed it must respect.", l.moves ? "You choose" : "Fixed: all five, top speed 2 · opens at 2.2", l.moves, l.number == Ladder.MovesLevel);
        Part(parts.transform, ref y, "The points", "The finish, a crash, each step toward, each step away, each moment.", l.points ? "You write the table" : "Fixed: finish 100, crash -100, toward 2 · opens at 2.3", l.points, l.number == Ladder.PointsLevel || l.number == 7);
        Part(parts.transform, ref y, "New moves", "How often it tries something new, and how that falls over attempts.", l.explore ? "You set it" : "Fixed: 9 in 10, falling · opens at 2.5", l.explore, l.number == Ladder.ExploreLevel);
        Part(parts.transform, ref y, "The knobs", "How fast it rewrites its rule book, how many attempts, the seed.", l.knobs ? "You set them, one at a time" : l.attempts ? "You set the attempts" : "Fixed · attempts open at 1.2, the rest at 3.3", l.knobs || l.attempts, l.number == Ladder.KnobsLevel || l.number == Ladder.AttemptsLevel);
        Part(parts.transform, ref y, "The tracks and the measure", "Training tracks, a test track it never trains on, and the number to judge by.", l.tracks || l.measure ? "You choose" : "Fixed · open at 3.1 and 3.2", l.tracks || l.measure, l.number == Ladder.TracksLevel || l.number == Ladder.MeasureLevel);

        var go = Ui.Button(root, "Go to the pit lane", app.ShowPit, Ui.ButtonStyle.Accent, 22);
        Ui.Place(go.GetComponent<RectTransform>(), 160, 926, 320, 64);
        var home = Ui.Button(root, "Title screen", app.ShowTitle, Ui.ButtonStyle.Quiet, 18);
        Ui.Place(home.GetComponent<RectTransform>(), 500, 926, 200, 64);
        Ui.Emblem(root);

        if (l.recall != null) Recall(app, l.recall);
    }

    // The track as a small map: road light, walls dark, start and finish marked.
    public static void Map(Transform parent, Track t, float x, float y, float w, float h)
    {
        float cell = Mathf.Min(w / t.W, h / t.H);
        float ox = x + (w - cell * t.W) / 2;
        for (int cy = 0; cy < t.H; cy++)
            for (int cx = 0; cx < t.W; cx++)
            {
                var p = new Vector2Int(cx, cy);
                char c = t.At(p);
                if (c == '#') continue;
                var tile = Ui.Box(parent, "cell", c == 'S' ? Theme.Aqua : c == 'F' ? Theme.Ink : c == 'O' ? Theme.Hex("#3A4A66") : Theme.Hex("#5C6270"), false);
                Ui.Place(tile.rectTransform, ox + cx * cell + 1, y + cy * cell + 1, cell - 2, cell - 2);
                tile.raycastTarget = false;
            }
    }

    static void Part(Transform t, ref float y, string name, string what, string state, bool open, bool lit)
    {
        var row = Ui.Card(t, name, lit ? Theme.Surface2 : Theme.Panel, lit ? Theme.BorderTeal : Theme.Border1);
        Ui.Place(row.rectTransform, 20, y, 560, 82);
        if (lit) { var dot = Ui.Dot(row.transform, Theme.Aqua, 10); Ui.Place(dot.rectTransform, 18, 20, 10, 10); }
        var n = Ui.Text(row.transform, name, 18, Theme.Ink, true);
        Ui.Place(n.rectTransform, lit ? 34 : 18, 12, 260, 26);
        var s = Ui.Text(row.transform, state, 12, open ? Theme.TealDeep : Theme.Muted, true, TextAlignmentOptions.TopRight);
        Ui.Place(s.rectTransform, 260, 14, 284, 40);
        var w = Ui.Text(row.transform, what, 13, Theme.Text2);
        Ui.Place(w.rectTransform, 18, 42, 524, 36);
        y += 88;
    }

    // The warm-up: one question from an earlier level, over the briefing. No score.
    void Recall(App app, Recall r)
    {
        var overlay = Ui.Node("Recall", root);
        Ui.Stretch(overlay);
        var dim = Ui.Box(overlay, "Dim", new Color(Theme.Ink.r, Theme.Ink.g, Theme.Ink.b, 0.72f), false);
        Ui.Stretch(dim.rectTransform);
        dim.raycastTarget = true;
        var card = Ui.Float(overlay, "Card", 560, 300, 800, 440);
        var t = card.transform;
        var tag = Ui.Eyebrow(t, $"WARM-UP  ·  FROM LEVEL {r.from}  ·  NO SCORE", Theme.TealDeep);
        tag.font = Theme.Bold;
        Ui.Place(tag.rectTransform, 32, 28, 600, 22);
        var q = Ui.Text(t, r.question, 26, Theme.Ink, true);
        Ui.Place(q.rectTransform, 32, 60, 736, 80);
        var verdict = Ui.Text(t, "", 17, Theme.Ink);
        Ui.Place(verdict.rectTransform, 32, 330, 736, 56);
        var go = Ui.Button(t, "Continue", () => Object.Destroy(overlay.gameObject), Ui.ButtonStyle.Accent, 17);
        Ui.Place(go.GetComponent<RectTransform>(), 800 - 32 - 160, 380, 160, 46);
        go.gameObject.SetActive(false);
        var buttons = new Button[r.options.Length];
        for (int i = 0; i < r.options.Length; i++)
        {
            int index = i;
            buttons[i] = Ui.Button(t, r.options[i], () =>
            {
                bool right = index == r.answer;
                verdict.text = right ? $"Right. That is from level {r.from}." : $"Not quite. The answer: {r.options[r.answer]}. That is from level {r.from}.";
                verdict.color = right ? Theme.TealDeep : Theme.Danger;
                for (int j = 0; j < buttons.Length; j++)
                {
                    buttons[j].interactable = false;
                    buttons[j].transform.Find("Fill").GetComponent<Image>().color = j == r.answer ? Theme.Surface2 : j == index ? Theme.DangerSoft : Theme.Panel;
                }
                go.gameObject.SetActive(true);
            }, Ui.ButtonStyle.Secondary, 17);
            Ui.Place(buttons[i].GetComponent<RectTransform>(), 32, 150 + i * 56, 736, 48);
            var label = buttons[i].GetComponentInChildren<TextMeshProUGUI>();
            label.alignment = TextAlignmentOptions.MidlineLeft;
            Ui.Stretch(label.rectTransform, 20, 0, 20, 0);
        }
    }
}
