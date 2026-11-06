using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The briefing before a level: the level's circuit on the right, sharp, the car on
// its grid; on the left, in dark glass, the level's question, its headline, LUMA's
// opening line spoken and typed, the track with its map, and the robot's seven
// parts with this level's lit. Everything the course says beyond that is in the
// file. A warm-up question from an earlier level comes first (no score; a wrong
// answer shows the right one).
public class LessonScreen
{
    public readonly RectTransform root;
    static readonly string[] Parts = { "The robot", "What it senses", "What it may do", "The points", "New moves", "The knobs", "Tracks and measure" };

    public LessonScreen(Transform parent, App app)
    {
        var l = app.lesson;
        root = Ui.Node("Lesson", parent);
        Ui.Stretch(root);
        app.Paper(Color.clear);

        // The world: this level's track, by day, on the right, sharp.
        var window = Ui.Node("Window", root);
        Ui.Place(window, 900, 150, 980, 880);
        var track = Track.Get(l.training[0]);
        app.world.Build(track, false);
        app.world.ShowOverview(window);
        app.atmosphere.MenuBlur(false);

        // The briefing, left, in dark glass over the park.
        var panel = Ui.Glass(root, "Brief", 40, 40, 840, 840, false);
        var b = panel.transform;
        var eyebrow = Ui.Eyebrow(b, $"LEVEL {l.id} OF 3.4  ·  THE BRIEFING", Theme.Teal);
        eyebrow.font = Theme.Bold;
        Ui.Place(eyebrow.rectTransform, 40, 36, 760, 22);
        var title = Ui.Text(b, l.title, 46, Color.white, true);
        Ui.Place(title.rectTransform, 36, 64, 780, 110);
        var kicker = Ui.Text(b, l.kicker, 24, Theme.Teal, true);
        Ui.Place(kicker.rectTransform, 40, 180, 760, 70);

        app.luma.Line(b, 40, 262, 760, app.luma.Say("intro-" + l.number), true, true, 18);

        // The track, as the pit lane will show it: its name, one line, the map.
        var msg = Ui.Box(b, "Track", new Color(1f, 1f, 1f, 0.08f));
        Ui.Place(msg.rectTransform, 40, 380, 760, 132);
        msg.raycastTarget = false;
        var from = Ui.Eyebrow(msg.transform, $"THE TRACK  ·  {track.name.ToUpperInvariant()}" + (l.training.Length > 1 ? $"  ·  AND {l.training.Length - 1} MORE" : "") + (l.test != l.training[0] ? $"  ·  TEST: {Track.Get(l.test).name.ToUpperInvariant()}" : ""), Theme.Teal);
        Ui.Place(from.rectTransform, 24, 16, 500, 22);
        var note = Ui.Text(msg.transform, track.note, 20, Color.white);
        Ui.Place(note.rectTransform, 24, 42, 480, 80);
        Map(msg.transform, track, 530, 14, 210, 104);

        // The seven parts: this level's lit, the ones already open in white, the rest dim.
        var partsTag = Ui.Eyebrow(b, "YOUR ROBOT, PART BY PART  ·  THIS LEVEL OPENS", Theme.Teal);
        Ui.Place(partsTag.rectTransform, 40, 538, 760, 20);
        bool[] open = { true, l.sensors, l.moves, l.points, l.explore, l.knobs || l.attempts, l.tracks || l.measure };
        bool[] lit =
        {
            l.number == 1, l.number == Ladder.SensorsLevel, l.number == Ladder.MovesLevel, l.number == Ladder.PointsLevel || l.number == 7,
            l.number == Ladder.ExploreLevel, l.number == Ladder.KnobsLevel || l.number == Ladder.AttemptsLevel, l.number == Ladder.TracksLevel || l.number == Ladder.MeasureLevel,
        };
        for (int i = 0; i < Parts.Length; i++)
        {
            float x = 40 + (i % 3) * 256, y = 566 + (i / 3) * 54;
            var chip = Ui.Box(b, Parts[i], lit[i] ? Theme.Teal : open[i] ? new Color(1f, 1f, 1f, 0.14f) : new Color(1f, 1f, 1f, 0.05f));
            Ui.Place(chip.rectTransform, x, y, 240, 44);
            chip.raycastTarget = false;
            var name = Ui.Text(chip.transform, Parts[i], 15, lit[i] ? Theme.Ink : open[i] ? Color.white : Theme.Faint, true, TextAlignmentOptions.Center);
            Ui.Stretch(name.rectTransform);
        }

        var go = Ui.Button(b, "Go to the pit lane", app.ShowPit, Ui.ButtonStyle.Accent, 22);
        Ui.Place(go.GetComponent<RectTransform>(), 40, 740, 320, 64);
        var file = Ui.Button(b, "The file", () => app.ShowFile(), Ui.ButtonStyle.GhostOnDark, 18);
        Ui.Place(file.GetComponent<RectTransform>(), 380, 740, 130, 64);
        var home = Ui.Button(b, "Title screen", app.ShowTitle, Ui.ButtonStyle.GhostOnDark, 18);
        Ui.Place(home.GetComponent<RectTransform>(), 530, 740, 170, 64);

        // The ladder, over the track: the levels. Any other step jumps there.
        var strip = Ui.Glass(root, "Ladder", 920, 40, 960, 82, false);
        var ladder = Ui.Eyebrow(strip.transform, "THE LADDER", Theme.Teal);
        Ui.Place(ladder.rectTransform, 20, 10, 600, 20);
        int n = 0;
        foreach (var step in Ladder.All)
        {
            bool current = step.number == l.number, done = step.number <= app.LevelsDone;
            var card = Ui.Card(strip.transform, "Step", current ? Theme.Teal : done ? Theme.TealDeep : new Color(1f, 1f, 1f, 0.08f), current ? Theme.Teal : done ? Theme.TealDeep : new Color(1f, 1f, 1f, 0.3f));
            Ui.Place(card.rectTransform, 20 + n * 70, 34, 62, 38);
            var num = Ui.Text(card.transform, step.id, 13, current ? Theme.Ink : Color.white, true, TextAlignmentOptions.Center);
            Ui.Stretch(num.rectTransform);
            if (!current)
            {
                card.raycastTarget = true;
                var button = card.gameObject.AddComponent<Button>();
                button.targetGraphic = card;
                int number = step.number;
                button.onClick.AddListener(() => { Sfx.Tick(); app.StartLevel(number); });
            }
            n++;
        }
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
        var verdict = Ui.Text(t, "", 20, Theme.Ink, true);
        Ui.Place(verdict.rectTransform, 32, 330, 560, 56);
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
                verdict.text = right ? "Right." : "Not quite.";
                verdict.color = right ? Theme.TealDeep : Theme.Danger;
                if (right) Sfx.Chime(); else Sfx.Ruin();
                for (int j = 0; j < buttons.Length; j++)
                {
                    buttons[j].interactable = false;
                    buttons[j].transform.Find("Fill").GetComponent<Image>().color = j == r.answer ? Theme.Surface2 : j == index ? Theme.DangerSoft : Theme.Panel;
                    buttons[j].GetComponent<Image>().color = j == r.answer ? Theme.Aqua : j == index ? Theme.Danger : Theme.Border;
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
