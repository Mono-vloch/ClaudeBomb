using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Maaloomatiia.Games;

// The cover: the track at dusk with the robot driving it on its own, the camera
// turning slowly; over it the wordmark, the pitch, LUMA, the campaign (three
// tiers of levels with the stars each earned) and the way in.
public class TitleScreen
{
    public readonly RectTransform root;

    public TitleScreen(Transform parent, App app)
    {
        root = Ui.Node("Title", parent);
        Ui.Stretch(root);
        app.Paper(Color.clear);

        var frame = Ui.Node("Frame", root);
        Ui.Place(frame, 1020, 40, 900, 960);
        app.world.Build(Track.Get("grand"), true);
        app.world.ShowOrbit(frame);
        app.world.Demo(true);

        var shade = Ui.Box(root, "Shade", Theme.Ink, false);
        Ui.Place(shade.rectTransform, -800, -300, 1720, 1700);
        shade.raycastTarget = false;
        var fade = Ui.Node("Fade", root).gameObject.AddComponent<Image>();
        fade.sprite = Ui.FadeStrip; fade.color = Theme.Ink; fade.raycastTarget = false;
        Ui.Place(fade.rectTransform, 920, -300, 440, 1700);

        Ui.Wordmark(root, 120, 64, 250);
        if (Platform.IsWeb)
        {
            var menu = Ui.Button(root, "Games menu", Platform.GoToMenu, Ui.ButtonStyle.GhostOnDark, 18);
            Ui.Place(menu.GetComponent<RectTransform>(), 1600, 58, 200, 52);
        }
        Button sound = null;
        sound = Ui.Button(root, Sfx.Enabled ? "Sound on" : "Sound off", () => { Sfx.Enabled = !Sfx.Enabled; Ui.SetLabel(sound, Sfx.Enabled ? "Sound on" : "Sound off"); }, Ui.ButtonStyle.GhostOnDark, 16);
        Ui.Place(sound.GetComponent<RectTransform>(), 1440, 58, 140, 52);
        Button voice = null;
        voice = Ui.Button(root, Luma.Voice ? "Voice on" : "Voice off", () => { Luma.Voice = !Luma.Voice; if (!Luma.Voice) app.luma.Hush(); Ui.SetLabel(voice, Luma.Voice ? "Voice on" : "Voice off"); }, Ui.ButtonStyle.GhostOnDark, 16);
        Ui.Place(voice.GetComponent<RectTransform>(), 1290, 58, 134, 52);

        var eyebrow = Ui.Eyebrow(root, "ROBOT RACING  ·  GAME 03", Theme.Faint);
        Ui.Place(eyebrow.rectTransform, 120, 170, 600, 22);
        var title = Ui.Text(root, "Nobody Drives", 116, Color.white, true);
        Ui.Place(title.rectTransform, 114, 196, 1100, 140);
        var tag = Ui.Text(root, "You set the points. It teaches itself to race.", 40, Theme.Teal, true);
        Ui.Place(tag.rectTransform, 120, 350, 1000, 56);
        int done = app.LevelsDone, next = Mathf.Min(done + 1, Ladder.Count);
        app.luma.Line(root, 120, 436, 820,
            done == 0 ? "I'm LUMA, your coach. Thirteen levels in three tiers. Press Start: the robot is on the grid."
                      : $"Welcome back. {done} of {Ladder.Count} levels done, {app.TotalStars} stars: {app.Rank}. Next: {Ladder.Get(next).title.ToLowerInvariant()}",
            true, true, 17);

        // The campaign: three tiers, each a track of stops with its stars.
        float y = 560;
        for (int tier = 1; tier <= 3; tier++)
        {
            var label = Ui.Eyebrow(root, $"TIER {tier}  ·  {Ladder.TierNames[tier - 1].ToUpperInvariant()}", Theme.Faint);
            Ui.Place(label.rectTransform, 120, y, 700, 18);
            var levels = System.Array.FindAll(Ladder.All, l => l.tier == tier);
            float x0 = 150, step = 112, ty = y + 42;
            var track = Ui.Box(root, "Track", new Color(1, 1, 1, 0.22f), false);
            Ui.Place(track.rectTransform, x0, ty - 1, step * (levels.Length - 1), 2);
            track.raycastTarget = false;
            for (int i = 0; i < levels.Length; i++)
            {
                var l = levels[i];
                bool isNext = l.number == next, isDone = l.number <= done;
                float x = x0 + i * step;
                // Rounded squares, never circles: the level's stop is a button.
                if (isNext)
                {
                    var halo = Ui.Box(root, "Halo", new Color(Theme.Teal.r, Theme.Teal.g, Theme.Teal.b, 0.3f));
                    Ui.Place(halo.rectTransform, x - 26, ty - 26, 52, 52);
                    halo.raycastTarget = false;
                }
                var rim = Ui.Box(root, "Rim", isDone || isNext ? Theme.Teal : new Color(1, 1, 1, 0.45f));
                Ui.Place(rim.rectTransform, x - 19, ty - 19, 38, 38);
                var stop = Ui.Box(root, "Stop", isDone || isNext ? Theme.Teal : Theme.Ink);
                Ui.Place(stop.rectTransform, x - 17, ty - 17, 34, 34);
                var num = Ui.Text(stop.transform, l.id, 12, isDone || isNext ? Theme.Ink : Theme.Faint, true, TextAlignmentOptions.Center);
                Ui.Stretch(num.rectTransform);
                int stars = app.StarsOf(l.number);
                for (int s = 0; s < 3; s++)
                {
                    var star = Ui.Dot(root, s < stars ? Theme.Teal : new Color(1, 1, 1, 0.18f), 6);
                    Ui.Place(star.rectTransform, x - 11 + s * 9, ty + 22, 6, 6);
                    star.raycastTarget = false;
                }
                var hit = Ui.Box(root, "Level " + l.id, Color.clear, false);
                Ui.Place(hit.rectTransform, x - 50, ty - 26, 100, 60);
                var button = hit.gameObject.AddComponent<Button>();
                button.targetGraphic = stop;
                int number = l.number;
                button.onClick.AddListener(() => { Sfx.Tick(); app.StartLevel(number); });
            }
            y += 86;
        }

        var start = Ui.Button(root, done == 0 ? "Start" : $"Continue: level {Ladder.Get(next).id}", () => app.StartLevel(next), Ui.ButtonStyle.Accent, 22);
        Ui.Place(start.GetComponent<RectTransform>(), 120, 850, 300, 60);
        var free = Ui.Button(root, "Free play", app.StartFree, Ui.ButtonStyle.GhostOnDark, 18);
        Ui.Place(free.GetComponent<RectTransform>(), 440, 850, 170, 60);
    }
}
