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

        var eyebrow = Ui.Eyebrow(root, "ROBOT RACING  ·  GAME 03", Theme.Faint);
        Ui.Place(eyebrow.rectTransform, 120, 170, 600, 22);
        var title = Ui.Text(root, "Nobody Drives", 116, Color.white, true);
        Ui.Place(title.rectTransform, 114, 196, 1100, 140);
        var tag = Ui.Text(root, "You set the points. It teaches itself to race.", 40, Theme.Teal, true);
        Ui.Place(tag.rectTransform, 120, 350, 1000, 56);
        var blurb = Ui.Text(root,
            "Every student gets the same robot, and nobody drives it. You decide what it senses, what it may do and what earns points. " +
            "Then watch it crash for a hundred attempts, slowly get good, and find out what it learns when you reward slightly the wrong thing.",
            22, Theme.Faint);
        Ui.Place(blurb.rectTransform, 120, 424, 820, 120);

        int done = app.LevelsDone, next = Mathf.Min(done + 1, Ladder.Count);
        Luma.Card(root, 120, 560, 820, 92,
            done == 0 ? "I'm LUMA, your coach. Thirteen levels in three tiers. Press Start: the robot is on the grid."
                      : $"Welcome back. {done} of {Ladder.Count} levels done, {app.TotalStars} stars: {app.Rank}. Next: {Ladder.Get(next).title.ToLowerInvariant()}",
            16, null, null, true);

        // The campaign: three tiers, each a track of stops with its stars.
        float y = 684;
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
                if (isNext)
                {
                    var halo = Ui.Dot(root, new Color(Theme.Teal.r, Theme.Teal.g, Theme.Teal.b, 0.3f), 48);
                    Ui.Place(halo.rectTransform, x - 24, ty - 24, 48, 48);
                    halo.raycastTarget = false;
                }
                var rim = Ui.Dot(root, isDone || isNext ? Theme.Teal : new Color(1, 1, 1, 0.45f), 34);
                Ui.Place(rim.rectTransform, x - 17, ty - 17, 34, 34);
                var stop = Ui.Dot(root, isDone || isNext ? Theme.Teal : Theme.Ink, 30);
                Ui.Place(stop.rectTransform, x - 15, ty - 15, 30, 30);
                var num = Ui.Text(stop.transform, l.id, 11, isDone || isNext ? Theme.Ink : Theme.Faint, true, TextAlignmentOptions.Center);
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
            // The tier's levels, named, to the right of its track.
            var names = Ui.Text(root, string.Join("  ·  ", System.Array.ConvertAll(levels, l => l.title.Replace("Capstone: ", ""))), 11, Theme.Faint);
            Ui.Place(names.rectTransform, x0 + step * (levels.Length - 1) + 40, ty - 12, 900 - step * (levels.Length - 1), 36);
            y += 86;
        }

        var start = Ui.Button(root, done == 0 ? "Start" : $"Continue: level {Ladder.Get(next).id}", () => app.StartLevel(next), Ui.ButtonStyle.Accent, 22);
        Ui.Place(start.GetComponent<RectTransform>(), 120, 950, 300, 60);
        var free = Ui.Button(root, "Free play", app.StartFree, Ui.ButtonStyle.GhostOnDark, 18);
        Ui.Place(free.GetComponent<RectTransform>(), 440, 950, 170, 60);
        var foot = Ui.Text(root, "A Maaloomatiia Academy game about teaching a machine to learn.", 14, Theme.Faint);
        Ui.Place(foot.rectTransform, 120, 1030, 1000, 24);
    }
}
