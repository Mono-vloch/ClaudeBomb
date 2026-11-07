using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The cover: the robot car on the grid at dusk under the start gantry, the camera turning
// slowly round it, framed as the trackside camera's feed; beside it the wordmark, the
// pitch, LUMA's one line, one big Continue and a small Levels. Levels opens the season's
// calendar: the thirteen rounds as cards, each with its circuit and the stars it earned.
public class TitleScreen
{
    public readonly RectTransform root;
    RectTransform calendar;

    public TitleScreen(Transform parent, App app)
    {
        root = Ui.Node("Title", parent);
        Ui.Stretch(root);
        app.Paper(Color.clear);

        var frame = Ui.Node("Frame", root);
        Ui.Place(frame, 1020, 40, 900, 960);
        app.world.Build(Track.Get("grand"), true);
        if (Circuit.HasKit) app.world.ShowCarOrbit(frame, 1.32f, 8f);
        else { app.world.ShowOrbit(frame); app.world.Demo(true); }

        var shade = Ui.Box(root, "Shade", Theme.Ink, false);
        Ui.Place(shade.rectTransform, -800, -300, 1720, 1700);
        shade.raycastTarget = false;
        var fade = Ui.Node("Fade", root).gameObject.AddComponent<Image>();
        fade.sprite = Ui.FadeStrip; fade.color = Theme.Ink; fade.raycastTarget = false;
        Ui.Place(fade.rectTransform, 920, -300, 440, 1700);
        if (Circuit.HasKit)
        {
            var feedArea = Ui.Node("Feed", root);
            Ui.Place(feedArea, 1000, 0, 920, 1080);
            CameraFeed.Build(feedArea, "CAM 3  /  THE GRID", "MAALOOMATIIA RACEWAY  /  EVENING PRACTICE");
        }

        Ui.Wordmark(root, 120, 64, 250);
        GameMenu.Icon(root);

        var eyebrow = Ui.Eyebrow(root, "ROBOT RACING  ·  GAME 03", Theme.Faint);
        Ui.Place(eyebrow.rectTransform, 120, 170, 600, 22);
        var title = Ui.Text(root, "Nobody Drives", 116, Color.white, true);
        Ui.Place(title.rectTransform, 114, 196, 1100, 140);
        var tag = Ui.Text(root, "You set the points. It teaches itself to race.", 40, Theme.Teal, true);
        Ui.Place(tag.rectTransform, 120, 350, 1000, 56);
        int done = app.LevelsDone, next = Mathf.Min(done + 1, Ladder.Count);
        if (done == 0) app.luma.Cue(root, "title", 120, 436, 820, stay: true);
        else app.luma.Line(root, 120, 436, 820, $"Welcome back. Next: {Ladder.Get(next).title.Replace("Capstone: ", "").ToLowerInvariant()}", true, true, 17, type: false);

        var start = Ui.Button(root, done == 0 ? "Start" : $"Continue: round {next}", () => app.StartLevel(next), Ui.ButtonStyle.Accent, 24);
        Ui.Place(start.GetComponent<RectTransform>(), 120, 580, 360, 72);
        var levels = Ui.Button(root, "Levels", () => Calendar(app), Ui.ButtonStyle.GhostOnDark, 16);
        Ui.Place(levels.GetComponent<RectTransform>(), 500, 592, 120, 48);
    }

    // The season's calendar: a card per round, its circuit, its stars; tap one to go there.
    void Calendar(App app)
    {
        if (calendar != null) { Object.Destroy(calendar.gameObject); calendar = null; return; }
        calendar = Ui.Node("Calendar", root);
        Ui.Stretch(calendar);
        var dim = Ui.Box(calendar, "Dim", Theme.Ink, false); // opaque: the title must not show through the cards
        Ui.Stretch(dim.rectTransform);
        dim.gameObject.AddComponent<Button>().onClick.AddListener(() => { Object.Destroy(calendar.gameObject); calendar = null; });
        var head = Ui.Eyebrow(calendar, "THE SEASON", Theme.Teal); head.font = Theme.Bold;
        Ui.Place(head.rectTransform, 120, 118, 400, 22);
        var close = Ui.Button(calendar, "Close", () => { Object.Destroy(calendar.gameObject); calendar = null; }, Ui.ButtonStyle.GhostOnDark, 16);
        Ui.Place(close.GetComponent<RectTransform>(), Ui.W - 120 - 100, 108, 100, 40);
        int done = app.LevelsDone, next = Mathf.Min(done + 1, Ladder.Count);
        const float cw = 320f, ch = 170f, gap = 20f;
        for (int i = 0; i < Ladder.All.Length; i++)
        {
            var l = Ladder.All[i];
            int col = i % 5, row = i / 5;
            float x = 120 + col * (cw + gap), y = 156 + row * (ch + gap);
            bool isNext = l.number == next, isDone = l.number <= done;
            var card = Ui.Card(calendar, "Round " + l.number, isNext ? new Color(0.07f, 0.32f, 0.36f, 1f) : new Color(0.08f, 0.14f, 0.27f, 1f), isNext ? Theme.Teal : isDone ? Theme.TealDeep : Ui.Hairline, isNext ? 2f : 1f);
            Ui.Place(card.rectTransform, x, y, cw, ch);
            var t = card.transform;
            var round = Ui.Text(t, $"R{l.number}", 44, isDone || isNext ? Theme.Teal : Color.white, true);
            Ui.Place(round.rectTransform, 20, 14, 120, 56);
            var track = Track.Get(l.training[0]);
            var name = Ui.Text(t, track.name, 20, Color.white, true);
            Ui.Place(name.rectTransform, 20, 76, cw - 40, 30);
            name.enableWordWrapping = false; name.overflowMode = TextOverflowModes.Ellipsis;
            LessonScreen.Map(t, track, cw - 20 - 96, 12, 96, 60);
            int stars = app.StarsOf(l.number);
            for (int s = 0; s < 3; s++)
            {
                var star = Ui.Dot(t, s < stars ? Theme.Amber : new Color(1, 1, 1, 0.18f), 14);
                Ui.Place(star.rectTransform, 20 + s * 20, ch - 38, 14, 14);
                star.raycastTarget = false;
            }
            card.raycastTarget = true;
            var button = card.gameObject.AddComponent<Button>();
            button.targetGraphic = card;
            int number = l.number;
            button.onClick.AddListener(() => { Sfx.Tick(); app.StartLevel(number); });
        }
    }
}
